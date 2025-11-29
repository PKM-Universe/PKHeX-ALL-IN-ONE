/**
 * Pokemon HOME Tracker
 * Track HOME value/tracker ID, transfer history, and compatibility
 */

const fs = require('fs');
const path = require('path');

class PokemonHomeTracker {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json', 'home_tracker_data.json');

        // Games that can connect to HOME
        this.compatibleGames = {
            'lets-go-pikachu': { gen: 7, canSend: true, canReceive: true, restrictions: ['kanto-dex'] },
            'lets-go-eevee': { gen: 7, canSend: true, canReceive: true, restrictions: ['kanto-dex'] },
            'sword': { gen: 8, canSend: true, canReceive: true, restrictions: ['galar-dex', 'dlc'] },
            'shield': { gen: 8, canSend: true, canReceive: true, restrictions: ['galar-dex', 'dlc'] },
            'brilliant-diamond': { gen: 8, canSend: true, canReceive: true, restrictions: ['sinnoh-dex'] },
            'shining-pearl': { gen: 8, canSend: true, canReceive: true, restrictions: ['sinnoh-dex'] },
            'legends-arceus': { gen: 8, canSend: true, canReceive: true, restrictions: ['hisui-dex'] },
            'scarlet': { gen: 9, canSend: true, canReceive: true, restrictions: ['paldea-dex', 'dlc'] },
            'violet': { gen: 9, canSend: true, canReceive: true, restrictions: ['paldea-dex', 'dlc'] },
            'pokemon-go': { gen: 0, canSend: true, canReceive: false, restrictions: ['go-transporter'] },
            'bank': { gen: 7, canSend: true, canReceive: false, restrictions: ['one-way'] }
        };

        // Species not available in HOME
        this.unavailableInHome = [];

        // Species restricted to certain games
        this.gameExclusive = {
            'spinda': ['bank'], // Cannot transfer Spinda to newer games due to pattern data
        };

