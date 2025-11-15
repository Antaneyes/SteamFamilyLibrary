# Playnite – Steam family library integration

This repository contains a **GameLibrary** plugin for Playnite that adds a new source called `Steam Family Group`. It aggregates the libraries of the Steam Family Sharing members you configure and keeps their metadata in sync with the Steam store.

## Features

- Calls the official Steam Web API (`IPlayerService/GetOwnedGames`) for every configured SteamID64.
- Merges the responses into a local cache (`steamFamilyGamesCache.json`) so you can browse the combined collection even when the API is unavailable.
- Imports the games into Playnite with the custom source `Steam Family Group` and the category `Steam Family` for easy filtering.
- Downloads descriptions, covers and backgrounds from `store.steampowered.com` when metadata download is enabled.
- Provides “Install via Steam” and “Play via Steam” actions that launch the official Steam client through the `steam://` protocol.

## Requirements

1. **Steam Web API key** (https://steamcommunity.com/dev/apikey)
2. **SteamID64** for each account in your family sharing group (you can copy their profile URL and paste it into https://steamid.io to obtain the ID; add one per line or separate them with commas).
3. Playnite 10/11 with .NET plugin support (`RequiresApiVersion: 6.11.0`). See the [official extension docs](https://api.playnite.link/docs/tutorials/extensions/intro.html).

## Configuration

1. Open Playnite → `Settings` → `Extensions` → select “Steam Family Group”.
2. Paste your Web API key and the list of SteamID64 values.
3. (Optional) Adjust metadata language, price region and request delay if you need throttling.
4. Keep “Download metadata automatically” enabled to pull imagery from Steam.
5. Save and trigger `Library → Update game libraries`.

## Project structure

- `SteamFamilyLibraryPlugin.cs`: Playnite entry point and library implementation.
- `Configuration/`: settings model, WPF view and view model.
- `Services/`: Steam Web API client, metadata downloader, cache and helpers.
- `Models/`: DTOs for Steam responses and aggregated family data.
- `Cache/`: serialization helpers for `steamFamilyGamesCache.json`.
- `extension.yaml`: plugin manifest consumed by Playnite.

## Build

```powershell
dotnet restore SteamFamilyLibrary/SteamFamilyLibrary.csproj
dotnet build SteamFamilyLibrary/SteamFamilyLibrary.csproj -c Release
```

## Installation

### Option 1: `.pext` package

Check the **Releases** tab for a ready-made `.pext` (for example `SteamFamilyGroup-v0.1.0-beta.pext`). Download it and simply double-click the file

### Option 2: manual copy

1. Create a folder inside `Playnite/Extensions`, for example `SteamFamilyGroup`.
2. Copy **all** files from `SteamFamilyLibrary/bin/Release/net472/` (SteamFamilyLibrary.dll plus every dependency such as `Newtonsoft.Json.dll`, `System.Text.Json.dll`, etc.).
3. Copy `extension.yaml` into the same folder.
4. Restart Playnite.

