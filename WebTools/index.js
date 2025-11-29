/**
 * PKHeX WebTools - Main Entry Point
 * Export all utilities for easy importing
 */

// Core Utilities
const PokemonDamageCalculator = require('./utils/pokemonDamageCalculator');
const PokemonBreedingCalculator = require('./utils/pokemonBreedingCalculator');
const PokemonTeamBuilder = require('./utils/pokemonTeamBuilder');
const PokemonLivingDex = require('./utils/pokemonLivingDex');
const PokemonWonderTradeSimulator = require('./utils/pokemonWonderTradeSimulator');

// Advanced Editors
const PokemonSaveReader = require('./utils/pokemonSaveReader');
const PokemonPKExport = require('./utils/pokemonPKExport');
const PokemonMemoryEditor = require('./utils/pokemonMemoryEditor');
const PokemonRibbonEditor = require('./utils/pokemonRibbonEditor');
const PokemonHomeTracker = require('./utils/pokemonHomeTracker');
const PokemonContestStats = require('./utils/pokemonContestStats');

// Quality of Life
const PokemonAutoLegality = require('./utils/pokemonAutoLegality');
const PokemonBulkImport = require('./utils/pokemonBulkImport');
const PokemonCloudSync = require('./utils/pokemonCloudSync');
const PokemonTradeHistory = require('./utils/pokemonTradeHistory');
const PokemonTradeQueue = require('./utils/pokemonTradeQueue');

// API
const PokemonAPI = require('./api/pokemonAPI');

// Databases
const mysteryGifts = require('./data/mystery_gift_expanded.json');
const raidDatabase = require('./data/raid_database.json');
const smogonMovesets = require('./data/smogon_movesets.json');
const locationDatabase = require('./data/location_database.json');

module.exports = {
    // Core
    DamageCalculator: PokemonDamageCalculator,
    BreedingCalculator: PokemonBreedingCalculator,
    TeamBuilder: PokemonTeamBuilder,
    LivingDex: PokemonLivingDex,
    WonderTrade: PokemonWonderTradeSimulator,

    // Advanced
    SaveReader: PokemonSaveReader,
    PKExport: PokemonPKExport,
    MemoryEditor: PokemonMemoryEditor,
    RibbonEditor: PokemonRibbonEditor,
    HomeTracker: PokemonHomeTracker,
    ContestStats: PokemonContestStats,

    // QoL
    AutoLegality: PokemonAutoLegality,
    BulkImport: PokemonBulkImport,
    CloudSync: PokemonCloudSync,
    TradeHistory: PokemonTradeHistory,
    TradeQueue: PokemonTradeQueue,

    // API
    API: PokemonAPI,

    // Data
    data: {
        mysteryGifts,
        raids: raidDatabase,
        smogon: smogonMovesets,
        locations: locationDatabase
    },

    // Quick access helpers
    createDamageCalculator: () => new PokemonDamageCalculator(),
    createBreedingCalculator: () => new PokemonBreedingCalculator(),
    createTeamBuilder: () => new PokemonTeamBuilder(),
    createLivingDex: (userId) => new PokemonLivingDex(userId),
    createAutoLegality: () => new PokemonAutoLegality(),
    startAPI: (port) => {
        const api = new PokemonAPI(port);
        api.start();
        return api;
    }
};
