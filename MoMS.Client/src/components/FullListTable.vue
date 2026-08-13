<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <span>Full List</span>
      <v-spacer />
      <v-btn color="primary" @click="fullList.fetchFullList()">Refresh</v-btn>
    </v-card-title>

    <v-alert v-if="fullList.error" type="error" density="compact" class="mx-4">
      {{ fullList.error }}
    </v-alert>

    <v-data-table
      :headers="headers"
      :items="fullList.details"
      :loading="fullList.loading"
      item-value="SNum"
      density="comfortable"
    >
      <template #item.LastServ="{ item }">
        {{ formatDate(item.LastServ) }}
      </template>
      <template #item.PlanServ="{ item }">
        {{ formatDate(item.PlanServ) }}
      </template>
    </v-data-table>
  </v-card>
</template>

<script setup>
import { onMounted } from "vue";
import { useFullListStore } from "@/store/fullList";

const fullList = useFullListStore();

const headers = [
  { title: "ITEM", key: "Item", width: "250px" },
  { title: "SERIAL NUM", key: "SNum", width: "120px" },
  { title: "TYPE", key: "Type" },
  { title: "RACK", key: "Rack" },
  { title: "LEVEL", key: "Level" },
  { title: "LOCATION", key: "Location" },
  { title: "STATUS", key: "Status" },
  { title: "USAGE", key: "Usage" },
  { title: "LAST SERVICE", key: "LastServ", width: "150px" },
  { title: "PLAN SERVICE", key: "PlanServ", width: "150px" },
];

function formatDate(value) {
  if (!value) return "";
  return new Date(value).toISOString().slice(0, 10);
}

onMounted(() => {
  fullList.fetchFullList();
  fullList.fetchLocations();
});
</script>
