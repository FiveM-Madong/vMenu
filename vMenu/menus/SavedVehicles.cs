using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient.menus
{
    public class SavedVehicles
    {
        // Variables
        private Menu classMenu;
        private Menu savedVehicleTypeMenu;
        private readonly Menu vehicleCategoryMenu = new("카테고리", "저장된 차량 관리");
        private readonly Menu savedVehiclesCategoryMenu = new("카테고리", "실행 중 자동으로 갱신됩니다!");
        private readonly Menu selectedVehicleMenu = new("차량 관리", "이 저장된 차량을 관리합니다.");
        private readonly Menu unavailableVehiclesMenu = new("누락된 차량", "사용할 수 없는 저장 차량");
        private Dictionary<string, VehicleInfo> savedVehicles = new();
        private readonly List<Menu> subMenus = new();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new();
        private int deleteButtonPressedCount = 0;
        private int replaceButtonPressedCount = 0;
        private SavedVehicleCategory currentCategory;

        // Need to be editable from other functions
        private readonly MenuListItem setCategoryBtn = new("차량 카테고리 설정", new List<string> { }, 0, "이 차량의 카테고리를 설정합니다. 선택하면 저장됩니다.");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateClassMenu()
        {
            var menuTitle = "저장된 차량";
            #region Create menus and submenus
            // Create the menu.
            classMenu = new Menu(menuTitle, "저장된 차량 관리");

            for (var i = 0; i < 23; i++)
            {
                var categoryMenu = new Menu("저장된 차량", GetLabelText($"VEH_CLASS_{i}"));

                var vehClassButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"{GetLabelText($"VEH_CLASS_{i}")} 카테고리의 저장된 차량 전체입니다.");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(classMenu, categoryMenu);
                classMenu.AddMenuItem(vehClassButton);
                vehClassButton.Label = "→→→";
                MenuController.BindMenuItem(classMenu, categoryMenu, vehClassButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            var unavailableModels = new MenuItem("사용할 수 없는 저장 차량", "이 차량들은 현재 게임에 모델이 존재하지 않아 사용할 수 없습니다. 서버에서 스트리밍되지 않는 차량일 가능성이 높습니다.")
            {
                Label = "→→→"
            };

            classMenu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(classMenu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(classMenu, unavailableVehiclesMenu);


            MenuController.AddMenu(savedVehicleTypeMenu);
            MenuController.AddMenu(savedVehiclesCategoryMenu);
            MenuController.AddMenu(selectedVehicleMenu);

            // Load selected category
            vehicleCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not SavedVehicleCategory)
                {
                    var name = await GetUserInput(windowTitle: "카테고리 이름을 입력하세요.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "카테고리 설명을 입력하세요. (선택 사항)", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"카테고리(~g~<C>{name}</C>~s~)가 저장되었습니다.");
                            Log($"Saved Category {name}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"저장에 실패했습니다. 이 이름(~y~<C>{name}</C>~s~)은 이미 사용 중일 가능성이 높습니다.");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = item.ItemData;
                }

                bool isUncategorized = currentCategory.Name == "Uncategorized";

                savedVehiclesCategoryMenu.MenuTitle = currentCategory.Name;
                savedVehiclesCategoryMenu.MenuSubtitle = $"~s~카테고리: ~y~{currentCategory.Name}";
                savedVehiclesCategoryMenu.ClearMenuItems();

                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();

                string ChangeCallback(MenuDynamicListItem item, bool left)
                {
                    int currentIndex = iconNames.IndexOf(item.CurrentItem);
                    int newIndex = left ? currentIndex - 1 : currentIndex + 1;

                    // If going past the start or end of the list
                    if (iconNames.ElementAtOrDefault(newIndex) == default)
                    {
                        if (left)
                        {
                            newIndex = iconNames.Count - 1;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }

                    item.RightIcon = (MenuItem.Icon)newIndex;

                    return iconNames[newIndex];
                }

                var renameBtn = new MenuItem("카테고리 이름 변경", "이 카테고리의 이름을 변경합니다.")
                {
                    Enabled = !isUncategorized
                };
                var descriptionBtn = new MenuItem("카테고리 설명 변경", "이 카테고리의 설명을 변경합니다.")
                {
                    Enabled = !isUncategorized
                };
                var iconBtn = new MenuDynamicListItem("카테고리 아이콘 변경", iconNames[(int)currentCategory.Icon], new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "이 카테고리의 아이콘을 변경합니다. 선택하면 저장됩니다.")
                {
                    Enabled = !isUncategorized,
                    RightIcon = currentCategory.Icon
                };
                var deleteBtn = new MenuItem("카테고리 삭제", "이 카테고리를 삭제합니다. 이 작업은 되돌릴 수 없습니다!")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("모든 차량 삭제", "체크된 상태에서 \"카테고리 삭제\"를 누르면 이 카테고리의 저장 차량도 모두 삭제됩니다. 체크하지 않으면 저장 차량은 \"미분류\"로 이동합니다.")
                {
                    Enabled = !isUncategorized
                };

                savedVehiclesCategoryMenu.AddMenuItem(renameBtn);
                savedVehiclesCategoryMenu.AddMenuItem(descriptionBtn);
                savedVehiclesCategoryMenu.AddMenuItem(iconBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ 차량 ↓");
                savedVehiclesCategoryMenu.AddMenuItem(spacer);

                if (savedVehicles.Count > 0)
                {
                    List<MenuItem> spawnableVehicles = [];
                    List<MenuItem> unspawnableVehicles = [];

                    foreach (var kvp in savedVehicles)
                    {
                        string name = kvp.Key;
                        VehicleInfo vehicle = kvp.Value;

                        if (string.IsNullOrEmpty(vehicle.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (vehicle.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        string buttonName = name.Substring(4);
                        bool canUse = IsModelInCdimage(vehicle.model);
                        string buttonDescription = "이 저장된 차량을 관리합니다.";

                        if (!canUse)
                        {
                            buttonName = $"~italic~{buttonName}~italic~";
                            buttonDescription += "\n\n~r~참고~w~~s~: 이 모델을 찾을 수 없어 스폰할 수 없습니다.";
                        }

                        var btn = new MenuItem(buttonName, buttonDescription)
                        {
                            Label = $"({vehicle.name}) →→→",
                            LeftIcon = canUse ? MenuItem.Icon.NONE : MenuItem.Icon.LOCK,
                            ItemData = kvp,
                        };

                        if (canUse)
                        {
                            spawnableVehicles.Add(btn);
                        }
                        else
                        {
                            unspawnableVehicles.Add(btn);
                        }
                    }

                    // Menu order: Category buttons -> Spawnable vehs -> Unspawnable vehs
                    foreach (MenuItem menuItem in spawnableVehicles.Concat(unspawnableVehicles))
                    {
                        savedVehiclesCategoryMenu.AddMenuItem(menuItem);
                    }
                }
            };

            savedVehiclesCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "새 카테고리 이름을 입력하세요", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("saved_veh_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("saved_veh_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    vehicle.Category = name;

                                    if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                    {
                                        updatedCount++;
                                        Log($"Updated category for \"{saveName}\"");
                                    }
                                    else
                                    {
                                        Log($"Something went wrong when updating category for \"{saveName}\"");
                                    }
                                }
                            }

                            Notify.Success($"카테고리 이름이 ~g~<C>{name}</C>~s~(으)로 변경되었습니다. 차량 {updatedCount}/{totalCount}개가 갱신되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("카테고리 이름을 변경하는 중 문제가 발생했습니다. 기존 카테고리는 삭제되지 않습니다.");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "새 카테고리 설명을 입력하세요", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success("카테고리 설명이 변경되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("카테고리 설명을 변경하는 중 문제가 발생했습니다.");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "정말 삭제할까요?")
                        {
                            bool deleteVehicles = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("saved_veh_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deleteVehicles)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp(saveName);
                                    }
                                    else
                                    {
                                        vehicle.Category = "Uncategorized";

                                        if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                        {
                                            updatedCount++;
                                            Log($"Updated category for \"{saveName}\"");
                                        }
                                        else
                                        {
                                            Log($"Something went wrong when updating category for \"{saveName}\"");
                                        }
                                    }
                                }
                            }

                            Notify.Success($"저장된 카테고리가 삭제되었습니다. 차량 {updatedCount}/{totalCount}개가 {(deleteVehicles ? "삭제" : "갱신")}되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            item.Label = "정말 삭제할까요?";
                        }
                        break;

                    // Load saved vehicle menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];

                        var vehInfo = item.ItemData;
                        selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
                        currentlySelectedVehicle = vehInfo;
                        MenuController.CloseAllMenus();
                        selectedVehicleMenu.OpenMenu();
                        MenuController.AddSubmenu(savedVehiclesCategoryMenu, selectedVehicleMenu);
                        break;
                }
            };

            // Change Category Icon
            savedVehiclesCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
                int iconIndex = iconNames.IndexOf(currentItem);

                currentCategory.Icon = (MenuItem.Icon)iconIndex;

                if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                {
                    Notify.Success($"카테고리 아이콘이 ~g~<C>{iconNames[iconIndex]}</C>~s~(으)로 변경되었습니다.");
                    UpdateSavedVehicleCategoriesMenu();
                }
                else
                {
                    Notify.Error("카테고리 아이콘을 변경하는 중 문제가 발생했습니다.");
                }
            };

            var spawnVehicle = new MenuItem("차량 스폰");
            var renameVehicle = new MenuItem("차량 이름 변경", "저장된 차량의 이름을 변경합니다.");
            var replaceVehicle = new MenuItem("~r~차량 교체", "저장된 차량이 현재 탑승 중인 차량으로 교체됩니다. ~r~경고: 이 작업은 되돌릴 수 없습니다!");
            var deleteVehicle = new MenuItem("~r~차량 삭제", "~r~이 작업은 저장된 차량을 삭제합니다. 경고: 이 작업은 되돌릴 수 없습니다!");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(setCategoryBtn);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                bool vehicleModelExists = IsModelInCdimage(currentlySelectedVehicle.Value.model);

                spawnVehicle.Enabled = vehicleModelExists;
                spawnVehicle.Description = vehicleModelExists ? "이 저장된 차량을 스폰합니다." : "이 모델은 게임 파일에서 찾을 수 없습니다. 애드온 차량이며 현재 서버에서 스트리밍되지 않는 것 같습니다.";

                spawnVehicle.Label = "(" + GetDisplayNameFromVehicleModel(currentlySelectedVehicle.Value.model).ToLower() + ")";
            };

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
                replaceButtonPressedCount = 0;
                replaceVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    var newName = await GetUserInput(windowTitle: "이 차량의 새 이름을 입력하세요.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("차량 이름이 성공적으로 변경되었습니다.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("이 이름은 이미 사용 중이거나 알 수 없는 이유로 실패했습니다. 문제가 있다고 생각되면 서버 관리자에게 문의하세요.");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        if (replaceButtonPressedCount == 0)
                        {
                            replaceButtonPressedCount = 1;
                            item.Label = "한 번 더 눌러 확인";
                            Notify.Alert("정말 이 차량으로 교체하시겠습니까? 확인하려면 버튼을 한 번 더 누르세요.");
                        }
                        else
                        {
                            replaceButtonPressedCount = 0;
                            item.Label = "";
                            SaveVehicle(currentlySelectedVehicle.Key.Substring(4), currentlySelectedVehicle.Value.Category);
                            selectedVehicleMenu.CloseMenu();
                            Notify.Success("저장된 차량이 현재 차량으로 교체되었습니다.");
                        }
                    }
                    else
                    {
                        Notify.Error("기존 차량을 교체하려면 먼저 차량에 탑승해야 합니다.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "한 번 더 눌러 확인";
                        Notify.Alert("정말 이 차량을 삭제하시겠습니까? 확인하려면 버튼을 한 번 더 누르세요.");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("저장된 차량이 삭제되었습니다.");
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
                if (item != replaceVehicle)
                {
                    replaceButtonPressedCount = 0;
                    replaceVehicle.Label = "";
                }
            };

            // Update category preview icon
            selectedVehicleMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) => listItem.RightIcon = listItem.ItemData[newSelectionIndex];

            // Update vehicle's category
            selectedVehicleMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                string name = listItem.ListItems[listIndex];

                if (name == "Create New")
                {
                    var newName = await GetUserInput(windowTitle: "카테고리 이름을 입력하세요.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "카테고리 설명을 입력하세요. (선택 사항)", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"카테고리(~g~<C>{newName}</C>~s~)가 저장되었습니다.");
                            Log($"Saved Category {newName}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"저장에 실패했습니다. 이 이름(~y~<C>{newName}</C>~s~)은 이미 사용 중일 가능성이 높습니다.");
                            return;
                        }
                    }
                }

                VehicleInfo vehicle = currentlySelectedVehicle.Value;

                vehicle.Category = name;

                if (StorageManager.SaveVehicleInfo(currentlySelectedVehicle.Key, vehicle, true))
                {
                    Notify.Success("차량이 성공적으로 저장되었습니다.");
                }
                else
                {
                    Notify.Error("차량을 저장할 수 없습니다. 원인을 알 수 없습니다. :(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedVehicleCategoriesMenu();
                vehicleCategoryMenu.OpenMenu();
            };

            unavailableVehiclesMenu.InstructionalButtons.Add(Control.FrontendDelete, "차량 삭제!");

            unavailableVehiclesMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.FrontendDelete, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>((m, c) =>
            {
                if (m.Size > 0)
                {
                    var index = m.CurrentIndex;
                    if (index < m.Size)
                    {
                        var item = m.GetMenuItems().Find(i => i.Index == index);
                        if (item != null && item.ItemData is KeyValuePair<string, VehicleInfo> sd)
                        {
                            if (item.Label == "~r~정말 삭제할까요?")
                            {
                                Log("Unavailable saved vehicle deleted, data: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = "~r~정말 삭제할까요?";
                            }
                        }
                        else
                        {
                            Notify.Error("어떤 이유에서인지 이 차량을 찾을 수 없습니다.");
                        }
                    }
                    else
                    {
                        Notify.Error("존재하지 않는 메뉴 항목의 삭제를 어떻게 눌렀는지 모르겠네요...?");
                    }
                }
                else
                {
                    Notify.Error("현재 삭제할 수 있는 사용 불가 차량이 없습니다!");
                }
            }), true));

            void ResetAreYouSure()
            {
                foreach (var i in unavailableVehiclesMenu.GetMenuItems())
                {
                    if (i.ItemData is KeyValuePair<string, VehicleInfo> vd)
                    {
                        i.Label = $"({vd.Value.name})";
                    }
                }
            }
            unavailableVehiclesMenu.OnMenuClose += (sender) => ResetAreYouSure();
            unavailableVehiclesMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) => ResetAreYouSure();

            #endregion
        }

        private void CreateTypeMenu()
        {
            savedVehicleTypeMenu = new("저장된 차량", "클래스 또는 사용자 카테고리에서 선택");

            var saveVehicle = new MenuItem("현재 차량 저장", "현재 탑승 중인 차량을 저장합니다.")
            {
                LeftIcon = MenuItem.Icon.CAR
            };
            var classButton = new MenuItem("차량 클래스", "클래스로 저장된 차량을 선택합니다.")
            {
                Label = "→→→"
            };
            var categoryButton = new MenuItem("차량 카테고리", "사용자 지정 카테고리로 저장된 차량을 선택합니다.")
            {
                Label = "→→→"
            };

            savedVehicleTypeMenu.AddMenuItem(saveVehicle);
            savedVehicleTypeMenu.AddMenuItem(classButton);
            savedVehicleTypeMenu.AddMenuItem(categoryButton);

            savedVehicleTypeMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("현재 탑승 중인 차량이 없습니다. 저장하려면 먼저 차량에 탑승하세요.");
                    }
                }
                else if (item == classButton)
                {
                    UpdateMenuAvailableCategories();
                }
                else if (item == categoryButton)
                {
                    UpdateSavedVehicleCategoriesMenu();
                }
            };

            MenuController.BindMenuItem(savedVehicleTypeMenu, GetClassMenu(), classButton);
            MenuController.BindMenuItem(savedVehicleTypeMenu, vehicleCategoryMenu, categoryButton);
        }

        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            var vehInfo = selectedItem.ItemData;
            List<string> categoryNames = GetAllCategoryNames();
            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
            setCategoryBtn.ItemData = categoryIcons;
            setCategoryBtn.ListItems = categoryNames;
            setCategoryBtn.ListIndex = 0;
            setCategoryBtn.RightIcon = categoryIcons[0];
            selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
            MenuController.CloseAllMenus();
            selectedVehicleMenu.OpenMenu();
            if (parentMenu != null)
            {
                MenuController.AddSubmenu(parentMenu, selectedVehicleMenu);
            }
            return true;
        }


        /// <summary>
        /// Updates the available vehicle category list.
        /// </summary>
        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = GetSavedVehicles();

            for (var i = 0; i < GetClassMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i && IsModelInCdimage(a.Value.model)))
                {
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetClassMenu().GetMenuItems()[i].Label = "→→→";
                    GetClassMenu().GetMenuItems()[i].Enabled = true;
                    GetClassMenu().GetMenuItems()[i].Description = $"{GetClassMenu().GetMenuItems()[i].Text} 카테고리의 저장된 차량 전체입니다.";
                }
                else
                {
                    GetClassMenu().GetMenuItems()[i].Label = "";
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetClassMenu().GetMenuItems()[i].Enabled = false;
                    GetClassMenu().GetMenuItems()[i].Description = $"{GetClassMenu().GetMenuItems()[i].Text} 카테고리에 속한 저장 차량이 없습니다.";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (var m in subMenus)
            {
                var size = m.Size;
                var vclass = subMenus.IndexOf(m);

                var count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (var m in subMenus)
            {
                // Clear items but don't reset the index because we can guarantee that the index won't be out of bounds.
                // this is the case because of the loop above where we reset the index if the items count changes.
                m.ClearMenuItems(true);
            }

            // Always clear this index because it's useless anyway and it's safer.
            unavailableVehiclesMenu.ClearMenuItems();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    var vclass = GetVehicleClassFromName(sv.Value.model);
                    var menu = subMenus[vclass];

                    var savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"이 저장된 차량을 관리합니다.")
                    {
                        Label = $"({sv.Value.name}) →→→",
                        ItemData = sv
                    };
                    menu.AddMenuItem(savedVehicleBtn);
                }
                else
                {
                    var missingVehItem = new MenuItem(sv.Key.Substring(4), "이 모델은 게임 파일에서 찾을 수 없습니다. 애드온 차량이며 현재 서버에서 스트리밍되지 않는 것 같습니다.")
                    {
                        Label = "(" + sv.Value.name + ")",
                        Enabled = false,
                        LeftIcon = MenuItem.Icon.LOCK,
                        ItemData = sv
                    };
                    //SetResourceKvp(sv.Key + "_tmp_dupe", JsonConvert.SerializeObject(sv.Value));
                    unavailableVehiclesMenu.AddMenuItem(missingVehItem);
                }
            }
            foreach (var m in subMenus)
            {
                m.SortMenuItems((A, B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
        }

        /// <summary>
        /// Updates the saved vehicle categories menu.
        /// </summary>
        private void UpdateSavedVehicleCategoriesMenu()
        {
            savedVehicles = GetSavedVehicles();

            var categories = GetAllCategoryNames();

            vehicleCategoryMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("카테고리 생성", "새 차량 카테고리를 생성합니다.")
            {
                Label = "→→→"
            };
            vehicleCategoryMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ 차량 카테고리 ↓");
            vehicleCategoryMenu.AddMenuItem(spacer);

            var uncategorized = new SavedVehicleCategory
            {
                Name = "Uncategorized",
                Description = "카테고리에 지정되지 않은 모든 저장 차량입니다."
            };
            var uncategorizedBtn = new MenuItem("미분류", uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            vehicleCategoryMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, uncategorizedBtn);

            // Remove "Create New" and "Uncategorized"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    SavedVehicleCategory category = StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    vehicleCategoryMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, btn);
                }
            }

            vehicleCategoryMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("saved_veh_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(19));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "Create New");
            categories.Insert(1, "Uncategorized");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + name).Icon);
            }

            return icons;
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetClassMenu()
        {
            if (classMenu == null)
            {
                CreateClassMenu();
            }
            return classMenu;
        }

        public Menu GetTypeMenu()
        {
            if (savedVehicleTypeMenu == null)
            {
                CreateTypeMenu();
            }
            return savedVehicleTypeMenu;
        }

        public struct SavedVehicleCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
