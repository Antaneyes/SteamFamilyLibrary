using System;
using System.Globalization;
using System.Threading;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamFamilyLibrary.Configuration;

namespace SteamFamilyLibrary.Services
{
    public class SteamFamilyMetadataProvider : LibraryMetadataProvider
    {
        private readonly SteamMetadataService metadataService;
        private readonly SteamFamilyLibrarySettingsViewModel settingsViewModel;
        private static readonly ILogger Logger = LogManager.GetLogger();

        public SteamFamilyMetadataProvider(
            SteamMetadataService metadataService,
            SteamFamilyLibrarySettingsViewModel settingsViewModel)
        {
            this.metadataService = metadataService;
            this.settingsViewModel = settingsViewModel;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            if (game == null)
            {
                return new GameMetadata();
            }

            if (!uint.TryParse(game.GameId, NumberStyles.Any, CultureInfo.InvariantCulture, out var appId))
            {
                return new GameMetadata();
            }

            try
            {
                var cancellationToken = CancellationToken.None;
                return metadataService.GetMetadataAsync(
                    appId,
                    settingsViewModel.Settings.MetadataLanguage,
                    settingsViewModel.Settings.MetadataRegion,
                    cancellationToken).GetAwaiter().GetResult() ?? new GameMetadata();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to download metadata for app {appId}.");
                return new GameMetadata();
            }
        }
    }
}

