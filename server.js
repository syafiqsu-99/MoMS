import express from "express";
import sql from "mssql";
import multer from "multer";
import cors from "cors";
import dotenv from "dotenv";
import path from "path";
import { fileURLToPath } from "url";
import fs from "fs/promises";
import pdfMake from "pdfmake/build/pdfmake.js";
import pdfFonts from "pdfmake/build/vfs_fonts.js";

// Get the directory name of the current module
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

dotenv.config();
const PORT = process.env.SERVER_PORT;
const VITE_SERVER_IP = process.env.VITE_SERVER_IP;
const VITE_API_URL = process.env.VITE_API_URL;
const viteBuildPath = path.join(__dirname, 'dist');

// Define static directories
const staticDirs = {
  toolsetImages: path.join(__dirname, 'dist/toolset_img'),
  docketPDFs: path.join(__dirname, 'dist/docket_pdf')
};

const app = express();
app.use(cors());
app.use(express.json());

// Create static directories if they don't exist
for (const dir of Object.values(staticDirs)) {
  fs.mkdir(dir, { recursive: true }).catch(console.error);
}

// Configure MoMS connection
const config = {
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  server: process.env.DB_SERVER,
  database: process.env.DB_DATABASE,
  options: {
    encrypt: false,
    trustServerCertificate: true,
  },
};

// Set up multer storage
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    const dir = staticDirs.toolsetImages;
    fs.mkdir(dir, { recursive: true }).catch(() => { });
    cb(null, dir);
  },
  filename: (req, file, cb) => {
    cb(null, file.originalname);
  },
});

const upload = multer({ storage });

// format datetime
function formatDatetime(isoDatetime) {
  if (isoDatetime instanceof Date) {
    const date = new Date(isoDatetime);
    return date.toISOString().replace("T", " ").replace("Z", "");
  } else if (typeof isoDatetime === "string") {
    // If it's already a string, process it as before
    return isoDatetime.replace("T", " ").replace("Z", "");
  }
};

function getColumnName(type) {
  const typeMapping = {
    'BP': 'back_plate',
    'BS': 'base_mould',
    'BC': 'blow_core',
    'BM': 'blow_mould',
    'ER': 'ejector',
    'HR': 'hot_runner',
    'CT': 'injection_cavity',
    'IC': 'injection_core',
    'LS': 'lip_cavity'
  };
  return typeMapping[type] || null;
};

function getCategoryColor(category) {
  const upperCategory = category?.toUpperCase() || '';

  switch (upperCategory) {
    case "PRODUCTION RUNNING":
      return "#00ff00";

    case "PRODUCT BUYOFF":
      return "#808080";

    case "NO OPERATOR":
    case "NO SCHEDULE":
    case "MATERIAL DRYING":
    case "OTHERS PROD":
      return "#ffff00";

    case "QUALITY ISSUE":
    case "SAMPLE RUNNING":
    case "MOULD CHANGE":
    case "OTHERS TECH":
      return "#ff0000";

    case "SCHEDULED MAINTENANCE":
    case "MACHINE BREAKDOWN":
    case "OTHERS MAIN":
      return "#ffa500";

    default:
      return "#808080";
  }
};

// Serve static files from specific directories
app.use('/static/toolset_img', express.static(staticDirs.toolsetImages));
app.use('/static/docket_pdf', express.static(staticDirs.docketPDFs));

// Download endpoint for dockets
app.get('/api/download/docket/:filename', async (req, res) => {
  try {
    const filename = req.params.filename;
    const filePath = path.join(staticDirs.docketPDFs, filename);

    try {
      await fs.access(filePath);
    } catch {
      return res.status(404).send('File not found');
    }

    res.setHeader('Content-Disposition', `attachment; filename=${filename}`);
    res.setHeader('Content-Type', 'application/pdf');
    res.sendFile(filePath);
  } catch (error) {
    console.error('Download error:', error);
    res.status(500).send('Error downloading file');
  }
});

