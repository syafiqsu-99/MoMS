import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import dotenv from "dotenv";
import path from 'path';

dotenv.config();

export default defineConfig({
  plugins: [vue()],
  base: "./",
  server: {
    host: process.env.VITE_SERVER_IP,
    port: process.env.DEV_PORT,
    proxy: {
      "/api": process.env.VITE_API_URL,
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  define: {
    'process.env.NODE_ENV': '"production"',
  },
  assetsInclude: ['**/*.PNG', '**/*.jpg', '**/*.jpeg', '**/*.gif', '**/*.svg'],
  build: {
    assetsDir: 'assets',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes("node_modules")) {
            if (id.includes("vue")) return "vue";
            if (id.includes("vuetify")) return "vuetify";
            return "vendor";
          }
        },
        // Add asset handling
        assetFileNames: (assetInfo) => {
          const info = assetInfo.name.split('.');
          const ext = info[info.length - 1];
          if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(ext)) {
            return `assets/images/[name]-[hash][extname]`;
          }
          return `assets/[name]-[hash][extname]`;
        },
      },
    },
  },
})
