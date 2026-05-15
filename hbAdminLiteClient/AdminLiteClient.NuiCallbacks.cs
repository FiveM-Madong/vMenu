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
        private void OnClose(IDictionary<string, object> data, CallbackDelegate cb)
        {
            CloseMenu();
            cb("ok");
        }

        private void OnReady(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!hasAccess)
            {
                cb(BuildNoAccessUiState());
                return;
            }

            cb(BuildUiStateWithVisibility());
        }

        private void OnState(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!hasAccess)
            {
                cb(BuildNoAccessUiState());
                return;
            }

            cb(BuildUiStateWithVisibility());
        }

        private void OnDevAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            var model = ReadString(data, "model");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission(action == "toggleNoclip" ? "HBNoClip" : "HBDevTools", cb))
            {
                return;
            }

            switch (action)
            {
                case "clearArea":
                    TriggerServerEvent("hb_adminlite:clearArea");
                    ShowFeed("주변 정리를 요청했습니다.");
                    break;

                case "toggleCoords":
                    showCoordinates = !showCoordinates;
                    ShowFeed($"좌표 표시 {(showCoordinates ? "활성화" : "비활성화")}");
                    break;

                case "toggleNoclip":
                    ToggleNoclip();
                    break;

                case "teleportWaypoint":
                    TeleportToWaypoint();
                    break;
                case "toggleVehicleDimensions":
                    showVehicleModelDimensions = !showVehicleModelDimensions;
                    break;
                case "togglePropDimensions":
                    showPropModelDimensions = !showPropModelDimensions;
                    break;
                case "togglePedDimensions":
                    showPedModelDimensions = !showPedModelDimensions;
                    break;
                case "toggleEntityHandles":
                    showEntityHandles = !showEntityHandles;
                    break;
                case "toggleEntityModels":
                    showEntityModels = !showEntityModels;
                    break;
                case "toggleEntityNetOwners":
                    showEntityNetOwners = !showEntityNetOwners;
                    break;
                case "entityRangeDown":
                    entityDisplayRangeStep = Math.Max(0, entityDisplayRangeStep - 1);
                    break;
                case "entityRangeUp":
                    entityDisplayRangeStep = Math.Min(20, entityDisplayRangeStep + 1);
                    break;
                case "timecycleCycle":
                    timecycleIndex = timecycleIndex >= TimeCycles.Timecycles.Count - 1 ? 0 : timecycleIndex + 1;
                    ApplyTimecycleState();
                    break;
                case "toggleTimecycle":
                    timecycleEnabled = !timecycleEnabled;
                    ApplyTimecycleState();
                    break;
                case "timecycleStrengthCycle":
                    timecycleStrength = timecycleStrength >= 20 ? 0 : timecycleStrength + 1;
                    ApplyTimecycleState();
                    break;
                case "spawnEntity":
                    if (DevEntitySpawner.Active || DevEntitySpawner.CurrentEntity != null)
                    {
                        ShowFeed("이미 배치 중인 엔티티가 있습니다.");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(model))
                    {
                        ShowFeed("모델명을 입력하세요.");
                        break;
                    }

                    CloseMenu();
                    DevEntitySpawner.SpawnEntity(model, Game.PlayerPed.Position);
                    break;
                case "confirmEntity":
                    DevEntitySpawner.FinishPlacement(false);
                    break;
                case "duplicateEntity":
                    DevEntitySpawner.FinishPlacement(true);
                    break;
                case "cancelEntity":
                    DevEntitySpawner.CancelPlacement();
                    break;
            }

            cb(BuildUiState());
        }

        private void OnPlayerAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            var model = ReadString(data, "model");
            var name = ReadString(data, "name");
            var key = ReadString(data, "key");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            var permission = action is "saveCurrentPed" or "cycleWalkingStyle" or "cycleClothingGlow" or "spawnPedByName" or "spawnSavedPed" or "deleteSavedPed"
                or "pedComponentNext" or "pedComponentTextureNext" or "pedPropNext" or "pedPropTextureNext"
                or "pedCollectionComponentNext" or "pedCollectionComponentTextureNext" or "pedCollectionPropNext" or "pedCollectionPropTextureNext"
                ? "HBPlayerAppearance"
                : "HBPlayerOptions";
            if (RejectIfMissingPermission(permission, cb))
            {
                return;
            }

            switch (action)
            {
                case "toggleGodMode":
                    playerGodMode = !playerGodMode;
                    SetEntityInvincible(Game.PlayerPed.Handle, playerGodMode || noclipEnabled);
                    break;

                case "toggleInvisible":
                    playerInvisible = !playerInvisible;
                    SetEntityVisible(Game.PlayerPed.Handle, !playerInvisible, false);
                    break;

                case "toggleStamina":
                    playerUnlimitedStamina = !playerUnlimitedStamina;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), playerUnlimitedStamina ? 100 : 0, true);
                    break;

                case "toggleFastRun":
                    playerFastRun = !playerFastRun;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, playerFastRun ? 1.49f : 1f);
                    break;

                case "toggleFastSwim":
                    playerFastSwim = !playerFastSwim;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, playerFastSwim ? 1.49f : 1f);
                    break;

                case "toggleSuperJump":
                    playerSuperJump = !playerSuperJump;
                    ShowFeed($"HB Toggle 슈퍼 점프 {(playerSuperJump ? "ON" : "OFF")}");
                    break;

                case "toggleNoRagdoll":
                    playerNoRagdoll = !playerNoRagdoll;
                    SetPedCanRagdoll(Game.PlayerPed.Handle, !playerNoRagdoll);
                    Game.PlayerPed.CanBeKnockedOffBike = !playerNoRagdoll;
                    ShowFeed($"HB Toggle 레그돌 비활성화 {(playerNoRagdoll ? "ON" : "OFF")}");
                    break;

                case "toggleNeverWanted":
                    playerNeverWanted = !playerNeverWanted;
                    if (playerNeverWanted)
                    {
                        ClearPlayerWantedLevel(Game.Player.Handle);
                        SetPlayerWantedLevelNow(Game.Player.Handle, false);
                        if (GetMaxWantedLevel() > 0)
                        {
                            SetMaxWantedLevel(0);
                        }
                    }
                    else
                    {
                        SetMaxWantedLevel(5);
                    }
                    break;

                case "toggleIgnored":
                    playerIgnored = !playerIgnored;
                    SetEveryoneIgnorePlayer(Game.Player.Handle, playerIgnored);
                    break;

                case "heal":
                    SetEntityHealth(Game.PlayerPed.Handle, GetEntityMaxHealth(Game.PlayerPed.Handle));
                    AddArmourToPed(Game.PlayerPed.Handle, 100);
                    ShowFeed("체력과 방어구를 회복했습니다.");
                    break;

                case "clean":
                    ClearPedBloodDamage(Game.PlayerPed.Handle);
                    ResetPedVisibleDamage(Game.PlayerPed.Handle);
                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ShowFeed("플레이어를 세척했습니다.");
                    break;

                case "dry":
                    ClearPedWetness(Game.PlayerPed.Handle);
                    ShowFeed("플레이어를 건조하게 했습니다.");
                    break;

                case "wet":
                    SetPedWetnessHeight(Game.PlayerPed.Handle, 1f);
                    ShowFeed("플레이어를 젖게 했습니다.");
                    break;

                case "suicide":
                    SetEntityHealth(Game.PlayerPed.Handle, 0);
                    break;
                case "saveCurrentPed":
                    SaveCurrentPed(name);
                    break;
                case "cycleWalkingStyle":
                    playerWalkingStyleIndex = (playerWalkingStyleIndex + 1) % WalkingStyleKeys.Length;
                    SetWalkingStyleLocal(WalkingStyleKeys[playerWalkingStyleIndex]);
                    ShowFeed($"걷기 스타일 {WalkingStyleLabels[playerWalkingStyleIndex]}");
                    break;
                case "cycleClothingGlow":
                    playerClothingGlowIndex = (playerClothingGlowIndex + 1) % ClothingGlowLabels.Length;
                    ShowFeed($"발광 의상 스타일 {ClothingGlowLabels[playerClothingGlowIndex]}");
                    break;
                case "pedComponentNext":
                    CyclePedComponent(ReadInt(data, "component") ?? 0);
                    break;
                case "pedComponentTextureNext":
                    CyclePedComponentTexture(ReadInt(data, "component") ?? 0);
                    break;
                case "pedPropNext":
                    CyclePedProp(ReadInt(data, "prop") ?? 0);
                    break;
                case "pedPropTextureNext":
                    CyclePedPropTexture(ReadInt(data, "prop") ?? 0);
                    break;
                case "pedCollectionComponentNext":
                    CyclePedCollectionComponent(ReadInt(data, "component") ?? 0, ReadString(data, "collection"));
                    break;
                case "pedCollectionComponentTextureNext":
                    CyclePedCollectionComponentTexture(ReadInt(data, "component") ?? 0, ReadString(data, "collection"));
                    break;
                case "pedCollectionPropNext":
                    CyclePedCollectionProp(ReadInt(data, "prop") ?? 0, ReadString(data, "collection"));
                    break;
                case "pedCollectionPropTextureNext":
                    CyclePedCollectionPropTexture(ReadInt(data, "prop") ?? 0, ReadString(data, "collection"));
                    break;
                case "spawnPedByName":
                    SpawnPedByName(model);
                    break;
                case "spawnSavedPed":
                    SpawnSavedPed(key);
                    break;
                case "deleteSavedPed":
                    DeleteSavedPed(key);
                    break;
            }

            cb(BuildUiStateSafe());
        }

        private void OnWeaponAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            var model = ReadString(data, "model");
            var ammoCount = ReadInt(data, "ammoCount");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission("HBWeaponOptions", cb))
            {
                return;
            }

            if (TryHandleExtendedWeaponAction(action))
            {
                cb(BuildUiState());
                return;
            }

            ExecuteWeaponAction(action, model, ammoCount);
            cb(BuildUiState());
        }

        private void OnWeaponCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBWeaponOptions"))
            {
                cb(new
                {
                    addonWeapons = new List<object>()
                });
                return;
            }

            cb(new
            {
                addonWeapons = BuildAddonWeaponCatalog()
            });
        }

        private void OnPedCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBPlayerAppearance"))
            {
                cb(new
                {
                    items = new List<object>(),
                    itemsJson = "[]"
                });
                return;
            }

            try
            {
                var section = "";
                if (data != null && data.TryGetValue("section", out var rawSection) && rawSection != null)
                {
                    section = rawSection.ToString() ?? "";
                }

                object payload;
                switch (section)
                {
                    case "appearanceAddonPeds":
                        payload = new
                        {
                            items = BuildAddonPedCatalog()
                        };
                        break;
                    case "appearanceMainPeds":
                    case "appearanceAnimalPeds":
                    case "appearanceMalePeds":
                    case "appearanceFemalePeds":
                    case "appearanceOtherPeds":
                        payload = new
                        {
                            itemsJson = "[]"
                        };
                        break;
                    default:
                        payload = new
                        {
                            itemsJson = "[]"
                        };
                        break;
                }

                cb(payload);
            }
            catch (Exception)
            {
                cb(new
                {
                    items = new List<object>()
                });
            }
        }

        private void OnPedCatalogRequest(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBPlayerAppearance"))
            {
                cb("denied");
                return;
            }

            var section = "";
            if (data != null && data.TryGetValue("section", out var rawSection) && rawSection != null)
            {
                section = rawSection.ToString() ?? "";
            }

            TriggerServerEvent("hb_adminlite:requestPedCatalog", section);
            cb("ok");
        }

        private void OnReceivePedCatalog(string section, string itemsJson)
        {
            var payload = "{\"type\":\"pedCatalogResult\",\"section\":\""
                + EscapeJson(section ?? "")
                + "\",\"itemsJson\":\""
                + EscapeJson(string.IsNullOrWhiteSpace(itemsJson) ? "[]" : itemsJson)
                + "\"}";
            SendNuiMessage(payload);
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private void OnReceivePedCatalogStart(string section, int totalChunks)
        {
            pendingPedCatalogChunkSection = section ?? "";
            pendingPedCatalogChunkTotal = totalChunks < 1 ? 1 : totalChunks;
            pendingPedCatalogChunks = new Dictionary<int, string>();
        }

        private void OnReceivePedCatalogChunk(string section, int index, string chunk)
        {
            if (!string.Equals(pendingPedCatalogChunkSection ?? "", section ?? "", StringComparison.Ordinal))
            {
                return;
            }

            pendingPedCatalogChunks[index] = chunk ?? "";
        }

        private void OnReceivePedCatalogEnd(string section)
        {
            if (!string.Equals(pendingPedCatalogChunkSection ?? "", section ?? "", StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                var parts = new List<string>();
                for (var index = 0; index < pendingPedCatalogChunkTotal; index++)
                {
                    parts.Add(pendingPedCatalogChunks.TryGetValue(index, out var chunk) ? (chunk ?? "") : "");
                }

                var itemsJson = string.Concat(parts);
                OnReceivePedCatalog(section, itemsJson);
            }
            catch (Exception)
            {
            }
            finally
            {
                pendingPedCatalogChunkSection = "";
                pendingPedCatalogChunkTotal = 0;
                pendingPedCatalogChunks.Clear();
            }
        }

        private void OnReceiveVehicleCatalogStart(int totalChunks)
        {
            pendingVehicleCatalogChunkTotal = totalChunks < 1 ? 1 : totalChunks;
            pendingVehicleCatalogChunks = new Dictionary<int, string>();
        }

        private void OnReceiveVehicleCatalogChunk(int index, string chunk)
        {
            pendingVehicleCatalogChunks[index] = chunk ?? "";
        }

        private void OnReceiveVehicleCatalogEnd()
        {
            try
            {
                var parts = new List<string>();
                for (var index = 0; index < pendingVehicleCatalogChunkTotal; index++)
                {
                    parts.Add(pendingVehicleCatalogChunks.TryGetValue(index, out var chunk) ? (chunk ?? "") : "");
                }

                var catalogJson = string.Concat(parts);
                var payload = "{\"type\":\"vehicleCatalogResult\",\"catalogJson\":\""
                    + EscapeJson(string.IsNullOrWhiteSpace(catalogJson) ? "{\"classes\":[]}" : catalogJson)
                    + "\"}";
                SendNuiMessage(payload);
            }
            catch (Exception)
            {
                SendNuiMessage("{\"type\":\"vehicleCatalogResult\",\"catalogJson\":\"{\\\"classes\\\":[]}\"}");
            }
            finally
            {
                pendingVehicleCatalogChunkTotal = 0;
                pendingVehicleCatalogChunks.Clear();
            }
        }

        private void OnWeaponLoadoutCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBWeaponLoadouts"))
            {
                cb(new
                {
                    loadouts = new List<object>(),
                    defaultLoadout = "",
                    setOnRespawn = false
                });
                return;
            }

            try
            {
                var loadouts = new List<object>();
                var rawIndex = GetResourceKvpString("hb_adminlite_weapon_loadout_index") ?? "";
                var lines = rawIndex.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var separator = line.IndexOf('|');
                    if (separator <= 0 || separator >= line.Length - 1)
                    {
                        continue;
                    }

                    var key = line.Substring(0, separator).Trim();
                    var name = line.Substring(separator + 1).Trim();
                    var rawLoadout = GetResourceKvpString(key) ?? "";
                    var count = 0;
                    var marker = "\"Hash\":";
                    var searchIndex = 0;

                    while (true)
                    {
                        var found = rawLoadout.IndexOf(marker, searchIndex, StringComparison.Ordinal);
                        if (found < 0)
                        {
                            break;
                        }

                        count++;
                        searchIndex = found + marker.Length;
                    }

                    loadouts.Add(new
                    {
                        id = key,
                        name,
                        count
                    });
                }

                cb(new
                {
                    loadouts,
                    defaultLoadout = GetResourceKvpString("hb_adminlite_weapon_default_loadout")
                                     ?? "",
                    setOnRespawn = weaponLoadoutsSetOnRespawn
                });
            }
            catch (Exception)
            {
                cb(new
                {
                    loadouts = new List<object>(),
                    defaultLoadout = "",
                    setOnRespawn = weaponLoadoutsSetOnRespawn
                });
            }
        }

        private void OnWeaponLoadoutAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            var name = ReadString(data, "name");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission("HBWeaponLoadouts", cb))
            {
                return;
            }

            ExecuteWeaponLoadoutAction(action, name);
            cb(BuildUiState());
        }

        private void OnSavedPedCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBPlayerAppearance"))
            {
                cb(new { peds = new List<object>() });
                return;
            }

            try
            {
                var peds = new List<object>();
                var index = GetSavedPedIndex();
                for (var i = 0; i < index.Count; i++)
                {
                    var entry = index[i];
                    var raw = GetResourceKvpString(entry.Key);
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    peds.Add(new
                    {
                        key = entry.Key,
                        name = entry.Name,
                        model = raw
                    });
                }

                cb(new { peds });
            }
            catch (Exception ex)
            {
                ShowFeed($"PED 카탈로그 오류: {ex.Message}");
                cb(new { peds = new List<object>() });
            }
        }

        private void OnPedCollectionCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBPlayerAppearance"))
            {
                cb(new { collections = new List<object>() });
                return;
            }

            try
            {
                var pedHandle = Game.PlayerPed.Handle;
                var count = GetPedCollectionsCount(pedHandle);
                var collections = new List<object>();

                for (var index = count - 1; index >= 0; index--)
                {
                    var collection = index == 0 ? "" : GetPedCollectionName(pedHandle, index);
                    collections.Add(new
                    {
                        id = string.IsNullOrWhiteSpace(collection) ? "base" : collection,
                        name = string.IsNullOrWhiteSpace(collection) ? "기본 컬렉션" : collection,
                        collection,
                        index
                    });
                }

                cb(new { collections });
            }
            catch
            {
                cb(new { collections = new List<object>() });
            }
        }

        private void OnInputMode(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var active = ReadBool(data, "active");
            if (menuOpen)
            {
                if (active)
                {
                    SetNuiFocus(true, true);
                    SetNuiFocusKeepInput(false);
                }
                else
                {
                    SetNuiFocus(true, false);
                    SetNuiFocusKeepInput(true);
                }
            }

            cb("ok");
        }

        private void OnVehicleAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission("HBVehicleOptions", cb))
            {
                return;
            }

            var vehicle = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle : null;
            if (vehicle == null || !vehicle.Exists())
            {
                ShowFeed("차량에 탑승 중이 아닙니다.");
                cb(BuildUiState());
                return;
            }

            switch (action)
            {
                case "repair":
                    SetVehicleFixed(vehicle.Handle);
                    SetVehicleDirtLevel(vehicle.Handle, 0f);
                    ShowFeed("차량을 수리했습니다.");
                    break;

                case "clean":
                    SetVehicleDirtLevel(vehicle.Handle, 0f);
                    ShowFeed("차량을 세차했습니다.");
                    break;

                case "toggleKeepClean":
                    vehicleKeepClean = !vehicleKeepClean;
                    break;

                case "toggleEngineAlwaysOn":
                    vehicleEngineAlwaysOn = !vehicleEngineAlwaysOn;
                    if (vehicleEngineAlwaysOn)
                    {
                        SetVehicleEngineState(vehicle, true);
                    }
                    break;

                case "toggleInfiniteFuel":
                    vehicleInfiniteFuel = !vehicleInfiniteFuel;
                    break;

                case "toggleFreeze":
                    vehicleFrozen = !vehicleFrozen;
                    break;

                case "flip":
                {
                    var pos = vehicle.Position;
                    SetEntityRotation(vehicle.Handle, 0f, 0f, vehicle.Rotation.Z, 2, true);
                    SetEntityCoordsNoOffset(vehicle.Handle, pos.X, pos.Y, pos.Z + 0.5f, false, false, false);
                    ShowFeed("차량 자세를 바로잡았습니다.");
                    break;
                }

                case "delete":
                {
                    var vehicleHandle = vehicle.Handle;
                    SetEntityAsMissionEntity(vehicleHandle, true, true);
                    DeleteVehicle(ref vehicleHandle);
                    ShowFeed("차량을 삭제했습니다.");
                    break;
                }

                case "toggleEngine":
                {
                    var enable = !GetIsVehicleEngineRunning(vehicle.Handle);
                    SetVehicleEngineState(vehicle, enable);
                    ShowFeed($"엔진 {(enable ? "켜짐" : "꺼짐")}");
                    break;
                }
            }

            cb(BuildUiState());
        }

        private async void OnSpawnerAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var modelName = ReadString(data, "model");
            if (!menuOpen || string.IsNullOrWhiteSpace(modelName))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission("HBVehicleSpawner", cb))
            {
                return;
            }

            var spawned = await SpawnVehicleByName(modelName);
            if (!spawned)
            {
                ShowFeed("차량 스폰에 실패했습니다. 모델명을 확인하세요.");
            }

            cb(BuildUiStateSafe());
        }

        private void OnSpawnerCatalog(IDictionary<string, object> data, CallbackDelegate cb)
        {
            if (!HasPermission("HBVehicleSpawner"))
            {
                cb(new
                {
                    classes = new List<object>()
                });
                return;
            }

            TriggerServerEvent("hb_adminlite:requestVehicleCatalog");
            cb(new
            {
                classes = new List<object>()
            });
        }

        private async void OnPersonalVehicleAction(IDictionary<string, object> data, CallbackDelegate cb)
        {
            var action = ReadString(data, "action");
            if (!menuOpen || string.IsNullOrWhiteSpace(action))
            {
                cb("invalid");
                return;
            }

            if (RejectIfMissingPermission("HBVehicleOptions", cb))
            {
                return;
            }

            switch (action)
            {
                case "saveCurrent":
                    if (!Game.PlayerPed.IsInVehicle() || Game.PlayerPed.CurrentVehicle == null || !Game.PlayerPed.CurrentVehicle.Exists())
                    {
                        ShowFeed("현재 탑승 중인 차량이 없습니다.");
                        break;
                    }

                    personalVehicleModel = Game.PlayerPed.CurrentVehicle.Model.Hash.ToString();
                    SetResourceKvp("hb_adminlite_personal_vehicle", personalVehicleModel);
                    ShowFeed("현재 차량을 개인 차량으로 저장했습니다.");
                    break;

                case "spawnSaved":
                    if (string.IsNullOrWhiteSpace(personalVehicleModel))
                    {
                        ShowFeed("저장된 개인 차량이 없습니다.");
                        break;
                    }

                    if (!await SpawnVehicleByHash(personalVehicleModel))
                    {
                        ShowFeed("개인 차량 스폰에 실패했습니다.");
                    }
                    break;

                case "clearSaved":
                    personalVehicleModel = "";
                    DeleteResourceKvp("hb_adminlite_personal_vehicle");
                    ShowFeed("개인 차량 저장을 삭제했습니다.");
                    break;
            }

            cb(BuildUiState());
        }


    }
}