app.get("/api/status", async (req, res) => {
  try {
    const result = await sql.query`
    DECLARE @date DATETIME;

    -- Calculate Starting Datetime
    IF GETDATE() BETWEEN CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 06:00:00' AS DATETIME) AND CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME)
    BEGIN
        SET @date = CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 06:00:00' AS DATETIME);
    END
    ELSE IF GETDATE() BETWEEN CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME) AND CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 23:59:59' AS DATETIME)
    BEGIN
        SET @date = CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME);
    END
    ELSE
    BEGIN
        SET @date = CAST(CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 112) + ' 18:00:00' AS DATETIME);
    END;

    SELECT 
        COALESCE(SUM(ct.ct * mm.shot), 0) AS SAP_Output_Time,
        COALESCE(SUM(mm.act_ct * mm.shot), 0) AS ACT_Output_Time,
      COALESCE(SUM(ng.defect_second), 0) AS Reject_Time,
        COALESCE(SUM(tu.start_second), 0) AS Run_Time,
        COALESCE(SUM(ts.stop_second), 0) AS Down_Time,
        COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) AS Avail_Time,
        -- Calculate Availability (Run_Time / Avail_Time)
        CASE 
            WHEN COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) = 0 
            THEN 0
            ELSE (CAST(COALESCE(SUM(tu.start_second), 0) AS FLOAT) / 
                (COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0))) * 100
        END AS Avail,
        -- Calculate Performance (SAP_Output_Time / ACT_Output_Time)
        CASE 
            WHEN COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0 
            THEN 0
            ELSE (CAST(COALESCE(SUM(ct.ct * mm.shot), 0) AS FLOAT) / 
                COALESCE(SUM(mm.act_ct * mm.shot), 0)) * 100
        END AS Perf,
        -- Calculate Quality ((ACT_Output_Time - Reject_Time) / ACT_Output_Time)
        CASE 
            WHEN COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0 
            THEN 0
            ELSE (CAST(COALESCE(SUM(mm.act_ct * mm.shot), 0) - COALESCE(SUM(ng.defect_second), 0) AS FLOAT) / 
                COALESCE(SUM(mm.act_ct * mm.shot), 0)) * 100
        END AS Quality,
        -- Calculate OEE (Avail * Perf * Quality)
        CASE 
            WHEN COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) = 0 
                OR COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0
            THEN 0
            ELSE 
                ((CAST(COALESCE(SUM(tu.start_second), 0) AS FLOAT) / 
                (COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0))) *
                (CAST(COALESCE(SUM(ct.ct * mm.shot), 0) AS FLOAT) / 
                COALESCE(SUM(mm.act_ct * mm.shot), 0)) *
                (CAST(COALESCE(SUM(mm.act_ct * mm.shot), 0) - COALESCE(SUM(ng.defect_second), 0) AS FLOAT) / 
                COALESCE(SUM(mm.act_ct * mm.shot), 0))) * 100
        END AS OEE
    FROM 
        OEE.dbo.machine_master mm
    JOIN 
        OEE.dbo.list_ct ct ON mm.id_type = ct.id_type AND mm.mould = ct.mould
    LEFT JOIN (
        SELECT 
            id_machine, 
            SUM(DATEDIFF(second, start, ISNULL(finish, GETDATE()))) AS start_second
        FROM 
            OEE.dbo.trans_uptime
        WHERE 
            start BETWEEN @date AND GETDATE()
        GROUP BY 
            id_machine
    ) tu ON mm.id_machine = tu.id_machine
    LEFT JOIN (
        SELECT 
            id_machine, 
            SUM(DATEDIFF(second, start, ISNULL(finish, GETDATE()))) AS stop_second
        FROM 
            OEE.dbo.trans_stop
        WHERE 
            start BETWEEN @date AND GETDATE()
        GROUP BY 
            id_machine
    ) ts ON mm.id_machine = ts.id_machine
    LEFT JOIN (
        SELECT 
            id_machine, 
            COALESCE(sum((ct / NULLIF(qty_perct, 0)) * qty),0) as defect_second
        FROM 
            OEE.dbo.trans_ng
        WHERE 
            time BETWEEN @date AND GETDATE()
        GROUP BY 
            id_machine
    ) ng ON mm.id_machine = ng.id_machine
    WHERE 
        mm.machine_name <> 'M16';
    `;

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching status data:", error);
    res.status(500).send("Error fetching status data");
  }
});

