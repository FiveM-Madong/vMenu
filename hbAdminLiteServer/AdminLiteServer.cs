using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
namespace hbAdminLiteServer
{
    public partial class AdminLiteServer : BaseScript
    {
        private enum HbPermission
        {
            HBEverything,
            HBNoClip,
            HBDevTools,
            HBPlayerOptions,
            HBVehicleOptions,
            HBVehicleSpawner,
            HBWeaponOptions,
            HBWeaponLoadouts,
            HBPlayerAppearance,
            HBOnlinePlayers,
        }

        private static readonly HbPermission[] AccessPermissions =
        {
            HbPermission.HBEverything,
            HbPermission.HBNoClip,
            HbPermission.HBDevTools,
            HbPermission.HBPlayerOptions,
            HbPermission.HBVehicleOptions,
            HbPermission.HBVehicleSpawner,
            HbPermission.HBWeaponOptions,
            HbPermission.HBWeaponLoadouts,
            HbPermission.HBPlayerAppearance,
            HbPermission.HBOnlinePlayers,
        };

        private static readonly Dictionary<HbPermission, string> AceNames = new()
        {
            [HbPermission.HBEverything] = "Everything",
            [HbPermission.HBNoClip] = "NoClip",
            [HbPermission.HBDevTools] = "DevTools",
            [HbPermission.HBPlayerOptions] = "PlayerOptions",
            [HbPermission.HBVehicleOptions] = "VehicleOptions",
            [HbPermission.HBVehicleSpawner] = "VehicleSpawner",
            [HbPermission.HBWeaponOptions] = "WeaponOptions",
            [HbPermission.HBWeaponLoadouts] = "WeaponLoadouts",
            [HbPermission.HBPlayerAppearance] = "PlayerAppearance",
            [HbPermission.HBOnlinePlayers] = "OnlinePlayers",
        };

        private static class WorldSyncConvars
        {
            public const string CurrentHour = "hb_worldsync_current_hour";
            public const string CurrentMinute = "hb_worldsync_current_minute";
            public const string FreezeTime = "hb_worldsync_freeze_time";
            public const string CurrentWeather = "hb_worldsync_current_weather";
            public const string EnableDynamicWeather = "hb_worldsync_enable_dynamic_weather";
            public const string EnableSnow = "hb_worldsync_enable_snow";
            public const string BlackoutEnabled = "hb_worldsync_blackout_enabled";

            public static bool GetBool(string convarName)
            {
                return (GetConvar(convarName, "false") ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class PlayerSummary
        {
            public int ServerId { get; set; }
            public string Name { get; set; }
        }

        public AdminLiteServer()
        {
            EventHandlers.Add("hb_adminlite:requestAccess", new Action<Player>(RequestAccess));
            EventHandlers.Add("hb_adminlite:requestPlayerList", new Action<Player>(RequestPlayerList));
            EventHandlers.Add("hb_adminlite:requestPedCatalog", new Func<Player, string, Task>(RequestPedCatalog));
            EventHandlers.Add("hb_adminlite:requestVehicleCatalog", new Func<Player, Task>(RequestVehicleCatalog));
            EventHandlers.Add("hb_adminlite:clearArea", new Action<Player>(ClearAreaNearSource));
            EventHandlers.Add("hb_adminlite:killPlayer", new Action<Player, int>(KillPlayer));
            EventHandlers.Add("hb_adminlite:kickPlayer", new Action<Player, int>(KickPlayer));
            EventHandlers.Add("hb_adminlite:summonPlayer", new Action<Player, int>(SummonPlayer));
            EventHandlers.Add("hb_adminlite:teleportToPlayer", new Action<Player, int>(TeleportToPlayer));
            EventHandlers.Add("hb_adminlite:setTime", new Action<Player, int, int>(SetTime));
            EventHandlers.Add("hb_adminlite:toggleFreezeTime", new Action<Player>(ToggleFreezeTime));
            EventHandlers.Add("hb_adminlite:setWeather", new Action<Player, string>(SetWeather));
            EventHandlers.Add("hb_adminlite:toggleBlackout", new Action<Player>(ToggleBlackout));
        }



    }
}
