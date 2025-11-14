using System;

namespace SteamFamilyLibrary.Services
{
    public static class SteamCdnHelper
    {
        public static string GetIconUrl(uint appId, string hash)
        {
            if (appId == 0 || string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            return $"https://media.steampowered.com/steamcommunity/public/images/apps/{appId}/{hash}.ico";
        }

        public static string GetLogoUrl(uint appId, string hash)
        {
            if (appId == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(hash))
            {
                return $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/{hash}.jpg";
            }

            return $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_600x900.jpg";
        }

        public static Uri GetStoreUrl(uint appId)
        {
            return new Uri($"https://store.steampowered.com/app/{appId}/");
        }
    }
}

