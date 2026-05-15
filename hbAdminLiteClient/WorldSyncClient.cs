using System;
using System.Threading.Tasks;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace hbAdminLiteClient
{
    public class WorldSyncClient : BaseScript
    {
        private static bool IsSnowEnabled => GetBool("hb_worldsync_enable_snow", false);
        private static bool IsTimeSyncEnabled => GetBool("hb_worldsync_enable_time_sync", true);
        private static int GetServerMinutes => MathUtil.Clamp(GetInt("hb_worldsync_current_minute", 0), 0, 59);
        private static int GetServerHours => MathUtil.Clamp(GetInt("hb_worldsync_current_hour", 9), 0, 23);
        private static int GetServerMinuteDuration => GetInt("hb_worldsync_ingame_minute_duration", 2000);
        private static bool IsServerTimeFrozen => GetBool("hb_worldsync_freeze_time", false);
        private static bool IsServerTimeSyncedWithMachineTime => GetBool("hb_worldsync_sync_to_machine_time", false);
        private static string GetServerWeather => GetConvar("hb_worldsync_current_weather", "CLEAR");
        private static bool IsBlackoutEnabled => GetBool("hb_worldsync_blackout_enabled", false);
        private static bool IsVehicleLightsEnabled => GetBool("hb_worldsync_vehicle_blackout_enabled", false);
        private static int WeatherChangeTime => MathUtil.Clamp(GetInt("hb_worldsync_weather_change_duration", 10), 0, 45);

        private bool raceTimeLock;
        private int raceHour = 14;
        private int raceMinute;
        private bool wasRaceTimeLock;

        public WorldSyncClient()
        {
            if (!GetBool("hb_worldsync_enabled", true))
            {
                return;
            }

            EventHandlers.Add("hb_worldsync:setClouds", new Action<float, string>(SetClouds));
            if (GetBool("hb_worldsync_enable_weather_sync", true))
            {
                Tick += WeatherSync;
            }

            Tick += TimeSync;
        }

        [EventHandler("CS_race:SetLocalTimeLock")]
        private void SetLocalTimeLock(bool enabled, int hour, int minute)
        {
            raceTimeLock = enabled;
            raceHour = hour;
            raceMinute = minute;
        }

        private async Task UpdateWeatherParticles()
        {
            ForceSnowPass(IsSnowEnabled);
            SetForceVehicleTrails(IsSnowEnabled);
            SetForcePedFootstepsTracks(IsSnowEnabled);

            if (IsSnowEnabled)
            {
                if (!HasNamedPtfxAssetLoaded("core_snow"))
                {
                    RequestNamedPtfxAsset("core_snow");
                    while (!HasNamedPtfxAssetLoaded("core_snow"))
                    {
                        await Delay(0);
                    }
                }

                UseParticleFxAssetNextCall("core_snow");
            }
            else
            {
                RemoveNamedPtfxAsset("core_snow");
            }
        }

        private async Task WeatherSync()
        {
            await UpdateWeatherParticles();
            SetArtificialLightsState(IsBlackoutEnabled);
            SetArtificialLightsStateAffectsVehicles(!IsVehicleLightsEnabled);

            if (GetNextWeatherType() != GetHashKey(GetServerWeather))
            {
                SetWeatherTypeOvertimePersist(GetServerWeather, WeatherChangeTime);
                await Delay((WeatherChangeTime * 1000) + 2000);
                BaseScript.TriggerEvent("hb_worldsync:weatherChangeComplete", GetServerWeather);
            }

            await Delay(1000);
        }

        private async Task TimeSync()
        {
            if (raceTimeLock)
            {
                wasRaceTimeLock = true;
                NetworkOverrideClockTime(raceHour, raceMinute, 0);
                await Delay(0);
                return;
            }

            if (wasRaceTimeLock)
            {
                wasRaceTimeLock = false;

                if (IsTimeSyncEnabled)
                {
                    NetworkOverrideClockTime(GetServerHours, GetServerMinutes, 0);
                }
            }

            if (!IsTimeSyncEnabled)
            {
                await Delay(1000);
                return;
            }

            NetworkOverrideClockTime(GetServerHours, GetServerMinutes, 0);
            if (IsServerTimeFrozen || IsServerTimeSyncedWithMachineTime)
            {
                await Delay(1000);
            }
            else
            {
                await Delay(MathUtil.Clamp(GetServerMinuteDuration, 100, 2000));
            }
        }

        private void SetClouds(float opacity, string cloudsType)
        {
            if (opacity == 0f && cloudsType == "removed")
            {
                ClearCloudHat();
            }
            else
            {
                SetCloudHatOpacity(opacity);
                SetCloudHatTransition(cloudsType, 4f);
            }
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
