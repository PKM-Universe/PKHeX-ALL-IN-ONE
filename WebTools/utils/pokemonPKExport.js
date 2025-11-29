/**
 * Pokemon PK File Export
 * Export to .pk7/.pk8/.pk9 formats for use with PKHeX
 */

class PokemonPKExport {
    constructor() {
        this.formats = {
            pk7: { size: 232, gen: 7, games: ['Sun', 'Moon', 'Ultra Sun', 'Ultra Moon'] },
            pk8: { size: 344, gen: 8, games: ['Sword', 'Shield', 'Brilliant Diamond', 'Shining Pearl', 'Legends Arceus'] },
            pk9: { size: 376, gen: 9, games: ['Scarlet', 'Violet'] }
        };
        this.natures = ['Hardy', 'Lonely', 'Brave', 'Adamant', 'Naughty', 'Bold', 'Docile', 'Relaxed', 'Impish', 'Lax', 'Timid', 'Hasty', 'Serious', 'Jolly', 'Naive', 'Modest', 'Mild', 'Quiet', 'Bashful', 'Rash', 'Calm', 'Gentle', 'Sassy', 'Careful', 'Quirky'];
    }

    exportToPK(pokemon, format = 'pk9') {
        const formatInfo = this.formats[format];
        if (!formatInfo) return { error: `Unknown format: ${format}` };

        const buffer = Buffer.alloc(formatInfo.size);

        // Common structure (simplified)
        // Encryption constant (random)
        buffer.writeUInt32LE(Math.floor(Math.random() * 0xFFFFFFFF), 0x00);

        // Sanity check
        buffer.writeUInt16LE(0, 0x04);

        // Checksum placeholder
        buffer.writeUInt16LE(0, 0x06);

        // Species
        buffer.writeUInt16LE(pokemon.speciesId || this.getSpeciesId(pokemon.species), 0x08);

        // Held Item
        buffer.writeUInt16LE(pokemon.itemId || 0, 0x0A);

        // TID/SID
        buffer.writeUInt16LE(pokemon.tid || Math.floor(Math.random() * 65535), 0x0C);
        buffer.writeUInt16LE(pokemon.sid || Math.floor(Math.random() * 65535), 0x0E);

        // Experience
        buffer.writeUInt32LE(this.getExpForLevel(pokemon.level || 50), 0x10);

        // Ability
        buffer.writeUInt16LE(pokemon.abilityId || 0, 0x14);

        // Ability Number
        buffer.writeUInt8(pokemon.abilityNumber || 1, 0x16);

        // Nature
        const natureId = this.natures.indexOf(pokemon.nature?.charAt(0).toUpperCase() + pokemon.nature?.slice(1).toLowerCase());
        buffer.writeUInt8(natureId >= 0 ? natureId : 0, 0x20);

        // Gender/Form flags
        let genderFlag = 0;
        if (pokemon.gender === 'female') genderFlag = 2;
        else if (pokemon.gender === 'male') genderFlag = 0;
        buffer.writeUInt8(genderFlag | (pokemon.formId || 0) << 3, 0x22);

        // EVs
        buffer.writeUInt8(pokemon.evs?.hp || 0, 0x26);
        buffer.writeUInt8(pokemon.evs?.atk || 0, 0x27);
        buffer.writeUInt8(pokemon.evs?.def || 0, 0x28);
        buffer.writeUInt8(pokemon.evs?.spe || 0, 0x29);
        buffer.writeUInt8(pokemon.evs?.spa || 0, 0x2A);
        buffer.writeUInt8(pokemon.evs?.spd || 0, 0x2B);

        // Moves (4 slots)
        const moves = pokemon.moves || [];
        for (let i = 0; i < 4; i++) {
            buffer.writeUInt16LE(this.getMoveId(moves[i]) || 0, 0x72 + (i * 2));
        }

        // IVs (packed into 32-bit value)
        const ivs = pokemon.ivs || { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 };
        let ivValue = ivs.hp & 0x1F;
        ivValue |= (ivs.atk & 0x1F) << 5;
        ivValue |= (ivs.def & 0x1F) << 10;
        ivValue |= (ivs.spe & 0x1F) << 15;
        ivValue |= (ivs.spa & 0x1F) << 20;
        ivValue |= (ivs.spd & 0x1F) << 25;
        if (pokemon.isEgg) ivValue |= (1 << 30);
        if (pokemon.shiny) ivValue |= (1 << 31);
        buffer.writeUInt32LE(ivValue, 0x8C);

        // Shiny type calculation
        if (pokemon.shiny) {
            const pid = this.calculateShinyPID(pokemon.tid || 0, pokemon.sid || 0);
            buffer.writeUInt32LE(pid, 0x00);
        }

        // OT Name (up to 12 characters)
        const otName = pokemon.ot || 'PKHex';
        this.writeString(buffer, otName, 0xB0, 26);

        // Calculate checksum
        const checksum = this.calculateChecksum(buffer);
        buffer.writeUInt16LE(checksum, 0x06);

        return {
            buffer,
            format,
            size: formatInfo.size,
            pokemon: pokemon.species || pokemon.nickname,
            filename: `${pokemon.species || 'pokemon'}.${format}`
        };
    }

