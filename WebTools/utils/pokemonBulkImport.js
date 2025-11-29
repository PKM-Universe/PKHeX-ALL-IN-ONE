/**
 * Pokemon Bulk Import Utility
 * Import multiple Pokemon from various formats
 */

const fs = require('fs');
const path = require('path');

class PokemonBulkImport {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json');
        this.importHistoryFile = path.join(this.dataPath, 'import_history.json');

        this.supportedFormats = {
            'showdown': { extension: '.txt', parser: 'parseShowdown' },
            'pk7': { extension: '.pk7', parser: 'parsePKFile' },
            'pk8': { extension: '.pk8', parser: 'parsePKFile' },
            'pk9': { extension: '.pk9', parser: 'parsePKFile' },
            'json': { extension: '.json', parser: 'parseJSON' },
            'csv': { extension: '.csv', parser: 'parseCSV' },
            'paste': { extension: null, parser: 'parseShowdown' }
        };
    }

    // Import from file
    async importFromFile(filePath) {
        if (!fs.existsSync(filePath)) {
            return { success: false, error: 'File not found' };
        }

        const ext = path.extname(filePath).toLowerCase();
        const format = this.detectFormat(ext);

        if (!format) {
            return { success: false, error: `Unsupported file format: ${ext}` };
        }

        try {
            const content = fs.readFileSync(filePath);
            return this.importFromBuffer(content, format);
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Import from buffer/string
    importFromBuffer(content, format = 'showdown') {
        const parser = this.supportedFormats[format]?.parser;
        if (!parser) {
            return { success: false, error: `Unknown format: ${format}` };
        }

        try {
            const pokemon = this[parser](content);
            return {
                success: true,
                format,
                count: pokemon.length,
                pokemon
            };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Detect format from extension
    detectFormat(extension) {
        for (const [format, info] of Object.entries(this.supportedFormats)) {
            if (info.extension === extension) {
                return format;
            }
        }
        return null;
    }

    // Parse Showdown format (multiple Pokemon)
    parseShowdown(content) {
        const text = typeof content === 'string' ? content : content.toString('utf8');
        const pokemon = [];

        // Split by double newlines (Pokemon separator)
        const blocks = text.split(/\n\s*\n/).filter(b => b.trim());

        for (const block of blocks) {
            try {
                const mon = this.parseShowdownBlock(block);
                if (mon) pokemon.push(mon);
            } catch (e) {
                console.error('Failed to parse block:', e);
            }
        }

        return pokemon;
    }

    parseShowdownBlock(block) {
        const lines = block.trim().split('\n');
        if (lines.length === 0) return null;

        const pokemon = {
            species: '',
            nickname: null,
            item: null,
            ability: null,
            level: 100,
            shiny: false,
            nature: 'Hardy',
            teraType: null,
            evs: { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
            ivs: { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
            moves: []
        };

        // Parse first line
        const firstLine = lines[0];
        if (firstLine.includes('@')) {
            const [namePart, itemPart] = firstLine.split('@').map(s => s.trim());
            pokemon.item = itemPart;

            const nickMatch = namePart.match(/(.+)\s*\(([^)]+)\)/);
            if (nickMatch) {
                pokemon.nickname = nickMatch[1].trim();
                pokemon.species = nickMatch[2].trim();
            } else {
                pokemon.species = namePart.replace(/\s*\([MF]\)\s*$/, '').trim();
            }
        } else {
            const nickMatch = firstLine.match(/(.+)\s*\(([^)]+)\)/);
            if (nickMatch) {
                pokemon.nickname = nickMatch[1].trim();
                pokemon.species = nickMatch[2].trim();
            } else {
                pokemon.species = firstLine.replace(/\s*\([MF]\)\s*$/, '').trim();
            }
        }

        // Parse remaining lines
        for (let i = 1; i < lines.length; i++) {
            const line = lines[i].trim();

            if (line.startsWith('Ability:')) {
                pokemon.ability = line.replace('Ability:', '').trim();
            } else if (line.startsWith('Level:')) {
                pokemon.level = parseInt(line.replace('Level:', '').trim());
            } else if (line.startsWith('Shiny:')) {
                pokemon.shiny = line.toLowerCase().includes('yes');
            } else if (line.startsWith('Tera Type:')) {
                pokemon.teraType = line.replace('Tera Type:', '').trim();
            } else if (line.startsWith('EVs:')) {
                this.parseStatLine(line.replace('EVs:', ''), pokemon.evs);
            } else if (line.startsWith('IVs:')) {
                this.parseStatLine(line.replace('IVs:', ''), pokemon.ivs);
            } else if (line.includes('Nature')) {
                const natureMatch = line.match(/(\w+)\s+Nature/);
                if (natureMatch) pokemon.nature = natureMatch[1];
            } else if (line.startsWith('-')) {
                pokemon.moves.push(line.replace(/^-\s*/, '').trim());
            }
        }

        return pokemon;
    }

    parseStatLine(text, statsObj) {
        const parts = text.trim().split('/').map(s => s.trim());
        const statMap = {
            'hp': 'hp', 'atk': 'atk', 'def': 'def',
            'spa': 'spa', 'spd': 'spd', 'spe': 'spe'
        };

        for (const part of parts) {
            const match = part.match(/(\d+)\s*(HP|Atk|Def|SpA|SpD|Spe)/i);
            if (match) {
                const stat = statMap[match[2].toLowerCase()];
                if (stat) statsObj[stat] = parseInt(match[1]);
            }
        }
    }

    // Parse JSON format
    parseJSON(content) {
        const text = typeof content === 'string' ? content : content.toString('utf8');
        const data = JSON.parse(text);

        if (Array.isArray(data)) {
            return data.map(p => this.normalizePokemon(p));
        } else if (data.pokemon && Array.isArray(data.pokemon)) {
            return data.pokemon.map(p => this.normalizePokemon(p));
        } else if (data.species) {
            return [this.normalizePokemon(data)];
        }

        return [];
    }

    // Parse CSV format
    parseCSV(content) {
        const text = typeof content === 'string' ? content : content.toString('utf8');
        const lines = text.trim().split('\n');

        if (lines.length < 2) return [];

        const headers = lines[0].split(',').map(h => h.trim().toLowerCase());
        const pokemon = [];

        for (let i = 1; i < lines.length; i++) {
            const values = this.parseCSVLine(lines[i]);
            if (values.length !== headers.length) continue;

            const obj = {};
            headers.forEach((h, idx) => {
                obj[h] = values[idx];
            });

            pokemon.push(this.normalizePokemon(obj));
        }

        return pokemon;
    }

    parseCSVLine(line) {
        const values = [];
        let current = '';
        let inQuotes = false;

        for (const char of line) {
            if (char === '"') {
                inQuotes = !inQuotes;
            } else if (char === ',' && !inQuotes) {
                values.push(current.trim());
                current = '';
            } else {
                current += char;
            }
        }
        values.push(current.trim());

        return values;
    }

    // Parse PK file format (simplified)
    parsePKFile(buffer) {
        // This would need proper binary parsing for real PK files
        // For now, return empty as this requires complex binary handling
        console.warn('PK file parsing requires binary implementation');
        return [];
    }

    // Normalize Pokemon data to standard format
    normalizePokemon(data) {
        return {
            species: data.species || data.name || 'Unknown',
            nickname: data.nickname || null,
            level: parseInt(data.level) || 100,
            shiny: data.shiny === true || data.shiny === 'true' || data.shiny === 'yes',
            nature: data.nature || 'Hardy',
            ability: data.ability || null,
            item: data.item || data.heldItem || null,
            teraType: data.teraType || data.tera_type || null,
            ball: data.ball || 'Poke Ball',
            gender: data.gender || null,
            ivs: this.normalizeStats(data.ivs || data.iv || {}),
            evs: this.normalizeStats(data.evs || data.ev || {}),
            moves: this.normalizeMoves(data.moves || data.moveset || []),
            ot: data.ot || data.originalTrainer || 'Import',
            tid: parseInt(data.tid) || Math.floor(Math.random() * 65535),
            sid: parseInt(data.sid) || Math.floor(Math.random() * 65535),
            language: data.language || 'ENG'
        };
    }

    normalizeStats(stats) {
        const defaultIVs = { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 };

        if (typeof stats === 'string') {
            // Parse "31/31/31/31/31/31" format
            const parts = stats.split('/').map(v => parseInt(v.trim()) || 0);
            return {
                hp: parts[0] ?? 31,
                atk: parts[1] ?? 31,
                def: parts[2] ?? 31,
                spa: parts[3] ?? 31,
                spd: parts[4] ?? 31,
                spe: parts[5] ?? 31
            };
        }

        return {
            hp: parseInt(stats.hp) || defaultIVs.hp,
            atk: parseInt(stats.atk) || defaultIVs.atk,
            def: parseInt(stats.def) || defaultIVs.def,
            spa: parseInt(stats.spa || stats.spatk) || defaultIVs.spa,
            spd: parseInt(stats.spd || stats.spdef) || defaultIVs.spd,
            spe: parseInt(stats.spe || stats.speed) || defaultIVs.spe
        };
    }

    normalizeMoves(moves) {
        if (typeof moves === 'string') {
            return moves.split(',').map(m => m.trim()).filter(m => m);
        }
        if (Array.isArray(moves)) {
            return moves.filter(m => m).slice(0, 4);
        }
        return [];
    }

    // Import from URL (fetch and parse)
    async importFromURL(url) {
        try {
            const https = require('https');
            const http = require('http');

            return new Promise((resolve, reject) => {
                const protocol = url.startsWith('https') ? https : http;

                protocol.get(url, (res) => {
                    let data = '';
                    res.on('data', chunk => data += chunk);
                    res.on('end', () => {
                        // Detect format from content or URL
                        let format = 'showdown';
                        if (url.endsWith('.json')) format = 'json';
                        else if (url.endsWith('.csv')) format = 'csv';

                        resolve(this.importFromBuffer(data, format));
                    });
                }).on('error', reject);
            });
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Import multiple files
    async importFromDirectory(dirPath, options = {}) {
        if (!fs.existsSync(dirPath)) {
            return { success: false, error: 'Directory not found' };
        }

        const files = fs.readdirSync(dirPath);
        const results = [];
        let totalPokemon = [];

        for (const file of files) {
            const filePath = path.join(dirPath, file);
            const stat = fs.statSync(filePath);

            if (stat.isFile()) {
                const result = await this.importFromFile(filePath);
                results.push({ file, ...result });

                if (result.success) {
                    totalPokemon = totalPokemon.concat(result.pokemon);
                }
            }
        }

        return {
            success: true,
            filesProcessed: results.length,
            totalPokemon: totalPokemon.length,
            pokemon: totalPokemon,
            results
        };
    }

    // Export to various formats
    exportToShowdown(pokemon) {
        const lines = [];

        for (const mon of pokemon) {
            let line1 = mon.species;
            if (mon.nickname && mon.nickname !== mon.species) {
                line1 = `${mon.nickname} (${mon.species})`;
            }
            if (mon.item) line1 += ` @ ${mon.item}`;
            lines.push(line1);

            if (mon.ability) lines.push(`Ability: ${mon.ability}`);
            if (mon.level && mon.level !== 100) lines.push(`Level: ${mon.level}`);
            if (mon.shiny) lines.push('Shiny: Yes');
            if (mon.teraType) lines.push(`Tera Type: ${mon.teraType}`);

            // EVs
            if (mon.evs) {
                const evParts = [];
                const names = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
                for (const [stat, value] of Object.entries(mon.evs)) {
                    if (value > 0) evParts.push(`${value} ${names[stat]}`);
                }
                if (evParts.length > 0) lines.push(`EVs: ${evParts.join(' / ')}`);
            }

            if (mon.nature) lines.push(`${mon.nature} Nature`);

            // IVs
            if (mon.ivs) {
                const ivParts = [];
                const names = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
                for (const [stat, value] of Object.entries(mon.ivs)) {
                    if (value < 31) ivParts.push(`${value} ${names[stat]}`);
                }
                if (ivParts.length > 0) lines.push(`IVs: ${ivParts.join(' / ')}`);
            }

            if (mon.moves) {
                for (const move of mon.moves) {
                    lines.push(`- ${move}`);
                }
            }

            lines.push(''); // Separator
        }

        return lines.join('\n');
    }

    exportToJSON(pokemon) {
        return JSON.stringify({ pokemon, exportedAt: new Date().toISOString() }, null, 2);
    }

    exportToCSV(pokemon) {
        const headers = ['species', 'nickname', 'level', 'shiny', 'nature', 'ability', 'item', 'teraType', 'moves'];
        const lines = [headers.join(',')];

        for (const mon of pokemon) {
            const row = [
                mon.species,
                mon.nickname || '',
                mon.level,
                mon.shiny ? 'Yes' : 'No',
                mon.nature || '',
                mon.ability || '',
                mon.item || '',
                mon.teraType || '',
                (mon.moves || []).join(';')
            ];
            lines.push(row.map(v => `"${v}"`).join(','));
        }

        return lines.join('\n');
    }

    // Save import history
    saveImportHistory(importData) {
        try {
            let history = [];
            if (fs.existsSync(this.importHistoryFile)) {
                history = JSON.parse(fs.readFileSync(this.importHistoryFile, 'utf8'));
            }

            history.push({
                timestamp: new Date().toISOString(),
                format: importData.format,
                count: importData.count,
                source: importData.source || 'unknown'
            });

            // Keep last 100 entries
            history = history.slice(-100);

            fs.writeFileSync(this.importHistoryFile, JSON.stringify(history, null, 2));
        } catch (error) {
            console.error('Failed to save import history:', error);
        }
    }
}

module.exports = PokemonBulkImport;