app.get("/api/machines", async (req, res) => {

  try {
    const result = await sql.query`
    SELECT mm.machine_name, mm.status_start, mm.status_stop,
        UPPER(
            CASE 
                WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                WHEN tt.category IS NULL THEN 
                    CASE 
                        WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                        ELSE 'Status Undefined'
                    END
                ELSE tt.category
            END
        ) AS category,
        CASE 
            WHEN UPPER(
                CASE 
                    WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                    WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                    WHEN tt.category IS NULL THEN 
                        CASE 
                            WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                ELSE 'Status Undefined'
                        END
                    ELSE tt.category
                END
            ) = 'PRODUCTION RUNNING' THEN '#00ff00'
        WHEN UPPER(
                CASE 
                    WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                    WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                    WHEN tt.category IS NULL THEN 
                        CASE 
                            WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                ELSE 'Status Undefined'
                        END
                    ELSE tt.category
                END
            ) IN ('PRODUCT BUYOFF') THEN '#808080'
            WHEN UPPER(
                CASE 
                    WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                    WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                    WHEN tt.category IS NULL THEN 
                        CASE 
                            WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                ELSE 'Status Undefined'
                        END
                    ELSE tt.category
                END
            ) IN ('NO OPERATOR', 'NO SCHEDULE', 'MATERIAL DRYING', 'OTHERS PROD') THEN '#ffff00'
            WHEN UPPER(
                CASE 
                    WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                    WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                    WHEN tt.category IS NULL THEN 
                        CASE 
                            WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                ELSE 'Status Undefined'
                        END
                    ELSE tt.category
                END
            ) IN ('QUALITY ISSUE', 'PRODUCTION SAMPLE', 'MOULD CHANGE', 'OTHERS TECH') THEN '#ff0000'
            WHEN UPPER(
                CASE 
                    WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
                    WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
                    WHEN tt.category IS NULL THEN 
                        CASE 
                            WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running'
                ELSE 'Status Undefined'
                        END
                    ELSE tt.category
                END
            ) IN ('SCHEDULED MAINTENANCE', 'MACHINE BREAKDOWN') THEN '#ffa500'
            ELSE '#ffffff'
        END AS color
    FROM OEE.dbo.machine_master mm
    INNER JOIN OEE.dbo.trans_time tt ON mm.id_machine = tt.id_machine
    LEFT JOIN (
        SELECT id_machine, problem
        FROM OEE.dbo.trans_stop
        WHERE finish IS NULL AND category = 'Others' AND id_machine != 16
    ) ts ON mm.id_machine = ts.id_machine
    WHERE mm.machine_name != 'M16'
    ORDER BY mm.id_machine ASC;
    `;

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching machines data:", error);
    res.status(500).send("Error fetching machines data");
  }
});

app.get("/api/loadMachineMaster", async (req, res) => {
  try {
    const result = await sql.query`
      WITH latest_logs AS (
        SELECT TOP 1 1 as id_machine, machine_name, category FROM CMS.dbo.machine_log_1 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 2, machine_name, category FROM CMS.dbo.machine_log_2 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 3, machine_name, category FROM CMS.dbo.machine_log_3 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 4, machine_name, category FROM CMS.dbo.machine_log_4 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 5, machine_name, category FROM CMS.dbo.machine_log_5 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 6, machine_name, category FROM CMS.dbo.machine_log_6 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 7, machine_name, category FROM CMS.dbo.machine_log_7 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 8, machine_name, category FROM CMS.dbo.machine_log_8 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 9, machine_name, category FROM CMS.dbo.machine_log_9 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 10, machine_name, category FROM CMS.dbo.machine_log_10 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 11, machine_name, category FROM CMS.dbo.machine_log_11 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 12, machine_name, category FROM CMS.dbo.machine_log_12 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 13, machine_name, category FROM CMS.dbo.machine_log_13 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 14, machine_name, category FROM CMS.dbo.machine_log_14 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 15, machine_name, category FROM CMS.dbo.machine_log_15 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 16, machine_name, category FROM CMS.dbo.machine_log_16 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 17, machine_name, category FROM CMS.dbo.machine_log_17 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 18, machine_name, category FROM CMS.dbo.machine_log_18 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 19, machine_name, category FROM CMS.dbo.machine_log_19 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 20, machine_name, category FROM CMS.dbo.machine_log_20 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 21, machine_name, category FROM CMS.dbo.machine_log_21 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 22, machine_name, category FROM CMS.dbo.machine_log_22 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 23, machine_name, category FROM CMS.dbo.machine_log_23 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 24, machine_name, category FROM CMS.dbo.machine_log_24 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 25, machine_name, category FROM CMS.dbo.machine_log_25 ORDER BY start DESC
        UNION ALL
        SELECT TOP 1 26, machine_name, category FROM CMS.dbo.machine_log_26 ORDER BY start DESC
      )
      SELECT
        mm.machine_name,
        mm.shot * mm.qty_perct AS output,
        mm.act_ct,
        mm.type,
        CASE 
          WHEN (ll.category IS NULL OR ll.category = '') 
               AND mm.status_start = 1 
               AND mm.status_off = 1 
          THEN 'PRODUCTION RUNNING'
          ELSE COALESCE(ll.category, '')
        END AS category
      FROM CMS.dbo.machine_master mm
      LEFT JOIN latest_logs ll ON mm.id_machine = ll.id_machine
    `;

    const results = result.recordset.map(record => ({
      machine_name: record.machine_name,
      type: record.type,
      category: record.category,
      output: record.output,
      act_ct: record.act_ct,
      color: getCategoryColor(record.category)
    }));

    res.json(results);
  } catch (error) {
    console.error("Error fetching machine master data:", error);
    console.error("Error details:", error.message);
    res.status(500).json({ error: "Error fetching machine master data", details: error.message });
  }
});

