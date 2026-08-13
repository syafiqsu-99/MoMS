<template>
    <v-sheet class="background_img" v-if="loading">
        <v-container>
            <v-card variant="text" class="ma-auto" align="center" justify="center">
                <v-card-title>
                    <v-img :width="400" src="/JJfullblue.png"></v-img>
                </v-card-title>
                <v-card-text>
                    <p class="title-text">MoMS</p>
                </v-card-text>
            </v-card>
        </v-container>
    </v-sheet>
</template>

<script>
import { mapActions } from "pinia";
import { useMainStore } from "@/store/main";

export default {
    data() {
        return {
            loading: true,
        };
    },
    methods: {
        ...mapActions(useMainStore, [
            "fetchListOptions",
            "fetchAllData",
            "initializeHeaders",
            "initializeTimeline",
        ]),
        async initializeData() {
            try {
                // List options must load before full-list data, since
                // fetchAllData buckets rows by production/vendor/mould_maker.
                await this.fetchListOptions();
                await Promise.all([
                    this.fetchAllData(),
                    this.initializeHeaders(),
                ]);
                this.initializeTimeline();
            } catch (error) {
                console.error("Error initializing data:", error);
            } finally {
                this.loading = false;
                this.$router.push("/dashboard");
            }
        },
    },
    created() {
        this.initializeData();
    },
};
</script>

<style scoped>
.background_img {
    background: linear-gradient(rgba(0, 128, 128, 0.7),
            rgba(0, 128, 128, 0.7)), url("/SB_Plant.PNG");
    background-size: cover;
    background-position: center;
    height: 100vh;
    width: 100%;
    position: fixed;
    overflow: hidden;
}

.title-text {
    font-size: 10rem;
    color: #ffffff;
    font-family: "Brush Script MT", cursive;
}
</style>