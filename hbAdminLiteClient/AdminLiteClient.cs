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
        private sealed class WeaponLoadoutIndexEntry
        {
            public string Key { get; set; }
            public string Name { get; set; }
        }

        private sealed class SavedPedIndexEntry
        {
            public string Key { get; set; }
            public string Name { get; set; }
        }

        private sealed class PedComponentSummary
        {
            public int Component { get; set; }
            public int Drawable { get; set; }
            public int DrawableCount { get; set; }
            public int Texture { get; set; }
            public int TextureCount { get; set; }
        }

        private sealed class PedPropSummary
        {
            public int Prop { get; set; }
            public int Drawable { get; set; }
            public int DrawableCount { get; set; }
            public int Texture { get; set; }
            public int TextureCount { get; set; }
            public bool IsClear { get; set; }
        }

        private sealed class WeaponLoadoutRecord
        {
            public string Name { get; set; }
            public List<WeaponLoadoutWeaponRecord> Weapons { get; set; }
        }

        private sealed class WeaponLoadoutWeaponRecord
        {
            public uint Hash { get; set; }
            public string Name { get; set; }
            public Dictionary<string, uint> Components { get; set; }
            public int Perm { get; set; }
            public string SpawnName { get; set; }
            public int CurrentAmmo { get; set; }
            public int CurrentTint { get; set; }
        }

        private bool hasAccess;
        private bool accessResolved;
        private bool permissionsResolved;
        private bool adminTicksRegistered;
        private readonly HashSet<string> grantedPermissions = new(StringComparer.OrdinalIgnoreCase);
        private bool menuOpen;
        private bool noclipEnabled;
        private int noclipMovingSpeed;
        private int noclipScaleform = -1;
        private bool noclipInstructionalLoopActive;
        private bool noclipMovementLoopActive;
        private bool noclipFollowCamMode = true;
        private bool showCoordinates;
        private bool showVehicleModelDimensions;
        private bool showPropModelDimensions;
        private bool showPedModelDimensions;
        private bool showEntityHandles;
        private bool showEntityModels;
        private bool showEntityNetOwners;
        private int entityDisplayRangeStep = 20;
        private bool timecycleEnabled;
        private int timecycleIndex;
        private int timecycleStrength = 20;

        private bool playerGodMode;
        private bool playerInvisible;
        private bool playerUnlimitedStamina;
        private bool playerFastRun;
        private bool playerFastSwim;
        private bool playerSuperJump;
        private bool playerNoRagdoll;
        private bool playerNeverWanted;
        private bool playerIgnored;
        private bool weaponUnlimitedAmmo;
        private bool weaponNoReload;
        private bool weaponAutoEquipParachutes;
        private bool weaponUnlimitedParachutes;
        private int parachuteSmokeColorIndex;
        private int parachutePrimaryStyleIndex;
        private int parachuteReserveStyleIndex;
        private bool weaponLoadoutsSetOnRespawn;
        private int playerWalkingStyleIndex;
        private int playerClothingGlowIndex;
        private readonly List<string> noclipSpeeds = new()
        {
            "Very Slow",
            "Slow",
            "Normal",
            "Fast",
            "Very Fast",
            "Extremely Fast",
            "Extremely Fast v2.0",
            "Max Speed"
        };
        private string pendingPedCatalogChunkSection;
        private int pendingPedCatalogChunkTotal;
        private Dictionary<int, string> pendingPedCatalogChunks = new();
        private int pendingVehicleCatalogChunkTotal;
        private Dictionary<int, string> pendingVehicleCatalogChunks = new();

        private bool vehicleKeepClean;
        private bool vehicleEngineAlwaysOn;
        private bool vehicleInfiniteFuel;
        private bool vehicleFrozen;
        private string personalVehicleModel = "";
        private string currentCategory = "root";
        private int selectedIndex;
        private bool stopVehiclesLoop;
        private bool stopPropsLoop;
        private bool stopPedsLoop;
        private List<Vehicle> debugVehicles = new();
        private List<Prop> debugProps = new();
        private List<Ped> debugPeds = new();

        private int lastToggleAt;
        private const float MaxEntityDisplayRange = 2000f;
        private static readonly string[] WalkingStyleKeys = { "Normal", "Injured", "Tough Guy", "Femme", "Gangster", "Posh", "Sexy", "Business", "Drunk", "Hipster" };
        private static readonly string[] WalkingStyleLabels = { "기본", "부상", "터프가이", "여성적", "갱스터", "고급스러움", "섹시", "비즈니스", "취함", "힙스터" };
        private static readonly string[] ClothingGlowLabels = { "켜짐", "꺼짐", "서서히", "점멸" };
        public AdminLiteClient()
        {
            EventHandlers.Add("hb_adminlite:setAccess", new Action<bool>(SetAccess));
            EventHandlers.Add("hb_adminlite:setPermissions", new Action<string>(SetPermissions));
            EventHandlers.Add("hb_adminlite:teleportTo", new Action<Vector3>(TeleportToCoords));
            EventHandlers.Add("hb_adminlite:clearArea", new Action<Vector3>(ClearAreaNearPos));
            EventHandlers.Add("hb_adminlite:killMe", new Action<string>(KillMe));
            EventHandlers.Add("playerSpawned", new Action<dynamic>(OnPlayerSpawned));

            RegisterCommand("+hbadmin", new Action<int, List<object>, string>((_, _, _) =>
            {
                var now = GetGameTimer();
                if (now - lastToggleAt < 250)
                {
                    return;
                }

                lastToggleAt = now;

                if (!hasAccess)
                {
                    ShowFeed("HB Admin Lite 접근 권한이 없습니다.");
                    return;
                }

                ToggleMenu();
            }), false);

            RegisterCommand("-hbadmin", new Action<int, List<object>, string>((_, _, _) => { }), false);
            RegisterKeyMapping("+hbadmin", "HB Admin Lite Toggle", "keyboard", GetConvar("hb_adminlite_menu_key", "M"));

            RegisterCommand("+hbadminnoclip", new Action<int, List<object>, string>((_, _, _) =>
            {
                if (!HasPermission("HBNoClip"))
                {
                    return;
                }

                ToggleNoclip();
            }), false);

            RegisterCommand("-hbadminnoclip", new Action<int, List<object>, string>((_, _, _) => { }), false);
            RegisterKeyMapping("+hbadminnoclip", "HB Admin Lite NoClip", "keyboard", GetConvar("hb_adminlite_noclip_key", "F2"));

            RegisterNuiCallbackType("close");
            RegisterNuiCallbackType("ready");
            RegisterNuiCallbackType("state");
            RegisterNuiCallbackType("devAction");
            RegisterNuiCallbackType("playerAction");
            RegisterNuiCallbackType("weaponAction");
            RegisterNuiCallbackType("weaponCatalog");
            RegisterNuiCallbackType("pedCatalog");
            RegisterNuiCallbackType("pedCatalogRequest");
            RegisterNuiCallbackType("weaponLoadoutAction");
            RegisterNuiCallbackType("weaponLoadoutCatalog");
            RegisterNuiCallbackType("savedPedCatalog");
            RegisterNuiCallbackType("pedCollectionCatalog");
            RegisterNuiCallbackType("vehicleAction");
            RegisterNuiCallbackType("spawnerAction");
            RegisterNuiCallbackType("spawnerCatalog");
            RegisterNuiCallbackType("personalVehicleAction");
            RegisterNuiCallbackType("inputMode");

            EventHandlers["__cfx_nui:close"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnClose);
            EventHandlers["__cfx_nui:ready"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnReady);
            EventHandlers["__cfx_nui:state"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnState);
            EventHandlers["__cfx_nui:devAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnDevAction);
            EventHandlers["__cfx_nui:playerAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnPlayerAction);
            EventHandlers["__cfx_nui:weaponAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnWeaponAction);
            EventHandlers["__cfx_nui:weaponCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnWeaponCatalog);
            EventHandlers["__cfx_nui:pedCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnPedCatalog);
            EventHandlers["__cfx_nui:pedCatalogRequest"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnPedCatalogRequest);
            EventHandlers["__cfx_nui:weaponLoadoutAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnWeaponLoadoutAction);
            EventHandlers["__cfx_nui:weaponLoadoutCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnWeaponLoadoutCatalog);
            EventHandlers["__cfx_nui:savedPedCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnSavedPedCatalog);
            EventHandlers["__cfx_nui:pedCollectionCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnPedCollectionCatalog);
            EventHandlers["__cfx_nui:vehicleAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnVehicleAction);
            EventHandlers["__cfx_nui:spawnerAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnSpawnerAction);
            EventHandlers["__cfx_nui:spawnerCatalog"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnSpawnerCatalog);
            EventHandlers["__cfx_nui:personalVehicleAction"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnPersonalVehicleAction);
            EventHandlers["__cfx_nui:inputMode"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnInputMode);
            EventHandlers["hb_adminlite:receivePedCatalog"] += new Action<string, string>(OnReceivePedCatalog);
            EventHandlers["hb_adminlite:receivePedCatalogStart"] += new Action<string, int>(OnReceivePedCatalogStart);
            EventHandlers["hb_adminlite:receivePedCatalogChunk"] += new Action<string, int, string>(OnReceivePedCatalogChunk);
            EventHandlers["hb_adminlite:receivePedCatalogEnd"] += new Action<string>(OnReceivePedCatalogEnd);
            EventHandlers["hb_adminlite:receiveVehicleCatalogStart"] += new Action<int>(OnReceiveVehicleCatalogStart);
            EventHandlers["hb_adminlite:receiveVehicleCatalogChunk"] += new Action<int, string>(OnReceiveVehicleCatalogChunk);
            EventHandlers["hb_adminlite:receiveVehicleCatalogEnd"] += new Action(OnReceiveVehicleCatalogEnd);

            personalVehicleModel = GetResourceKvpString("hb_adminlite_personal_vehicle") ?? "";
            weaponLoadoutsSetOnRespawn = (GetResourceKvpString("hb_adminlite_weapon_loadout_on_respawn") ?? "false") == "true";
            RequestAccess();
        }









        private object BuildUiStateWithVisibility()
        {
            return new
            {
                open = menuOpen,
                state = BuildUiStateSafe(),
            };
        }

        private object BuildNoAccessUiState()
        {
            return new
            {
                open = false,
                state = new
                {
                    hasAccess = false,
                    noclipEnabled = false,
                    coords = "",
                    showCoordinates = false,
                    showVehicleModelDimensions = false,
                    showPropModelDimensions = false,
                    showPedModelDimensions = false,
                    showEntityHandles = false,
                    showEntityModels = false,
                    showEntityNetOwners = false,
                    entityDisplayRangeStep = 20,
                    entityDisplayRange = 2000,
                    entitySpawnerActive = false,
                    entitySpawnerModel = "",
                    timecycleEnabled = false,
                    timecycleIndex = 0,
                    timecycleStrength = 20,
                    timecycleName = "default",
                    menuCategory = "root",
                    selectedAction = "",
                    playerGodMode = false,
                    playerInvisible = false,
                    playerUnlimitedStamina = false,
                    playerFastRun = false,
                    playerFastSwim = false,
                    playerSuperJump = false,
                    playerNoRagdoll = false,
                    playerNeverWanted = false,
                    playerIgnored = false,
                    inVehicle = false,
                    vehicleKeepClean = false,
                    vehicleEngineAlwaysOn = false,
                    vehicleInfiniteFuel = false,
                    vehicleFrozen = false,
                    vehicleEngineOn = false,
                    personalVehicleModel = "",
                }
            };
        }

    }
}