    calculateChecksum(buffer) {
        let checksum = 0;
        for (let i = 8; i < buffer.length; i += 2) {
            checksum += buffer.readUInt16LE(i);
        }
        return checksum & 0xFFFF;
    }

    calculateShinyPID(tid, sid) {
        const high = Math.floor(Math.random() * 0xFFFF);
        const low = tid ^ sid ^ high;
        return (high << 16) | low;
    }

    getSpeciesId(species) {
        const speciesMap = {
            'bulbasaur': 1, 'ivysaur': 2, 'venusaur': 3, 'charmander': 4, 'charmeleon': 5,
            'charizard': 6, 'squirtle': 7, 'wartortle': 8, 'blastoise': 9, 'pikachu': 25,
            'raichu': 26, 'eevee': 133, 'vaporeon': 134, 'jolteon': 135, 'flareon': 136,
            'mewtwo': 150, 'mew': 151, 'ditto': 132, 'dragonite': 149, 'garchomp': 445,
            'lucario': 448, 'greninja': 658, 'mimikyu': 778, 'dragapult': 887
        };
        return speciesMap[species?.toLowerCase()] || 0;
    }

    getMoveId(move) {
        const moveMap = {
            'tackle': 33, 'scratch': 10, 'pound': 1, 'thunderbolt': 85, 'flamethrower': 53,
            'ice-beam': 58, 'earthquake': 89, 'psychic': 94, 'surf': 57, 'hydro-pump': 56,
            'fire-blast': 126, 'thunder': 87, 'blizzard': 59, 'hyper-beam': 63,
            'dragon-claw': 337, 'close-combat': 370, 'shadow-ball': 247, 'stone-edge': 444,
            'iron-head': 442, 'play-rough': 583, 'moonblast': 585, 'draco-meteor': 434
        };
        return moveMap[move?.toLowerCase()] || 0;
    }

    getExpForLevel(level) {
        // Medium-slow growth rate approximation
        return Math.floor((6 * Math.pow(level, 3) / 5) - (15 * Math.pow(level, 2)) + (100 * level) - 140);
    }

    writeString(buffer, str, offset, maxLength) {
        for (let i = 0; i < Math.min(str.length, maxLength / 2); i++) {
            buffer.writeUInt16LE(str.charCodeAt(i), offset + (i * 2));
        }
        buffer.writeUInt16LE(0, offset + (Math.min(str.length, maxLength / 2) * 2));
    }

    exportToFile(pokemon, format, outputPath) {
        const fs = require('fs');
        const result = this.exportToPK(pokemon, format);
        if (result.error) return result;

        fs.writeFileSync(outputPath || result.filename, result.buffer);
        return { success: true, path: outputPath || result.filename, size: result.size };
    }

    batchExport(pokemonList, format = 'pk9', outputDir = './exports') {
        const fs = require('fs');
        const path = require('path');

        if (!fs.existsSync(outputDir)) fs.mkdirSync(outputDir, { recursive: true });

        const results = [];
        for (let i = 0; i < pokemonList.length; i++) {
            const pokemon = pokemonList[i];
            const filename = `${pokemon.species || 'pokemon'}_${i + 1}.${format}`;
            const filepath = path.join(outputDir, filename);
            results.push(this.exportToFile(pokemon, format, filepath));
        }
        return results;
    }

    importFromPK(buffer, format = 'pk9') {
        const formatInfo = this.formats[format];
        if (!formatInfo || buffer.length !== formatInfo.size) {
            return { error: 'Invalid PK file format or size mismatch' };
        }

        const ivData = buffer.readUInt32LE(0x8C);

        return {
            speciesId: buffer.readUInt16LE(0x08),
            itemId: buffer.readUInt16LE(0x0A),
            tid: buffer.readUInt16LE(0x0C),
            sid: buffer.readUInt16LE(0x0E),
            experience: buffer.readUInt32LE(0x10),
            nature: this.natures[buffer.readUInt8(0x20)] || 'Hardy',
            ivs: {
                hp: ivData & 0x1F,
                atk: (ivData >> 5) & 0x1F,
                def: (ivData >> 10) & 0x1F,
                spe: (ivData >> 15) & 0x1F,
                spa: (ivData >> 20) & 0x1F,
                spd: (ivData >> 25) & 0x1F
            },
            evs: {
                hp: buffer.readUInt8(0x26),
                atk: buffer.readUInt8(0x27),
                def: buffer.readUInt8(0x28),
                spe: buffer.readUInt8(0x29),
                spa: buffer.readUInt8(0x2A),
                spd: buffer.readUInt8(0x2B)
            },
            isEgg: (ivData >> 30) & 1,
            isShiny: (ivData >> 31) & 1
        };
    }
}

module.exports = PokemonPKExport;
