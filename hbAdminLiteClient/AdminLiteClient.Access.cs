using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using hbAdminLiteClient.data;

namespace hbAdminLiteClient
{
    public partial class AdminLiteClient : BaseScript
    {
        private void RequestAccess()
        {
            TriggerServerEvent("hb_adminlite:requestAccess");
        }

        private void OnPlayerSpawned(dynamic _)
        {
            RequestAccess();
            if (weaponLoadoutsSetOnRespawn)
            {
                RestoreDefaultWeaponLoadout();
            }
        }

        private void SetAccess(bool allowed)
        {
            accessResolved = true;
            hasAccess = allowed;

            if (allowed)
            {
                RegisterAdminTicks();
            }
            else
            {
                UnregisterAdminTicks();
            }

            if (!allowed && menuOpen)
            {
                CloseMenu();
            }
        }

        private void RegisterAdminTicks()
        {
            if (adminTicksRegistered)
            {
                return;
            }

            Tick += OnTick;
            Tick += OnSpecialPlayerStatesTick;
            Tick += OnSharedPlayerChecksTick;
            Tick += OnWeaponStatesTick;
            Tick += OnDevToolsRenderTick;
            Tick += OnDevToolsScanTick;
            adminTicksRegistered = true;
        }

        private void UnregisterAdminTicks()
        {
            if (!adminTicksRegistered)
            {
                return;
            }

            Tick -= OnTick;
            Tick -= OnSpecialPlayerStatesTick;
            Tick -= OnSharedPlayerChecksTick;
            Tick -= OnWeaponStatesTick;
            Tick -= OnDevToolsRenderTick;
            Tick -= OnDevToolsScanTick;
            adminTicksRegistered = false;
        }

        private void KillMe(string sourceName)
        {
            SetEntityHealth(Game.PlayerPed.Handle, 0);
            if (!string.IsNullOrWhiteSpace(sourceName))
            {
                ShowFeed($"{sourceName} 님이 플레이어를 처치했습니다.");
            }
        }

        private void SetPermissions(string permissionsJson)
        {
            grantedPermissions.Clear();
            permissionsResolved = true;

            if (string.IsNullOrWhiteSpace(permissionsJson))
            {
                return;
            }

            try
            {
                var permissions = JsonConvert.DeserializeObject<Dictionary<string, bool>>(permissionsJson);
                if (permissions == null)
                {
                    return;
                }

                foreach (var permission in permissions)
                {
                    if (permission.Value && !string.IsNullOrWhiteSpace(permission.Key))
                    {
                        grantedPermissions.Add(permission.Key);
                    }
                }
            }
            catch
            {
            }
        }

        private bool HasPermission(string permission)
        {
            if (!hasAccess)
            {
                return false;
            }

            if (!permissionsResolved)
            {
                return true;
            }

            return grantedPermissions.Contains("HBEverything")
                || grantedPermissions.Contains(permission);
        }

        private bool RejectIfMissingPermission(string permission, CallbackDelegate cb)
        {
            if (HasPermission(permission))
            {
                return false;
            }

            ShowFeed("이 기능에 대한 권한이 없습니다.");
            cb(BuildUiState());
            return true;
        }

    }
}
