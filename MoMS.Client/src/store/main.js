import { defineStore } from "pinia";

function calculatePercentage(numerator, denominator) {
    if (
        numerator === undefined ||
        denominator === undefined ||
        denominator === 0 ||
        isNaN(numerator / denominator)
    ) {
        return 0;
    }
    if (Number.isFinite(numerator / denominator)) {
        return (numerator / denominator) * 100;
    }
    return 100;
}

function markDateColumns(item, additionalDateHeaders) {
    const planServDate = new Date(item.PLAN_SERV);
    const repeatInterval = item.REPEAT || 0;
    const firstMonday = new Date();
    firstMonday.setDate(firstMonday.getDate() - firstMonday.getDay() + 1);

    const oneYearFromNow = new Date();
    oneYearFromNow.setFullYear(oneYearFromNow.getFullYear() + 1);

    const serviceDate = new Date(planServDate);

    while (serviceDate <= oneYearFromNow) {
        additionalDateHeaders.forEach((header, index) => {
            const weekStart = new Date(firstMonday);
            weekStart.setDate(firstMonday.getDate() + index * 7);
            const weekEnd = new Date(weekStart);
            weekEnd.setDate(weekStart.getDate() + 6);

            if (serviceDate >= weekStart && serviceDate <= weekEnd) {
                item[header.value] = true;
            }
        });

        if (repeatInterval > 0) {
            serviceDate.setDate(serviceDate.getDate() + repeatInterval);
        } else {
            break;
        }
    }
}

export const useMainStore = defineStore("main", {
    state: () => ({
        details: [],
        scanvalue: null,
        scanDialog: false,
        production: [],
        list_production: [],
        vendor: [],
        list_vendor: [],
        mould_maker: [],
        list_mould_maker: [],
        purpose: [],
        prepared: [],
        rack: [],
        headers_timeline: [],
        headers_progress: [],
        additionalDateHeaders: [],
    }),

    actions: {
        setScannedValue(value) {
            this.scanvalue = value;
        },
        setScanDialog(value) {
            this.scanDialog = value;
        },

        async fetchAllData() {
            try {
                const response = await fetch("/api/full-list");
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data = await response.json();

                const processedData = data.map((item) => {
                    const plannedServiceDate = item.PLAN_SERV ? new Date(item.PLAN_SERV) : null;
                    const lastServiceDate = item.LAST_SERV ? new Date(item.LAST_SERV) : null;

                    const servPer =
                        lastServiceDate && plannedServiceDate
                            ? calculatePercentage(Date.now() - lastServiceDate, plannedServiceDate - lastServiceDate)
                            : 0;
                    const usagePer = calculatePercentage(item.USAGE, item.PLAN_USAGE);
                    const totalPer = Math.max(usagePer, servPer);

                    return {
                        ITEM: item.ITEM,
                        TYPE: item.TYPE,
                        RACK: item.RACK,
                        LEVEL: item.LEVEL,
                        NO: item.NO || 1,
                        S_NUM: item.S_NUM,
                        LOCATION: item.LOCATION,
                        STATUS: item.STATUS,
                        REMARK: item.REMARK,
                        ACCUM_USAGE: item.ACCUM_USAGE || 0,
                        USAGE: item.USAGE || 0,
                        PLAN_USAGE: item.PLAN_USAGE || 0,
                        LAST_SERV: lastServiceDate ? lastServiceDate.toISOString().slice(0, 10) : null,
                        PLAN_SERV: plannedServiceDate ? plannedServiceDate.toISOString().slice(0, 10) : null,
                        REPEAT: item.REPEAT || 0,
                        USAGE_PER: usagePer.toFixed(2),
                        SERV_PER: servPer.toFixed(2),
                        TOTAL_PER: totalPer.toFixed(2),
                    };
                });

                this.details = processedData;
                this.list_production = processedData.filter((i) => this.production.includes(i.LOCATION));
                this.list_vendor = processedData.filter((i) => this.vendor.includes(i.LOCATION));
                this.list_mould_maker = processedData.filter((i) => this.mould_maker.includes(i.LOCATION));

                this.initializeTimeline();
            } catch (error) {
                console.error("Error fetching full-list data:", error);
            }
        },

        // Loads all list-option categories in one request from the list_option
        // table (replaces the six /list/*.txt fetches).
        async fetchListOptions() {
            const response = await fetch("/api/list-options");
            if (!response.ok) {
                throw new Error(`Failed to load list options (${response.status})`);
            }
            const data = await response.json();
            this.production = data.production ?? [];
            this.vendor = data.vendor ?? [];
            this.mould_maker = data.mould_maker ?? [];
            this.prepared = data.prepared ?? [];
            this.purpose = data.purpose ?? [];
            this.rack = data.rack ?? [];
        },

        initializeHeaders() {
            const headers_timeline = [
                { title: "ITEMS", value: "ITEM", width: "250px", fixed: true },
                { title: "SERIAL NUM", value: "S_NUM", width: "100px" },
                { title: "LAST SERVICE", value: "LAST_SERV", width: "150px" },
                { title: "NEXT SERVICE", value: "PLAN_SERV", width: "150px" },
            ];
            const headers_progress = [
                { title: "ITEMS", value: "ITEM", width: "20%" },
                { title: "SERIAL NUM", value: "S_NUM", width: "10%" },
                { title: "ACC USAGE", value: "ACCUM_USAGE", width: "7.5%" },
                { title: "USAGE", value: "USAGE", width: "7.5%" },
                { title: "MAX USAGE", value: "PLAN_USAGE", width: "15%" },
                { title: "LAST SERVICE", value: "LAST_SERV", width: "15%" },
                { title: "PLAN SERVICE", value: "PLAN_SERV", width: "15%" },
                { title: "PROGRESS", value: "TOTAL_PER", width: "10%" },
            ];

            const today = new Date();
            const firstMonday = new Date(today);
            firstMonday.setDate(today.getDate() - today.getDay() + 1);

            const additionalDateHeaders = [];
            for (let i = 0; i < 30; i++) {
                const date = new Date(firstMonday);
                date.setDate(firstMonday.getDate() + i * 7);
                additionalDateHeaders.push({
                    title: new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short" }).format(date),
                    value: `date_${i}`,
                    width: "50px",
                });
            }

            this.headers_timeline = [...headers_timeline, ...additionalDateHeaders];
            this.headers_progress = headers_progress;
            this.additionalDateHeaders = additionalDateHeaders;
        },

        initializeTimeline() {
            this.details.forEach((item) => markDateColumns(item, this.additionalDateHeaders));
        },

        handleScanAction(scannedValue) {
            return this.details.find((item) => item.S_NUM === scannedValue) || null;
        },
    },
});