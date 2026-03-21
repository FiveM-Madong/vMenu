using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class MiscSettings
    {
        // Variables
        private Menu menu;
        private Menu teleportOptionsMenu;
        private Menu developerToolsMenu;
        private Menu entitySpawnerMenu;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool LockCameraX { get; private set; } = false;
        public bool LockCameraY { get; private set; } = false;
        public bool MPPedPreviews { get; private set; } = UserDefaults.MPPedPreviews;
        public bool ShowLocationBlips { get; private set; } = UserDefaults.MiscLocationBlips;
        public bool ShowPlayerBlips { get; private set; } = UserDefaults.MiscShowPlayerBlips;
        public bool MiscShowOverheadNames { get; private set; } = UserDefaults.MiscShowOverheadNames;
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool ShowPedModelDimensions { get; private set; } = false;
        public bool ShowPropModelDimensions { get; private set; } = false;
        public bool ShowEntityHandles { get; private set; } = false;
        public bool ShowEntityModels { get; private set; } = false;
        public bool ShowEntityNetOwners { get; private set; } = false;
        public bool MiscRespawnDefaultCharacter { get; private set; } = UserDefaults.MiscRespawnDefaultCharacter;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;
        private bool _disablePrivateMessages;
        public bool MiscDisablePrivateMessages
        {
            get => _disablePrivateMessages;
            set
            {
                _disablePrivateMessages = value;
                Game.Player.State.Set("vmenu_pms_disabled", value, true);
            }
        }
        public bool MiscDisableControllerSupport { get; private set; } = UserDefaults.MiscDisableControllerSupport;

        internal bool TimecycleEnabled { get; private set; } = false;
        internal int LastTimeCycleModifierIndex { get; private set; } = UserDefaults.MiscLastTimeCycleModifierIndex;
        internal int LastTimeCycleModifierStrength { get; private set; } = UserDefaults.MiscLastTimeCycleModifierStrength;


        // keybind states
        public bool KbTpToWaypoint { get; private set; } = UserDefaults.KbTpToWaypoint;
        public int KbTpToWaypointKey { get; } = vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key) != -1
            ? vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key)
            : 168; // 168 (F7 by default)
        public bool KbDriftMode { get; private set; } = UserDefaults.KbDriftMode;
        public bool KbRecordKeys { get; private set; } = UserDefaults.KbRecordKeys;
        public bool KbRadarKeys { get; private set; } = UserDefaults.KbRadarKeys;
        public bool KbPointKeys { get; private set; } = UserDefaults.KbPointKeys;

        internal static List<vMenuShared.ConfigManager.TeleportLocation> TpLocations = new();

        public MiscSettings()
        {
            // Sets statebag when resource starts
            MiscDisablePrivateMessages = UserDefaults.MiscDisablePrivateMessages;
        }

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            if (MenuController.MenuAlignment != (MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);

                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "기타 설정");
            teleportOptionsMenu = new Menu(Game.Player.Name, "텔레포트 설정");
            developerToolsMenu = new Menu(Game.Player.Name, "개발 도구");
            entitySpawnerMenu = new Menu(Game.Player.Name, "엔티티 생성기");

            // teleport menu
            var teleportMenu = new Menu(Game.Player.Name, "텔레포트 위치");
            var teleportMenuBtn = new MenuItem("텔레포트 위치", "서버 관리자가 미리 설정한 위치로 이동합니다.");
            MenuController.AddSubmenu(menu, teleportMenu);
            MenuController.BindMenuItem(menu, teleportMenu, teleportMenuBtn);

            // keybind settings menu
            var keybindMenu = new Menu(Game.Player.Name, "키바인드 설정");
            var keybindMenuBtn = new MenuItem("키바인드 설정", "일부 기능의 키바인드를 켜거나 끕니다.");
            MenuController.AddSubmenu(menu, keybindMenu);
            MenuController.BindMenuItem(menu, keybindMenu, keybindMenuBtn);

            // keybind settings menu items
            var kbTpToWaypoint = new MenuCheckboxItem("웨이포인트로 이동", "키를 누르면 현재 설정한 웨이포인트로 이동합니다. 기본 키는 ~r~F7~s~이며, 서버 운영자가 변경할 수 있으니 모를 경우 서버 관리자에게 문의하세요.", KbTpToWaypoint);
            var kbDriftMode = new MenuCheckboxItem("드리프트 모드", "키보드의 Left Shift 또는 컨트롤러의 X를 누르고 있을 때 차량 접지력을 거의 없앱니다.", KbDriftMode);
            var kbRecordKeys = new MenuCheckboxItem("녹화 조작", "키보드와 컨트롤러의 녹화(락스타 에디터용 게임플레이 녹화) 단축키를 켜거나 끕니다.", KbRecordKeys);
            var kbRadarKeys = new MenuCheckboxItem("미니맵 조작", "멀티플레이어 정보 키(키보드 Z, 컨트롤러 아래 화살표)를 눌러 확장 레이더와 일반 레이더를 전환합니다.", KbRadarKeys);
            var kbPointKeysCheckbox = new MenuCheckboxItem("손가락 가리키기 조작", "손가락 가리키기 기능의 토글 키를 활성화합니다. 기본 QWERTY 키보드 기준 키는 'B'이며, 컨트롤러는 오른쪽 아날로그 스틱을 빠르게 두 번 누르면 됩니다.", KbPointKeys);
            var backBtn = new MenuItem("뒤로");

            // Create the menu items.
            var rightAlignMenu = new MenuCheckboxItem("메뉴 오른쪽 정렬", "vMenu를 화면 왼쪽에 표시하고 싶다면 이 옵션을 끄세요. 이 설정은 즉시 저장되며, 따로 설정 저장을 누를 필요가 없습니다.", MiscRightAlignMenu);
            var disablePms = new MenuCheckboxItem("개인 메시지 차단", "온라인 플레이어 메뉴를 통해 다른 플레이어가 나에게 개인 메시지를 보내지 못하게 합니다. 이 옵션을 켜면 나도 다른 플레이어에게 메시지를 보낼 수 없습니다.", MiscDisablePrivateMessages);
            var disableControllerKey = new MenuCheckboxItem("컨트롤러 지원 비활성화", "컨트롤러로 메뉴를 여는 키를 비활성화합니다. 메뉴 이동 버튼까지 비활성화되지는 않습니다.", MiscDisableControllerSupport);
            var speedKmh = new MenuCheckboxItem("속도 표시 (KM/H)", "화면에 현재 속도를 KM/h 단위로 표시합니다.", ShowSpeedoKmh);
            var speedMph = new MenuCheckboxItem("속도 표시 (MPH)", "화면에 현재 속도를 MPH 단위로 표시합니다.", ShowSpeedoMph);
            var coords = new MenuCheckboxItem("좌표 표시", "화면 상단에 현재 좌표를 표시합니다.", ShowCoordinates);
            var hideRadar = new MenuCheckboxItem("레이더 숨기기", "레이더/미니맵을 숨깁니다.", HideRadar);
            var hideHud = new MenuCheckboxItem("HUD 숨기기", "모든 HUD 요소를 숨깁니다.", HideHud);
            var showLocation = new MenuCheckboxItem("위치 표시", "현재 위치, 바라보는 방향, 가장 가까운 교차로를 표시합니다. PLD와 비슷한 기능입니다. ~r~경고: 이 기능은 60Hz 기준 최대 약 4.6 FPS 정도 성능 저하가 발생할 수 있습니다.", ShowLocation) { LeftIcon = MenuItem.Icon.WARNING };
            var drawTime = new MenuCheckboxItem("화면에 시간 표시", "화면에 현재 시간을 표시합니다.", DrawTimeOnScreen);
            var saveSettings = new MenuItem("개인 설정 저장", "현재 설정을 저장합니다. 저장은 클라이언트 측에서만 이루어지므로 Windows를 다시 설치하면 설정이 사라질 수 있습니다. 이 설정은 vMenu를 사용하는 모든 서버에서 공유됩니다.")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            var exportData = new MenuItem("데이터 내보내기/가져오기", "곧 제공 예정(TM): 저장된 데이터를 가져오고 내보내는 기능입니다.");
            var joinQuitNotifs = new MenuCheckboxItem("접속 / 퇴장 알림", "누군가 서버에 접속하거나 퇴장할 때 알림을 받습니다.", JoinQuitNotifications);
            var deathNotifs = new MenuCheckboxItem("사망 알림", "누군가 사망하거나 처치되었을 때 알림을 받습니다.", DeathNotifications);
            var nightVision = new MenuCheckboxItem("야간 투시경 켜기/끄기", "야간 투시경을 켜거나 끕니다.", false);
            var thermalVision = new MenuCheckboxItem("열 감지 시야 켜기/끄기", "열 감지 시야를 켜거나 끕니다.", false);
            var vehModelDimensions = new MenuCheckboxItem("차량 외곽선 표시", "주변에 있는 모든 차량의 모델 외곽선을 표시합니다.", ShowVehicleModelDimensions);
            var propModelDimensions = new MenuCheckboxItem("오브젝트 외곽선 표시", "주변에 있는 모든 오브젝트의 모델 외곽선을 표시합니다.", ShowPropModelDimensions);
            var pedModelDimensions = new MenuCheckboxItem("보행자 외곽선 표시", "주변에 있는 모든 보행자의 모델 외곽선을 표시합니다.", ShowPedModelDimensions);
            var showEntityHandles = new MenuCheckboxItem("엔티티 핸들 표시", "주변 엔티티들의 핸들을 표시합니다. 이 기능을 사용하려면 위 외곽선 표시 기능이 켜져 있어야 합니다.", ShowEntityHandles);
            var showEntityModels = new MenuCheckboxItem("엔티티 모델 표시", "주변 엔티티들의 모델 정보를 표시합니다. 이 기능을 사용하려면 위 외곽선 표시 기능이 켜져 있어야 합니다.", ShowEntityModels);
            var showEntityNetOwners = new MenuCheckboxItem("네트워크 소유자 표시", "주변 엔티티들의 네트워크 소유자를 표시합니다. 이 기능을 사용하려면 위 외곽선 표시 기능이 켜져 있어야 합니다.", ShowEntityNetOwners);
            var dimensionsDistanceSlider = new MenuSliderItem("표시 반경", "엔티티 모델/핸들/외곽선 표시 범위를 설정합니다.", 0, 20, 20, false);

            var clearArea = new MenuItem("주변 정리", "플레이어 주변 100미터 범위를 정리합니다. 손상, 오염, 보행자, 오브젝트, 차량 등을 모두 정리하고 기본 월드 상태로 되돌립니다.");
            var lockCamX = new MenuCheckboxItem("카메라 좌우 회전 고정", "카메라의 좌우 회전을 고정합니다. 헬리콥터 탑승 시 유용할 수 있습니다.", false);
            var lockCamY = new MenuCheckboxItem("카메라 상하 회전 고정", "카메라의 상하 회전을 고정합니다. 헬리콥터 탑승 시 유용할 수 있습니다.", false);

            var mpPedPreview = new MenuCheckboxItem("3D MP 캐릭터 미리보기", "저장된 MP 캐릭터를 볼 때 3D 미리보기를 표시합니다.", MPPedPreviews);

            // Entity spawner
            var spawnNewEntity = new MenuItem("새 엔티티 생성", "월드에 엔티티를 생성하고 위치와 회전을 설정할 수 있습니다.");
            var confirmEntityPosition = new MenuItem("엔티티 위치 확정", "엔티티 배치를 종료하고 현재 위치에 확정합니다.");
            var cancelEntity = new MenuItem("취소", "현재 엔티티를 삭제하고 배치를 취소합니다.");
            var confirmAndDuplicate = new MenuItem("엔티티 위치 확정 및 복제", "엔티티를 현재 위치에 확정한 뒤 새 엔티티를 하나 더 생성해 배치합니다.");

            var connectionSubmenu = new Menu(Game.Player.Name, "연결 옵션");
            var connectionSubmenuBtn = new MenuItem("연결 옵션", "세션, 서버 연결 해제, 게임 종료 관련 옵션입니다.");

            var quitSession = new MenuItem("세션 나가기", "서버 연결은 유지한 채 네트워크 세션만 종료합니다. ~r~호스트일 때는 사용할 수 없습니다.");
            var rejoinSession = new MenuItem("세션 재참가", "모든 상황에서 작동하는 것은 아니지만, '세션 나가기'를 누른 뒤 이전 세션에 다시 참가하고 싶을 때 시도해볼 수 있습니다.");
            var quitGame = new MenuItem("게임 종료", "5초 후 게임을 종료합니다.");
            var disconnectFromServer = new MenuItem("서버 연결 해제", "서버 연결을 끊고 서버 목록으로 돌아갑니다. ~r~권장되지 않는 기능이며, 더 안정적인 사용을 위해서는 게임을 완전히 종료한 뒤 다시 실행하는 것이 좋습니다.");
            connectionSubmenu.AddMenuItem(quitSession);
            connectionSubmenu.AddMenuItem(rejoinSession);
            connectionSubmenu.AddMenuItem(quitGame);
            connectionSubmenu.AddMenuItem(disconnectFromServer);

            var enableTimeCycle = new MenuCheckboxItem("타임사이클 효과 사용", "아래 목록의 타임사이클 효과를 켜거나 끕니다.", TimecycleEnabled);
            var timeCycleModifiersListData = TimeCycles.Timecycles.ToList();
            for (var i = 0; i < timeCycleModifiersListData.Count; i++)
            {
                timeCycleModifiersListData[i] += $" ({i + 1}/{timeCycleModifiersListData.Count})";
            }
            var timeCycles = new MenuListItem("TM", timeCycleModifiersListData, MathUtil.Clamp(LastTimeCycleModifierIndex, 0, Math.Max(0, timeCycleModifiersListData.Count - 1)), "타임사이클 효과를 선택한 뒤 위 체크박스를 활성화하세요.");
            var timeCycleIntensity = new MenuSliderItem("타임사이클 효과 강도", "타임사이클 효과의 강도를 설정합니다.", 0, 20, LastTimeCycleModifierStrength, true);

            var locationBlips = new MenuCheckboxItem("위치 블립 표시", "일부 주요 위치를 지도에 블립으로 표시합니다.", ShowLocationBlips);
            var playerBlips = new MenuCheckboxItem("플레이어 블립 표시", "모든 플레이어를 지도에 블립으로 표시합니다. ~y~참고: 서버가 OneSync Infinity를 사용하는 경우, 너무 멀리 있는 플레이어에게는 작동하지 않을 수 있습니다.", ShowPlayerBlips);
            var playerNames = new MenuCheckboxItem("플레이어 이름 표시", "플레이어 머리 위 이름 표시를 켜거나 끕니다.", MiscShowOverheadNames);
            var respawnDefaultCharacter = new MenuCheckboxItem("기본 MP 캐릭터로 리스폰", "이 옵션을 켜면 기본으로 저장한 MP 캐릭터로 리스폰합니다. 단, 서버 운영자가 이 기능을 전체 비활성화할 수 있습니다. 기본 캐릭터를 설정하려면 저장된 MP 캐릭터 중 하나로 이동한 뒤 '기본 캐릭터로 설정' 버튼을 누르세요.", MiscRespawnDefaultCharacter);
            var restorePlayerAppearance = new MenuCheckboxItem("플레이어 외형 복원", "사망 후 리스폰할 때 플레이어 외형을 복원합니다. 서버에 다시 접속하는 경우 이전 외형은 복원되지 않습니다.", RestorePlayerAppearance);
            var restorePlayerWeapons = new MenuCheckboxItem("플레이어 무기 복원", "사망 후 리스폰할 때 무기를 복원합니다. 서버에 다시 접속하는 경우 이전 무기는 복원되지 않습니다.", RestorePlayerWeapons);

            MenuController.AddSubmenu(menu, connectionSubmenu);
            MenuController.BindMenuItem(menu, connectionSubmenu, connectionSubmenuBtn);

            keybindMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == kbTpToWaypoint)
                {
                    KbTpToWaypoint = _checked;
                }
                else if (item == kbDriftMode)
                {
                    KbDriftMode = _checked;
                }
                else if (item == kbRecordKeys)
                {
                    KbRecordKeys = _checked;
                }
                else if (item == kbRadarKeys)
                {
                    KbRadarKeys = _checked;
                }
                else if (item == kbPointKeysCheckbox)
                {
                    KbPointKeys = _checked;
                }
            };
            keybindMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backBtn)
                {
                    keybindMenu.GoBack();
                }
            };

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    CommonFunctions.QuitGame();
                }
                else if (item == quitSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        if (NetworkIsHost())
                        {
                            Notify.Error("호스트인 상태에서는 세션을 나갈 수 없습니다. 그렇게 하면 다른 플레이어가 서버에 접속하거나 머무를 수 없게 됩니다.");
                        }
                        else
                        {
                            QuitSession();
                        }
                    }
                    else
                    {
                        Notify.Error("현재 어떤 세션에도 참여 중이 아닙니다.");
                    }
                }
                else if (item == rejoinSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        Notify.Error("이미 세션에 연결되어 있습니다.");
                    }
                    else
                    {
                        Notify.Info("세션에 다시 참가를 시도하는 중입니다.");
                        NetworkSessionHost(-1, 32, false);
                    }
                }
                else if (item == disconnectFromServer)
                {

                    RegisterCommand("disconnect", new Action<dynamic, dynamic, dynamic>((a, b, c) => { }), false);
                    ExecuteCommand("disconnect");
                }
            };

            // Teleportation options
            if (IsAllowed(Permission.MSTeleportToWp) || IsAllowed(Permission.MSTeleportLocations) || IsAllowed(Permission.MSTeleportToCoord))
            {
                var teleportOptionsMenuBtn = new MenuItem("텔레포트 설정", "다양한 텔레포트 옵션입니다.") { Label = "→→→" };
                menu.AddMenuItem(teleportOptionsMenuBtn);
                MenuController.BindMenuItem(menu, teleportOptionsMenu, teleportOptionsMenuBtn);

                var tptowp = new MenuItem("웨이포인트로 이동", "지도에 설정한 웨이포인트로 이동합니다.");
                var tpToCoord = new MenuItem("좌표로 텔레포트", "x, y, z 좌표를 입력하면 해당 위치로 이동합니다.");
                var saveLocationBtn = new MenuItem("텔레포트 위치 저장", "현재 위치를 텔레포트 위치 메뉴에 추가하고 서버에 저장합니다.");
                teleportOptionsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    // Teleport to waypoint.
                    if (item == tptowp)
                    {
                        TeleportToWp();
                    }
                    else if (item == tpToCoord)
                    {
                        var x = await GetUserInput("X 좌표를 입력하세요.");
                        if (string.IsNullOrEmpty(x))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        var y = await GetUserInput("Y 좌표를 입력하세요.");
                        if (string.IsNullOrEmpty(y))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        var z = await GetUserInput("Z 좌표를 입력하세요.");
                        if (string.IsNullOrEmpty(z))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }


                        if (!float.TryParse(x, out var posX))
                        {
                            if (int.TryParse(x, out var intX))
                            {
                                posX = intX;
                            }
                            else
                            {
                                Notify.Error("올바른 X 좌표를 입력하지 않았습니다.");
                                return;
                            }
                        }
                        if (!float.TryParse(y, out var posY))
                        {
                            if (int.TryParse(y, out var intY))
                            {
                                posY = intY;
                            }
                            else
                            {
                                Notify.Error("올바른 Y 좌표를 입력하지 않았습니다.");
                                return;
                            }
                        }
                        if (!float.TryParse(z, out var posZ))
                        {
                            if (int.TryParse(z, out var intZ))
                            {
                                posZ = intZ;
                            }
                            else
                            {
                                Notify.Error("올바른 Z 좌표를 입력하지 않았습니다.");
                                return;
                            }
                        }

                        await TeleportToCoords(new Vector3(posX, posY, posZ), true);
                    }
                    else if (item == saveLocationBtn)
                    {
                        SavePlayerLocationToLocationsFile();
                    }
                };

                if (IsAllowed(Permission.MSTeleportToWp))
                {
                    teleportOptionsMenu.AddMenuItem(tptowp);
                    keybindMenu.AddMenuItem(kbTpToWaypoint);
                }
                if (IsAllowed(Permission.MSTeleportToCoord))
                {
                    teleportOptionsMenu.AddMenuItem(tpToCoord);
                }
                if (IsAllowed(Permission.MSTeleportLocations))
                {
                    teleportOptionsMenu.AddMenuItem(teleportMenuBtn);

                    MenuController.AddSubmenu(teleportOptionsMenu, teleportMenu);
                    MenuController.BindMenuItem(teleportOptionsMenu, teleportMenu, teleportMenuBtn);
                    teleportMenuBtn.Label = "→→→";

                    teleportMenu.OnMenuOpen += (sender) =>
                    {
                        if (teleportMenu.Size != TpLocations.Count())
                        {
                            teleportMenu.ClearMenuItems();
                            foreach (var location in TpLocations)
                            {
                                var x = Math.Round(location.coordinates.X, 2);
                                var y = Math.Round(location.coordinates.Y, 2);
                                var z = Math.Round(location.coordinates.Z, 2);
                                var heading = Math.Round(location.heading, 2);
                                var tpBtn = new MenuItem(location.name, $"~y~{location.name}~s~ 위치로 이동합니다.~n~~s~x: ~y~{x}~n~~s~y: ~y~{y}~n~~s~z: ~y~{z}~n~~s~heading: ~y~{heading}") { ItemData = location };
                                teleportMenu.AddMenuItem(tpBtn);
                            }
                        }
                    };

                    teleportMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item.ItemData is vMenuShared.ConfigManager.TeleportLocation tl)
                        {
                            await TeleportToCoords(tl.coordinates, true);
                            SetEntityHeading(Game.PlayerPed.Handle, tl.heading);
                            SetGameplayCamRelativeHeading(0f);
                        }
                    };

                    if (IsAllowed(Permission.MSTeleportSaveLocation))
                    {
                        teleportOptionsMenu.AddMenuItem(saveLocationBtn);
                    }
                }

            }

            #region dev tools menu

            var devToolsBtn = new MenuItem("개발자 도구", "다양한 개발/디버그 도구입니다.") { Label = "→→→" };
            menu.AddMenuItem(devToolsBtn);
            MenuController.AddSubmenu(menu, developerToolsMenu);
            MenuController.BindMenuItem(menu, developerToolsMenu, devToolsBtn);

            // clear area and coordinates
            if (IsAllowed(Permission.MSClearArea))
            {
                developerToolsMenu.AddMenuItem(clearArea);
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                developerToolsMenu.AddMenuItem(coords);
            }

            // model outlines
            if ((!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_entity_outlines_tool)) && (IsAllowed(Permission.MSDevTools)))
            {
                developerToolsMenu.AddMenuItem(vehModelDimensions);
                developerToolsMenu.AddMenuItem(propModelDimensions);
                developerToolsMenu.AddMenuItem(pedModelDimensions);
                developerToolsMenu.AddMenuItem(showEntityHandles);
                developerToolsMenu.AddMenuItem(showEntityModels);
                developerToolsMenu.AddMenuItem(showEntityNetOwners);
                developerToolsMenu.AddMenuItem(dimensionsDistanceSlider);
            }


            // timecycle modifiers
            developerToolsMenu.AddMenuItem(timeCycles);
            developerToolsMenu.AddMenuItem(enableTimeCycle);
            developerToolsMenu.AddMenuItem(timeCycleIntensity);

            developerToolsMenu.OnSliderPositionChange += (sender, item, oldPos, newPos, itemIndex) =>
            {
                if (item == timeCycleIntensity)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = newPos / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
                else if (item == dimensionsDistanceSlider)
                {
                    FunctionsController.entityRange = newPos / 20f * 2000f; // max radius = 2000f;
                }
            };

            developerToolsMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == timeCycles)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = timeCycleIntensity.Position / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
            };

            developerToolsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == clearArea)
                {
                    BaseScript.TriggerServerEvent("vMenu:ClearArea");
                }
            };

            developerToolsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == vehModelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
                else if (item == propModelDimensions)
                {
                    ShowPropModelDimensions = _checked;
                }
                else if (item == pedModelDimensions)
                {
                    ShowPedModelDimensions = _checked;
                }
                else if (item == showEntityHandles)
                {
                    ShowEntityHandles = _checked;
                }
                else if (item == showEntityModels)
                {
                    ShowEntityModels = _checked;
                }
                else if (item == showEntityNetOwners)
                {
                    ShowEntityNetOwners = _checked;
                }
                else if (item == enableTimeCycle)
                {
                    TimecycleEnabled = _checked;
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = timeCycleIntensity.Position / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
            };

            if (IsAllowed(Permission.MSEntitySpawner))
            {
                var entSpawnerMenuBtn = new MenuItem("엔티티 생성기", "엔티티를 생성하고 이동시킵니다.") { Label = "→→→" };
                developerToolsMenu.AddMenuItem(entSpawnerMenuBtn);
                MenuController.BindMenuItem(developerToolsMenu, entitySpawnerMenu, entSpawnerMenuBtn);

                entitySpawnerMenu.AddMenuItem(spawnNewEntity);
                entitySpawnerMenu.AddMenuItem(confirmEntityPosition);
                entitySpawnerMenu.AddMenuItem(confirmAndDuplicate);
                entitySpawnerMenu.AddMenuItem(cancelEntity);

                entitySpawnerMenu.OnItemSelect += async (sender, item, index) =>
                {
                    if (item == spawnNewEntity)
                    {
                        if (EntitySpawner.CurrentEntity != null || EntitySpawner.Active)
                        {
                            Notify.Error("이미 배치 중인 엔티티가 있습니다. 위치를 확정하거나 취소한 뒤 다시 시도하세요!");
                            return;
                        }

                        var result = await GetUserInput(windowTitle: "모델 이름을 입력하세요");

                        if (string.IsNullOrEmpty(result))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }

                        EntitySpawner.SpawnEntity(result, Game.PlayerPed.Position);
                    }
                    else if (item == confirmEntityPosition || item == confirmAndDuplicate)
                    {
                        if (EntitySpawner.CurrentEntity != null)
                        {
                            EntitySpawner.FinishPlacement(item == confirmAndDuplicate);
                        }
                        else
                        {
                            Notify.Error("위치를 확정할 엔티티가 없습니다!");
                        }
                    }
                    else if (item == cancelEntity)
                    {
                        if (EntitySpawner.CurrentEntity != null)
                        {
                            EntitySpawner.CurrentEntity.Delete();
                        }
                        else
                        {
                            Notify.Error("취소할 엔티티가 없습니다!");
                        }
                    }
                };
            }

            #endregion


            // Keybind options
            if (IsAllowed(Permission.MSDriftMode))
            {
                keybindMenu.AddMenuItem(kbDriftMode);
            }
            // always allowed keybind menu options
            keybindMenu.AddMenuItem(kbRecordKeys);
            keybindMenu.AddMenuItem(kbRadarKeys);
            keybindMenu.AddMenuItem(kbPointKeysCheckbox);
            keybindMenu.AddMenuItem(backBtn);

            // Always allowed
            menu.AddMenuItem(rightAlignMenu);
            menu.AddMenuItem(disablePms);
            menu.AddMenuItem(disableControllerKey);
            menu.AddMenuItem(speedKmh);
            menu.AddMenuItem(speedMph);
            menu.AddMenuItem(keybindMenuBtn);
            keybindMenuBtn.Label = "→→→";
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddMenuItem(connectionSubmenuBtn);
                connectionSubmenuBtn.Label = "→→→";
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddMenuItem(showLocation);
            }
            menu.AddMenuItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddMenuItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddMenuItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddMenuItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddMenuItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddMenuItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddMenuItem(playerBlips);
            }
            if (IsAllowed(Permission.MSOverheadNames))
            {
                menu.AddMenuItem(playerNames);
            }
            // always allowed, it just won't do anything if the server owner disabled the feature, but players can still toggle it.
            menu.AddMenuItem(respawnDefaultCharacter);
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddMenuItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddMenuItem(restorePlayerWeapons);
            }

            // Always allowed
            menu.AddMenuItem(hideRadar);
            menu.AddMenuItem(hideHud);
            menu.AddMenuItem(lockCamX);
            menu.AddMenuItem(lockCamY);

            // If disabled at a server level, don't show the option to players
            if (GetSettingsBool(Setting.vmenu_mp_ped_preview))
            {
                menu.AddMenuItem(mpPedPreview);
            }

            if (MainMenu.EnableExperimentalFeatures)
            {
                menu.AddMenuItem(exportData);
            }
            menu.AddMenuItem(saveSettings);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == rightAlignMenu)
                {

                    MenuController.MenuAlignment = _checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
                    MiscRightAlignMenu = _checked;
                    UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;

                    if (MenuController.MenuAlignment != (_checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
                    {
                        Notify.Error(CommonErrors.RightAlignedNotSupported);
                        // (re)set the default to left just in case so they don't get this error again in the future.
                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = false;
                        UserDefaults.MiscRightAlignMenu = false;
                    }

                }
                else if (item == disablePms)
                {
                    MiscDisablePrivateMessages = _checked;
                }
                else if (item == disableControllerKey)
                {
                    MiscDisableControllerSupport = _checked;
                    MenuController.EnableMenuToggleKeyOnController = !_checked;
                }
                else if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
                }
                else if (item == hideHud)
                {
                    HideHud = _checked;
                    DisplayHud(!_checked);
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                    if (!_checked)
                    {
                        DisplayRadar(true);
                    }
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
                else if (item == drawTime)
                {
                    DrawTimeOnScreen = _checked;
                }
                else if (item == deathNotifs)
                {
                    DeathNotifications = _checked;
                }
                else if (item == joinQuitNotifs)
                {
                    JoinQuitNotifications = _checked;
                }
                else if (item == nightVision)
                {
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    SetSeethrough(_checked);
                }
                else if (item == lockCamX)
                {
                    LockCameraX = _checked;
                }
                else if (item == lockCamY)
                {
                    LockCameraY = _checked;
                }
                else if (item == mpPedPreview)
                {
                    MPPedPreviews = _checked;
                }
                else if (item == locationBlips)
                {
                    ToggleBlips(_checked);
                    ShowLocationBlips = _checked;
                }
                else if (item == playerBlips)
                {
                    ShowPlayerBlips = _checked;
                }
                else if (item == playerNames)
                {
                    MiscShowOverheadNames = _checked;
                }
                else if (item == respawnDefaultCharacter)
                {
                    MiscRespawnDefaultCharacter = _checked;
                }
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }

            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // export data
                if (item == exportData)
                {
                    MenuController.CloseAllMenus();
                    var vehicles = GetSavedVehicles();
                    var normalPeds = StorageManager.GetSavedPeds();
                    var mpPeds = StorageManager.GetSavedMpPeds();
                    var weaponLoadouts = WeaponLoadouts.GetSavedWeapons();
                    var data = JsonConvert.SerializeObject(new
                    {
                        saved_vehicles = vehicles,
                        normal_peds = normalPeds,
                        mp_characters = mpPeds,
                        weapon_loadouts = weaponLoadouts
                    });
                    SendNuiMessage(data);
                    SetNuiFocus(true, true);
                }
                // save settings
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
                }
            };
        }


        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

        private readonly struct Blip
        {
            public readonly Vector3 Location;
            public readonly int Sprite;
            public readonly string Name;
            public readonly int Color;
            public readonly int blipID;

            public Blip(Vector3 Location, int Sprite, string Name, int Color, int blipID)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
                this.blipID = blipID;
            }
        }

        private readonly List<Blip> blips = new();

        /// <summary>
        /// Toggles blips on/off.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleBlips(bool enable)
        {
            if (enable)
            {
                try
                {
                    foreach (var bl in vMenuShared.ConfigManager.GetLocationBlipsData())
                    {
                        var blipID = AddBlipForCoord(bl.coordinates.X, bl.coordinates.Y, bl.coordinates.Z);
                        SetBlipSprite(blipID, bl.spriteID);
                        BeginTextCommandSetBlipName("STRING");
                        AddTextComponentSubstringPlayerName(bl.name);
                        EndTextCommandSetBlipName(blipID);
                        SetBlipColour(blipID, bl.color);
                        SetBlipAsShortRange(blipID, true);

                        var b = new Blip(bl.coordinates, bl.spriteID, bl.name, bl.color, blipID);
                        blips.Add(b);
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.Write($"\n\n[vMenu] locations.json 파일을 불러오는 중 오류가 발생했습니다. 이 문제를 해결하려면 서버 관리자에게 문의하세요.\n관리자에게 문의할 때 아래 오류 상세 내용을 함께 전달하세요:\n{ex.Message}.\n\n\n");
                }
            }
            else
            {
                if (blips.Count > 0)
                {
                    foreach (var blip in blips)
                    {
                        var id = blip.blipID;
                        if (DoesBlipExist(id))
                        {
                            RemoveBlip(ref id);
                        }
                    }
                }
                blips.Clear();
            }
        }

    }
}