app.get("/api/timeline", async (req, res) => {

  try {
    const result = await sql.query`
    WITH timeline AS (
      SELECT 
          id_machine,
          id_type,
          mould,
          COALESCE(start, GETDATE()) AS start,
          COALESCE(finish, GETDATE()) AS finish,
          COALESCE(category, 'Undefined') AS category,
          mould_category,
          shift,
          production_date
      FROM OEE.dbo.trans_stop
      WHERE production_date = 
          CASE 
              WHEN CAST(GETDATE() AS TIME) BETWEEN '00:00:00' AND '05:59:59' THEN DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
              ELSE CAST(GETDATE() AS DATE)
          END
      AND shift = 
          CASE 
              WHEN CAST(GETDATE() AS TIME) BETWEEN '06:00:00' AND '17:59:59' THEN 1
              ELSE 2
          END
      
      UNION ALL
      
      SELECT 
          id_machine,
          id_type,
          mould,
          COALESCE(start, GETDATE()) AS start,
          COALESCE(finish, GETDATE()) AS finish,
          'Production Running' AS category,
          NULL AS mould_category,
          shift,
          production_date
      FROM OEE.dbo.trans_uptime
      WHERE production_date = 
          CASE 
              WHEN CAST(GETDATE() AS TIME) BETWEEN '00:00:00' AND '05:59:59' THEN DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
              ELSE CAST(GETDATE() AS DATE)
          END
      AND shift = 
          CASE 
              WHEN CAST(GETDATE() AS TIME) BETWEEN '06:00:00' AND '17:59:59' THEN 1
              ELSE 2
          END
    )
    SELECT 
        mm.machine_name,
        tl.id_machine,
        lc.type AS product,
        tl.id_type,
        tl.mould,
        tl.start,
        tl.finish,
        DATEDIFF(MINUTE, tl.start, tl.finish) / 60.0 AS duration,
        tl.category,
        tl.mould_category,
        (mm.shot * mm.qty_perct) AS output,
        CASE 
            WHEN DATEPART(HOUR, GETDATE()) BETWEEN 6 AND 17 THEN 
                (((DATEPART(HOUR, GETDATE()) - 6) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
            WHEN DATEPART(HOUR, GETDATE()) < 6 THEN 
                (((DATEPART(HOUR, GETDATE()) + 24 - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
            ELSE 
                (((DATEPART(HOUR, GETDATE()) - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
        END AS plan_output,
        ROUND(
            COALESCE(
                ((mm.shot * mm.qty_perct) / 
                CASE 
                    WHEN DATEPART(HOUR, GETDATE()) BETWEEN 6 AND 17 THEN 
                        (((DATEPART(HOUR, GETDATE()) - 6) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
                    WHEN DATEPART(HOUR, GETDATE()) < 6 THEN 
                        (((DATEPART(HOUR, GETDATE()) + 24 - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
                    ELSE 
                        (((DATEPART(HOUR, GETDATE()) - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
                END) * 100, 0
            ), 2
        ) AS efficiency,
        tl.shift,
        tl.production_date,
    CASE
      WHEN tl.category = 'Production Running' THEN 'rgb(0, 255, 0)'
      WHEN tl.category IN ('No Operator', 'No Schedule', 'Material Drying', 'Others') THEN 'rgb(255, 255, 0)'
      WHEN tl.category IN ('Machine Breakdown', 'Scheduled Maintenance') THEN 'rgb(255, 165, 0)'
      WHEN tl.category = 'Product Buyoff' THEN 'rgb(128, 128, 128)'
      WHEN tl.category IN ('Quality Issue', 'Production Sample', 'Mould Change') THEN 'rgb(255, 0, 0)'
      ELSE 'rgb(255, 255, 255)'
    END AS color
    FROM timeline tl
    LEFT JOIN OEE.dbo.list_ct lc
        ON tl.id_type = lc.id_type AND tl.mould = lc.mould
    LEFT JOIN OEE.dbo.machine_master mm
        ON tl.id_machine = mm.id_machine
    WHERE mm.machine_name <> 'M16'
    ORDER BY tl.start DESC;       
    `;

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching timeline data:", error);
    res.status(500).send("Error fetching timeline data");
  }
});

