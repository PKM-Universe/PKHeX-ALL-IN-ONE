/**
 * Pokemon Memory Editor
 * OT memories, geo locations, affection, and handler data
 */

class PokemonMemoryEditor {
    constructor() {
        this.memoryTypes = {
            0: 'No memory',
            1: 'Met in a place',
            2: 'Ate together',
            3: 'Traveled together',
            4: 'Caught in a ball',
            5: 'Observed a battle',
            6: 'Battled together',
            7: 'Was praised',
            8: 'Was surprised',
            9: 'Explored together',
            10: 'Played together',
            11: 'Won a contest',
            12: 'Went shopping together',
            14: 'Found an item',
            15: 'Hatched from an egg',
            16: 'Received as a gift',
            17: 'Joined from event',
            18: 'Met through Wonder Trade',
            19: 'Met through GTS',
            20: 'Transferred from another game',
            21: 'Received through Mystery Gift',
            44: 'Made a new friend',
            50: 'Received a present',
            51: 'Was called cute',
            52: 'Took a walk together',
            53: 'Found treasure',
            60: 'Met at a Pokemon Center',
            65: 'Saw an impressive move',
            69: 'Shared a moment together',
            70: 'Was worried',
            79: 'Made eye contact'
        };

        this.feelings = {
            0: 'Happy', 1: 'Joyful', 2: 'Delighted', 3: 'Excited',
            4: 'Amazed', 5: 'Curious', 6: 'Puzzled', 7: 'Embarrassed',
            8: 'Sad', 9: 'Angry', 10: 'Worried', 11: 'Confused',
            12: 'Relaxed', 13: 'Comfortable', 14: 'Sleepy', 15: 'Energetic'
        };

        this.regions = {
            1: 'Kanto', 2: 'Johto', 3: 'Hoenn', 4: 'Sinnoh',
            5: 'Unova', 6: 'Kalos', 7: 'Alola', 8: 'Galar', 9: 'Paldea',
            10: 'Hisui'
        };

        this.locations = {
            kanto: ['Pallet Town', 'Viridian City', 'Pewter City', 'Cerulean City', 'Vermilion City', 'Lavender Town', 'Celadon City', 'Fuchsia City', 'Saffron City', 'Cinnabar Island', 'Indigo Plateau', 'Route 1', 'Viridian Forest', 'Mt. Moon', 'Rock Tunnel', 'Pokemon Tower', 'Safari Zone', 'Victory Road', 'Cerulean Cave'],
            johto: ['New Bark Town', 'Cherrygrove City', 'Violet City', 'Azalea Town', 'Goldenrod City', 'Ecruteak City', 'Olivine City', 'Cianwood City', 'Mahogany Town', 'Blackthorn City', 'Sprout Tower', 'Ruins of Alph', 'National Park', 'Lake of Rage', 'Mt. Silver'],
            hoenn: ['Littleroot Town', 'Oldale Town', 'Petalburg City', 'Rustboro City', 'Dewford Town', 'Slateport City', 'Mauville City', 'Verdanturf Town', 'Fallarbor Town', 'Lavaridge Town', 'Fortree City', 'Lilycove City', 'Mossdeep City', 'Sootopolis City', 'Ever Grande City', 'Meteor Falls', 'Mt. Chimney', 'Sky Pillar'],
            sinnoh: ['Twinleaf Town', 'Sandgem Town', 'Jubilife City', 'Oreburgh City', 'Floaroma Town', 'Eterna City', 'Hearthome City', 'Solaceon Town', 'Veilstone City', 'Pastoria City', 'Celestic Town', 'Canalave City', 'Snowpoint City', 'Sunyshore City', 'Pokemon League', 'Mt. Coronet', 'Spear Pillar', 'Distortion World'],
            galar: ['Postwick', 'Wedgehurst', 'Motostoke', 'Turffield', 'Hulbury', 'Hammerlocke', 'Stow-on-Side', 'Ballonlea', 'Circhester', 'Spikemuth', 'Wyndon', 'Wild Area', 'Isle of Armor', 'Crown Tundra'],
            paldea: ['Cabo Poco', 'Mesagoza', 'Cortondo', 'Artazon', 'Levincia', 'Cascarrafa', 'Porto Marinada', 'Medali', 'Montenevera', 'Alfornada', 'Area Zero', 'Pokemon League']
        };

        this.contestStats = ['Cool', 'Beauty', 'Cute', 'Smart', 'Tough'];
    }

    createMemory(options = {}) {
        return {
            type: options.type || 0,
            typeLabel: this.memoryTypes[options.type] || 'No memory',
            intensity: Math.min(7, Math.max(0, options.intensity || 1)),
            feeling: options.feeling || 0,
            feelingLabel: this.feelings[options.feeling] || 'Happy',
            variable: options.variable || 0,
            textArgs: options.textArgs || []
        };
    }

    createGeoLocation(region, location, options = {}) {
        return {
            region: region,
            regionName: this.regions[region] || `Region ${region}`,
            location: location,
            day: options.day || new Date().getDate(),
            month: options.month || new Date().getMonth() + 1,
            year: options.year || new Date().getFullYear()
        };
    }

