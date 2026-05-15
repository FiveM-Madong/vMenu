using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using hbAdminLiteClient.data;

namespace hbAdminLiteClient
{
    public partial class AdminLiteClient : BaseScript
    {
        private void ApplyVehicleState()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                return;
            }

            var vehicle = Game.PlayerPed.CurrentVehicle;
            if (vehicle == null || !vehicle.Exists())
            {
                return;
            }

            if (vehicleKeepClean)
            {
                SetVehicleDirtLevel(vehicle.Handle, 0f);
            }

            if (vehicleEngineAlwaysOn)
            {
                SetVehicleEngineOn(vehicle.Handle, true, true, true);
            }

            if (vehicleInfiniteFuel)
            {
                SetVehicleFuelLevel(vehicle.Handle, 100f);
            }

            FreezeEntityPosition(vehicle.Handle, vehicleFrozen);
        }

        private void SetVehicleEngineState(Vehicle vehicle, bool enable)
        {
            if (vehicle == null || !vehicle.Exists())
            {
                return;
            }

            if (!enable)
            {
                vehicleEngineAlwaysOn = false;
            }

            SetVehicleUndriveable(vehicle.Handle, !enable);
            SetVehicleEngineOn(vehicle.Handle, enable, false, true);
        }


        private void ExecuteVehicleAction(string action)
        {
            var vehicle = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle : null;
            if (vehicle == null || !vehicle.Exists())
            {
                ShowFeed("차량에 탑승 중이 아닙니다.");
                return;
            }

            switch (action)
            {
                case "repair":
                    SetVehicleFixed(vehicle.Handle);
                    SetVehicleDirtLevel(vehicle.Handle, 0f);
                    ShowFeed("차량을 수리했습니다.");
                    break;
                case "clean":
                    SetVehicleDirtLevel(vehicle.Handle, 0f);
                    ShowFeed("차량을 세차했습니다.");
                    break;
                case "toggleKeepClean":
                    vehicleKeepClean = !vehicleKeepClean;
                    break;
                case "toggleEngineAlwaysOn":
                    vehicleEngineAlwaysOn = !vehicleEngineAlwaysOn;
                    if (vehicleEngineAlwaysOn)
                    {
                        SetVehicleEngineState(vehicle, true);
                    }
                    break;
                case "toggleInfiniteFuel":
                    vehicleInfiniteFuel = !vehicleInfiniteFuel;
                    break;
                case "toggleFreeze":
                    vehicleFrozen = !vehicleFrozen;
                    break;
                case "flip":
                    var pos = vehicle.Position;
                    SetEntityRotation(vehicle.Handle, 0f, 0f, vehicle.Rotation.Z, 2, true);
                    SetEntityCoordsNoOffset(vehicle.Handle, pos.X, pos.Y, pos.Z + 0.5f, false, false, false);
                    ShowFeed("차량 자세를 바로잡았습니다.");
                    break;
                case "delete":
                    var vehicleHandle = vehicle.Handle;
                    SetEntityAsMissionEntity(vehicleHandle, true, true);
                    DeleteVehicle(ref vehicleHandle);
                    ShowFeed("차량을 삭제했습니다.");
                    break;
                case "toggleEngine":
                    var enable = !GetIsVehicleEngineRunning(vehicle.Handle);
                    SetVehicleEngineState(vehicle, enable);
                    ShowFeed($"엔진 {(enable ? "켰음" : "끔")}");
                    break;
            }
        }

        private async void ExecuteSpawnerAction(string action)
        {
            await Task.FromResult(0);
        }

        private async void ExecutePersonalVehicleAction(string action)
        {
            switch (action)
            {
                case "saveCurrent":
                    if (!Game.PlayerPed.IsInVehicle() || Game.PlayerPed.CurrentVehicle == null || !Game.PlayerPed.CurrentVehicle.Exists())
                    {
                        ShowFeed("현재 탑승 중인 차량이 없습니다.");
                        return;
                    }

                    personalVehicleModel = Game.PlayerPed.CurrentVehicle.Model.Hash.ToString();
                    SetResourceKvp("hb_adminlite_personal_vehicle", personalVehicleModel);
                    ShowFeed("현재 차량을 개인 차량으로 저장했습니다.");
                    return;

                case "spawnSaved":
                    if (string.IsNullOrWhiteSpace(personalVehicleModel))
                    {
                        ShowFeed("저장한 개인 차량이 없습니다.");
                        return;
                    }

                    if (!await SpawnVehicleByHash(personalVehicleModel))
                    {
                        ShowFeed("개인 차량 스폰에 실패했습니다.");
                    }
                    return;

                case "clearSaved":
                    personalVehicleModel = "";
                    DeleteResourceKvp("hb_adminlite_personal_vehicle");
                    ShowFeed("개인 차량 저장을 삭제했습니다.");
                    return;
            }
        }







    }
}