app.get("/api/preparation", async (req, res) => {
  try {
    const result = await sql.query`
    SELECT * FROM preparation
    `;

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching sharing data:", error);
    res.status(500).send("Error fetching sharing data");
  }
});

app.get("/api/sharing-product", async (req, res) => {
  const { type } = req.query;

  try {
    const result = await sql.query`EXEC GetSharingProduct @Type = ${type}`;
    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching sharing product:", error);
    res.status(500).send("Error fetching sharing product");
  }
});

app.get("/api/list-history", async (req, res) => {

  try {
    const result = await sql.query`
    SELECT * FROM list_history ORDER BY DATETIME DESC, ITEM ASC
    `;

    res.json(result.recordset.map(record => {
      record.DATETIME = formatDatetime(record.DATETIME);
      return record;
    }));
  } catch (error) {
    console.error("Error fetching list history:", error);
    res.status(500).send("Error fetching list history");
  }
});

app.post("/api/list-history", async (req, res) => {
  const items = req.body;

  try {

    if (!Array.isArray(items)) {
      // Case: Single object received
      const { ITEM, S_NUM, FROM, TO, STATUS, REMARK, RESET } = items;

      await sql.query`
          DECLARE @currentDate DATETIME = GETDATE();

          INSERT INTO list_history (ITEM, S_NUM, [FROM], [TO], DATETIME, STATUS, REMARK)
          VALUES (${ITEM}, ${S_NUM}, ${FROM}, ${TO}, @currentDate, ${STATUS}, ${REMARK})

          UPDATE full_list 
          SET LOCATION = ${TO}, 
              REMARK = ${REMARK},
              USAGE = CASE WHEN ${RESET} = 1 THEN 0 ELSE USAGE END,
              LAST_SERV = @currentDate,
              PLAN_SERV = CASE 
                   WHEN REPEAT IS NULL THEN NULL 
                   ELSE DATEADD(DAY, REPEAT, @currentDate) 
               END
          WHERE S_NUM = ${S_NUM};
        `;
      res.status(200).send("Item added successfully.");
    } else {
      // Case: Array of objects received
      for (const item of items) {

        const { ITEM, S_NUM, TYPE, FROM, TO, STATUS, REMARK } = item;

        // Get the column name dynamically in table mach_details
        const columnName = getColumnName(TYPE);
        if (!columnName) {
          console.error(`Invalid TYPE: ${TYPE}`);
          continue;
        }

        const request = new sql.Request();
        request.input('item', sql.NVarChar, ITEM);
        request.input('to', sql.NVarChar, TO);

        const updateMachDetailsQuery = `
          UPDATE mach_details 
          SET [${columnName}] = @item 
          WHERE machine_name = @to
        `;

        await request.query(updateMachDetailsQuery);

        await sql.query`
          INSERT INTO list_history (ITEM, S_NUM, [FROM], [TO], DATETIME, STATUS, REMARK)
          VALUES (${ITEM}, ${S_NUM}, ${FROM}, ${TO}, GETDATE(), ${STATUS}, ${REMARK})

          UPDATE full_list SET LOCATION = ${TO}, REMARK = ${REMARK} WHERE S_NUM = ${S_NUM}
        `;
      }

      await sql.query`
      WITH RankedMatches AS (
        SELECT 
                t2.machine_name,
                t2.wait_type,
            t2.type,
                t2.back_plate, 
                t2.base_mould, 
                t2.blow_core, 
                t2.ejector, 
                t2.hot_runner, 
                t2.injection_cavity, 
                t2.injection_core, 
                t2.lip_cavity,
                t1.type AS matched_product_name,
                ROW_NUMBER() OVER (PARTITION BY t2.machine_name ORDER BY 
                    -- Count the number of matching toolset components
                    (CASE WHEN t1.back_plate = t2.back_plate THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.base_mould = t2.base_mould THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.blow_core = t2.blow_core THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.ejector = t2.ejector THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.hot_runner = t2.hot_runner THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.injection_cavity = t2.injection_cavity THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.injection_core = t2.injection_core THEN 1 ELSE 0 END) +
                    (CASE WHEN t1.lip_cavity = t2.lip_cavity THEN 1 ELSE 0 END) DESC
                ) AS rank
            FROM mach_details t2
          JOIN preparation t1 ON 
                -- Ensure at least one toolset match
                (t1.back_plate = t2.back_plate OR
                 t1.base_mould = t2.base_mould OR
                 t1.blow_core = t2.blow_core OR
                 t1.ejector = t2.ejector OR
                 t1.hot_runner = t2.hot_runner OR
                 t1.injection_cavity = t2.injection_cavity OR
                 t1.injection_core = t2.injection_core OR
                 t1.lip_cavity = t2.lip_cavity)
        )
        UPDATE t2
        SET t2.wait_type = rm.matched_product_name
        FROM mach_details t2
        JOIN RankedMatches rm 
            ON t2.machine_name = rm.machine_name
            AND rm.rank = 1;
        `;

      res.status(200).send("All items added successfully.");
    }
  } catch (error) {
    console.error("Error inserting list_history:", error);
    res.status(500).send("Error inserting list_history");
  }
});

