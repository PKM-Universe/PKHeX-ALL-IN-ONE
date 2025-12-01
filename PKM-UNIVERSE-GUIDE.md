# PKM-Universe Complete User Guide

## Table of Contents
1. [Getting Started with PKHeX](#getting-started-with-pkhex)
2. [New PKM-Universe Features](#new-pkm-universe-features)
3. [Step-by-Step Tutorials](#step-by-step-tutorials)

---

# Getting Started with PKHeX

## What is PKHeX/PKM-Universe?

PKM-Universe is an enhanced version of PKHeX - a save editor for main series Pokemon games. It allows you to:
- View and edit Pokemon in your save files
- Check legality of Pokemon
- Create new Pokemon from scratch
- Manage your boxes and party
- Import/Export Pokemon in various formats

## Supported Games

| Generation | Games |
|------------|-------|
| Gen 1 | Red, Blue, Yellow |
| Gen 2 | Gold, Silver, Crystal |
| Gen 3 | Ruby, Sapphire, Emerald, FireRed, LeafGreen |
| Gen 4 | Diamond, Pearl, Platinum, HeartGold, SoulSilver |
| Gen 5 | Black, White, Black 2, White 2 |
| Gen 6 | X, Y, Omega Ruby, Alpha Sapphire |
| Gen 7 | Sun, Moon, Ultra Sun, Ultra Moon, Let's Go Pikachu/Eevee |
| Gen 8 | Sword, Shield, Brilliant Diamond, Shining Pearl, Legends Arceus |
| Gen 9 | Scarlet, Violet |

## Loading a Save File

### Step 1: Get Your Save File
- **Switch Games**: Use tools like Checkpoint or JKSV to backup saves
- **3DS Games**: Use Checkpoint or JKSM
- **Emulator**: Find save files in emulator's save folder

### Step 2: Open in PKM-Universe
1. Launch PKM-Universe
2. Go to **File → Open** (or drag and drop)
3. Select your save file
4. Your boxes and party will appear on the right side

### Step 3: Understanding the Interface

```
┌─────────────────────────────────────────────────────────────┐
│  File  Edit  Tools  Options  PKM-Universe                   │
├─────────────────────────────────────────────────────────────┤
│                           │                                 │
│   POKEMON EDITOR          │      BOX VIEWER                 │
│   (Left Side)             │      (Right Side)               │
│                           │                                 │
│   - Species               │   [Pokemon Grid]                │
│   - Nickname              │   [Box 1] [Box 2] [Box 3]...    │
│   - Level                 │                                 │
│   - Moves                 │   PARTY                         │
│   - Stats/IVs/EVs         │   [6 Pokemon slots]             │
│   - OT Info               │                                 │
│                           │                                 │
└─────────────────────────────────────────────────────────────┘
```

## Basic Pokemon Editing

### Changing Species
1. Click on a Pokemon in your box
2. In the left panel, click the Species dropdown
3. Select the new species
4. The Pokemon will update with default moves/abilities

### Editing Stats

**IVs (Individual Values)**: 0-31, higher = better
- Click the "Stats" tab
- Adjust IV sliders or type values
- Click "Max" button to set all to 31

**EVs (Effort Values)**: 0-252 per stat, 510 total
- Each stat can have up to 252 EVs
- Common spreads: 252/252/4 for offensive Pokemon

### Changing Moves
1. Go to the "Moves" tab
2. Click each move dropdown
3. Select new moves
4. PP will auto-adjust

### Making a Pokemon Shiny
1. Click the star icon next to the PID
2. Or go to OT/Misc tab and check "Shiny"

## Legality Checking

The legality checker shows a colored indicator:
- **Green checkmark**: Legal Pokemon
- **Red X**: Illegal Pokemon (click to see why)

Common legality issues:
- Invalid ball combination
- Impossible move combinations
- Wrong ability for the species
- Invalid met location

---

# New PKM-Universe Features

## 1. Pokemon Search (Ctrl+F)

**What it does**: Search for any Pokemon across ALL your boxes instantly.

### How to Use:
1. Press **Ctrl+F** or go to **PKM-Universe → Search Pokemon**
2. Enter search criteria:
   - Species name (e.g., "Pikachu")
   - Filter by Shiny only
   - Filter by specific types
3. Click **Search**
4. Results show box/slot location
5. Double-click a result to load it into the editor

---

## 2. Team Coverage Analyzer

**What it does**: Analyzes your team's type coverage and shows weaknesses/resistances.

### How to Use:
1. Go to **PKM-Universe → Team Coverage Analyzer**
2. Your party is automatically loaded
3. View the type chart showing:
   - **Green**: Types you resist
   - **Red**: Types you're weak to
   - **Yellow**: Neutral coverage
4. See recommendations for team improvements

---

## 3. Damage Calculator

**What it does**: Calculate damage between two Pokemon with specific moves.

### How to Use:
1. Go to **PKM-Universe → Damage Calculator**
2. Set up the attacking Pokemon:
   - Select species, level, nature
   - Set EVs/IVs
   - Choose the attack move
3. Set up the defending Pokemon similarly
4. Click **Calculate**
5. See damage ranges and OHKO/2HKO chances

---

## 4. Showdown Import/Export

**What it does**: Import Pokemon from Pokemon Showdown format or export your Pokemon to share.

### How to Import:
1. Go to **PKM-Universe → Showdown Import/Export**
2. Paste Showdown format text:
```
Pikachu @ Light Ball
Ability: Static
EVs: 252 SpA / 4 SpD / 252 Spe
Timid Nature
- Thunderbolt
- Volt Switch
- Grass Knot
- Hidden Power Ice
```
3. Click **Import to Editor**
4. Pokemon appears in the editor

### How to Export:
1. Load a Pokemon in the editor
2. Open Showdown Import/Export
3. Click **Export Current**
4. Copy the text to share

---

## 5. Smogon Set Importer

**What it does**: Import popular competitive sets directly from Smogon.

### How to Use:
1. Go to **PKM-Universe → Smogon Set Importer**
2. Select a Pokemon species
3. Browse available sets (OU, UU, etc.)
4. Click a set to preview
5. Click **Import** to load into editor

---

## 6. Tournament Team Manager

**What it does**: Save and load complete teams for tournaments.

### Saving a Team:
1. Go to **PKM-Universe → Tournament Team Manager**
2. Your current party is displayed
3. Enter a team name (e.g., "VGC 2024 Team")
4. Click **Save Team**

### Loading a Team:
1. Open Tournament Team Manager
2. Select a saved team from the list
3. Click **Load Team**
4. Team is loaded into your party

---

## 7. Generate QR Code (NEW)

**What it does**: Creates a QR code image for any Pokemon.

### How to Use:
1. Load a Pokemon in the editor
2. Go to **PKM-Universe → Generate QR Code**
3. A QR code is generated showing:
   - Pokemon sprite
   - Pokemon data encoded
4. Click **Save QR Code** to save as PNG
5. Click **Copy to Clipboard** to share

---

## 8. Compare Pokemon (NEW)

**What it does**: Compare two Pokemon side-by-side to see stat differences.

### How to Use:
1. Load a Pokemon in the editor
2. Go to **PKM-Universe → Compare Pokemon**
3. Pokemon 1 is auto-loaded from editor
4. Click **Load from Box** to select Pokemon 2
5. View comparison:
   - Stat bars show each Pokemon's stats
   - Green numbers = higher stat
   - Red numbers = lower stat
6. Use **Swap** button to switch positions

---

## 9. Trade History Log (NEW)

**What it does**: Keeps a history of all Pokemon trades you've made.

### How to Use:
1. Go to **PKM-Universe → Trade History Log**
2. View all logged trades with:
   - Date/Time
   - Pokemon traded
   - Original Trainer info
   - Game version
3. Click **Export CSV** to save history
4. Click **Clear History** to reset

---

## 10. Discord Rich Presence (NEW)

**What it does**: Shows your PKM-Universe activity in Discord.

### How to Use:
1. Go to **PKM-Universe → Discord Rich Presence**
2. Check/uncheck to toggle
3. When enabled, Discord shows:
   - "Playing PKM-Universe"
   - Current game you're editing
   - How long you've been editing

---

## 11. Quick Start Tutorial (F1)

**What it does**: Interactive tutorial for new users.

### How to Access:
1. Press **F1** or go to **PKM-Universe → Quick Start Tutorial**
2. Follow the step-by-step guide
3. Learn basic editing features

---

## 12. Auto-Backup Manager

**What it does**: Automatically backs up your save file periodically.

### How it Works:
- Backups are created every 5 minutes by default
- Stored in the "Backups" folder
- Keeps last 20 backups
- Access via **PKM-Universe → Backup Manager**

---

## 13. Recent Files

**What it does**: Quick access to recently opened save files.

### How to Use:
1. Go to **PKM-Universe → Recent Files**
2. Click any recent file to open it instantly

---

## 14. Box Wallpapers

**What it does**: Customize your box backgrounds.

### How to Use:
1. Go to **PKM-Universe → Box Wallpapers**
2. Select a box
3. Choose from available wallpapers
4. Apply to current or all boxes

---

## 15. Shiny Living Dex Generator

**What it does**: Helps you track and generate a complete shiny living dex.

### How to Use:
1. Go to **PKM-Universe → Shiny Living Dex**
2. View which shinies you have/need
3. Options to generate missing Pokemon
4. Calculate boxes needed

---

# Step-by-Step Tutorials

## Tutorial 1: Creating a Competitive Pokemon from Scratch

### Goal: Create a competitive Garchomp

1. **Start Fresh**
   - Go to **File → New** or use a blank slot

2. **Set Species**
   - Species: Garchomp
   - Form: 0 (default)

3. **Set Level**
   - Level: 100 (for competitive)

4. **Set Nature**
   - Go to OT/Misc tab
   - Nature: Jolly (+Spe, -SpA)

5. **Set Ability**
   - Ability: Rough Skin

6. **Set IVs**
   - Go to Stats tab
   - Click "Max" for 31 in all stats
   - Or set SpA to 0 if not using special moves

7. **Set EVs**
   - HP: 0
   - ATK: 252
   - DEF: 4
   - SPA: 0
   - SPD: 0
   - SPE: 252

8. **Set Moves**
   - Move 1: Earthquake
   - Move 2: Outrage
   - Move 3: Swords Dance
   - Move 4: Scale Shot

9. **Set Item**
   - Held Item: Choice Scarf (or Life Orb)

10. **Check Legality**
    - Look for green checkmark
    - Fix any issues shown

11. **Save**
    - Drag to a box slot
    - File → Save

---

## Tutorial 2: Importing a Team from Showdown

### Goal: Import a full 6-Pokemon team

1. **Copy from Showdown**
   - Go to Pokemon Showdown teambuilder
   - Copy entire team (all 6 Pokemon)

2. **Open Importer**
   - **PKM-Universe → Showdown Import/Export**

3. **Paste Team**
   - Paste all Pokemon text
   - Each Pokemon separated by blank line

4. **Import One by One**
   - Click **Import to Editor**
   - Drag to party slot
   - Clear textbox
   - Paste next Pokemon
   - Repeat for all 6

5. **Verify Team**
   - Check each Pokemon's legality
   - Use Team Coverage Analyzer to check weaknesses

6. **Save**
   - Save your team with Tournament Team Manager
   - Save your save file

---

## Tutorial 3: Finding All Shiny Pokemon in Your Save

1. **Open Search**
   - Press **Ctrl+F**

2. **Set Filters**
   - Leave species blank
   - Check "Shiny Only"

3. **Search**
   - Click Search button
   - All shiny Pokemon listed with locations

4. **Review Results**
   - Double-click any result to view
   - Note box/slot for organization

---

## Tutorial 4: Comparing Your Pokemon's Stats

1. **Load First Pokemon**
   - Click on Pokemon in box
   - It loads in editor

2. **Open Comparison Tool**
   - **PKM-Universe → Compare Pokemon**

3. **Load Second Pokemon**
   - Click "Load from Box"
   - Navigate to box with Pokemon to compare
   - Click the Pokemon

4. **Analyze Differences**
   - View stat bars
   - Green = winner in that stat
   - Note: Different Pokemon have different base stats!

5. **Use for Decisions**
   - Compare EVs, IVs, natures
   - Decide which to keep/use

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+O | Open save file |
| Ctrl+S | Save save file |
| Ctrl+F | Search Pokemon |
| Ctrl+E | Export Pokemon |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| F1 | Quick Start Tutorial |
| Delete | Clear selected slot |

---

## Troubleshooting

### "Invalid save file"
- Make sure you're opening the correct save type
- Check file isn't corrupted
- Try re-dumping from console

### Pokemon shows as illegal
- Click the red X to see specific issues
- Common fixes:
  - Change ball to legal option
  - Fix met location/date
  - Remove impossible moves

### Can't find PKM-Universe menu
- It's in the menu bar at the top
- Look for red text "PKM-Universe"

### Discord Rich Presence not showing
- Make sure Discord is running
- Toggle the option off and on
- Restart PKM-Universe

---

## Credits

PKM-Universe is built on top of PKHeX by Kaphotics.
Enhanced features developed for the PKM-Universe community.

For support, join our Discord or visit our website through the PKM-Universe menu!
