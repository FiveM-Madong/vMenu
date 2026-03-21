using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;

        public int PlayerBlood { get; private set; } = 0;

        private readonly Menu CustomDrivingStyleMenu = new("주행 스타일", "사용자 지정 주행 스타일");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "플레이어 옵션");

            // Create all checkboxes.
            var playerGodModeCheckbox = new MenuCheckboxItem("무적 모드", "플레이어를 무적으로 만듭니다.", PlayerGodMode);
            var invisibleCheckbox = new MenuCheckboxItem("투명", "자신과 다른 사람에게 보이지 않게 됩니다.", PlayerInvisible);
            var unlimitedStaminaCheckbox = new MenuCheckboxItem("무제한 스태미나", "지치거나 데미지를 받지 않고 계속 달릴 수 있습니다.", PlayerStamina);
            var fastRunCheckbox = new MenuCheckboxItem("빠른 달리기", "~g~달팽이~s~ 파워를 얻어 매우 빠르게 달립니다!", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f);
            var fastSwimCheckbox = new MenuCheckboxItem("빠른 수영", "~g~달팽이 2.0~s~ 파워를 얻어 매우 빠르게 수영합니다!", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f);
            var superJumpCheckbox = new MenuCheckboxItem("슈퍼 점프", "~g~달팽이 3.0~s~ 파워를 얻어 엄청 높이 점프합니다!", PlayerSuperJump);
            var noRagdollCheckbox = new MenuCheckboxItem("래그돌 비활성화", "플레이어 래그돌을 비활성화하여 자전거에서 넘어지지 않게 합니다.", PlayerNoRagdoll);
            var neverWantedCheckbox = new MenuCheckboxItem("수배 해제", "모든 수배 레벨을 비활성화합니다.", PlayerNeverWanted);
            var everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("모두가 플레이어 무시", "모든 NPC가 당신을 신경 쓰지 않습니다.", PlayerIsIgnored);
            var playerStayInVehicleCheckbox = new MenuCheckboxItem("차량 탑승 유지", "이 기능이 켜져 있으면 NPC가 화가 나더라도 차량에서 끌어내리지 못합니다.", PlayerStayInVehicle);
            var playerFrozenCheckbox = new MenuCheckboxItem("플레이어 정지", "현재 위치를 고정합니다.", PlayerFrozen);

            // Wanted level options
            var wantedLevelList = new List<string> { "수배 없음", "1", "2", "3", "4", "5" };
            var setWantedLevel = new MenuListItem("수배 레벨 설정", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "값을 선택한 뒤 엔터를 눌러 수배 레벨을 설정합니다.");
            var setArmorItem = new MenuListItem("방탄복 종류 설정", new List<string> { "방탄복 없음", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "플레이어의 방탄복 레벨/종류를 설정합니다.");

            // Blood level options
            var clearBloodBtn = new MenuItem("피 제거", "플레이어에 묻은 피를 제거합니다.");
            var bloodList = new List<string> { "BigHitByVehicle", "SCR_Torture", "SCR_TrevorTreeBang", "HOSPITAL_0", "HOSPITAL_1", "HOSPITAL_2", "HOSPITAL_3", "HOSPITAL_4", "HOSPITAL_5", "HOSPITAL_6", "HOSPITAL_7", "HOSPITAL_8", "HOSPITAL_9", "Explosion_Med", "Skin_Melee_0", "Explosion_Large", "Car_Crash_Light", "Car_Crash_Heavy", "Fall_Low", "Fall", "HitByVehicle", "BigRunOverByVehicle", "RunOverByVehicle", "TD_KNIFE_FRONT", "TD_KNIFE_FRONT_VA", "TD_KNIFE_FRONT_VB", "TD_KNIFE_REAR", "TD_KNIFE_REAR_VA", "TD_KNIFE_REAR_VB", "TD_KNIFE_STEALTH", "TD_MELEE_FRONT", "TD_MELEE_REAR", "TD_MELEE_STEALTH", "TD_MELEE_BATWAIST", "TD_melee_face_l", "MTD_melee_face_r", "MTD_melee_face_jaw", "TD_PISTOL_FRONT", "TD_PISTOL_FRONT_KILL", "TD_PISTOL_REAR", "TD_PISTOL_REAR_KILL", "TD_RIFLE_FRONT_KILL", "TD_RIFLE_NONLETHAL_FRONT", "TD_RIFLE_NONLETHAL_REAR", "TD_SHOTGUN_FRONT_KILL", "TD_SHOTGUN_REAR_KILL" };
            var setBloodLevel = new MenuListItem("피 상태 설정", bloodList, PlayerBlood, "플레이어의 피 상태를 설정합니다.");

            var healPlayerBtn = new MenuItem("플레이어 회복", "플레이어의 체력을 최대로 회복합니다.");
            var cleanPlayerBtn = new MenuItem("플레이어 옷 청소", "플레이어의 옷을 깨끗하게 합니다.");
            var dryPlayerBtn = new MenuItem("플레이어 옷 건조", "플레이어의 옷을 마르게 합니다.");
            var wetPlayerBtn = new MenuItem("플레이어 옷 젖게 하기", "플레이어의 옷을 젖게 합니다.");
            var suicidePlayerBtn = new MenuItem("~r~자살", "알약을 먹거나, 권총이 있다면 권총으로 스스로를 죽입니다.");

            var vehicleAutoPilot = new Menu("오토파일럿", "차량 오토파일럿 옵션입니다.");

            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            var vehicleAutoPilotBtn = new MenuItem("차량 오토파일럿 메뉴", "차량 오토파일럿 옵션을 관리합니다.")
            {
                Label = "→→→"
            };

            var drivingStyles = new List<string>() { "기본", "빠르게", "고속도로 회피", "후진 주행", "사용자 지정" };
            var drivingStyle = new MenuListItem("주행 스타일", drivingStyles, 0, "경로지점 이동 및 무작위 주행 기능에 사용할 주행 스타일을 설정합니다.");

            // Scenarios (list can be found in the PedScenarios class)
            var playerScenarios = new MenuListItem("플레이어 시나리오", PedScenarios.Scenarios, 0, "시나리오를 선택한 뒤 엔터를 눌러 시작합니다. 다른 시나리오를 선택하면 현재 시나리오를 덮어씁니다. 이미 해당 시나리오를 실행 중이라면 다시 선택 시 중지됩니다.");
            var stopScenario = new MenuItem("시나리오 강제 중지", "현재 실행 중인 시나리오를 종료 애니메이션을 기다리지 않고 즉시 중단합니다.");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POClearBlood))
            {
                menu.AddMenuItem(clearBloodBtn);
            }
            if (IsAllowed(Permission.POSetBlood))
            {
                menu.AddMenuItem(setBloodLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                var startDrivingWaypoint = new MenuItem("경로지점까지 운전", "플레이어 PED가 차량을 경로지점까지 운전하게 합니다.");
                var startDrivingRandomly = new MenuItem("무작위로 주행", "플레이어 PED가 맵을 무작위로 돌아다니며 운전하게 합니다.");
                var stopDriving = new MenuItem("운전 중지", "플레이어 PED가 적절한 장소를 찾아 차량을 정차합니다. 차량이 정차 위치에 도달하면 작업이 종료됩니다.");
                var forceStopDriving = new MenuItem("운전 강제 중지", "정차 장소를 찾지 않고 즉시 운전 작업을 중단합니다.");
                var customDrivingStyle = new MenuItem("사용자 지정 주행 스타일", "사용자 지정 주행 스타일을 선택합니다. 먼저 주행 스타일 목록에서 '사용자 지정'을 선택해 활성화해야 합니다.") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                var knownNames = new Dictionary<int, string>()
                {
                    { 0, "차량에 정지" },
                    { 1, "보행자에 정지" },
                    { 2, "모든 차량을 피해 회피" },
                    { 3, "정차된 차량을 피해 조향" },
                    { 4, "보행자를 피해 조향" },
                    { 5, "도로 위 물체를 피해 조향" },
                    { 6, "플레이어 보행자는 회피하지 않음" },
                    { 7, "신호등 준수" },
                    { 8, "회피 시 오프로드 허용" },
                    { 9, "역주행 허용" },
                    { 10, "후진 기어 사용" },
                    { 11, "직선 대신 배회 경로 사용" },
                    { 12, "제한 구역 회피" },
                    { 13, "백그라운드 경로 탐색 방지" },
                    { 14, "도로 속도에 맞춰 순항 속도 조정" },
                    { 18, "지름길 사용 (최단 경로)" },
                    { 19, "장애물 주변 차선 변경" },
                    { 21, "비활성 노드 사용" },
                    { 22, "내비메시 경로 우선" },
                    { 23, "비행기 택싱 모드" },
                    { 24, "직선 주행 강제" },
                    { 25, "교차로에서 스트링 풀링 사용" },
                    { 29, "고속도로 회피 (가능하면)" },
                    { 30, "도로 방향에 맞춰 합류 강제" },
                };
                for (var i = 0; i < 31; i++)
                {
                    var name = "~r~알 수 없는 플래그";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    var checkbox = new MenuCheckboxItem(name, "이 주행 스타일 플래그를 켜거나 끕니다.", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"사용자 지정 스타일: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("주행 스타일이 업데이트되었습니다.");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("이전 메뉴에서 사용자 지정 주행 스타일을 활성화하지 않아 주행 스타일이 업데이트되지 않았습니다.");
                    }
                };

                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("이제 플레이어 PED가 대신 차량을 운전합니다. 언제든지 운전 중지 버튼을 눌러 취소할 수 있습니다. 목적지에 도착하면 차량이 정차합니다.");
                                    }
                                    else
                                    {
                                        Notify.Error("해당 위치로 운전하려면 먼저 경로지점을 설정해야 합니다!");
                                    }

                                }
                                else if (item == startDrivingRandomly)
                                {
                                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("이제 플레이어 PED가 대신 차량을 운전합니다. 언제든지 운전 중지 버튼을 눌러 취소할 수 있습니다.");
                                }
                            }
                            else
                            {
                                Notify.Error("이 차량의 운전석에 있어야 합니다!");
                            }
                        }
                        else
                        {
                            Notify.Error("차량이 파손되었거나 존재하지 않습니다!");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("먼저 차량에 탑승해야 합니다!");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                var outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("플레이어 PED가 적절한 장소를 찾아 차량을 세운 뒤 운전을 멈춥니다. 잠시만 기다려 주세요.");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("플레이어 PED가 운전을 멈췄습니다.");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("플레이어 PED가 어떤 차량에도 탑승하고 있지 않습니다.");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("운전 작업이 취소되었습니다.");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        var style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"주행 작업 스타일이 이제 ~r~{drivingStyles[listIndex]}~s~(으)로 설정되었습니다.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Set blood level
                else if (listItem == setBloodLevel)
                {
                    ApplyPedDamagePack(Game.PlayerPed.Handle, bloodList[listIndex], 100, 100);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = listItem.ListIndex * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    PlayScenario("forcestop");
                }
                else if (item == clearBloodBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Game.PlayerPed.ResetVisibleDamage();
                    // not ideal for removing visible bruises & scars, may have some sync issues but could not find an alternative method, anyone who does feel free to update

                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 0, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 1, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 2, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 3, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 4, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 5, "ALL");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("플레이어를 회복했습니다.");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("플레이어 옷이 깨끗해졌습니다.");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("플레이어가 마른 상태가 되었습니다.");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("플레이어가 젖은 상태가 되었습니다.");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };
            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            var binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (var i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            var style = index switch
            {
                0 => 443,// normal
                1 => 575,// rushed
                2 => 536871355,// Avoid highways
                3 => 1467,// Go in reverse
                4 => GetCustomDrivingStyle(),// custom driving style;
                _ => 0,// no style (impossible, but oh well)
            };
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

    }
}
