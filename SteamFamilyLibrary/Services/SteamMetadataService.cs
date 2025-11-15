using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Playnite.SDK.Models;

namespace SteamFamilyLibrary.Services
{
    public class SteamMetadataService
    {
        private readonly SteamWebClient steamWebClient;

        public SteamMetadataService(SteamWebClient steamWebClient)
        {
            this.steamWebClient = steamWebClient;
        }

        public async Task<GameMetadata> GetMetadataAsync(uint appId, string languageCode, string regionCode, CancellationToken cancellationToken)
        {
            var data = await steamWebClient.GetAppDetailsAsync(appId, languageCode, regionCode, cancellationToken).ConfigureAwait(false);
            if (data == null)
            {
                return null;
            }

            var metadata = new GameMetadata
            {
                Name = data["name"]?.Value<string>(),
                GameId = appId.ToString(CultureInfo.InvariantCulture),
                Description = data["about_the_game"]?.Value<string>() ?? data["short_description"]?.Value<string>(),
                Links = BuildLinks(data),
                Developers = BuildPropertySet(data["developers"] as JArray),
                Publishers = BuildPropertySet(data["publishers"] as JArray),
                Genres = BuildPropertySet((data["genres"] as JArray)?.Select(j => j["description"]?.Value<string>())),
                Tags = BuildPropertySet((data["categories"] as JArray)?.Select(j => j["description"]?.Value<string>())),
                ReleaseDate = ParseReleaseDate(data["release_date"]?["date"]?.Value<string>()),
                CommunityScore = data["metacritic"]?["score"]?.Value<int?>()
            };

            var header = data["header_image"]?.Value<string>();
            var background = data["background_raw"]?.Value<string>() ?? data["background"]?.Value<string>();

            if (!string.IsNullOrWhiteSpace(header))
            {
                var coverBytes = await steamWebClient.DownloadBytesAsync(header, cancellationToken).ConfigureAwait(false);
                if (coverBytes != null)
                {
                    metadata.CoverImage = new MetadataFile($"steam_{appId}_cover.jpg", coverBytes);
                }
            }

            if (!string.IsNullOrWhiteSpace(background))
            {
                var backgroundBytes = await steamWebClient.DownloadBytesAsync(background, cancellationToken).ConfigureAwait(false);
                if (backgroundBytes != null)
                {
                    metadata.BackgroundImage = new MetadataFile($"steam_{appId}_background.jpg", backgroundBytes);
                }
            }

            if (data["screenshots"] is JArray screenshots && screenshots.Any())
            {
                if (metadata.Links == null)
                {
                    metadata.Links = new List<Link>();
                }

                metadata.Links.Add(new Link("Screenshots", $"https://store.steampowered.com/app/{appId}/#app_{appId}_screenshots"));
            }

            return metadata;
        }

        private static List<Link> BuildLinks(JToken data)
        {
            var links = new List<Link>();

            var website = data?["website"]?.Value<string>();
            if (!string.IsNullOrWhiteSpace(website))
            {
                links.Add(new Link("Official site", website));
            }

            var storeUrl = data?["steam_appid"]?.Value<uint?>();
            if (storeUrl.HasValue)
            {
                links.Add(new Link("Steam", $"https://store.steampowered.com/app/{storeUrl.Value}/"));
            }

            return links;
        }

        private static HashSet<MetadataProperty> BuildPropertySet(IEnumerable<string> values)
        {
            if (values == null)
            {
                return null;
            }

            var set = new HashSet<MetadataProperty>();
            foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                set.Add(new MetadataNameProperty(value.Trim()));
            }

            return set.Count == 0 ? null : set;
        }

        private static HashSet<MetadataProperty> BuildPropertySet(JArray values)
        {
            if (values == null)
            {
                return null;
            }

            var set = new HashSet<MetadataProperty>();
            foreach (var value in values)
            {
                var description = value.Type == JTokenType.String
                    ? value.Value<string>()
                    : value["description"]?.Value<string>();

                if (!string.IsNullOrWhiteSpace(description))
                {
                    set.Add(new MetadataNameProperty(description.Trim()));
                }
            }

            return set.Count == 0 ? null : set;
        }

        private static ReleaseDate? ParseReleaseDate(string rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
            {
                return null;
            }

            if (DateTime.TryParse(rawDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var parsed) ||
                DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
            {
                if (parsed.Day >= 1)
                {
                    return new ReleaseDate(parsed.Year, parsed.Month, parsed.Day);
                }

                if (parsed.Month > 0)
                {
                    return new ReleaseDate(parsed.Year, parsed.Month);
                }

                return new ReleaseDate(parsed.Year);
            }

            return null;
        }
    }
}

