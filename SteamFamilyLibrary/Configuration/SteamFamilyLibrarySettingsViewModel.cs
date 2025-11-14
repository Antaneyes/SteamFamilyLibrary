using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Playnite.SDK;
using SteamFamilyLibrary.Configuration;

namespace SteamFamilyLibrary.Configuration
{
    public class SteamFamilyLibrarySettingsViewModel : ObservableObject, ISettings, IEditableObject
    {
        private readonly SteamFamilyLibraryPlugin plugin;
        private SteamFamilyLibrarySettings editingClone;

        public SteamFamilyLibrarySettings Settings { get; private set; }

        public SteamFamilyLibrarySettingsViewModel(SteamFamilyLibraryPlugin plugin)
        {
            this.plugin = plugin;
            Settings = plugin.LoadPluginSettings<SteamFamilyLibrarySettings>() ?? new SteamFamilyLibrarySettings();
        }

        public void BeginEdit()
        {
            editingClone = Settings.Clone();
        }

        public void CancelEdit()
        {
            if (editingClone != null)
            {
                Settings = editingClone;
            }
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Settings.SteamApiKey))
            {
                errors.Add("Debes introducir una clave de la Web API de Steam.");
            }

            if (!Settings.GetNormalizedSteamIds().Any())
            {
                errors.Add("Agrega al menos un SteamID 64 del grupo familiar.");
            }

            return errors.Count == 0;
        }
    }
}

