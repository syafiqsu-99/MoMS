import { defineStore } from "pinia";
import { ref } from "vue";

// Replaces the Vuex full-list slice. Uses relative /api paths so the Vite
// dev proxy (and IIS in production) route requests to the .NET backend.
export const useFullListStore = defineStore("fullList", () => {
  const details = ref([]);
  const locations = ref([]);
  const loading = ref(false);
  const error = ref(null);

  async function fetchFullList() {
    loading.value = true;
    error.value = null;
    try {
      const response = await fetch("/api/full-list");
      if (!response.ok) throw new Error(`Failed to load full list (${response.status})`);
      details.value = await response.json();
    } catch (err) {
      error.value = err.message;
      console.error("Error fetching full-list data:", err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchLocations() {
    try {
      const response = await fetch("/api/locations");
      if (!response.ok) throw new Error(`Failed to load locations (${response.status})`);
      locations.value = await response.json();
    } catch (err) {
      error.value = err.message;
      console.error("Error fetching locations:", err);
    }
  }

  async function createItem(payload) {
    const response = await fetch("/api/full-list", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) throw new Error(`Create failed (${response.status})`);
    await fetchFullList();
  }

  async function updateItem(sNum, payload) {
    const response = await fetch(`/api/full-list/${encodeURIComponent(sNum)}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) throw new Error(`Update failed (${response.status})`);
    await fetchFullList();
  }

  async function deleteItem(sNum) {
    const response = await fetch(`/api/full-list/${encodeURIComponent(sNum)}`, {
      method: "DELETE",
    });
    if (!response.ok) throw new Error(`Delete failed (${response.status})`);
    await fetchFullList();
  }

  return {
    details,
    locations,
    loading,
    error,
    fetchFullList,
    fetchLocations,
    createItem,
    updateItem,
    deleteItem,
  };
});
