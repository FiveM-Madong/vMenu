using System;
using System.Threading.Tasks;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace hbAdminLiteClient
{
    public class DevEntitySpawner : BaseScript
    {
        public static bool Active { get; private set; }
        public static Entity CurrentEntity { get; private set; }
        public static string CurrentModelName { get; private set; } = "";

        private static float headingOffset;
        private const float RayDistance = 25f;
        private const float RotateSpeed = 20f;

        public DevEntitySpawner()
        {
            Tick += OnPlacementTick;
        }

        public static async void SpawnEntity(string modelName, Vector3 coords)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                AdminLiteClient.ShowFeed("모델명을 입력하세요.");
                return;
            }

            if (CurrentEntity != null || Active)
            {
                AdminLiteClient.ShowFeed("이미 배치 중인 엔티티가 있습니다.");
                return;
            }

            var model = (uint)GetHashKey(modelName);
            if (!IsModelValid(model))
            {
                AdminLiteClient.ShowFeed("유효하지 않은 모델명입니다.");
                return;
            }

            RequestModel(model);
            while (!HasModelLoaded(model))
            {
                await Delay(1);
            }

            int handle;
            if (IsModelAPed(model))
            {
                handle = CreatePed(4, model, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading, true, true);
            }
            else if (IsModelAVehicle(model))
            {
                handle = CreateVehicle(model, coords.X, coords.Y, coords.Z, Game.PlayerPed.Heading, true, true);
            }
            else
            {
                handle = CreateObject((int)model, coords.X, coords.Y, coords.Z, true, true, true);
            }

            if (handle == 0)
            {
                AdminLiteClient.ShowFeed("엔티티 생성에 실패했습니다.");
                return;
            }

            CurrentEntity = Entity.FromHandle(handle);
            if (CurrentEntity == null || !CurrentEntity.Exists())
            {
                AdminLiteClient.ShowFeed("엔티티 생성에 실패했습니다.");
                return;
            }

            SetEntityAsMissionEntity(handle, true, true);
            CurrentModelName = modelName;
            headingOffset = 0f;
            Active = true;
            AdminLiteClient.ShowFeed("엔티티 배치를 시작했습니다. 위치를 맞춘 뒤 M에서 확정/복제/취소하세요.");
        }

        public static async void FinishPlacement(bool duplicate = false)
        {
            if (CurrentEntity == null || !CurrentEntity.Exists())
            {
                AdminLiteClient.ShowFeed("확정할 엔티티가 없습니다.");
                return;
            }

            if (duplicate)
            {
                var hash = (uint)CurrentEntity.Model.Hash;
                var position = CurrentEntity.Position;
                var heading = CurrentEntity.Heading;
                var modelName = CurrentModelName;

                Active = false;
                CurrentEntity = null;
                CurrentModelName = "";
                await Delay(1);

                SpawnEntity(modelName, position);
                await Delay(1);
                if (CurrentEntity != null && CurrentEntity.Exists())
                {
                    CurrentEntity.Heading = heading;
                }
            }
            else
            {
                Active = false;
                CurrentEntity = null;
                CurrentModelName = "";
                AdminLiteClient.ShowFeed("엔티티 위치를 확정했습니다.");
            }
        }

        public static void CancelPlacement()
        {
            if (CurrentEntity != null && CurrentEntity.Exists())
            {
                var handle = CurrentEntity.Handle;
                SetEntityAsMissionEntity(handle, true, true);
                DeleteEntity(ref handle);
            }

            Active = false;
            CurrentEntity = null;
            CurrentModelName = "";
            AdminLiteClient.ShowFeed("엔티티 배치를 취소했습니다.");
        }

        private async Task OnPlacementTick()
        {
            if (!Active)
            {
                await Delay(1000);
                return;
            }

            if (CurrentEntity == null || !CurrentEntity.Exists())
            {
                Active = false;
                CurrentEntity = null;
                CurrentModelName = "";
                await Delay(1000);
                return;
            }

            var handle = CurrentEntity.Handle;
            var newPosition = GetCoordsPlayerIsLookingAt();

            FreezeEntityPosition(handle, true);
            SetEntityInvincible(handle, true);
            SetEntityCollision(handle, false, false);
            SetEntityAlpha(handle, (int)(255 * 0.4f), 0);
            CurrentEntity.Heading = (GetGameplayCamRot(0).Z + headingOffset) % 360f;
            CurrentEntity.Position = newPosition;

            if (CurrentEntity.HeightAboveGround < 3.0f)
            {
                if (CurrentEntity.Model.IsVehicle)
                {
                    SetVehicleOnGroundProperly(handle);
                }
                else if (!CurrentEntity.Model.IsPed)
                {
                    PlaceObjectOnGroundProperly(handle);
                }
            }

            if (Game.IsControlPressed(0, Control.VehicleFlyRollLeftOnly))
            {
                headingOffset += RotateSpeed * Game.LastFrameTime;
            }
            else if (Game.IsControlPressed(0, Control.VehicleFlyRollRightOnly))
            {
                headingOffset -= RotateSpeed * Game.LastFrameTime;
            }

            await Delay(0);

            FreezeEntityPosition(handle, false);
            SetEntityInvincible(handle, false);
            SetEntityCollision(handle, true, true);
            ResetEntityAlpha(handle);
        }

        private static Vector3 GetCoordsPlayerIsLookingAt()
        {
            var camRotation = GetGameplayCamRot(0);
            var camCoords = GetGameplayCamCoord();
            var direction = RotationToDirection(camRotation);

            var dest = new Vector3(
                camCoords.X + direction.X * RayDistance,
                camCoords.Y + direction.Y * RayDistance,
                camCoords.Z + direction.Z * RayDistance
            );

            var hit = World.Raycast(camCoords, dest, IntersectOptions.Everything, Game.PlayerPed);
            return hit.DitHit ? hit.HitPosition : dest;
        }

        private static Vector3 RotationToDirection(Vector3 rotation)
        {
            var adj = new Vector3(
                (float)Math.PI / 180f * rotation.X,
                (float)Math.PI / 180f * rotation.Y,
                (float)Math.PI / 180f * rotation.Z
            );

            return new Vector3(
                (float)(-Math.Sin(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)(Math.Cos(adj.Z) * Math.Abs(Math.Cos(adj.X))),
                (float)Math.Sin(adj.X)
            );
        }
    }
}
