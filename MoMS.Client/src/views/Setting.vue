<template>
  <v-container height="100%" fluid>
    <v-row align="center" justify="center">
      <h1>SETTING</h1>
    </v-row>
    <v-row align="center" justify="center">
      <v-expansion-panels>
        <!-- List options -->
        <v-expansion-panel title="Modify List">
          <v-expansion-panel-text>
            <v-tabs align-tabs="center" v-model="fileTab">
              <v-tab v-for="file in files" :key="file.id">{{ file.label }}</v-tab>
            </v-tabs>

            <v-tabs-window v-model="fileTab">
              <v-tabs-window-item v-for="file in files" :key="file.id">
                <h2>{{ file.label }} List</h2>

                <v-list height="400px">
                  <v-list-item v-for="(item, index) in categoryItems(file.id)" :value="item" :key="index"
                    @click="selected = item" rounded="xl">
                    <v-list-item-title v-text="item"></v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-tabs-window-item>
            </v-tabs-window>

            <v-btn @click="promptAdd()" color="primary">Add New</v-btn>
            <v-btn @click="promptUpdate()" color="secondary" :disabled="!selected">Update</v-btn>
            <v-btn @click="promptDelete()" color="error" :disabled="!selected">Delete</v-btn>
          </v-expansion-panel-text>
        </v-expansion-panel>

        <!-- Full list -->
        <v-expansion-panel title="Modify Full List">
          <v-expansion-panel-text>
            <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
              density="compact" hide-details single-line></v-text-field>
            <v-data-table v-model="fullSelected" :headers="headers_full_list" :items="details" :search="search"
              fixed-header select-strategy="single" item-value="S_NUM" height="400px" show-select
              return-object></v-data-table>
            <v-btn @click="promptAddList()" color="primary">Add New</v-btn>
            <v-btn @click="promptUpdateList(fullSelected)" color="secondary"
              :disabled="!fullSelected.length">Update</v-btn>
            <v-btn @click="promptDeleteList(fullSelected)" color="error" :disabled="!fullSelected.length">Delete</v-btn>
          </v-expansion-panel-text>
        </v-expansion-panel>

        <!-- Usage / service / repeat -->
        <v-expansion-panel title="Modify Usage, Service Date and Repetition">
          <v-expansion-panel-text>
            <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
              density="compact" hide-details single-line clearable></v-text-field>
            <v-data-table v-model="repeatSelected" :headers="headers_usage" :items="details" :search="search"
              fixed-header select-strategy="page" item-value="S_NUM" height="400px" show-select
              return-object></v-data-table>

            <v-btn @click="modifyDialog" :disabled="!repeatSelected.length" color="primary">Modify Selected</v-btn>
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
          <v-autocomplete v-model="fulllist_form.LEVEL" :items="filteredLevels"
            :rules="[v => !!v || 'Level is required']" label="LEVEL" required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.STATUS" :items="['GOOD', 'NOT GOOD']"
            :rules="[v => !!v || 'Status is required']" label="STATUS" required></v-autocomplete>
          <v-text-field v-model="fulllist_form.REMARK" :rules="[v => !!v || 'Remark is required']" label="REMARK"
            required></v-text-field>
          <v-text-field v-model="fulllist_form.PLAN_USAGE" :rules="[v => !!v || 'Planned usage is required']"
            label="PLANNED USAGE" required></v-text-field>
          <v-date-input v-model="fulllist_form.PLAN_SERV" :rules="[v => !!v || 'Planned service is required']"
            label="PLANNED SERVICE" :min="new Date().toISOString().split('T')[0]" required></v-date-input>
          <v-text-field v-model="fulllist_form.REPEAT" :rules="[v => !!v || 'Repetition is required']"
            label="REPETITION" required></v-text-field>
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
          <v-autocomplete v-model="fulllist_form.RACK" :items="racks" :rules="[v => !!v || 'Rack is required']"
            label="RACK" required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.LEVEL" :items="filteredLevels"
            :rules="[v => !!v || 'Level is required']" label="LEVEL" required></v-autocomplete>
          <v-autocomplete v-model="fulllist_form.STATUS" :items="['GOOD', 'NOT GOOD']"
            :rules="[v => !!v || 'Status is required']" label="STATUS" required></v-autocomplete>
          <v-text-field v-model="fulllist_form.REMARK" :rules="[v => !!v || 'Remark is required']" label="REMARK"
            required></v-text-field>
          <v-text-field v-model="fulllist_form.PLAN_USAGE" :rules="[v => !!v || 'Planned usage is required']"
            label="PLANNED USAGE" required></v-text-field>
          <v-date-input v-model="fulllist_form.PLAN_SERV" :rules="[v => !!v || 'Planned service is required']"
            label="PLANNED SERVICE" :min="new Date().toISOString().split('T')[0]" required></v-date-input>
          <v-text-field v-model="fulllist_form.REPEAT" :rules="[v => !!v || 'Repetition is required']"
            label="REPETITION" required></v-text-field>
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

  <!-- Repeat list edit dialog -->
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
          <v-btn text @click="submitForm()" :disabled="!isFormUsageValid">Submit</v-btn>
          <v-btn text @click="resetForm()">Cancel</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>

  <!-- List option add dialog -->
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
        <v-btn @click="saveOption">Save</v-btn>
        <v-btn @click="Add_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- List option update dialog -->
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
        <v-btn @click="updateOption">Update</v-btn>
        <v-btn @click="Update_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- List option delete dialog -->
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
        <v-btn @click="deleteOption">Delete</v-btn>
        <v-btn @click="Delete_List = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import { mapState, mapActions } from "pinia";