    generateOTMemory(pokemon, options = {}) {
        const memories = [];

        // Met memory
        memories.push(this.createMemory({
            type: options.caught ? 4 : (options.hatched ? 15 : (options.traded ? 18 : 16)),
            intensity: 4,
            feeling: 1,
            variable: options.catchBall || 4
        }));

        // Add some random memories
        const randomMemories = [6, 7, 44, 52, 69];
        for (let i = 0; i < Math.min(3, options.memoryCount || 1); i++) {
            const type = randomMemories[Math.floor(Math.random() * randomMemories.length)];
            memories.push(this.createMemory({
                type,
                intensity: Math.floor(Math.random() * 5) + 1,
                feeling: Math.floor(Math.random() * 16)
            }));
        }

        return {
            pokemon: pokemon.species || pokemon.name,
            otName: options.otName || 'Trainer',
            otGender: options.otGender || 'male',
            memories
        };
    }

    generateHandlerMemory(pokemon, options = {}) {
        return {
            pokemon: pokemon.species || pokemon.name,
            handlerName: options.handlerName || 'Handler',
            handlerGender: options.handlerGender || 'male',
            currentHandler: options.currentHandler || 1,
            memory: this.createMemory({
                type: options.memoryType || 44,
                intensity: options.intensity || 3,
                feeling: options.feeling || 0
            }),
            friendship: options.friendship || 70,
            affection: options.affection || 0
        };
    }

    generateGeoHistory(pokemon, options = {}) {
        const history = [];
        const regions = options.regions || [9]; // Default to Paldea

        for (const regionId of regions) {
            const regionName = this.regions[regionId]?.toLowerCase() || 'paldea';
            const locationList = this.locations[regionName] || this.locations.paldea;
            const location = locationList[Math.floor(Math.random() * locationList.length)];

            history.push(this.createGeoLocation(regionId, location, {
                year: options.year || 2024,
                month: Math.floor(Math.random() * 12) + 1,
                day: Math.floor(Math.random() * 28) + 1
            }));
        }

        return {
            pokemon: pokemon.species || pokemon.name,
            originRegion: regions[0],
            geoHistory: history
        };
    }

    setAffection(pokemon, level) {
        // Affection levels: 0-5 (Gen 6-7) or 0-255 (internal)
        const affectionLevels = {
            0: { hearts: 0, label: 'None', effects: 'No effects' },
            1: { hearts: 1, label: 'Low', effects: 'None' },
            2: { hearts: 2, label: 'Medium', effects: '+10% crit chance' },
            3: { hearts: 3, label: 'High', effects: '+10% crit, sometimes survives KO' },
            4: { hearts: 4, label: 'Very High', effects: '+10% crit, survives KO, evades moves' },
            5: { hearts: 5, label: 'Max', effects: 'All bonuses, 1.2x EXP' }
        };

        const clampedLevel = Math.min(5, Math.max(0, level));

        return {
            pokemon: pokemon.species || pokemon.name,
            affectionLevel: clampedLevel,
            ...affectionLevels[clampedLevel],
            internalValue: Math.floor(clampedLevel * 51) // 0-255 scale
        };
    }

    setFriendship(pokemon, value, options = {}) {
        const clampedValue = Math.min(255, Math.max(0, value));

        const friendshipTiers = {
            low: { min: 0, max: 49, label: 'Unfriendly' },
            base: { min: 50, max: 99, label: 'Neutral' },
            medium: { min: 100, max: 149, label: 'Friendly' },
            high: { min: 150, max: 199, label: 'Close' },
            max: { min: 200, max: 255, label: 'Best Friends' }
        };

        let tier = 'low';
        for (const [key, data] of Object.entries(friendshipTiers)) {
            if (clampedValue >= data.min && clampedValue <= data.max) {
                tier = key;
                break;
            }
        }

        return {
            pokemon: pokemon.species || pokemon.name,
            friendship: clampedValue,
            tier,
            tierLabel: friendshipTiers[tier].label,
            canEvolve: clampedValue >= 220, // Most friendship evolutions require 220+
            returnPower: Math.floor(clampedValue / 2.5), // Return move power
            frustrationPower: Math.floor((255 - clampedValue) / 2.5) // Frustration move power
        };
    }

    generateFullMemoryData(pokemon, options = {}) {
        return {
            pokemon: pokemon.species || pokemon.name,
            otMemory: this.generateOTMemory(pokemon, {
                otName: options.otName,
                caught: options.caught,
                hatched: options.hatched,
                traded: options.traded,
                memoryCount: options.memoryCount || 3
            }),
            handlerMemory: this.generateHandlerMemory(pokemon, {
                handlerName: options.handlerName,
                friendship: options.friendship || 70
            }),
            geoHistory: this.generateGeoHistory(pokemon, {
                regions: options.regions
            }),
            affection: this.setAffection(pokemon, options.affectionLevel || 0),
            friendship: this.setFriendship(pokemon, options.friendship || 70)
        };
    }

    getMemoryText(memory) {
        const baseText = this.memoryTypes[memory.type] || 'had an experience';
        const intensity = ['vaguely', 'slightly', '', 'fondly', 'clearly', 'deeply', 'strongly', 'intensely'][memory.intensity] || '';
        const feeling = this.feelings[memory.feeling] || 'happy';

        return `${intensity} remembers: "${baseText}". ${this.capitalize(pokemon?.species || 'It')} felt ${feeling.toLowerCase()}.`;
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    exportMemoryData(memoryData) {
        return JSON.stringify(memoryData, null, 2);
    }
}

module.exports = PokemonMemoryEditor;
