import { createRouter, createWebHistory } from "vue-router";
import FullListView from "@/views/FullListView.vue";

const routes = [
  { path: "/", redirect: "/full-list" },
  { path: "/full-list", name: "full-list", component: FullListView },
];

export default createRouter({
  history: createWebHistory(),
  routes,
});
