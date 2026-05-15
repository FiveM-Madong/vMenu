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
        private static string GetPermissionPrefix()
        {
            var prefix = GetConvar("hb_adminlite_permission_prefix", "hb_adminlite.");
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "hb_adminlite.";
            }

            return prefix.EndsWith(".") ? prefix : prefix + ".";
        }

        private static bool HasLegacyAdminAccess(Player source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.Handle))
            {
                return false;
            }

            var aceName = GetConvar("hb_adminlite_admin_ace", "hb_adminlite.admin");
            if (string.IsNullOrWhiteSpace(aceName))
            {
                aceName = "hb_adminlite.admin";
            }

            return IsPlayerAceAllowed(source.Handle, aceName);
        }

        private static bool HasAdminAccess(Player source, HbPermission permission)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.Handle))
            {
                return false;
            }

            if (permission != HbPermission.HBEverything && HasAdminAccess(source, HbPermission.HBEverything))
            {
                return true;
            }

            if (permission == HbPermission.HBEverything && HasLegacyAdminAccess(source))
            {
                return true;
            }

            return IsPlayerAceAllowed(source.Handle, GetAceName(permission))
                || IsPlayerAceAllowed(source.Handle, GetPermissionPrefix() + permission);
        }

        private static string GetAceName(HbPermission permission)
        {
            return GetPermissionPrefix() + (AceNames.TryGetValue(permission, out var aceName) ? aceName : permission.ToString());
        }

        private static bool HasAdminAccess(Player source)
        {
            return AccessPermissions.Any(permission => HasAdminAccess(source, permission));
        }

        private static string BuildPermissionsJson(Player source)
        {
            var permissions = AccessPermissions.ToDictionary(
                permission => permission.ToString(),
                permission => HasAdminAccess(source, permission));

            return JsonConvert.SerializeObject(permissions);
        }

        private void RequestAccess([FromSource] Player source)
        {
            source.TriggerEvent("hb_adminlite:setPermissions", BuildPermissionsJson(source));
            source.TriggerEvent("hb_adminlite:setAccess", HasAdminAccess(source));
        }

    }
}
