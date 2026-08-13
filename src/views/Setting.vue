<template>
  <v-container height="100%" fluid>
    <v-row align="center" justify="center">
      <h1>SETTING</h1>
    </v-row>
    <v-row align="center" justify="center">
      <v-expansion-panels>
        <!-- Text lists -->
        <v-expansion-panel title="Modify List">
          <v-expansion-panel-text>
            <v-tabs align-tabs="center" v-model="fileTab">
              <v-tab v-for="file in files" :key="file.id">{{ file.label }}</v-tab>
            </v-tabs>

            <v-tabs-window v-model="fileTab">
              <v-tabs-window-item v-for="file in files" :key="file.id">
                <h2>{{ file.label }} List</h2>

                <v-list height="400px">
                  <v-list-item v-for="(item, index) in allFiles[file.id]" :value="item" :key="index"
                    @click="selected = item" rounded="xl">
                    <v-list-item-title v-text="item"></v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-tabs-window-item>
            </v-tabs-window>

            <v-btn @click="PromptAdd()" color="primary">Add New</v-btn>
            <v-btn @click="PromptUpdate(selected)" color="secondary" :disabled="!selected">Update</v-btn>
            <v-btn @click="PromptDelete(selected)" color="error" :disabled="!selected">Delete</v-btn>

          </v-expansion-panel-text>
        </v-expansion-panel>
        <!-- Full Lists -->
        <v-expansion-panel title="Modify Full List">
          <v-expansion-panel-text>
            <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
              density="compact" hide-details single-line></v-text-field>
            <v-data-table v-model="fullSelected" :headers="headers_full_list" :items="details" :search="search"
              fixed-header select-strategy="single" item-value="S_NUM" height="400px" show-select
              return-object></v-data-table>
            <v-btn @click="PromptAddList()" color="primary">Add New</v-btn>
            <v-btn @click="PromptUpdateList(fullSelected)" color="secondary"
              :disabled="!fullSelected.length">Update</v-btn>
            <v-btn @click="PromptDeleteList(fullSelected)" color="error" :disabled="!fullSelected.length">Delete</v-btn>

          </v-expansion-panel-text>
        </v-expansion-panel>
        <!-- Repeat lists -->
        <v-expansion-panel title="Modify Usage, Service Date and Repetition">
          <v-expansion-panel-text>
            <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
              density="compact" hide-details single-line clearable></v-text-field>
            <v-data-table v-model="repeatSelected" :headers="headers_usage" :items="details" :search="search" fixed-header
              select-strategy="page" item-value="S_NUM" height="400px" show-select return-object></v-data-table>

            <v-btn @click="ModifyDialog" :disabled="!repeatSelected.length" color="primary">Modify Selected</v-btn>

          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-row>
  </v-container>

  <!-- Add Full List Dialog -->
  <v-dialog v-model="Add_FullList" max-width="500">
    <v-card>
      <v-card-title>Add New Item</v-card-title>
      <v-card-text>
        <v-form @submit.prevent ref="fullListForm">
          <v-text-field v-model="fulllist_form.ITEM" :rules="[v => !!v || 'Item is required']" label="ITEM"
            required></v-text-field>
          <v-autocomplete v-model="fulllist_form.TYPE" :items="types" :rules="[v => !!v || 'Type is required']"
            label="TYPE" @update:modelValue="generateSNumber" required></v-autocomplete>
          <v-text-field v-model="fulllist_form.S_NUM" label="SERIAL NUMBER" readonly></v-text-field>
          <v-autocomplete v-model="fulllist_form.RACK" :items="racks" :rules="[v => !!v || 'Rack is required']"
            label="RACK" required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.LEVEL" :items="filteredLevels" :rules="[v => !!v || 'Level is required']"
            label="LEVEL" required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.STATUS" :items="['GOOD', 'NOT GOOD']"
            :rules="[v => !!v || 'Status is required']" label="STATUS" required></v-autocomplete>
          <v-text-field v-model="fulllist_form.REMARK" :rules="[v => !!v || 'Remark is required']" label="REMARK"
            required></v-text-field>
          <v-text-field v-model="fulllist_form.PLAN_USAGE" :rules="[v => !!v || 'Planned usage is required']"
            label="PLANNED USAGE" required></v-text-field>
          <v-date-input v-model="fulllist_form.PLAN_SERV" :rules="[v => !!v || 'Planned service is required']"
            label="PLANNED SERVICE" :min="new Date().toISOString().split('T')[0]" required></v-date-input>
          <v-text-field v-model="fulllist_form.REPEAT" :rules="[v => !!v || 'Repetition is required']" label="REPETITION"
            required></v-text-field>
          <v-btn @click="submitFormList('add_new')" type="submit">Save</v-btn>
          <v-btn @click="Add_FullList = false">Cancel</v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>

  <!-- Update Full List Dialog -->
  <v-dialog v-model="Update_FullList" max-width="500">
    <v-card>
      <v-card-title>Update Item</v-card-title>
      <v-card-text>
        <v-form ref="fullListForm">
          <v-text-field v-model="fulllist_form.ITEM" :rules="[v => !!v || 'Item is required']" label="ITEM"
            required></v-text-field>
          <v-autocomplete v-model="fulllist_form.S_NUM" label="SERIAL NUMBER" readonly></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.RACK" :rules="[v => !!v || 'Rack is required']" label="RACK"
            required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.LEVEL" :rules="[v => !!v || 'Level is required']" label="LEVEL"
            required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.STATUS" :rules="[v => !!v || 'Status is required']" label="STATUS"
            required></v-autocomplete>
          <v-text-field v-model="fulllist_form.REMARK" :rules="[v => !!v || 'Remark is required']" label="REMARK"
            required></v-text-field>
          <v-text-field v-model="fulllist_form.PLAN_USAGE" :rules="[v => !!v || 'Planned usage is required']"
            label="PLANNED USAGE" required></v-text-field>
          <v-date-input v-model="fulllist_form.PLAN_SERV" :rules="[v => !!v || 'Planned service is required']"
            label="PLANNED SERVICE" :min="new Date().toISOString().split('T')[0]" required></v-date-input>
          <v-text-field v-model="fulllist_form.REPEAT" :rules="[v => !!v || 'Repetition is required']" label="REPETITION"
            required></v-text-field>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-btn @click="submitFormList('update')">Save</v-btn>
        <v-btn @click="Update_FullList = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Delete Full List Dialog -->
  <v-dialog v-model="Delete_FullList" max-width="500">
    <v-card>
      <v-card-title>Delete Item?</v-card-title>
      <v-card-text>Are you sure you want to delete the selected item?</v-card-text>
      <v-card-actions>
        <v-btn @click="submitFormList('delete')" color="error">Delete</v-btn>
        <v-btn @click="Delete_FullList = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Repeat list edit dialog-->
  <v-dialog v-model="Modify_Repeat" max-width="500">
    <v-card>
      <v-form ref="formUsage">
        <v-card-title>MODIFY</v-card-title>
        <v-card-text>
          <v-textarea v-model="ITEM" label="ITEM" readonly></v-textarea>
          <v-text-field v-model="S_NUM" label="SERIAL NUMBER" readonly></v-text-field>
          <v-text-field v-model="PLAN_USAGE" label="PLANNED USAGE (SHOT)" type="number" required min="0"
            :rules="blankrules"></v-text-field>
          <v-text-field v-model="PLAN_SERV" label="PLANNED SERVICE DATE" type="date" required
            :rules="blankrules"></v-text-field>
          <v-text-field v-model="REPEAT" label="REPETITION (DAYS)" type="number" required min="0"
            :rules="blankrules"></v-text-field>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn text @click="submitForm()" :disabled="!isFormUsageValid">
            Submit
          </v-btn>
          <v-btn text @click="ResetForm()">
            Cancel
          </v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>

  <!-- Add/Edit/Delete Lists dialog -->
  <v-dialog v-model="Add_List" max-width="500">
    <v-card>
      <v-card-title>Add New Item</v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="3">
            <p>New Item:</p>
          </v-col>
          <v-col cols="9">
            <v-text-field density="compact" variant="solo" v-model="new_data"></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions>
        <v-btn @click="saveFile">Save</v-btn>
        <v-btn @click="Add_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-dialog v-model="Update_List" max-width="500">
    <v-card>
      <v-card-title>Update Item</v-card-title>
      <v-card-text>
        <v-row dense>
          <v-col cols="3">
            <p>Selected:</p>
          </v-col>
          <v-col cols="9">
            <v-text-field density="compact" variant="solo" :model-value="selected" readonly></v-text-field>
          </v-col>
          <v-col cols="3">
            <p>New Value:</p>
          </v-col>
          <v-col cols="9">
            <v-text-field density="compact" variant="solo" v-model="new_data"></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions>
        <v-btn @click="updateFile">Update</v-btn>
        <v-btn @click="Update_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-dialog v-model="Delete_List" max-width="500">
    <v-card>
      <v-card-title>Confirm Delete?</v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="3">
            <p>Selected:</p>
          </v-col>
          <v-col cols="9">
            <v-text-field density="compact" variant="solo" :model-value="selected" readonly></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions>
        <v-btn @click="deleteFile">Delete</v-btn>
        <v-btn @click="Delete_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import { mapActions, mapGetters } from "vuex";

