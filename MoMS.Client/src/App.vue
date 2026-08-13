<template>
  <v-app>
    <router-view></router-view>

    <v-dialog v-model="actionDialog" persistent max-width="300">
      <v-card>
        <v-card-title class="text-h5">
          Select Action
        </v-card-title>
        <v-card-text>
          Please choose an action for the scanned item:
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" text @click="handleActionSelection('change_mould')">
            Change Mould
          </v-btn>
          <v-btn color="primary" text @click="handleActionSelection('single_item')">
            Single Item
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script>
import { mapState, mapActions } from "pinia";
import { useMainStore } from "@/store/main";

export default {
  data() {
    return {
      loading: true,
      actionDialog: false,
      scanTimeout: null,
    };
  },
  computed: {
    ...mapState(useMainStore, ["scanDialog"]),
  },
  watch: {
    scanDialog(newValue) {
      if (newValue) {
        this.removeEventListener();
      } else {
        this.addEventListener();
      }
    },
  },
  mounted() {
    this.addEventListener();
  },
  beforeUnmount() {
    this.removeEventListener();
  },
  methods: {
    ...mapActions(useMainStore, ["setScannedValue", "setScanDialog", "handleScanAction"]),

    addEventListener() {
      window.addEventListener("keypress", this.detectScannerInput);
    },
    removeEventListener() {
      window.removeEventListener("keypress", this.detectScannerInput);
    },
    detectScannerInput(event) {
      if (event.key === "Enter") return;

      if (!this.scanBuffer) this.scanBuffer = "";
      this.scanBuffer += event.key;

      clearTimeout(this.scanTimeout);
      this.scanTimeout = setTimeout(() => {
        if (this.scanBuffer.length > 5) {
          this.showActionDialog(this.scanBuffer);
        }
        this.scanBuffer = "";
      }, 50);
    },
    showActionDialog(scannedValue) {
      this.setScannedValue(scannedValue);
      this.actionDialog = true;
    },
    handleActionSelection(action) {
      this.actionDialog = false;

      const route = action === "change_mould" ? "mould_prep" : "scan";
      this.setScanDialog(true);
      this.$router.push({ name: route });
    },
  },
};
</script>

<style>
.table-width table {
  table-layout: fixed;
}
</style>