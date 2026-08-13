import { createApp } from "vue";
import router from "./router";
import store from "./store";
import "./style.css";

import "vuetify/styles";
import { createVuetify } from "vuetify";
import * as components from "vuetify/components";
import * as directives from "vuetify/directives";
import "@mdi/font/css/materialdesignicons.css";
import { VCalendar } from 'vuetify/labs/VCalendar';
import { VDateInput } from 'vuetify/labs/VDateInput'

import App from "./App.vue";

const momsTheme = {
  dark: false,
  colors: {
    background: '#E0E0E0', // Light Gray
    primary: '#009688', // Teal
    secondary: '#000080', // Navy Blue
    error: '#B00020', // Error
    info: '#2196F3', // Info
    success: '#4CAF50', // Success
    warning: '#FB8C00', // Warning
  },
  variables: {
    'border-color': '#000000',
    'border-opacity': 0.12,
    'high-emphasis-opacity': 0.87,
    'medium-emphasis-opacity': 0.60,
    'disabled-opacity': 0.38,
    'idle-opacity': 0.04,
    'hover-opacity': 0.04,
    'focus-opacity': 0.12,
    'selected-opacity': 0.08,
    'activated-opacity': 0.12,
    'pressed-opacity': 0.12,
    'dragged-opacity': 0.08,
    'theme-kbd': '#212529',
    'theme-on-kbd': '#FFFFFF',
    'theme-code': '#F5F5F5',
    'theme-on-code': '#000000',
  }
}

const vuetify = createVuetify({
  components: {
    ...components,
    VDateInput,
    VCalendar,
  },
  directives,
  icons: {
    defaultSet: "mdi",
  },
  theme: {
    defaultTheme: 'momsTheme',
    themes: {
      momsTheme,
    },
  },
});

createApp(App).use(router).use(store).use(vuetify).mount("#app");
