<template>
  <!-- Navigation Drawer -->
  <v-navigation-drawer expand-on-hover permanent rail class="nav-bar" @update:rail="isRail = $event">
    <!-- LOGO -->
    <v-list>
      <v-list-item :prepend-avatar="jjlogo" subtitle="MoMS" title="Mould Management System"></v-list-item>
    </v-list>

    <v-divider></v-divider>

    <!-- Navigation Links -->
    <v-list>
      <v-list-item prepend-icon="mdi-monitor-dashboard" title="Dashboard" :to="{ name: 'dashboard' }" link></v-list-item>

      <v-list-item prepend-icon="mdi-home" title="Mould Preparation" :to="{ name: 'mould_prep' }" link></v-list-item>

      <v-list-item prepend-icon="mdi-book-open-page-variant" title="Scan" :to="{ name: 'scan' }" link></v-list-item>

      <v-list-item prepend-icon="mdi-wrench" title="Maintenance" :to="{ name: 'maintenance' }" link></v-list-item>

      <v-list-item prepend-icon="mdi-warehouse" title="Inventory" :to="{ name: 'inventory' }" link></v-list-item>

      <v-list-item prepend-icon="mdi-cog-outline" title="Setting" :to="{ name: 'setting' }" link></v-list-item>
    </v-list>
  </v-navigation-drawer>

  <!-- Main Content -->
  <v-main>
    <router-view></router-view>
  </v-main>
</template>

<script>
import jjlogo from "../assets/JJlogo.png";

export default {
  data() {
    return {
      jjlogo: jjlogo,
    };
  }
};
</script>
