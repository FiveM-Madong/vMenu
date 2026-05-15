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
        private async Task OnTick()
        {
            if (!hasAccess)
            {
                await Delay(accessResolved ? 300000 : 500);
                return;
            }

            if (HasPermission("HBPlayerOptions"))
            {
                ApplyPlayerState();
            }

            if (HasPermission("HBVehicleOptions"))
            {
                ApplyVehicleState();
            }
            if (menuOpen)
            {
                DisableControlAction(0, (int)Control.FrontendUp, true);
                DisableControlAction(0, (int)Control.FrontendDown, true);
                DisableControlAction(0, (int)Control.FrontendLeft, true);
                DisableControlAction(0, (int)Control.FrontendRight, true);
                DisableControlAction(0, (int)Control.FrontendPause, true);
                DisableControlAction(0, (int)Control.FrontendPauseAlternate, true);
                DisableControlAction(0, (int)Control.Phone, true);

                await Delay(0);
                return;
            }

            if (RequiresRealtimeUpdates())
            {
                await Delay(250);
                return;
            }

            await Delay(250);
        }

        private void ApplyPlayerState()
        {
            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists())
            {
                return;
            }

            if (!noclipEnabled)
            {
                SetEntityInvincible(ped.Handle, playerGodMode);
                SetEntityVisible(ped.Handle, !playerInvisible, false);
                SetEveryoneIgnorePlayer(Game.Player.Handle, playerIgnored);
            }
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, playerFastRun ? 1.49f : 1f);
            SetSwimMultiplierForPlayer(Game.Player.Handle, playerFastSwim ? 1.49f : 1f);

            if (playerUnlimitedStamina)
            {
                RestorePlayerStamina(Game.Player.Handle, 1f);
            }

            if (playerNeverWanted)
            {
                ClearPlayerWantedLevel(Game.Player.Handle);
                SetPlayerWantedLevelNow(Game.Player.Handle, false);
            }
        }

        private async Task OnSpecialPlayerStatesTick()
        {
            if (!hasAccess)
            {
                await Delay(accessResolved ? 300000 : 500);
                return;
            }

            if (!HasPermission("HBPlayerOptions") || (!playerSuperJump && !playerNoRagdoll))
            {
                await Delay(250);
                return;
            }

            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists())
            {
                await Delay(1000);
                return;
            }

            if (playerSuperJump)
            {
                SetSuperJumpThisFrame(Game.Player.Handle);
                SetSuperJumpThisFrame(PlayerId());
            }

            SetPedCanRagdoll(ped.Handle, !playerNoRagdoll);
            await Delay(playerSuperJump ? 0 : 1000);
        }

        private async Task OnSharedPlayerChecksTick()
        {
            if (!hasAccess || !HasPermission("HBPlayerOptions"))
            {
                await Delay(accessResolved ? 300000 : 500);
                return;
            }

            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists())
            {
                await Delay(1000);
                return;
            }

            var cantBeKnockedOff = playerGodMode || playerNoRagdoll;
            var cantBeDraggedOut = playerGodMode || playerIgnored;
            var cantBeShotInVehicle = playerGodMode;

            ped.CanBeKnockedOffBike = !cantBeKnockedOff;
            ped.CanBeDraggedOutOfVehicle = !cantBeDraggedOut;
            ped.CanBeShotInVehicle = !cantBeShotInVehicle;
            await Delay(1000);
        }

        private async Task OnWeaponStatesTick()
        {
            if (!hasAccess || !HasPermission("HBWeaponOptions"))
            {
                await Delay(accessResolved ? 300000 : 500);
                return;
            }

            if (!weaponUnlimitedAmmo && !weaponNoReload && !weaponUnlimitedParachutes && !weaponAutoEquipParachutes)
            {
                await Delay(500);
                return;
            }

            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists() || ped.Weapons == null || ped.Weapons.Current == null)
            {
                await Delay(250);
                return;
            }

            var current = ped.Weapons.Current;
            if (current.Hash != WeaponHash.Unarmed)
            {
                current.InfiniteAmmo = weaponUnlimitedAmmo;

                if (weaponNoReload && current.Hash != WeaponHash.Minigun)
                {
                    SetAmmoInClip(ped.Handle, (uint)current.Hash, 5);
                }
            }

            if (weaponUnlimitedParachutes)
            {
                EnsurePrimaryParachute();
                SetPlayerHasReserveParachute(Game.Player.Handle);
            }

            if (weaponAutoEquipParachutes)
            {
                var vehicle = ped.IsInVehicle() ? ped.CurrentVehicle : null;
                if (vehicle != null && vehicle.Exists() && (vehicle.Model.IsPlane || vehicle.Model.IsHelicopter))
                {
                    EnsurePrimaryParachute();
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                }
            }

            await Delay(weaponNoReload ? 50 : 500);
        }

        private bool RequiresRealtimeUpdates()
        {
            return playerGodMode
                || playerInvisible
                || playerUnlimitedStamina
                || playerFastRun
                || playerFastSwim
                || playerNeverWanted
                || playerIgnored
                || vehicleKeepClean
                || vehicleEngineAlwaysOn
                || vehicleInfiniteFuel
                || vehicleFrozen;
        }

    }
}
