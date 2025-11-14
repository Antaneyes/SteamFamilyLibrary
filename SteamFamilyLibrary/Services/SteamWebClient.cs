using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamFamilyLibrary.Models;

namespace SteamFamilyLibrary.Services
{
    public class SteamWebClient
    {
        private static readonly Uri OwnedGamesEndpoint = new Uri("https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/", UriKind.Absolute);
        private static readonly string AppDetailsUrlTemplate = "https://store.steampowered.com/api/appdetails?appids={0}&l={1}&cc={2}";
        private static readonly HttpClient HttpClient;

        static SteamWebClient()
        {
            HttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SteamFamilyPlaynitePlugin/1.0 (+https://playnite.link)");
        }

        public async Task<SteamOwnedGamesResponse> GetOwnedGamesAsync(string apiKey, string steamId, CancellationToken cancellationToken)
        {
            var builder = new UriBuilder(OwnedGamesEndpoint)
            {
                Query = $"key={Uri.EscapeDataString(apiKey)}&steamid={Uri.EscapeDataString(steamId)}&include_appinfo=1&include_played_free_games=1"
            };

            using (var response = await HttpClient.GetAsync(builder.Uri, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<SteamOwnedGamesResponse>(json);
            }
        }

        public async Task<JToken> GetAppDetailsAsync(uint appId, string languageCode, string countryCode, CancellationToken cancellationToken)
        {
            var url = string.Format(AppDetailsUrlTemplate, appId, languageCode ?? "es", countryCode ?? "es");
            using (var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var parsed = JsonConvert.DeserializeObject<JObject>(json);
                if (parsed == null)
                {
                    return null;
                }

                if (!parsed.TryGetValue(appId.ToString(), out var payload))
                {
                    return null;
                }

                var success = payload["success"]?.Value<bool>() ?? false;
                if (!success)
                {
                    return null;
                }

                return payload["data"];
            }
        }

        public async Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }
    }
}

