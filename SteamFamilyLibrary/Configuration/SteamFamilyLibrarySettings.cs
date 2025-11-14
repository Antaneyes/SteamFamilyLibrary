using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SteamFamilyLibrary.Configuration
{
    public class SteamFamilyLibrarySettings
    {
        private const string DefaultLanguage = "es";
        private const string DefaultRegion = "es";

        public string SteamApiKey { get; set; } = string.Empty;

        public string FamilySteamIds { get; set; } = string.Empty;

        public bool FetchMetadataOnImport { get; set; } = true;

        public string MetadataLanguage { get; set; } = DefaultLanguage;

        public string MetadataRegion { get; set; } = DefaultRegion;

        public int RequestDelayMs { get; set; } = 250;

        public bool UseCacheWhenOffline { get; set; } = true;

        [JsonIgnore]
        public bool IsConfigured => !string.IsNullOrWhiteSpace(SteamApiKey) && GetNormalizedSteamIds().Any();

        public IEnumerable<string> GetNormalizedSteamIds()
        {
            if (string.IsNullOrWhiteSpace(FamilySteamIds))
            {
                return Enumerable.Empty<string>();
            }

            return FamilySteamIds
                .Split(new[] { '\r', '\n', ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public SteamFamilyLibrarySettings Clone()
        {
            return JsonConvert.DeserializeObject<SteamFamilyLibrarySettings>(JsonConvert.SerializeObject(this));
        }
    }
}