app.post("/api/upload-images-server", upload.array("images[]"), async (req, res) => {
  try {
    if (!req.files || req.files.length === 0) {
      return res.status(400).send("No files uploaded.");
    }
    res.status(200).send("Images saved successfully.");
  } catch (error) {
    console.error("Upload error:", error);
    res.status(500).send({ message: "Upload failed", error });
  }
});

app.post("/api/upload-images-sql", async (req, res) => {
  const { ITEM, S_NUM, FROM, TO, DATETIME, STATUS, IMG_NAME } = req.body;

  try {
    await sql.query`
      UPDATE list_history SET IMG_NAME = ${IMG_NAME} WHERE 
        ITEM = ${ITEM} AND 
        S_NUM = ${S_NUM} AND 
        [FROM] = ${FROM} AND 
        [TO] = ${TO} AND 
        DATETIME = ${DATETIME} AND
        STATUS = ${STATUS}
    `;

    res.status(200).send("Image names stored in SQL database successfully.");
  } catch (error) {
    console.error("Error saving image names to SQL database:", error);
    res.status(500).send("Error saving image names to SQL database.");
  }
});

app.put("/api/upload-images", async (req, res) => {
  const { ITEM, S_NUM, FROM, TO, DATETIME, STATUS, IMG_NAME } = req.body;

  const files = IMG_NAME.split(",").map(name => name.trim());

  for (const file of files) {
    const filePath = path.join(__dirname, "dist/toolset_img", file);
    try {
      await fs.unlink(filePath);
    } catch (err) {
      console.error(`Failed to delete file: ${filePath}.`, err);
    }
  }

  try {
    const FORMATDATE = formatDatetime(DATETIME);

    await sql.query`
      UPDATE list_history SET IMG_NAME = NULL WHERE 
        ITEM = ${ITEM} AND 
        S_NUM = ${S_NUM} AND 
        [FROM] = ${FROM} AND 
        [TO] = ${TO} AND 
        DATETIME = ${FORMATDATE} AND
        STATUS = ${STATUS}
    `;

    res.status(200).send("Images deleted successfully.");
  } catch (error) {
    console.error("Error deleting images:", error);
    res.status(500).send("Error deleting images.");
  }
});

app.get("/api/full-list", async (req, res) => {
  try {
    const result = await sql.query(`
      SELECT * FROM full_list WHERE S_NUM IS NOT NULL
    `);

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching full list:", error);
    res.status(500).send("Error fetching full list");
  }
});

app.post("/api/full-list", async (req, res) => {
  const FORM = req.body;
  try {
    await sql.query(`
      INSERT INTO full_list (ITEM, S_NUM, TYPE, RACK, LEVEL, NO, STATUS, REMARK, ACCUM_USAGE, USAGE, PLAN_USAGE, LAST_SERV, PLAN_SERV, REPEAT)
      VALUES (${FORM.ITEM}, ${FORM.S_NUM}, ${FORM.TYPE}, ${FORM.RACK}, ${FORM.LEVEL},, ${FORM.STATUS}, ${FORM.REMARK}, 0, 0, ${FORM.PLAN_USAGE}, GETDATE(), ${FORM.PLAN_SERV}, ${FORM.REPEAT})
    `);

    res.send("full list inserted successfully");
  } catch (error) {
    console.error("Error inserting full list:", error);
    res.status(500).send("Error inserting full list");
  }
});

