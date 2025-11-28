# Installation Guide

## Requirements

- Windows 10/11
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (usually auto-installed)

## Download

1. Go to the [Releases Page](https://github.com/PKM-Universe/PKHeX-ALL-IN-ONE/releases/latest)
2. Download `PKM-Universe-vXX.XX.XX.zip`
3. Extract to a folder of your choice

## Running PKHeX

1. Navigate to the extracted folder
2. Double-click `PKHeX.exe`
3. That's it! The plugins are pre-installed in the `plugins` folder.

## Updating

PKHeX will automatically check for updates on startup. When a new version is available:
1. A notification will appear in the top-right corner
2. Click it to go to the releases page
3. Download the new version and replace your files

## Folder Structure

```
PKHeX/
├── PKHeX.exe          # Main application
├── PKHeX.Core.dll     # Core library
├── plugins/           # Plugin folder
│   ├── AutoModPlugins.dll
│   ├── PKHeX.Plugin.LivingDex.dll
│   └── ...
└── cfg.json          # Settings (created after first run)
```

## Troubleshooting

### "PKHeX.Core.dll not found"
Make sure you extracted ALL files from the ZIP, not just PKHeX.exe.

### Plugins not loading
1. Check that the `plugins` folder exists
2. Ensure `.dll` files are inside it
3. Go to Options > Settings > Startup > Enable Plugin Loading

### Application won't start
Install the [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0/runtime).
