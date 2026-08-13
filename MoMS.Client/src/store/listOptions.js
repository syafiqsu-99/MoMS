import { defineStore } from "pinia";
import { ref } from "vue";

export const useListOptionsStore = defineStore("listOptions", () => {
    const production = ref([]);
    const vendor = ref([]);
    const mould_maker = ref([]);
    const prepared = ref([]);
    const purpose = ref([]);
    const rack = ref([]);
    const loading = ref(false);
    const error = ref(null);

    async function fetchAll() {
        loading.value = true;
        error.value = null;
        try {
            const response = await fetch("/api/list-options");
            if (!response.ok) throw new Error(`Failed to load list options (${response.status})`);
            const data = await response.json();
            production.value = data.production ?? [];
            vendor.value = data.vendor ?? [];
            mould_maker.value = data.mould_maker ?? [];
            prepared.value = data.prepared ?? [];
            purpose.value = data.purpose ?? [];
            rack.value = data.rack ?? [];
        } catch (err) {
            error.value = err.message;
            console.error("Error fetching list options:", err);
        } finally {
            loading.value = false;
        }
    }

    async function addOption(category, value) {
        const response = await fetch("/api/list-options", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ category, value }),
        });
        if (!response.ok) throw new Error(`Add failed (${response.status})`);
        await fetchAll();
    }

    async function updateOption(category, oldValue, newValue) {
        const response = await fetch("/api/list-options", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ category, oldValue, newValue }),
        });
        if (!response.ok) throw new Error(`Update failed (${response.status})`);
        await fetchAll();
    }

    async function deleteOption(category, value) {
        const response = await fetch("/api/list-options", {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ category, value }),
        });
        if (!response.ok) throw new Error(`Delete failed (${response.status})`);
        await fetchAll();
    }

    return {
        production,
        vendor,
        mould_maker,
        prepared,
        purpose,
        rack,
        loading,
        error,
        fetchAll,
        addOption,
        updateOption,
        deleteOption,
    };
});