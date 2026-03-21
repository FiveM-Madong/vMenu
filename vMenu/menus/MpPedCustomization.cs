using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuClient.MpPedDataManager;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class MpPedCustomization
    {
        // Variables
        private Menu menu;
        public Menu createCharacterMenu = new("캐릭터 생성", "새 캐릭터 생성");
        public Menu savedCharactersMenu = new("vMenu", "저장된 캐릭터 관리");
        public Menu savedCharactersCategoryMenu = new("Category", "실행 중 자동으로 갱신됩니다!");
        public Menu inheritanceMenu = new("vMenu", "캐릭터 유전 옵션");
        public Menu appearanceMenu = new("vMenu", "캐릭터 외형 옵션");
        public Menu faceShapeMenu = new("vMenu", "캐릭터 얼굴형 옵션");
        public Menu tattoosMenu = new("vMenu", "캐릭터 문신 옵션");
        public Menu clothesMenu = new("vMenu", "캐릭터 의상 옵션");
        public Menu propsMenu = new("vMenu", "캐릭터 소품 옵션");
        private readonly Menu manageSavedCharacterMenu = new("vMenu", "MP 캐릭터 관리");

        // Need to be able to disable/enable these buttons from another class.
        internal MenuItem createMaleBtn = new("남성 캐릭터 생성", "새 남성 캐릭터를 생성합니다.") { Label = "→→→" };
        internal MenuItem createFemaleBtn = new("여성 캐릭터 생성", "새 여성 캐릭터를 생성합니다.") { Label = "→→→" };
        internal MenuItem editPedBtn = new("저장된 캐릭터 수정", "저장된 캐릭터의 모든 항목을 수정할 수 있습니다. 저장 버튼을 누르면 변경 사항이 이 캐릭터의 저장 파일에 반영됩니다.");

        // Need to be editable from other functions
        private readonly MenuListItem setCategoryBtn = new("캐릭터 카테고리 설정", new List<string> { }, 0, "이 캐릭터의 카테고리를 설정합니다. 선택하면 저장됩니다.");
        private readonly MenuListItem categoryBtn = new("캐릭터 카테고리", new List<string> { }, 0, "이 캐릭터의 카테고리를 설정합니다.");

        public static bool DontCloseMenus { get { return MenuController.PreventExitingMenu; } set { MenuController.PreventExitingMenu = value; } }
        public static bool DisableBackButton { get { return MenuController.DisableBackButton; } set { MenuController.DisableBackButton = value; } }
        string selectedSavedCharacterManageName = "";
        private bool isEdidtingPed = false;
        private readonly List<string> facial_expressions = new() { "mood_Normal_1", "mood_Happy_1", "mood_Angry_1", "mood_Aiming_1", "mood_Injured_1", "mood_stressed_1", "mood_smug_1", "mood_sulk_1", };

        private readonly List<string> parents = [];
        private readonly List<float> mixValues = [0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f];
        private readonly List<float> faceFeaturesValuesList =
        [
            -1.0f,    // 0
            -0.9f,    // 1
            -0.8f,    // 2
            -0.7f,    // 3
            -0.6f,    // 4
            -0.5f,    // 5
            -0.4f,    // 6
            -0.3f,    // 7
            -0.2f,    // 8
            -0.1f,    // 9
            0.0f,    // 10
            0.1f,    // 11
            0.2f,    // 12
            0.3f,    // 13
            0.4f,    // 14
            0.5f,    // 15
            0.6f,    // 16
            0.7f,    // 17
            0.8f,    // 18
            0.9f,    // 19
            1.0f     // 20
        ];
        private readonly Dictionary<int, KeyValuePair<string, string>> hairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
        {
            { 0, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            { 1, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 2, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 3, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a") },
            { 4, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 5, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 6, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 7, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 8, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a") },
            { 9, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 10, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 11, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 12, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 13, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 14, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
            { 15, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
            { 16, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
            { 17, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            { 18, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a") },
            { 19, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a") },
            { 20, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a") },
            { 21, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a") },
            { 22, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
        };
        private readonly List<string> overlayColorsList = [];
        private readonly List<string> blemishesStyleList = [];
        private readonly List<string> beardStylesList = [];
        private readonly List<string> eyebrowsStyleList = [];
        private readonly List<string> ageingStyleList = [];
        private readonly List<string> makeupStyleList = [];
        private readonly List<string> blushStyleList = [];
        private readonly List<string> complexionStyleList = [];
        private readonly List<string> sunDamageStyleList = [];
        private readonly List<string> lipstickStyleList = [];
        private readonly List<string> molesFrecklesStyleList = [];
        private readonly List<string> chestHairStyleList = [];
        private readonly List<string> bodyBlemishesList = [];


        private readonly Random _random = new Random();
        private int _dadSelection;
        private int _mumSelection;
        private float _shapeMixValue;
        private float _skinMixValue;
        private readonly Dictionary<int, int> shapeFaceValues = [];
        // TODO: Chris: Replace with enums or something more sane - updating with index/magic numbers is nuts
        private readonly Dictionary<int, Tuple<int, int, float>> appearanceValues = [];
        private int _hairSelection;
        private int _hairColorSelection;
        private int _hairHighlightColorSelection;
        private int _eyeColorSelection;
        private int _facialExpressionSelection;

        private MultiplayerPedData currentCharacter = new();
        private MpCharacterCategory currentCategory = new();

        private Ped _clone;

        /// <summary>
        /// Makes or updates the character creator menu. Also has an option to load data from the <see cref="currentCharacter"/> data, to allow for editing an existing ped.
        /// </summary>
        /// <param name="male"></param>
        /// <param name="editPed"></param>
        private void MakeCreateCharacterMenu(bool male, bool editPed = false)
        {
            isEdidtingPed = editPed;
            if (!editPed)
            {
                currentCharacter = new MultiplayerPedData();
                currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
                currentCharacter.Version = 1;
                currentCharacter.ModelHash = male ? (uint)GetHashKey("mp_m_freemode_01") : (uint)GetHashKey("mp_f_freemode_01");
                currentCharacter.IsMale = male;

                // Places the sliders in the middle by default
                _shapeMixValue = 0.5f;
                _skinMixValue = 0.5f;

                SetPlayerClothing();
            }
            else
            {
                PedHeadBlendData headBlendData = currentCharacter.PedHeadBlendData;

                _dadSelection = headBlendData.FirstFaceShape;
                _mumSelection = headBlendData.SecondFaceShape;
                _shapeMixValue = headBlendData.ParentFaceShapePercent;
                _skinMixValue = headBlendData.ParentSkinTonePercent;

                if (_shapeMixValue > 1f)
                {
                    Log("얼굴 혼합 값이 최대값보다 크게 잘못 저장되어 최대값으로 초기화합니다.");
                    _shapeMixValue = 1f;
                }

                if (_skinMixValue > 1f)
                {
                    Log("피부 혼합 값이 최대값보다 크게 잘못 저장되어 최대값으로 초기화합니다.");
                    _skinMixValue = 1f;
                }
            }

            currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();
            currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();

            // Set the facial expression to default in case it doesn't exist yet, or keep the current one if it does.
            currentCharacter.FacialExpression ??= facial_expressions[0];

            // Set the facial expression on the ped itself.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);

            // Set the facial expression item list to the correct saved index.
            if (createCharacterMenu.GetMenuItems().ElementAt(6) is MenuListItem li)
            {
                var index = facial_expressions.IndexOf(currentCharacter.FacialExpression ?? facial_expressions[0]);
                if (index < 0)
                {
                    index = 0;
                }
                li.ListIndex = index;
            }

            appearanceMenu.ClearMenuItems();
            tattoosMenu.ClearMenuItems();
            clothesMenu.ClearMenuItems();
            propsMenu.ClearMenuItems();

            #region appearance menu.
            if (!editPed)
            {
                // Clears any saved appearance values from prior peds
                _hairSelection = 0;
                _hairColorSelection = 0;
                _hairHighlightColorSelection = 0;
                _eyeColorSelection = 0;

                for (int i = 0; i < 12; i++)
                {
                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                }
            }
            else
            {
                PedAppearance appearanceData = currentCharacter.PedAppearance;

                _hairSelection = appearanceData.hairStyle;
                _hairColorSelection = appearanceData.hairColor;
                _hairHighlightColorSelection = appearanceData.hairHighlightColor;

                appearanceValues[0] = new(appearanceData.blemishesStyle, 0, appearanceData.blemishesOpacity);
                appearanceValues[1] = new(appearanceData.beardStyle, appearanceData.beardColor, appearanceData.beardOpacity);
                appearanceValues[2] = new(appearanceData.eyebrowsStyle, appearanceData.eyebrowsColor, appearanceData.eyebrowsOpacity);
                appearanceValues[3] = new(appearanceData.ageingStyle, 0, appearanceData.ageingOpacity);
                appearanceValues[4] = new(appearanceData.makeupStyle, appearanceData.makeupColor, appearanceData.makeupOpacity);
                appearanceValues[5] = new(appearanceData.blushStyle, appearanceData.blushColor, appearanceData.blushOpacity);
                appearanceValues[6] = new(appearanceData.complexionStyle, 0, appearanceData.complexionOpacity);
                appearanceValues[7] = new(appearanceData.sunDamageStyle, 0, appearanceData.sunDamageOpacity);
                appearanceValues[8] = new(appearanceData.lipstickStyle, appearanceData.lipstickColor, appearanceData.lipstickOpacity);
                appearanceValues[9] = new(appearanceData.molesFrecklesStyle, 0, appearanceData.molesFrecklesOpacity);
                appearanceValues[10] = new(appearanceData.chestHairStyle, appearanceData.chestHairColor, appearanceData.chestHairOpacity);
                appearanceValues[11] = new(appearanceData.bodyBlemishesStyle, 0, appearanceData.bodyBlemishesOpacity);

                _eyeColorSelection = appearanceData.eyeColor;
            }

            var opacity = new List<string>() { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

            var maxHairStyles = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2);
            //if (currentCharacter.ModelHash == (uint)PedHash.FreemodeFemale01)
            //{
            //    maxHairStyles /= 2;
            //}
            var hairStylesList = new List<string>();
            for (var i = 0; i < maxHairStyles; i++)
            {
                hairStylesList.Add($"스타일 #{i + 1}");
            }
            hairStylesList.Add($"스타일 #{maxHairStyles + 1}");

            var eyeColorList = new List<string>();
            for (var i = 0; i < 32; i++)
            {
                eyeColorList.Add($"눈 색상 #{i + 1}");
            }

            /*

            0               Blemishes             0 - 23,   255  
            1               Facial Hair           0 - 28,   255  
            2               Eyebrows              0 - 33,   255  
            3               Ageing                0 - 14,   255  
            4               Makeup                0 - 74,   255  
            5               Blush                 0 - 6,    255  
            6               Complexion            0 - 11,   255  
            7               Sun Damage            0 - 10,   255  
            8               Lipstick              0 - 9,    255  
            9               Moles/Freckles        0 - 17,   255  
            10              Chest Hair            0 - 16,   255  
            11              Body Blemishes        0 - 11,   255  
            12              Add Body Blemishes    0 - 1,    255  
            
            */


            // hair
            var currentHairStyle = editPed ? currentCharacter.PedAppearance.hairStyle : GetPedDrawableVariation(Game.PlayerPed.Handle, 2);
            var currentHairColor = editPed ? currentCharacter.PedAppearance.hairColor : 0;
            var currentHairHighlightColor = editPed ? currentCharacter.PedAppearance.hairHighlightColor : 0;

            // 0 blemishes
            var currentBlemishesStyle = editPed ? currentCharacter.PedAppearance.blemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) : 0;
            var currentBlemishesOpacity = editPed ? currentCharacter.PedAppearance.blemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, currentBlemishesStyle, currentBlemishesOpacity);

            // 1 beard
            var currentBeardStyle = editPed ? currentCharacter.PedAppearance.beardStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) : 0;
            var currentBeardOpacity = editPed ? currentCharacter.PedAppearance.beardOpacity : 0f;
            var currentBeardColor = editPed ? currentCharacter.PedAppearance.beardColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, currentBeardStyle, currentBeardOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, currentBeardColor, currentBeardColor);

            // 2 eyebrows
            var currentEyebrowStyle = editPed ? currentCharacter.PedAppearance.eyebrowsStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) : 0;
            var currentEyebrowOpacity = editPed ? currentCharacter.PedAppearance.eyebrowsOpacity : 0f;
            var currentEyebrowColor = editPed ? currentCharacter.PedAppearance.eyebrowsColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, currentEyebrowStyle, currentEyebrowOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, currentEyebrowColor, currentEyebrowColor);

            // 3 ageing
            var currentAgeingStyle = editPed ? currentCharacter.PedAppearance.ageingStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) : 0;
            var currentAgeingOpacity = editPed ? currentCharacter.PedAppearance.ageingOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, currentAgeingStyle, currentAgeingOpacity);

            // 4 makeup
            var currentMakeupStyle = editPed ? currentCharacter.PedAppearance.makeupStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) : 0;
            var currentMakeupOpacity = editPed ? currentCharacter.PedAppearance.makeupOpacity : 0f;
            var currentMakeupColor = editPed ? currentCharacter.PedAppearance.makeupColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, currentMakeupStyle, currentMakeupOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, currentMakeupColor, currentMakeupColor);

            // 5 blush
            var currentBlushStyle = editPed ? currentCharacter.PedAppearance.blushStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) : 0;
            var currentBlushOpacity = editPed ? currentCharacter.PedAppearance.blushOpacity : 0f;
            var currentBlushColor = editPed ? currentCharacter.PedAppearance.blushColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, currentBlushStyle, currentBlushOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, currentBlushColor, currentBlushColor);

            // 6 complexion
            var currentComplexionStyle = editPed ? currentCharacter.PedAppearance.complexionStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) : 0;
            var currentComplexionOpacity = editPed ? currentCharacter.PedAppearance.complexionOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, currentComplexionStyle, currentComplexionOpacity);

            // 7 sun damage
            var currentSunDamageStyle = editPed ? currentCharacter.PedAppearance.sunDamageStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) : 0;
            var currentSunDamageOpacity = editPed ? currentCharacter.PedAppearance.sunDamageOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, currentSunDamageStyle, currentSunDamageOpacity);

            // 8 lipstick
            var currentLipstickStyle = editPed ? currentCharacter.PedAppearance.lipstickStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) : 0;
            var currentLipstickOpacity = editPed ? currentCharacter.PedAppearance.lipstickOpacity : 0f;
            var currentLipstickColor = editPed ? currentCharacter.PedAppearance.lipstickColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, currentLipstickStyle, currentLipstickOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, currentLipstickColor, currentLipstickColor);

            // 9 moles/freckles
            var currentMolesFrecklesStyle = editPed ? currentCharacter.PedAppearance.molesFrecklesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) : 0;
            var currentMolesFrecklesOpacity = editPed ? currentCharacter.PedAppearance.molesFrecklesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, currentMolesFrecklesStyle, currentMolesFrecklesOpacity);

            // 10 chest hair
            var currentChesthairStyle = editPed ? currentCharacter.PedAppearance.chestHairStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) : 0;
            var currentChesthairOpacity = editPed ? currentCharacter.PedAppearance.chestHairOpacity : 0f;
            var currentChesthairColor = editPed ? currentCharacter.PedAppearance.chestHairColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, currentChesthairStyle, currentChesthairOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, currentChesthairColor, currentChesthairColor);

            // 11 body blemishes
            var currentBodyBlemishesStyle = editPed ? currentCharacter.PedAppearance.bodyBlemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) : 0;
            var currentBodyBlemishesOpacity = editPed ? currentCharacter.PedAppearance.bodyBlemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, currentBodyBlemishesStyle, currentBodyBlemishesOpacity);

            var currentEyeColor = editPed ? currentCharacter.PedAppearance.eyeColor : 0;
            SetPedEyeColor(Game.PlayerPed.Handle, currentEyeColor);

            var hairStyles = new MenuListItem("헤어 스타일", hairStylesList, currentHairStyle, "헤어 스타일을 선택하세요.");
            //MenuListItem hairColors = new MenuListItem("머리 색상", overlayColorsList, currentHairColor, "머리 색상을 선택하세요.");
            var hairColors = new MenuListItem("머리 색상", overlayColorsList, currentHairColor, "머리 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuListItem hairHighlightColors = new MenuListItem("머리 하이라이트 색상", overlayColorsList, currentHairHighlightColor, "머리 하이라이트 색상을 선택하세요.");
            var hairHighlightColors = new MenuListItem("머리 하이라이트 색상", overlayColorsList, currentHairHighlightColor, "머리 하이라이트 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };

            var blemishesStyle = new MenuListItem("잡티 스타일", blemishesStyleList, currentBlemishesStyle, "잡티 스타일을 선택하세요.");
            //MenuSliderItem blemishesOpacity = new MenuSliderItem("잡티 투명도", "잡티 투명도를 선택하세요.", 0, 10, (int)(currentBlemishesOpacity * 10f), false);
            var blemishesOpacity = new MenuListItem("잡티 투명도", opacity, (int)(currentBlemishesOpacity * 10f), "잡티 투명도를 선택하세요.") { ShowOpacityPanel = true };

            var beardStyles = new MenuListItem("수염 스타일", beardStylesList, currentBeardStyle, "수염/얼굴 털 스타일을 선택하세요.");
            var beardOpacity = new MenuListItem("수염 투명도", opacity, (int)(currentBeardOpacity * 10f), "수염/얼굴 털의 투명도를 선택하세요.") { ShowOpacityPanel = true };
            var beardColor = new MenuListItem("수염 색상", overlayColorsList, currentBeardColor, "수염 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuSliderItem beardOpacity = new MenuSliderItem("수염 투명도", "수염/얼굴 털의 투명도를 선택하세요.", 0, 10, (int)(currentBeardOpacity * 10f), false);
            //MenuListItem beardColor = new MenuListItem("수염 색상", overlayColorsList, currentBeardColor, "수염 색상을 선택하세요");

            var eyebrowStyle = new MenuListItem("눈썹 스타일", eyebrowsStyleList, currentEyebrowStyle, "눈썹 스타일을 선택하세요.");
            var eyebrowOpacity = new MenuListItem("눈썹 투명도", opacity, (int)(currentEyebrowOpacity * 10f), "눈썹의 투명도를 선택하세요.") { ShowOpacityPanel = true };
            var eyebrowColor = new MenuListItem("눈썹 색상", overlayColorsList, currentEyebrowColor, "눈썹 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };
            //MenuSliderItem eyebrowOpacity = new MenuSliderItem("눈썹 투명도", "눈썹의 투명도를 선택하세요.", 0, 10, (int)(currentEyebrowOpacity * 10f), false);

            var ageingStyle = new MenuListItem("노화 스타일", ageingStyleList, currentAgeingStyle, "노화 스타일을 선택하세요.");
            var ageingOpacity = new MenuListItem("노화 투명도", opacity, (int)(currentAgeingOpacity * 10f), "노화 투명도를 선택하세요.") { ShowOpacityPanel = true };
            //MenuSliderItem ageingOpacity = new MenuSliderItem("노화 투명도", "노화 투명도를 선택하세요.", 0, 10, (int)(currentAgeingOpacity * 10f), false);

            var makeupStyle = new MenuListItem("메이크업 스타일", makeupStyleList, currentMakeupStyle, "메이크업 스타일을 선택하세요.");
            var makeupOpacity = new MenuListItem("메이크업 투명도", opacity, (int)(currentMakeupOpacity * 10f), "메이크업 투명도를 선택하세요") { ShowOpacityPanel = true };
            //MenuSliderItem makeupOpacity = new MenuSliderItem("메이크업 투명도", 0, 10, (int)(currentMakeupOpacity * 10f), "메이크업 투명도를 선택하세요.");
            var makeupColor = new MenuListItem("메이크업 색상", overlayColorsList, currentMakeupColor, "메이크업 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var blushStyle = new MenuListItem("블러셔 스타일", blushStyleList, currentBlushStyle, "블러셔 스타일을 선택하세요.");
            var blushOpacity = new MenuListItem("블러셔 투명도", opacity, (int)(currentBlushOpacity * 10f), "블러셔 투명도를 선택하세요.") { ShowOpacityPanel = true };
            //MenuSliderItem blushOpacity = new MenuSliderItem("블러셔 투명도", 0, 10, (int)(currentBlushOpacity * 10f), "블러셔 투명도를 선택하세요.");
            var blushColor = new MenuListItem("블러셔 색상", overlayColorsList, currentBlushColor, "블러셔 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var complexionStyle = new MenuListItem("피부결 스타일", complexionStyleList, currentComplexionStyle, "피부결 스타일을 선택하세요.");
            //MenuSliderItem complexionOpacity = new MenuSliderItem("피부결 투명도", 0, 10, (int)(currentComplexionOpacity * 10f), "피부결 투명도를 선택하세요.");
            var complexionOpacity = new MenuListItem("피부결 투명도", opacity, (int)(currentComplexionOpacity * 10f), "피부결 투명도를 선택하세요.") { ShowOpacityPanel = true };

            var sunDamageStyle = new MenuListItem("햇빛 손상 스타일", sunDamageStyleList, currentSunDamageStyle, "햇빛 손상 스타일을 선택하세요.");
            //MenuSliderItem sunDamageOpacity = new MenuSliderItem("햇빛 손상 투명도", 0, 10, (int)(currentSunDamageOpacity * 10f), "햇빛 손상 투명도를 선택하세요.");
            var sunDamageOpacity = new MenuListItem("햇빛 손상 투명도", opacity, (int)(currentSunDamageOpacity * 10f), "햇빛 손상 투명도를 선택하세요.") { ShowOpacityPanel = true };

            var lipstickStyle = new MenuListItem("립스틱 스타일", lipstickStyleList, currentLipstickStyle, "립스틱 스타일을 선택하세요.");
            //MenuSliderItem lipstickOpacity = new MenuSliderItem("립스틱 투명도", 0, 10, (int)(currentLipstickOpacity * 10f), "립스틱 투명도를 선택하세요.");
            var lipstickOpacity = new MenuListItem("립스틱 투명도", opacity, (int)(currentLipstickOpacity * 10f), "립스틱 투명도를 선택하세요.") { ShowOpacityPanel = true };
            var lipstickColor = new MenuListItem("립스틱 색상", overlayColorsList, currentLipstickColor, "립스틱 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Makeup };

            var molesFrecklesStyle = new MenuListItem("점/주근깨 스타일", molesFrecklesStyleList, currentMolesFrecklesStyle, "점/주근깨 스타일을 선택하세요.");
            //MenuSliderItem molesFrecklesOpacity = new MenuSliderItem("점/주근깨 투명도", 0, 10, (int)(currentMolesFrecklesOpacity * 10f), "점/주근깨 투명도를 선택하세요.");
            var molesFrecklesOpacity = new MenuListItem("점/주근깨 투명도", opacity, (int)(currentMolesFrecklesOpacity * 10f), "점/주근깨 투명도를 선택하세요.") { ShowOpacityPanel = true };

            var chestHairStyle = new MenuListItem("가슴 털 스타일", chestHairStyleList, currentChesthairStyle, "가슴 털 스타일을 선택하세요.");
            //MenuSliderItem chestHairOpacity = new MenuSliderItem("가슴 털 투명도", 0, 10, (int)(currentChesthairOpacity * 10f), "가슴 털 투명도를 선택하세요.");
            var chestHairOpacity = new MenuListItem("가슴 털 투명도", opacity, (int)(currentChesthairOpacity * 10f), "가슴 털 투명도를 선택하세요.") { ShowOpacityPanel = true };
            var chestHairColor = new MenuListItem("가슴 털 색상", overlayColorsList, currentChesthairColor, "가슴 털 색상을 선택하세요.") { ShowColorPanel = true, ColorPanelColorType = MenuListItem.ColorPanelType.Hair };

            // Body blemishes
            var bodyBlemishesStyle = new MenuListItem("몸 잡티 스타일", bodyBlemishesList, currentBodyBlemishesStyle, "몸 잡티 스타일을 선택하세요.");
            var bodyBlemishesOpacity = new MenuListItem("몸 잡티 투명도", opacity, (int)(currentBodyBlemishesOpacity * 10f), "몸 잡티 투명도를 선택하세요.") { ShowOpacityPanel = true };

            var eyeColor = new MenuListItem("눈 색상", eyeColorList, currentEyeColor, "눈/렌즈 색상을 선택하세요.");

            appearanceMenu.AddMenuItem(hairStyles);
            appearanceMenu.AddMenuItem(hairColors);
            appearanceMenu.AddMenuItem(hairHighlightColors);

            appearanceMenu.AddMenuItem(blemishesStyle);
            appearanceMenu.AddMenuItem(blemishesOpacity);

            appearanceMenu.AddMenuItem(beardStyles);
            appearanceMenu.AddMenuItem(beardOpacity);
            appearanceMenu.AddMenuItem(beardColor);

            appearanceMenu.AddMenuItem(eyebrowStyle);
            appearanceMenu.AddMenuItem(eyebrowOpacity);
            appearanceMenu.AddMenuItem(eyebrowColor);

            appearanceMenu.AddMenuItem(ageingStyle);
            appearanceMenu.AddMenuItem(ageingOpacity);

            appearanceMenu.AddMenuItem(makeupStyle);
            appearanceMenu.AddMenuItem(makeupOpacity);
            appearanceMenu.AddMenuItem(makeupColor);

            appearanceMenu.AddMenuItem(blushStyle);
            appearanceMenu.AddMenuItem(blushOpacity);
            appearanceMenu.AddMenuItem(blushColor);

            appearanceMenu.AddMenuItem(complexionStyle);
            appearanceMenu.AddMenuItem(complexionOpacity);

            appearanceMenu.AddMenuItem(sunDamageStyle);
            appearanceMenu.AddMenuItem(sunDamageOpacity);

            appearanceMenu.AddMenuItem(lipstickStyle);
            appearanceMenu.AddMenuItem(lipstickOpacity);
            appearanceMenu.AddMenuItem(lipstickColor);

            appearanceMenu.AddMenuItem(molesFrecklesStyle);
            appearanceMenu.AddMenuItem(molesFrecklesOpacity);

            appearanceMenu.AddMenuItem(chestHairStyle);
            appearanceMenu.AddMenuItem(chestHairOpacity);
            appearanceMenu.AddMenuItem(chestHairColor);

            appearanceMenu.AddMenuItem(bodyBlemishesStyle);
            appearanceMenu.AddMenuItem(bodyBlemishesOpacity);

            appearanceMenu.AddMenuItem(eyeColor);

            if (male)
            {
                // There are weird people out there that wanted makeup for male characters
                // so yeah.... here you go I suppose... strange...

                /*
                makeupStyle.Enabled = false;
                makeupStyle.LeftIcon = MenuItem.Icon.LOCK;
                makeupStyle.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                makeupOpacity.Enabled = false;
                makeupOpacity.LeftIcon = MenuItem.Icon.LOCK;
                makeupOpacity.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                makeupColor.Enabled = false;
                makeupColor.LeftIcon = MenuItem.Icon.LOCK;
                makeupColor.Description = "남성 캐릭터에서는 사용할 수 없습니다.";


                blushStyle.Enabled = false;
                blushStyle.LeftIcon = MenuItem.Icon.LOCK;
                blushStyle.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                blushOpacity.Enabled = false;
                blushOpacity.LeftIcon = MenuItem.Icon.LOCK;
                blushOpacity.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                blushColor.Enabled = false;
                blushColor.LeftIcon = MenuItem.Icon.LOCK;
                blushColor.Description = "남성 캐릭터에서는 사용할 수 없습니다.";


                lipstickStyle.Enabled = false;
                lipstickStyle.LeftIcon = MenuItem.Icon.LOCK;
                lipstickStyle.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                lipstickOpacity.Enabled = false;
                lipstickOpacity.LeftIcon = MenuItem.Icon.LOCK;
                lipstickOpacity.Description = "남성 캐릭터에서는 사용할 수 없습니다.";

                lipstickColor.Enabled = false;
                lipstickColor.LeftIcon = MenuItem.Icon.LOCK;
                lipstickColor.Description = "남성 캐릭터에서는 사용할 수 없습니다.";
                */
            }
            else
            {
                beardStyles.Enabled = false;
                beardStyles.LeftIcon = MenuItem.Icon.LOCK;
                beardStyles.Description = "여성 캐릭터에서는 사용할 수 없습니다.";

                beardOpacity.Enabled = false;
                beardOpacity.LeftIcon = MenuItem.Icon.LOCK;
                beardOpacity.Description = "여성 캐릭터에서는 사용할 수 없습니다.";

                beardColor.Enabled = false;
                beardColor.LeftIcon = MenuItem.Icon.LOCK;
                beardColor.Description = "여성 캐릭터에서는 사용할 수 없습니다.";


                chestHairStyle.Enabled = false;
                chestHairStyle.LeftIcon = MenuItem.Icon.LOCK;
                chestHairStyle.Description = "여성 캐릭터에서는 사용할 수 없습니다.";

                chestHairOpacity.Enabled = false;
                chestHairOpacity.LeftIcon = MenuItem.Icon.LOCK;
                chestHairOpacity.Description = "여성 캐릭터에서는 사용할 수 없습니다.";

                chestHairColor.Enabled = false;
                chestHairColor.LeftIcon = MenuItem.Icon.LOCK;
                chestHairColor.Description = "여성 캐릭터에서는 사용할 수 없습니다.";
            }

            #endregion

            #region clothing options menu
            var clothingCategoryNames = new string[12] { "미사용(머리)", "마스크", "미사용(머리카락)", "상체", "하체", "가방 및 낙하산", "신발", "스카프 및 체인", "셔츠 및 액세서리", "방탄복 및 액세서리 2", "배지 및 로고", "셔츠 오버레이 및 재킷" };
            for (var i = 0; i < 12; i++)
            {
                if (i is not 0 and not 2)
                {
                    var currentVariationIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Key : GetPedDrawableVariation(Game.PlayerPed.Handle, i);
                    var currentVariationTextureIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Value : GetPedTextureVariation(Game.PlayerPed.Handle, i);

                    var maxDrawables = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, i);

                    var items = new List<string>();
                    for (var x = 0; x < maxDrawables; x++)
                    {
                        items.Add($"드로어블 #{x} / 총 {maxDrawables}개");
                    }

                    var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, i, currentVariationIndex);

                    var listItem = new MenuListItem(clothingCategoryNames[i], items, currentVariationIndex, $"방향키로 드로어블을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{currentVariationTextureIndex + 1} / 총 {maxTextures}개.");
                    clothesMenu.AddMenuItem(listItem);
                }
            }
            #endregion

            #region props options menu
            var propNames = new string[5] { "모자 및 헬멧", "안경", "기타 소품", "시계", "팔찌" };
            for (var x = 0; x < 5; x++)
            {
                var propId = x;
                if (x > 2)
                {
                    propId += 3;
                }

                var currentProp = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Key : GetPedPropIndex(Game.PlayerPed.Handle, propId);
                var currentPropTexture = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Value : GetPedPropTextureIndex(Game.PlayerPed.Handle, propId);

                var propsList = new List<string>();
                for (var i = 0; i < GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId); i++)
                {
                    propsList.Add($"소품 #{i} / 총 {GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId)}개");
                }
                propsList.Add("소품 없음");


                if (GetPedPropIndex(Game.PlayerPed.Handle, propId) != -1)
                {
                    var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propId, currentProp);
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{currentPropTexture + 1} / 총 {maxPropTextures}개.");
                    propsMenu.AddMenuItem(propListItem);
                }
                else
                {
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, "방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다.");
                    propsMenu.AddMenuItem(propListItem);
                }


            }
            #endregion

            #region face features menu
            foreach (MenuSliderItem item in faceShapeMenu.GetMenuItems())
            {
                if (editPed)
                {
                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                    }
                    else
                    {
                        if (currentCharacter.FaceShapeFeatures.features.ContainsKey(item.Index))
                        {
                            item.Position = (int)(currentCharacter.FaceShapeFeatures.features[item.Index] * 10f) + 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, currentCharacter.FaceShapeFeatures.features[item.Index]);
                        }
                        else
                        {
                            item.Position = 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                        }
                    }
                }
                else
                {
                    item.Position = 10;
                    SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                }
            }
            #endregion

            #region Tattoos menu
            var headTattoosList = new List<string>();
            var torsoTattoosList = new List<string>();
            var leftArmTattoosList = new List<string>();
            var rightArmTattoosList = new List<string>();
            var leftLegTattoosList = new List<string>();
            var rightLegTattoosList = new List<string>();
            var badgeTattoosList = new List<string>();

            TattoosData.GenerateTattoosData();
            if (male)
            {
                var counter = 1;
                foreach (var tattoo in MaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.HEAD.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.TORSO.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.LEFT_ARM.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.RIGHT_ARM.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.LEFT_LEG.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"문신 #{counter} / 총 {MaleTattoosCollection.RIGHT_LEG.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"배지 #{counter} / 총 {MaleTattoosCollection.BADGES.Count}개");
                    counter++;
                }
            }
            else
            {
                var counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.HEAD.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.TORSO.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.LEFT_ARM.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.RIGHT_ARM.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.LEFT_LEG.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"문신 #{counter} / 총 {FemaleTattoosCollection.RIGHT_LEG.Count}개");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"배지 #{counter} / 총 {FemaleTattoosCollection.BADGES.Count}개");
                    counter++;
                }
            }

            const string tatDesc = "목록을 넘기며 문신을 미리 볼 수 있습니다. 마음에 들면 Enter를 눌러 선택하세요. 현재 없으면 추가되고, 이미 있으면 제거됩니다.";
            var headTatts = new MenuListItem("머리 문신", headTattoosList, 0, tatDesc);
            var torsoTatts = new MenuListItem("몸통 문신", torsoTattoosList, 0, tatDesc);
            var leftArmTatts = new MenuListItem("왼팔 문신", leftArmTattoosList, 0, tatDesc);
            var rightArmTatts = new MenuListItem("오른팔 문신", rightArmTattoosList, 0, tatDesc);
            var leftLegTatts = new MenuListItem("왼다리 문신", leftLegTattoosList, 0, tatDesc);
            var rightLegTatts = new MenuListItem("오른다리 문신", rightLegTattoosList, 0, tatDesc);
            var badgeTatts = new MenuListItem("배지 오버레이", badgeTattoosList, 0, tatDesc);

            tattoosMenu.AddMenuItem(headTatts);
            tattoosMenu.AddMenuItem(torsoTatts);
            tattoosMenu.AddMenuItem(leftArmTatts);
            tattoosMenu.AddMenuItem(rightArmTatts);
            tattoosMenu.AddMenuItem(leftLegTatts);
            tattoosMenu.AddMenuItem(rightLegTatts);
            tattoosMenu.AddMenuItem(badgeTatts);
            tattoosMenu.AddMenuItem(new MenuItem("모든 문신 제거", "모든 문신을 지우고 처음부터 다시 시작하려면 클릭하세요."));
            #endregion

            List<string> categoryNames = GetAllCategoryNames();

            categoryNames.RemoveAt(0);

            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);

            categoryBtn.ItemData = new Tuple<List<string>, List<MenuItem.Icon>>(categoryNames, categoryIcons);
            categoryBtn.ListItems = categoryNames;

            if (editPed)
            {
                int characterCategoryIndex = categoryNames.IndexOf(currentCharacter.Category);

                categoryBtn.ListIndex = characterCategoryIndex;
            }
            else
            {
                categoryBtn.ListIndex = 0;
            }

            categoryBtn.RightIcon = categoryIcons[categoryBtn.ListIndex];

            createCharacterMenu.RefreshIndex();
            appearanceMenu.RefreshIndex();
            inheritanceMenu.RefreshIndex();
            tattoosMenu.RefreshIndex();
        }

        /// <summary>
        /// Saves the mp character and quits the editor if successful.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SavePed()
        {
            currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
            if (isEdidtingPed)
            {
                var json = JsonConvert.SerializeObject(currentCharacter);
                if (StorageManager.SaveJsonData(currentCharacter.SaveName, json, true))
                {
                    Notify.Success("캐릭터가 성공적으로 저장되었습니다.");
                    return true;
                }
                else
                {
                    Notify.Error("캐릭터를 저장할 수 없습니다. 원인을 알 수 없습니다. :(");
                    return false;
                }
            }
            else
            {
                var name = await GetUserInput(windowTitle: "저장 이름을 입력하세요.", maxInputLength: 30);
                if (string.IsNullOrEmpty(name))
                {
                    Notify.Error(CommonErrors.InvalidInput);
                    return false;
                }
                else
                {
                    currentCharacter.SaveName = "mp_ped_" + name;
                    var json = JsonConvert.SerializeObject(currentCharacter);

                    if (StorageManager.SaveJsonData("mp_ped_" + name, json, false))
                    {
                        Notify.Success($"캐릭터 (~g~<C>{name}</C>~s~)가 저장되었습니다.");
                        Log($"캐릭터 {name} 저장됨. 데이터: {json}");
                        return true;
                    }
                    else
                    {
                        Notify.Error($"저장에 실패했습니다. 아마 이 이름(~y~<C>{name}</C>~s~)이 이미 사용 중이기 때문입니다.");
                        return false;
                    }
                }
            }

        }

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            for (int i = 0; i < 46; i++)
            {
                parents.Add($"#{i}");
            }

            for (int i = 0; i < GetNumHairColors(); i++)
            {
                overlayColorsList.Add($"색상 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(0); i++)
            {
                blemishesStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(1); i++)
            {
                beardStylesList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(2); i++)
            {
                eyebrowsStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(3); i++)
            {
                ageingStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(4); i++)
            {
                makeupStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(5); i++)
            {
                blushStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(6); i++)
            {
                complexionStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(7); i++)
            {
                sunDamageStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(8); i++)
            {
                lipstickStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(9); i++)
            {
                molesFrecklesStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(10); i++)
            {
                chestHairStyleList.Add($"스타일 #{i + 1}");
            }

            for (int i = 0; i < GetNumHeadOverlayValues(11); i++)
            {
                bodyBlemishesList.Add($"스타일 #{i + 1}");
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "MP 캐릭터 꾸미기");

            var savedCharacters = new MenuItem("저장된 캐릭터", "기존에 저장한 멀티플레이어 캐릭터를 소환, 수정 또는 삭제합니다.")
            {
                Label = "→→→"
            };

            MenuController.AddMenu(createCharacterMenu);
            MenuController.AddMenu(savedCharactersMenu);
            MenuController.AddMenu(savedCharactersCategoryMenu);
            MenuController.AddMenu(inheritanceMenu);
            MenuController.AddMenu(appearanceMenu);
            MenuController.AddMenu(faceShapeMenu);
            MenuController.AddMenu(tattoosMenu);
            MenuController.AddMenu(clothesMenu);
            MenuController.AddMenu(propsMenu);

            CreateSavedPedsMenu();

            menu.AddMenuItem(createMaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createMaleBtn);
            menu.AddMenuItem(createFemaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createFemaleBtn);
            menu.AddMenuItem(savedCharacters);
            MenuController.BindMenuItem(menu, savedCharactersMenu, savedCharacters);

            menu.RefreshIndex();

            createCharacterMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            inheritanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            appearanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            faceShapeMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            tattoosMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            clothesMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");
            propsMenu.InstructionalButtons.Add(Control.MoveLeftRight, "머리 돌리기");

            createCharacterMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            inheritanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            appearanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            faceShapeMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            tattoosMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            clothesMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");
            propsMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "캐릭터 회전");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "카메라 오른쪽 회전");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "카메라 왼쪽 회전");


            var randomizeButton = new MenuItem("캐릭터 랜덤 생성", "캐릭터 외형을 무작위로 설정합니다.");
            var inheritanceButton = new MenuItem("캐릭터 유전", "캐릭터 유전 옵션입니다.");
            var appearanceButton = new MenuItem("캐릭터 외형", "캐릭터 외형 옵션입니다.");
            var faceButton = new MenuItem("캐릭터 얼굴형 옵션", "캐릭터 얼굴형 옵션입니다.");
            var tattoosButton = new MenuItem("캐릭터 문신 옵션", "캐릭터 문신 옵션입니다.");
            var clothesButton = new MenuItem("캐릭터 의상", "캐릭터 의상입니다.");
            var propsButton = new MenuItem("캐릭터 소품", "캐릭터 소품입니다.");
            var saveButton = new MenuItem("캐릭터 저장", "캐릭터를 저장합니다.");
            var exitNoSave = new MenuItem("저장하지 않고 종료", "정말 종료하시겠습니까? 저장하지 않은 작업은 모두 사라집니다.");
            var faceExpressionList = new MenuListItem("표정", new List<string> { "기본", "행복", "화남", "조준", "부상", "스트레스", "능글", "시무룩" }, 0, "캐릭터가 가만히 있을 때 사용할 표정을 설정합니다.");

            inheritanceButton.Label = "→→→";
            appearanceButton.Label = "→→→";
            faceButton.Label = "→→→";
            tattoosButton.Label = "→→→";
            clothesButton.Label = "→→→";
            propsButton.Label = "→→→";

            createCharacterMenu.AddMenuItem(randomizeButton);
            createCharacterMenu.AddMenuItem(inheritanceButton);
            createCharacterMenu.AddMenuItem(appearanceButton);
            createCharacterMenu.AddMenuItem(faceButton);
            createCharacterMenu.AddMenuItem(tattoosButton);
            createCharacterMenu.AddMenuItem(clothesButton);
            createCharacterMenu.AddMenuItem(propsButton);
            createCharacterMenu.AddMenuItem(faceExpressionList);
            createCharacterMenu.AddMenuItem(categoryBtn);
            createCharacterMenu.AddMenuItem(saveButton);
            createCharacterMenu.AddMenuItem(exitNoSave);

            MenuController.BindMenuItem(createCharacterMenu, inheritanceMenu, inheritanceButton);
            MenuController.BindMenuItem(createCharacterMenu, appearanceMenu, appearanceButton);
            MenuController.BindMenuItem(createCharacterMenu, faceShapeMenu, faceButton);
            MenuController.BindMenuItem(createCharacterMenu, tattoosMenu, tattoosButton);
            MenuController.BindMenuItem(createCharacterMenu, clothesMenu, clothesButton);
            MenuController.BindMenuItem(createCharacterMenu, propsMenu, propsButton);

            #region inheritance
            var dads = new Dictionary<string, int>();
            var moms = new Dictionary<string, int>();

            void AddInheritance(Dictionary<string, int> dict, int listId, string textPrefix)
            {
                var baseIdx = dict.Count;
                var basePed = GetPedHeadBlendFirstIndex(listId);

                // list 0/2 are male, list 1/3 are female
                var suffix = $" ({(listId % 2 == 0 ? "Male" : "Female")})";

                for (var i = 0; i < GetNumParentPedsOfType(listId); i++)
                {
                    // get the actual parent name, or the index if none
                    var label = GetLabelText($"{textPrefix}{i}");
                    if (string.IsNullOrWhiteSpace(label) || label == "NULL")
                    {
                        label = $"{baseIdx + i}";
                    }

                    // append the gender of the list
                    label += suffix;
                    dict[label] = basePed + i;
                }
            }

            int GetInheritance(Dictionary<string, int> list, MenuListItem listItem)
            {
                if (listItem.ListIndex < listItem.ListItems.Count)
                {
                    if (list.TryGetValue(listItem.ListItems[listItem.ListIndex], out var idx))
                    {
                        return idx;
                    }
                }

                return 0;
            }

            var listIdx = 0;
            foreach (var list in new[] { dads, moms })
            {
                void AddDads()
                {
                    AddInheritance(list, 0, "Male_");
                    AddInheritance(list, 2, "Special_Male_");
                }

                void AddMoms()
                {
                    AddInheritance(list, 1, "Female_");
                    AddInheritance(list, 3, "Special_Female_");
                }

                if (listIdx == 0)
                {
                    AddDads();
                    AddMoms();
                }
                else
                {
                    AddMoms();
                    AddDads();
                }

                listIdx++;
            }

            var inheritanceDads = new MenuListItem("아버지", dads.Keys.ToList(), 0, "아버지를 선택하세요.");
            var inheritanceMoms = new MenuListItem("어머니", moms.Keys.ToList(), 0, "어머니를 선택하세요.");
            var inheritanceShapeMix = new MenuSliderItem("얼굴형 혼합", "얼굴형이 아버지와 어머니 중 누구를 얼마나 닮을지 설정합니다. 왼쪽 끝은 아버지, 오른쪽 끝은 어머니입니다.", 0, 10, 5, true) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "shape_mix" };
            var inheritanceSkinMix = new MenuSliderItem("피부톤 혼합", "피부톤이 아버지와 어머니 중 누구를 얼마나 닮을지 설정합니다. 왼쪽 끝은 아버지, 오른쪽 끝은 어머니입니다.", 0, 10, 5, true) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "skin_mix" };

            inheritanceMenu.AddMenuItem(inheritanceDads);
            inheritanceMenu.AddMenuItem(inheritanceMoms);
            inheritanceMenu.AddMenuItem(inheritanceShapeMix);
            inheritanceMenu.AddMenuItem(inheritanceSkinMix);

            // formula from maintransition.#sc
            float GetMinimum()
            {
                return currentCharacter.IsMale ? 0.05f : 0.3f;
            }

            float GetMaximum()
            {
                return currentCharacter.IsMale ? 0.7f : 0.95f;
            }

            float ClampMix(int value)
            {
                var sliderFraction = mixValues[value];
                var min = GetMinimum();
                var max = GetMaximum();

                return min + (sliderFraction * (max - min));
            }

            int UnclampMix(float value)
            {
                var min = GetMinimum();
                var max = GetMaximum();

                var origFraction = (value - min) / (max - min);
                return Math.Max(Math.Min((int)(origFraction * 10), 10), 0);
            }

            inheritanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                _dadSelection = inheritanceDads.ListIndex;
                _mumSelection = inheritanceMoms.ListIndex;

                SetHeadBlend();
            };

            inheritanceMenu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, itemIndex) =>
            {
                // Chris: We can't call `.Position` on the slider items here because it returns the value *prior* to the change
                switch (item.ItemData)
                {
                    case "shape_mix":
                        _shapeMixValue = newPosition / 10f;
                        break;

                    case "skin_mix":
                        _skinMixValue = newPosition / 10f;
                        break;

                    default:
                        break;
                }

                SetHeadBlend();
            };
            #endregion

            #region appearance
            // manage the list changes for appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex == 0) // hair style
                {
                    ChangePlayerHair(newSelectionIndex);
                }
                else if (itemIndex is 1 or 2) // hair colors
                {
                    var tmp = (MenuListItem)_menu.GetMenuItems()[1];
                    var hairColor = tmp.ListIndex;
                    tmp = (MenuListItem)_menu.GetMenuItems()[2];
                    var hairHighlightColor = tmp.ListIndex;

                    ChangePlayerHairColor(hairColor, hairHighlightColor);

                    currentCharacter.PedAppearance.hairColor = hairColor;
                    currentCharacter.PedAppearance.hairHighlightColor = hairHighlightColor;
                }
                else if (itemIndex == 33) // eye color
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    ChangePlayerEyeColor(selection);
                    currentCharacter.PedAppearance.eyeColor = selection;
                }
                else
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

                    switch (itemIndex)
                    {
                        case 3: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 5: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 7: // beards color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, selection, selection);
                            currentCharacter.PedAppearance.beardColor = selection;
                            break;
                        case 8: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 10: // eyebrows color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, selection, selection);
                            currentCharacter.PedAppearance.eyebrowsColor = selection;
                            break;
                        case 11: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 13: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 15: // makeup color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, selection, selection);
                            currentCharacter.PedAppearance.makeupColor = selection;
                            break;
                        case 16: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 18: // blush color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, selection, selection);
                            currentCharacter.PedAppearance.blushColor = selection;
                            break;
                        case 19: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 21: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 23: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 25: // lipstick color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, selection, selection);
                            currentCharacter.PedAppearance.lipstickColor = selection;
                            break;
                        case 26: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 28: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                        case 30: // chest hair color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, selection, selection);
                            currentCharacter.PedAppearance.chestHairColor = selection;
                            break;
                        case 31: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };

            // manage the slider changes for opacity on the appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex is > 2 and < 33)
                {

                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex - 1]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

                    switch (itemIndex)
                    {
                        case 4: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 6: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 9: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 12: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 14: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 17: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 20: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 22: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 24: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 27: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 29: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                        case 32: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };
            #endregion

            #region clothes
            clothesMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) =>
            {
                var componentIndex = realIndex + 1;
                if (realIndex > 0)
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = 0;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, newSelectionIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, newSelectionIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(newSelectionIndex, newTextureIndex);
                listItem.Description = $"방향키로 드로어블을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{newTextureIndex + 1} / 총 {maxTextures}개.";
            };

            clothesMenu.OnListItemSelect += (sender, listItem, listIndex, realIndex) =>
            {
                var componentIndex = realIndex + 1; // skip face options as that fucks up with inheritance faces
                if (realIndex > 0) // skip hair features as that is done in the appeareance menu
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                listItem.Description = $"방향키로 드로어블을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{newTextureIndex + 1} / 총 {maxTextures}개.";
            };
            #endregion

            #region props
            propsMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) =>
            {
                var propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = 0;
                if (newSelectionIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, newSelectionIndex, textureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(newSelectionIndex, textureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다.";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, newSelectionIndex);
                        listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{textureIndex + 1} / 총 {maxPropTextures}개.";
                    }
                }
            };

            propsMenu.OnListItemSelect += (sender, listItem, listIndex, realIndex) =>
            {
                var propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = GetPedPropTextureIndex(Game.PlayerPed.Handle, propIndex);
                var newTextureIndex = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                if (textureIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다.";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, listIndex, newTextureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다.";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex);
                        listItem.Description = $"방향키로 소품을 선택하고 ~o~Enter~s~를 눌러 사용 가능한 모든 텍스처를 순환합니다. 현재 선택된 텍스처: #{newTextureIndex + 1} / 총 {maxPropTextures}개.";
                    }
                }
                //propsMenu.UpdateScaleform();
            };
            #endregion

            #region face shape data
            /*
            Nose_Width  
            Nose_Peak_Hight  
            Nose_Peak_Lenght  
            Nose_Bone_High  
            Nose_Peak_Lowering  
            Nose_Bone_Twist  
            EyeBrown_High  
            EyeBrown_Forward  
            Cheeks_Bone_High  
            Cheeks_Bone_Width  
            Cheeks_Width  
            Eyes_Openning  
            Lips_Thickness  
            Jaw_Bone_Width 'Bone size to sides  
            Jaw_Bone_Back_Lenght 'Bone size to back  
            Chimp_Bone_Lowering 'Go Down  
            Chimp_Bone_Lenght 'Go forward  
            Chimp_Bone_Width  
            Chimp_Hole  
            Neck_Thikness  
            */

            var faceFeaturesNamesList = new string[20]
            {
                "코 너비",               // 0
                "코끝 높이",         // 1
                "코끝 길이",         // 2
                "콧대 높이",         // 3
                "코끝 내려감",       // 4
                "콧대 비틀림",          // 5
                "눈썹 높이",          // 6
                "눈썹 깊이",           // 7
                "광대 높이",        // 8
                "광대 너비",         // 9
                "볼 너비",             // 10
                "눈 뜬 정도",             // 11
                "입술 두께",           // 12
                "턱뼈 너비",           // 13
                "턱뼈 깊이/길이",    // 14
                "턱 높이",              // 15
                "턱 깊이/길이",        // 16
                "턱 너비",               // 17
                "턱 홈 크기",           // 18
                "목 두께"            // 19
            };

            for (var i = 0; i < 20; i++)
            {
                var faceFeature = new MenuSliderItem(faceFeaturesNamesList[i], $"{faceFeaturesNamesList[i]} 얼굴 특징 값을 설정합니다.", 0, 20, 10, true);
                faceShapeMenu.AddMenuItem(faceFeature);
            }

            faceShapeMenu.OnSliderPositionChange += (sender, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                currentCharacter.FaceShapeFeatures.features ??= new Dictionary<int, float>();
                var value = faceFeaturesValuesList[newPosition];
                currentCharacter.FaceShapeFeatures.features[itemIndex] = value;
                SetPedFaceFeature(Game.PlayerPed.Handle, itemIndex, value);
            };

            #endregion

            #region tattoos
            void CreateListsIfNull()
            {
                currentCharacter.PedTatttoos.HeadTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.TorsoTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.BadgeTattoos ??= new List<KeyValuePair<string, string>>();
            }

            void ApplySavedTattoos()
            {
                // remove all decorations, and then manually re-add them all. what a retarded way of doing this R*....
                ClearPedDecorations(Game.PlayerPed.Handle);

                foreach (var tattoo in currentCharacter.PedTatttoos.HeadTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.TorsoTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.BadgeTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }

                if (!string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Key) && !string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Value))
                {
                    // reset hair value
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(currentCharacter.PedAppearance.HairOverlay.Key), (uint)GetHashKey(currentCharacter.PedAppearance.HairOverlay.Value));
                }
            }

            tattoosMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
            };

            #region tattoos menu list select events
            tattoosMenu.OnListIndexChange += (sender, item, oldIndex, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
                if (menuIndex == 0) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 1) // torso
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 2) // left arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 3) // right arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 4) // left leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 5) // right leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 6) // badges
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
            };

            tattoosMenu.OnListItemSelect += (sender, item, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();

                if (menuIndex == 0) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.HeadTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.HeadTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 1) // torso
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.TorsoTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.TorsoTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 2) // left arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 3) // right arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.RightArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.RightArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 4) // left leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 5) // right leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.RightLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"문신 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.RightLegTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 6) // badges
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"배지 #{tattooIndex + 1}이(가) ~r~제거~s~되었습니다.");
                        currentCharacter.PedTatttoos.BadgeTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"배지 #{tattooIndex + 1}이(가) ~g~추가~s~되었습니다.");
                        currentCharacter.PedTatttoos.BadgeTattoos.Add(tat);
                    }
                }

                ApplySavedTattoos();

            };

            // eventhandler for when a tattoo is selected.
            tattoosMenu.OnItemSelect += (sender, item, index) =>
            {
                Notify.Success("모든 문신이 제거되었습니다.");
                currentCharacter.PedTatttoos.HeadTattoos.Clear();
                currentCharacter.PedTatttoos.TorsoTattoos.Clear();
                currentCharacter.PedTatttoos.LeftArmTattoos.Clear();
                currentCharacter.PedTatttoos.RightArmTattoos.Clear();
                currentCharacter.PedTatttoos.LeftLegTattoos.Clear();
                currentCharacter.PedTatttoos.RightLegTattoos.Clear();
                currentCharacter.PedTatttoos.BadgeTattoos.Clear();
                ClearPedDecorations(Game.PlayerPed.Handle);
            };

            #endregion
            #endregion


            // handle list changes in the character creator menu.
            createCharacterMenu.OnListIndexChange += (sender, item, oldListIndex, newListIndex, itemIndex) =>
            {
                if (item == faceExpressionList)
                {
                    currentCharacter.FacialExpression = facial_expressions[newListIndex];
                    SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
                }
                else if (item == categoryBtn)
                {
                    List<string> categoryNames = categoryBtn.ItemData.Item1;
                    List<MenuItem.Icon> categoryIcons = categoryBtn.ItemData.Item2;
                    currentCharacter.Category = categoryNames[newListIndex];
                    categoryBtn.RightIcon = categoryIcons[newListIndex];
                }
            };

            // handle button presses for the createCharacter menu.
            createCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == randomizeButton)
                {
                    _dadSelection = _random.Next(parents.Count);
                    _mumSelection = _random.Next(parents.Count);
                    _skinMixValue = (float)_random.NextDouble();
                    _shapeMixValue = (float)_random.NextDouble();

                    SetHeadBlend();

                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = [];
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        shapeFaceValues[i] = _random.Next(5, 15);
                        SetPedFaceFeature(Game.PlayerPed.Handle, i, faceFeaturesValuesList[shapeFaceValues[i]]);
                        currentCharacter.FaceShapeFeatures.features[i] = faceFeaturesValuesList[shapeFaceValues[i]];
                    }

                    int bodyHair = _random.Next(31);

                    ChangePlayerHair(_random.Next(0, GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2)));
                    ChangePlayerHairColor(bodyHair, _random.Next(31));
                    ChangePlayerEyeColor(_random.Next(0, 9));

                    for (int i = 0; i < 12; i++)
                    {
                        int value;
                        int colorIndex = 0;
                        bool colorRequired = false;

                        int color = i == 1 || i == 2 || i == 10 ? bodyHair : _random.Next(17);
                        float opacity = (float)_random.NextDouble();

                        switch (i)
                        {
                            case 0:
                                value = _random.Next(blemishesStyleList.Count);

                                currentCharacter.PedAppearance.blemishesStyle = value;
                                currentCharacter.PedAppearance.blemishesOpacity = opacity;
                                break;

                            case 1:
                                if (!currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(beardStylesList.Count);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.beardStyle = value;
                                currentCharacter.PedAppearance.beardColor = color;
                                currentCharacter.PedAppearance.beardOpacity = opacity;
                                break;

                            case 2:
                                value = _random.Next(eyebrowsStyleList.Count);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.eyebrowsColor = value;
                                currentCharacter.PedAppearance.eyebrowsStyle = color;
                                currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                                break;

                            case 3:
                                value = _random.Next(ageingStyleList.Count);

                                currentCharacter.PedAppearance.ageingStyle = value;
                                currentCharacter.PedAppearance.ageingOpacity = opacity;
                                break;

                            case 8:
                                if (currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(6);
                                colorRequired = true;
                                colorIndex = 2;

                                currentCharacter.PedAppearance.lipstickStyle = value;
                                currentCharacter.PedAppearance.lipstickColor = color;
                                currentCharacter.PedAppearance.lipstickOpacity = opacity;
                                break;

                            case 9:
                                value = _random.Next(molesFrecklesStyleList.Count);

                                currentCharacter.PedAppearance.molesFrecklesStyle = value;
                                currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                                break;

                            case 10:
                                if (!currentCharacter.IsMale)
                                {
                                    appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0f);
                                    continue;
                                }

                                value = _random.Next(8);
                                colorRequired = true;
                                colorIndex = 1;

                                currentCharacter.PedAppearance.chestHairStyle = value;
                                currentCharacter.PedAppearance.chestHairColor = color;
                                currentCharacter.PedAppearance.chestHairOpacity = opacity;
                                break;

                            case 11:
                                value = _random.Next(bodyBlemishesList.Count);

                                currentCharacter.PedAppearance.bodyBlemishesStyle = value;
                                currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                                break;

                            default:
                                appearanceValues[i] = new Tuple<int, int, float>(0, 0, 0);
                                continue;
                        }

                        appearanceValues[i] = new Tuple<int, int, float>(value, color, opacity);
                        SetPedHeadOverlay(Game.PlayerPed.Handle, i, appearanceValues[i].Item1, appearanceValues[i].Item3);

                        if (colorRequired)
                        {
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, i, colorIndex, appearanceValues[i].Item2, appearanceValues[i].Item2);
                        }
                    }

                    _facialExpressionSelection = _random.Next(facial_expressions.Count);

                    SetFacialIdleAnimOverride(Game.PlayerPed.Handle, facial_expressions[_facialExpressionSelection], null);

                    currentCharacter.FacialExpression = facial_expressions[_facialExpressionSelection];

                    ((MenuListItem)createCharacterMenu.GetMenuItems()[7]).ListIndex = _facialExpressionSelection;

                    SetPlayerClothing();
                }
                else if (item == saveButton) // save ped
                {
                    if (await SavePed())
                    {
                        while (!MenuController.IsAnyMenuOpen())
                        {
                            await BaseScript.Delay(0);
                        }

                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);

                        createCharacterMenu.GoBack();
                    }
                }
                else if (item == exitNoSave) // exit without saving
                {
                    var confirm = false;
                    AddTextEntry("vmenu_warning_message_first_line", "정말 캐릭터 생성기를 종료하시겠습니까?");
                    AddTextEntry("vmenu_warning_message_second_line", "저장하지 않은 커스터마이징 내용이 모두 사라집니다!");
                    createCharacterMenu.CloseMenu();

                    // wait for confirmation or cancel input.
                    while (true)
                    {
                        await BaseScript.Delay(0);
                        var unk = 1;
                        var unk2 = 1;
                        SetWarningMessage("vmenu_warning_message_first_line", 20, "vmenu_warning_message_second_line", true, 0, ref unk, ref unk2, true, 0);
                        if (IsControlJustPressed(2, 201) || IsControlJustPressed(2, 217)) // continue/accept
                        {
                            confirm = true;
                            break;
                        }
                        else if (IsControlJustPressed(2, 202)) // cancel
                        {
                            break;
                        }
                    }

                    // if confirmed to discard changes quit the editor.
                    if (confirm)
                    {
                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);
                        menu.OpenMenu();
                    }
                    else // otherwise cancel and go back to the editor.
                    {
                        createCharacterMenu.OpenMenu();
                    }
                }
                else if (item == inheritanceButton) // update the inheritance menu anytime it's opened to prevent some weird glitch where old data is used.
                {
                    inheritanceDads.ListIndex = _dadSelection;
                    inheritanceMoms.ListIndex = _mumSelection;
                    inheritanceShapeMix.Position = (int)(_shapeMixValue * 10f);
                    inheritanceSkinMix.Position = (int)(_skinMixValue * 10f);
                    inheritanceMenu.RefreshIndex();
                }
                else if (item == faceButton)
                {
                    List<MenuItem> items = faceShapeMenu.GetMenuItems();

                    for (int i = 0; i < 20; i++)
                    {
                        if (items[i] is MenuSliderItem sliderItem)
                        {
                            sliderItem.Position = shapeFaceValues[i];
                        }
                    }

                    faceShapeMenu.RefreshIndex();
                }
                else if (item == appearanceButton)
                {
                    List<MenuListItem> menuListItems = [.. appearanceMenu.GetMenuItems().OfType<MenuListItem>()];

                    menuListItems.First(i => i.Text == "헤어 스타일").ListIndex = _hairSelection;
                    menuListItems.First(i => i.Text == "머리 색상").ListIndex = _hairColorSelection;
                    menuListItems.First(i => i.Text == "머리 하이라이트 색상").ListIndex = _hairHighlightColorSelection;

                    menuListItems.First(i => i.Text == "잡티 스타일").ListIndex = appearanceValues[0].Item1;
                    menuListItems.First(i => i.Text == "잡티 투명도").ListIndex = (int)(appearanceValues[0].Item3 * 10);

                    menuListItems.First(i => i.Text == "수염 스타일").ListIndex = appearanceValues[1].Item1;
                    menuListItems.First(i => i.Text == "수염 투명도").ListIndex = (int)(appearanceValues[1].Item3 * 10);
                    menuListItems.First(i => i.Text == "수염 색상").ListIndex = appearanceValues[1].Item2;

                    menuListItems.First(i => i.Text == "눈썹 스타일").ListIndex = appearanceValues[2].Item1;
                    menuListItems.First(i => i.Text == "눈썹 투명도").ListIndex = (int)(appearanceValues[2].Item3 * 10);
                    menuListItems.First(i => i.Text == "눈썹 색상").ListIndex = appearanceValues[2].Item2;

                    menuListItems.First(i => i.Text == "노화 스타일").ListIndex = appearanceValues[3].Item1;
                    menuListItems.First(i => i.Text == "노화 투명도").ListIndex = (int)(appearanceValues[3].Item3 * 10);

                    menuListItems.First(i => i.Text == "메이크업 스타일").ListIndex = appearanceValues[4].Item1;
                    menuListItems.First(i => i.Text == "메이크업 투명도").ListIndex = (int)(appearanceValues[4].Item3 * 10);
                    menuListItems.First(i => i.Text == "메이크업 색상").ListIndex = appearanceValues[4].Item2;

                    menuListItems.First(i => i.Text == "블러셔 스타일").ListIndex = appearanceValues[5].Item1;
                    menuListItems.First(i => i.Text == "블러셔 투명도").ListIndex = (int)(appearanceValues[5].Item3 * 10);
                    menuListItems.First(i => i.Text == "블러셔 색상").ListIndex = appearanceValues[5].Item2;

                    menuListItems.First(i => i.Text == "피부결 스타일").ListIndex = appearanceValues[6].Item1;
                    menuListItems.First(i => i.Text == "피부결 투명도").ListIndex = (int)(appearanceValues[6].Item3 * 10);

                    menuListItems.First(i => i.Text == "햇빛 손상 스타일").ListIndex = appearanceValues[7].Item1;
                    menuListItems.First(i => i.Text == "햇빛 손상 투명도").ListIndex = (int)(appearanceValues[7].Item3 * 10);

                    menuListItems.First(i => i.Text == "립스틱 스타일").ListIndex = appearanceValues[8].Item1;
                    menuListItems.First(i => i.Text == "립스틱 투명도").ListIndex = (int)(appearanceValues[8].Item3 * 10);
                    menuListItems.First(i => i.Text == "립스틱 색상").ListIndex = appearanceValues[8].Item2;

                    menuListItems.First(i => i.Text == "점/주근깨 스타일").ListIndex = appearanceValues[9].Item1;
                    menuListItems.First(i => i.Text == "점/주근깨 투명도").ListIndex = (int)(appearanceValues[9].Item3 * 10);

                    menuListItems.First(i => i.Text == "가슴 털 스타일").ListIndex = appearanceValues[10].Item1;
                    menuListItems.First(i => i.Text == "가슴 털 투명도").ListIndex = (int)(appearanceValues[10].Item3 * 10);
                    menuListItems.First(i => i.Text == "가슴 털 색상").ListIndex = appearanceValues[10].Item2;

                    menuListItems.First(i => i.Text == "몸 잡티 스타일").ListIndex = appearanceValues[11].Item1;
                    menuListItems.First(i => i.Text == "몸 잡티 투명도").ListIndex = (int)(appearanceValues[11].Item3 * 10);

                    menuListItems.First(i => i.Text == "눈 색상").ListIndex = _eyeColorSelection;

                    appearanceMenu.RefreshIndex();

                    SetHeadBlend();
                }
            };

            // eventhandler for whenever a menu item is selected in the main mp characters menu.
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == createMaleBtn)
                {
                    var model = (uint)GetHashKey("mp_m_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    ClearAllPedProps(Game.PlayerPed.Handle);
                    DefaultPlayerColors();

                    MakeCreateCharacterMenu(male: true);
                }
                else if (item == createFemaleBtn)
                {
                    var model = (uint)GetHashKey("mp_f_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    ClearAllPedProps(Game.PlayerPed.Handle);
                    DefaultPlayerColors();

                    MakeCreateCharacterMenu(male: false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
            };
        }

        /// <summary>
        /// Spawns this saved ped.
        /// </summary>
        /// <param name="name"></param>
        internal async Task SpawnThisCharacter(string name, bool restoreWeapons)
        {
            currentCharacter = StorageManager.GetSavedMpCharacterData(name);
            await SpawnSavedPed(restoreWeapons);
        }

        /// <summary>
        /// Spawns the ped from the data inside <see cref="currentCharacter"/>.
        /// Character data MUST be set BEFORE calling this function.
        /// </summary>
        /// <returns></returns>
        private async Task SpawnSavedPed(bool restoreWeapons)
        {
            if (currentCharacter.Version < 1)
            {
                return;
            }
            if (IsModelInCdimage(currentCharacter.ModelHash))
            {
                if (!HasModelLoaded(currentCharacter.ModelHash))
                {
                    RequestModel(currentCharacter.ModelHash);
                    while (!HasModelLoaded(currentCharacter.ModelHash))
                    {
                        await BaseScript.Delay(0);
                    }
                }
                var maxHealth = Game.PlayerPed.MaxHealth;
                var maxArmour = Game.Player.MaxArmor;
                var health = Game.PlayerPed.Health;
                var armour = Game.PlayerPed.Armor;

                SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                SetPlayerModel(Game.Player.Handle, currentCharacter.ModelHash);
                await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                Game.Player.MaxArmor = maxArmour;
                Game.PlayerPed.MaxHealth = maxHealth;
                Game.PlayerPed.Health = health;
                Game.PlayerPed.Armor = armour;

                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                SetPedEyeColor(Game.PlayerPed.Handle, 0);
                ClearAllPedProps(Game.PlayerPed.Handle);

                await AppySavedDataToPed(currentCharacter, Game.PlayerPed.Handle);
            }

            // Set the facial expression, or set it to 'normal' if it wasn't saved/set before.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
        }

        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {
            UpdateSavedPedsMenu();

            MenuController.AddMenu(manageSavedCharacterMenu);

            var spawnPed = new MenuItem("저장된 캐릭터 소환", "선택한 저장 캐릭터를 소환합니다.");
            editPedBtn = new MenuItem("저장된 캐릭터 수정", "저장된 캐릭터의 모든 항목을 수정할 수 있습니다. 저장 버튼을 누르면 변경 사항이 이 캐릭터의 저장 파일에 반영됩니다.");
            var clonePed = new MenuItem("저장된 캐릭터 복제", "저장된 캐릭터를 복제합니다. 새 캐릭터 이름을 입력해야 하며, 이미 사용 중인 이름이면 작업이 취소됩니다.");
            var setAsDefaultPed = new MenuItem("기본 캐릭터로 설정", "이 캐릭터를 기본 캐릭터로 설정하고 기타 설정 메뉴에서 '기본 MP 캐릭터로 다시 스폰' 옵션을 켜면, (재)스폰할 때마다 이 캐릭터로 적용됩니다.");
            var renameCharacter = new MenuItem("저장된 캐릭터 이름 변경", "이 저장된 캐릭터의 이름을 변경할 수 있습니다. 이미 사용 중인 이름이면 작업이 취소됩니다.");
            var saveCurrentPedAsCharacter = new MenuItem("캐릭터 의상 업데이트", "현재 의상을 이 저장된 캐릭터에 적용합니다. ~r~기존 저장 의상이 덮어써집니다.~w~ 의상만 업데이트되며 다른 외형 요소는 변경되지 않습니다.")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };
            var delPed = new MenuItem("저장된 캐릭터 삭제", "선택한 저장 캐릭터를 삭제합니다. 이 작업은 되돌릴 수 없습니다!")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };
            manageSavedCharacterMenu.AddMenuItem(spawnPed);
            manageSavedCharacterMenu.AddMenuItem(editPedBtn);
            manageSavedCharacterMenu.AddMenuItem(clonePed);
            manageSavedCharacterMenu.AddMenuItem(setCategoryBtn);
            manageSavedCharacterMenu.AddMenuItem(setAsDefaultPed);
            manageSavedCharacterMenu.AddMenuItem(renameCharacter);
            manageSavedCharacterMenu.AddMenuItem(saveCurrentPedAsCharacter);
            manageSavedCharacterMenu.AddMenuItem(delPed);

            MenuController.BindMenuItem(manageSavedCharacterMenu, createCharacterMenu, editPedBtn);

            manageSavedCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == editPedBtn)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);

                    MakeCreateCharacterMenu(male: currentCharacter.IsMale, editPed: true);
                }
                else if (item == spawnPed)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);
                }
                else if (item == clonePed)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "복제할 캐릭터 이름을 입력하세요", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("mp_ped_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            tmpCharacter.SaveName = "mp_ped_" + name;
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                Notify.Success($"캐릭터가 복제되었습니다. 복제된 캐릭터 이름: ~g~<C>{name}</C>~s~.");
                                MenuController.CloseAllMenus();
                                UpdateSavedPedsMenu();
                                savedCharactersMenu.OpenMenu();
                            }
                            else
                            {
                                Notify.Error("복제를 생성할 수 없습니다. 원인을 알 수 없습니다. 같은 이름의 캐릭터가 이미 있나요? :(");
                            }
                        }
                    }
                }
                else if (item == renameCharacter)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "새 캐릭터 이름을 입력하세요", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("mp_ped_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            tmpCharacter.SaveName = "mp_ped_" + name;
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                StorageManager.DeleteSavedStorageItem("mp_ped_" + selectedSavedCharacterManageName);
                                Notify.Success($"캐릭터 이름이 ~g~<C>{name}</C>~s~(으)로 변경되었습니다.");
                                UpdateSavedPedsMenu();
                                while (!MenuController.IsAnyMenuOpen())
                                {
                                    await BaseScript.Delay(0);
                                }
                                manageSavedCharacterMenu.GoBack();
                            }
                            else
                            {
                                Notify.Error("캐릭터 이름 변경 중 문제가 발생했습니다. 기존 캐릭터는 삭제되지 않습니다.");
                            }
                        }
                    }
                }
                else if (item == saveCurrentPedAsCharacter)
                {
                    if (saveCurrentPedAsCharacter.Label == "정말 진행하시겠습니까?")
                    {
                        saveCurrentPedAsCharacter.Label = "";
                        var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);

                        tmpCharacter = ReplacePedDataClothing(tmpCharacter);

                        if (StorageManager.SaveJsonData(tmpCharacter.SaveName, JsonConvert.SerializeObject(tmpCharacter), true))
                        {
                            Notify.Success($"이 캐릭터의 의상이 업데이트되었습니다!");
                            UpdateSavedPedsMenu();
                        }
                        else
                        {
                            Notify.Error("이 캐릭터의 의상을 업데이트할 수 없습니다. 원인은 알 수 없습니다.");
                        }
                    }
                    else
                    {
                        saveCurrentPedAsCharacter.Label = "정말 진행하시겠습니까?";
                    }
                }
                else if (item == delPed)
                {
                    if (delPed.Label == "정말 진행하시겠습니까?")
                    {
                        delPed.Label = "";
                        DeleteResourceKvp("mp_ped_" + selectedSavedCharacterManageName);
                        Notify.Success("저장된 캐릭터가 삭제되었습니다.");
                        manageSavedCharacterMenu.GoBack();
                        UpdateSavedPedsMenu();
                        manageSavedCharacterMenu.RefreshIndex();
                    }
                    else
                    {
                        delPed.Label = "정말 진행하시겠습니까?";
                    }
                }
                else if (item == setAsDefaultPed)
                {
                    Notify.Success($"이제 <C>{selectedSavedCharacterManageName}</C> 캐릭터가 (재)스폰 시 기본 캐릭터로 사용됩니다.");
                    SetResourceKvp("vmenu_default_character", "mp_ped_" + selectedSavedCharacterManageName);
                }

                if (item != delPed)
                {
                    if (delPed.Label == "정말 진행하시겠습니까?")
                    {
                        delPed.Label = "";
                    }
                }

                if (item != saveCurrentPedAsCharacter)
                {
                    if (saveCurrentPedAsCharacter.Label == "정말 진행하시겠습니까?")
                    {
                        saveCurrentPedAsCharacter.Label = "";
                    }
                }
            };

            // Update category preview icon
            manageSavedCharacterMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) => listItem.RightIcon = listItem.ItemData[newSelectionIndex];

            // Update character's category
            manageSavedCharacterMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);

                string name = listItem.ListItems[listIndex];

                if (name == "새로 만들기")
                {
                    var newName = await GetUserInput(windowTitle: "카테고리 이름을 입력하세요.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "새로 만들기")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "카테고리 설명을 입력하세요. (선택 사항)", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"카테고리 (~g~<C>{newName}</C>~s~)가 저장되었습니다.");
                            Log($"카테고리 {newName} 저장 완료.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"저장에 실패했습니다. 아마 이 이름(~y~<C>{newName}</C>~s~)이 이미 사용 중이기 때문입니다.");
                            return;
                        }
                    }
                }

                tmpCharacter.Category = name;

                var json = JsonConvert.SerializeObject(tmpCharacter);
                if (StorageManager.SaveJsonData(tmpCharacter.SaveName, json, true))
                {
                    Notify.Success("캐릭터가 성공적으로 저장되었습니다.");
                }
                else
                {
                    Notify.Error("캐릭터를 저장할 수 없습니다. 원인을 알 수 없습니다. :(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedPedsMenu();
                savedCharactersMenu.OpenMenu();
            };

            // reset the "정말 진행하시겠습니까" state.
            manageSavedCharacterMenu.OnMenuClose += (sender) =>
            {
                foreach (MenuItem item in manageSavedCharacterMenu.GetMenuItems())
                {
                    if (item.Label == "정말 진행하시겠습니까?")
                    {
                        item.Label = "";
                    }
                }
            };

            // Load selected category
            savedCharactersMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not MpCharacterCategory)
                {
                    var name = await GetUserInput(windowTitle: "카테고리 이름을 입력하세요.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "새로 만들기")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "카테고리 설명을 입력하세요. (선택 사항)", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"카테고리 (~g~<C>{name}</C>~s~)가 저장되었습니다.");
                            Log($"카테고리 {name} 저장 완료.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"저장에 실패했습니다. 아마 이 이름(~y~<C>{name}</C>~s~)이 이미 사용 중이기 때문입니다.");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = item.ItemData;
                }

                bool isUncategorized = currentCategory.Name == "미분류";

                savedCharactersCategoryMenu.MenuTitle = currentCategory.Name;
                savedCharactersCategoryMenu.MenuSubtitle = $"~s~카테고리: ~y~{currentCategory.Name}";
                savedCharactersCategoryMenu.ClearMenuItems();

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
                var deleteCharsBtn = new MenuCheckboxItem("모든 캐릭터 삭제", "If checked, when \"Delete Category\" is pressed, all the saved characters in this category will be deleted as well. If not checked, saved characters will be moved to \"Uncategorized\".")
                {
                    Enabled = !isUncategorized
                };

                savedCharactersCategoryMenu.AddMenuItem(renameBtn);
                savedCharactersCategoryMenu.AddMenuItem(descriptionBtn);
                savedCharactersCategoryMenu.AddMenuItem(iconBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ 캐릭터 목록 ↓");
                savedCharactersCategoryMenu.AddMenuItem(spacer);

                List<string> names = GetAllMpCharacterNames();

                if (names.Count > 0)
                {
                    var defaultChar = GetResourceKvpString("vmenu_default_character") ?? "";

                    names.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                    foreach (var name in names)
                    {
                        var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + name);

                        if (string.IsNullOrEmpty(tmpData.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (tmpData.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        var btn = new MenuItem(name, "클릭하여 이 저장된 캐릭터를 소환, 수정, 복제, 이름 변경 또는 삭제할 수 있습니다.")
                        {
                            Label = "→→→",
                            LeftIcon = tmpData.IsMale ? MenuItem.Icon.MALE : MenuItem.Icon.FEMALE,
                            ItemData = tmpData.IsMale
                        };
                        if (defaultChar == "mp_ped_" + name)
                        {
                            btn.LeftIcon = MenuItem.Icon.TICK;
                            btn.Description += " ~g~이 캐릭터는 현재 기본 캐릭터로 설정되어 있으며 (재)스폰할 때마다 사용됩니다.";
                        }
                        savedCharactersCategoryMenu.AddMenuItem(btn);
                        MenuController.BindMenuItem(savedCharactersCategoryMenu, manageSavedCharacterMenu, btn);
                    }
                }
            };

            savedCharactersCategoryMenu.OnIndexChange += async (menu, oldItem, newItem, oldIndex, newIndex) =>
            {
                if (!GetSettingsBool(Setting.vmenu_mp_ped_preview) || !MainMenu.MiscSettingsMenu.MPPedPreviews)
                {
                    return;
                }

                if (Entity.Exists(_clone))
                {
                    _clone.Delete();
                }

                // Only show preview for ped items, not menu items
                if (newItem.ItemData == null)
                {
                    return;
                }

                MultiplayerPedData character = StorageManager.GetSavedMpCharacterData(newItem.Text);

                if (!HasModelLoaded(character.ModelHash))
                {
                    RequestModel(character.ModelHash);
                    while (!HasModelLoaded(character.ModelHash))
                    {
                        await Delay(0);
                    }
                }

                ///
                /// Credit to whbl (https://forum.cfx.re/u/whbl) for the inspiration for this feature.
                /// https://forum.cfx.re/t/free-standalone-virtual-ped/5052458
                ///

                Ped playerPed = Game.PlayerPed;
                Vector3 clientPedPosition = playerPed.Position;

                _clone = new Ped(CreatePed(26, character.ModelHash, clientPedPosition.X, clientPedPosition.Y, clientPedPosition.Z - 3f, playerPed.Heading, false, false))
                {
                    IsCollisionEnabled = false,
                    IsInvincible = true,
                    BlockPermanentEvents = true,
                    IsPositionFrozen = true
                };

                int cloneHandle = _clone.Handle;

                await AppySavedDataToPed(character, cloneHandle);

                SetEntityCanBeDamaged(cloneHandle, false);
                SetPedAoBlobRendering(cloneHandle, false);

                while (Entity.Exists(_clone))
                {
                    Vector3 worldCoord = Vector3.Zero;
                    Vector3 normal = Vector3.Zero;

                    GetWorldCoordFromScreenCoord(0.6f, 0.8f, ref worldCoord, ref normal);

                    Vector3 cameraRotation = GameplayCamera.Rotation;

                    _clone.Position = worldCoord + (normal * 3.5f);
                    _clone.Rotation = new Vector3(cameraRotation.X * -1, 0f, cameraRotation.Z + 180);
                    _clone.Heading = cameraRotation.Z + 180;

                    GameplayCamera.ClampPitch(0f, 0f);

                    await Delay(0);
                }
            };


            savedCharactersCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "새 카테고리 이름을 입력하세요", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "새로 만들기")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("mp_character_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("mp_character_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;
                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    tmpData.Category = name;

                                    if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                    {
                                        updatedCount++;
                                        Log($"\"{tmpData.SaveName}\"의 카테고리를 업데이트했습니다.");
                                    }
                                    else
                                    {
                                        Log($"\"{tmpData.SaveName}\"의 카테고리 업데이트 중 문제가 발생했습니다.");
                                    }
                                }
                            }

                            Notify.Success($"카테고리 이름이 ~g~<C>{name}</C>~s~(으)로 변경되었습니다. {updatedCount}/{totalCount}개의 캐릭터가 업데이트되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("카테고리 이름 변경 중 문제가 발생했습니다. 기존 카테고리는 삭제되지 않습니다.");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "새 카테고리 설명을 입력하세요", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success($"카테고리 설명이 변경되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("카테고리 설명 변경 중 문제가 발생했습니다.");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "정말 진행하시겠습니까?")
                        {
                            bool deletePeds = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("mp_character_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deletePeds)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp("mp_ped_" + tmpData.SaveName);
                                    }
                                    else
                                    {
                                        tmpData.Category = "미분류";

                                        if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                        {
                                            updatedCount++;
                                            Log($"\"{tmpData.SaveName}\"의 카테고리를 업데이트했습니다.");
                                        }
                                        else
                                        {
                                            Log($"\"{tmpData.SaveName}\"의 카테고리 업데이트 중 문제가 발생했습니다.");
                                        }
                                    }
                                }
                            }

                            Notify.Success($"저장된 카테고리가 삭제되었습니다. {updatedCount}/{totalCount}개의 캐릭터가 {(deletePeds ? "삭제" : "업데이트")}되었습니다.");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            item.Label = "정말 진행하시겠습니까?";
                        }
                        break;

                    // Load saved character menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];
                        selectedSavedCharacterManageName = item.Text;
                        manageSavedCharacterMenu.MenuSubtitle = item.Text;
                        manageSavedCharacterMenu.CounterPreText = $"{(item.LeftIcon == MenuItem.Icon.MALE ? "(Male)" : "(Female)")} ";
                        manageSavedCharacterMenu.RefreshIndex();
                        break;
                }
            };

            // Change Category Icon
            savedCharactersCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
                int iconIndex = iconNames.IndexOf(currentItem);

                currentCategory.Icon = (MenuItem.Icon)iconIndex;

                if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                {
                    Notify.Success($"카테고리 아이콘이 ~g~<C>{iconNames[iconIndex]}</C>~s~(으)로 변경되었습니다.");
                    UpdateSavedPedsMenu();
                }
                else
                {
                    Notify.Error("카테고리 아이콘 변경 중 문제가 발생했습니다.");
                }
            };

            savedCharactersCategoryMenu.OnMenuClose += (_) =>
            {
                if (Entity.Exists(_clone))
                {
                    _clone.Delete();
                }
            };

        }

        /// <summary>
        /// Updates the saved peds menu.
        /// </summary>
        private void UpdateSavedPedsMenu()
        {
            var categories = GetAllCategoryNames();

            savedCharactersMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("카테고리 생성", "새 캐릭터 카테고리를 생성합니다.")
            {
                Label = "→→→"
            };
            savedCharactersMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ 캐릭터 카테고리 ↓");
            savedCharactersMenu.AddMenuItem(spacer);

            var uncategorized = new MpCharacterCategory
            {
                Name = "미분류",
                Description = "카테고리가 지정되지 않은 모든 저장된 MP 캐릭터입니다."
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            savedCharactersMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, uncategorizedBtn);

            // Remove "새로 만들기" and "미분류"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    MpCharacterCategory category = StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    savedCharactersMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, btn);
                }
            }

            savedCharactersMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("mp_character_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(22));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "새로 만들기");
            categories.Insert(1, "미분류");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + name).Icon);
            }

            return icons;
        }

        private List<string> GetAllMpCharacterNames()
        {
            var names = new List<string>();
            var handle = StartFindKvp("mp_ped_");
            while (true)
            {
                var foundName = FindKvp(handle);
                if (string.IsNullOrEmpty(foundName))
                {
                    break;
                }
                else
                {
                    names.Add(foundName.Substring(7));
                }
            }
            EndFindKvp(handle);

            return names;
        }

        private MultiplayerPedData ReplacePedDataClothing(MultiplayerPedData character)
        {
            int handle = Game.PlayerPed.Handle;

            // Drawables
            for (int i = 0; i < 12; i++)
            {
                int drawable = GetPedDrawableVariation(handle, i);
                int texture = GetPedTextureVariation(handle, i);
                character.DrawableVariations.clothes[i] = new KeyValuePair<int, int>(drawable, texture);
            }

            for (int i = 0; i < 8; i++)
            {
                int prop = GetPedPropIndex(handle, i);
                int texture = GetPedPropTextureIndex(handle, i);
                character.PropVariations.props[i] = new KeyValuePair<int, int>(prop, texture);
            }

            return character;
        }

        internal void SetHeadBlend()
        {
            SetPedHeadBlendData(Game.PlayerPed.Handle, _dadSelection, _mumSelection, 0, _dadSelection, _mumSelection, 0, _shapeMixValue, _skinMixValue, 0f, false);
        }

        internal void ChangePlayerHair(int newHairIndex)
        {
            ClearPedFacialDecorations(Game.PlayerPed.Handle);
            currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>("", "");

            if (newHairIndex >= GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2))
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, 0, 0, 0);
                currentCharacter.PedAppearance.hairStyle = 0;
            }
            else
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, newHairIndex, 0, 0);
                currentCharacter.PedAppearance.hairStyle = newHairIndex;
                if (hairOverlays.ContainsKey(newHairIndex))
                {
                    SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(hairOverlays[newHairIndex].Key), (uint)GetHashKey(hairOverlays[newHairIndex].Value));
                    currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>(hairOverlays[newHairIndex].Key, hairOverlays[newHairIndex].Value);
                }
            }

            _hairSelection = newHairIndex;
        }

        internal void ChangePlayerHairColor(int color, int highlight)
        {
            SetPedHairColor(Game.PlayerPed.Handle, color, highlight);

            currentCharacter.PedAppearance.hairColor = color;
            currentCharacter.PedAppearance.hairHighlightColor = highlight;

            _hairColorSelection = color;
            _hairHighlightColorSelection = highlight;
        }

        internal void ChangePlayerEyeColor(int color)
        {
            SetPedEyeColor(Game.PlayerPed.Handle, color);

            currentCharacter.PedAppearance.eyeColor = color;

            _eyeColorSelection = color;
        }

        internal void SetPlayerClothing()
        {
            SetPedComponentVariation(Game.PlayerPed.Handle, 3, 15, 0, 0);

            currentCharacter.DrawableVariations.clothes[3] = new KeyValuePair<int, int>(15, 0);

            if (currentCharacter.IsMale)
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 15, 0, 0);

                currentCharacter.DrawableVariations.clothes[8] = new KeyValuePair<int, int>(15, 0);

                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 15, 0, 0);

                currentCharacter.DrawableVariations.clothes[11] = new KeyValuePair<int, int>(15, 0);

                int pantsColor = _random.Next(GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 4, 61));

                SetPedComponentVariation(Game.PlayerPed.Handle, 4, 61, pantsColor, 0);

                currentCharacter.DrawableVariations.clothes[4] = new KeyValuePair<int, int>(61, pantsColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 6, 34, 0, 0);

                currentCharacter.DrawableVariations.clothes[6] = new KeyValuePair<int, int>(34, 0);
            }
            else
            {
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 14, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 14, 0, 0);

                currentCharacter.DrawableVariations.clothes[8] = new KeyValuePair<int, int>(14, 0);

                int braColor = _random.Next(GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 4, 17));

                SetPedComponentVariation(Game.PlayerPed.Handle, 4, 17, braColor, 0);

                currentCharacter.DrawableVariations.clothes[4] = new KeyValuePair<int, int>(17, braColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 18, braColor, 0);

                currentCharacter.DrawableVariations.clothes[11] = new KeyValuePair<int, int>(18, braColor);

                SetPedComponentVariation(Game.PlayerPed.Handle, 6, 35, 0, 0);

                currentCharacter.DrawableVariations.clothes[6] = new KeyValuePair<int, int>(35, 0);
            }
        }

        /// <summary>
        /// Sets all the ped's overlay colors to their default (0) entry.
        /// When called, prevents default color being bright green.
        /// </summary>
        internal void DefaultPlayerColors()
        {
            SetHeadBlend();

            for (int i = 0; i < 12; i++)
            {
                int color = 0;
                int colorIndex = 0;

                switch (i)
                {
                    case 1:
                        colorIndex = 1;
                        break;

                    case 2:
                        colorIndex = 1;
                        break;

                    case 8:
                        colorIndex = 2;
                        break;

                    case 10:
                        colorIndex = 1;
                        break;

                    default:
                        continue;
                }

                SetPedHeadOverlay(Game.PlayerPed.Handle, i, 0, 0f);

                if (colorIndex > 0)
                {
                    SetPedHeadOverlayColor(Game.PlayerPed.Handle, i, colorIndex, color, color);
                }
            }
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

        internal async Task AppySavedDataToPed(MultiplayerPedData character, int pedHandle)
        {
            #region headblend
            PedHeadBlendData data = character.PedHeadBlendData;
            SetPedHeadBlendData(pedHandle, data.FirstFaceShape, data.SecondFaceShape, data.ThirdFaceShape, data.FirstSkinTone, data.SecondSkinTone, data.ThirdSkinTone, data.ParentFaceShapePercent, data.ParentSkinTonePercent, 0f, data.IsParentInheritance);

            while (!HasPedHeadBlendFinished(pedHandle))
            {
                await Delay(0);
            }
            #endregion

            #region appearance
            PedAppearance appData = character.PedAppearance;
            // hair
            SetPedComponentVariation(pedHandle, 2, appData.hairStyle, 0, 0);
            SetPedHairColor(pedHandle, appData.hairColor, appData.hairHighlightColor);
            if (!string.IsNullOrEmpty(appData.HairOverlay.Key) && !string.IsNullOrEmpty(appData.HairOverlay.Value))
            {
                SetPedFacialDecoration(pedHandle, (uint)GetHashKey(appData.HairOverlay.Key), (uint)GetHashKey(appData.HairOverlay.Value));
            }
            // blemishes
            SetPedHeadOverlay(pedHandle, 0, appData.blemishesStyle, appData.blemishesOpacity);
            // bread
            SetPedHeadOverlay(pedHandle, 1, appData.beardStyle, appData.beardOpacity);
            SetPedHeadOverlayColor(pedHandle, 1, 1, appData.beardColor, appData.beardColor);
            // eyebrows
            SetPedHeadOverlay(pedHandle, 2, appData.eyebrowsStyle, appData.eyebrowsOpacity);
            SetPedHeadOverlayColor(pedHandle, 2, 1, appData.eyebrowsColor, appData.eyebrowsColor);
            // ageing
            SetPedHeadOverlay(pedHandle, 3, appData.ageingStyle, appData.ageingOpacity);
            // makeup
            SetPedHeadOverlay(pedHandle, 4, appData.makeupStyle, appData.makeupOpacity);
            SetPedHeadOverlayColor(pedHandle, 4, 2, appData.makeupColor, appData.makeupColor);
            // blush
            SetPedHeadOverlay(pedHandle, 5, appData.blushStyle, appData.blushOpacity);
            SetPedHeadOverlayColor(pedHandle, 5, 2, appData.blushColor, appData.blushColor);
            // complexion
            SetPedHeadOverlay(pedHandle, 6, appData.complexionStyle, appData.complexionOpacity);
            // sundamage
            SetPedHeadOverlay(pedHandle, 7, appData.sunDamageStyle, appData.sunDamageOpacity);
            // lipstick
            SetPedHeadOverlay(pedHandle, 8, appData.lipstickStyle, appData.lipstickOpacity);
            SetPedHeadOverlayColor(pedHandle, 8, 2, appData.lipstickColor, appData.lipstickColor);
            // moles and freckles
            SetPedHeadOverlay(pedHandle, 9, appData.molesFrecklesStyle, appData.molesFrecklesOpacity);
            // chest hair 
            SetPedHeadOverlay(pedHandle, 10, appData.chestHairStyle, appData.chestHairOpacity);
            SetPedHeadOverlayColor(pedHandle, 10, 1, appData.chestHairColor, appData.chestHairColor);
            // body blemishes 
            SetPedHeadOverlay(pedHandle, 11, appData.bodyBlemishesStyle, appData.bodyBlemishesOpacity);
            // eyecolor
            SetPedEyeColor(pedHandle, appData.eyeColor);
            #endregion

            #region Face Shape Data
            for (var i = 0; i < 19; i++)
            {
                SetPedFaceFeature(pedHandle, i, 0f);
            }

            if (character.FaceShapeFeatures.features != null)
            {
                foreach (var t in character.FaceShapeFeatures.features)
                {
                    SetPedFaceFeature(pedHandle, t.Key, t.Value);
                }
            }
            else
            {
                character.FaceShapeFeatures.features = new Dictionary<int, float>();
            }

            #endregion

            #region Clothing Data
            if (character.DrawableVariations.clothes != null && character.DrawableVariations.clothes.Count > 0)
            {
                foreach (var cd in character.DrawableVariations.clothes)
                {
                    SetPedComponentVariation(pedHandle, cd.Key, cd.Value.Key, cd.Value.Value, 0);
                }
            }
            #endregion

            #region Props Data
            if (character.PropVariations.props != null && character.PropVariations.props.Count > 0)
            {
                foreach (var cd in character.PropVariations.props)
                {
                    if (cd.Value.Key > -1)
                    {
                        int textureIndex = cd.Value.Value > -1 ? cd.Value.Value : 0;
                        SetPedPropIndex(pedHandle, cd.Key, cd.Value.Key, textureIndex, true);
                    }
                }
            }
            #endregion

            #region Tattoos

            if (character.PedTatttoos.HeadTattoos == null)
            {
                character.PedTatttoos.HeadTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.TorsoTattoos == null)
            {
                character.PedTatttoos.TorsoTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.LeftArmTattoos == null)
            {
                character.PedTatttoos.LeftArmTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.RightArmTattoos == null)
            {
                character.PedTatttoos.RightArmTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.LeftLegTattoos == null)
            {
                character.PedTatttoos.LeftLegTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.RightLegTattoos == null)
            {
                character.PedTatttoos.RightLegTattoos = new List<KeyValuePair<string, string>>();
            }
            if (character.PedTatttoos.BadgeTattoos == null)
            {
                character.PedTatttoos.BadgeTattoos = new List<KeyValuePair<string, string>>();
            }

            foreach (var tattoo in character.PedTatttoos.HeadTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.TorsoTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.LeftArmTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.RightArmTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.LeftLegTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.RightLegTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.PedTatttoos.BadgeTattoos)
            {
                SetPedDecoration(pedHandle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            #endregion
        }

        public struct MpCharacterCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
