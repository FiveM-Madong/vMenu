using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;

        public Menu VehicleDoorsMenu { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "개인 차량 옵션");

            // menu items
            var setVehice = new MenuItem("차량 지정", "현재 타고 있는 차량을 개인 차량으로 지정합니다. 이미 개인 차량이 설정되어 있다면 기존 선택이 덮어써집니다.") { Label = "현재 차량: 없음" };
            var toggleEngine = new MenuItem("엔진 켜기/끄기", "차량에 탑승하고 있지 않아도 엔진을 켜거나 끕니다. 다른 사람이 현재 차량을 사용 중이면 작동하지 않습니다.");
            var toggleLights = new MenuListItem("차량 라이트 설정", new List<string>() { "강제 켜기", "강제 끄기", "초기화" }, 0, "차량 전조등을 켜거나 끕니다. 이 기능을 사용하려면 차량 엔진이 켜져 있어야 합니다.");
            var toggleStance = new MenuListItem("차량 높이", new List<string>() { "기본", "낮춤" }, 0, "개인 차량의 높이를 선택합니다.");
            var kickAllPassengers = new MenuItem("탑승자 내리기", "개인 차량에 탑승한 모든 승객을 내리게 합니다.");
            //MenuItem
            var lockDoors = new MenuItem("차량 문 잠금", "모든 플레이어에 대해 차량 문을 잠급니다. 이미 차량 안에 있는 사람은 문이 잠겨 있어도 언제든지 내릴 수 있습니다.");
            var unlockDoors = new MenuItem("차량 문 잠금 해제", "모든 플레이어에 대해 차량 문 잠금을 해제합니다.");
            var doorsMenuBtn = new MenuItem("차량 문", "여기서 차량 문을 열고, 닫고, 제거하고, 복구할 수 있습니다.")
            {
                Label = "→→→"
            };
            var soundHorn = new MenuItem("경적 울리기", "차량 경적을 울립니다.");
            var toggleAlarm = new MenuItem("경보음 켜기/끄기", "차량 경보음을 켜거나 끕니다. 경보를 설정하는 기능은 아니며, 현재 울리고 있는 경보음 상태만 전환합니다.");
            var enableBlip = new MenuCheckboxItem("개인 차량 블립 추가", "차량을 개인 차량으로 지정했을 때 표시되는 블립을 켜거나 끕니다.", EnableVehicleBlip) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            var exclusiveDriver = new MenuCheckboxItem("운전자 전용", "활성화하면 운전석에는 본인만 탑승할 수 있습니다. 다른 플레이어는 운전할 수 없지만 동승자는 될 수 있습니다.", false) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            //submenu
            VehicleDoorsMenu = new Menu("차량 문", "차량 문 관리");
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Toggle stance
            if (IsAllowed(Permission.PVToggleStance))
            {
                menu.AddMenuItem(toggleStance);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            if (IsAllowed(Permission.PVDoors))
            {
                menu.AddMenuItem(doorsMenuBtn);
            }

            // Sound horn
            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            // Toggle alarm sound
            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }

            // Enable blip for personal vehicle
            if (IsAllowed(Permission.PVAddBlip))
            {
                menu.AddMenuItem(enableBlip);
            }

            if (IsAllowed(Permission.PVExclusiveDriver))
            {
                menu.AddMenuItem(exclusiveDriver);
            }


            // Handle list presses
            menu.OnListItemSelect += (sender, item, itemIndex, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("현재 이 차량을 제어할 수 없습니다. 다른 사람이 차량을 운전 중인가요? 다른 플레이어가 차량을 제어하고 있지 않은지 확인한 뒤 다시 시도하세요.");
                            return;
                        }
                    }

                    if (item == toggleLights)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 3);
                        }
                        else if (itemIndex == 1)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 1);
                        }
                        else
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 0);
                        }
                    }
                    else if (item == toggleStance)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, false);
                        }
                        else if (itemIndex == 1)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, true);
                        }
                    }

                }
                else
                {
                    Notify.Error("아직 개인 차량을 선택하지 않았거나, 차량이 삭제되었습니다. 이 옵션을 사용하려면 먼저 개인 차량을 설정하세요.");
                }
            };

            // Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableBlip)
                {
                    EnableVehicleBlip = _checked;
                    if (EnableVehicleBlip)
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                        {
                            if (CurrentPersonalVehicle.AttachedBlip == null || !CurrentPersonalVehicle.AttachedBlip.Exists())
                            {
                                CurrentPersonalVehicle.AttachBlip();
                            }
                            CurrentPersonalVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            CurrentPersonalVehicle.AttachedBlip.Name = "개인 차량";
                        }
                        else
                        {
                            Notify.Error("아직 개인 차량을 선택하지 않았거나, 차량이 삭제되었습니다. 이 옵션을 사용하려면 먼저 개인 차량을 설정하세요.");
                        }

                    }
                    else
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists() && CurrentPersonalVehicle.AttachedBlip != null && CurrentPersonalVehicle.AttachedBlip.Exists())
                        {
                            CurrentPersonalVehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else if (item == exclusiveDriver)
                {
                    if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                    {
                        if (NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (_checked)
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, true);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle, 1);
                            }
                            else
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, false);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, 0, 1);
                            }
                        }
                        else
                        {
                            item.Checked = !_checked;
                            Notify.Error("현재 이 차량을 제어할 수 없습니다. 다른 사람이 차량을 운전 중인가요? 다른 플레이어가 차량을 제어하고 있지 않은지 확인한 뒤 다시 시도하세요.");
                        }
                    }
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists())
                        {
                            if (Game.PlayerPed == veh.Driver)
                            {
                                CurrentPersonalVehicle = veh;
                                veh.PreviouslyOwnedByPlayer = true;
                                veh.IsPersistent = true;
                                if (EnableVehicleBlip && IsAllowed(Permission.PVAddBlip))
                                {
                                    if (veh.AttachedBlip == null || !veh.AttachedBlip.Exists())
                                    {
                                        veh.AttachBlip();
                                    }
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.Name = "개인 차량";
                                }
                                var name = GetLabelText(veh.DisplayName);
                                if (string.IsNullOrEmpty(name) || name.ToLower() == "null")
                                {
                                    name = veh.DisplayName;
                                }
                                item.Label = $"현재 차량: {name}";
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
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                {
                    if (item == kickAllPassengers)
                    {
                        Ped[] occupants = CurrentPersonalVehicle.Occupants;

                        if (occupants.Count() > 0 && occupants.Any(p => p != Game.PlayerPed && p.IsPlayer))
                        {
                            TriggerServerEvent("vMenu:GetOutOfCar", CurrentPersonalVehicle.NetworkId);
                        }
                        else
                        {
                            Notify.Info("내리게 할 다른 플레이어가 현재 차량에 없습니다.");
                        }
                    }
                    else
                    {
                        if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                            {
                                Notify.Error("현재 이 차량을 제어할 수 없습니다. 다른 사람이 차량을 운전 중인가요? 다른 플레이어가 차량을 제어하고 있지 않은지 확인한 뒤 다시 시도하세요.");
                                return;
                            }
                        }

                        if (item == toggleEngine)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SetVehicleEngineOn(CurrentPersonalVehicle.Handle, !CurrentPersonalVehicle.IsEngineRunning, true, true);
                        }

                        else if (item == lockDoors || item == unlockDoors)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            var _lock = item == lockDoors;
                            LockOrUnlockDoors(CurrentPersonalVehicle, _lock);
                        }

                        else if (item == soundHorn)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SoundHorn(CurrentPersonalVehicle);
                        }

                        else if (item == toggleAlarm)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            ToggleVehicleAlarm(CurrentPersonalVehicle);
                        }
                    }
                }
                else
                {
                    Notify.Error("아직 개인 차량을 선택하지 않았거나, 차량이 삭제되었습니다. 이 옵션을 사용하려면 먼저 개인 차량을 설정하세요.");
                }
            };

            #region Doors submenu 
            var openAll = new MenuItem("모든 문 열기", "차량의 모든 문을 엽니다.");
            var closeAll = new MenuItem("모든 문 닫기", "차량의 모든 문을 닫습니다.");
            var LF = new MenuItem("왼쪽 앞문", "왼쪽 앞문을 열거나 닫습니다.");
            var RF = new MenuItem("오른쪽 앞문", "오른쪽 앞문을 열거나 닫습니다.");
            var LR = new MenuItem("왼쪽 뒷문", "왼쪽 뒷문을 열거나 닫습니다.");
            var RR = new MenuItem("오른쪽 뒷문", "오른쪽 뒷문을 열거나 닫습니다.");
            var HD = new MenuItem("보닛", "보닛을 열거나 닫습니다.");
            var TR = new MenuItem("트렁크", "트렁크를 열거나 닫습니다.");
            var E1 = new MenuItem("추가 문 1", "추가 문(#1)을 열거나 닫습니다. 대부분의 차량에는 이 문이 없습니다.");
            var E2 = new MenuItem("추가 문 2", "추가 문(#2)을 열거나 닫습니다. 대부분의 차량에는 이 문이 없습니다.");
            var BB = new MenuItem("폭탄창", "폭탄창을 열거나 닫습니다. 일부 비행기에서만 사용할 수 있습니다.");
            var doors = new List<string>() { "앞쪽 왼쪽", "앞쪽 오른쪽", "뒤쪽 왼쪽", "뒤쪽 오른쪽", "보닛", "트렁크", "추가 문 1", "추가 문 2", "폭탄창" };
            var removeDoorList = new MenuListItem("문 제거", doors, 0, "특정 차량 문을 완전히 제거합니다.");
            var deleteDoors = new MenuCheckboxItem("제거한 문 삭제", "활성화하면 위 목록에서 제거한 문이 월드에서 완전히 삭제됩니다. 비활성화하면 문은 바닥에 떨어지기만 합니다.", false);

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
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("현재 이 차량을 제어할 수 없습니다. 다른 사람이 차량을 운전 중인가요? 다른 플레이어가 차량을 제어하고 있지 않은지 확인한 뒤 다시 시도하세요.");
                            return;
                        }
                    }

                    if (item == removeDoorList)
                    {
                        PressKeyFob(veh);
                        SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                    }
                }
            };

            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("현재 이 차량을 제어할 수 없습니다. 다른 사람이 차량을 운전 중인가요? 다른 플레이어가 차량을 제어하고 있지 않은지 확인한 뒤 다시 시도하세요.");
                            return;
                        }
                    }

                    if (index < 8)
                    {
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;
                        PressKeyFob(veh);
                        if (open)
                        {
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    else if (item == openAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                    }
                    else if (item == closeAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorShut(veh.Handle, door, false);
                        }
                    }
                    else if (item == BB && veh.HasBombBay)
                    {
                        PressKeyFob(veh);
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                    else
                    {
                        Notify.Error("아직 개인 차량을 선택하지 않았거나, 차량이 삭제되었습니다. 이 옵션을 사용하려면 먼저 개인 차량을 설정하세요.");
                    }
                }
            };
            #endregion
        }



        private async void SoundHorn(Vehicle veh)
        {
            if (veh != null && veh.Exists())
            {
                var timer = GetGameTimer();
                while (GetGameTimer() - timer < 1000)
                {
                    SoundVehicleHornThisFrame(veh.Handle);
                    await Delay(0);
                }
            }
        }

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