app.put("/api/full-list/:S_NUM", async (req, res) => {
  const S_NUM = req.params;
  const FORM = req.body;

  try {
    await sql.query`
      UPDATE full_list SET
        ITEM = ${FORM.ITEM},
        RACK = ${FORM.RACK},
        LEVEL = ${FORM.LEVEL},
        LOCATION = ${FORM.LOCATION},
        STATUS = ${FORM.STATUS},
        REMARK = ${FORM.REMARK},
        USAGE = ${FORM.USAGE},
        LAST_SERV = ${FORM.LAST_SERV}
      WHERE S_NUM = ${S_NUM}
    `;

    res.send("full list item updated successfully");
  } catch (error) {
    console.error("Error updating full list item:", error);
    res.status(500).send("Error updating full list item");
  }
});

app.delete("/api/full-list/:S_NUM", async (req, res) => {
  const S_NUM = req.params;

  try {
    await sql.query`
      DELETE FROM full_list
      WHERE S_NUM = ${S_NUM}
    `;

    res.send("full list item deleted successfully");
  } catch (error) {
    console.error("Error deleting full list item:", error);
    res.status(500).send("Error deleting full list item");
  }
});

app.get("/api/locations", async (req, res) => {
  try {
    const result = await sql.query(`
      SELECT * FROM location
    `);

    res.json(result.recordset);
  } catch (error) {
    console.error("Error fetching location:", error);
    res.status(500).send("Error fetching location");
  }
});

app.get("/public/list/:fileName", (req, res) => {
  const { fileName } = req.params;
  const filePath = path.join(__dirname, "public", "list", `${fileName}`);

  // Read and return the content of the file
  fs.readFile(filePath, "utf-8", (err, data) => {
    if (err) {
      console.error(`Error reading file: ${fileName}`, err);
      return res.status(500).json({ error: `Unable to read file: ${fileName}` });
    }
    res.send(data);
  });
});

app.post("/api/save-file", async (req, res) => {
  const { fileName, content } = req.body;
  const filePath = path.join(__dirname, "public", "list", `${fileName}.txt`);

  try {
    await fs.writeFile(filePath, content);
    res.send("File saved successfully");
  } catch (err) {
    console.error(err);
    res.status(500).send("Failed to save file");
  }
});

app.post("/api/update-repeat", async (req, res) => {
  const data = req.body;

  try {
    for (const sNum of data.S_NUM) {
      await sql.query`
        UPDATE full_list
        SET 
          PLAN_SERV = ${data.PLAN_SERV},
          PLAN_USAGE = ${data.PLAN_USAGE},
          REPEAT = ${data.REPEAT}
        WHERE S_NUM = ${sNum}`;
    }

    res.status(200).send("Data updated successfully.");
  } catch (error) {
    console.error("Error updating Data:", error);
    res.status(500).send("Error updating Data.");
  }
});

