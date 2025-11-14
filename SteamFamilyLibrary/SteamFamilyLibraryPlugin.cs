using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamFamilyLibrary.Cache;
using SteamFamilyLibrary.Configuration;
using SteamFamilyLibrary.Controllers;
using SteamFamilyLibrary.Models;
using SteamFamilyLibrary.Services;

namespace SteamFamilyLibrary
{
    public class SteamFamilyLibraryPlugin : LibraryPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SteamFamilyLibrarySettingsViewModel settingsViewModel;
        private readonly SteamFamilyLibrarySettingsView settingsView;
        private readonly SteamFamilyService familyService;
        private readonly SteamMetadataService metadataService;
        private readonly SteamWebClient webClient;
        private readonly MetadataNameProperty sourceProperty = new MetadataNameProperty("Grupo familiar de Steam");
        private readonly MetadataNameProperty categoryProperty = new MetadataNameProperty("Familia Steam");

        public SteamFamilyLibraryPlugin(IPlayniteAPI api) : base(api)
        {
            settingsViewModel = new SteamFamilyLibrarySettingsViewModel(this);
            settingsView = new SteamFamilyLibrarySettingsView
            {
                DataContext = settingsViewModel
            };

            var dataPath = GetPluginUserDataPath();
            var cacheStore = new SteamFamilyCacheStore(dataPath);

            webClient = new SteamWebClient();
            metadataService = new SteamMetadataService(webClient);
            familyService = new SteamFamilyService(Logger, webClient, cacheStore);

            Properties = new LibraryPluginProperties
            {
                HasSettings = true,
                HasCustomizedGameImport = false,
                CanShutdownClient = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("961a2c92-6384-4d00-b5e7-66f4b70ac37c");

        public override string Name => "Grupo familiar de Steam";

        public override LibraryClient Client => null;

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settingsViewModel;
        }

        public override System.Windows.Controls.UserControl GetSettingsView(bool firstRunSettings)
        {
            return settingsView;
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var cancellation = args?.CancelToken ?? CancellationToken.None;

            if (!settingsViewModel.Settings.IsConfigured)
            {
                Logger.Warn("Se solicitó importar la biblioteca familiar de Steam pero la extensión no está configurada.");
                yield break;
            }

            IReadOnlyCollection<SteamFamilyGame> games;
            try
            {
                games = familyService.GetFamilyGamesAsync(settingsViewModel.Settings, cancellation).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error importando los juegos del grupo familiar de Steam.");
                throw;
            }

            foreach (var game in games)
            {
                cancellation.ThrowIfCancellationRequested();
                yield return ConvertToMetadata(game);
            }
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return settingsViewModel.Settings.FetchMetadataOnImport
                ? new SteamFamilyMetadataProvider(metadataService, settingsViewModel)
                : null;
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (!IsFamilyGame(args?.Game))
            {
                yield break;
            }

            yield return new SteamFamilyInstallController(args.Game);
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (!IsFamilyGame(args?.Game))
            {
                yield break;
            }

            yield return new SteamFamilyPlayController(args.Game);
        }

        private GameMetadata ConvertToMetadata(SteamFamilyGame game)
        {
            var storeUri = SteamCdnHelper.GetStoreUrl(game.AppId);
            var storeUrl = storeUri?.AbsoluteUri ?? $"https://store.steampowered.com/app/{game.AppId}/";

            var metadata = new GameMetadata
            {
                GameId = game.AppId.ToString(CultureInfo.InvariantCulture),
                Name = string.IsNullOrWhiteSpace(game.Name) ? $"Steam App {game.AppId}" : game.Name.Trim(),
                Source = sourceProperty,
                Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC (Steam)") },
                Categories = new HashSet<MetadataProperty> { categoryProperty },
                Links = new List<Link> { new Link("Steam", storeUrl) },
                LastActivity = game.LastPlayedUtc,
                Playtime = game.AggregatedPlaytimeMinutes
            };

            metadata.GameActions = new List<GameAction>
            {
                new GameAction
                {
                    Name = "Instalar en Steam",
                    Type = GameActionType.URL,
                    Path = $"steam://install/{game.AppId}"
                },
                new GameAction
                {
                    Name = "Jugar en Steam",
                    Type = GameActionType.URL,
                    Path = $"steam://run/{game.AppId}",
                    IsPlayAction = true
                },
                new GameAction
                {
                    Name = "Abrir en la tienda",
                    Type = GameActionType.URL,
                    Path = storeUrl
                }
            };

            return metadata;
        }

        private bool IsFamilyGame(Game game)
        {
            return game != null && game.PluginId == Id;
        }
    }
}

