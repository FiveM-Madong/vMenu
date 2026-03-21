using System;
using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class BannedPlayers
    {
        // Variables
        private Menu menu;

        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
            public string uuid;
        }

        BanRecord currentRecord = new();

        public List<BanRecord> banlist = new();

        readonly Menu bannedPlayer = new("차단된 플레이어", "차단 기록: ");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "차단된 플레이어 관리");

            menu.InstructionalButtons.Add(Control.Jump, "필터 옵션");
            menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(async (a, b) =>
            {
                if (banlist.Count > 1)
                {
                    var filterText = await GetUserInput("플레이어 이름 또는 차단 ID로 필터링 (비워두면 필터 초기화)");
                    if (string.IsNullOrEmpty(filterText))
                    {
                        Subtitle.Custom("필터가 초기화되었습니다.");
                        menu.ResetFilter();
                        UpdateBans();
                    }
                    else
                    {
                        menu.FilterMenuItems(item => item.ItemData is BanRecord br && (br.playerName.ToLower().Contains(filterText.ToLower()) || br.uuid.ToLower().Contains(filterText.ToLower())));
                        Subtitle.Custom("필터가 적용되었습니다.");
                    }
                }
                else
                {
                    Notify.Error("필터 기능을 사용하려면 최소 2명 이상의 차단된 플레이어가 필요합니다.");
                }

                Log($"Button pressed: {a} {b}");
            }), true));

            bannedPlayer.AddMenuItem(new MenuItem("플레이어 이름"));
            bannedPlayer.AddMenuItem(new MenuItem("차단한 관리자"));
            bannedPlayer.AddMenuItem(new MenuItem("차단 만료일"));
            bannedPlayer.AddMenuItem(new MenuItem("플레이어 식별자"));
            bannedPlayer.AddMenuItem(new MenuItem("차단 사유"));
            bannedPlayer.AddMenuItem(new MenuItem("~r~차단 해제", "~r~경고: 차단 해제는 되돌릴 수 없습니다. 해당 플레이어가 다시 서버에 접속하기 전까지는 다시 차단할 수 없습니다. 정말로 이 플레이어의 차단을 해제하시겠습니까? ~s~참고: 기간제 차단된 플레이어는 차단 기간이 만료된 뒤 서버에 접속하면 자동으로 차단이 해제됩니다."));

            // should be enough for now to cover all possible identifiers.
            var colors = new List<string>() { "~r~", "~g~", "~b~", "~o~", "~y~", "~p~", "~s~", "~t~", };

            bannedPlayer.OnMenuClose += (sender) =>
            {
                BaseScript.TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                bannedPlayer.GetMenuItems()[5].Label = "";
                UpdateBans();
            };

            bannedPlayer.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                bannedPlayer.GetMenuItems()[5].Label = "";
            };

            bannedPlayer.OnItemSelect += (sender, item, index) =>
            {
                if (index == 5 && IsAllowed(Permission.OPUnban))
                {
                    if (item.Label == "정말 하시겠습니까?")
                    {
                        if (banlist.Contains(currentRecord))
                        {
                            UnbanPlayer(banlist.IndexOf(currentRecord));
                            bannedPlayer.GetMenuItems()[5].Label = "";
                            bannedPlayer.GoBack();
                        }
                        else
                        {
                            Notify.Error("차단 해제 버튼을 눌렀지만 현재 보고 있는 차단 기록이 존재하지 않습니다.");
                        }
                    }
                    else
                    {
                        item.Label = "정말 하시겠습니까?";
                    }
                }
                else
                {
                    bannedPlayer.GetMenuItems()[5].Label = "";
                }

            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                currentRecord = item.ItemData;

                bannedPlayer.MenuSubtitle = "차단 기록: ~y~" + currentRecord.playerName;
                var nameItem = bannedPlayer.GetMenuItems()[0];
                var bannedByItem = bannedPlayer.GetMenuItems()[1];
                var bannedUntilItem = bannedPlayer.GetMenuItems()[2];
                var playerIdentifiersItem = bannedPlayer.GetMenuItems()[3];
                var banReasonItem = bannedPlayer.GetMenuItems()[4];
                nameItem.Label = currentRecord.playerName;
                nameItem.Description = "플레이어 이름: ~y~" + currentRecord.playerName;
                bannedByItem.Label = currentRecord.bannedBy;
                bannedByItem.Description = "차단한 관리자: ~y~" + currentRecord.bannedBy;
                if (currentRecord.bannedUntil.Date.Year == 3000)
                {
                    bannedUntilItem.Label = "영구";
                }
                else
                {
                    bannedUntilItem.Label = currentRecord.bannedUntil.Date.ToString();
                }

                bannedUntilItem.Description = "이 플레이어의 차단 만료일: " + currentRecord.bannedUntil.Date.ToString();
                playerIdentifiersItem.Description = "";

                var i = 0;
                foreach (var id in currentRecord.identifiers)
                {
                    // only (admins) people that can unban players are allowed to view IP's.
                    // this is just a slight 'safety' feature in case someone who doesn't know what they're doing
                    // gave builtin.everyone access to view the banlist.
                    if (id.StartsWith("ip:") && !IsAllowed(Permission.OPUnban))
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}ip: (숨김) ";
                    }
                    else
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{id.Replace(":", ": ")} ";
                    }
                    i++;
                }
                banReasonItem.Description = "차단 사유: " + currentRecord.banReason;

                var unbanPlayerBtn = bannedPlayer.GetMenuItems()[5];
                unbanPlayerBtn.Label = "";
                if (!IsAllowed(Permission.OPUnban))
                {
                    unbanPlayerBtn.Enabled = false;
                    unbanPlayerBtn.Description = "플레이어 차단을 해제할 권한이 없습니다. 차단 기록만 확인할 수 있습니다.";
                    unbanPlayerBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                bannedPlayer.RefreshIndex();
            };
            MenuController.AddMenu(bannedPlayer);

        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.ResetFilter();
            menu.ClearMenuItems();

            foreach (var ban in banlist)
            {
                var recordBtn = new MenuItem(ban.playerName, $"~y~{ban.playerName}~s~ 님은 ~y~{ban.bannedBy}~s~ 에 의해 ~y~{ban.bannedUntil}~s~ 까지 ~y~{ban.banReason}~s~ 사유로 차단되었습니다.")
                {
                    Label = "→→→",
                    ItemData = ban
                };
                menu.AddMenuItem(recordBtn);
                MenuController.BindMenuItem(menu, bannedPlayer, recordBtn);
            }
            menu.RefreshIndex();
        }

        /// <summary>
        /// Updates the list of ban records.
        /// </summary>
        /// <param name="banJsonString"></param>
        public void UpdateBanList(string banJsonString)
        {
            banlist.Clear();
            banlist = JsonConvert.DeserializeObject<List<BanRecord>>(banJsonString);
            UpdateBans();
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

        /// <summary>
        /// Sends an event to the server requesting the player to be unbanned.
        /// We'll just assume that worked fine, so remove the item from our local list, we'll re-sync once the menu is re-opened.
        /// </summary>
        /// <param name="index"></param>
        private void UnbanPlayer(int index)
        {
            var record = banlist[index];
            banlist.Remove(record);
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerUnban", record.uuid);
        }

        /// <summary>
        /// Converts the ban record (json object) into a BanRecord struct.
        /// </summary>
        /// <param name="banRecordJsonObject"></param>
        /// <returns></returns>
        public static BanRecord JsonToBanRecord(dynamic banRecordJsonObject)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in banRecordJsonObject)
            {
                var key = brValue.Name.ToString();
                var value = brValue.Value;
                if (key == "playerName")
                {
                    newBr.playerName = value.ToString();
                }
                else if (key == "identifiers")
                {
                    var tmpList = new List<string>();
                    foreach (var identifier in value)
                    {
                        tmpList.Add(identifier.ToString());
                    }
                    newBr.identifiers = tmpList;
                }
                else if (key == "bannedUntil")
                {
                    newBr.bannedUntil = DateTime.Parse(value.ToString());
                }
                else if (key == "banReason")
                {
                    newBr.banReason = value.ToString();
                }
                else if (key == "bannedBy")
                {
                    newBr.bannedBy = value.ToString();
                }
            }
            return newBr;
        }
    }
}