app.post('/api/dockets', async (req, res) => {
  const data = req.body;

  // Paths for storing PDFs and retrieving images
  const docketDir = path.join(__dirname, "dist/docket_pdf");
  const imgDir = path.join(__dirname, "dist/toolset_img");

  pdfMake.vfs = pdfFonts;

  try {
    const year = new Date(data.DATETIME).getFullYear();

    const files = await fs.readdir(docketDir);
    const today = new Date();
    const formattedDate = today.toISOString().slice(0, 10);

    // Filter files with the same year
    const matchingFilesByYear = files.filter((file) => file.startsWith(`${year}-`));
    const highestCount = matchingFilesByYear.length;

    const pdfName = `${formattedDate}(${highestCount + 1}).pdf`;
    const pdfPath = path.join(docketDir, pdfName);

    await sql.query`
        INSERT INTO list_docket (ID, ITEM, S_NUM, PDF_NAME, VENDOR, DATETIME, YEAR_CREATED)
        VALUES (${highestCount + 1}, ${data.ITEM}, ${data.S_NUM}, ${pdfName}, ${data.VENDOR}, ${data.DATETIME}, ${year})
        
        UPDATE full_list set REMARK = ${data.remarksDetails} where S_NUM = ${data.S_NUM}`

    const base64Images = await Promise.all(
      data.images.map(async (image) => {
        const filePath = path.join(imgDir, image);
        const fileData = await fs.readFile(filePath);
        return `data:image/jpeg;base64,${fileData.toString('base64')}`;
      })
    );

    const jjLogo = await fs.readFile(path.join(__dirname, "src/assets/JJfullblue.png"));

    const pdfTemplate = {
      pageSize: "A4",
      pageOrientation: "portrait",
      pageMargins: [40, 60, 40, 60],
      content: [
        {
          image: `data:image/png;base64,${jjLogo.toString('base64')}`,
          width: 150,
          alignment: 'center',
          margin: [0, 0, 0, 20]
        },
        {
          text: "Toolset's Docket - Revision 1",
          alignment: "center",
          style: "header"
        },
        {
          text: "To record parts, moulds & toolset sent out from JJPM-SB",
          alignment: "center",
          style: "subheader",
          margin: [0, 0, 0, 30]
        },
        { text: "1. Vendor Company Name", bold: true, margin: [0, 10, 0, 10] },
        { text: data.vendorName, margin: [10, 10, 0, 10] },
        { text: "2. Vendor PIC Name", bold: true, margin: [0, 10, 0, 10] },
        { text: data.picName, margin: [10, 10, 0, 10] },
        { text: "3. Date OUT", bold: true, margin: [0, 10, 0, 10] },
        { text: data.dateOut, margin: [10, 10, 0, 10] },
        { text: "4. Time OUT", bold: true, margin: [0, 10, 0, 10] },
        { text: data.timeOut, margin: [10, 10, 0, 10] },
        { text: "5. Target Date IN", bold: true, margin: [0, 10, 0, 10] },
        { text: data.dateIn, margin: [10, 10, 0, 10] },
        { text: "6. Purpose for Toolset Send Out", bold: true, margin: [0, 10, 0, 10] },
        { text: data.selectPurpose, margin: [10, 10, 0, 10] },
        { text: "7. Details (Model)", bold: true, margin: [0, 10, 0, 10] },
        { text: data.modelDetails, margin: [10, 10, 0, 10] },
        { text: "8. Details (Parts)", bold: true, margin: [0, 10, 0, 10] },
        { text: data.partsDetails, margin: [10, 10, 0, 10] },
        { text: "9. Details (Remarks)", bold: true, margin: [0, 10, 0, 10] },
        { text: data.remarksDetails, margin: [10, 10, 0, 10] },
        { text: "10. Docket Prepared By", bold: true, margin: [0, 10, 0, 10] },
        { text: data.selectPrepared, margin: [10, 10, 0, 10] },
        {
          text: "11. Photo & Evidence (Including Car Plate & Toolset)",
          alignment: "left",
          bold: true,
          margin: [0, 10, 0, 10]
        },
        ...base64Images.map(image => ({
          image: image,
          width: 150,
          margin: [0, 10, 0, 10]
        }))
      ],
      styles: {
        header: {
          fontSize: 22,
          bold: true,
          margin: [0, 0, 0, 10]
        },
        subheader: {
          fontSize: 16,
          margin: [0, 0, 0, 20]
        },
        tableHeader: {
          bold: true,
          fontSize: 12,
          color: 'black'
        }
      }
    };

    pdfMake.createPdf(pdfTemplate).getBuffer(async (buffer) => {
      await fs.writeFile(pdfPath, buffer);
    });

    res.status(201).send("Docket saved successfully.");
  } catch (error) {
    console.error("Error saving docket:", error);
    res.status(500).send("Error saving docket.");
  }
});

app.get("/api/dockets", async (req, res) => {
  try {
    const dockets = await sql.query`
      SELECT * FROM list_docket ORDER BY DATETIME DESC`;
    res.send(dockets.recordset);
  } catch (error) {
    console.error("Error fetching dockets:", error);
    res.status(500).send("Error fetching dockets.");
  }
});

app.delete("/api/dockets/:id", async (req, res) => {
  const { id } = req.params;

  try {
    // Retrieve the existing docket details
    const existingDocket = await sql.query`
      SELECT * FROM list_docket WHERE PDF_NAME = ${id}`;
    if (existingDocket.recordset.length === 0) {
      return res.status(404).send("Docket not found.");
    }

    // Delete the PDF file
    const filePath = path.join(__dirname, "dist/docket_pdf", id);
    await fs.unlink(filePath);

    // Delete the database record
    await sql.query`
      DELETE FROM list_docket WHERE PDF_NAME = ${id}`;

    res.status(200).send("Docket deleted successfully.");
  } catch (error) {
    console.error("Error deleting docket:", error);
    res.status(500).send("Error deleting docket.");
  }
});

// Serve static files
app.use(express.static(path.join(__dirname, 'dist')));

app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist', 'index.html'));
});

const startServer = async () => {
  try {
    // Connect to database
    await sql.connect(config);
    console.log("Connected to SQL Server successfully");

    // Start server
    app.listen(PORT, VITE_SERVER_IP, () => {
      console.log(`Server running at http://${VITE_SERVER_IP}:${PORT}`);
    });
  } catch (err) {
    console.error("Startup Error:", err);
    process.exit(1);
  }
};

startServer();
