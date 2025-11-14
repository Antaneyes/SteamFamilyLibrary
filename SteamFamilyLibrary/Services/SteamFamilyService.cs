using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using SteamFamilyLibrary.Cache;
using SteamFamilyLibrary.Configuration;
using SteamFamilyLibrary.Models;

namespace SteamFamilyLibrary.Services
{
    public class SteamFamilyService
    {
        private readonly ILogger logger;
        private readonly SteamWebClient webClient;
        private readonly SteamFamilyCacheStore cacheStore;

        public SteamFamilyService(ILogger logger, SteamWebClient webClient, SteamFamilyCacheStore cacheStore)
        {
            this.logger = logger;
            this.webClient = webClient;
            this.cacheStore = cacheStore;
        }

        public async Task<IReadOnlyCollection<SteamFamilyGame>> GetFamilyGamesAsync(
            SteamFamilyLibrarySettings settings,
            CancellationToken cancellationToken)
        {
            if (settings == null || !settings.IsConfigured)
            {
                return Array.Empty<SteamFamilyGame>();
            }

            var normalizedIds = settings.GetNormalizedSteamIds().ToList();
            if (normalizedIds.Count == 0)
            {
                return Array.Empty<SteamFamilyGame>();
            }

            try
            {
                var games = await FetchGamesAsync(settings, normalizedIds, cancellationToken).ConfigureAwait(false);
                cacheStore.Save(new SteamFamilyCache
                {
                    LastSyncUtc = DateTime.UtcNow,
                    SteamIds = normalizedIds,
                    Games = games.ToList()
                });

                return games;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "No se pudo sincronizar la biblioteca familiar de Steam. Se intentará usar la caché.");
                if (settings.UseCacheWhenOffline)
                {
                    var cache = cacheStore.Load();
                    if (cache?.Games?.Any() == true)
                    {
                        return cache.Games;
                    }
                }

                throw;
            }
        }

        private async Task<IReadOnlyCollection<SteamFamilyGame>> FetchGamesAsync(
            SteamFamilyLibrarySettings settings,
            IReadOnlyCollection<string> steamIds,
            CancellationToken cancellationToken)
        {
            var aggregate = new Dictionary<uint, SteamFamilyGame>();

            foreach (var steamId in steamIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await webClient.GetOwnedGamesAsync(settings.SteamApiKey, steamId, cancellationToken).ConfigureAwait(false);
                var games = response?.Response?.Games;
                if (games == null)
                {
                    continue;
                }

                foreach (var ownedGame in games)
                {
                    if (!aggregate.TryGetValue(ownedGame.AppId, out var familyGame))
                    {
                        familyGame = new SteamFamilyGame
                        {
                            AppId = ownedGame.AppId,
                            Name = ownedGame.Name,
                            IconHash = ownedGame.IconHash,
                            LogoHash = ownedGame.LogoHash,
                            AddedOnUtc = DateTime.UtcNow
                        };
                        aggregate.Add(familyGame.AppId, familyGame);
                    }

                    familyGame.RegisterOwner(steamId, ownedGame);
                }

                if (settings.RequestDelayMs > 0)
                {
                    await Task.Delay(settings.RequestDelayMs, cancellationToken).ConfigureAwait(false);
                }
            }

            return aggregate.Values
                .OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }
}

