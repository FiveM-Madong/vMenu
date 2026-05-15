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
        private void ClearAreaNearSource([FromSource] Player source)
        {
            if (!HasAdminAccess(source, HbPermission.HBDevTools))
            {
                return;
            }

            var pedHandle = GetPlayerPed(source.Handle);
            if (pedHandle <= 0)
            {
                return;
            }

            var position = GetEntityCoords(pedHandle);
            TriggerClientEvent("hb_adminlite:clearArea", position);
        }

        private void KillPlayer([FromSource] Player source, int targetServerId)
        {
            if (!HasAdminAccess(source, HbPermission.HBOnlinePlayers))
            {
                return;
            }

            var target = Players[targetServerId];
            target?.TriggerEvent("hb_adminlite:killMe", source.Name);
        }

        private void KickPlayer([FromSource] Player source, int targetServerId)
        {
            if (!HasAdminAccess(source, HbPermission.HBOnlinePlayers))
            {
                return;
            }

            var target = Players[targetServerId];
            target?.Drop("HB Admin Lite manager kick.");
        }

        private void SummonPlayer([FromSource] Player source, int targetServerId)
        {
            if (!HasAdminAccess(source, HbPermission.HBOnlinePlayers))
            {
                return;
            }

            var sourcePed = GetPlayerPed(source.Handle);
            if (sourcePed <= 0)
            {
                return;
            }

            var sourceCoords = GetEntityCoords(sourcePed);
            var target = Players[targetServerId];
            target?.TriggerEvent("hb_adminlite:teleportTo", sourceCoords);
        }

        private void TeleportToPlayer([FromSource] Player source, int targetServerId)
        {
            if (!HasAdminAccess(source, HbPermission.HBOnlinePlayers))
            {
                return;
            }

            var target = Players[targetServerId];
            if (target == null)
            {
                return;
            }

            var targetPed = GetPlayerPed(target.Handle);
            if (targetPed <= 0)
            {
                return;
            }

            var targetCoords = GetEntityCoords(targetPed);
            source.TriggerEvent("hb_adminlite:teleportTo", targetCoords);
        }

        private void SetTime([FromSource] Player source, int hour, int minute)
        {
            if (!HasAdminAccess(source, HbPermission.HBEverything))
            {
                return;
            }

            SetConvarReplicated(WorldSyncConvars.CurrentHour, MathUtil.Clamp(hour, 0, 23).ToString());
            SetConvarReplicated(WorldSyncConvars.CurrentMinute, MathUtil.Clamp(minute, 0, 59).ToString());
        }

        private void ToggleFreezeTime([FromSource] Player source)
        {
            if (!HasAdminAccess(source, HbPermission.HBEverything))
            {
                return;
            }

            var current = WorldSyncConvars.GetBool(WorldSyncConvars.FreezeTime);
            SetConvarReplicated(WorldSyncConvars.FreezeTime, (!current).ToString().ToLower());
        }

        private void SetWeather([FromSource] Player source, string weatherType)
        {
            if (!HasAdminAccess(source, HbPermission.HBEverything))
            {
                return;
            }

            var normalized = (weatherType ?? "CLEAR").ToUpperInvariant();
            SetConvarReplicated(WorldSyncConvars.CurrentWeather, normalized);
            SetConvarReplicated(WorldSyncConvars.EnableDynamicWeather, "false");
            SetConvarReplicated(WorldSyncConvars.EnableSnow, (normalized is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD").ToString().ToLower());
        }

        private void ToggleBlackout([FromSource] Player source)
        {
            if (!HasAdminAccess(source, HbPermission.HBEverything))
            {
                return;
            }

            var current = WorldSyncConvars.GetBool(WorldSyncConvars.BlackoutEnabled);
            SetConvarReplicated(WorldSyncConvars.BlackoutEnabled, (!current).ToString().ToLower());
        }

    }
}
