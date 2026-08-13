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
import { mapActions } from "vuex";

export default {
    data() {
        return {
            loading: true,
        };
    },
    methods: {
        ...mapActions([
            "fetchAllData",
            "fetchProduction",
            "fetchVendor",
            "fetchMouldMaker",
            "fetchPrepared",
            "fetchPurpose",
            "fetchRack",
            "initializeHeaders",
            "initializeTimeline",
            "initializeSceneManager",
        ]),
        async initializeData() {
            try {

                await Promise.all([
                    this.fetchAllData(),
                    this.fetchProduction(),
                    this.fetchVendor(),
                    this.fetchMouldMaker(),
                    this.fetchPrepared(),
                    this.fetchPurpose(),
                    this.fetchRack(),
                    this.initializeHeaders(),
                    this.initializeTimeline(),
                    this.initializeSceneManager()
                ]);
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