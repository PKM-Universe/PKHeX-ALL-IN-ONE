# PKHeX WebTools

Web-based Pokemon utilities and companion tools for PKHeX ALL-IN-ONE.

## Features

### Core Utilities
| Tool | Description | File |
|------|-------------|------|
| **Damage Calculator** | Calculate damage with type effectiveness, STAB, weather, abilities | `utils/pokemonDamageCalculator.js` |
| **Breeding Calculator** | IV inheritance, egg moves, nature passing, shiny odds | `utils/pokemonBreedingCalculator.js` |
| **Team Builder** | Build teams with synergy analysis, type coverage, Showdown export | `utils/pokemonTeamBuilder.js` |
| **Living Dex Tracker** | Track all 1025 Pokemon including forms and shinies | `utils/pokemonLivingDex.js` |
| **Wonder Trade Simulator** | Simulate wonder trades with rarity tiers | `utils/pokemonWonderTradeSimulator.js` |

### Advanced Editors
| Tool | Description | File |
|------|-------------|------|
| **Save File Reader** | Parse .sav files from Gen 3-9 games | `utils/pokemonSaveReader.js` |
| **PK File Export** | Export to .pk7/.pk8/.pk9 formats | `utils/pokemonPKExport.js` |
| **Memory Editor** | Edit OT memories, geo locations, affection | `utils/pokemonMemoryEditor.js` |
| **Ribbon Editor** | Full ribbon collection from Gen 3-9 | `utils/pokemonRibbonEditor.js` |
| **HOME Tracker** | Track HOME IDs and transfer history | `utils/pokemonHomeTracker.js` |
| **Contest Stats** | Beauty/Cool/Cute/Smart/Tough editing | `utils/pokemonContestStats.js` |

### Quality of Life
| Tool | Description | File |
|------|-------------|------|
| **Auto-Legality Fix** | Automatically fix illegal Pokemon | `utils/pokemonAutoLegality.js` |
| **Bulk Import** | Import from Showdown, JSON, CSV formats | `utils/pokemonBulkImport.js` |
| **Cloud Sync** | Backup collections to Discord/local storage | `utils/pokemonCloudSync.js` |
| **Trade History** | Track all generated/traded Pokemon | `utils/pokemonTradeHistory.js` |
| **Trade Queue** | Integration with SysBot for trading | `utils/pokemonTradeQueue.js` |

### Databases
| Database | Description | File |
|----------|-------------|------|
| **Mystery Gifts** | 108 event Pokemon (2004-2024) | `data/mystery_gift_expanded.json` |
| **Raid Database** | Tera Raids, Max Raids, Dynamax Adventures | `data/raid_database.json` |
| **Smogon Sets** | Competitive movesets for Gen 9 OU/VGC | `data/smogon_movesets.json` |
| **Locations** | 450+ met locations for Gen 7-9 | `data/location_database.json` |

## Installation

```bash
cd WebTools
npm install
npm start
```

The API will run on `http://localhost:3001`

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api` | GET | API info and available endpoints |
| `/api/pokemon` | GET | List all Pokemon species |
| `/api/species/:name` | GET | Get specific Pokemon data |
| `/api/events` | GET | List event Pokemon |
| `/api/raids` | GET | Get raid database |
| `/api/smogon/:format/:pokemon` | GET | Get Smogon sets |
| `/api/locations/:game` | GET | Get met locations |
| `/api/validate` | POST | Validate Pokemon legality |
| `/api/generate` | POST | Generate Pokemon |
| `/api/showdown/parse` | POST | Parse Showdown format |
| `/api/showdown/export` | POST | Export to Showdown format |

## Mobile PWA

Open `public/index.html` in a browser for the mobile-friendly Pokemon creator interface.

Features:
- Create Pokemon with full stat customization
- IV/EV sliders
- Nature and ability selection
- Move selection
- Competitive templates
- Trade queue integration

## Usage Examples

### Damage Calculator
```javascript
const DamageCalculator = require('./utils/pokemonDamageCalculator');
const calc = new DamageCalculator();

const result = calc.calculateDamage({
    attacker: { species: 'Garchomp', level: 100, atk: 359 },
    defender: { species: 'Tyranitar', level: 100, def: 350, hp: 404 },
    move: { name: 'Earthquake', power: 100, type: 'ground', category: 'physical' }
});
```

### Breeding Calculator
```javascript
const BreedingCalculator = require('./utils/pokemonBreedingCalculator');
const breeding = new BreedingCalculator();

const result = breeding.calculateOffspring({
    parent1: { species: 'Ditto', ivs: { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 } },
    parent2: { species: 'Garchomp', nature: 'Jolly', ability: 'Rough Skin' },
    items: { parent1: 'destiny-knot', parent2: 'everstone' }
});
```

### Team Builder
```javascript
const TeamBuilder = require('./utils/pokemonTeamBuilder');
const builder = new TeamBuilder();

builder.addPokemon({ species: 'Garchomp', moves: ['Earthquake', 'Dragon Claw'] });
builder.addPokemon({ species: 'Kingambit', moves: ['Kowtow Cleave', 'Sucker Punch'] });
const analysis = builder.analyzeTeam();
```

## License

MIT License - Part of PKHeX ALL-IN-ONE by PKM-Universe
