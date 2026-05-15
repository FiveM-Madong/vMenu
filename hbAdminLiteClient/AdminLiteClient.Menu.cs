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
        private void ToggleMenu()
        {
            if (menuOpen)
            {
                CloseMenu();
                return;
            }

            menuOpen = true;
            currentCategory = "root";
            selectedIndex = 0;
            SetNuiFocus(true, false);
            SetNuiFocusKeepInput(true);
        }

        private void CloseMenu()
        {
            menuOpen = false;
            currentCategory = "root";
            selectedIndex = 0;
            SetNuiFocusKeepInput(false);
            SetNuiFocus(false, false);
        }

        private object BuildUiPermissions()
        {
            return new
            {
                HBEverything = HasPermission("HBEverything"),
                HBNoClip = HasPermission("HBNoClip"),
                HBDevTools = HasPermission("HBDevTools"),
                HBPlayerOptions = HasPermission("HBPlayerOptions"),
                HBVehicleOptions = HasPermission("HBVehicleOptions"),
                HBVehicleSpawner = HasPermission("HBVehicleSpawner"),
                HBWeaponOptions = HasPermission("HBWeaponOptions"),
                HBWeaponLoadouts = HasPermission("HBWeaponLoadouts"),
                HBPlayerAppearance = HasPermission("HBPlayerAppearance"),
                HBOnlinePlayers = HasPermission("HBOnlinePlayers"),
            };
        }

        private object BuildUiState()
        {
            var ped = Game.PlayerPed;
            var vehicle = ped != null && ped.Exists() && ped.IsInVehicle() ? ped.CurrentVehicle : null;
            var inVehicle = vehicle != null && vehicle.Exists();
            var pedPosition = ped != null && ped.Exists() ? ped.Position : Vector3.Zero;
            var pedHeading = ped != null && ped.Exists() ? ped.Heading : 0f;

            return new
            {
                noclipEnabled,
                coords = FormatCoords(pedPosition, pedHeading),
                showCoordinates,
                showVehicleModelDimensions,
                showPropModelDimensions,
                showPedModelDimensions,
                showEntityHandles,
                showEntityModels,
                showEntityNetOwners,
                entityDisplayRangeStep,
                entityDisplayRange = (int)GetCurrentEntityDisplayRange(),
                entitySpawnerActive = DevEntitySpawner.Active,
                entitySpawnerModel = DevEntitySpawner.CurrentModelName,
                timecycleEnabled,
                timecycleIndex,
                timecycleStrength,
                timecycleName = TimeCycles.Timecycles[Math.Max(0, Math.Min(TimeCycles.Timecycles.Count - 1, timecycleIndex))],
                permissions = BuildUiPermissions(),
                menuCategory = currentCategory,
                selectedAction = GetSelectedMenuId(),
                playerGodMode,
                playerInvisible,
                playerUnlimitedStamina,
                playerFastRun,
                playerFastSwim,
                playerSuperJump,
                playerNoRagdoll,
                playerNeverWanted,
                playerIgnored,
                weaponUnlimitedAmmo,
                weaponNoReload,
                weaponAutoEquipParachutes,
                weaponUnlimitedParachutes,
                parachuteSmokeColorIndex,
                parachutePrimaryStyleIndex,
                parachuteReserveStyleIndex,
                weaponLoadoutsSetOnRespawn,
                playerWalkingStyleIndex,
                playerWalkingStyleName = WalkingStyleLabels[Math.Max(0, Math.Min(WalkingStyleLabels.Length - 1, playerWalkingStyleIndex))],
                playerClothingGlowIndex,
                playerClothingGlowName = ClothingGlowLabels[Math.Max(0, Math.Min(ClothingGlowLabels.Length - 1, playerClothingGlowIndex))],
                pedComponents = SafeBuildPedComponentSummary(),
                pedProps = SafeBuildPedPropSummary(),
                inVehicle,
                vehicleKeepClean,
                vehicleEngineAlwaysOn,
                vehicleInfiniteFuel,
                vehicleFrozen,
                vehicleEngineOn = inVehicle && GetIsVehicleEngineRunning(vehicle.Handle),
                personalVehicleModel,
                hasAccess,
            };
        }

        private object BuildUiStateSafe()
        {
            try
            {
                return BuildUiState();
            }
            catch (Exception ex)
            {
                ShowFeed($"메뉴 상태 오류: {ex.Message}");
                return new
                {
                    noclipEnabled,
                    coords = "",
                    showCoordinates,
                    showVehicleModelDimensions,
                    showPropModelDimensions,
                    showPedModelDimensions,
                    showEntityHandles,
                    showEntityModels,
                    showEntityNetOwners,
                    entityDisplayRangeStep,
                    entityDisplayRange = (int)GetCurrentEntityDisplayRange(),
                    entitySpawnerActive = DevEntitySpawner.Active,
                    entitySpawnerModel = DevEntitySpawner.CurrentModelName,
                    timecycleEnabled,
                    timecycleIndex,
                    timecycleStrength,
                    timecycleName = "",
                    permissions = BuildUiPermissions(),
                    menuCategory = currentCategory,
                    selectedAction = GetSelectedMenuId(),
                    playerGodMode,
                    playerInvisible,
                    playerUnlimitedStamina,
                    playerFastRun,
                    playerFastSwim,
                    playerSuperJump,
                    playerNoRagdoll,
                    playerNeverWanted,
                    playerIgnored,
                    weaponUnlimitedAmmo,
                    weaponNoReload,
                    weaponAutoEquipParachutes,
                    weaponUnlimitedParachutes,
                    parachuteSmokeColorIndex,
                    parachutePrimaryStyleIndex,
                    parachuteReserveStyleIndex,
                    weaponLoadoutsSetOnRespawn,
                    playerWalkingStyleIndex,
                    playerWalkingStyleName = "",
                    playerClothingGlowIndex,
                    playerClothingGlowName = "",
                    pedComponents = new List<object>(),
                    pedProps = new List<object>(),
                    inVehicle = false,
                    vehicleKeepClean,
                    vehicleEngineAlwaysOn,
                    vehicleInfiniteFuel,
                    vehicleFrozen,
                    vehicleEngineOn = false,
                    personalVehicleModel,
                    hasAccess,
                };
            }
        }

        private void HandleMenuNavigationTick()
        {
            if (Game.IsDisabledControlJustPressed(0, Control.FrontendUp))
            {
                MoveSelectionVertical(-1);
                return;
            }

            if (Game.IsDisabledControlJustPressed(0, Control.FrontendDown))
            {
                MoveSelectionVertical(1);
                return;
            }

            if (Game.IsDisabledControlJustPressed(0, Control.FrontendLeft))
            {
                MoveSelectionHorizontal(-1);
                return;
            }

            if (Game.IsDisabledControlJustPressed(0, Control.FrontendRight))
            {
                MoveSelectionHorizontal(1);
                return;
            }

            if (Game.IsControlJustPressed(0, Control.FrontendCancel))
            {
                CloseMenu();
            }
        }

        private void MoveSelectionVertical(int deltaRows)
        {
            var items = GetMenuItemIds();
            if (items.Count == 0)
            {
                selectedIndex = 0;
                return;
            }

            var columnCount = GetMenuColumnCount();
            if (columnCount <= 1)
            {
                selectedIndex += deltaRows;
                if (selectedIndex < 0)
                {
                    selectedIndex = items.Count - 1;
                }
                else if (selectedIndex >= items.Count)
                {
                    selectedIndex = 0;
                }

                return;
            }

            var nextIndex = selectedIndex + (deltaRows * columnCount);
            if (nextIndex < 0)
            {
                nextIndex = selectedIndex % columnCount;
            }
            else if (nextIndex >= items.Count)
            {
                var currentColumn = selectedIndex % columnCount;
                var lastRowStart = ((items.Count - 1) / columnCount) * columnCount;
                nextIndex = Math.Min(lastRowStart + currentColumn, items.Count - 1);
            }

            selectedIndex = nextIndex;
        }

        private void MoveSelectionHorizontal(int deltaColumns)
        {
            var items = GetMenuItemIds();
            if (items.Count == 0)
            {
                selectedIndex = 0;
                return;
            }

            var columnCount = GetMenuColumnCount();
            if (columnCount <= 1)
            {
                return;
            }

            var rowStart = (selectedIndex / columnCount) * columnCount;
            var rowEnd = Math.Min(rowStart + columnCount - 1, items.Count - 1);
            var nextIndex = selectedIndex + deltaColumns;

            if (nextIndex < rowStart || nextIndex > rowEnd)
            {
                return;
            }

            selectedIndex = nextIndex;
        }

        private void ActivateSelectedItem()
        {
            var selected = GetSelectedMenuId();
            if (string.IsNullOrWhiteSpace(selected))
            {
                return;
            }

            if (currentCategory == "root")
            {
                currentCategory = selected;
                selectedIndex = 0;
                return;
            }

            switch (currentCategory)
            {
                case "dev":
                    ExecuteDevAction(selected);
                    break;
                case "player":
                    ExecutePlayerAction(selected);
                    break;
                case "vehicle":
                    ExecuteVehicleAction(selected);
                    break;
                case "spawner":
                    ExecuteSpawnerAction(selected);
                    break;
            }

        }

        private void NavigateBack()
        {
            if (currentCategory != "root")
            {
                currentCategory = "root";
                selectedIndex = 0;
                return;
            }

            CloseMenu();
        }

        private List<string> GetMenuItemIds()
        {
            switch (currentCategory)
            {
                case "dev":
                    return new List<string>
                    {
                        "clearArea", "toggleCoords", "toggleNoclip", "teleportWaypoint",
                        "toggleVehicleDimensions", "togglePropDimensions", "togglePedDimensions",
                        "toggleEntityHandles", "toggleEntityModels", "toggleEntityNetOwners",
                        "entityRangeDown", "entityRangeUp",
                        "timecycleCycle", "toggleTimecycle", "timecycleStrengthCycle",
                        "spawnEntity", "confirmEntity", "duplicateEntity", "cancelEntity"
                    };
                case "player":
                    return new List<string>
                    {
                        "toggleGodMode", "toggleInvisible", "toggleStamina", "toggleFastRun", "toggleFastSwim",
                        "toggleSuperJump", "toggleNoRagdoll", "toggleNeverWanted", "toggleIgnored", "heal",
                        "clean", "dry", "wet", "suicide"
                    };
                case "vehicle":
                    return new List<string>
                    {
                        "repair", "clean", "toggleKeepClean", "toggleEngineAlwaysOn", "toggleInfiniteFuel",
                        "toggleFreeze", "toggleEngine", "flip", "delete",
                        "saveCurrent", "spawnSaved", "clearSaved"
                    };
                case "spawner":
                    return new List<string> { "spawnByModel" };
                default:
                    return new List<string> { "dev", "player", "vehicle", "spawner" };
            }
        }

        private int GetMenuColumnCount()
        {
            return currentCategory == "root" ? 1 : 2;
        }

        private string GetSelectedMenuId()
        {
            var items = GetMenuItemIds();
            if (items.Count == 0)
            {
                return string.Empty;
            }

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }
            else if (selectedIndex >= items.Count)
            {
                selectedIndex = items.Count - 1;
            }

            return items[selectedIndex];
        }


    }
}
