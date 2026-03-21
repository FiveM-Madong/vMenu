using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public MenuCheckboxItem vehicleBlackout;
        public MenuCheckboxItem snowEnabled;
        public static readonly List<string> weatherTypes = new()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "날씨 옵션");

            dynamicWeatherEnabled = new MenuCheckboxItem("동적 날씨 전환", "동적 날씨 변화를 활성화하거나 비활성화합니다.", EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem("정전 모드 전환", "맵 전체의 모든 조명을 비활성화하거나 활성화합니다.", EventManager.IsBlackoutEnabled);
            vehicleBlackout = new MenuCheckboxItem("차량 라이트 정전 모드 전환", "맵 전체의 모든 차량 라이트를 비활성화하거나 활성화합니다.", !EventManager.IsVehicleLightsEnabled);
            snowEnabled = new MenuCheckboxItem("눈 효과 활성화", "지면에 눈이 강제로 표시되며, 보행자와 차량에 눈 파티클 효과를 활성화합니다. 최상의 효과를 위해 X-MAS 또는 약한 눈 날씨와 함께 사용하세요.", ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));

            var extrasunny = new MenuItem("매우 맑음", "날씨를 ~y~매우 맑음~s~으로 설정합니다!") { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem("맑음", "날씨를 ~y~맑음~s~으로 설정합니다!") { ItemData = "CLEAR" };
            var neutral = new MenuItem("중립", "날씨를 ~y~중립~s~으로 설정합니다!") { ItemData = "NEUTRAL" };
            var smog = new MenuItem("스모그", "날씨를 ~y~스모그~s~로 설정합니다!") { ItemData = "SMOG" };
            var foggy = new MenuItem("안개", "날씨를 ~y~안개~s~로 설정합니다!") { ItemData = "FOGGY" };
            var clouds = new MenuItem("흐림", "날씨를 ~y~흐림~s~으로 설정합니다!") { ItemData = "CLOUDS" };
            var overcast = new MenuItem("매우 흐림", "날씨를 ~y~매우 흐림~s~으로 설정합니다!") { ItemData = "OVERCAST" };
            var clearing = new MenuItem("개임", "날씨를 ~y~개임~s~으로 설정합니다!") { ItemData = "CLEARING" };
            var rain = new MenuItem("비", "날씨를 ~y~비~s~로 설정합니다!") { ItemData = "RAIN" };
            var thunder = new MenuItem("천둥번개", "날씨를 ~y~천둥번개~s~로 설정합니다!") { ItemData = "THUNDER" };
            var blizzard = new MenuItem("폭설", "날씨를 ~y~폭설~s~로 설정합니다!") { ItemData = "BLIZZARD" };
            var snow = new MenuItem("눈", "날씨를 ~y~눈~s~으로 설정합니다!") { ItemData = "SNOW" };
            var snowlight = new MenuItem("약한 눈", "날씨를 ~y~약한 눈~s~으로 설정합니다!") { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem("크리스마스 눈", "날씨를 ~y~크리스마스 눈~s~으로 설정합니다!") { ItemData = "XMAS" };
            var halloween = new MenuItem("할로윈", "날씨를 ~y~할로윈~s~으로 설정합니다!") { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem("모든 구름 제거", "하늘의 모든 구름을 제거합니다!");
            var randomizeclouds = new MenuItem("구름 무작위 변경", "하늘에 무작위 구름을 추가합니다!");

            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
            }
            if (IsAllowed(Permission.WOVehBlackout))
            {
                menu.AddMenuItem(vehicleBlackout);
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                menu.AddMenuItem(snowEnabled);
                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
                else if (item.ItemData is string weatherType)
                {
                    Notify.Custom($"날씨가 ~y~{item.Text}~s~(으)로 변경됩니다. 적용까지 {EventManager.WeatherChangeTime}초가 걸립니다.");
                    UpdateServerWeather(weatherType, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom($"동적 날씨 변화가 이제 {(_checked ? "~g~활성화" : "~r~비활성화")}~s~되었습니다.");
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom($"정전 모드가 이제 {(_checked ? "~g~활성화" : "~r~비활성화")}~s~되었습니다.");
                    UpdateServerBlackout(_checked);
                }
                else if (item == vehicleBlackout)
                {
                    Notify.Custom($"차량 라이트 정전 모드가 이제 {(_checked ? "~g~활성화" : "~r~비활성화")}~s~되었습니다.");
                    UpdateServerVehicleBlackout(!_checked);
                }
                else if (item == snowEnabled)
                {
                    if (EventManager.GetServerWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
                    {
                        Notify.Custom($"날씨가 ~y~{EventManager.GetServerWeather}~s~일 때는 눈 효과를 비활성화할 수 없습니다.");
                        return;
                    }

                    Notify.Custom($"눈 효과가 이제 강제로 {(_checked ? "~g~활성화" : "~r~비활성화")}~s~됩니다.");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.DynamicWeatherEnabled, _checked);
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
    }
}
