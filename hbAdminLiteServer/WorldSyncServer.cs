using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace hbAdminLiteServer
{
    public class WorldSyncServer : BaseScript
    {
        private int CurrentHours
        {
            get => MathUtil.Clamp(GetInt("hb_worldsync_current_hour", 9), 0, 23);
            set => SetConvarReplicated("hb_worldsync_current_hour", MathUtil.Clamp(value, 0, 23).ToString());
        }

        private int CurrentMinutes
        {
            get => MathUtil.Clamp(GetInt("hb_worldsync_current_minute", 0), 0, 59);
            set => SetConvarReplicated("hb_worldsync_current_minute", MathUtil.Clamp(value, 0, 59).ToString());
        }

        private int MinuteClockSpeed
        {
            get
            {
                var value = GetInt("hb_worldsync_ingame_minute_duration", 2000);
                return value < 100 ? 2000 : value;
            }
        }

        private bool FreezeTime
        {
            get => GetBool("hb_worldsync_freeze_time", false);
            set => SetConvarReplicated("hb_worldsync_freeze_time", value.ToString().ToLower());
        }

        private bool IsServerTimeSynced => GetBool("hb_worldsync_sync_to_machine_time", false);

        private string CurrentWeather
        {
            get
            {
                var value = GetConvar("hb_worldsync_current_weather", "CLEAR");
                if (string.IsNullOrEmpty(value) || !WeatherTypes.Contains(value.ToUpper()))
                {
                    return "CLEAR";
                }

                return value.ToUpper();
            }
            set
            {
                var normalized = string.IsNullOrEmpty(value) ? "CLEAR" : value.ToUpper();
                if (!WeatherTypes.Contains(normalized))
                {
                    normalized = "CLEAR";
                }

                SetConvarReplicated("hb_worldsync_current_weather", normalized);
            }
        }

        private bool DynamicWeatherEnabled
        {
            get => GetBool("hb_worldsync_enable_dynamic_weather", false);
            set => SetConvarReplicated("hb_worldsync_enable_dynamic_weather", value.ToString().ToLower());
        }

        private bool ManualSnowEnabled
        {
            get => GetBool("hb_worldsync_enable_snow", false);
            set => SetConvarReplicated("hb_worldsync_enable_snow", value.ToString().ToLower());
        }

        private bool BlackoutEnabled
        {
            get => GetBool("hb_worldsync_blackout_enabled", false);
            set => SetConvarReplicated("hb_worldsync_blackout_enabled", value.ToString().ToLower());
        }

        private bool VehicleBlackoutEnabled
        {
            get => GetBool("hb_worldsync_vehicle_blackout_enabled", false);
            set => SetConvarReplicated("hb_worldsync_vehicle_blackout_enabled", value.ToString().ToLower());
        }

        private int DynamicWeatherMinutes => Math.Max(GetInt("hb_worldsync_dynamic_weather_timer", 10), 1);

        private long lastWeatherChange;

        private readonly List<string> CloudTypes =
        [
            "Cloudy 01",
            "RAIN",
            "horizonband1",
            "horizonband2",
            "Puffs",
            "Wispy",
            "Horizon",
            "Stormy 01",
            "Clear 01",
            "Snowy 01",
            "Contrails",
            "altostratus",
            "Nimbus",
            "Cirrus",
            "cirrocumulus",
            "stratoscumulus",
            "horizonband3",
            "Stripey",
            "horsey",
            "shower",
        ];

        private readonly HashSet<string> WeatherTypes =
        [
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
            "HALLOWEEN",
        ];

        public WorldSyncServer()
        {
            if (!GetBool("hb_worldsync_enabled", true))
            {
                return;
            }

            if (GetBool("hb_worldsync_enable_weather_sync", true))
            {
                Tick += WeatherLoop;
            }

            if (GetBool("hb_worldsync_enable_time_sync", true))
            {
                Tick += TimeLoop;
            }
        }

        private async Task TimeLoop()
        {
            if (IsServerTimeSynced)
            {
                var currentTime = DateTime.Now;
                CurrentMinutes = currentTime.Minute;
                CurrentHours = currentTime.Hour;
                await Delay(60000);
            }
            else
            {
                if (!FreezeTime)
                {
                    if ((CurrentMinutes + 1) > 59)
                    {
                        CurrentMinutes = 0;
                        CurrentHours = (CurrentHours + 1) > 23 ? 0 : CurrentHours + 1;
                    }
                    else
                    {
                        CurrentMinutes++;
                    }
                }

                await Delay(MinuteClockSpeed);
            }
        }

        private async Task WeatherLoop()
        {
            if (DynamicWeatherEnabled)
            {
                await Delay(DynamicWeatherMinutes * 60000);

                if (GetBool("hb_worldsync_enable_weather_sync", true))
                {
                    if (CurrentWeather is "XMAS" or "HALLOWEEN" or "NEUTRAL")
                    {
                        DynamicWeatherEnabled = false;
                        return;
                    }

                    if (GetGameTimer() - lastWeatherChange > (DynamicWeatherMinutes * 60000))
                    {
                        RefreshWeather();
                    }
                }
            }
            else
            {
                await Delay(5000);
            }
        }

        private void RefreshWeather()
        {
            var random = new Random().Next(20);
            if (CurrentWeather is "RAIN" or "THUNDER")
            {
                CurrentWeather = "CLEARING";
            }
            else if (CurrentWeather == "CLEARING")
            {
                CurrentWeather = "CLOUDS";
            }
            else
            {
                CurrentWeather = random switch
                {
                    0 or 1 or 2 or 3 or 4 or 5 => CurrentWeather == "EXTRASUNNY" ? "CLEAR" : "EXTRASUNNY",
                    6 or 7 or 8 => CurrentWeather == "SMOG" ? "FOGGY" : "SMOG",
                    9 or 10 or 11 => CurrentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS",
                    12 or 13 or 14 => CurrentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS",
                    15 => CurrentWeather == "OVERCAST" ? "THUNDER" : "OVERCAST",
                    16 => CurrentWeather == "CLOUDS" ? "EXTRASUNNY" : "RAIN",
                    _ => CurrentWeather == "FOGGY" ? "SMOG" : "FOGGY",
                };
            }
        }

        private void UpdateWeather([FromSource] Player source, string newWeather, bool dynamicWeatherNew, bool enableSnow)
        {
            if (newWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
            {
                enableSnow = true;
            }

            CurrentWeather = newWeather;
            DynamicWeatherEnabled = dynamicWeatherNew;
            ManualSnowEnabled = enableSnow;
            lastWeatherChange = GetGameTimer();
        }

        private void UpdateBlackout([FromSource] Player source, bool value)
        {
            BlackoutEnabled = value;
        }

        private void UpdateVehicleBlackout([FromSource] Player source, bool value)
        {
            VehicleBlackoutEnabled = value;
        }

        private void UpdateWeatherCloudsType([FromSource] Player source, bool removeClouds)
        {
            if (removeClouds)
            {
                TriggerClientEvent("hb_worldsync:setClouds", 0f, "removed");
            }
            else
            {
                var opacity = float.Parse(new Random().NextDouble().ToString());
                var type = CloudTypes[new Random().Next(0, CloudTypes.Count)];
                TriggerClientEvent("hb_worldsync:setClouds", opacity, type);
            }
        }

        private void UpdateTime([FromSource] Player source, int newHours, int newMinutes)
        {
            CurrentHours = newHours;
            CurrentMinutes = newMinutes;
        }

        private void FreezeServerTimeHandler([FromSource] Player source, bool freezeTime)
        {
            FreezeTime = freezeTime;
        }

        private static bool GetBool(string name, bool fallback)
        {
            return (GetConvar(name, fallback ? "true" : "false") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetInt(string name, int fallback)
        {
            return int.TryParse(GetConvar(name, fallback.ToString()), out var value) ? value : fallback;
        }
    }
}
