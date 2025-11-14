using System;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace SteamFamilyLibrary.Controllers
{
    public class SteamFamilyPlayController : PlayController
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public SteamFamilyPlayController(Game game) : base(game)
        {
            Name = "Steam";
        }

        public override void Play(PlayActionArgs args)
        {
            if (!uint.TryParse(Game?.GameId, out var appId))
            {
                Logger.Warn($"El juego '{Game?.Name}' no tiene un SteamID válido para ejecutarse.");
                return;
            }

            SteamFamilyInstallController.LaunchSteamUrl($"steam://run/{appId}");
        }
    }
}

