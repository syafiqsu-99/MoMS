<template>
  <v-container height="100%" fluid>
    <v-row align="center" justify="center">
      <h1>MOULD MOVEMENT</h1>
    </v-row>
    <v-row align="center" justify="center">
      <v-btn elevation="10" stacked width="100%" prepend-icon="mdi-barcode-scan" text="SCAN"
        @click="localScanDialog = true"></v-btn>
    </v-row>

    <v-row>
      <v-col cols="9">
        <!-- Production Table -->
        <v-card border="md">
          <v-card-title color="#26c6da" align="center">Production</v-card-title>
          <v-data-table-virtual :items="sortedProduction" :headers="headers_production" :group-by="groupBy"
            item-value="S_NUM" height="400px" fixed-header class="table-width">

            <!-- Custom Group Header -->
            <template v-slot:group-header="{ item, columns, toggleGroup, isGroupOpen }">
              <tr>
                <td :colspan="columns.length">
                  <v-btn :icon="isGroupOpen(item) ? 'mdi-chevron-down' : 'mdi-chevron-right'" size="small" variant="text"
                    @click="toggleGroup(item)"></v-btn>
                  {{ item.value }}
                </td>
              </tr>
            </template>

          </v-data-table-virtual>
        </v-card>
      </v-col>
      <v-col cols="3">
        <!-- Docket Table -->
        <v-card border="md">
          <v-card-title align="center">Saved Dockets</v-card-title>
          <v-list height="400px">
            <v-list-item v-for="docket in dockets" :key="docket.ID">
              <v-list-item-title>{{ docket.PDF_NAME }}</v-list-item-title>
              <v-list-item-subtitle>Vendor: {{ docket.VENDOR }}</v-list-item-subtitle>
              <template v-slot:append>
                <v-btn icon @click="downloadDocket(docket.PDF_NAME)">
                  <v-icon>mdi-download</v-icon>
                </v-btn>
                <v-btn icon color="red" @click="deleteDocket(docket.PDF_NAME)">
                  <v-icon>mdi-delete</v-icon>
                </v-btn></template>
            </v-list-item>
          </v-list>
        </v-card>
      </v-col>
    </v-row>

    <v-row align="center" justify="center">
      <h1>OUSIDE ITEMS</h1>
    </v-row>

    <v-row>
      <v-col cols="6">
        <v-card border="md">
          <v-card-title align="center">Vendor</v-card-title>
          <v-data-table-virtual :items="list_vendor" :headers="headers_short" height="400px" fixed-header
            class="table-width" show-select select-strategy="single" return-object item-value="S_NUM"
            v-model="selectedVendor" dense @update:modelValue="onVendorSelect" :item-selectable="isSelectable">
          </v-data-table-virtual>
        </v-card>
      </v-col>
      <v-col cols="6">
        <v-card border="md">
          <v-card-title align="center">Mould Maker</v-card-title>
          <v-data-table-virtual :items="list_mould_maker" :headers="headers_short" height="400px" fixed-header
            class="table-width" show-select select-strategy="single" return-object item-value="S_NUM"
            v-model="selectedMouldMaker" dense @update:modelValue="onMouldMakerSelect" :item-selectable="isSelectable">
          </v-data-table-virtual>
        </v-card>
      </v-col>
    </v-row>

    <!-- Toolset's Docket Button -->
    <v-row>
      <v-btn elevation="10" stacked block class="text-center mt-4 font-weight-regular" prepend-icon="mdi-account"
        :disabled="!selectedItem.length" color="primary" text="Toolset's Docket" variant="tonal"
        @click="openDocketDialog"></v-btn>
    </v-row>

    <v-row>
      <!-- History table -->
      <v-card border="md">
        <v-card-title align="center">Data Log</v-card-title>
        <v-data-table-virtual height="400px" fixed-header :items="history" :headers="headers" dense class="table-width">
          <template v-slot:item.IMG_NAME="{ item }">
            <!-- Display existing images -->
            <div align="center" v-if="item.IMG_NAME">
              <v-img v-for="(img, index) in ImgName(item.IMG_NAME)" :key="index" :src="`${imgpath}${img}`"
                height="64"></v-img>
            </div>
          </template>
        </v-data-table-virtual>
      </v-card>
    </v-row>
  </v-container>

  <!-- Barcode scan dialog -->
  <v-dialog v-model="localScanDialog" max-width="600" @afterLeave="syncScanDialog(false)">
    <v-card border="md" prepend-icon="mdi-barcode-scan" title="Mould Movement">
      <v-card-text>
        <v-form @submit.prevent="saveData" ref="formScan">
          <v-text-field v-model="form_Scan.S_NUM" label="Serial Number" maxlength="6"
            :rules="[v => !!v || 'Item is required', v => this.details.some((item) => item.S_NUM === v) || 'Invalid Serial Number']"
            required></v-text-field>
          <v-text-field v-model="form_Scan.FROM" label="From" readonly></v-text-field>
          <v-autocomplete v-model="form_Scan.TO" :items="location" label="To"
            :rules="[v => !!v || 'Location is required']"
            :hint="form_Scan.TO === 'Mould Room' ? `RACK: ${form_Scan.LOCATION}` : ''" persistent-hint required
            @update:modelValue="onLocationChange"></v-autocomplete>
          <v-text-field v-model="form_Scan.ITEM" label="Name" readonly></v-text-field>
          <v-select v-model="form_Scan.STATUS" :items="select_status" label="Status"></v-select>
          <v-text-field v-model="form_Scan.REMARK" label="Remark"></v-text-field>

          <div class="pa-4 d-flex align-center">
            <v-btn text="Close" variant="plain" @click="syncScanDialog(false)" class="me-4"></v-btn>
            <v-btn text="Save" variant="tonal" color="primary" type="submit" class="me-4"></v-btn>
            <v-btn v-if="showProceedToDocket" text="Save & Proceed to Docket" variant="tonal" color="success"
              @click="openDocketDialog"></v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>

  <!-- Docket Form dialog -->
  <v-dialog v-model="docketDialog" max-width="600">
    <v-card border="md">
      <v-form @submit.prevent="submitDocket" ref="formDocket">
        <v-img src="JJfullblue.png" height="100px"></v-img>
        <v-card-title>Toolset's Docket</v-card-title>
        <v-card-subtitle>To record parts, moulds & toolset that send out from JJPM-SB</v-card-subtitle>
        <v-card-text>
          <v-row dense>
            <v-col sm="6">
              <v-select v-model="form_Docket.vendorName" :items="vendorComp" label="1. Vendor Company Name"
                :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.picName" label="2. Vendor PIC Name" :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.dateOut" label="3. Date OUT" type="date" :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.timeOut" label="4. Time OUT" type="time" :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.dateIn" label="5. Target Date IN" type="date" :rules="blankrules"
                required :min="form_Docket.dateOut" />
            </v-col>
            <v-col sm="6">
              <v-select v-model="form_Docket.selectPurpose" :items="purpose" label="6. Purpose for Toolset Send Out"
                :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.modelDetails" label="7. Details (Model)" :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.partsDetails" label="8. Details (Parts)" :rules="blankrules" required />
            </v-col>
            <v-col sm="6">
              <v-text-field v-model="form_Docket.remarksDetails" label="9. Details (Remarks)" />
            </v-col>
            <v-col sm="6">
              <v-select v-model="form_Docket.selectPrepared" :items="prepared" label="10. Docket Prepared By"
                :rules="blankrules" required />
            </v-col>
            <v-col sm="12">
              <v-file-input v-model="form_Docket.images" accept="image/*" multiple show-size prepend-icon="mdi-camera"
                :clearable=false label="11. Photo & Evidence (Including Car Plate & Toolset)" @change="onFileChange"
                :rules="blankrules" />
              <v-row>
                <v-col v-for="(image, index) in tempImages" :key="index" sm="6" md="4">
                  <v-img :src="image" class="mb-2" />
                </v-col>
              </v-row>
            </v-col>
          </v-row>
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn text="Close" variant="plain" @click="docketDialog = false" />
          <v-btn color="primary" text="Save" variant="tonal" type="submit">Save</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>

