<template>
    <v-container height="100%" fluid>
        <v-row align="center" justify="center">
            <h1>TIMELINE</h1>
        </v-row>
        <v-row>
            <v-card border="md" flat width="100%">
                <v-text-field v-model="search" density="compact" label="Search" prepend-inner-icon="mdi-magnify"
                    variant="solo-filled" flat hide-details single-line></v-text-field>
            </v-card>
        </v-row>
        <v-row>
            <v-col cols="9">
                <v-row>
                    <v-card border="md">
                        <!-- Timeline Data Table -->
                        <v-card-title align="center">Timeline</v-card-title>
                        <v-data-table-virtual fixed-header v-model:search="search" :headers="headers_timeline"
                            :items="filteredDetails" v-model:sort-by="sortTimeline" height="400" class="table-width">
                            <template v-slot:item.LAST_SERV="{ item }">
                                <v-chip color="primary">
                                    {{ item.LAST_SERV }}
                                </v-chip>
                            </template>
                            <template v-slot:item.PLAN_SERV="{ item }">
                                <v-chip :color="getdateColor(item.PLAN_SERV)">
                                    {{ item.PLAN_SERV }}
                                </v-chip>
                            </template>
                            <template v-for="header in additionalDateHeaders" v-slot:[`item.${header.value}`]="{ item }">
                                <v-icon v-if="item[header.value]" color="primary">
                                    mdi-check
                                </v-icon>
                            </template>
                        </v-data-table-virtual>
                    </v-card>
                </v-row>
                <v-row>
                    <v-card border="md">
                        <!-- Progress Data Table -->
                        <v-card-title align="center">Progress</v-card-title>
                        <v-data-table-virtual fixed-header v-model:search="search" :headers="headers_progress"
                            :items="filteredDetails" v-model:sort-by="sortProgress" height="400" class="table-width">
                            <template v-slot:item.ACCUM_USAGE="{ item }">
                                {{ item.ACCUM_USAGE }}
                            </template>
                            <template v-slot:item.USAGE="{ item }">
                                <v-chip :color="getusageColor(item.USAGE, item.PLAN_USAGE)">
                                    {{ item.USAGE }}
                                </v-chip>
                            </template>
                            <template v-slot:item.PLAN_USAGE="{ item }">
                                <v-chip color="primary">
                                    {{ item.PLAN_USAGE }}
                                </v-chip>
                            </template>
                            <template v-slot:item.LAST_SERV="{ item }">
                                <v-chip color="primary">
                                    {{ item.LAST_SERV }}
                                </v-chip>
                            </template>
                            <template v-slot:item.PLAN_SERV="{ item }">
                                <v-chip :color="getdateColor(item.PLAN_SERV)">
                                    {{ item.PLAN_SERV }}
                                </v-chip>
                            </template>
                            <template v-slot:item.TOTAL_PER="{ item }">
                                <v-progress-linear :color="item.TOTAL_PER >= 100 ? 'red' : 'green'" height="20"
                                    v-model="item.TOTAL_PER" striped>
                                    <div class="text-white text-center">{{ item.TOTAL_PER }}%</div>
                                </v-progress-linear>
                            </template>
                        </v-data-table-virtual>
                    </v-card>
                </v-row>
            </v-col>
            <v-col cols="3" class="ma-0 pa-0">
                <v-card border="md" height="904px">
                    <!-- Urgent Service -->
                    <v-card-title align="center">Urgent Service</v-card-title>
                    <v-list height="100%">
                        <v-list-item v-for="item in progressMax" :key="item.S_NUM">
                            <v-list-item-title>{{ item.ITEM }}</v-list-item-title>
                            <v-list-item-subtitle>S. Num: {{ item.S_NUM }}</v-list-item-subtitle>
                        </v-list-item>
                    </v-list>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
    data() {
        return {
            search: "",
            sortTimeline: [{ key: 'PLAN_SERV', order: 'desc' }],
            sortProgress: [{ key: 'TOTAL_PER', order: 'desc' }],
        };
    },
    computed: {
        ...mapGetters(["headers_timeline", "headers_progress", "additionalDateHeaders", "details"]),

        // Filtered data for Injection Core and Injection Cavity
        filteredDetails() {
            if (this.search) {
                // If there's a search query, return all matching items
                return this.details.filter(item =>
                    Object.values(item).some(value =>
                        String(value).toLowerCase().includes(this.search.toLowerCase())
                    )
                );
            } else {
                // Default view: Only show TYPE = 'CT' or 'IC'
                return this.details.filter(item => item.TYPE === "CT" || item.TYPE === "IC");
            }
        },

        progressMax() {
            return this.details.filter(item => item.TOTAL_PER >= 100 && (item.TYPE == 'IC' || item.TYPE == 'CT'));
        },
    },
    methods: {
        ...mapActions(["initializeHeaders", "initializeTimeline"]),

        getusageColor(usage, plan_usage) {
            if (usage < plan_usage) return "green";
            else if (usage <= plan_usage - 100) return "orange";
            else return "red";
        },
        getdateColor(planServDate) {
            const today = new Date();
            const planServ = new Date(planServDate);
            const diffTime = planServ - today;
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            if (diffDays < 0) return "red";
            else if (diffDays <= 10) return "orange";
            else return "green";
        },
    },
};
</script>
