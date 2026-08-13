import Vuex from "vuex";

export default new Vuex.Store({
  state: {
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
  },
  mutations: {
    SET_SCANNED_VALUE(state, value) {
      state.scanvalue = value;
    },
    SET_DETAILS(state, data) {
      state.details = data;
    },
    SET_SCAN_DIALOG(state, value) {
      state.scanDialog = value;
    },
    SET_PRODUCTION(state, data) {
      state.production = data;
    },
    SET_LIST_PRODUCTION(state, data) {
      state.list_production = data;
    },
    SET_VENDOR(state, data) {
      state.vendor = data;
    },
    SET_LIST_VENDOR(state, data) {
      state.list_vendor = data;
    },
    SET_MOULD_MAKER(state, data) {
      state.mould_maker = data;
    },
    SET_LIST_MOULD_MAKER(state, data) {
      state.list_mould_maker = data;
    },
    SET_PREPARED(state, data) {
      state.prepared = data;
    },
    SET_PURPOSE(state, data) {
      state.purpose = data;
    },
    SET_RACK(state, data) {
      state.rack = data;
    },
    SET_HEADERS_TIMELINE(state, headers) {
      state.headers_timeline = headers;
    },
    SET_HEADERS_PROGRESS(state, headers) {
      state.headers_progress = headers;
    },
    SET_ADDITIONAL_DATE_HEADERS(state, headers) {
      state.additionalDateHeaders = headers;
    },
    MARK_DATE_COLUMNS(state, { item, additionalDateHeaders }) {
      const planServDate = new Date(item.PLAN_SERV);
      const repeatInterval = item.REPEAT || 0;
      const firstMonday = new Date();
      firstMonday.setDate(firstMonday.getDate() - firstMonday.getDay() + 1);

      const oneYearFromNow = new Date();
      oneYearFromNow.setFullYear(oneYearFromNow.getFullYear() + 1);

      let serviceDate = new Date(planServDate);

      while (serviceDate <= oneYearFromNow) {
        additionalDateHeaders.forEach((header, index) => {
          const weekStart = new Date(firstMonday);
          weekStart.setDate(firstMonday.getDate() + index * 7);
          const weekEnd = new Date(weekStart);
          weekEnd.setDate(weekStart.getDate() + 6);

          if (serviceDate >= weekStart && serviceDate <= weekEnd) {
            item[header.value] = true; // Mark this week as having a service
          }
        });

        if (repeatInterval > 0) {
          serviceDate.setDate(serviceDate.getDate() + repeatInterval);
        } else {
          break; // No repeat interval means a single service date
        }
      }
    },
    SET_FILE_CONTENT(state, { fileName, content }) {
      state[fileName] = content;
    },
  },
  actions: {
    ScannedData({ commit }, value) {
      commit('SET_SCANNED_VALUE', value);
    },
    async fetchAllData({ dispatch, commit }) {
      try {
        const response = await fetch('/api/full-list');

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        const processedData = data.map((item) => {
          const plannedServiceDate = item.PLAN_SERV ? new Date(item.PLAN_SERV) : null;
          const lastServiceDate = item.LAST_SERV ? new Date(item.LAST_SERV) : null;

          const servPer = lastServiceDate && plannedServiceDate ? calculatePercentage(Date.now() - lastServiceDate, plannedServiceDate - lastServiceDate) : 0;
          const usagePer = calculatePercentage(item.USAGE, item.PLAN_USAGE);

          const totalPer = Math.max(usagePer, servPer);

          function calculatePercentage(numerator, denominator) {
            if (numerator === undefined || denominator === undefined || denominator === 0 || isNaN(numerator / denominator)) {
              return 0;
            } else if (Number.isFinite(numerator / denominator)) {
              return (numerator / denominator) * 100;
            } else {
              return 100;
            }
          }

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

        const listProduction = processedData.filter(item => this.state.production.includes(item.LOCATION));
        const listVendor = processedData.filter(item => this.state.vendor.includes(item.LOCATION));
        const listMouldMaker = processedData.filter(item => this.state.mould_maker.includes(item.LOCATION));

        commit('SET_DETAILS', processedData);
        commit('SET_LIST_PRODUCTION', listProduction);
        commit('SET_LIST_VENDOR', listVendor);
        commit('SET_LIST_MOULD_MAKER', listMouldMaker);

        dispatch("initializeTimeline");
      } catch (error) {
        console.error('Error fetching full-list data:', error);
      }
    },

    initializeHeaders({ commit }) {
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

      // Generate additional date headers
      const today = new Date();
      const firstMonday = new Date(today);
      firstMonday.setDate(today.getDate() - today.getDay() + 1);

      const additionalDateHeaders = [];
      for (let i = 0; i < 30; i++) {
        const date = new Date(firstMonday);
        date.setDate(firstMonday.getDate() + i * 7);
        additionalDateHeaders.push({
          title: new Intl.DateTimeFormat("en-GB", {
            day: "numeric",
            month: "short",
          }).format(date),
          value: `date_${i}`,
          width: "50px",
        });
      }

      // Commit the headers to state
      commit("SET_HEADERS_TIMELINE", [...headers_timeline, ...additionalDateHeaders]);
      commit("SET_HEADERS_PROGRESS", headers_progress);
      commit("SET_ADDITIONAL_DATE_HEADERS", additionalDateHeaders);
    },

    initializeTimeline({ commit, state }) {
      state.details.forEach((item) => {
        commit("MARK_DATE_COLUMNS", {
          item,
          additionalDateHeaders: state.additionalDateHeaders,
        });
      });
    },

    async fetchProduction({ commit }) {
      const response = await fetch('/list/production.txt');
      const data = await response.text();
      commit('SET_PRODUCTION', data.split('\n').map(item => item.trim()).filter(Boolean));
    },

    async fetchVendor({ commit }) {
      const response = await fetch('/list/vendor.txt');
      const data = await response.text();
      commit('SET_VENDOR', data.split('\n').map(item => item.trim()).filter(Boolean));
    },

    async fetchMouldMaker({ commit }) {
      const response = await fetch('/list/mould_maker.txt');
      const data = await response.text();
      commit('SET_MOULD_MAKER', data.split('\n').map(item => item.trim()).filter(Boolean));
    },

    async fetchPrepared({ commit }) {
      const response = await fetch('/list/prepared.txt');
      const data = await response.text();
      commit('SET_PREPARED', data.split('\n').map(item => item.trim()).filter(Boolean));
    },

    async fetchPurpose({ commit }) {
      const response = await fetch('/list/purpose.txt');
      const data = await response.text();
      commit('SET_PURPOSE', data.split('\n').map(item => item.trim()).filter(Boolean));
    },

    async fetchRack({ commit }) {
      const response = await fetch('/list/rack.txt');
      const data = await response.text();
      commit('SET_RACK', data.split('\n').filter(Boolean));
    },

    async updateFileContent({ commit }, { fileName, content }) {
      commit("SET_FILE_CONTENT", { fileName, content });
    },

    handleScanAction({ state }, scannedValue) {
      const foundData = state.details.find(item => item.S_NUM === scannedValue);
      return foundData || null;
    },

  },
  getters: {
    details: (state) => state.details,
    scanvalue: (state) => state.scanvalue,
    scanDialog: (state) => state.scanDialog,
    production: (state) => state.production,
    list_production: (state) => state.list_production,
    vendor: (state) => state.vendor,
    list_vendor: (state) => state.list_vendor,
    mould_maker: (state) => state.mould_maker,
    list_mould_maker: (state) => state.list_mould_maker,
    prepared: (state) => state.prepared,
    purpose: (state) => state.purpose,
    rack: (state) => state.rack,
    headers_timeline: (state) => state.headers_timeline,
    headers_progress: (state) => state.headers_progress,
    additionalDateHeaders: (state) => state.additionalDateHeaders,
  },
});
