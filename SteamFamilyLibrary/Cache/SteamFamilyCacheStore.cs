using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SteamFamilyLibrary.Models;

namespace SteamFamilyLibrary.Cache
{
    public class SteamFamilyCache
    {
        public DateTime LastSyncUtc { get; set; } = DateTime.MinValue;

        public List<string> SteamIds { get; set; } = new List<string>();

        public List<SteamFamilyGame> Games { get; set; } = new List<SteamFamilyGame>();
    }

    public class SteamFamilyCacheStore
    {
        private readonly string cachePath;
        private readonly object fileLock = new object();

        public SteamFamilyCacheStore(string dataDirectory)
        {
            cachePath = Path.Combine(dataDirectory ?? string.Empty, "steamFamilyGamesCache.json");
        }

        public SteamFamilyCache Load()
        {
            try
            {
                if (!File.Exists(cachePath))
                {
                    return null;
                }

                lock (fileLock)
                {
                    var json = File.ReadAllText(cachePath);
                    return JsonConvert.DeserializeObject<SteamFamilyCache>(json);
                }
            }
            catch
            {
                return null;
            }
        }

        public void Save(SteamFamilyCache cache)
        {
            if (cache == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));

            lock (fileLock)
            {
                var json = JsonConvert.SerializeObject(cache, Formatting.Indented);
                File.WriteAllText(cachePath, json);
            }
        }
    }
}

