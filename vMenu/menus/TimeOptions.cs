using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class TimeOptions
    {
        // Variables
        private Menu menu;
        public MenuItem freezeTimeToggle;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "시간 설정");

            // Create all menu items.
            freezeTimeToggle = new MenuItem("시간 정지/해제", "시간 고정을 켜거나 끕니다.");
            var earlymorning = new MenuItem("이른 아침", "시간을 06:00으로 설정합니다.")
            {
                Label = "06:00"
            };
            var morning = new MenuItem("아침", "시간을 09:00으로 설정합니다.")
            {
                Label = "09:00"
            };
            var noon = new MenuItem("정오", "시간을 12:00으로 설정합니다.")
            {
                Label = "12:00"
            };
            var earlyafternoon = new MenuItem("이른 오후", "시간을 15:00으로 설정합니다.")
            {
                Label = "15:00"
            };
            var afternoon = new MenuItem("오후", "시간을 18:00으로 설정합니다.")
            {
                Label = "18:00"
            };
            var evening = new MenuItem("저녁", "시간을 21:00으로 설정합니다.")
            {
                Label = "21:00"
            };
            var midnight = new MenuItem("자정", "시간을 00:00으로 설정합니다.")
            {
                Label = "00:00"
            };
            var night = new MenuItem("심야", "시간을 03:00으로 설정합니다.")
            {
                Label = "03:00"
            };

            var hours = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            var minutes = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (var i = 10; i < 60; i++)
            {
                if (i < 24)
                {
                    hours.Add(i.ToString());
                }
                minutes.Add(i.ToString());
            }
            var manualHour = new MenuListItem("사용자 지정 시 설정", hours, 0);
            var manualMinute = new MenuListItem("사용자 지정 분 설정", minutes, 0);

            // Add all menu items to the menu.
            if (IsAllowed(Permission.TOFreezeTime))
            {
                menu.AddMenuItem(freezeTimeToggle);
            }
            if (IsAllowed(Permission.TOSetTime))
            {
                menu.AddMenuItem(earlymorning);
                menu.AddMenuItem(morning);
                menu.AddMenuItem(noon);
                menu.AddMenuItem(earlyafternoon);
                menu.AddMenuItem(afternoon);
                menu.AddMenuItem(evening);
                menu.AddMenuItem(midnight);
                menu.AddMenuItem(night);
                menu.AddMenuItem(manualHour);
                menu.AddMenuItem(manualMinute);
            }

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If it's the freeze time button.
                if (item == freezeTimeToggle)
                {
                    Subtitle.Info($"시간은 이제 {(EventManager.IsServerTimeFrozen ? "~y~계속 진행됩니다" : "~o~정지됩니다")}~s~.", prefix: "알림:");
                    FreezeServerTime(!EventManager.IsServerTimeFrozen);
                }
                else
                {
                    // Set the time using the index and some math :)
                    // eg: index = 3 (12:00) ---> 3 * 3 (=9) + 3 [= 12] ---> 12:00
                    // eg: index = 8 (03:00) ---> 8 * 3 (=24) + 3 (=27, >23 so 27-24) [=3] ---> 03:00
                    var newHour = 0;
                    if (IsAllowed(Permission.TOFreezeTime))
                    {
                        newHour = (index * 3) + 3 < 23 ? (index * 3) + 3 : (index * 3) + 3 - 24;
                    }
                    else
                    {
                        newHour = ((index + 1) * 3) + 3 < 23 ? ((index + 1) * 3) + 3 : ((index + 1) * 3) + 3 - 24;
                    }

                    var newMinute = 0;
                    Subtitle.Info($"시간이 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~(으)로 설정되었습니다.", prefix: "알림:");
                    UpdateServerTime(newHour, newMinute);
                }

            };

            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                var newHour = EventManager.GetServerHours;
                var newMinute = EventManager.GetServerMinutes;
                if (item == manualHour)
                {
                    newHour = item.ListIndex;
                }
                else if (item == manualMinute)
                {
                    newMinute = item.ListIndex;
                }

                Subtitle.Info($"시간이 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~(으)로 설정되었습니다.", prefix: "알림:");
                UpdateServerTime(newHour, newMinute);
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
    }
}