        // HOME features
        this.homeFeatures = {
            'gts': { description: 'Global Trade Station', requirement: 'basic' },
            'wonder-box': { description: 'Wonder Trade', requirement: 'basic' },
            'room-trade': { description: 'Trade rooms with friends', requirement: 'basic' },
            'friend-trade': { description: 'Direct friend trades', requirement: 'basic' },
            'judge': { description: 'IV Judge function', requirement: 'basic' },
            'national-dex': { description: 'Full National Pokedex', requirement: 'premium' },
            'expanded-boxes': { description: '6000 Pokemon storage', requirement: 'premium' },
            'enhanced-gts': { description: 'More GTS deposits', requirement: 'premium' }
        };
    }

    loadData() {
        try {
            if (fs.existsSync(this.dataPath)) {
                return JSON.parse(fs.readFileSync(this.dataPath, 'utf8'));
            }
        } catch (e) {
            console.error('Failed to load HOME tracker data:', e);
        }
        return { pokemon: {}, transfers: [] };
    }

    saveData(data) {
        try {
            fs.writeFileSync(this.dataPath, JSON.stringify(data, null, 2));
            return true;
        } catch (e) {
            console.error('Failed to save HOME tracker data:', e);
            return false;
        }
    }

    generateTrackerId() {
        // HOME tracker IDs are 64-bit values
        const high = Math.floor(Math.random() * 0xFFFFFFFF);
        const low = Math.floor(Math.random() * 0xFFFFFFFF);
        return `${high.toString(16).padStart(8, '0').toUpperCase()}${low.toString(16).padStart(8, '0').toUpperCase()}`;
    }

    generateHomeValue(pokemon) {
        // HOME value is used to track a Pokemon's transfer history
        return {
            trackerId: this.generateTrackerId(),
            originalGame: pokemon.originGame || 'unknown',
            originalTrainer: pokemon.ot || 'Unknown',
            registeredAt: new Date().toISOString(),
            transferCount: 0,
            lastTransfer: null
        };
    }

    registerPokemon(pokemon) {
        const data = this.loadData();
        const homeId = this.generateTrackerId();

        const homeData = {
            homeId,
            species: pokemon.species,
            speciesId: pokemon.speciesId,
            form: pokemon.form || 0,
            shiny: pokemon.shiny || false,
            originGame: pokemon.originGame || 'unknown',
            originTrainer: {
                name: pokemon.ot || 'Unknown',
                id: pokemon.tid || 0,
                sid: pokemon.sid || 0
            },
            currentGame: pokemon.currentGame || null,
            registeredAt: new Date().toISOString(),
            transferHistory: [],
            pokemonGoOrigin: pokemon.fromGo || false
        };

        data.pokemon[homeId] = homeData;
        this.saveData(data);

        return { success: true, homeId, homeData };
    }

    recordTransfer(homeId, fromGame, toGame) {
        const data = this.loadData();
        const pokemon = data.pokemon[homeId];

        if (!pokemon) {
            return { error: 'Pokemon not found in HOME database' };
        }

        // Check compatibility
        const compatibility = this.checkTransferCompatibility(pokemon.species, fromGame, toGame);
        if (!compatibility.canTransfer) {
            return { error: compatibility.reason };
        }

        const transfer = {
            from: fromGame,
            to: toGame,
            timestamp: new Date().toISOString()
        };

        pokemon.transferHistory.push(transfer);
        pokemon.currentGame = toGame;

        data.transfers.push({
            homeId,
            species: pokemon.species,
            ...transfer
        });

        this.saveData(data);

        return { success: true, transfer };
    }

    checkTransferCompatibility(species, fromGame, toGame) {
        const fromInfo = this.compatibleGames[fromGame];
        const toInfo = this.compatibleGames[toGame];

        if (!fromInfo) {
            return { canTransfer: false, reason: `${fromGame} is not compatible with HOME` };
        }

        if (!toInfo) {
            return { canTransfer: false, reason: `${toGame} is not compatible with HOME` };
        }

        if (!fromInfo.canSend) {
            return { canTransfer: false, reason: `Cannot send Pokemon from ${fromGame}` };
        }

        if (!toInfo.canReceive) {
            return { canTransfer: false, reason: `Cannot receive Pokemon in ${toGame}` };
        }

        // Check species-specific restrictions
        if (this.gameExclusive[species?.toLowerCase()]) {
            const allowed = this.gameExclusive[species.toLowerCase()];
            if (!allowed.includes(toGame)) {
                return { canTransfer: false, reason: `${species} cannot be transferred to ${toGame}` };
            }
        }

        return { canTransfer: true };
    }

    getTransferHistory(homeId) {
        const data = this.loadData();
        const pokemon = data.pokemon[homeId];

        if (!pokemon) {
            return { error: 'Pokemon not found' };
        }

        return {
            species: pokemon.species,
            originGame: pokemon.originGame,
            currentGame: pokemon.currentGame,
            transferCount: pokemon.transferHistory.length,
            history: pokemon.transferHistory
        };
    }

    getAvailableDestinations(species, currentGame) {
        const destinations = [];

        for (const [game, info] of Object.entries(this.compatibleGames)) {
            if (game === currentGame) continue;
            if (!info.canReceive) continue;

            const compatibility = this.checkTransferCompatibility(species, currentGame, game);
            destinations.push({
                game,
                canTransfer: compatibility.canTransfer,
                reason: compatibility.reason || 'Compatible',
                restrictions: info.restrictions
            });
        }

        return destinations;
    }

    // Check if a Pokemon can be transferred from GO
    checkGoTransfer(pokemon) {
        const goRestrictions = {
            // Pokemon that have special restrictions from GO
            'melmetal': { requirement: 'Must be traded in GO first for Gigantamax' },
            'mew': { requirement: 'Cannot be shiny from GO' },
            'celebi': { requirement: 'Cannot be shiny from GO' },
            'jirachi': { requirement: 'Cannot be shiny from GO' }
        };

        const species = pokemon.species?.toLowerCase();
        const restriction = goRestrictions[species];

        return {
            canTransfer: true,
            cpCost: this.calculateTransporterCost(pokemon),
            restrictions: restriction || null,
            goOriginMark: true
        };
    }

    calculateTransporterCost(pokemon) {
        // Simplified GO Transporter energy cost
        let baseCost = 10;

        // Legendary/Mythical cost more
        if (pokemon.isLegendary) baseCost = 1000;
        if (pokemon.isMythical) baseCost = 2000;

        // Shiny costs more
        if (pokemon.shiny) baseCost *= 2;

        // CP affects cost
        const cp = pokemon.cp || 0;
        baseCost += Math.floor(cp / 10);

        return Math.min(baseCost, 10000);
    }

    // Get HOME Pokedex progress
    getPokedexProgress(userId) {
        const data = this.loadData();
        const userPokemon = Object.values(data.pokemon).filter(p => p.originTrainer?.id === userId);

        const seen = new Set();
        const registered = new Set();

        for (const pokemon of userPokemon) {
            seen.add(pokemon.speciesId);
            registered.add(pokemon.speciesId);
        }

        return {
            seen: seen.size,
            registered: registered.size,
            total: 1025,
            percentage: ((registered.size / 1025) * 100).toFixed(2)
        };
    }

    // Generate HOME certificate
    generateCertificate(homeId) {
        const data = this.loadData();
        const pokemon = data.pokemon[homeId];

        if (!pokemon) {
            return { error: 'Pokemon not found' };
        }

        return {
            certificate: {
                homeId,
                species: pokemon.species,
                originalTrainer: pokemon.originTrainer?.name,
                originGame: pokemon.originGame,
                registrationDate: pokemon.registeredAt,
                transferCount: pokemon.transferHistory.length,
                isFromGo: pokemon.pokemonGoOrigin,
                verified: true
            }
        };
    }

    // Get all Pokemon in HOME
    getAllPokemon(filters = {}) {
        const data = this.loadData();
        let pokemon = Object.values(data.pokemon);

        if (filters.species) {
            pokemon = pokemon.filter(p => p.species?.toLowerCase() === filters.species.toLowerCase());
        }

        if (filters.shiny !== undefined) {
            pokemon = pokemon.filter(p => p.shiny === filters.shiny);
        }

        if (filters.originGame) {
            pokemon = pokemon.filter(p => p.originGame === filters.originGame);
        }

        if (filters.currentGame) {
            pokemon = pokemon.filter(p => p.currentGame === filters.currentGame);
        }

        return pokemon;
    }

    // Get statistics
    getStatistics() {
        const data = this.loadData();
        const pokemon = Object.values(data.pokemon);

        const stats = {
            totalPokemon: pokemon.length,
            totalTransfers: data.transfers.length,
            byOriginGame: {},
            byCurrentGame: {},
            shinyCount: pokemon.filter(p => p.shiny).length,
            fromGoCount: pokemon.filter(p => p.pokemonGoOrigin).length
        };

        for (const p of pokemon) {
            stats.byOriginGame[p.originGame] = (stats.byOriginGame[p.originGame] || 0) + 1;
            if (p.currentGame) {
                stats.byCurrentGame[p.currentGame] = (stats.byCurrentGame[p.currentGame] || 0) + 1;
            }
        }

        return stats;
    }
}

module.exports = PokemonHomeTracker;
