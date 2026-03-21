using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.menus;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        #region Variables

        public static bool PermissionsSetupComplete => ArePermissionsSetup;
        public static bool ConfigOptionsSetupComplete = false;

        public static string MenuToggleKey { get; private set; } = "M"; // M by default
        public static string NoClipKey { get; private set; } = "F2"; // F2 by default 
        public static Menu Menu { get; private set; }
        public static Menu PlayerSubmenu { get; private set; }
        public static Menu VehicleSubmenu { get; private set; }
        public static Menu WorldSubmenu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
        public static BannedPlayers BannedPlayersMenu { get; private set; }
        public static SavedVehicles SavedVehiclesMenu { get; private set; }
        public static PersonalVehicle PersonalVehicleMenu { get; private set; }
        public static VehicleOptions VehicleOptionsMenu { get; private set; }
        public static VehicleSpawner VehicleSpawnerMenu { get; private set; }
        public static PlayerAppearance PlayerAppearanceMenu { get; private set; }
        public static MpPedCustomization MpPedCustomizationMenu { get; private set; }
        public static TimeOptions TimeOptionsMenu { get; private set; }
        public static WeatherOptions WeatherOptionsMenu { get; private set; }
        public static WeaponOptions WeaponOptionsMenu { get; private set; }
        public static WeaponLoadouts WeaponLoadoutsMenu { get; private set; }
        public static Recording RecordingMenu { get; private set; }
        public static MiscSettings MiscSettingsMenu { get; private set; }
        public static VoiceChat VoiceChatSettingsMenu { get; private set; }
        public static About AboutMenu { get; private set; }
        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }
        public static IPlayerList PlayersList;

        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true";
        public static bool EnableExperimentalFeatures = (GetResourceMetadata(GetCurrentResourceName(), "experimental_features_enabled", 0) ?? "0") == "1";
        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static bool DontOpenMenus { get { return MenuController.DontOpenAnyMenu; } set { MenuController.DontOpenAnyMenu = value; } }
        public static bool DisableControls { get { return MenuController.DisableMenuButtons; } set { MenuController.DisableMenuButtons = value; } }

        public static bool MenuEnabled { get; private set; } = true;
        public static bool ClientFullyDisabled { get; private set; } = false;

        private const int currentCleanupVersion = 2;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {

            PlayersList = new NativePlayerList(Players);

            #region cleanup unused kvps
            var tmp_kvp_handle = StartFindKvp("");
            var cleanupVersionChecked = false;
            var tmp_kvp_names = new List<string>();
            while (true)
            {
                var k = FindKvp(tmp_kvp_handle);
                if (string.IsNullOrEmpty(k))
                {
                    break;
                }
                if (k == "vmenu_cleanup_version")
                {
                    if (GetResourceKvpInt("vmenu_cleanup_version") >= currentCleanupVersion)
                    {
                        cleanupVersionChecked = true;
                    }
                }
                tmp_kvp_names.Add(k);
            }
            EndFindKvp(tmp_kvp_handle);

            if (!cleanupVersionChecked)
            {
                SetResourceKvpInt("vmenu_cleanup_version", currentCleanupVersion);
                foreach (var kvp in tmp_kvp_names)
                {
#pragma warning disable CS8793 // The given expression always matches the provided pattern.
                    if (currentCleanupVersion is 1 or 2)
                    {
                        if (!kvp.StartsWith("settings_") && !kvp.StartsWith("vmenu") && !kvp.StartsWith("veh_") && !kvp.StartsWith("ped_") && !kvp.StartsWith("mp_ped_"))
                        {
                            DeleteResourceKvp(kvp);
                            Debug.WriteLine($"[vMenu] [정리 ID: 1] 사용되지 않는(오래된) KVP 제거됨: {kvp}.");
                        }
                    }
#pragma warning restore CS8793 // The given expression always matches the provided pattern.
                    if (currentCleanupVersion == 2)
                    {
                        if (kvp.StartsWith("mp_char"))
                        {
                            DeleteResourceKvp(kvp);
                            Debug.WriteLine($"[vMenu] [정리 ID: 2] 사용되지 않는(오래된) KVP 제거됨: {kvp}.");
                        }
                    }
                }
                Debug.WriteLine("[vMenu] 오래된 미사용 KVP 항목 정리가 완료되었습니다.");
            }
            #endregion
            #region keymapping
            string KeyMappingID = GetKeyMappingId();
            RegisterCommand($"vMenu:{KeyMappingID}:NoClip", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                if (IsAllowed(Permission.NoClip))
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
                        {
                            NoClipEnabled = !NoClipEnabled;
                        }
                        else
                        {
                            NoClipEnabled = false;
                            Notify.Error("이 차량은 존재하지 않거나, 노클립을 활성화하려면 운전석에 탑승해야 합니다!");
                        }
                    }
                    else
                    {
                        NoClipEnabled = !NoClipEnabled;
                    }
                }
            }), false);

            if (!(GetSettingsString(Setting.vmenu_noclip_toggle_key) == null))
            {
                NoClipKey = GetSettingsString(Setting.vmenu_noclip_toggle_key);
            }
            else
            {
                NoClipKey = "F2";
            }

            if (!(GetSettingsString(Setting.vmenu_menu_toggle_key) == null))
            {
                MenuToggleKey = GetSettingsString(Setting.vmenu_menu_toggle_key);
            }
            else
            {
                MenuToggleKey = "M";
            }
            MenuController.MenuToggleKey = (Control)(-1); // disables the menu toggle key
            RegisterKeyMapping($"vMenu:{KeyMappingID}:NoClip", "vMenu 노클립 전환 버튼", "keyboard", NoClipKey);
            RegisterKeyMapping($"vMenu:{KeyMappingID}:MenuToggle", "vMenu 열기/닫기 버튼", "keyboard", MenuToggleKey);
            RegisterKeyMapping($"vMenu:{KeyMappingID}:MenuToggle", "vMenu 열기/닫기 버튼 컨트롤러", "pad_digitalbuttonany", "start_index");
            #endregion
            if (EnableExperimentalFeatures)
            {
                RegisterCommand("testped", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
                {
                    var data = Game.PlayerPed.GetHeadBlendData();
                    Debug.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
                }), false);

                RegisterCommand("tattoo", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
                {
                    if (args != null && args[0] != null && args[1] != null)
                    {
                        Debug.WriteLine(args[0].ToString() + " " + args[1].ToString());
                        TattooCollectionData d = Game.GetTattooCollectionData(int.Parse(args[0].ToString()), int.Parse(args[1].ToString()));
                        Debug.WriteLine("check");
                        Debug.Write(JsonConvert.SerializeObject(d, Formatting.Indented) + "\n");
                    }
                }), false);

                RegisterCommand("clearfocus", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
                {
                    SetNuiFocus(false, false);
                }), false);
            }

            RegisterCommand("vmenuclient", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                if (args != null)
                {
                    if (args.Count > 0)
                    {
                        if (args[0].ToString().ToLower() == "debug")
                        {
                            DebugMode = !DebugMode;
                            Notify.Custom($"디버그 모드가 현재 다음과 같이 설정되었습니다: {DebugMode}.");
                            // Set discord rich precense once, allowing it to be overruled by other resources once those load.
                            if (DebugMode)
                            {
                                SetRichPresence($"vMenu {Version} 디버깅 중!");
                            }
                            else
                            {
                                SetRichPresence($"FiveM을 즐기는 중!");
                            }
                        }
                        else if (args[0].ToString().ToLower() == "gc")
                        {
                            GC.Collect();
                            Debug.Write("메모리를 정리했습니다.\n");
                        }
                        else if (args[0].ToString().ToLower() == "dump")
                        {
                            Notify.Info("전체 설정 덤프가 콘솔에 출력됩니다. 로그 파일을 확인하세요. 이 작업은 랙을 유발할 수 있습니다!");
                            Debug.WriteLine("\n\n\n########################### vMenu ###########################");
                            Debug.WriteLine($"실행 중인 vMenu 버전: {Version}, 실험 기능: {EnableExperimentalFeatures}, 디버그 모드: {DebugMode}.");
                            Debug.WriteLine("\n모든 KVP 목록을 덤프합니다:");
                            var handle = StartFindKvp("");
                            var names = new List<string>();
                            while (true)
                            {
                                var k = FindKvp(handle);
                                if (string.IsNullOrEmpty(k))
                                {
                                    break;
                                }
                                names.Add(k);
                            }
                            EndFindKvp(handle);

                            var kvps = new Dictionary<string, dynamic>();
                            foreach (var kvp in names)
                            {
                                var type = 0; // 0 = string, 1 = float, 2 = int.
                                if (kvp.StartsWith("settings_"))
                                {
                                    if (kvp == "settings_voiceChatProximity") // float
                                    {
                                        type = 1;
                                    }
                                    else if (kvp == "settings_clothingAnimationType") // int
                                    {
                                        type = 2;
                                    }
                                    else if (kvp == "settings_miscLastTimeCycleModifierIndex") // int
                                    {
                                        type = 2;
                                    }
                                    else if (kvp == "settings_miscLastTimeCycleModifierStrength") // int
                                    {
                                        type = 2;
                                    }
                                }
                                else if (kvp == "vmenu_cleanup_version") // int
                                {
                                    type = 2;
                                }
                                switch (type)
                                {
                                    case 0:
                                        var s = GetResourceKvpString(kvp);
                                        if (s.StartsWith("{") || s.StartsWith("["))
                                        {
                                            kvps.Add(kvp, JsonConvert.DeserializeObject(s));
                                        }
                                        else
                                        {
                                            kvps.Add(kvp, GetResourceKvpString(kvp));
                                        }
                                        break;
                                    case 1:
                                        kvps.Add(kvp, GetResourceKvpFloat(kvp));
                                        break;
                                    case 2:
                                        kvps.Add(kvp, GetResourceKvpInt(kvp));
                                        break;
                                }
                            }
                            Debug.WriteLine(@JsonConvert.SerializeObject(kvps, Formatting.None) + "\n");

                            Debug.WriteLine("\n\n허용된 권한 목록을 덤프합니다:");
                            Debug.WriteLine(@JsonConvert.SerializeObject(Permissions, Formatting.None));

                            Debug.WriteLine("\n\nvMenu 서버 설정 목록을 덤프합니다:");
                            var settings = new Dictionary<string, string>();
                            foreach (var a in Enum.GetValues(typeof(Setting)))
                            {
                                settings.Add(a.ToString(), GetSettingsString((Setting)a));
                            }
                            Debug.WriteLine(@JsonConvert.SerializeObject(settings, Formatting.None));
                            Debug.WriteLine("\nvMenu 덤프 종료!");
                            Debug.WriteLine("\n########################### vMenu ###########################");
                        }
                    }
                    else
                    {
                        Notify.Custom($"현재 실행 중인 vMenu 버전: {Version}.");
                    }
                }
            }), false);

            if (GetCurrentResourceName() != "vMenu")
            {
                MenuController.MainMenu = null;
                MenuController.DontOpenAnyMenu = true;
                MenuController.DisableMenuButtons = true;
                throw new Exception("\n[vMenu] 설치 오류!\n리소스 이름이 올바르지 않습니다. 폴더 이름을 '" + GetCurrentResourceName() + "'에서 'vMenu'(대소문자 구분)로 변경하세요!\n");
            }
            else
            {
                Tick += OnTick;
            }

            // Clear all previous pause menu info/brief messages on resource start.
            ClearBrief();

            if (GlobalState.Get("vmenu_onesync") ?? false)
            {
                PlayersList = new InfinityPlayerList(Players);
            }
        }

        #region Infinity bits
        [EventHandler("vMenu:ReceivePlayerList")]
        public void ReceivedPlayerList(IList<object> players)
        {
            PlayersList?.ReceivedPlayerList(players);
        }

        struct RPCData
        {
            public bool IsCompleted { get; set; }
            public Vector3 Coords { get; set; }
        }

        private static Dictionary<long, RPCData> rpcQueue = new Dictionary<long, RPCData>();
        private static long rpcIdCounter = 0;

        [EventHandler("vMenu:GetPlayerCoords:reply")]
        public static void PlayerCoordinatesReceived(long rpcId, Vector3 coords)
        {
            if (rpcQueue.ContainsKey(rpcId))
            {
                var rpcItem = rpcQueue[rpcId];
                rpcItem.IsCompleted = true;
                rpcItem.Coords = coords;
                rpcQueue[rpcId] = rpcItem;
            }
            else
            {
                Debug.WriteLine($"[vMenu] 경고: 알 수 없는 RPC ID의 플레이어 좌표를 받았습니다: {rpcId}");
            }
        }

        public static async Task<Vector3> RequestPlayerCoordinates(int serverId)
        {
            long rpcId = rpcIdCounter++;
            rpcQueue.Add(rpcId, new RPCData { IsCompleted = false, Coords = Vector3.Zero });

            TriggerServerEvent("vMenu:GetPlayerCoords", rpcId, serverId);

            while (!rpcQueue[rpcId].IsCompleted)
            {
                await Delay(0);
            }

            Vector3 coords = rpcQueue[rpcId].Coords;
            rpcQueue.Remove(rpcId);

            return coords;
        }
        #endregion

        #region Set Permissions function
        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        public static async void SetPermissions(string permissionsList)
        {
            vMenuShared.PermissionsManager.SetPermissions(permissionsList);

            VehicleSpawner.allowedCategories = new List<bool>()
            {
                IsAllowed(Permission.VSCompacts, checkAnyway: true),
                IsAllowed(Permission.VSSedans, checkAnyway: true),
                IsAllowed(Permission.VSSUVs, checkAnyway: true),
                IsAllowed(Permission.VSCoupes, checkAnyway: true),
                IsAllowed(Permission.VSMuscle, checkAnyway: true),
                IsAllowed(Permission.VSSportsClassic, checkAnyway: true),
                IsAllowed(Permission.VSSports, checkAnyway: true),
                IsAllowed(Permission.VSSuper, checkAnyway: true),
                IsAllowed(Permission.VSMotorcycles, checkAnyway: true),
                IsAllowed(Permission.VSOffRoad, checkAnyway: true),
                IsAllowed(Permission.VSIndustrial, checkAnyway: true),
                IsAllowed(Permission.VSUtility, checkAnyway: true),
                IsAllowed(Permission.VSVans, checkAnyway: true),
                IsAllowed(Permission.VSCycles, checkAnyway: true),
                IsAllowed(Permission.VSBoats, checkAnyway: true),
                IsAllowed(Permission.VSHelicopters, checkAnyway: true),
                IsAllowed(Permission.VSPlanes, checkAnyway: true),
                IsAllowed(Permission.VSService, checkAnyway: true),
                IsAllowed(Permission.VSEmergency, checkAnyway: true),
                IsAllowed(Permission.VSMilitary, checkAnyway: true),
                IsAllowed(Permission.VSCommercial, checkAnyway: true),
                IsAllowed(Permission.VSTrains, checkAnyway: true),
                IsAllowed(Permission.VSOpenWheel, checkAnyway: true)
            };
            ArePermissionsSetup = true;
            while (!ConfigOptionsSetupComplete)
            {
                await Delay(100);
            }
            PostPermissionsSetup();
        }
        #endregion

        /// <summary>
        /// This setups things as soon as the permissions are loaded.
        /// It triggers the menu creations, setting of initial flags like PVP, player stats,
        /// and triggers the creation of Tick functions from the FunctionsController class.
        /// </summary>
        private static void PostPermissionsSetup()
        {
            switch (GetSettingsInt(Setting.vmenu_pvp_mode))
            {
                case 1:
                    NetworkSetFriendlyFireOption(true);
                    SetCanAttackFriendly(Game.PlayerPed.Handle, true, false);
                    break;
                case 2:
                    NetworkSetFriendlyFireOption(false);
                    SetCanAttackFriendly(Game.PlayerPed.Handle, false, false);
                    break;
                case 0:
                default:
                    break;
            }

            static bool canUseMenu()
            {
                if (GetSettingsBool(Setting.vmenu_menu_staff_only) == false)
                {
                    return true;
                }
                else if (IsAllowed(Permission.Staff))
                {
                    return true;
                }

                return false;
            }

            if (!canUseMenu())
            {
                MenuController.MainMenu = null;
                MenuController.DisableMenuButtons = true;
                MenuController.DontOpenAnyMenu = true;
                MenuEnabled = false;
                ClientFullyDisabled = true;
                return;
            }
            // Create the main menu.
            Menu = new Menu(Game.Player.Name, "꿀벌서버 VMENU");
            PlayerSubmenu = new Menu(Game.Player.Name, "플레이어 메뉴");
            VehicleSubmenu = new Menu(Game.Player.Name, "차량 메뉴");
            WorldSubmenu = new Menu(Game.Player.Name, "월드 관련 메뉴");

            // Add the main menu to the menu pool.
            MenuController.AddMenu(Menu);
            MenuController.MainMenu = Menu;

            MenuController.AddSubmenu(Menu, PlayerSubmenu);
            MenuController.AddSubmenu(Menu, VehicleSubmenu);
            MenuController.AddSubmenu(Menu, WorldSubmenu);

            // Create all (sub)menus.
            CreateSubmenus();

            if (!GetSettingsBool(Setting.vmenu_disable_player_stats_setup))
            {
                // Manage Stamina
                if (PlayerOptionsMenu != null && PlayerOptionsMenu.PlayerStamina && IsAllowed(Permission.POUnlimitedStamina))
                {
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 100, true);
                }
                else
                {
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 0, true);
                }

                // Manage other stats, in order of appearance in the pause menu (stats) page.
                StatSetInt((uint)GetHashKey("MP0_SHOOTING_ABILITY"), 100, true);        // Shooting
                StatSetInt((uint)GetHashKey("MP0_STRENGTH"), 100, true);                // Strength
                StatSetInt((uint)GetHashKey("MP0_STEALTH_ABILITY"), 100, true);         // Stealth
                StatSetInt((uint)GetHashKey("MP0_FLYING_ABILITY"), 100, true);          // Flying
                StatSetInt((uint)GetHashKey("MP0_WHEELIE_ABILITY"), 100, true);         // Driving
                StatSetInt((uint)GetHashKey("MP0_LUNG_CAPACITY"), 100, true);           // Lung Capacity
                StatSetFloat((uint)GetHashKey("MP0_PLAYER_MENTAL_STATE"), 0f, true);    // Mental State
            }

            RegisterCommand($"vMenu:{GetKeyMappingId()}:MenuToggle", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                if (MenuEnabled)
                {
                    if (!MenuController.IsAnyMenuOpen())
                    {
                        Menu.OpenMenu();
                    }
                    else
                    {
                        MenuController.CloseAllMenus();
                    }
                }
            }), false);

            TriggerEvent("vMenu:SetupTickFunctions");
        }

        /// <summary>
        /// Main OnTick task runs every game tick and handles all the menu stuff.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (ClientFullyDisabled)
            {
                await Delay(10000);
                return;
            }
            // If the setup (permissions) is done, and it's not the first tick, then do this:
            if (ConfigOptionsSetupComplete)
            {
                #region Handle Opening/Closing of the menu.
                var tmpMenu = GetOpenMenu();
                if (MpPedCustomizationMenu != null)
                {
                    static bool IsOpen()
                    {
                        return
                            MpPedCustomizationMenu.appearanceMenu.Visible ||
                            MpPedCustomizationMenu.faceShapeMenu.Visible ||
                            MpPedCustomizationMenu.createCharacterMenu.Visible ||
                            MpPedCustomizationMenu.inheritanceMenu.Visible ||
                            MpPedCustomizationMenu.propsMenu.Visible ||
                            MpPedCustomizationMenu.clothesMenu.Visible ||
                            MpPedCustomizationMenu.tattoosMenu.Visible;
                    }

                    if (IsOpen())
                    {
                        if (tmpMenu == MpPedCustomizationMenu.createCharacterMenu)
                        {
                            MpPedCustomization.DisableBackButton = true;
                        }
                        else
                        {
                            MpPedCustomization.DisableBackButton = false;
                        }
                        MpPedCustomization.DontCloseMenus = true;
                    }
                    else
                    {
                        MpPedCustomization.DisableBackButton = false;
                        MpPedCustomization.DontCloseMenus = false;
                    }
                }

                if (Game.IsDisabledControlJustReleased(0, Control.PhoneCancel) && MpPedCustomization.DisableBackButton)
                {
                    await Delay(0);
                    Notify.Alert("종료하기 전에 먼저 페드를 저장해야 함. 원치 않은 경우 저장하지 않고 종료");
                }

                #endregion

                // Menu toggle button.
                //Game.DisableControlThisFrame(0, MenuToggleKey);
            }
        }

        #region Add Menu Function
        /// <summary>
        /// Add the menu to the menu pool and set it up correctly.
        /// Also add and bind the menu buttons.
        /// </summary>
        /// <param name="submenu"></param>
        /// <param name="menuButton"></param>
        private static void AddMenu(Menu parentMenu, Menu submenu, MenuItem menuButton)
        {
            parentMenu.AddMenuItem(menuButton);
            MenuController.AddSubmenu(parentMenu, submenu);
            MenuController.BindMenuItem(parentMenu, submenu, menuButton);
            submenu.RefreshIndex();
        }
        #endregion

        #region Create Submenus
        /// <summary>
        /// Creates all the submenus depending on the permissions of the user.
        /// </summary>
        private static void CreateSubmenus()
        {
            // Add the online players menu.
            if (IsAllowed(Permission.OPMenu))
            {
                OnlinePlayersMenu = new OnlinePlayers();
                var menu = OnlinePlayersMenu.GetMenu();
                var button = new MenuItem("접속 중인 플레이어", "현재 접속 중인 유저 목록을 볼 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
                Menu.OnItemSelect += async (sender, item, index) =>
                {
                    if (item == button)
                    {
                        PlayersList.RequestPlayerList();

                        await OnlinePlayersMenu.UpdatePlayerlist();
                        menu.RefreshIndex();
                    }
                };
            }
            if (IsAllowed(Permission.OPUnban) || IsAllowed(Permission.OPViewBannedPlayers))
            {
                BannedPlayersMenu = new BannedPlayers();
                var menu = BannedPlayersMenu.GetMenu();
                var button = new MenuItem("밴 목록", "이 메뉴에서 차단된 플레이어 목록을 확인하고 관리할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                        menu.RefreshIndex();
                    }
                };
            }

            var playerSubmenuBtn = new MenuItem("플레이어 관련 옵션", "플레이어 관련 하위 메뉴를 엽니다.") { Label = "→→→" };
            Menu.AddMenuItem(playerSubmenuBtn);

            // Add the player options menu.
            if (IsAllowed(Permission.POMenu))
            {
                PlayerOptionsMenu = new PlayerOptions();
                var menu = PlayerOptionsMenu.GetMenu();
                var button = new MenuItem("플레이어 옵션", "자주 사용하는 플레이어 옵션을 여기서 사용할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            var vehicleSubmenuBtn = new MenuItem("차량 관련 옵션", "차량 관련 하위 메뉴를 엽니다.") { Label = "→→→" };
            Menu.AddMenuItem(vehicleSubmenuBtn);
            // Add the vehicle options Menu.
            if (IsAllowed(Permission.VOMenu))
            {
                VehicleOptionsMenu = new VehicleOptions();
                var menu = VehicleOptionsMenu.GetMenu();
                var button = new MenuItem("차량 옵션", "여기에서 일반 차량 옵션을 변경하고, 차량 튜닝 및 스타일 설정을 할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add the vehicle spawner menu.
            if (IsAllowed(Permission.VSMenu))
            {
                VehicleSpawnerMenu = new VehicleSpawner();
                var menu = VehicleSpawnerMenu.GetMenu();
                var button = new MenuItem("차량 스포너", "차량 이름으로 스폰하거나 특정 카테고리에서 차량을 선택할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add Saved Vehicles menu.
            if (IsAllowed(Permission.SVMenu))
            {
                SavedVehiclesMenu = new SavedVehicles();
                var menu = SavedVehiclesMenu.GetTypeMenu();
                var button = new MenuItem("저장된 차량", "새 차량을 저장하거나, 이미 저장된 차량을 스폰 또는 삭제할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add the Personal Vehicle menu.
            if (IsAllowed(Permission.PVMenu))
            {
                PersonalVehicleMenu = new PersonalVehicle();
                var menu = PersonalVehicleMenu.GetMenu();
                var button = new MenuItem("개인 차량", "차량을 개인 차량으로 지정하고, 탑승하지 않은 상태에서도 일부 기능을 제어할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add the player appearance menu.
            if (IsAllowed(Permission.PAMenu))
            {
                PlayerAppearanceMenu = new PlayerAppearance();
                var menu = PlayerAppearanceMenu.GetMenu();
                var button = new MenuItem("플레이어 외형", "페드 모델을 선택하고 커스터마이즈한 뒤, 커스터마이즈한 캐릭터를 저장하거나 불러올 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);

                MpPedCustomizationMenu = new MpPedCustomization();
                var menu2 = MpPedCustomizationMenu.GetMenu();
                var button2 = new MenuItem("MP 페드 커스터마이징", "멀티플레이어 페드를 생성, 수정, 저장, 불러오기 할 수 있습니다. ~r~참고: 이 하위 메뉴에서 만든 페드만 저장할 수 있습니다. GTA의 제한으로 인해 vMenu는 이 메뉴 밖에서 생성된 페드는 감지할 수 없습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu2, button2);
            }

            var worldSubmenuBtn = new MenuItem("월드 관련 옵션", "월드 관련 하위 메뉴를 엽니다.") { Label = "→→→" };
            Menu.AddMenuItem(worldSubmenuBtn);

            // Add the time options menu.
            if (IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                TimeOptionsMenu = new TimeOptions();
                var menu = TimeOptionsMenu.GetMenu();
                var button = new MenuItem("시간 옵션", "시간을 변경하고 기타 시간 관련 옵션을 수정합니다.")
                {
                    Label = "→→→"
                };
                AddMenu(WorldSubmenu, menu, button);
            }

            // Add the weather options menu.
            if (IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                WeatherOptionsMenu = new WeatherOptions();
                var menu = WeatherOptionsMenu.GetMenu();
                var button = new MenuItem("날씨 옵션", "여기에서 모든 날씨 관련 옵션을 변경할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(WorldSubmenu, menu, button);
            }

            // Add the weapons menu.
            if (IsAllowed(Permission.WPMenu))
            {
                WeaponOptionsMenu = new WeaponOptions();
                var menu = WeaponOptionsMenu.GetMenu();
                var button = new MenuItem("무기 옵션", "무기를 추가/제거하고, 무기를 수정하거나 탄약 옵션을 설정할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            // Add Weapon Loadouts menu.
            if (IsAllowed(Permission.WLMenu))
            {
                WeaponLoadoutsMenu = new WeaponLoadouts();
                var menu = WeaponLoadoutsMenu.GetMenu();
                var button = new MenuItem("무기 로드아웃", "저장된 무기 로드아웃을 관리하고 불러올 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            if (IsAllowed(Permission.NoClip))
            {
                var toggleNoclip = new MenuItem("노클립 전환", "노클립 기능을 켜거나 끕니다.");
                PlayerSubmenu.AddMenuItem(toggleNoclip);
                PlayerSubmenu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == toggleNoclip)
                    {
                        NoClipEnabled = !NoClipEnabled;
                    }
                };
            }

            // Add Voice Chat Menu.
            if (IsAllowed(Permission.VCMenu))
            {
                VoiceChatSettingsMenu = new VoiceChat();
                var menu = VoiceChatSettingsMenu.GetMenu();
                var button = new MenuItem("보이스 채팅 설정", "여기에서 보이스 채팅 옵션을 변경할 수 있습니다.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            {
                RecordingMenu = new Recording();
                var menu = RecordingMenu.GetMenu();
                var button = new MenuItem("녹화 옵션", "인게임 녹화 옵션입니다.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            // Add misc settings menu.
            {
                MiscSettingsMenu = new MiscSettings();
                var menu = MiscSettingsMenu.GetMenu();
                var button = new MenuItem("기타 설정", "각종 vMenu 옵션/설정을 여기에서 변경할 수 있습니다. 이 메뉴에서 설정 저장도 가능합니다.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            // Add About Menu.
            AboutMenu = new About();
            var sub = AboutMenu.GetMenu();
            var btn = new MenuItem("vMenu 정보", "vMenu에 대한 정보입니다.")
            {
                Label = "→→→"
            };
            AddMenu(Menu, sub, btn);

            // Refresh everything.
            MenuController.Menus.ForEach((m) => m.RefreshIndex());

            if (!GetSettingsBool(Setting.vmenu_use_permissions))
            {
                Notify.Alert("vMenu가 권한을 무시하도록 설정되어 있습니다. 기본 권한이 사용됩니다.");
            }

            if (PlayerSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, PlayerSubmenu, playerSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(playerSubmenuBtn);
            }

            if (VehicleSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, VehicleSubmenu, vehicleSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(vehicleSubmenuBtn);
            }

            if (WorldSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, WorldSubmenu, worldSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(worldSubmenuBtn);
            }

            if (MiscSettingsMenu != null)
            {
                MenuController.EnableMenuToggleKeyOnController = !MiscSettingsMenu.MiscDisableControllerSupport;
            }
        }
        #endregion

        #region Utilities
        private static string GetKeyMappingId() => string.IsNullOrWhiteSpace(GetSettingsString(Setting.vmenu_keymapping_id)) ? "Default" : GetSettingsString(Setting.vmenu_keymapping_id);
        #endregion
    }
}
