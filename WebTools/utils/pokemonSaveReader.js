/**
 * Pokemon Save File Reader
 * Parse .sav files to extract Pokemon data (Gen 3-9)
 */

class PokemonSaveReader {
    constructor() {
        this.saveFormats = {
            gen3: { size: [0x20000], pokemonSize: 80, games: ['ruby', 'sapphire', 'emerald', 'firered', 'leafgreen'] },
            gen4: { size: [0x80000], pokemonSize: 136, games: ['diamond', 'pearl', 'platinum', 'heartgold', 'soulsilver'] },
            gen5: { size: [0x80000], pokemonSize: 136, games: ['black', 'white', 'black2', 'white2'] },
            gen6: { size: [0x65600, 0x76000], pokemonSize: 232, games: ['x', 'y', 'omegaruby', 'alphasapphire'] },
            gen7: { size: [0x6BE00, 0x6CC00], pokemonSize: 232, games: ['sun', 'moon', 'ultrasun', 'ultramoon'] },
            gen8: { size: [0x1716B3], pokemonSize: 344, games: ['sword', 'shield'] },
            gen9: { size: [0x2E0000], pokemonSize: 376, games: ['scarlet', 'violet'] }
        };
        this.natures = ['Hardy', 'Lonely', 'Brave', 'Adamant', 'Naughty', 'Bold', 'Docile', 'Relaxed', 'Impish', 'Lax', 'Timid', 'Hasty', 'Serious', 'Jolly', 'Naive', 'Modest', 'Mild', 'Quiet', 'Bashful', 'Rash', 'Calm', 'Gentle', 'Sassy', 'Careful', 'Quirky'];
        this.speciesMap = { 1: 'Bulbasaur', 2: 'Ivysaur', 3: 'Venusaur', 4: 'Charmander', 5: 'Charmeleon', 6: 'Charizard', 7: 'Squirtle', 8: 'Wartortle', 9: 'Blastoise', 25: 'Pikachu', 26: 'Raichu', 150: 'Mewtwo', 151: 'Mew' };
    }

    detectFormat(buffer) {
        const size = buffer.length;
        for (const [gen, format] of Object.entries(this.saveFormats)) {
            if (format.size.includes(size)) return { generation: gen, format };
        }
        return { generation: 'unknown', format: null };
    }

    async readSaveFile(filePath) {
        const fs = require('fs').promises;
        try {
            const buffer = await fs.readFile(filePath);
            const detection = this.detectFormat(buffer);
            if (detection.generation === 'unknown') return { error: 'Unknown save format' };

            return {
                filePath, fileSize: buffer.length,
                generation: detection.generation,
                games: detection.format.games,
                party: this.parseParty(buffer, detection),
                trainer: this.parseTrainer(buffer, detection)
            };
        } catch (e) { return { error: e.message }; }
    }

    parseParty(buffer, detection) {
        // Simplified party parsing
        const party = [];
        const gen = detection.generation;

        if (gen === 'gen3') {
            const partyOffset = 0x0234;
            for (let i = 0; i < 6; i++) {
                const offset = partyOffset + (i * 100);
                if (offset + 80 <= buffer.length) {
                    const pokemon = this.parseGen3Pokemon(buffer, offset);
                    if (pokemon.speciesId > 0) party.push(pokemon);
                }
            }
        }
        return party;
    }

    parseGen3Pokemon(buffer, offset) {
        const pid = buffer.readUInt32LE(offset);
        const otid = buffer.readUInt32LE(offset + 4);

        return {
            pid,
            speciesId: buffer.readUInt16LE(offset + 32) || 0,
            species: this.speciesMap[buffer.readUInt16LE(offset + 32)] || 'Unknown',
            level: this.calculateLevel(buffer.readUInt32LE(offset + 36)),
            nature: this.natures[pid % 25],
            shiny: this.isShiny(pid, otid),
            ivs: this.extractIVs(buffer.readUInt32LE(offset + 72)),
            evs: {
                hp: buffer.readUInt8(offset + 56), atk: buffer.readUInt8(offset + 57),
                def: buffer.readUInt8(offset + 58), spe: buffer.readUInt8(offset + 59),
                spa: buffer.readUInt8(offset + 60), spd: buffer.readUInt8(offset + 61)
            }
        };
    }

    extractIVs(data) {
        return {
            hp: data & 0x1F, atk: (data >> 5) & 0x1F, def: (data >> 10) & 0x1F,
            spe: (data >> 15) & 0x1F, spa: (data >> 20) & 0x1F, spd: (data >> 25) & 0x1F
        };
    }

    isShiny(pid, otid) {
        const tidXor = ((otid >> 16) ^ (otid & 0xFFFF)) ^ ((pid >> 16) ^ (pid & 0xFFFF));
        return tidXor < 8;
    }

    calculateLevel(exp) {
        const expTable = [0, 10, 33, 80, 156, 270, 428, 640, 911, 1250, 1663, 2160, 2746, 3430, 4218, 5120, 6141, 7290, 8573, 10000];
        for (let level = expTable.length - 1; level >= 0; level--) {
            if (exp >= expTable[level]) return level + 1;
        }
        return 1;
    }

    parseTrainer(buffer, detection) {
        if (detection.generation === 'gen3') {
            return {
                trainerId: buffer.readUInt16LE(10),
                secretId: buffer.readUInt16LE(12)
            };
        }
        return null;
    }

    getSummary(saveData) {
        if (saveData.error) return `Error: ${saveData.error}`;
        let summary = `Save File: ${saveData.generation.toUpperCase()}\n`;
        summary += `Size: ${(saveData.fileSize / 1024).toFixed(1)} KB\n`;
        summary += `Games: ${saveData.games.join(', ')}\n\n`;
        summary += `Party (${saveData.party.length}):\n`;
        for (const p of saveData.party) {
            summary += `- ${p.shiny ? '★ ' : ''}${p.species} Lv.${p.level} (${p.nature})\n`;
        }
        return summary;
    }
}

module.exports = PokemonSaveReader;
