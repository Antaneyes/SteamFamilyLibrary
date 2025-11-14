using System;
using System.Diagnostics;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace SteamFamilyLibrary.Controllers
{
    public class SteamFamilyInstallController : InstallController
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public SteamFamilyInstallController(Game game) : base(game)
        {
            Name = "Steam";
        }

        public override void Install(InstallActionArgs args)
        {
            if (!uint.TryParse(Game?.GameId, out var appId))
            {
                Logger.Warn($"El juego '{Game?.Name}' no tiene un SteamID válido para instalar.");
                return;
            }

            LaunchSteamUrl($"steam://install/{appId}");
        }

        internal static void LaunchSteamUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"No se pudo ejecutar la URL de Steam: {url}");
            }
        }
    }
}