export default {
  data() {
    return {
      search: "",
      files: [
        { id: "production", label: "Production" },
        { id: "vendor", label: "Vendor" },
        { id: "mould_maker", label: "Mould Maker" },
        { id: "prepared", label: "Prepared" },
        { id: "purpose", label: "Purpose" },
        { id: "rack", label: "Rack" },
      ],
      headers_full_list: [
        { title: 'ITEM', key: 'ITEM' },
        { title: 'TYPE', key: 'TYPE' },
        { title: 'RACK', key: 'RACK' },
        { title: 'LEVEL', key: 'LEVEL' },
        { title: 'SERIAL NUMBER', key: 'S_NUM' },
        { title: 'LOCATION', key: 'LOCATION' },
        { title: 'STATUS', key: 'STATUS' },
        { title: 'REMARK', key: 'REMARK' },
        { title: 'PLANNED USAGE', key: 'PLAN_USAGE' },
        { title: 'PLANNED SERVICE', key: 'PLAN_SERV' },
      ],
      headers_usage: [
        { title: 'ITEM', key: 'ITEM' },
        { title: 'SERIAL NUMBER', key: 'S_NUM' },
        { title: 'USAGE', key: 'USAGE' },
        { title: 'PLANNED USAGE', key: 'PLAN_USAGE' },
        { title: 'LAST SERVICE', key: 'LAST_SERV' },
        { title: 'PLANNED SERVICE', key: 'PLAN_SERV' },
        { title: 'REPETITION (DAYS)', key: 'REPEAT' },
      ],
      fileTab: 0,
      new_data: "",
      selected: null,
      Add_FullList: false,
      Update_FullList: false,
      Delete_FullList: false,
      fullSelected: [],
      fulllist_form: {
        ITEM: '',
        TYPE: '',
        RACK: '',
        LEVEL: '',
        S_NUM: '',
        STATUS: '',
        REMARK: '',
        PLAN_USAGE: '',
        PLAN_SERV: '',
        REPEAT: '',
      },
      Modify_Repeat: false,
      Add_List: false,
      Update_List: false,
      Delete_List: false,
      repeatSelected: [],
      filename: "",
      allFiles: {},
      ITEM: "",
      S_NUM: "",
      PLAN_USAGE: null,
      PLAN_SERV: null,
      REPEAT: null,
      blankrules: [
        v => !!v || 'Do not leave blank.'
      ],
    };
  },
  computed: {
    ...mapGetters(["details", "prepared", "purpose", "production", "vendor", "mould_maker", "rack"]),
    isFormUsageValid() {
      return this.PLAN_USAGE !== null && this.PLAN_USAGE !== '' && this.PLAN_USAGE >= 0 &&
        this.PLAN_SERV !== null &&
        this.REPEAT !== null && this.REPEAT !== '' && this.REPEAT >= 0;
    },
    types() {
      return [...new Set(this.details
        .map(item => item.TYPE)
        .filter(type => type !== null && type !== ''))];
    },
    racks() {
      const racks = [...new Set(this.details
        .map(item => item.RACK)
        .filter(rack => rack !== null && rack !== ''))];
      // Separate letters and numbers, then combine with letters first
      const letters = racks.filter(rack => isNaN(rack));
      const numbers = racks.filter(rack => !isNaN(rack));
      return [...letters, ...numbers];
    },
    levels() {
      const levels = [...new Set(this.details
        .map(item => item.LEVEL)
        .filter(level => level !== null && level !== ''))];
      // Separate letters and numbers, then combine with letters first
      const letters = levels.filter(level => isNaN(level));
      const numbers = levels.filter(level => !isNaN(level));
      return [...letters, ...numbers];
    },
    filteredLevels() {
      const rack = this.fulllist_form.RACK;
      // If rack contains a letter, only show numeric levels
      if (isNaN(rack)) {
        return this.levels.filter(level => !isNaN(level));
      }
      // If rack contains a number, only show letter levels
      return this.levels.filter(level => isNaN(level));
    }
  },
  methods: {
    ...mapActions([
      "fetchAllData",
      "fetchProduction",
      "fetchVendor",
      "fetchMouldMaker",
      "fetchPrepared",
      "fetchPurpose",
      "fetchRack",
      "saveFile",
    ]),
    async fetchAllFiles() {
      try {
        await Promise.all([
          this.fetchProduction(),
          this.fetchVendor(),
          this.fetchMouldMaker(),
          this.fetchPrepared(),
          this.fetchPurpose(),
          this.fetchRack(),
        ]);

        this.allFiles = {
          production: this.production,
          vendor: this.vendor,
          mould_maker: this.mould_maker,
          prepared: this.prepared,
          purpose: this.purpose,
          rack: this.rack,
        };
      } catch (error) {
        console.error("Failed to fetch files:", error);
      }
    },
    // List Dialog
    PromptAdd() {
      this.Add_List = true;
      this.filename = this.files[this.fileTab]?.id;
    },
    async saveFile() {
      try {
        const updatedContent = [...this[this.filename], this.new_data];
        const response = await fetch("/api/save-file", {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            fileName: this.filename,
            content: updatedContent.join("\n"),
          })
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.new_data = "";
        this.Add_List = false;
        this.fetchAllFiles();
      } catch (error) {
        console.error(`Failed to save ${this.filename}:`, error);
      }
    },
    PromptUpdate() {
      this.Update_List = true;
      this.filename = this.files[this.fileTab]?.id;;
    },
    async updateFile() {
      try {
        const index = this[this.filename].indexOf(this.selected);
        if (index > -1) {
          const updatedContent = [...this[this.filename]];
          updatedContent[index] = this.new_data;

          const response = await fetch("/api/save-file", {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              fileName: this.filename,
              content: updatedContent.join("\n"),
            })
          });

          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }

          this.new_data = "";
          this.selected = null;
          this.Update_List = false;
          this.fetchAllFiles();
        } else {
          console.error("Selected item not found in the list.");
        }
      } catch (error) {
        console.error(`Failed to update ${this.filename}:`, error);
      }
    },
    PromptDelete() {
      this.Delete_List = true;
      this.filename = this.files[this.fileTab]?.id;;
    },
    async deleteFile() {
      try {
        const updatedContent = this[this.filename].filter((item) => item !== this.selected);

        const response = await fetch("/api/save-file", {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            fileName: this.filename,
            content: updatedContent.join("\n"),
          })
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.selected = null;
        this.Delete_List = false;
        this.fetchAllFiles();
      } catch (error) {
        console.error(`Failed to delete item from ${this.filename}:`, error);
      }
    },
    // Full List Dialog
    generateSNumber() {
      if (!this.fulllist_form.TYPE) {
        this.fulllist_form.S_NUM = '';
        return;
      }

      // Filter S_NUMs based on the selected TYPE
      const type = this.fulllist_form.TYPE;
      const filtered = this.details
        .map((item) => item.S_NUM)
        .filter((snum) => snum.startsWith(type));

      // Extract numeric part and find the max value
      const maxNumber = filtered.reduce((max, snum) => {
        const num = parseInt(snum.slice(type.length), 10);
        return num > max ? num : max;
      }, 0);

      // Generate the new S_NUM
      const newNumber = (maxNumber + 1).toString().padStart(4, '0');
      this.fulllist_form.S_NUM = `${type}${newNumber}`;
    },
    PromptAddList() {
      this.fulllist_form = {};
      this.Add_FullList = true;
    },
    PromptUpdateList(selected) {
      this.fulllist_form = { ...selected[0] };
      this.Update_FullList = true;
    },
    PromptDeleteList(selected) {
      this.fulllist_form = { ...selected[0] };
      this.Delete_FullList = true;
    },
    async submitFormList(action) {
      const { valid } = await this.$refs.fullListForm.validate();
      try {
        let response;

        if (action === 'add_new') {
          response = await fetch("/api/full-list", {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(this.fulllist_form)
          });
        } else if (action === 'update') {
          response = await fetch(`/api/full-list/${this.fulllist_form.S_NUM}`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(this.fulllist_form)
          });
        } else if (action === 'delete') {
          response = await fetch(`/api/full-list/${this.fulllist_form.S_NUM}`, {
            method: 'DELETE'
          });
        }

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.fetchAllData();
        this.Add_FullList = this.Update_FullList = this.Delete_FullList = false;
      } catch (error) {
        console.error('Error submitting form:', error);
      }
    },
    // Repeat Dialog
    ModifyDialog() {
      this.ITEM = this.repeatSelected.map(item => item.ITEM).join("\n");
      this.S_NUM = this.repeatSelected.map(item => item.S_NUM).join(",");
      this.Modify_Repeat = true;
    },
    async submitForm() {
      const data = {
        ITEM: this.ITEM.split("\n"),
        S_NUM: this.S_NUM.split(",").map(num => num.trim()),
        PLAN_USAGE: this.PLAN_USAGE,
        PLAN_SERV: this.PLAN_SERV,
        REPEAT: this.REPEAT,
      }
      try {
        const response = await fetch("/api/update-repeat", {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(data)
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        this.Modify_Repeat = false;
        this.fetchAllData();
      } catch (error) {
        console.error("Error uploading data:", error);
      }
    },
    ResetForm() {
      if (this.$refs.formList) {
        this.$refs.formList.reset();
        this.allImages = [];
        this.docketDialog = false;
      } else if (this.$refs.formUsage) {
        this.$refs.formUsage.reset()
        this.Modify_Repeat = false;
      }
    },
  },
  mounted() {
    this.fetchAllFiles();
  }
};
</script>
