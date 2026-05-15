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
        private async Task HandleRealtimeMovement()
        {
            if (!noclipEnabled)
            {
                await Delay(0);
                return;
            }

            var noclipEntity = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle.Handle : Game.PlayerPed.Handle;

            FreezeEntityPosition(noclipEntity, true);
            SetEntityInvincible(noclipEntity, true);

            Vector3 newPos;
            Game.DisableControlThisFrame(0, Control.MoveUpOnly);
            Game.DisableControlThisFrame(0, Control.MoveUp);
            Game.DisableControlThisFrame(0, Control.MoveUpDown);
            Game.DisableControlThisFrame(0, Control.MoveDown);
            Game.DisableControlThisFrame(0, Control.MoveDownOnly);
            Game.DisableControlThisFrame(0, Control.MoveLeft);
            Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
            Game.DisableControlThisFrame(0, Control.MoveLeftRight);
            Game.DisableControlThisFrame(0, Control.MoveRight);
            Game.DisableControlThisFrame(0, Control.MoveRightOnly);
            Game.DisableControlThisFrame(0, Control.Cover);
            Game.DisableControlThisFrame(0, Control.MultiplayerInfo);
            Game.DisableControlThisFrame(0, Control.VehicleHeadlight);
            if (Game.PlayerPed.IsInVehicle())
            {
                Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
            }

            var yoff = 0.0f;
            var zoff = 0.0f;

            if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0 && !Game.IsPaused)
            {
                if (Game.IsControlJustPressed(0, Control.Sprint))
                {
                    noclipMovingSpeed++;
                    if (noclipMovingSpeed == 8)
                    {
                        noclipMovingSpeed = 0;
                    }
                }

                if (Game.IsDisabledControlPressed(0, Control.MoveUpOnly))
                {
                    yoff = 0.5f;
                }
                if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
                {
                    yoff = -0.5f;
                }
                if (!noclipFollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
                {
                    SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) + 3f);
                }
                if (!noclipFollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
                {
                    SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) - 3f);
                }
                if (Game.IsDisabledControlPressed(0, Control.Cover))
                {
                    zoff = 0.21f;
                }
                if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
                {
                    zoff = -0.21f;
                }
                if (Game.IsDisabledControlJustPressed(0, Control.VehicleHeadlight))
                {
                    noclipFollowCamMode = !noclipFollowCamMode;
                }
            }

            float moveSpeed = noclipMovingSpeed;
            if (noclipMovingSpeed > 4)
            {
                moveSpeed *= 1.8f;
            }
            moveSpeed = moveSpeed / (1f / GetFrameTime()) * 60;
            newPos = GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

            var heading = GetEntityHeading(noclipEntity);
            SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
            SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
            SetEntityHeading(noclipEntity, noclipFollowCamMode ? GetGameplayCamRelativeHeading() : heading);
            SetEntityCollision(noclipEntity, false, false);
            SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);

            SetEntityVisible(noclipEntity, false, false);
            SetLocalPlayerVisibleLocally(true);
            SetEntityAlpha(noclipEntity, (int)(255 * 0.2f), 0);

            SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, true);
            SetPoliceIgnorePlayer(Game.PlayerPed.Handle, true);

            await Delay(0);
        }

        private async Task DrawNoclipInstructionalButtons()
        {
            if (IsHudHidden())
            {
                return;
            }

            if (noclipScaleform == -1)
            {
                noclipScaleform = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
                while (!HasScaleformMovieLoaded(noclipScaleform))
                {
                    await Delay(0);
                }

                DrawScaleformMovieFullscreen(noclipScaleform, 255, 255, 255, 0, 0);
            }

            BeginScaleformMovieMethod(noclipScaleform, "CLEAR_ALL");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(0);
            PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
            PushScaleformMovieMethodParameterString($"Change Speed ({noclipSpeeds[noclipMovingSpeed]})");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(1);
            PushScaleformMovieMethodParameterString("~INPUT_MOVE_LR~");
            PushScaleformMovieMethodParameterString("Turn Left/Right");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(2);
            PushScaleformMovieMethodParameterString("~INPUT_MOVE_UD~");
            PushScaleformMovieMethodParameterString("Move");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(3);
            PushScaleformMovieMethodParameterString("~INPUT_MULTIPLAYER_INFO~");
            PushScaleformMovieMethodParameterString("Down");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(4);
            PushScaleformMovieMethodParameterString("~INPUT_COVER~");
            PushScaleformMovieMethodParameterString("Up");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(5);
            PushScaleformMovieMethodParameterString("~INPUT_VEH_HEADLIGHT~");
            PushScaleformMovieMethodParameterString("Cam Mode");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(6);
            PushScaleformMovieMethodParameterString("M");
            PushScaleformMovieMethodParameterString("Toggle NoClip");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(noclipScaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
            ScaleformMovieMethodAddParamInt(0);
            EndScaleformMovieMethod();

            DrawScaleformMovieFullscreen(noclipScaleform, 255, 255, 255, 255, 0);
        }

        private async void StartNoclipInstructionalLoop()
        {
            if (noclipInstructionalLoopActive)
            {
                return;
            }

            noclipInstructionalLoopActive = true;
            while (noclipEnabled)
            {
                await DrawNoclipInstructionalButtons();
                await Delay(0);
            }

            ReleaseNoclipScaleform();
            noclipInstructionalLoopActive = false;
        }

        private async void StartNoclipMovementLoop()
        {
            if (noclipMovementLoopActive)
            {
                return;
            }

            noclipMovementLoopActive = true;
            while (noclipEnabled && hasAccess)
            {
                await HandleRealtimeMovement();
            }

            noclipMovementLoopActive = false;
        }

        private void ReleaseNoclipScaleform()
        {
            if (noclipScaleform == -1)
            {
                return;
            }

            SetScaleformMovieAsNoLongerNeeded(ref noclipScaleform);
            noclipScaleform = -1;
        }

        private void ToggleNoclip()
        {
            if (!HasPermission("HBNoClip"))
            {
                ShowFeed("노클립 권한이 없습니다.");
                return;
            }

            if (!noclipEnabled && Game.PlayerPed.IsInVehicle())
            {
                var vehicle = Game.PlayerPed.CurrentVehicle;
                if (vehicle == null || !vehicle.Exists() || vehicle.Driver != Game.PlayerPed)
                {
                    noclipEnabled = false;
                    ShowFeed("차량 노클립은 운전석에서만 사용할 수 있습니다.");
                    return;
                }
            }

            noclipEnabled = !noclipEnabled;
            if (!noclipEnabled)
            {
                RestoreNoclipEntityState();
            }
            else
            {
                StartNoclipMovementLoop();
                StartNoclipInstructionalLoop();
            }

            ShowFeed($"노클립 {(noclipEnabled ? "활성화" : "비활성화")}");
        }

        private void RestoreNoclipEntityState()
        {
            ReleaseNoclipScaleform();
            var entity = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : Game.PlayerPed;
            if (entity == null || !entity.Exists())
            {
                return;
            }

            FreezeEntityPosition(entity.Handle, false);
            SetEntityInvincible(entity.Handle, entity.Handle == Game.PlayerPed.Handle && playerGodMode);
            SetEntityCollision(entity.Handle, true, true);
            ResetEntityAlpha(entity.Handle);
            SetEntityVisible(entity.Handle, entity.Handle != Game.PlayerPed.Handle || !playerInvisible, false);
            SetLocalPlayerVisibleLocally(true);
            SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, playerIgnored);
            SetPoliceIgnorePlayer(Game.PlayerPed.Handle, false);
        }

        private void RestoreLocalPlayerPedVisibility()
        {
            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists())
            {
                return;
            }

            ResetEntityAlpha(ped.Handle);
            SetEntityCollision(ped.Handle, true, true);
            SetEntityVisible(ped.Handle, !playerInvisible, false);
            SetLocalPlayerVisibleLocally(true);
        }

    }
}
