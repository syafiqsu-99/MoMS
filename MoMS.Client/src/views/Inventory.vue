<template>
    <v-container height="100%" fluid>
        <v-row align="center" justify="center">
            <h1>MOULD ROOM</h1>
        </v-row>

        <v-row align="center" justify="center">
            <div class="layout">
                <!-- First row -->
                <div class="lift">LIFT</div>
                <div class="square">POLISH</div>
                <div class="spacer"></div>
                <div v-for="label in ['7', '8', '9', '10']" :key="label" class="rack"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>
                <div class="spacer"></div>
                <div class="square">SCREW RACK</div>
                <div v-for="label in ['11', '12', '13', '14', '15', '16', '17']" :key="label" class="rack"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>
                <div class="square">DOOR</div>
                <div class="store">MAINTENANCE STORAGE AREA</div>

                <!-- Second row -->
                <template v-for="n in 16">
                    <div class="spacer"></div>
                </template>

                <!-- Third row -->
                <div class="misc">MISC</div>
                <div class="spacer"></div>
                <div class="trolley">TROLLEY STANDBY AREA</div>
                <div class="spacer"></div>
                <div v-for="label in ['A', 'B', 'C', 'D']" :key="label" class="rect"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>
                <div class="spacer"></div>

                <!-- Fourth row -->
                <template v-for="n in 2">
                    <div class="spacer"></div>
                </template>
                <div v-for="label in ['E', 'F', 'G', 'H']" :key="label" class="rect"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>

                <!-- Fifth row -->
                <template v-for="n in 20">
                    <div class="spacer"></div>
                </template>

                <!-- Sixth row -->
                <div class="square">DOOR</div>
                <div class="square">CABINET</div>
                <div class="spacer"></div>
                <div v-for="label in ['1', '2', '3', '4', '5', '6']" :key="label" class="rack"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>
                <div class="square">PARK RACK</div>
                <div v-for="label in ['18', '19', '20', '21', '22', '23', '24', '25', '26']" :key="label" class="rack"
                    :class="{ selected: selectRack === label }" @click="handleClick(label)">
                    {{ label }}
                </div>
            </div>
        </v-row>

        <v-row align="center" justify="center">
            <v-card border="md" width="100%">
                <v-card-title class="d-flex justify-center text-h4">
                    Rack {{ selectRack }}
                </v-card-title>

                <v-divider></v-divider>

                <v-card-text>
                    <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
                        density="compact" hide-details single-line></v-text-field>

                    <v-data-table-virtual :headers="headers" :items="inventory" height="400px" :search="search"
                        item-value="S_NUM" density="compact" hover fixed-header>
                        <template v-slot:item="{ item }">
                            <tr>
                                <td>
                                    <v-text-field v-model="editedItem.ITEM" variant="outlined" min-width="200px"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                                    <span v-else>{{ item.ITEM }}</span>
                                </td>
                                <td align="center">
                                    <v-text-field v-model="editedItem.TYPE" variant="outlined"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                                    <span v-else>{{ item.TYPE }}</span>
                                </td>
                                <td align="center">
                                    <v-autocomplete v-model="editedItem.RACK_LEVEL" variant="outlined" min-width="70px"
                                        menu-icon="" :items="rack"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-autocomplete>
                                    <span v-else>{{ item.RACK }}{{ item.LEVEL }}</span>
                                </td>
                                <!-- <td align="center">
                            <v-text-field v-model="editedItem.LEVEL" variant="outlined"
                                v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                            <span v-else>{{ item.LEVEL }}</span>
                        </td> -->
                                <td align="center">{{ item.S_NUM }}</td>
                                <td align="center" :bgcolor="getLocationColor(item.LOCATION)">
                                    <v-autocomplete v-model="editedItem.LOCATION" variant="outlined" min-width="150px"
                                        menu-icon="" :items="location"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-autocomplete>
                                    <span v-else>{{ item.LOCATION }}</span>
                                </td>
                                <td align="center" :bgcolor="getStatusColor(item.STATUS)">
                                    <v-select v-model="editedItem.STATUS" variant="outlined" min-width="150px"
                                        menu-icon="" :items="['GOOD', 'NOT GOOD']"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-select>
                                    <span v-else>{{ item.STATUS }}</span>
                                </td>
                                <td><v-text-field v-model="editedItem.REMARK" variant="outlined" min-width="200px"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                                    <span v-else>{{ item.REMARK }}</span>
                                </td>
                                <td align="center"><v-text-field v-model="editedItem.USAGE" variant="outlined"
                                        type="number" min="0" v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                                    <span v-else>{{ item.USAGE }}</span>
                                </td>
                                <td align="center"><v-text-field v-model="editedItem.LAST_SERV" variant="outlined"
                                        type="date" min-width="170px"
                                        v-if="item.S_NUM === editedItem.S_NUM"></v-text-field>
                                    <span v-else>{{ item.LAST_SERV }}</span>
                                </td>
                                <td align="center">
                                    <!-- Actions for edit and save -->
                                    <template v-if="item.S_NUM === editedItem.S_NUM">
                                        <v-icon color="red" class="mr-3" @click="closeEdit">mdi-window-close</v-icon>
                                        <v-icon color="green" @click="saveItem()">mdi-content-save</v-icon>
                                    </template>
                                    <template v-else>
                                        <v-icon color="green" class="mr-3"
                                            @click.stop="editItem(item)">mdi-pencil</v-icon>
                                        <v-icon color="red" class="mr-3"
                                            @click.stop="promptDelete(item)">mdi-delete</v-icon>
                                    </template>
                                </td>
                            </tr>
                        </template>
                    </v-data-table-virtual>
                </v-card-text>
            </v-card>
        </v-row>
    </v-container>

    <!-- Confirm Delete Dialog -->
    <v-dialog v-model="confirmDelete" max-width="600">
        <v-card border="md">
            <v-card-title class="text-h5">Delete</v-card-title>
            <v-card-text>Confirm delete?
                <p><strong>Item</strong>: {{ deleteID.ITEM }}</p>
                <p><strong>Serial Number</strong>: {{ deleteID.S_NUM }}</p>
            </v-card-text>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="red" @click="deleteItem">Delete</v-btn>
                <v-btn text @click="confirmDelete = false">Cancel</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script>
