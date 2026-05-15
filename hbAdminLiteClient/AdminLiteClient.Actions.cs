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
        private void ExecuteDevAction(string action)
        {
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
                    ShowFeed("NUI에서 모델명을 입력해 엔티티를 생성하세요.");
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
        }

        private void ExecutePlayerAction(string action)
        {
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
            }
        }

    }
}
