<template>
  <v-container height="100%" fluid>
    <v-row align="center" justify="center">
      <h1>MOULD PREPARATION</h1>
    </v-row>

    <v-row align="center" justify="center">
      <v-card width="100%" border="md">
        <v-autocomplete v-model="selectedProduct" :items="productNames" label="Product Name" variant="solo-filled"
          clearable @update:modelValue="fetchProductDetails"></v-autocomplete>
        <v-card-item class="mx-auto" v-if="selectedProductDetails">
          <div>
            <strong>Drawing Code:</strong> {{ selectedProductDetails.drawing_code }}
          </div>
          <div>
            <strong>Preform Weight:</strong> {{ selectedProductDetails.preform_weight }}
          </div>
          <div>
            <strong>Cavity Block:</strong> {{ selectedProductDetails.cavity_block }}
          </div>
        </v-card-item>

        <!-- SCAN ALL DIALOG -->
        <template v-slot:actions>
          <v-btn width="100%" :disabled="!selectedProduct" stacked @click="localScanDialog = true"
            prepend-icon="mdi-barcode-scan">SCAN ALL</v-btn>

          <v-dialog v-model="localScanDialog" width="90%" @afterLeave="clearFields">
            <v-card border="md">
              <v-card-title align="center" class="text-h3">CHANGE MOULD</v-card-title>

              <v-card-text>
                <v-container fluid>
                  <v-row dense>
                    <v-col cols="10">
                      <!-- Serial Number Input -->
                      <v-text-field v-model="serialNumber" append-icon="mdi-send" label="SERIAL NUMBER"
                        :messages="typeTooltip" @input="updateSerialMessage" @keyup.enter="scanSerialNumber"
                        @click:append="scanSerialNumber" @click="serialNotFound = false" required maxlength="6">
                      </v-text-field>
                    </v-col>

                    <v-col cols="2">
                      <!-- Location Selector -->
                      <v-autocomplete v-model="selectedLocation" :items="production" label="LOCATION"
                        :rules="[v => !!v || 'Location is required']" required>
                      </v-autocomplete>
                    </v-col>

                    <v-col>
                      <v-alert v-if="serialSuccess" type="success" dense>
                        {{ serialSuccessMessage }}
                      </v-alert>
                      <v-alert v-if="serialNotFound" type="error" dense>
                        Serial number not found.
                      </v-alert>
                    </v-col>
                  </v-row>

                  <v-row>
                    <!-- Dynamic Sections -->
                    <v-col v-for="(section, index) in sections" :key="index" cols="4">
                      <v-card border="md">
                        <v-card-title>
                          {{ section.title }}
                          <span class="ml-auto text-caption"
                            :style="{ color: section.table.length === getMaxLimit(section.type) ? 'green' : 'red' }">
                            ({{ section.table.length }} / {{ getMaxLimit(section.type) }})
                          </span>
                        </v-card-title>
                        <v-card-text>
                          <v-list dense style="max-height: 400px; overflow-y: auto;">
                            <v-list-item v-for="(item, idx) in section.table" :key="idx">
                              <v-list-item-title>
                                ITEM: {{ item.item }}
                              </v-list-item-title>
                              <v-list-item-subtitle>
                                S. NUM: {{ item.serialNumber }}
                              </v-list-item-subtitle>
                              <v-list-item-subtitle>
                                REMARK:
                                <v-text-field v-model="item.remark" dense solo-inverted hide-details>
                                </v-text-field>
                              </v-list-item-subtitle>
                              <v-list-item-action>
                                <v-btn icon @click="removeItem(section.type, idx)">
                                  <v-icon>mdi-delete</v-icon>
                                </v-btn>
                              </v-list-item-action>
                            </v-list-item>
                          </v-list>
                        </v-card-text>
                      </v-card>
                    </v-col>
                  </v-row>
                </v-container>
              </v-card-text>

              <v-card-actions>
                <v-btn :disabled="!selectedLocation" @click="submitForm">Submit</v-btn>
                <v-btn @click="clearFields">Cancel</v-btn>
              </v-card-actions>
            </v-card>

            <!-- Confirmation Dialog -->
            <v-dialog v-model="confirmationDialog">
              <v-card>
                <v-card-title>Confirm Submission</v-card-title>
                <v-card-text>
                  Some sections do not have the required number of items. Do you want to proceed?
                </v-card-text>
                <v-card-actions>
                  <v-btn color="green darken-1" text @click="confirmSubmit(true)">Yes</v-btn>
                  <v-btn color="red darken-1" text @click="confirmSubmit(false)">No</v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
          </v-dialog>
        </template>
      </v-card>
    </v-row>

    <!-- ITEM LIST -->
    <v-row align="center" justify="center">
      <v-col cols="12" md="4" v-for="section in sections" :key="section.type">
        <v-card border="md">
          <div class="text-truncate font-weight-bold text-h5 mb-2 d-flex justify-center">
            {{ section.title }}
          </div>

          <v-data-iterator :items="sharingProduct.filter(item => item.TYPE === section.type)" item-value="S_NUM"
            items-per-page="-1" style="height: 400px; overflow-y: auto;">
            <template v-slot:default="{ items }">
              <v-row no-gutters dense>
                <v-col cols="12" v-for="part in items" :key="part.raw.S_NUM">
                  <v-card border="md" :color="part.raw.LOCATION === 'Mould Room' ? '#81C784' : '#E57373'" class="mb-2"
                    elevation="2" @click="toggleExpand(part)">
                    <v-card-title class="d-flex align-center">
                      <h3>{{ part.raw.ITEM }}</h3>
                    </v-card-title>

                    <v-card-text>
                      {{ part.raw.S_NUM }}
                    </v-card-text>

                    <v-divider></v-divider>

                    <v-expand-transition>
                      <div v-if="isExpanded(part)">
                        <v-list :lines="false" density="compact">
                          <v-list-item :title="`RACK: ${part.raw.RACK}`" active></v-list-item>
                          <v-list-item :title="`LEVEL: ${part.raw.LEVEL}`"></v-list-item>
                          <v-list-item :title="`LOCATION: ${part.raw.LOCATION}`"></v-list-item>
                          <v-list-item :title="`STATUS: ${part.raw.STATUS}`"></v-list-item>
                          <v-list-item :title="`REMARK: ${part.raw.REMARK}`"></v-list-item>
                        </v-list>
                      </div>
                    </v-expand-transition>
                  </v-card>
                </v-col>
              </v-row>
            </template>
          </v-data-iterator>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import { mapState, mapActions } from "pinia";
