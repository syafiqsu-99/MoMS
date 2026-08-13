<template>
    <v-container height="100vh" class="pa-0 ma-0" fluid>
        <v-row no-gutters style="height:5vh" align="center" justify="center">
            <h1>DASHBOARD</h1>
        </v-row>

        <v-row no-gutters class="position-relative" style="height: 95vh;">
            <v-card width="15%" variant="text" class="position-absolute top-0 right-0">
                <v-card-title align="center" justify="center"><strong>LEGENDS</strong></v-card-title>
                <v-card-text>
                    <v-row>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="green" class="me-2">mdi-circle</v-icon>
                            <span>Prod. Run</span>
                        </v-col>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="yellow" class="me-2">mdi-circle</v-icon>
                            <span>Prod. Issue</span>
                        </v-col>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="red" class="me-2">mdi-circle</v-icon>
                            <span>Tech. Issue</span>
                        </v-col>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="orange" class="me-2">mdi-circle</v-icon>
                            <span>Maint. Issue</span>
                        </v-col>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="gray" class="me-2">mdi-circle</v-icon>
                            <span>QC Issue</span>
                        </v-col>
                        <v-col cols="12" class="d-flex align-center">
                            <v-icon color="white" class="me-2">mdi-circle</v-icon>
                            <span>Unidentified</span>
                        </v-col>
                    </v-row>
                </v-card-text>
            </v-card>

            <v-col cols="12" style="height: 65vh; max-height: 65vh; overflow: hidden;">
                <svg width="100%" height="100%" viewBox="0 0 1050 650">
                    <defs>
                        <pattern id="grid" width="50" height="50" patternUnits="userSpaceOnUse">
                            <path d="M 50 0 L 0 0 0 50" fill="none" stroke="#f5f5f5" stroke-width="1" />
                        </pattern>
                    </defs>
                    <rect width="100%" height="100%" fill="url(#grid)" />

                    <g v-for="(position, machineKey) in machinePositions" :key="machineKey">
                        <rect :x="position.x" :y="position.y" :width="position.width || 50" :height="position.height || 50"
                            :fill="getMachineColor(machineKey)" stroke="#333" stroke-width="2" rx="4" />
                        <text :x="position.x + (position.width || 50) / 2" :y="position.y + (position.height || 50) / 2"
                            text-anchor="middle" dominant-baseline="middle" font-size="12" font-weight="bold" fill="black">
                            {{ machineKey }}
                        </text>
                    </g>
                </svg>
            </v-col>

            <v-col cols="12" style="height: 30vh; max-height: 30vh;">
                <v-window v-model="running_slideshow" show-arrows="hover" continuous class="h-100 overflow-hidden">
                    <v-window-item v-for="(group, index) in paginatedMachines" :key="index" class="h-100">
                        <v-row class="fill-height ma-0">
                            <v-col v-for="machine in group" :key="machine.id_machine" cols="2"
                                class="pa-1 d-flex flex-column">
                                <v-card rounded="xl" variant="elevated" elevation="8" :color="machine.color"
                                    class="flex-fill d-flex flex-column overflow-hidden" style="max-height: 100%;">
                                    <v-card-title class="text-center pa-1 text-h6 font-weight-bold flex-shrink-0"
                                        style="min-height: 40px;">
                                        {{ machine.machine_name }}
                                    </v-card-title>
                                    <v-card-text class="pa-1 flex-fill d-flex flex-column overflow-hidden"
                                        style="flex: 1 1 0; min-height: 0;">
                                        <div class="marquee-wrapper flex-grow-1 overflow-hidden">
                                            <div class="marquee-content">
                                                <span class="text-caption font-weight-bold">{{ machine.type }}</span>
                                                <span class="text-caption font-weight-bold">{{ machine.type }}</span>
                                            </div>
                                        </div>

                                        <v-divider class="my-1 flex-shrink-0"></v-divider>

                                        <v-row dense class="text-caption flex-shrink-0 ma-0" style="height: 55px;">
                                            <v-col cols="6" class="text-right pa-1">
                                                <div>Output:</div>
                                                <div>Cycle Time:</div>
                                            </v-col>
                                            <v-col cols="6" class="text-left pa-1">
                                                <div>{{ machine.output }} pcs</div>
                                                <div>{{ machine.act_ct.toFixed(2) }} s</div>
                                            </v-col>
                                        </v-row>

                                        <v-divider class="my-1 flex-shrink-0"></v-divider>

                                        <div class="d-flex justify-center align-center flex-shrink-0">
                                            <v-chip :color="machine.color" size="small" variant="elevated"
                                                class="text-caption px-2" style="height: 24px;">
                                                {{ machine.category || 'No Data' }}
                                            </v-chip>
                                        </div>
                                    </v-card-text>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-window-item>
                </v-window>
            </v-col>
        </v-row>
    </v-container>
</template>
  
<script>
import { MACHINEPOSITIONS } from '@/store/constant.js';

export default {
    data() {
        return {
            machines: [],
            machineColor: {},
            machinePositions: MACHINEPOSITIONS,
            running_slideshow: 0,
            card_timer: null,
        }
    },

    mounted() {
        this.fetchMachineMaster();
        this.startDataRefresh();
    },

    beforeDestroy() {
        if (this.card_timer) {
            clearInterval(this.card_timer);
        }
    },

    computed: {
        paginatedMachines() {
            const chunkSize = 6;
            return Array.from(
                { length: Math.ceil(this.machines.length / chunkSize) },
                (_, i) => this.machines.slice(i * chunkSize, (i + 1) * chunkSize)
            );
        }
    },

    methods: {
        getMachineColor(machineKey) {
            return this.machineColor[machineKey] || '#cccccc';
        },

        startDataRefresh() {
            this.card_timer = setInterval(() => {
                this.running_slideshow =
                    (this.running_slideshow + 1) % this.paginatedMachines.length;
                this.fetchMachineMaster();
            }, 10000);
        },

        async fetchMachineMaster() {
            try {
                const response = await fetch('/api/loadMachineMaster');
                const data = await response.json();

                this.machines = data;
                const colorMap = {};
                data.forEach(item => {
                    colorMap[item.machine_name] = item.color;
                });
                this.machineColor = colorMap;

            } catch (error) {
                console.error("Error fetching machine master data:", error);
            }
        },
    },
};
</script>

<style scoped>
.marquee-wrapper {
    overflow: hidden;
    white-space: nowrap;
}

.marquee-content {
    display: inline-block;
    animation: marquee 10s linear infinite;
}

.marquee-content span {
    padding: 0 2rem;
}

@keyframes marquee {
    0% {
        transform: translateX(0);
    }

    100% {
        transform: translateX(-50%);
    }
}
</style>