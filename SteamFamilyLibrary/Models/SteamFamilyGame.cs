using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamFamilyLibrary.Models
{
    public class SteamFamilyGame
    {
        public uint AppId { get; set; }

        public string Name { get; set; } = string.Empty;

        public HashSet<string> Owners { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ulong AggregatedPlaytimeMinutes { get; set; }

        public DateTime? LastPlayedUtc { get; set; }

        public string IconHash { get; set; } = string.Empty;

        public string LogoHash { get; set; } = string.Empty;

        public DateTime AddedOnUtc { get; set; } = DateTime.UtcNow;

        public void RegisterOwner(string steamId, SteamOwnedGame ownedGame)
        {
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                Owners.Add(steamId);
            }

            if (ownedGame != null)
            {
                Name = string.IsNullOrWhiteSpace(ownedGame.Name) ? Name : ownedGame.Name;
                IconHash = string.IsNullOrWhiteSpace(ownedGame.IconHash) ? IconHash : ownedGame.IconHash;
                LogoHash = string.IsNullOrWhiteSpace(ownedGame.LogoHash) ? LogoHash : ownedGame.LogoHash;
                AggregatedPlaytimeMinutes += ownedGame.PlaytimeMinutes;

                if (ownedGame.LastPlayedUnix > 0)
                {
                    var candidate = DateTimeOffset.FromUnixTimeSeconds((long)ownedGame.LastPlayedUnix).UtcDateTime;
                    if (!LastPlayedUtc.HasValue || candidate > LastPlayedUtc)
                    {
                        LastPlayedUtc = candidate;
                    }
                }
            }
        }

        public override string ToString()
        {
            var owners = Owners?.Any() == true ? string.Join(",", Owners) : "??";
            return $"{AppId}:{Name} ({owners})";
        }
    }
}