<script>
import { mapActions, mapGetters } from "vuex";

export default {
  data() {
    return {
      history: [],
      imgpath: '/static/toolset_img/', // Image directory path
      docketDir: "/static/docket_pdf/", // Docket directory path
      docketDialog: false,
      localScanDialog: false,
      showProceedToDocket: false,
      location: [],
      vendorComp: [],
      select_status: ['GOOD', 'NOT GOOD'],
      selected: [],
      selectedVendor: [],
      selectedMouldMaker: [],
      images: [], // For new uploads
      allImages: [], // For already uploaded images
      tempImages: [], // Stores temporary preview URLs
      selectedFiles: [], // Store files for submission
      form_Docket: {
        ITEM: "",
        S_NUM: "",
        VENDOR: "",
        DATETIME: "",
        vendorName: "",
        picName: "",
        dateOut: "",
        timeOut: "",
        dateIn: "",
        selectPurpose: "",
        modelDetails: "",
        partsDetails: "",
        remarksDetails: "",
        selectPrepared: "",
        images: [],
      },
      form_Scan: {
        S_NUM: "",
        FROM: "",
        TO: "",
        ITEM: "",
        STATUS: "",
        REMARK: "",
        LOCATION: "",
        RESET: "",
      },
      headers: [
        { title: 'Item', key: 'ITEM', width: '20%' },
        { title: 'Serial Number', key: 'S_NUM', width: '15%' },
        { title: 'From', key: 'FROM', width: '10%' },
        { title: 'To', key: 'TO', width: '10%' },
        { title: 'Date/Time', key: 'DATETIME', width: '15%' },
        { title: 'Status', key: 'STATUS', width: '10%' },
        { title: 'Remark', key: 'REMARK', width: '15%' },
        { title: 'Image', key: 'IMG_NAME', width: '10%' },
      ],
      groupBy: [{ key: "LOCATION" }],
      headers_short: [
        { title: 'Item', key: 'ITEM', width: '35%' },
        { title: 'Serial Number', key: 'S_NUM', width: '15%' },
        { title: 'Remark', key: 'REMARK', width: '20%' },
        { title: 'Location', key: 'LOCATION', width: '15%' },
        { title: 'Service Date', key: 'LAST_SERV', width: '15%' },
      ],
      headers_production: [
        { title: 'Item', key: 'ITEM', width: '40%' },
        { title: 'Serial Number', key: 'S_NUM', width: '20%' },
        { title: 'Remark', key: 'REMARK', width: '20%' },
        { title: 'Shot', key: 'USAGE', width: '20%' },
      ],
      blankrules: [
        v => !!v || 'Do not leave blank.'
      ],
      rules: [
        value => {
          return !value || !value.length || value[0].size < 2000000 || 'Image size should be less than 2 MB!'
        },
      ],
      dockets: [], // List of saved dockets
    };
  },

  computed: {
    ...mapGetters(["details", "scanDialog", "scanvalue", "prepared", "purpose", "production", "list_production", "vendor", "list_vendor", "mould_maker", "list_mould_maker"]),
    // Unifies selected item
    selectedItem() {
      return this.selectedVendor || this.selectedMouldMaker;
    },
    sortedProduction() {
      const locationOrder = [
        "A5", "A6", "A7", "A8", "A9", "A10", "A12", "A13", "A14",
        "A15", "A16", "A17", "A18", "A19", "A21", "A23"
      ];

      return [...this.list_production].sort((a, b) => {
        return locationOrder.indexOf(a.LOCATION) - locationOrder.indexOf(b.LOCATION);
      });
    },
    savedDocketSNums() {
      return new Set(this.dockets.map(docket => docket.S_NUM));
    }
  },

  methods: {
    ...mapActions(["fetchAllData", "handleScanAction", "fetchListVendor", "fetchListMouldMaker"]),

    async fetchHistory() {
      try {
        const response = await fetch("/api/list-history");

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.history = await response.json();
      } catch (error) {
        console.error('Error fetching history data:', error);
      }
    },

    ImgName(imgNames) {
      return imgNames.split(",").map(img => img.trim());
    },

    onFileChange(event) {
      this.tempImages = [];  // Clear preview images
      this.allImages = [];   // Clear stored image names
      this.selectedFiles = []; // Clear stored files

      const files = event.target.files || event;
      if (!files) return;

      const timestamp = new Date().getTime();
      const updatedImageNames = [];
      const tempImageUrls = [];
      const selectedFiles = [];

      for (let file of files) {
        const uniqueName = `${timestamp}_${file.name}`;
        updatedImageNames.push(uniqueName);
        tempImageUrls.push(URL.createObjectURL(file));
        selectedFiles.push({ file, uniqueName });
      }

      // Store only filenames in allImages
      this.allImages = [...this.allImages, ...updatedImageNames];
      // Store temporary previews
      this.tempImages = [...this.tempImages, ...tempImageUrls];
      // Store actual files for upload
      this.selectedFiles = [...this.selectedFiles, ...selectedFiles];
    },

    onVendorSelect(selection) {
      if (selection) {
        this.selectedMouldMaker = null; // Clear Mould Maker selection
      }
    },

    onMouldMakerSelect(selection) {
      if (selection) {
        this.selectedVendor = null; // Clear Vendor selection
      }
    },

    isSelectable(item) {
      return !this.savedDocketSNums.has(item.S_NUM);
    },

    syncScanDialog(value) {
      this.localScanDialog = value;
      this.$store.commit("SET_SCAN_DIALOG", value);
    },

    async saveData() {
      const { valid } = await this.$refs.formScan.validate()

      if (valid) {
        this.form_Scan.RESET = (this.form_Scan.TO === 'Mould Room' && (this.vendor.includes(this.form_Scan.FROM) || this.mould_maker.includes(this.form_Scan.FROM)));

        try {
          const response = await fetch('/api/list-history', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(this.form_Scan)
          });

          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }

          this.fetchHistory();
          this.fetchAllData();
        } catch (error) {
          console.error('Error submitting data:', error);
        }
        this.syncScanDialog(false);
        return true;
      } else {
        return false;
      }
    },

    async openDocketDialog() {
      // Case 1: When an item is selected from the data table
      if (this.selectedItem && this.selectedItem.length > 0) {
        const historyItem = this.history.find(hist => hist.S_NUM === this.selectedItem[0]?.S_NUM);

        if (historyItem) {
          this.selected = [{ ...this.selectedItem[0], IMG_NAME: historyItem.IMG_NAME }];
        } else {
          this.selected = [{ ...this.selectedItem[0] }];
        }

        const currentItem = this.selected[0];
        const now = new Date();
        this.form_Docket.vendorName = currentItem.LOCATION || "";
        this.form_Docket.modelDetails = currentItem.ITEM || "";
        this.form_Docket.partsDetails = currentItem.S_NUM || "";
        this.form_Docket.remarksDetails = currentItem.REMARK || "";
        this.form_Docket.dateOut = now.toISOString().split("T")[0]; // Format: YYYY-MM-DD
        this.form_Docket.timeOut = now.toTimeString().split(" ")[0]; // Format: HH:MM:SS
        this.form_Docket.DATETIME = now.toISOString();
        this.form_Docket.ITEM = currentItem.ITEM || "";
        this.form_Docket.S_NUM = currentItem.S_NUM || "";
        this.form_Docket.VENDOR = currentItem.LOCATION || "";

        this.docketDialog = true;
        return;
      }

      // Case 2: Validate `formScan` before opening the docket dialog
      const isScanValid = await this.saveData();
      if (isScanValid) {
        const now = new Date();
        this.form_Docket.vendorName = this.form_Scan.TO || "";
        this.form_Docket.modelDetails = this.form_Scan.ITEM || "";
        this.form_Docket.partsDetails = this.form_Scan.S_NUM || "";
        this.form_Docket.remarksDetails = this.form_Scan.REMARK || "";
        this.form_Docket.dateOut = now.toISOString().split("T")[0]; // Format: YYYY-MM-DD
        this.form_Docket.timeOut = now.toTimeString().split(" ")[0]; // Format: HH:MM:SS
        this.form_Docket.DATETIME = now.toISOString();
        this.form_Docket.ITEM = this.form_Scan.ITEM || "";
        this.form_Docket.S_NUM = this.form_Scan.S_NUM || "";
        this.form_Docket.VENDOR = this.form_Scan.TO || "";

        this.docketDialog = true;
      }
    },

    async submitDocket() {
      if (this.selectedFiles.length > 0) {
        const formData = new FormData();
        this.selectedFiles.forEach(({ file, uniqueName }) => {
          formData.append("images[]", file, uniqueName);
        });

        try {
          const response = await fetch("/api/upload-images-server", {
            method: 'POST',
            body: formData
          });

          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
        } catch (error) {
          console.error("Error uploading images to server:", error);
          return;
        }
      }

      const { valid } = await this.$refs.formDocket.validate();
      if (!valid) {
        return;
      }

      this.form_Docket.images = this.allImages;

      try {
        const response = await fetch('/api/dockets', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(this.form_Docket)
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.form_Docket = {};
        this.selectedFiles = [];
        this.tempImages = [];
        this.allImages = [];
        this.selected = [];
        this.docketDialog = false;
        this.fetchDockets();
      } catch (error) {
        console.error("Error saving docket.", error);
      }
    },

    onLocationChange(updatedValue) {
      this.showProceedToDocket = this.vendor.includes(updatedValue) || this.mould_maker.includes(updatedValue);
    },

    // Fetch Dockets
    async fetchDockets() {
      try {
        const response = await fetch("/api/dockets");

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.dockets = await response.json();
      } catch (error) {
        console.error("Error fetching dockets:", error);
      }
    },

    // Delete Docket
    async deleteDocket(id) {
      try {
        const response = await fetch(`/api/dockets/${id}`, {
          method: 'DELETE'
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.fetchDockets();
      } catch (error) {
        console.error("Failed to overwrite the existing docket.", error);
      }
    },

    // Download Docket
    downloadDocket(pdfName) {
      const link = document.createElement('a');
      link.href = `${this.docketDir}${pdfName}`;
      link.download = pdfName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    },
  },
  watch: {
    "form_Scan.S_NUM": {
      immediate: true,
      handler(newVal) {
        if (newVal) {
          // Find the matching item in `details`
          const matchingDetail = this.details.find(item => item.S_NUM === newVal);

          if (matchingDetail) {
            // Set the other fields based on the matching detail
            this.form_Scan.FROM = matchingDetail.LOCATION || "";
            this.form_Scan.ITEM = matchingDetail.ITEM || "";
            this.form_Scan.STATUS = matchingDetail.STATUS || "";
            this.form_Scan.REMARK = matchingDetail.REMARK || "";
            this.form_Scan.LOCATION = matchingDetail.RACK + matchingDetail.LEVEL || "";
          } else {
            // Clear fields if no match is found
            this.form_Scan.FROM = "";
            this.form_Scan.ITEM = "";
            this.form_Scan.STATUS = "";
            this.form_Scan.REMARK = "";
            this.form_Scan.LOCATION = "";
          }
        }
      },
    },
    scanDialog: {
      immediate: true,
      handler(newVal) {
        this.localScanDialog = newVal;
      },
    },
    scanvalue: {
      immediate: true, // Trigger immediately when the component is created
      handler(newVal) {
        if (newVal) {
          this.form_Scan.S_NUM = newVal; // Set scanvalue to form_Scan.S_NUM
        }
      },
    },
  },

  mounted() {
    this.fetchHistory();
    this.fetchDockets();
    this.vendorComp = [
      ...this.vendor,
      ...this.mould_maker
    ];
    this.location = [
      ...this.vendor.filter(item => item === 'SERVICE'),
      'MOULD ROOM',
      ...this.production,
      ...this.vendor.filter(item => item !== 'SERVICE'),
      ...this.mould_maker
    ];
  },
};
</script>
