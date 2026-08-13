import { createRouter, createWebHistory } from "vue-router";
import NavBar from "../components/NavBar.vue";

const routes = [
  { path: "/", name: "loading", component: () => import("../views/Loading.vue") },
  {
    path: "/",
    component: NavBar,
    children: [
      { path: "dashboard", name: "dashboard", component: () => import("../views/Dashboard.vue") },
      {
        path: "mould_prep",
        name: "mould_prep",
        component: () => import("../views/MouldPrep.vue"),
      },
      {
        path: "scan",
        name: "scan",
        component: () => import("../views/Scan.vue"),
      },
      {
        path: "maintenance",
        name: "maintenance",
        component: () => import("../views/Maintenance.vue"),
      },
      {
        path: "inventory",
        name: "inventory",
        component: () => import("../views/Inventory.vue"),
      },
      {
        path: "setting",
        name: "setting",
        component: () => import("../views/Setting.vue"),
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  if (to.name !== "loading" && from.name == null) {
    next({ name: "loading" });
  } else {
    next();
  }
});

export default router;
