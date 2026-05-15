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
        private static string ReadString(IDictionary<string, object> data, string key)
        {
            if (data != null && data.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString();
            }

            return null;
        }

        private static int? ReadInt(IDictionary<string, object> data, string key)
        {
            if (data != null && data.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static bool ReadBool(IDictionary<string, object> data, string key)
        {
            if (data != null && data.TryGetValue(key, out var value) && value != null && bool.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }

            return false;
        }

        private static string FormatCoords(Vector3 position, float heading)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "X {0:0.00} | Y {1:0.00} | Z {2:0.00} | H {3:0.00}",
                position.X,
                position.Y,
                position.Z,
                heading);
        }

        private void TeleportToWaypoint()
        {
            var waypoint = GetFirstBlipInfoId(8);
            if (!DoesBlipExist(waypoint))
            {
                ShowFeed("웨이포인트가 없습니다.");
                return;
            }

            var coords = GetBlipInfoIdCoord(waypoint);
            SetEntityCoordsNoOffset(Game.PlayerPed.Handle, coords.X, coords.Y, coords.Z + 1f, false, false, false);
            ShowFeed("웨이포인트로 이동했습니다.");
        }

        private void TeleportToCoords(Vector3 coords)
        {
            SetEntityCoordsNoOffset(Game.PlayerPed.Handle, coords.X, coords.Y, coords.Z + 1f, false, false, false);
        }

        private void ClearAreaNearPos(Vector3 position)
        {
            ClearAreaOfEverything(position.X, position.Y, position.Z, 100f, false, false, false, false);
        }

        private async Task<bool> SpawnVehicleByName(string modelName)
        {
            var hash = (uint)GetHashKey(modelName.ToLowerInvariant());
            return await SpawnVehicle(hash);
        }

        private void GiveAllWeapons()
        {
            foreach (var weaponHash in GetWeaponHashes())
            {
                GiveWeaponToPed(Game.PlayerPed.Handle, weaponHash, 250, false, false);
            }
        }

        private void RefillAllAmmo()
        {
            foreach (var weaponHash in GetOwnedWeaponHashes())
            {
                var ammo = 9999;
                GetMaxAmmo(Game.PlayerPed.Handle, weaponHash, ref ammo);
                SetPedAmmo(Game.PlayerPed.Handle, weaponHash, ammo);
            }
        }

        private void SetAllAmmo(int amount)
        {
            foreach (var weaponHash in GetOwnedWeaponHashes())
            {
                SetPedAmmo(Game.PlayerPed.Handle, weaponHash, amount);
            }
        }

        private bool GiveWeaponByName(string weaponName)
        {
            var normalized = weaponName.Trim().ToLowerInvariant();
            if (!normalized.StartsWith("weapon_"))
            {
                normalized = $"weapon_{normalized}";
            }

            var hash = (uint)GetHashKey(normalized);
            if (!IsWeaponValid(hash))
            {
                return false;
            }

            GiveWeaponToPed(Game.PlayerPed.Handle, hash, 250, false, true);
            ShowFeed($"무기 {normalized} 을(를) 지급했습니다.");
            return true;
        }

        private static IEnumerable<uint> GetWeaponHashes()
        {
            return Enum.GetValues(typeof(WeaponHash))
                .Cast<WeaponHash>()
                .Select(hash => (uint)hash)
                .Where(hash => hash != 0 && IsWeaponValid(hash))
                .Distinct()
                .ToList();
        }

        private static IEnumerable<uint> GetOwnedWeaponHashes()
        {
            return GetWeaponHashes().Where(hash => HasPedGotWeapon(Game.PlayerPed.Handle, hash, false));
        }

        private async Task<bool> SpawnVehicleByHash(string hashText)
        {
            if (!uint.TryParse(hashText, out var hash))
            {
                return false;
            }

            return await SpawnVehicle(hash);
        }

        private async Task<bool> SpawnVehicle(uint modelHash)
        {
            if (!IsModelInCdimage(modelHash) || !IsModelAVehicle(modelHash))
            {
                return false;
            }

            RequestModel(modelHash);
            var timeout = GetGameTimer() + 5000;
            while (!HasModelLoaded(modelHash))
            {
                await Delay(0);
                if (GetGameTimer() > timeout)
                {
                    return false;
                }
            }

            var ped = Game.PlayerPed;
            var currentVehicle = ped.IsInVehicle() ? ped.CurrentVehicle : null;
            var replacingCurrentVehicle = currentVehicle != null && currentVehicle.Exists();

            var spawnPos = replacingCurrentVehicle
                ? currentVehicle.Position
                : GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, 5f, 0.5f);
            var heading = replacingCurrentVehicle ? currentVehicle.Heading : ped.Heading;
            var velocity = replacingCurrentVehicle ? GetEntityVelocity(currentVehicle.Handle) : Vector3.Zero;

            if (replacingCurrentVehicle)
            {
                var oldVehicleHandle = currentVehicle.Handle;
                SetEntityAsMissionEntity(oldVehicleHandle, true, true);
                DeleteVehicle(ref oldVehicleHandle);
            }

            var vehicleHandle = CreateVehicle(modelHash, spawnPos.X, spawnPos.Y, spawnPos.Z, heading, true, false);
            if (vehicleHandle == 0)
            {
                SetModelAsNoLongerNeeded(modelHash);
                return false;
            }

            SetVehicleOnGroundProperly(vehicleHandle);
            if (replacingCurrentVehicle)
            {
                SetEntityVelocity(vehicleHandle, velocity.X, velocity.Y, velocity.Z);
            }
            SetPedIntoVehicle(ped.Handle, vehicleHandle, -1);
            SetEntityAsNoLongerNeeded(ref vehicleHandle);
            SetModelAsNoLongerNeeded(modelHash);
            ShowFeed("차량을 스폰했습니다.");
            return true;
        }

        private float GetCurrentEntityDisplayRange()
        {
            return entityDisplayRangeStep / 20f * MaxEntityDisplayRange;
        }

        private bool IsAnyEntityDebugEnabled()
        {
            return showVehicleModelDimensions || showPropModelDimensions || showPedModelDimensions;
        }

        private void ApplyTimecycleState()
        {
            if (!timecycleEnabled || TimeCycles.Timecycles.Count == 0)
            {
                ClearTimecycleModifier();
                return;
            }

            var safeIndex = Math.Max(0, Math.Min(TimeCycles.Timecycles.Count - 1, timecycleIndex));
            SetTimecycleModifier(TimeCycles.Timecycles[safeIndex]);
            SetTimecycleModifierStrength(timecycleStrength / 20f);
        }

        private Task OnDevToolsScanTick()
        {
            if (!hasAccess || !HasPermission("HBDevTools"))
            {
                return Delay(accessResolved ? 300000 : 500);
            }

            if (!IsAnyEntityDebugEnabled())
            {
                debugVehicles.Clear();
                debugProps.Clear();
                debugPeds.Clear();
                return Delay(1000);
            }

            var position = Game.PlayerPed.Position;
            var rangeSquared = GetCurrentEntityDisplayRange() * GetCurrentEntityDisplayRange();

            if (showPropModelDimensions)
            {
                stopPropsLoop = true;
                debugProps = World.GetAllProps()
                    .Where(p => p != null && p.Exists() && p.IsOnScreen && p.Position.DistanceToSquared(position) < rangeSquared)
                    .ToList();
                stopPropsLoop = false;

                return Delay(50);
            }
            debugProps.Clear();

            if (showPedModelDimensions)
            {
                stopPedsLoop = true;
                debugPeds = World.GetAllPeds()
                    .Where(p => p != null && p.Exists() && p.IsOnScreen && p.Handle != Game.PlayerPed.Handle && p.Position.DistanceToSquared(position) < rangeSquared)
                    .ToList();
                stopPedsLoop = false;

                return Delay(50);
            }
            debugPeds.Clear();

            if (showVehicleModelDimensions)
            {
                stopVehiclesLoop = true;
                debugVehicles = World.GetAllVehicles()
                    .Where(v => v != null && v.Exists() && v.IsOnScreen && v.Position.DistanceToSquared(position) < rangeSquared)
                    .ToList();
                stopVehiclesLoop = false;

                return Delay(50);
            }

            debugVehicles.Clear();
            return Task.FromResult(0);
        }

        private Task OnDevToolsRenderTick()
        {
            if (!hasAccess || !HasPermission("HBDevTools"))
            {
                return Delay(accessResolved ? 300000 : 500);
            }

            if (!IsAnyEntityDebugEnabled())
            {
                return Delay(250);
            }

            foreach (var vehicle in debugVehicles)
            {
                if (stopVehiclesLoop)
                {
                    break;
                }

                if (vehicle == null || !vehicle.Exists())
                {
                    continue;
                }

                DrawEntityBoundingBox(vehicle.Handle, 250, 150, 0, 100);
                DrawEntityDebugText(vehicle, "Veh");
            }

            foreach (var prop in debugProps)
            {
                if (stopPropsLoop)
                {
                    break;
                }

                if (prop == null || !prop.Exists())
                {
                    continue;
                }

                DrawEntityBoundingBox(prop.Handle, 255, 0, 0, 100);
                DrawEntityDebugText(prop, "Prop");
            }

            foreach (var ped in debugPeds)
            {
                if (stopPedsLoop)
                {
                    break;
                }

                if (ped == null || !ped.Exists())
                {
                    continue;
                }

                DrawEntityBoundingBox(ped.Handle, 50, 255, 50, 100);
                DrawEntityDebugText(ped, "Ped");
            }

            return Task.FromResult(0);
        }

        private void DrawEntityDebugText(Entity entity, string prefix)
        {
            if (!entity.IsOnScreen)
            {
                return;
            }

            var position = entity.Position;
            if (showEntityHandles)
            {
                DrawWorldText($"{prefix} {entity.Handle}", position.X, position.Y, position.Z);
            }

            if (showEntityModels)
            {
                var model = GetEntityModel(entity.Handle);
                DrawWorldText($"Hash {model} / 0x{model:X8}", position.X, position.Y, position.Z - 0.3f);
            }

            if (showEntityNetOwners)
            {
                var ownerLocalId = NetworkGetEntityOwner(entity.Handle);
                if (ownerLocalId != 0)
                {
                    DrawWorldText($"Owner ID {GetPlayerServerId(ownerLocalId)} ({GetPlayerName(ownerLocalId)})", position.X, position.Y, position.Z + 0.3f);
                }
            }
        }

        private static void DrawWorldText(string text, float x, float y, float z)
        {
            SetDrawOrigin(x, y, z, 0);
            SetTextFont(0);
            SetTextScale(1.0f, 0.3f);
            SetTextJustification(0);
            SetTextOutline();
            SetTextCentre(true);
            BeginTextCommandDisplayText("STRING");
            AddTextComponentSubstringPlayerName(text);
            EndTextCommandDisplayText(0f, 0f);
            ClearDrawOrigin();
        }

        private static void DrawEntityBoundingBox(int entityHandle, int r, int g, int b, int a)
        {
            var box = GetEntityBoundingBox(entityHandle);
            foreach (var poly in GetBoundingBoxPolyMatrix(box))
            {
                DrawPoly(poly[0].X, poly[0].Y, poly[0].Z, poly[1].X, poly[1].Y, poly[1].Z, poly[2].X, poly[2].Y, poly[2].Z, r, g, b, a);
            }

            foreach (var line in GetBoundingBoxEdgeMatrix(box))
            {
                DrawLine(line[0].X, line[0].Y, line[0].Z, line[1].X, line[1].Y, line[1].Z, 255, 255, 255, 255);
            }
        }

        private static Vector3[] GetEntityBoundingBox(int entityHandle)
        {
            var min = Vector3.Zero;
            var max = Vector3.Zero;
            GetModelDimensions((uint)GetEntityModel(entityHandle), ref min, ref max);
            const float pad = 0.001f;
            return new[]
            {
                GetOffsetFromEntityInWorldCoords(entityHandle, min.X - pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, max.X + pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, max.X + pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, min.X - pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, min.X - pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, max.X + pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, max.X + pad, max.Y + pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entityHandle, min.X - pad, max.Y + pad, max.Z + pad),
            };
        }

        private static Vector3[][] GetBoundingBoxPolyMatrix(Vector3[] box)
        {
            return new[]
            {
                new[] { box[2], box[1], box[0] },
                new[] { box[3], box[2], box[0] },
                new[] { box[4], box[5], box[6] },
                new[] { box[4], box[6], box[7] },
                new[] { box[2], box[3], box[6] },
                new[] { box[7], box[6], box[3] },
                new[] { box[0], box[1], box[4] },
                new[] { box[5], box[4], box[1] },
                new[] { box[1], box[2], box[5] },
                new[] { box[2], box[6], box[5] },
                new[] { box[4], box[7], box[3] },
                new[] { box[4], box[3], box[0] },
            };
        }

        private static Vector3[][] GetBoundingBoxEdgeMatrix(Vector3[] box)
        {
            return new[]
            {
                new[] { box[0], box[1] },
                new[] { box[1], box[2] },
                new[] { box[2], box[3] },
                new[] { box[3], box[0] },
                new[] { box[4], box[5] },
                new[] { box[5], box[6] },
                new[] { box[6], box[7] },
                new[] { box[7], box[4] },
                new[] { box[0], box[4] },
                new[] { box[1], box[5] },
                new[] { box[2], box[6] },
                new[] { box[3], box[7] },
            };
        }

        internal static void ShowFeed(string message)
        {
            BeginTextCommandThefeedPost("STRING");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandThefeedPostTicker(false, false);
        }

    }
}
