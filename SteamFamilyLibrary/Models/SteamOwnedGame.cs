using System.Collections.Generic;
using Newtonsoft.Json;

namespace SteamFamilyLibrary.Models
{
    public class SteamOwnedGamesResponse
    {
        [JsonProperty("response")]
        public SteamOwnedGamesBody Response { get; set; }
    }

    public class SteamOwnedGamesBody
    {
        [JsonProperty("game_count")]
        public int GameCount { get; set; }

        [JsonProperty("games")]
        public List<SteamOwnedGame> Games { get; set; } = new List<SteamOwnedGame>();
    }

    public class SteamOwnedGame
    {
        [JsonProperty("appid")]
        public uint AppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("img_icon_url")]
        public string IconHash { get; set; } = string.Empty;

        [JsonProperty("img_logo_url")]
        public string LogoHash { get; set; } = string.Empty;

        [JsonProperty("playtime_forever")]
        public ulong PlaytimeMinutes { get; set; }

        [JsonProperty("playtime_windows_forever")]
        public ulong PlaytimeWindowsMinutes { get; set; }

        [JsonProperty("playtime_mac_forever")]
        public ulong PlaytimeMacMinutes { get; set; }

        [JsonProperty("playtime_linux_forever")]
        public ulong PlaytimeLinuxMinutes { get; set; }

        [JsonProperty("rtime_last_played")]
        public ulong LastPlayedUnix { get; set; }
    }
}

