using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;
        public static Dictionary<uint, Dictionary<int, string>> VehicleExtras;

        // Submenus
        public Menu VehicleModMenu { get; private set; }
        public Menu VehicleDoorsMenu { get; private set; }
        public Menu VehicleWindowsMenu { get; private set; }
        public Menu VehicleComponentsMenu { get; private set; }
        public Menu VehicleLiveriesMenu { get; private set; }
        public Menu VehicleColorsMenu { get; private set; }
        public Menu DeleteConfirmMenu { get; private set; }
        public Menu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleGodInvincible { get; private set; } = UserDefaults.VehicleGodInvincible;
        public bool VehicleGodEngine { get; private set; } = UserDefaults.VehicleGodEngine;
        public bool VehicleGodVisual { get; private set; } = UserDefaults.VehicleGodVisual;
        public bool VehicleGodStrongWheels { get; private set; } = UserDefaults.VehicleGodStrongWheels;
        public bool VehicleGodRamp { get; private set; } = UserDefaults.VehicleGodRamp;
        public bool VehicleGodAutoRepair { get; private set; } = UserDefaults.VehicleGodAutoRepair;

        public bool VehicleNeverDirty { get; private set; } = UserDefaults.VehicleNeverDirty;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool DisablePlaneTurbulence { get; private set; } = UserDefaults.VehicleDisablePlaneTurbulence;
        public bool DisableHelicopterTurbulence { get; private set; } = UserDefaults.VehicleDisableHelicopterTurbulence;
        public bool AnchorBoat { get; private set; } = UserDefaults.VehicleAnchorBoat;
        public bool VehicleBikeSeatbelt { get; private set; } = UserDefaults.VehicleBikeSeatbelt;
        public bool VehicleInfiniteFuel { get; private set; } = false;
        public bool VehicleShowHealth { get; private set; } = false;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private readonly Dictionary<MenuItem, int> vehicleExtras = new();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "차량 옵션");

            #region menu items variables
            // vehicle god mode menu
            var vehGodMenu = new Menu("차량 무적 모드", "차량 무적 설정");
            var vehGodMenuBtn = new MenuItem("무적 옵션", "특정 피해 유형을 켜거나 끕니다.") { Label = "→→→" };
            MenuController.AddSubmenu(menu, vehGodMenu);

            // Create Checkboxes.
            var vehicleGod = new MenuCheckboxItem("차량 무적", "차량이 피해를 받지 않게 합니다. 아래 무적 설정 메뉴에서 어떤 피해를 막을지 선택할 수 있습니다.", VehicleGodMode);
            var vehicleNeverDirty = new MenuCheckboxItem("차량 항상 깨끗하게", "차량 오염도가 0보다 높아지면 계속 세차합니다. 단, ~o~먼지~s~나 ~o~흙먼지~s~만 제거됩니다. 진흙, 눈, 기타 ~r~손상 자국~s~은 지워지지 않으므로 제거하려면 차량을 수리하세요.", VehicleNeverDirty);
            var vehicleBikeSeatbelt = new MenuCheckboxItem("오토바이 안전장치", "오토바이, 자전거, ATV 등에서 떨어지지 않게 합니다.", VehicleBikeSeatbelt);
            var vehicleEngineAO = new MenuCheckboxItem("엔진 항상 켜짐", "차량에서 내려도 엔진이 꺼지지 않습니다.", VehicleEngineAlwaysOn);
            var vehicleNoTurbulence = new MenuCheckboxItem("비행기 난기류 비활성화", "모든 비행기의 난기류를 비활성화합니다.", DisablePlaneTurbulence);
            var vehicleNoTurbulenceHeli = new MenuCheckboxItem("헬리콥터 난기류 비활성화", "모든 헬리콥터의 난기류를 비활성화합니다.", DisableHelicopterTurbulence);
            var vehicleSetAnchor = new MenuCheckboxItem("보트 정박", "현재 차량이 보트이고 정박 가능한 위치일 때만 작동합니다.", AnchorBoat);
            var vehicleNoSiren = new MenuCheckboxItem("사이렌 비활성화", "차량의 사이렌을 끕니다. 사이렌이 있는 차량에서만 작동합니다.", VehicleNoSiren);
            var vehicleNoBikeHelmet = new MenuCheckboxItem("헬멧 자동 착용 끄기", "오토바이 또는 ATV 탑승 시 헬멧을 자동으로 착용하지 않습니다.", VehicleNoBikeHelemet);
            var vehicleFreeze = new MenuCheckboxItem("차량 고정", "차량 위치를 고정합니다.", VehicleFrozen);
            var torqueEnabled = new MenuCheckboxItem("토크 배율 활성화", "아래 목록에서 선택한 토크 배율을 활성화합니다.", VehicleTorqueMultiplier);
            var powerEnabled = new MenuCheckboxItem("출력 배율 활성화", "아래 목록에서 선택한 출력 배율을 활성화합니다.", VehiclePowerMultiplier);
            var highbeamsOnHonk = new MenuCheckboxItem("경적 시 상향등 점멸", "경적을 울릴 때 상향등이 점멸합니다. 낮에 라이트가 꺼져 있으면 작동하지 않습니다.", FlashHighbeamsOnHonk);
            var showHealth = new MenuCheckboxItem("차량 내구도 표시", "화면에 차량 내구도를 표시합니다.", VehicleShowHealth);
            var infiniteFuel = new MenuCheckboxItem("무한 연료", "이 차량의 무한 연료를 켜거나 끕니다. FRFuel이 설치되어 있을 때만 작동합니다.", VehicleInfiniteFuel);

            // Create buttons.
            var fixVehicle = new MenuItem("차량 수리", "차량의 외형 및 물리적 손상을 모두 수리합니다.");
            var cleanVehicle = new MenuItem("차량 세차", "차량을 세차합니다.");
            var toggleEngine = new MenuItem("엔진 켜기/끄기", "엔진을 켜거나 끕니다.");
            var setLicensePlateText = new MenuItem("번호판 문자 설정", "차량 번호판 문구를 직접 입력합니다.");
            var modMenuBtn = new MenuItem("튜닝 메뉴", "여기서 차량을 튜닝하고 꾸밀 수 있습니다.")
            {
                Label = "→→→"
            };
            var doorsMenuBtn = new MenuItem("차량 문", "차량 문을 열고, 닫고, 제거하고, 복구할 수 있습니다.")
            {
                Label = "→→→"
            };
            var windowsMenuBtn = new MenuItem("차량 창문", "창문을 올리거나 내리고, 제거하거나 복구할 수 있습니다.")
            {
                Label = "→→→"
            };
            var componentsMenuBtn = new MenuItem("차량 추가옵션", "차량 부품 및 추가옵션을 추가/제거합니다.")
            {
                Label = "→→→"
            };
            var liveriesMenuBtn = new MenuItem("차량 리버리", "차량에 다양한 리버리를 적용합니다!")
            {
                Label = "→→→"
            };
            var colorsMenuBtn = new MenuItem("차량 색상", "멋진 색상을 적용해 차량을 더 꾸며보세요!")
            {
                Label = "→→→"
            };
            var underglowMenuBtn = new MenuItem("차량 네온 키트", "화려한 네온 언더글로우로 차량을 빛나게 해보세요!")
            {
                Label = "→→→"
            };
            var vehicleInvisible = new MenuItem("차량 표시/숨김 전환", "차량을 보이게/숨기게 전환합니다. ~r~차량에서 내리면 다시 보이게 됩니다. 그렇지 않으면 다시 탑승할 수 없습니다.");
            var flipVehicle = new MenuItem("차량 바로 세우기", "현재 차량을 네 바퀴로 바로 세웁니다.");
            var vehicleAlarm = new MenuItem("차량 경보 전환", "차량 경보를 켜거나 끕니다.");
            var cycleSeats = new MenuItem("좌석 순환 이동", "탑승 가능한 좌석을 순서대로 이동합니다.");
            var lights = new List<string>()
            {
                "비상등",
                "좌측 방향지시등",
                "우측 방향지시등",
                "실내등",
                //"Taxi Light", // this doesn't seem to work no matter what.
                "헬기 탐조등",
            };
            var vehicleLights = new MenuListItem("차량 라이트", lights, 0, "차량 라이트를 켜거나 끕니다.");

            var stationNames = new List<string>();

            foreach (var radioStationName in Enum.GetNames(typeof(RadioStation)))
            {
                stationNames.Add(radioStationName);
            }

            var radioIndex = UserDefaults.VehicleDefaultRadio;

            if (radioIndex == (int)RadioStation.RadioOff)
            {
                var stations = (RadioStation[])Enum.GetValues(typeof(RadioStation));
                var index = Array.IndexOf(stations, RadioStation.RadioOff);
                radioIndex = index;
            }

            var radioStations = new MenuListItem("기본 라디오 채널", stationNames, radioIndex, "새 차량 생성 시 적용할 기본 라디오 채널을 선택합니다.");

            var tiresList = new List<string>() { "모든 타이어", "타이어 #1", "타이어 #2", "타이어 #3", "타이어 #4", "타이어 #5", "타이어 #6", "타이어 #7", "타이어 #8" };
            var vehicleTiresList = new MenuListItem("타이어 수리 / 파괴", tiresList, 0, "특정 타이어 하나 또는 전체 타이어를 한 번에 수리하거나 파괴합니다. 모든 차량에서 모든 인덱스가 유효한 것은 아니며, 일부 차량에서는 동작하지 않을 수 있습니다.");

            var destroyEngine = new MenuItem("엔진 파괴", "차량 엔진을 파괴합니다.");

            var deleteBtn = new MenuItem("~r~차량 삭제", "차량을 삭제합니다. 이 작업은 ~r~되돌릴 수 없습니다~s~!")
            {
                LeftIcon = MenuItem.Icon.WARNING,
                Label = "→→→"
            };
            var deleteNoBtn = new MenuItem("아니오, 취소", "아니오, 차량을 삭제하지 않고 돌아갑니다!");
            var deleteYesBtn = new MenuItem("~r~예, 삭제", "예, 차량을 삭제합니다. 이 작업이 되돌릴 수 없다는 점을 이해했습니다.")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };

            // Create lists.
            var dirtlevel = new List<string> { "오염 없음", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            var setDirtLevel = new MenuListItem("오염도 설정", dirtlevel, 0, "Select how much dirt should be visible on your vehicle, press ~r~enter~s~ " +
                "to apply the selected level.");
            var licensePlates = new List<string> {
                GetLabelText("CMOD_PLA_0"), // Plate Index 0 // BlueOnWhite1
                GetLabelText("CMOD_PLA_1"), // Plate Index 1 // BlueOnWhite2
                GetLabelText("CMOD_PLA_2"), // Plate Index 2 // BlueOnWhite3
                GetLabelText("CMOD_PLA_3"), // Plate Index 3 // YellowOnBlue
                GetLabelText("CMOD_PLA_4"), // Plate Index 4 // YellowOnBlack
                GetLabelText("PROL"), // Plate Index 5 // NorthYankton
                GetLabelText("CMOD_PLA_6"), // Plate Index 6 // ECola
                GetLabelText("CMOD_PLA_7"), // Plate Index 7 // LasVenturas
                GetLabelText("CMOD_PLA_8"), // Plate Index 8 // LibertyCity
                GetLabelText("CMOD_PLA_9"), // Plate Index 9 // LSCarMeet
                GetLabelText("CMOD_PLA_10"), // Plate Index 10 // LSPanic
                GetLabelText("CMOD_PLA_11"), // Plate Index 11 // LSPounders
                GetLabelText("CMOD_PLA_12"), // Plate Index 12 // Sprunk
            };
            var setLicensePlateType = new MenuListItem("번호판 종류", licensePlates, 0, "Choose a license plate type and press ~r~enter ~s~to apply " +
                "it to your vehicle.");
            var torqueMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var torqueMultiplier = new MenuListItem("엔진 토크 배율 설정", torqueMultiplierList, 0, "엔진 토크 배율을 설정합니다.");
            var powerMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var powerMultiplier = new MenuListItem("엔진 출력 배율 설정", powerMultiplierList, 0, "엔진 출력 배율을 설정합니다.");
            var speedLimiterOptions = new List<string>() { "설정", "초기화", "사용자 지정 속도 제한" };
            var speedLimiter = new MenuListItem("속도 제한기", speedLimiterOptions, 0, "현재 ~y~주행 속도~s~를 차량의 최고 속도로 제한합니다. 초기화하면 현재 차량의 최고 속도가 기본값으로 돌아갑니다. 이 옵션은 현재 차량에만 적용됩니다.");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new Menu("튜닝 메뉴", "차량 튜닝");
            VehicleModMenu.InstructionalButtons.Add(Control.Jump, "차량 문 열기/닫기");
            VehicleModMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_PRESSED, new Action<Menu, Control>((m, c) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var open = GetVehicleDoorAngleRatio(veh.Handle, 0) < 0.1f;
                    if (open)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            SetVehicleDoorOpen(veh.Handle, i, false, false);
                        }
                    }
                    else
                    {
                        SetVehicleDoorsShut(veh.Handle, false);
                    }
                }
            }), false));
            VehicleDoorsMenu = new Menu("차량 문", "차량 문 관리");
            VehicleWindowsMenu = new Menu("차량 창문", "차량 창문 관리");
            VehicleComponentsMenu = new Menu("차량 추가옵션", "차량 추가옵션/부품");
            VehicleLiveriesMenu = new Menu("차량 리버리", "차량 리버리");
            VehicleColorsMenu = new Menu("차량 색상", "차량 색상");
            DeleteConfirmMenu = new Menu("작업 확인", "차량을 삭제하시겠습니까?");
            VehicleUnderglowMenu = new Menu("차량 네온 키트", "차량 네온 언더글로우 설정");

            MenuController.AddSubmenu(menu, VehicleModMenu);
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.AddSubmenu(menu, VehicleWindowsMenu);
            MenuController.AddSubmenu(menu, VehicleComponentsMenu);
            MenuController.AddSubmenu(menu, VehicleLiveriesMenu);
            MenuController.AddSubmenu(menu, VehicleColorsMenu);
            MenuController.AddSubmenu(menu, DeleteConfirmMenu);
            MenuController.AddSubmenu(menu, VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddMenuItem(vehicleGod);
                menu.AddMenuItem(vehGodMenuBtn);
                MenuController.BindMenuItem(menu, vehGodMenu, vehGodMenuBtn);

                var godInvincible = new MenuCheckboxItem("완전 무적", "차량을 완전 무적으로 만듭니다. 화재, 폭발, 충돌 등 대부분의 피해가 포함됩니다.", VehicleGodInvincible);
                var godEngine = new MenuCheckboxItem("엔진 손상 방지", "엔진이 피해를 받지 않게 합니다.", VehicleGodEngine);
                var godVisual = new MenuCheckboxItem("외형 손상 방지", "차량에 흠집 및 기타 손상 자국이 생기지 않게 합니다. 단, 차체 변형까지 막지는 않습니다.", VehicleGodVisual);
                var godStrongWheels = new MenuCheckboxItem("강화 휠", "휠 변형으로 인해 조작성이 낮아지는 것을 막습니다. 타이어가 방탄이 되지는 않습니다.", VehicleGodStrongWheels);
                var godRamp = new MenuCheckboxItem("램프 손상 방지", "Ramp Buggy 같은 차량이 램프 사용 시 피해를 받지 않게 합니다.", VehicleGodRamp);
                var godAutoRepair = new MenuCheckboxItem("~r~자동 수리", "어떤 종류의 손상이든 발생하면 차량을 자동 수리합니다. 버그성 동작을 막기 위해 보통은 꺼두는 것이 좋습니다.", VehicleGodAutoRepair);

                vehGodMenu.AddMenuItem(godInvincible);
                vehGodMenu.AddMenuItem(godEngine);
                vehGodMenu.AddMenuItem(godVisual);
                vehGodMenu.AddMenuItem(godStrongWheels);
                vehGodMenu.AddMenuItem(godRamp);
                vehGodMenu.AddMenuItem(godAutoRepair);

                vehGodMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == godInvincible)
                    {
                        VehicleGodInvincible = _checked;
                    }
                    else if (item == godEngine)
                    {
                        VehicleGodEngine = _checked;
                    }
                    else if (item == godVisual)
                    {
                        VehicleGodVisual = _checked;
                    }
                    else if (item == godStrongWheels)
                    {
                        VehicleGodStrongWheels = _checked;
                    }
                    else if (item == godRamp)
                    {
                        VehicleGodRamp = _checked;
                    }
                    else if (item == godAutoRepair)
                    {
                        VehicleGodAutoRepair = _checked;
                    }
                };

            }
            if (IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddMenuItem(fixVehicle);
            }
            if (IsAllowed(Permission.VOKeepClean))
            {
                menu.AddMenuItem(vehicleNeverDirty);
            }
            if (IsAllowed(Permission.VOWash))
            {
                menu.AddMenuItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddMenuItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddMenuItem(modMenuBtn);
            }
            if (IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddMenuItem(colorsMenuBtn);
            }
            if (IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddMenuItem(underglowMenuBtn);
                MenuController.BindMenuItem(menu, VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddMenuItem(liveriesMenuBtn);
            }
            if (IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddMenuItem(componentsMenuBtn);
            }
            if (IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddMenuItem(toggleEngine);
            }
            if (IsAllowed(Permission.VOChangePlate))
            {
                menu.AddMenuItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddMenuItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddMenuItem(doorsMenuBtn);
            }
            if (IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddMenuItem(windowsMenuBtn);
            }
            if (IsAllowed(Permission.VOBikeSeatbelt))
            {
                menu.AddMenuItem(vehicleBikeSeatbelt);
            }
            if (IsAllowed(Permission.VOSpeedLimiter)) // SPEED LIMITER
            {
                menu.AddMenuItem(speedLimiter);
            }
            if (IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddMenuItem(torqueEnabled); // TORQUE ENABLED
                menu.AddMenuItem(torqueMultiplier); // TORQUE LIST
            }
            if (IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddMenuItem(powerEnabled); // POWER ENABLED
                menu.AddMenuItem(powerMultiplier); // POWER LIST
            }
            if (IsAllowed(Permission.VODisableTurbulence))
            {
                menu.AddMenuItem(vehicleNoTurbulence);
                menu.AddMenuItem(vehicleNoTurbulenceHeli);
            }
            if (IsAllowed(Permission.VOAnchorBoat))
            {
                menu.AddMenuItem(vehicleSetAnchor);
            }
            if (IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddMenuItem(flipVehicle);
            }
            if (IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddMenuItem(vehicleAlarm);
            }
            if (IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddMenuItem(cycleSeats);
            }
            if (IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddMenuItem(vehicleLights);
            }
            if (IsAllowed(Permission.VOFixOrDestroyTires))
            {
                menu.AddMenuItem(vehicleTiresList);
            }
            if (IsAllowed(Permission.VODestroyEngine))
            {
                menu.AddMenuItem(destroyEngine);
            }
            if (IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddMenuItem(vehicleFreeze);
            }
            if (IsAllowed(Permission.VOInvisible)) // MAKE VEHICLE INVISIBLE
            {
                menu.AddMenuItem(vehicleInvisible);
            }
            if (IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddMenuItem(vehicleEngineAO);
            }
            if (IsAllowed(Permission.VOInfiniteFuel)) // INFINITE FUEL
            {
                menu.AddMenuItem(infiniteFuel);
            }
            // always allowed
            menu.AddMenuItem(showHealth); // SHOW VEHICLE HEALTH

            // I don't really see why would you want to disable this so I will not add useless permissions
            menu.AddMenuItem(radioStations);

            if (IsAllowed(Permission.VONoSiren) && !GetSettingsBool(Setting.vmenu_use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddMenuItem(vehicleNoSiren);
            }
            if (IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddMenuItem(vehicleNoBikeHelmet);
            }
            if (IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddMenuItem(highbeamsOnHonk);
            }

            if (IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddMenuItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddMenuItem(deleteNoBtn);
            DeleteConfirmMenu.AddMenuItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    var veh = GetVehicle();
                    if (veh != null && veh.Exists() && GetVehicle().Driver == Game.PlayerPed)
                    {
                        SetVehicleHasBeenOwnedByPlayer(veh.Handle, false);
                        SetEntityAsMissionEntity(veh.Handle, false, false);
                        veh.Delete();
                    }
                    else
                    {
                        if (!Game.PlayerPed.IsInVehicle())
                        {
                            Notify.Alert(CommonErrors.NoVehicle);
                        }
                        else
                        {
                            Notify.Alert("차량을 삭제하려면 운전석에 앉아 있어야 합니다.");
                        }

                    }
                    DeleteConfirmMenu.GoBack();
                    menu.GoBack();
                }
            };
            #endregion

            #region Bind Submenus to their buttons.
            MenuController.BindMenuItem(menu, VehicleModMenu, modMenuBtn);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleWindowsMenu, windowsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleComponentsMenu, componentsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleLiveriesMenu, liveriesMenuBtn);
            MenuController.BindMenuItem(menu, VehicleColorsMenu, colorsMenuBtn);
            MenuController.BindMenuItem(menu, DeleteConfirmMenu, deleteBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteBtn) // reset the index so that "no" / "cancel" will always be selected by default.
                {
                    DeleteConfirmMenu.RefreshIndex();
                }
                // If the player is actually in a vehicle, continue.
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    // Create a vehicle object.
                    var vehicle = GetVehicle();

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.PlayerPed)
                    {
                        // Repair vehicle.
                        if (item == fixVehicle)
                        {
                            vehicle.Repair();
                        }
                        // Clean vehicle.
                        else if (item == cleanVehicle)
                        {
                            vehicle.Wash();
                        }
                        // Flip vehicle.
                        else if (item == flipVehicle)
                        {
                            SetVehicleOnGroundProperly(vehicle.Handle);
                        }
                        // Toggle alarm.
                        else if (item == vehicleAlarm)
                        {
                            ToggleVehicleAlarm(vehicle);
                        }
                        // Toggle engine
                        else if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(vehicle.Handle, !vehicle.IsEngineRunning, false, true);
                        }
                        // Set license plate text
                        else if (item == setLicensePlateText)
                        {
                            SetLicensePlateCustomText();
                        }
                        // Make vehicle invisible.
                        else if (item == vehicleInvisible)
                        {
                            if (vehicle.IsVisible)
                            {
                                // Check the visibility of all peds inside before setting the vehicle as invisible.
                                var visiblePeds = new Dictionary<Ped, bool>();
                                foreach (var p in vehicle.Occupants)
                                {
                                    visiblePeds.Add(p, p.IsVisible);
                                }

                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;

                                // Restore visibility for each ped.
                                foreach (var pe in visiblePeds)
                                {
                                    pe.Key.IsVisible = pe.Value;
                                }
                            }
                            else
                            {
                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;
                            }
                        }
                        // Destroy vehicle engine
                        else if (item == destroyEngine)
                        {
                            SetVehicleEngineHealth(vehicle.Handle, -4000);
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("이 메뉴를 사용하려면 차량 운전석에 있어야 합니다!", true, false);
                    }

                    // Cycle vehicle seats
                    if (item == cycleSeats)
                    {
                        CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // Create a vehicle object.
                var vehicle = GetVehicle();

                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                    if (!_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            FreezeEntityPosition(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == torqueEnabled) // Enable Torque Multiplier Toggled
                {
                    VehicleTorqueMultiplier = _checked;
                }
                else if (item == powerEnabled) // Enable Power Multiplier Toggled
                {
                    VehiclePowerMultiplier = _checked;
                    if (_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == showHealth) // show vehicle health on screen.
                {
                    VehicleShowHealth = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    if (vehicle != null && vehicle.Exists())
                    {
                        vehicle.IsSirenSilent = _checked;
                    }
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
                else if (item == vehicleNoTurbulence)
                {
                    DisablePlaneTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence)
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                    }
                }
                else if (item == vehicleNoTurbulenceHeli)
                {
                    DisableHelicopterTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsHelicopter)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisableHelicopterTurbulence)
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleSetAnchor)
                {
                    AnchorBoat = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsBoat && CanAnchorBoatHere(vehicle.Handle))
                    {
                        if (MainMenu.VehicleOptionsMenu.AnchorBoat)
                        {
                            SetBoatAnchor(vehicle.Handle, true);
                            SetBoatFrozenWhenAnchored(vehicle.Handle, true);
                            SetForcedBoatLocationWhenAnchored(vehicle.Handle, true);
                        }
                        else
                        {
                            SetBoatAnchor(vehicle.Handle, false);
                            SetBoatFrozenWhenAnchored(vehicle.Handle, false);
                            SetForcedBoatLocationWhenAnchored(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == vehicleNeverDirty)
                {
                    VehicleNeverDirty = _checked;
                }
                else if (item == vehicleBikeSeatbelt)
                {
                    VehicleBikeSeatbelt = _checked;
                }
                else if (item == infiniteFuel)
                {
                    VehicleInfiniteFuel = _checked;
                    TriggerEvent("vMenu:InfiniteFuelToggled", _checked);
                }
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    var veh = GetVehicle();
                    // If the torque multiplier changed. Change the torque multiplier to the new value.
                    if (item == torqueMultiplier)
                    {
                        // Get the selected value and remove the "x" in the string with nothing.
                        var value = torqueMultiplierList[newIndex].ToString().Replace("x", "");
                        // Convert the value to a float and set it as a public variable.
                        VehicleTorqueMultiplierAmount = float.Parse(value);
                    }
                    // If the power multiplier is changed. Change the power multiplier to the new value.
                    else if (item == powerMultiplier)
                    {
                        // Get the selected value. Remove the "x" from the string.
                        var value = powerMultiplierList[newIndex].ToString().Replace("x", "");
                        // Conver the string into a float and set it to be the value of the public variable.
                        VehiclePowerMultiplierAmount = float.Parse(value);
                        if (VehiclePowerMultiplier)
                        {
                            SetVehicleEnginePowerMultiplier(veh.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else if (item == setLicensePlateType)
                    {
                        // Set the license plate style.
                        switch (newIndex)
                        {
                            case 0:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            case 6:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.ECola;
                                break;
                            case 7:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LasVenturas;
                                break;
                            case 8:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LibertyCity;
                                break;
                            case 9:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSCarMeet;
                                break;
                            case 10:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSPanic;
                                break;
                            case 11:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.LSPounders;
                                break;
                            case 12:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.Sprunk;
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListItemSelect += async (sender, item, listIndex, itemIndex) =>
            {
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        GetVehicle().DirtLevel = float.Parse(listIndex.ToString());
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        // We need to do % 4 because this seems to be some sort of flags system. For a taxi, this function returns 65, 66, etc.
                        // So % 4 takes care of that.
                        var state = GetVehicleIndicatorLights(veh.Handle) % 4; // 0 = none, 1 = left, 2 = right, 3 = both

                        if (listIndex == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 3) // Interior lights
                        {
                            SetVehicleInteriorlight(veh.Handle, !IsVehicleInteriorLightOn(veh.Handle));
                            //CommonFunctions.Log("Something cool here.");
                        }
                        //else if (listIndex == 4) // taxi light
                        //{
                        //    veh.IsTaxiLightOn = !veh.IsTaxiLightOn;
                        //    //    SetTaxiLights(veh, true);
                        //    //    SetTaxiLights(veh, false);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, true);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, false);
                        //    //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    //    CommonFunctions.Log
                        //}
                        else if (listIndex == 4) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh.Handle, !IsVehicleSearchlightOn(veh.Handle), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Speed Limiter
                else if (item == speedLimiter)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var vehicle = GetVehicle();

                        if (vehicle != null && vehicle.Exists())
                        {
                            if (listIndex == 0) // Set
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                SetEntityMaxSpeed(vehicle.Handle, vehicle.Speed);

                                if (ShouldUseMetricMeasurements()) // kph
                                {
                                    Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(vehicle.Speed * 3.6f, 1)} KPH~s~.");
                                }
                                else // mph
                                {
                                    Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(vehicle.Speed * 2.237f, 1)} MPH~s~.");
                                }

                            }
                            else if (listIndex == 1) // Reset
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f); // Default max speed seemingly for all vehicles.
                                Notify.Info("차량 속도 제한이 해제되었습니다.");
                            }
                            else if (listIndex == 2) // custom speed
                            {
                                var inputSpeed = await GetUserInput("속도를 입력하세요 (m/s)", "20.0", 5);
                                if (!string.IsNullOrEmpty(inputSpeed))
                                {
                                    if (float.TryParse(inputSpeed, out var outFloat))
                                    {
                                        //vehicle.MaxSpeed = outFloat;
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outFloat + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outFloat * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outFloat * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else if (int.TryParse(inputSpeed, out var outInt))
                                    {
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outInt + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outInt * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outInt * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Error("올바른 숫자가 아닙니다. m/s 단위의 유효한 속도를 입력하세요.");
                                    }
                                }
                                else
                                {
                                    Notify.Error(CommonErrors.InvalidInput);
                                }
                            }
                        }
                    }
                }
                else if (item == vehicleTiresList)
                {
                    //bool fix = item == vehicleTiresList;

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        if (Game.PlayerPed == veh.Driver)
                        {
                            if (listIndex == 0)
                            {
                                if (IsVehicleTyreBurst(veh.Handle, 0, false))
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreFixed(veh.Handle, i);
                                    }
                                    Notify.Success("모든 차량 타이어가 수리되었습니다.");
                                }
                                else
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreBurst(veh.Handle, i, false, 1f);
                                    }
                                    Notify.Success("모든 차량 타이어가 파괴되었습니다.");
                                }
                            }
                            else
                            {
                                var index = listIndex - 1;
                                if (IsVehicleTyreBurst(veh.Handle, index, false))
                                {
                                    SetVehicleTyreFixed(veh.Handle, index);
                                    Notify.Success($"Vehicle tyre #{listIndex} has been fixed.");
                                }
                                else
                                {
                                    SetVehicleTyreBurst(veh.Handle, index, false, 1f);
                                    Notify.Success($"Vehicle tyre #{listIndex} has been destroyed.");
                                }
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NeedToBeTheDriver);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (item == radioStations)
                {
                    var newStation = (RadioStation)Enum.GetValues(typeof(RadioStation)).GetValue(listIndex);

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        veh.RadioStation = newStation;
                    }

                    UserDefaults.VehicleDefaultRadio = (int)newStation;
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff
            // color customization menu
            var customizeColorMenu = new Menu("차량 색상", "색상 사용자 설정");
            MenuController.AddSubmenu(VehicleColorsMenu, customizeColorMenu);

            var colorsCustomizationBtn = new MenuItem("색상 사용자 설정") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(colorsCustomizationBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, customizeColorMenu, colorsCustomizationBtn);

            // primary menu
            var primaryColorsMenu = new Menu("차량 색상", "기본 색상");
            MenuController.AddSubmenu(customizeColorMenu, primaryColorsMenu);

            var primaryColorsBtn = new MenuItem("기본 색상") { Label = "→→→" };
            customizeColorMenu.AddMenuItem(primaryColorsBtn);
            MenuController.BindMenuItem(customizeColorMenu, primaryColorsMenu, primaryColorsBtn);

            // secondary menu
            var secondaryColorsMenu = new Menu("차량 색상", "보조 색상");
            MenuController.AddSubmenu(customizeColorMenu, secondaryColorsMenu);

            var secondaryColorsBtn = new MenuItem("보조 색상") { Label = "→→→" };
            customizeColorMenu.AddMenuItem(secondaryColorsBtn);
            MenuController.BindMenuItem(customizeColorMenu, secondaryColorsMenu, secondaryColorsBtn);

            var presetColorsBtn = new MenuListItem("프리셋 색상", [], 0);
            customizeColorMenu.AddMenuItem(presetColorsBtn);

            var chrome = new MenuItem("크롬");
            customizeColorMenu.AddMenuItem(chrome);

            // color lists
            var classic = new List<string>();
            var matte = new List<string>();
            var metals = new List<string>();
            var util = new List<string>();
            var worn = new List<string>();
            var chameleon = new List<string>();
            var wheelColors = new List<string>() { "기본 휠 색상" };

            // Just quick and dirty solution to put this in a new enclosed section so that we can still use 'i' as a counter in the other code parts.
            {
                var i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                {
                    i = 0;
                    foreach (var vc in VehicleData.ChameleonColors)
                    {
                        chameleon.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ChameleonColors.Count})");
                        i++;
                    }
                }

                wheelColors.AddRange(classic);
            }

            var wheelColorsList = new MenuListItem("휠 색상", wheelColors, 0);
            var dashColorList = new MenuListItem("대시보드 색상", classic, 0);
            var intColorList = new MenuListItem("실내 / 트림 색상", classic, 0);
            var vehicleEnveffScale = new MenuSliderItem("차량 Enveff 강도", "이 기능은 Besra 같은 일부 차량에서만 작동하며, 특정 도색 레이어를 흐리게 만듭니다.", 0, 20, 10, true);

            VehicleColorsMenu.AddMenuItem(vehicleEnveffScale);

            VehicleColorsMenu.OnSliderPositionChange += (m, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Driver == Game.PlayerPed && !veh.IsDead)
                {
                    if (sliderItem == vehicleEnveffScale)
                    {
                        SetVehicleEnveffScale(veh.Handle, newPosition / 20f);
                    }
                }
                else
                {
                    Notify.Error("이 슬라이더를 변경하려면 운전 가능한 차량의 운전석에 있어야 합니다.");
                }
            };

            VehicleColorsMenu.AddMenuItem(dashColorList);
            VehicleColorsMenu.AddMenuItem(intColorList);
            VehicleColorsMenu.AddMenuItem(wheelColorsList);

            VehicleColorsMenu.OnListIndexChange += HandleListIndexChanges;

            void HandleListIndexChanges(Menu sender, MenuListItem listItem, int oldIndex, int newIndex, int itemIndex)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var primaryColor = 0;
                    var secondaryColor = 0;
                    var pearlColor = 0;
                    var wheelColor = 0;
                    var dashColor = 0;
                    var intColor = 0;

                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleExtraColours(veh.Handle, ref pearlColor, ref wheelColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref intColor);

                    if (sender == primaryColorsMenu)
                    {
                        if (itemIndex == 2)
                        {
                            pearlColor = VehicleData.ClassicColors[newIndex].id;
                        }
                        else
                        {
                            pearlColor = 0;
                        }

                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                            case 2:
                                primaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 3:
                                primaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 4:
                                primaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 5:
                                primaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 6:
                                primaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }

                        if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                        {
                            if (itemIndex == 7)
                            {
                                primaryColor = VehicleData.ChameleonColors[newIndex].id;
                                secondaryColor = VehicleData.ChameleonColors[newIndex].id;

                                SetVehicleModKit(veh.Handle, 0);
                            }
                        }

                        ClearVehicleCustomPrimaryColour(veh.Handle);
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == secondaryColorsMenu)
                    {
                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                                pearlColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 2:
                            case 3:
                                secondaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 4:
                                secondaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 5:
                                secondaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 6:
                                secondaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 7:
                                secondaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }

                        ClearVehicleCustomSecondaryColour(veh.Handle);
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == VehicleColorsMenu)
                    {
                        if (listItem == wheelColorsList)
                        {
                            if (newIndex == 0)
                            {
                                wheelColor = 156; // default alloy color.
                            }
                            else
                            {
                                wheelColor = VehicleData.ClassicColors[newIndex - 1].id;
                            }
                        }
                        else if (listItem == dashColorList)
                        {
                            dashColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleDashboardColour
                            SetVehicleInteriorColour(veh.Handle, dashColor);
                        }
                        else if (listItem == intColorList)
                        {
                            intColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleInteriorColour
                            SetVehicleDashboardColour(veh.Handle, intColor);
                        }
                    }

                    SetVehicleExtraColours(veh.Handle, pearlColor, wheelColor);
                }
                else
                {
                    Notify.Error("차량 색상을 변경하려면 차량 운전석에 있어야 합니다.");
                }
            }

            async void HandleItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    string rawRValue = await GetUserInput("사용자 지정 RGB - R 값 (0~255)", "0", 3);
                    string rawGValue = await GetUserInput("사용자 지정 RGB - G 값 (0~255)", "0", 3);
                    string rawBValue = await GetUserInput("사용자 지정 RGB - B 값 (0~255)", "0", 3);
                    int rValue;
                    int gValue;
                    int bValue;

                    if (!int.TryParse(rawRValue, out rValue) || !int.TryParse(rawGValue, out gValue) || !int.TryParse(rawBValue, out bValue))
                    {
                        Notify.Error("잘못된 RGB 값이 입력되었습니다. 0~255 사이의 숫자를 입력하세요.");
                        return;
                    }

                    if (menu == primaryColorsMenu)
                    {
                        SetVehicleCustomPrimaryColour(veh.Handle, rValue, gValue, bValue);
                    }
                    else if (menu == secondaryColorsMenu)
                    {
                        SetVehicleCustomSecondaryColour(veh.Handle, rValue, gValue, bValue);
                    }
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var customColour = new MenuItem("사용자 지정 RGB") { Label = "→→→" };
                var pearlescentList = new MenuListItem("펄레슨트", classic, 0);
                var classicList = new MenuListItem("클래식", classic, 0);
                var metallicList = new MenuListItem("메탈릭", classic, 0);
                var matteList = new MenuListItem("무광", matte, 0);
                var metalList = new MenuListItem("메탈", metals, 0);
                var utilList = new MenuListItem("유틸", util, 0);
                var wornList = new MenuListItem("낡은 색상", worn, 0);

                if (i == 0)
                {
                    primaryColorsMenu.AddMenuItem(customColour);
                    primaryColorsMenu.AddMenuItem(classicList);
                    primaryColorsMenu.AddMenuItem(metallicList);
                    primaryColorsMenu.AddMenuItem(matteList);
                    primaryColorsMenu.AddMenuItem(metalList);
                    primaryColorsMenu.AddMenuItem(utilList);
                    primaryColorsMenu.AddMenuItem(wornList);

                    if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                    {
                        var chameleonList = new MenuListItem("카멜레온", chameleon, 0);

                        primaryColorsMenu.AddMenuItem(chameleonList);
                    }

                    primaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                    primaryColorsMenu.OnItemSelect += HandleItemSelect;
                }
                else
                {
                    secondaryColorsMenu.AddMenuItem(customColour);
                    secondaryColorsMenu.AddMenuItem(pearlescentList);
                    secondaryColorsMenu.AddMenuItem(classicList);
                    secondaryColorsMenu.AddMenuItem(metallicList);
                    secondaryColorsMenu.AddMenuItem(matteList);
                    secondaryColorsMenu.AddMenuItem(metalList);
                    secondaryColorsMenu.AddMenuItem(utilList);
                    secondaryColorsMenu.AddMenuItem(wornList);

                    secondaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                    secondaryColorsMenu.OnItemSelect += HandleItemSelect;
                }
            }

            customizeColorMenu.OnMenuOpen += (_) =>
            {
                int numVehColors = GetNumberOfVehicleColours(GetVehicle().Handle);

                if (numVehColors == 0)
                {
                    presetColorsBtn.Enabled = false;
                    presetColorsBtn.ListItems = ["프리셋 색상 없음"];
                    presetColorsBtn.ListIndex = 0;
                    return;
                }

                List<string> colorOptions = [];

                presetColorsBtn.Enabled = true;

                for (int i = 0; i < numVehColors; i++)
                {
                    colorOptions.Add($"Preset Color #{i + 1}");
                }

                int currentColor = GetVehicleColourCombination(GetVehicle().Handle);

                presetColorsBtn.ListItems = colorOptions;
                presetColorsBtn.ListIndex = currentColor < 0 ? 0 : currentColor;
            };

            customizeColorMenu.OnItemSelect += (_, item, _) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    if (item == chrome)
                    {
                        SetVehicleColours(veh.Handle, 120, 120); // chrome is index 120
                    }
                }
                else
                {
                    Notify.Error("이 항목을 변경하려면 운전 가능한 차량의 운전석에 있어야 합니다.");
                }
            };

            customizeColorMenu.OnListItemSelect += (_, _, index, _) => ChangeVehiclePresetColor(index);

            customizeColorMenu.OnListIndexChange += (_, _, _, index, _) => ChangeVehiclePresetColor(index);

            void ChangeVehiclePresetColor(int index)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    SetVehicleColourCombination(veh.Handle, index);
                }
                else
                {
                    Notify.Error("이 항목을 변경하려면 운전 가능한 차량의 운전석에 있어야 합니다.");
                }
            }
            #endregion

            #region Vehicle Doors Submenu Stuff
            var openAll = new MenuItem("모든 문 열기", "차량의 모든 문을 엽니다.");
            var closeAll = new MenuItem("모든 문 닫기", "차량의 모든 문을 닫습니다.");
            var LF = new MenuItem("왼쪽 앞문", "왼쪽 앞문을 엽니다/닫습니다.");
            var RF = new MenuItem("오른쪽 앞문", "오른쪽 앞문을 엽니다/닫습니다.");
            var LR = new MenuItem("왼쪽 뒷문", "왼쪽 뒷문을 엽니다/닫습니다.");
            var RR = new MenuItem("오른쪽 뒷문", "오른쪽 뒷문을 엽니다/닫습니다.");
            var HD = new MenuItem("보닛", "보닛을 엽니다/닫습니다.");
            var TR = new MenuItem("트렁크", "트렁크를 엽니다/닫습니다.");
            var E1 = new MenuItem("추가 문 1", "추가 문(#1)을 엽니다/닫습니다. 대부분의 차량에는 없는 문일 수 있습니다.");
            var E2 = new MenuItem("추가 문 2", "추가 문(#2)을 엽니다/닫습니다. 대부분의 차량에는 없는 문일 수 있습니다.");
            var BB = new MenuItem("폭탄창", "폭탄창을 엽니다/닫습니다. 일부 비행기에서만 사용할 수 있습니다.");
            var doors = new List<string>() { "앞 왼쪽", "앞 오른쪽", "뒤 왼쪽", "뒤 오른쪽", "보닛", "트렁크", "추가 문 1", "추가 문 2" };
            var removeDoorList = new MenuListItem("문 제거", doors, 0, "특정 차량 문을 완전히 제거합니다.");
            var deleteDoors = new MenuCheckboxItem("제거한 문 삭제", "활성화하면 위 목록으로 제거한 문이 월드에서 완전히 삭제됩니다. 비활성화하면 문이 바닥에 떨어집니다.", false);

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(E1);
            VehicleDoorsMenu.AddMenuItem(E2);
            VehicleDoorsMenu.AddMenuItem(BB);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);
            VehicleDoorsMenu.AddMenuItem(removeDoorList);
            VehicleDoorsMenu.AddMenuItem(deleteDoors);

            VehicleDoorsMenu.OnListItemSelect += (sender, item, index, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (veh.Driver == Game.PlayerPed)
                    {
                        if (item == removeDoorList)
                        {
                            SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NeedToBeTheDriver);
                    }
                }
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }
            };

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 8)
                    {
                        // If the door is open.
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    // If the index >= 8, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                        if (veh.HasBombBay)
                        {
                            veh.OpenBombBay();
                        }
                    }
                    // If the index >= 8, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh.Handle, false);
                        if (veh.HasBombBay)
                        {
                            veh.CloseBombBay();
                        }
                    }
                    // If bomb bay doors button is pressed and the vehicle has bomb bay doors.
                    else if (item == BB && veh.HasBombBay)
                    {
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        // If open, close them.
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        // Otherwise, open them.
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "to open/close a vehicle door");
                }
            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            var fwu = new MenuItem("~y~↑~s~ 앞창문 올리기", "앞창문 두 개를 모두 올립니다.");
            var fwd = new MenuItem("~o~↓~s~ 앞창문 내리기", "앞창문 두 개를 모두 내립니다.");
            var rwu = new MenuItem("~y~↑~s~ 뒷창문 올리기", "뒷창문 두 개를 모두 올립니다.");
            var rwd = new MenuItem("~o~↓~s~ 뒷창문 내리기", "뒷창문 두 개를 모두 내립니다.");
            VehicleWindowsMenu.AddMenuItem(fwu);
            VehicleWindowsMenu.AddMenuItem(fwd);
            VehicleWindowsMenu.AddMenuItem(rwu);
            VehicleWindowsMenu.AddMenuItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh.Handle, 0);
                        RollUpWindow(veh.Handle, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh.Handle, 0);
                        RollDownWindow(veh.Handle, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh.Handle, 2);
                        RollUpWindow(veh.Handle, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh.Handle, 2);
                        RollDownWindow(veh.Handle, 3);
                    }
                }
            };
            #endregion

            #region Vehicle Liveries Submenu Stuff
            menu.OnItemSelect += (sender, item, idex) =>
            {
                // If the liverys menu button is selected.
                if (item == liveriesMenuBtn)
                {
                    // Get the player's vehicle.
                    var veh = GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (veh != null && veh.Exists() && !veh.IsDead)
                    {
                        if (veh.Driver == Game.PlayerPed)
                        {
                            VehicleLiveriesMenu.ClearMenuItems();
                            SetVehicleModKit(veh.Handle, 0);
                            var liveryCount = GetVehicleLiveryCount(veh.Handle);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<string>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh.Handle, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                                    liveryList.Add(livery);
                                }
                                var liveryListItem = new MenuListItem("리버리 설정", liveryList, GetVehicleLivery(veh.Handle), "이 차량에 적용할 리버리를 선택합니다.");
                                VehicleLiveriesMenu.AddMenuItem(liveryListItem);
                                VehicleLiveriesMenu.OnListIndexChange += (_menu, listItem, oldIndex, newIndex, itemIndex) =>
                                {
                                    if (listItem == liveryListItem)
                                    {
                                        veh = GetVehicle();
                                        SetVehicleLivery(veh.Handle, newIndex);
                                    }
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("이 차량에는 리버리가 없습니다.");
                                VehicleLiveriesMenu.CloseMenu();
                                menu.OpenMenu();
                                var backBtn = new MenuItem("사용 가능한 리버리 없음 :(", "눌러서 돌아가기")
                                {
                                    Label = "뒤로가기"
                                };
                                VehicleLiveriesMenu.AddMenuItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                        }
                        else
                        {
                            Notify.Error("You have to be the driver of a vehicle to access this menu.");
                        }
                    }
                    else
                    {
                        Notify.Error("You have to be the driver of a vehicle to access this menu.");
                    }
                }
            };
            #endregion

            #region Vehicle Mod Submenu Stuff
            menu.OnItemSelect += (sender, item, index) =>
            {
                // When the mod submenu is openend, reset all items in there.
                if (item == modMenuBtn)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.CloseMenu();
                        menu.OpenMenu();
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            // when the components menu is opened.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.Size > 0)
                    {
                        VehicleComponentsMenu.ClearMenuItems();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    var veh = GetVehicle();

                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                    {
                        Dictionary<int, string> extraLabels;
                        if (!VehicleExtras.TryGetValue((uint)veh.Model.Hash, out extraLabels))
                        {
                            extraLabels = new Dictionary<int, string>();
                        }

                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (veh.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create the checkbox label
                                string extraLabel;
                                if (!extraLabels.TryGetValue(extra, out extraLabel))
                                    extraLabel = $"Extra #{extra}";
                                // Create a checkbox for it.
                                var extraCheckbox = new MenuCheckboxItem(extraLabel, extra.ToString(), veh.IsExtraOn(extra));
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddMenuItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }



                        if (vehicleExtras.Count > 0)
                        {
                            var backBtn = new MenuItem("뒤로가기", "차량 옵션 메뉴로 돌아갑니다.");
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            var backBtn = new MenuItem("사용 가능한 추가옵션 없음 :(", "차량 옵션 메뉴로 돌아갑니다.")
                            {
                                Label = "뒤로가기"
                            };
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };

            // Disable all extra options if vehicle is too damaged
            VehicleComponentsMenu.OnMenuOpen += (menu) =>
            {
                Vehicle vehicle;
                bool checkDamageBeforeChangingExtras = GetSettingsBool(Setting.vmenu_prevent_extras_when_damaged) && !IsAllowed(Permission.VOBypassExtraDamage);

                if (!checkDamageBeforeChangingExtras || !Entity.Exists(vehicle = GetVehicle()))
                {
                    return;
                }

                List<MenuItem> menuItems = menu.GetMenuItems();
                bool isTooDamaged = IsVehicleTooDamagedToChangeExtras(vehicle);

                menu.ClearMenuItems();

                if (isTooDamaged && !menuItems.Exists(i => i.Text.Contains("too damaged")))
                {
                    MenuItem spacer = GetSpacerMenuItem("차량이 너무 손상됨!", "차량 손상이 너무 심해 추가옵션을 변경할 수 없습니다. 먼저 수리하세요!");

                    // Place at the start of the menu
                    menuItems.Insert(0, spacer);
                }

                foreach (MenuItem item in menuItems)
                {
                    // Check for spacer
                    if (item.Text.Contains("too damaged"))
                    {
                        if (!isTooDamaged)
                        {
                            continue;
                        }
                    }
                    else if (item.Text != "뒤로가기")
                    {
                        item.Enabled = !isTooDamaged;
                    }

                    menu.AddMenuItem(item);
                }

                menu.RefreshIndex();
            };

            // when a checkbox in the components menu changes
            VehicleComponentsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                // Then toggle that extra.
                if (vehicleExtras.TryGetValue(item, out var extra))
                {
                    var veh = GetVehicle();

                    if (!Entity.Exists(veh))
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                        return;
                    }

                    bool checkDamageBeforeChangingExtras = GetSettingsBool(Setting.vmenu_prevent_extras_when_damaged) && !IsAllowed(Permission.VOBypassExtraDamage);

                    if (checkDamageBeforeChangingExtras)
                    {
                        bool isTooDamaged = IsVehicleTooDamagedToChangeExtras(veh);

                        if (isTooDamaged)
                        {
                            // Send message to player when extra change is denied
                            Notify.Alert("차량 손상이 너무 심해 추가옵션을 변경할 수 없습니다. 먼저 수리하세요!", true, false);

                            // Send to previous menu
                            VehicleComponentsMenu.GoBack();
                            return;
                        }
                    }

                    veh.ToggleExtra(extra, _checked);
                }
            };
            #endregion

            #region Underglow Submenu
            var underglowFront = new MenuCheckboxItem("앞쪽 라이트 활성화", "차량 앞쪽 언더글로우를 켜거나 끕니다. 일부 차량에는 라이트가 없을 수 있습니다.", false);
            var underglowBack = new MenuCheckboxItem("뒤쪽 라이트 활성화", "차량 왼쪽 언더글로우를 켜거나 끕니다. 일부 차량에는 라이트가 없을 수 있습니다.", false);
            var underglowLeft = new MenuCheckboxItem("왼쪽 라이트 활성화", "차량 오른쪽 언더글로우를 켜거나 끕니다. 일부 차량에는 라이트가 없을 수 있습니다.", false);
            var underglowRight = new MenuCheckboxItem("오른쪽 라이트 활성화", "차량 뒤쪽 언더글로우를 켜거나 끕니다. 일부 차량에는 라이트가 없을 수 있습니다.", false);
            var underglowColorsList = new List<string>();
            for (var i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }
            var underglowColor = new MenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0, "네온 언더글로우 색상을 선택합니다.");

            VehicleUnderglowMenu.AddMenuItem(underglowFront);
            VehicleUnderglowMenu.AddMenuItem(underglowBack);
            VehicleUnderglowMenu.AddMenuItem(underglowLeft);
            VehicleUnderglowMenu.AddMenuItem(underglowRight);

            VehicleUnderglowMenu.AddMenuItem(underglowColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    var veh = GetVehicle();
                    if (veh != null)
                    {
                        if (veh.Mods.HasNeonLights)
                        {
                            underglowFront.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Front) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                            underglowBack.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Back) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                            underglowLeft.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Left) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                            underglowRight.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Right) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                            underglowFront.Enabled = true;
                            underglowBack.Enabled = true;
                            underglowLeft.Enabled = true;
                            underglowRight.Enabled = true;

                            underglowFront.LeftIcon = MenuItem.Icon.NONE;
                            underglowBack.LeftIcon = MenuItem.Icon.NONE;
                            underglowLeft.LeftIcon = MenuItem.Icon.NONE;
                            underglowRight.LeftIcon = MenuItem.Icon.NONE;
                        }
                        else
                        {
                            underglowFront.Checked = false;
                            underglowBack.Checked = false;
                            underglowLeft.Checked = false;
                            underglowRight.Checked = false;

                            underglowFront.Enabled = false;
                            underglowBack.Enabled = false;
                            underglowLeft.Enabled = false;
                            underglowRight.Enabled = false;

                            underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                            underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                            underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                            underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                        }
                    }
                    else
                    {
                        underglowFront.Checked = false;
                        underglowBack.Checked = false;
                        underglowLeft.Checked = false;
                        underglowRight.Checked = false;

                        underglowFront.Enabled = false;
                        underglowBack.Enabled = false;
                        underglowLeft.Enabled = false;
                        underglowRight.Enabled = false;

                        underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                        underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                        underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                        underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    underglowColor.ListIndex = GetIndexFromColor();
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    var veh = GetVehicle();
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.ListIndex);
                        if (item == underglowLeft)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && _checked);
                        }
                        else if (item == underglowRight)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && _checked);
                        }
                        else if (item == underglowBack)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && _checked);
                        }
                        else if (item == underglowFront)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && _checked);
                        }
                    }
                }
            };

            VehicleUnderglowMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == underglowColor)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh.Mods.HasNeonLights)
                        {
                            veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                        }
                    }
                }
            };
            #endregion

            #region Handle menu-opening refreshing license plate
            menu.OnMenuOpen += (sender) =>
            {
                menu.GetMenuItems().ForEach((item) =>
                {
                    var veh = GetVehicle(true);

                    if (item == setLicensePlateType && item is MenuListItem listItem && veh != null && veh.Exists())
                    {
                        // Set the license plate style.
                        switch (veh.Mods.LicensePlateStyle)
                        {
                            case LicensePlateStyle.BlueOnWhite1:
                                listItem.ListIndex = 0;
                                break;
                            case LicensePlateStyle.BlueOnWhite2:
                                listItem.ListIndex = 1;
                                break;
                            case LicensePlateStyle.BlueOnWhite3:
                                listItem.ListIndex = 2;
                                break;
                            case LicensePlateStyle.YellowOnBlue:
                                listItem.ListIndex = 3;
                                break;
                            case LicensePlateStyle.YellowOnBlack:
                                listItem.ListIndex = 4;
                                break;
                            case LicensePlateStyle.NorthYankton:
                                listItem.ListIndex = 5;
                                break;
                            case LicensePlateStyle.ECola:
                                listItem.ListIndex = 6;
                                break;
                            case LicensePlateStyle.LasVenturas:
                                listItem.ListIndex = 7;
                                break;
                            case LicensePlateStyle.LibertyCity:
                                listItem.ListIndex = 8;
                                break;
                            case LicensePlateStyle.LSCarMeet:
                                listItem.ListIndex = 9;
                                break;
                            case LicensePlateStyle.LSPanic:
                                listItem.ListIndex = 10;
                                break;
                            case LicensePlateStyle.LSPounders:
                                listItem.ListIndex = 11;
                                break;
                            case LicensePlateStyle.Sprunk:
                                listItem.ListIndex = 12;
                                break;
                            default:
                                break;
                        }
                    }
                });
            };
            #endregion

        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
        public Menu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.Size > 0)
            {
                if (selectedIndex != 0)
                {
                    VehicleModMenu.ClearMenuItems(true);
                }
                else
                {
                    VehicleModMenu.ClearMenuItems(false);
                }

            }

            // Get the vehicle.
            var veh = GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh.Handle, 0);

                // Get all mods available on this vehicle.
                var mods = GetAllVehicleMods(veh);

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = GetVehicle();

                    // Get the proper localized mod type (suspension, armor, etc) name.
                    var typeName = mod.LocalizedModTypeName;

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<string>();

                    // Get the current item index ({current}/{max upgrades})
                    var currentItem = $"[1/{mod.ModCount + 1}]";

                    // Add the stock value for this mod.
                    var name = $"Stock {typeName} {currentItem}";
                    modlist.Add(name);

                    // Loop through all available upgrades for this specific mod type.
                    for (var x = 0; x < mod.ModCount; x++)
                    {
                        // Create the item index.
                        currentItem = $"[{2 + x}/{mod.ModCount + 1}]";

                        // Create the name (again, converting to proper case), then add the name.
                        name = mod.GetLocalizedModName(x) != "" ? $"{ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the MenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh.Handle, (int)mod.ModType) + 1;
                    var modTypeListItem = new MenuListItem(
                        typeName,
                        modlist,
                        currIndex,
                        $"Choose a ~y~{typeName}~s~ upgrade, it will be automatically applied to your vehicle."
                    )
                    {
                        ItemData = (int)mod.ModType
                    };

                    // Add the list item to the menu.
                    VehicleModMenu.AddMenuItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                var wheelTypes = new List<string>()
                {
                    "스포츠",       // 0
                    "머슬",       // 1
                    "로우라이더",     // 2
                    "SUV",          // 3
                    "오프로드",      // 4
                    "튜너",        // 5
                    "바이크 휠",  // 6
                    "하이엔드",     // 7
                    "베니스 (1)",  // 8
                    "베니스 (2)",  // 9
                    "오픈휠",   // 10
                    "스트리트",       // 11
                    "트랙"         // 12    
                };
                var vehicleWheelType = new MenuListItem("휠 타입", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(veh.Handle), 0, 12), $"차량에 적용할 ~y~휠 타입~s~을 선택하세요.");
                if (!veh.Model.IsBoat && !veh.Model.IsHelicopter && !veh.Model.IsPlane && !veh.Model.IsBicycle && !veh.Model.IsTrain)
                {
                    VehicleModMenu.AddMenuItem(vehicleWheelType);
                }

                // Create the checkboxes for some options.
                var toggleCustomWheels = new MenuCheckboxItem("커스텀 휠 전환", "눌러서 ~y~커스텀~s~ 휠을 추가하거나 제거합니다.", GetVehicleModVariation(veh.Handle, 23));
                var xenonHeadlights = new MenuCheckboxItem("제논 헤드라이트", "~b~제논~s~ 헤드라이트를 켜거나 끕니다.", IsToggleModOn(veh.Handle, 22));
                var turbo = new MenuCheckboxItem("터보", "이 차량의 ~y~터보~s~를 켜거나 끕니다.", IsToggleModOn(veh.Handle, 18));
                var bulletProofTires = new MenuCheckboxItem("방탄 타이어", "이 차량의 ~y~방탄 타이어~s~를 켜거나 끕니다.", !GetVehicleTyresCanBurst(veh.Handle));

                // Add the checkboxes to the menu.
                VehicleModMenu.AddMenuItem(toggleCustomWheels);
                VehicleModMenu.AddMenuItem(xenonHeadlights);
                var currentHeadlightColor = GetHeadlightsColorForVehicle(veh);
                if (currentHeadlightColor is < 0 or > 12)
                {
                    currentHeadlightColor = 13;
                }
                var headlightColor = new MenuListItem("헤드라이트 색상", new List<string>() { "흰색", "파란색", "전기 파란색", "민트 그린", "라임 그린", "노란색", "골드", "주황색", "빨간색", "포니 핑크", "핫핑크", "보라색", "블랙라이트", "기본 제논" }, currentHeadlightColor, "Arena Wars 업데이트로 추가된 유색 헤드라이트입니다. 먼저 제논 헤드라이트를 활성화해야 합니다.");
                VehicleModMenu.AddMenuItem(headlightColor);
                VehicleModMenu.AddMenuItem(turbo);
                VehicleModMenu.AddMenuItem(bulletProofTires);

                bool isLowGripAvailable = GetGameBuildNumber() >= 2372;
                var lowGripTires = new MenuCheckboxItem("저그립 타이어", "이 차량의 ~y~저그립 타이어~s~를 켜거나 끕니다.", isLowGripAvailable ? GetDriftTyresEnabled(veh.Handle) : false);
                if (isLowGripAvailable)
                {
                    VehicleModMenu.AddMenuItem(lowGripTires);
                }

                // Create a list of tire smoke options.
                var tireSmokes = new List<string>() { "빨간색", "주황색", "노란색", "Gold", "연두색", "진한 초록", "하늘색", "진한 파랑", "보라색", "핑크", "검정색" };
                var tireSmokeColors = new Dictionary<string, int[]>()
                {
                    ["빨간색"] = new int[] { 244, 65, 65 },
                    ["주황색"] = new int[] { 244, 167, 66 },
                    ["노란색"] = new int[] { 244, 217, 65 },
                    ["Gold"] = new int[] { 181, 120, 0 },
                    ["연두색"] = new int[] { 158, 255, 84 },
                    ["진한 초록"] = new int[] { 44, 94, 5 },
                    ["하늘색"] = new int[] { 65, 211, 244 },
                    ["진한 파랑"] = new int[] { 24, 54, 163 },
                    ["보라색"] = new int[] { 108, 24, 192 },
                    ["핑크"] = new int[] { 192, 24, 172 },
                    ["검정색"] = new int[] { 1, 1, 1 }
                };
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb; });
                var index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                var tireSmoke = new MenuListItem("타이어 연기 색상", tireSmokes, index, $"차량에 적용할 ~y~타이어 연기 색상~s~을 선택하세요.");
                VehicleModMenu.AddMenuItem(tireSmoke);

                // Create the checkbox to enable/disable the tiresmoke.
                var tireSmokeEnabled = new MenuCheckboxItem("타이어 연기", "차량의 ~y~타이어 연기~s~를 켜거나 끕니다. ~h~~r~중요:~s~ 비활성화 후 효과가 적용되려면 잠시 주행해야 합니다.", IsToggleModOn(veh.Handle, 20));
                VehicleModMenu.AddMenuItem(tireSmokeEnabled);

                // Create list for window tint
                var windowTints = new List<string>() { "기본 [1/7]", "없음 [2/7]", "리무진 [3/7]", "연한 스모크 [4/7]", "진한 스모크 [5/7]", "순검정 [6/7]", "초록 [7/7]" };
                var currentTint = GetVehicleWindowTint(veh.Handle);
                if (currentTint == -1)
                {
                    currentTint = 4; // stock
                }

                // Convert window tint to the correct index of the list above.
                switch (currentTint)
                {
                    case 0:
                        currentTint = 1; // None
                        break;
                    case 1:
                        currentTint = 5; // Pure Black
                        break;
                    case 2:
                        currentTint = 4; // Dark Smoke
                        break;
                    case 3:
                        currentTint = 3; // Light Smoke
                        break;
                    case 4:
                        currentTint = 0; // Stock
                        break;
                    case 5:
                        currentTint = 2; // Limo
                        break;
                    case 6:
                        currentTint = 6; // Green
                        break;
                    default:
                        break;
                }

                var windowTint = new MenuListItem("창문 선팅", windowTints, currentTint, "창문 선팅을 적용합니다.");
                VehicleModMenu.AddMenuItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh.Handle, 22, _checked);
                    }
                    // Turbo
                    else if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh.Handle, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh.Handle, !_checked);
                    }
                    // Low Grip Tyres
                    else if (item2 == lowGripTires)
                    {
                        SetDriftTyresEnabled(veh.Handle, _checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh.Handle, 23, GetVehicleMod(veh.Handle, 23), !GetVehicleModVariation(veh.Handle, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh.Handle)))
                        {
                            SetVehicleMod(veh.Handle, 24, GetVehicleMod(veh.Handle, 24), GetVehicleModVariation(veh.Handle, 23));
                        }
                    }
                    // Toggle Tire Smoke
                    else if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh.Handle, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh.Handle, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh.Handle, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh.Handle, 20);
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (item2.ItemData is int modType)
                    {
                        var selectedUpgrade = item2.ListIndex - 1;
                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        SetVehicleMod(veh.Handle, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, tire smoke color, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        var vehicleClass = GetVehicleClass(veh.Handle);
                        var isBikeOrOpenWheel = (newIndex == 6 && veh.Model.IsBike) || (newIndex == 10 && vehicleClass == 22);
                        var isNotBikeNorOpenWheel = newIndex != 6 && !veh.Model.IsBike && newIndex != 10 && vehicleClass != 22;
                        var isCorrectVehicleType = isBikeOrOpenWheel || isNotBikeNorOpenWheel;
                        if (!isCorrectVehicleType)
                        {
                            // Go past the index if it's not a bike.
                            if (!veh.Model.IsBike && vehicleClass != 22)
                            {
                                if (newIndex > oldIndex)
                                {
                                    item2.ListIndex++;
                                }
                                else
                                {
                                    item2.ListIndex--;
                                }
                            }
                            // Reset the index to 6 if it is a bike
                            else
                            {
                                item2.ListIndex = veh.Model.IsBike ? 6 : 10;
                            }
                        }
                        // Set the wheel type
                        SetVehicleWheelType(veh.Handle, item2.ListIndex);

                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        // Reset the wheel mod index for front wheels
                        SetVehicleMod(veh.Handle, 23, -1, customWheels);

                        // If the model is a bike, do the same thing for the rear wheels.
                        if (veh.Model.IsBike)
                        {
                            SetVehicleMod(veh.Handle, 24, -1, customWheels);
                        }

                        // Refresh the menu with the item index so that the view doesn't change
                        UpdateMods(selectedIndex: itemIndex);
                    }
                    // Tire smoke
                    else if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[newIndex]][0];
                        var g = tireSmokeColors[tireSmokes[newIndex]][1];
                        var b = tireSmokeColors[tireSmokes[newIndex]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                    }
                    // Window Tint
                    else if (item2 == windowTint)
                    {
                        // Stock = 4,
                        // None = 0,
                        // Limo = 5,
                        // LightSmoke = 3,
                        // DarkSmoke = 2,
                        // PureBlack = 1,
                        // Green = 6,

                        switch (newIndex)
                        {
                            case 1:
                                SetVehicleWindowTint(veh.Handle, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh.Handle, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh.Handle, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh.Handle, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh.Handle, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh.Handle, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh.Handle, 4); // Stock
                                break;
                        }
                    }
                    else if (item2 == headlightColor)
                    {
                        if (newIndex == 13) // default
                        {
                            SetHeadlightsColorForVehicle(veh, 255);
                        }
                        else if (newIndex is > (-1) and < 13)
                        {
                            SetHeadlightsColorForVehicle(veh, newIndex);
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            if (selectedIndex == 0)
            {
                VehicleModMenu.RefreshIndex();
            }

            //VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            //VehicleModMenu.CurrentIndex = selectedIndex;
        }

        internal static void SetHeadlightsColorForVehicle(Vehicle veh, int newIndex)
        {

            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex is > (-1) and < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, -1);
                }
            }
        }

        internal static int GetHeadlightsColorForVehicle(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    var val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val is > (-1) and < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }
        #endregion

        #region GetColorFromIndex function (underglow)

        private readonly List<int[]> _VehicleNeonLightColors = new()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        /// <summary>
        /// Converts a list index to a <see cref="System.Drawing.Color"/> struct.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index is >= 0 and < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        /// <summary>
        /// Returns the color index that is applied on the current vehicle. 
        /// If a color is active on the vehicle which is not in the list, it'll return the default index 0 (white).
        /// </summary>
        /// <returns></returns>
        private int GetIndexFromColor()
        {
            var veh = GetVehicle();

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
        #endregion

        private bool IsVehicleTooDamagedToChangeExtras(Vehicle vehicle)
        {
            float bodyHealth = vehicle.BodyHealth;
            float engineHealth = vehicle.EngineHealth;
            float allowedBodyHealth = GetSettingsInt(Setting.vmenu_allowed_body_damage_for_extra_change);
            float allowedEngineHealth = GetSettingsInt(Setting.vmenu_allowed_engine_damage_for_extra_change);

            return bodyHealth < allowedBodyHealth || engineHealth < allowedEngineHealth;
        }
    }
}
