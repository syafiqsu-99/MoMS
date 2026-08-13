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
            <v-data-table v-model="repeatSelected" :headers="headers_usage" :items="details" :search="search"
              fixed-header select-strategy="page" item-value="S_NUM" height="400px" show-select
              return-object></v-data-table>

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

<script setup>
import { ref, computed } from "vue";
import { storeToRefs } from "pinia";
import { useListOptionsStore } from "@/store/listOptions";

const listOptions = useListOptionsStore();
const { production, vendor, mould_maker, prepared, purpose, rack, error } =
  storeToRefs(listOptions);

const files = [
  { id: "production", label: "Production" },
  { id: "vendor", label: "Vendor" },
  { id: "mould_maker", label: "Mould Maker" },
  { id: "prepared", label: "Prepared" },
  { id: "purpose", label: "Purpose" },
  { id: "rack", label: "Rack" },
];

const categoryRefs = { production, vendor, mould_maker, prepared, purpose, rack };

const fileTab = ref(0);
const newData = ref("");
const selected = ref(null);
const addDialog = ref(false);
const updateDialog = ref(false);
const deleteDialog = ref(false);

const currentCategory = computed(() => files[fileTab.value]?.id ?? "");
const currentItems = computed(() => categoryRefs[currentCategory.value]?.value ?? []);

function promptAdd() {
  newData.value = "";
  addDialog.value = true;
}

async function submitAdd() {
  try {
    await listOptions.addOption(currentCategory.value, newData.value);
    newData.value = "";
    addDialog.value = false;
  } catch (err) {
    console.error(`Failed to add option to ${currentCategory.value}:`, err);
  }
}

function promptUpdate() {
  newData.value = selected.value ?? "";
  updateDialog.value = true;
}

async function submitUpdate() {
  try {
    await listOptions.updateOption(currentCategory.value, selected.value, newData.value);
    newData.value = "";
    selected.value = null;
    updateDialog.value = false;
  } catch (err) {
    console.error(`Failed to update option in ${currentCategory.value}:`, err);
  }
}

function promptDelete() {
  deleteDialog.value = true;
}

async function submitDelete() {
  try {
    await listOptions.deleteOption(currentCategory.value, selected.value);
    selected.value = null;
    deleteDialog.value = false;
  } catch (err) {
    console.error(`Failed to delete option from ${currentCategory.value}:`, err);
  }
}

listOptions.fetchAll();
</script>