import { useMainStore } from "@/store/main";

export default {
  data() {
    return {
      confirmationDialog: false,
      localScanDialog: false,
      selectedLocation: '',
      location: [],
      tableList: [],
      sections: [
        { type: 'BP', title: 'Back Plate', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'BS', title: 'Base Mould', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'BC', title: 'Blow Core', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'BM', title: 'Blow Mould', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'ER', title: 'Ejector', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'HR', title: 'Hot Runner', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'CT', title: 'Injection Cavity', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'IC', title: 'Injection Core', item: '', serialNumber: '', remark: '', table: [] },
        { type: 'LS', title: 'Lip Cavity', item: '', serialNumber: '', remark: '', table: [] },
      ],
      headers: [
        { title: 'SERIAL NUMBER', key: 'S_NUM' },
        { title: 'ITEM', key: 'ITEM' },
      ],
      serialNotFound: false,
      serialSuccess: false,
      serialSuccessMessage: '',
      typeTooltip: '',
      products: [],
      productNames: [],
      selectedProduct: null,
      selectedProductDetails: null,
      sharingProduct: [],
      scanTimeout: null,
      serialNumber: "",
    };
  },
  computed: {
    ...mapState(useMainStore, ["production", "scanDialog", "details", "scanvalue"]),
    canSubmit() {
      return this.selectedLocation && this.sections.some(section => section.table.length > 0);
    },
  },
  watch: {
    scanvalue: {
      immediate: true,
      handler(newVal) {
        if (newVal) {
          this.serialNumber = newVal;
        }
      },
    },
    scanDialog: {
      immediate: true,
      handler(newVal) {
        if (newVal) {
          this.localScanDialog = newVal;
          this.addEventListener();
        } else {
          this.removeEventListener();
        }
      },
    },
  },
  methods: {
    ...mapActions(useMainStore, ["handleScanAction", "setScanDialog"]),
    addEventListener() {
      window.addEventListener("keypress", this.detectScannerInput);
    },
    removeEventListener() {
      window.removeEventListener("keypress", this.detectScannerInput);
    },
    detectScannerInput(event) {
      if (event.key === "Enter") return;

      if (document.activeElement.tagName === "INPUT") return;

      if (!this.scanBuffer) this.scanBuffer = "";
      this.scanBuffer += event.key;

      clearTimeout(this.scanTimeout);
      this.scanTimeout = setTimeout(() => {
        if (this.scanBuffer.length > 5) {
          this.serialNumber = this.scanBuffer;
          this.scanSerialNumber();
        }
        this.scanBuffer = "";
      }, 50);
    },
    async fetchProducts() {
      try {
        const response = await fetch("/api/preparation");

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.products = await response.json();
        this.productNames = this.products.filter(product => product.type !== null).map((product) => product.type);
      } catch (error) {
        console.error('Error fetching sharing data:', error);
      }
    },
    async fetchProductDetails() {
      this.selectedProductDetails = this.products.find(
        (product) => product.type === this.selectedProduct
      );

      try {
        const params = new URLSearchParams({
          type: this.selectedProduct
        });

        const response = await fetch(`/api/sharing-product?${params}`);

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.sharingProduct = await response.json();
      } catch (error) {
        console.error('Error fetching sharing data:', error);
      }
    },
    isExpanded(part) {
      return part.expanded;
    },
    toggleExpand(part) {
      part.expanded = !part.expanded;
    },
    scanSerialNumber() {
      this.serialNotFound = false;
      this.serialSuccess = false;

      const item = this.details.find(items => items.S_NUM === this.serialNumber);
      if (item) {
        this.serialSuccess = true;
        const prefix = this.serialNumber.slice(0, 2);
        this.updateItemDetails(prefix, item);
      } else {
        this.serialNotFound = true;
      }

      this.serialNumber = '';
    },
    updateItemDetails(prefix, item) {
      const section = this.sections.find(section => section.type === prefix);
      if (section) {
        const existingIndex = section.table.findIndex(
          (entry) => entry.serialNumber === this.serialNumber
        );
        if (existingIndex !== -1) {
          // Overwrite the duplicate entry
          section.table[existingIndex] = {
            serialNumber: this.serialNumber,
            item: item.ITEM,
            remark: item.REMARK,
          };
          this.serialSuccessMessage = `Duplicate serial found. Updated entry for ${this.getPrefixName(prefix)}.`;
        } else {
          // Add new entry if it's not a duplicate
          section.table.push({
            serialNumber: this.serialNumber,
            item: item.ITEM,
            remark: item.REMARK,
          });
          this.serialSuccessMessage = `${this.getPrefixName(prefix)} input successful.`;
        }
      }
    },
    updateSerialMessage() {
      if (this.serialNumber.length >= 2) {
        const prefix = this.serialNumber.slice(0, 2);
        this.typeTooltip = this.getPrefixName(prefix) || 'Unknown type';
      } else {
        this.serialSuccess = false;
        this.typeTooltip = 'Type not identified';
      }
    },
    getMaxLimit(type) {
      const limits = {
        BC: this.selectedProductDetails?.cavity_block || 1, // Blow Core
        BM: this.selectedProductDetails?.cavity_block || 1, // Blow Mould
        BS: this.selectedProductDetails?.cavity_block || 1, // Base Mould
        BP: this.selectedProductDetails ? 1 : 1, // Back Plate
        CT: this.selectedProductDetails ? 1 : 1, // Injection Cavity
        ER: this.selectedProductDetails ? 1 : 1, // Ejector
        HR: this.selectedProductDetails ? 1 : 1, // Hot Runner
        IC: this.selectedProductDetails?.cavity_block || 1, // Injection Core
        LS: this.selectedProductDetails?.cavity_block * 3 || 1, // Lip Cavity
      };

      return limits[type] || 1; // Default limit is 1 if type is not found
    },
    getPrefixName(prefix) {
      const prefixNames = {
        BC: 'Blow Core',
        BM: 'Blow Mould',
        BP: 'Back Plate',
        BS: 'Base Mould',
        CT: 'Injection Cavity',
        ER: 'Ejector',
        HR: 'Hot Runner',
        IC: 'Injection Core',
        LS: 'Lip Cavity',
      };
      return prefixNames[prefix] || '';
    },
    isSerialNumbersFilled() {
      return this.sections.every(section => section.table.length === this.getMaxLimit(section.type));
    },
    submitForm() {
      if (!this.isSerialNumbersFilled()) {
        this.confirmationDialog = true; // Trigger confirmation dialog
      } else {
        this.confirmSubmit(true); // Proceed without confirmation
      }
    },
    syncScanDialog(value) {
      this.localScanDialog = value;
      this.setScanDialog(value);
    },
    async confirmSubmit(proceed) {
      if (proceed) {
        const payload = this.sections.flatMap(section =>
          section.table.map(item => {
            const itemDetail = this.details.find(detail => detail.S_NUM === item.serialNumber);

            return {
              ITEM: itemDetail.ITEM,
              TYPE: itemDetail.TYPE,
              S_NUM: item.serialNumber,
              FROM: itemDetail.LOCATION,
              TO: this.selectedLocation,
              STATUS: itemDetail.STATUS,
              REMARK: item.remark
            };
          })
        );

        try {
          const response = await fetch("/api/list-history", {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload)
          });

          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }

          this.fetchProducts();
          this.clearFields();
        } catch (error) {
          console.error('Error submitting list history:', error);
        }
      } else {
        this.fetchProducts();
        this.clearFields();
      }
      this.confirmationDialog = false;
    },
    clearFields() {
      this.syncScanDialog(false);
      this.serialNumber = '';
      this.selectedLocation = '';
      this.sections.forEach((section) => (section.table = []));
    },
    removeItem(type, idx) {
      const section = this.sections.find(section => section.type === type);
      if (section) {
        section.table.splice(idx, 1); // Remove the item at index
      }
    },
  },
  mounted() {
    this.fetchProducts();
  },
};
</script>

<style></style>