import { mapState, mapActions } from "pinia";
import { useMainStore } from "@/store/main";

export default {
    data() {
        return {
            search: "",
            inventory: [],
            selectRack: "",
            location: [],
            headers: [
                { title: 'ITEM', key: 'ITEM', width: '20%' },
                { title: 'TYPE', key: 'TYPE', width: '5%' },
                { title: 'RACK', key: 'LEVEL', width: '5%' },
                // { title: 'LEVEL', key: 'LEVEL', width: '5%' },
                { title: 'SERIAL NUMBER', key: 'S_NUM', width: '10%' },
                { title: 'LOCATION', key: 'LOCATION', width: '10%' },
                { title: 'STATUS', key: 'STATUS', width: '10%' },
                { title: 'REMARK', key: 'REMARK', width: '15%' },
                { title: 'USAGE', key: 'USAGE', width: '5%' },
                { title: 'LAST SERVICE', key: 'LAST_SERV', width: '10%' },
                { title: 'ACTIONS', key: "ACTIONS", sortable: false, width: '5%' }
            ],
            editedItem: {},
            deleteID: {},
            confirmDelete: false,
        };
    },
    computed: {
        ...mapState(useMainStore, ["details", "rack", "production", "vendor", "mould_maker"]),
    },
    methods: {
        ...mapActions(useMainStore, ["fetchAllData"]),
        handleClick(label) {
            this.selectRack = label;
            this.inventory = this.details.filter((item) => item.RACK === label);
        },
        getLocationColor(location) {
            return location === 'Mould Room' ? 'green' : 'red';
        },
        getStatusColor(status) {
            return status === 'GOOD' ? 'green' : 'red';
        },
        editItem(item) {
            this.editedItem = { ...item, RACK_LEVEL: `${item.RACK}${item.LEVEL}` };
        },
        async saveItem() {
            try {
                this.editedItem.RACK_LEVEL = this.editedItem.RACK_LEVEL;
                const match = this.editedItem.RACK_LEVEL.match(/^(\d+|\D+)(\d+|\D+)$/);
                if (match) {
                    const [, rack, level] = match;
                    this.editedItem.RACK = rack;
                    this.editedItem.LEVEL = level;
                }
                const item = this.editedItem;

                const response = await fetch(`/api/full-list/${item.S_NUM}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(item)
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                await this.fetchAllData();
                this.handleClick(this.selectRack);
                this.editedItem = {};
            } catch (error) {
                console.error("Error updating appraisal", error);
            }
        },
        closeEdit() {
            this.editedItem = {};
        },
        promptDelete(item) {
            this.deleteID = item;
            this.confirmDelete = true;
        },
        async deleteItem() {
            try {
                const response = await fetch(`/api/full-list/${this.deleteID.S_NUM}`, {
                    method: 'DELETE'
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                this.confirmDelete = false;
                await this.fetchAllData();
            } catch (error) {
                console.error("Error deleting data:", error);
                alert("Failed to delete the data.");
            }
        },
    },
    mounted() {
        this.location = [
            'Mould Room',
            ...this.production,
            ...this.vendor,
            ...this.mould_maker
        ]
    },
};
</script>

<style>
.layout {
    display: grid;
    grid-template-columns: repeat(21, 55px);
    grid-template-rows: repeat(6, 55px);
}

.lift {
    grid-column: 1 / span 3;
    grid-row: 1 / span 4;
    background-color: RGBA(107, 144, 179, 1);
    display: flex;
    justify-content: center;
    align-items: center;
    border: 1px solid #000;
}

.rack,
.square,
.rect,
.misc,
.trolley,
.store {
    background-color: RGBA(107, 144, 179, 1);
    display: flex;
    justify-content: center;
    align-items: center;
    border: 1px solid #000;
}

.selected {
    background-color: gray !important;
}

.square {
    width: 55px;
    height: 55px;
}

.rack {
    width: 55px;
    height: 55px;
    cursor: pointer;
}

.rack:hover,
.rect:hover {
    background-color: white;
}

.rect {
    grid-column: span 2;
    cursor: pointer;
}

.misc {
    grid-column: 4 / span 1;
    grid-row: 3 / span 2;
}

.trolley {
    grid-column: 6 / span 4;
    grid-row: 3 / span 2;
}

.store {
    grid-column: 20 / span 2;
    grid-row: 1 / span 6;
}

.spacer {
    width: 55PX;
    height: 55px;
}
</style>