using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoMS.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddListOptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "full_list",
                columns: table => new
                {
                    S_NUM = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ITEM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RACK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LEVEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NO = table.Column<int>(type: "int", nullable: true),
                    LOCATION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    REMARK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCUM_USAGE = table.Column<long>(type: "bigint", nullable: true),
                    USAGE = table.Column<long>(type: "bigint", nullable: true),
                    PLAN_USAGE = table.Column<long>(type: "bigint", nullable: true),
                    LAST_SERV = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PLAN_SERV = table.Column<DateTime>(type: "datetime2", nullable: true),
                    REPEAT = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_full_list", x => x.S_NUM);
                });

            migrationBuilder.CreateTable(
                name: "list_docket",
                columns: table => new
                {
                    PDF_NAME = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ID = table.Column<int>(type: "int", nullable: false),
                    ITEM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    S_NUM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VENDOR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATETIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YEAR_CREATED = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_docket", x => x.PDF_NAME);
                });

            migrationBuilder.CreateTable(
                name: "list_option",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_option", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "location",
                columns: table => new
                {
                    LOCATION = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CATEGORY = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location", x => x.LOCATION);
                });

            migrationBuilder.CreateTable(
                name: "preparation",
                columns: table => new
                {
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    back_plate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    base_mould = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    blow_core = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    blow_mould = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ejector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hot_runner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    injection_cavity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    injection_core = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lip_cavity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.InsertData(
                table: "list_option",
                columns: new[] { "id", "category", "sort_order", "value" },
                values: new object[,]
                {
                    { 1, "mould_maker", 1, "RB" },
                    { 2, "mould_maker", 2, "V. TOPLAS" },
                    { 3, "mould_maker", 3, "YH. ENG" },
                    { 4, "mould_maker", 4, "GS MOULD" },
                    { 5, "prepared", 1, "Hew CP" },
                    { 6, "prepared", 2, "Low PS" },
                    { 7, "prepared", 3, "Ameera" },
                    { 8, "prepared", 4, "Kamarul" },
                    { 9, "prepared", 5, "Alif" },
                    { 10, "production", 1, "A5" },
                    { 11, "production", 2, "A6" },
                    { 12, "production", 3, "A7" },
                    { 13, "production", 4, "A8" },
                    { 14, "production", 5, "A9" },
                    { 15, "production", 6, "A10" },
                    { 16, "production", 7, "A12" },
                    { 17, "production", 8, "A13" },
                    { 18, "production", 9, "A14" },
                    { 19, "production", 10, "A15" },
                    { 20, "production", 11, "A16" },
                    { 21, "production", 12, "A17" },
                    { 22, "production", 13, "A18" },
                    { 23, "production", 14, "A19" },
                    { 24, "production", 15, "A21" },
                    { 25, "purpose", 1, "Sample/Development" },
                    { 26, "purpose", 2, "Repair" },
                    { 27, "purpose", 3, "Modification" },
                    { 28, "purpose", 4, "Service" },
                    { 29, "vendor", 1, "JLG" },
                    { 30, "vendor", 2, "SNS" },
                    { 31, "vendor", 3, "JUSEN" },
                    { 32, "vendor", 4, "MAGNUM" },
                    { 33, "vendor", 5, "SERVICE" },
                    { 34, "rack", 1, "1A" },
                    { 35, "rack", 2, "1B" },
                    { 36, "rack", 3, "1C" },
                    { 37, "rack", 4, "1D" },
                    { 38, "rack", 5, "2A" },
                    { 39, "rack", 6, "2B" },
                    { 40, "rack", 7, "2C" },
                    { 41, "rack", 8, "2D" },
                    { 42, "rack", 9, "3A" },
                    { 43, "rack", 10, "3B" },
                    { 44, "rack", 11, "3C" },
                    { 45, "rack", 12, "3D" },
                    { 46, "rack", 13, "4A" },
                    { 47, "rack", 14, "4B" },
                    { 48, "rack", 15, "4C" },
                    { 49, "rack", 16, "4D" },
                    { 50, "rack", 17, "5A" },
                    { 51, "rack", 18, "5B" },
                    { 52, "rack", 19, "5C" },
                    { 53, "rack", 20, "5D" },
                    { 54, "rack", 21, "6A" },
                    { 55, "rack", 22, "6B" },
                    { 56, "rack", 23, "6C" },
                    { 57, "rack", 24, "6D" },
                    { 58, "rack", 25, "7A" },
                    { 59, "rack", 26, "7B" },
                    { 60, "rack", 27, "7C" },
                    { 61, "rack", 28, "7D" },
                    { 62, "rack", 29, "8A" },
                    { 63, "rack", 30, "8B" },
                    { 64, "rack", 31, "8C" },
                    { 65, "rack", 32, "8D" },
                    { 66, "rack", 33, "9A" },
                    { 67, "rack", 34, "9B" },
                    { 68, "rack", 35, "9C" },
                    { 69, "rack", 36, "9D" },
                    { 70, "rack", 37, "10A" },
                    { 71, "rack", 38, "10B" },
                    { 72, "rack", 39, "10C" },
                    { 73, "rack", 40, "10D" },
                    { 74, "rack", 41, "11A" },
                    { 75, "rack", 42, "11B" },
                    { 76, "rack", 43, "11C" },
                    { 77, "rack", 44, "11D" },
                    { 78, "rack", 45, "12A" },
                    { 79, "rack", 46, "12B" },
                    { 80, "rack", 47, "12C" },
                    { 81, "rack", 48, "12D" },
                    { 82, "rack", 49, "13A" },
                    { 83, "rack", 50, "13B" },
                    { 84, "rack", 51, "13C" },
                    { 85, "rack", 52, "13D" },
                    { 86, "rack", 53, "14A" },
                    { 87, "rack", 54, "14B" },
                    { 88, "rack", 55, "14C" },
                    { 89, "rack", 56, "14D" },
                    { 90, "rack", 57, "15A" },
                    { 91, "rack", 58, "15B" },
                    { 92, "rack", 59, "15C" },
                    { 93, "rack", 60, "15D" },
                    { 94, "rack", 61, "16A" },
                    { 95, "rack", 62, "16B" },
                    { 96, "rack", 63, "16C" },
                    { 97, "rack", 64, "16D" },
                    { 98, "rack", 65, "17A" },
                    { 99, "rack", 66, "17B" },
                    { 100, "rack", 67, "17C" },
                    { 101, "rack", 68, "17D" },
                    { 102, "rack", 69, "18A" },
                    { 103, "rack", 70, "18B" },
                    { 104, "rack", 71, "18C" },
                    { 105, "rack", 72, "18D" },
                    { 106, "rack", 73, "19A" },
                    { 107, "rack", 74, "19B" },
                    { 108, "rack", 75, "19C" },
                    { 109, "rack", 76, "19D" },
                    { 110, "rack", 77, "20A" },
                    { 111, "rack", 78, "20B" },
                    { 112, "rack", 79, "20C" },
                    { 113, "rack", 80, "20D" },
                    { 114, "rack", 81, "21A" },
                    { 115, "rack", 82, "21B" },
                    { 116, "rack", 83, "21C" },
                    { 117, "rack", 84, "21D" },
                    { 118, "rack", 85, "22A" },
                    { 119, "rack", 86, "22B" },
                    { 120, "rack", 87, "22C" },
                    { 121, "rack", 88, "22D" },
                    { 122, "rack", 89, "23A" },
                    { 123, "rack", 90, "23B" },
                    { 124, "rack", 91, "23C" },
                    { 125, "rack", 92, "23D" },
                    { 126, "rack", 93, "24A" },
                    { 127, "rack", 94, "24B" },
                    { 128, "rack", 95, "24C" },
                    { 129, "rack", 96, "24D" },
                    { 130, "rack", 97, "25A" },
                    { 131, "rack", 98, "25B" },
                    { 132, "rack", 99, "25C" },
                    { 133, "rack", 100, "25D" },
                    { 134, "rack", 101, "26A" },
                    { 135, "rack", 102, "26B" },
                    { 136, "rack", 103, "26C" },
                    { 137, "rack", 104, "26D" },
                    { 138, "rack", 105, "A1" },
                    { 139, "rack", 106, "A2" },
                    { 140, "rack", 107, "A3" },
                    { 141, "rack", 108, "A4" },
                    { 142, "rack", 109, "A5" },
                    { 143, "rack", 110, "A6" },
                    { 144, "rack", 111, "A7" },
                    { 145, "rack", 112, "A8" },
                    { 146, "rack", 113, "A9" },
                    { 147, "rack", 114, "A10" },
                    { 148, "rack", 115, "A11" },
                    { 149, "rack", 116, "A12" },
                    { 150, "rack", 117, "A13" },
                    { 151, "rack", 118, "A14" },
                    { 152, "rack", 119, "A15" },
                    { 153, "rack", 120, "A16" },
                    { 154, "rack", 121, "A17" },
                    { 155, "rack", 122, "A18" },
                    { 156, "rack", 123, "A19" },
                    { 157, "rack", 124, "A20" },
                    { 158, "rack", 125, "A21" },
                    { 159, "rack", 126, "A22" },
                    { 160, "rack", 127, "A23" },
                    { 161, "rack", 128, "A24" },
                    { 162, "rack", 129, "B1" },
                    { 163, "rack", 130, "B2" },
                    { 164, "rack", 131, "B3" },
                    { 165, "rack", 132, "B4" },
                    { 166, "rack", 133, "B5" },
                    { 167, "rack", 134, "B6" },
                    { 168, "rack", 135, "B7" },
                    { 169, "rack", 136, "B8" },
                    { 170, "rack", 137, "B9" },
                    { 171, "rack", 138, "B10" },
                    { 172, "rack", 139, "B11" },
                    { 173, "rack", 140, "B12" },
                    { 174, "rack", 141, "B13" },
                    { 175, "rack", 142, "B14" },
                    { 176, "rack", 143, "B15" },
                    { 177, "rack", 144, "B16" },
                    { 178, "rack", 145, "B17" },
                    { 179, "rack", 146, "B18" },
                    { 180, "rack", 147, "B19" },
                    { 181, "rack", 148, "B20" },
                    { 182, "rack", 149, "B21" },
                    { 183, "rack", 150, "B22" },
                    { 184, "rack", 151, "B23" },
                    { 185, "rack", 152, "B24" },
                    { 186, "rack", 153, "C1" },
                    { 187, "rack", 154, "C2" },
                    { 188, "rack", 155, "C3" },
                    { 189, "rack", 156, "C4" },
                    { 190, "rack", 157, "C5" },
                    { 191, "rack", 158, "C6" },
                    { 192, "rack", 159, "C7" },
                    { 193, "rack", 160, "C8" },
                    { 194, "rack", 161, "C9" },
                    { 195, "rack", 162, "C10" },
                    { 196, "rack", 163, "C11" },
                    { 197, "rack", 164, "C12" },
                    { 198, "rack", 165, "C13" },
                    { 199, "rack", 166, "C14" },
                    { 200, "rack", 167, "C15" },
                    { 201, "rack", 168, "C16" },
                    { 202, "rack", 169, "C17" },
                    { 203, "rack", 170, "C18" },
                    { 204, "rack", 171, "C19" },
                    { 205, "rack", 172, "C20" },
                    { 206, "rack", 173, "C21" },
                    { 207, "rack", 174, "C22" },
                    { 208, "rack", 175, "C23" },
                    { 209, "rack", 176, "C24" },
                    { 210, "rack", 177, "D1" },
                    { 211, "rack", 178, "D2" },
                    { 212, "rack", 179, "D3" },
                    { 213, "rack", 180, "D4" },
                    { 214, "rack", 181, "D5" },
                    { 215, "rack", 182, "D6" },
                    { 216, "rack", 183, "D7" },
                    { 217, "rack", 184, "D8" },
                    { 218, "rack", 185, "D9" },
                    { 219, "rack", 186, "D10" },
                    { 220, "rack", 187, "D11" },
                    { 221, "rack", 188, "D12" },
                    { 222, "rack", 189, "D13" },
                    { 223, "rack", 190, "D14" },
                    { 224, "rack", 191, "D15" },
                    { 225, "rack", 192, "D16" },
                    { 226, "rack", 193, "D17" },
                    { 227, "rack", 194, "D18" },
                    { 228, "rack", 195, "D19" },
                    { 229, "rack", 196, "D20" },
                    { 230, "rack", 197, "D21" },
                    { 231, "rack", 198, "D22" },
                    { 232, "rack", 199, "D23" },
                    { 233, "rack", 200, "D24" },
                    { 234, "rack", 201, "E1" },
                    { 235, "rack", 202, "E2" },
                    { 236, "rack", 203, "E3" },
                    { 237, "rack", 204, "E4" },
                    { 238, "rack", 205, "E5" },
                    { 239, "rack", 206, "E6" },
                    { 240, "rack", 207, "E7" },
                    { 241, "rack", 208, "E8" },
                    { 242, "rack", 209, "E9" },
                    { 243, "rack", 210, "E10" },
                    { 244, "rack", 211, "E11" },
                    { 245, "rack", 212, "E12" },
                    { 246, "rack", 213, "E13" },
                    { 247, "rack", 214, "E14" },
                    { 248, "rack", 215, "E15" },
                    { 249, "rack", 216, "E16" },
                    { 250, "rack", 217, "E17" },
                    { 251, "rack", 218, "E18" },
                    { 252, "rack", 219, "E19" },
                    { 253, "rack", 220, "E20" },
                    { 254, "rack", 221, "E21" },
                    { 255, "rack", 222, "E22" },
                    { 256, "rack", 223, "E23" },
                    { 257, "rack", 224, "E24" },
                    { 258, "rack", 225, "F1" },
                    { 259, "rack", 226, "F2" },
                    { 260, "rack", 227, "F3" },
                    { 261, "rack", 228, "F4" },
                    { 262, "rack", 229, "F5" },
                    { 263, "rack", 230, "F6" },
                    { 264, "rack", 231, "F7" },
                    { 265, "rack", 232, "F8" },
                    { 266, "rack", 233, "F9" },
                    { 267, "rack", 234, "F10" },
                    { 268, "rack", 235, "F11" },
                    { 269, "rack", 236, "F12" },
                    { 270, "rack", 237, "F13" },
                    { 271, "rack", 238, "F14" },
                    { 272, "rack", 239, "F15" },
                    { 273, "rack", 240, "F16" },
                    { 274, "rack", 241, "F17" },
                    { 275, "rack", 242, "F18" },
                    { 276, "rack", 243, "F19" },
                    { 277, "rack", 244, "F20" },
                    { 278, "rack", 245, "F21" },
                    { 279, "rack", 246, "F22" },
                    { 280, "rack", 247, "F23" },
                    { 281, "rack", 248, "F24" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_list_option_category_value",
                table: "list_option",
                columns: new[] { "category", "value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "full_list");

            migrationBuilder.DropTable(
                name: "list_docket");

            migrationBuilder.DropTable(
                name: "list_option");

            migrationBuilder.DropTable(
                name: "location");

            migrationBuilder.DropTable(
                name: "preparation");
        }
    }
}