import { useMainStore } from "@/store/main";
import { useListOptionsStore } from "@/store/listOptions";

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
        ITEM: '', TYPE: '', RACK: '', LEVEL: '', S_NUM: '',
        STATUS: '', REMARK: '', PLAN_USAGE: '', PLAN_SERV: '', REPEAT: '',
      },
      Modify_Repeat: false,
      Add_List: false,
      Update_List: false,
      Delete_List: false,
      repeatSelected: [],
      ITEM: "",
      S_NUM: "",
      PLAN_USAGE: null,
      PLAN_SERV: null,
      REPEAT: null,
      blankrules: [v => !!v || 'Do not leave blank.'],
    };
  },
  computed: {
    ...mapState(useMainStore, ["details"]),
    ...mapState(useListOptionsStore, ["production", "vendor", "mould_maker", "prepared", "purpose", "rack"]),
    currentCategory() {
      return this.files[this.fileTab]?.id ?? "";
    },
    isFormUsageValid() {
      return this.PLAN_USAGE !== null && this.PLAN_USAGE !== '' && this.PLAN_USAGE >= 0 &&
        this.PLAN_SERV !== null &&
        this.REPEAT !== null && this.REPEAT !== '' && this.REPEAT >= 0;
    },
    types() {
      return [...new Set(this.details.map(i => i.TYPE).filter(t => t !== null && t !== ''))];
    },
    racks() {
      const racks = [...new Set(this.details.map(i => i.RACK).filter(r => r !== null && r !== ''))];
      const letters = racks.filter(r => isNaN(r));
      const numbers = racks.filter(r => !isNaN(r));
      return [...letters, ...numbers];
    },
    levels() {
      const levels = [...new Set(this.details.map(i => i.LEVEL).filter(l => l !== null && l !== ''))];
      const letters = levels.filter(l => isNaN(l));
      const numbers = levels.filter(l => !isNaN(l));
      return [...letters, ...numbers];
    },
    filteredLevels() {
      const rack = this.fulllist_form.RACK;
      if (isNaN(rack)) {
        return this.levels.filter(l => !isNaN(l));
      }
      return this.levels.filter(l => isNaN(l));
    },
  },
  methods: {
    ...mapActions(useMainStore, ["fetchAllData"]),
    ...mapActions(useListOptionsStore, {
      fetchAll: "fetchAll",
      addOption: "addOption",
      storeUpdateOption: "updateOption",
      storeDeleteOption: "deleteOption",
    }),

    categoryItems(id) {
      return this[id] ?? [];
    },

    // List option dialogs (backed by the list_option table)
    promptAdd() {
      this.new_data = "";
      this.Add_List = true;
    },
    async saveOption() {
      try {
        await this.addOption(this.currentCategory, this.new_data);
        this.new_data = "";
        this.Add_List = false;
      } catch (error) {
        console.error(`Failed to add option to ${this.currentCategory}:`, error);
      }
    },
    promptUpdate() {
      this.new_data = this.selected ?? "";
      this.Update_List = true;
    },
    async updateOption() {
      try {
        await this.storeUpdateOption(this.currentCategory, this.selected, this.new_data);
        this.new_data = "";
        this.selected = null;
        this.Update_List = false;
      } catch (error) {
        console.error(`Failed to update option in ${this.currentCategory}:`, error);
      }
    },
    promptDelete() {
      this.Delete_List = true;
    },
    async deleteOption() {
      try {
        await this.storeDeleteOption(this.currentCategory, this.selected);
        this.selected = null;
        this.Delete_List = false;
      } catch (error) {
        console.error(`Failed to delete option from ${this.currentCategory}:`, error);
      }
    },

    // Full list dialogs
    generateSNumber() {
      if (!this.fulllist_form.TYPE) {
        this.fulllist_form.S_NUM = '';
        return;
      }
      const type = this.fulllist_form.TYPE;
      const filtered = this.details.map(i => i.S_NUM).filter(s => s.startsWith(type));
      const maxNumber = filtered.reduce((max, snum) => {
        const num = parseInt(snum.slice(type.length), 10);
        return num > max ? num : max;
      }, 0);
      const newNumber = (maxNumber + 1).toString().padStart(4, '0');
      this.fulllist_form.S_NUM = `${type}${newNumber}`;
    },
    promptAddList() {
      this.fulllist_form = {};
      this.Add_FullList = true;
    },
    promptUpdateList(selected) {
      this.fulllist_form = { ...selected[0] };
      this.Update_FullList = true;
    },
    promptDeleteList(selected) {
      this.fulllist_form = { ...selected[0] };
      this.Delete_FullList = true;
    },
    async submitFormList(action) {
      await this.$refs.fullListForm?.validate();
      try {
        let response;
        if (action === 'add_new') {
          response = await fetch("/api/full-list", {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(this.fulllist_form),
          });
        } else if (action === 'update') {
          response = await fetch(`/api/full-list/${this.fulllist_form.S_NUM}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(this.fulllist_form),
          });
        } else if (action === 'delete') {
          response = await fetch(`/api/full-list/${this.fulllist_form.S_NUM}`, { method: 'DELETE' });
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

    // Usage / service / repeat dialog
    modifyDialog() {
      this.ITEM = this.repeatSelected.map(i => i.ITEM).join("\n");
      this.S_NUM = this.repeatSelected.map(i => i.S_NUM).join(",");
      this.Modify_Repeat = true;
    },
    async submitForm() {
      const data = {
        ITEM: this.ITEM.split("\n"),
        S_NUM: this.S_NUM.split(",").map(n => n.trim()),
        PLAN_USAGE: this.PLAN_USAGE,
        PLAN_SERV: this.PLAN_SERV,
        REPEAT: this.REPEAT,
      };
      try {
        const response = await fetch("/api/update-repeat", {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data),
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
    resetForm() {
      if (this.$refs.formUsage) {
        this.$refs.formUsage.reset();
        this.Modify_Repeat = false;
      }
    },
  },
  mounted() {
    this.fetchAll();
  },
};
</script>