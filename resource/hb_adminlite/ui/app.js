var resourceName = typeof GetParentResourceName === "function"
  ? GetParentResourceName()
  : "hb_adminlite";

var app = document.getElementById("app");
var content = document.getElementById("content");
var coordinateOverlay = document.getElementById("coordinateOverlay");
var coordinateText = document.getElementById("coordinateText");
var inputModal = document.getElementById("inputModal");
var inputModalTitle = document.getElementById("inputModalTitle");
var inputModalDescription = document.getElementById("inputModalDescription");
var inputModalField = document.getElementById("inputModalField");
var inputModalConfirm = document.getElementById("inputModalConfirm");

var latestState = null;
var vehicleModelInput = "";
var entityModelInput = "";
var pollTimer = null;

var currentCategory = "root";
var currentSelection = 0;
var currentItems = [];
var currentItemNodes = [];
var lastStructureSignature = "";
var activeInputPrompt = null;
var playerCurrentSection = null;

var spawnerCatalog = [];
var spawnerCatalogLoaded = false;
var spawnerCatalogLoading = false;
var spawnerCurrentClassId = null;
var weaponAddonCatalog = [];
var weaponAddonCatalogLoaded = false;
var weaponAddonCatalogLoading = false;
var weaponLoadoutCatalog = [];
var weaponLoadoutCatalogLoaded = false;
var weaponLoadoutCatalogLoading = false;
var weaponLoadoutCatalogError = "";
var savedPedCatalog = [];
var savedPedCatalogLoaded = false;
var savedPedCatalogLoading = false;
var savedPedCatalogError = "";
var pedCollectionCatalog = [];
var pedCollectionCatalogLoaded = false;
var pedCollectionCatalogLoading = false;
var pedCollectionCatalogError = "";
var addonPedCatalog = [];
var addonPedCatalogLoaded = false;
var addonPedCatalogLoading = false;
var addonPedCatalogError = "";

var pedSpawnCatalog = {
  appearanceMainPeds: [],
  appearanceAnimalPeds: [],
  appearanceMalePeds: [],
  appearanceFemalePeds: [],
  appearanceOtherPeds: []
};

var pedComponentLabels = [
  "머리",
  "마스크 / 수염",
  "헤어스타일 / 색상",
  "손 / 상체",
  "다리 / 바지",
  "가방 / 낙하산",
  "신발",
  "목 / 스카프",
  "셔츠 / 액세서리",
  "방탄복 / 액세서리 2",
  "배지 / 로고",
  "셔츠 오버레이 / 재킷"
];

var pedPropLabels = [
  { prop: 0, label: "모자 / 헬멧" },
  { prop: 1, label: "안경" },
  { prop: 2, label: "기타" },
  { prop: 6, label: "시계" },
  { prop: 7, label: "팔찌" }
];

var pedCollectionNameMap = {
  "": "기본 컬렉션",
  "base": "기본 컬렉션",
  "mp_m_freemode_01": "온라인 남성 기본",
  "mp_f_freemode_01": "온라인 여성 기본",
  "mp_m_freemode_01_mp_m_heist3": "다이아몬드 카지노 습격",
  "mp_f_freemode_01_mp_f_heist3": "다이아몬드 카지노 습격",
  "mp_m_freemode_01_mp_m_heist4": "카요 페리코 습격",
  "mp_f_freemode_01_mp_f_heist4": "카요 페리코 습격",
  "mp_m_freemode_01_mp_m_tuner": "로스 산토스 튜너",
  "mp_f_freemode_01_mp_f_tuner": "로스 산토스 튜너",
  "mp_m_freemode_01_mp_m_security": "계약",
  "mp_f_freemode_01_mp_f_security": "계약",
  "mp_m_freemode_01_mp_m_sum2": "범죄 조직",
  "mp_f_freemode_01_mp_f_sum2": "범죄 조직",
  "mp_m_freemode_01_mp_m_christmas3": "로스 산토스 마약 전쟁",
  "mp_f_freemode_01_mp_f_christmas3": "로스 산토스 마약 전쟁",
  "mp_m_freemode_01_mp_m_2023_01": "산 안드레아스 용병",
  "mp_f_freemode_01_mp_f_2023_01": "산 안드레아스 용병",
  "mp_m_freemode_01_mp_m_2023_02": "찹 샵",
  "mp_f_freemode_01_mp_f_2023_02": "찹 샵",
  "mp_m_freemode_01_mp_m_2024_01": "바텀 달러 바운티",
  "mp_f_freemode_01_mp_f_2024_01": "바텀 달러 바운티",
  "mp_m_freemode_01_mp_m_2024_02": "에이전트 오브 사보타지",
  "mp_f_freemode_01_mp_f_2024_02": "에이전트 오브 사보타지"
};

var weaponCategoryCatalog = [
  {
    id: "weaponCategoryHandguns",
    title: "권총",
    weapons: [
      { label: "Pistol", model: "weapon_pistol" },
      { label: "Combat Pistol", model: "weapon_combatpistol" },
      { label: "AP Pistol", model: "weapon_appistol" },
      { label: "SNS Pistol", model: "weapon_snspistol" },
      { label: "Heavy Pistol", model: "weapon_heavypistol" },
      { label: "Pistol .50", model: "weapon_pistol50" },
      { label: "Vintage Pistol", model: "weapon_vintagepistol" },
      { label: "Revolver", model: "weapon_revolver" }
    ]
  },
  {
    id: "weaponCategoryRifles",
    title: "권총",
    weapons: [
      { label: "Assault Rifle", model: "weapon_assaultrifle" },
      { label: "Carbine Rifle", model: "weapon_carbinerifle" },
      { label: "Advanced Rifle", model: "weapon_advancedrifle" },
      { label: "Special Carbine", model: "weapon_specialcarbine" },
      { label: "Bullpup Rifle", model: "weapon_bullpuprifle" },
      { label: "Compact Rifle", model: "weapon_compactrifle" }
    ]
  },
  {
    id: "weaponCategoryShotguns",
    title: "샷건",
    weapons: [
      { label: "Pump Shotgun", model: "weapon_pumpshotgun" },
      { label: "Sawed-Off Shotgun", model: "weapon_sawnoffshotgun" },
      { label: "Bullpup Shotgun", model: "weapon_bullpupshotgun" },
      { label: "Assault Shotgun", model: "weapon_assaultshotgun" },
      { label: "Heavy Shotgun", model: "weapon_heavyshotgun" }
    ]
  },
  {
    id: "weaponCategorySmgs",
    title: "SMG / Machine Guns",
    weapons: [
      { label: "Micro SMG", model: "weapon_microsmg" },
      { label: "SMG", model: "weapon_smg" },
      { label: "Assault SMG", model: "weapon_assaultsmg" },
      { label: "Combat PDW", model: "weapon_combatpdw" },
      { label: "MG", model: "weapon_mg" },
      { label: "Combat MG", model: "weapon_combatmg" }
    ]
  },
  {
    id: "weaponCategoryThrowables",
    title: "투척 무기",
    weapons: [
      { label: "Grenade", model: "weapon_grenade" },
      { label: "Sticky Bomb", model: "weapon_stickybomb" },
      { label: "Molotov", model: "weapon_molotov" },
      { label: "Smoke Grenade", model: "weapon_smokegrenade" },
      { label: "Proximity Mine", model: "weapon_proxmine" }
    ]
  },
  {
    id: "weaponCategoryMelee",
    title: "근접 무기",
    weapons: [
      { label: "Knife", model: "weapon_knife" },
      { label: "Nightstick", model: "weapon_nightstick" },
      { label: "Hammer", model: "weapon_hammer" },
      { label: "Bat", model: "weapon_bat" },
      { label: "Crowbar", model: "weapon_crowbar" },
      { label: "Machete", model: "weapon_machete" }
    ]
  },
  {
    id: "weaponCategoryHeavy",
    title: "Heavy Weapons",
    weapons: [
      { label: "Grenade Launcher", model: "weapon_grenadelauncher" },
      { label: "RPG", model: "weapon_rpg" },
      { label: "Minigun", model: "weapon_minigun" },
      { label: "Homing Launcher", model: "weapon_hominglauncher" },
      { label: "Railgun", model: "weapon_railgun" }
    ]
  },
  {
    id: "weaponCategorySnipers",
    title: "저격총",
    weapons: [
      { label: "Sniper Rifle", model: "weapon_sniperrifle" },
      { label: "Heavy Sniper", model: "weapon_heavysniper" },
      { label: "Marksman Rifle", model: "weapon_marksmanrifle" }
    ]
  }
];

function nui(path, body) {
  var payload = body || {};
  return fetch("https://" + resourceName + "/" + path, {
    method: "POST",
    headers: { "Content-Type": "application/json; charset=UTF-8" },
    body: JSON.stringify(payload)
  }).then(function (res) {
    if (!res) {
      return null;
    }

    return res.json().catch(function () {
      return null;
    });
  }).catch(function () {
    return null;
  });
}

function nuiRaw(path, body) {
  var payload = body || {};
  return fetch("https://" + resourceName + "/" + path, {
    method: "POST",
    headers: { "Content-Type": "application/json; charset=UTF-8" },
    body: JSON.stringify(payload)
  }).then(function (res) {
    if (!res) {
      return {
        ok: false,
        status: "no-response",
        text: "",
        json: null,
        error: "응답 객체 없음"
      };
    }

    return res.text().then(function (text) {
      var parsed = null;
      var parseError = "";
      if (text) {
        try {
          parsed = JSON.parse(text);
        } catch (error) {
          parseError = (error && error.message) ? error.message : "JSON 파싱 실패";
        }
      }

      return {
        ok: res.ok,
        status: res.status,
        text: text || "",
        json: parsed,
        error: parseError
      };
    });
  }).catch(function (error) {
    return {
      ok: false,
      status: "fetch-error",
      text: "",
      json: null,
      error: (error && error.message) ? error.message : "fetch 실패"
    };
  });
}

function getVehicleClassFallbackTitle(vehicleClass) {
  var titles = {
    vehclass_0: "Compacts",
    vehclass_1: "Sedans",
    vehclass_2: "SUVs",
    vehclass_3: "Coupes",
    vehclass_4: "Muscle",
    vehclass_5: "Sports Classics",
    vehclass_6: "Sports",
    vehclass_7: "Super",
    vehclass_8: "Motorcycles",
    vehclass_9: "Off-Road",
    vehclass_10: "Industrial",
    vehclass_11: "Utility",
    vehclass_12: "Vans",
    vehclass_13: "Cycles",
    vehclass_14: "Boats",
    vehclass_15: "Helicopters",
    vehclass_16: "Planes",
    vehclass_17: "Service",
    vehclass_18: "Emergency",
    vehclass_19: "Military",
    vehclass_20: "Commercial",
    vehclass_21: "Trains",
    vehclass_22: "Open Wheel"
  };

  return vehicleClass.title || titles[vehicleClass.id] || vehicleClass.source || vehicleClass.id || "차량";
}

function normalizeVehicleCatalog(raw) {
  var rawClasses = raw && Array.isArray(raw.classes) ? raw.classes : [];
  var classes = [];


  for (var i = 0; i < rawClasses.length; i++) {
    var vehicleClass = rawClasses[i];
    if (!vehicleClass) {
      continue;
    }

    var rawVehicles = Array.isArray(vehicleClass.vehicles) ? vehicleClass.vehicles : [];
    var vehicles = [];
    for (var j = 0; j < rawVehicles.length; j++) {
      var entry = rawVehicles[j];
      var model = "";
      var label = "";

      if (typeof entry === "string") {
        model = entry;
      } else if (entry && typeof entry === "object") {
        model = entry.model || entry.id || "";
        label = entry.label || "";
      }

      model = String(model || "").trim().toLowerCase();
      if (!model) {
        continue;
      }

      vehicles.push({
        id: model,
        model: model,
        label: label || model
      });
    }

    if (vehicles.length === 0) {
      continue;
    }

    classes.push({
      id: vehicleClass.id || ("vehclass_" + i),
      title: getVehicleClassFallbackTitle(vehicleClass),
      vehicles: vehicles
    });
  }

  return {
    classes: classes
  };
}

function normalizeCatalogArray(value) {
  if (!value) {
    return [];
  }

  if (Array.isArray(value)) {
    return value;
  }

  if (typeof value === "object") {
    var keys = Object.keys(value);
    var result = [];
    for (var i = 0; i < keys.length; i++) {
      result.push(value[keys[i]]);
    }
    return result;
  }

  return [];
}

function parseCatalogItemsJson(value) {
  if (!value || typeof value !== "string") {
    return [];
  }

  try {
    var parsed = JSON.parse(value);
    return normalizeCatalogArray(parsed);
  } catch (error) {
    addonPedCatalogError = (error && error.message) ? error.message : "PED 카탈로그 JSON 파싱에 실패했습니다.";
    return [];
  }
}

function applyState(payload) {
  if (!app || !payload) {
    return;
  }

  if (payload.open) {
    app.classList.remove("hidden");
  } else {
    app.classList.add("hidden");
    currentCategory = "root";
    currentSelection = 0;
    spawnerCurrentClassId = null;
    playerCurrentSection = null;
    closeInputPrompt();
  }

  if (!payload.state) {
    return;
  }

  latestState = payload.state;
  renderCoordinateOverlay();
  renderIfNeeded();
}

function renderCoordinateOverlay() {
  if (!coordinateOverlay || !coordinateText || !latestState) {
    return;
  }

  if (!latestState.showCoordinates) {
    coordinateOverlay.classList.add("hidden");
    coordinateText.textContent = "";
    return;
  }

  coordinateText.textContent = (latestState.coords || "").replace(/\s+\|\s+/g, "\n");
  coordinateOverlay.classList.remove("hidden");
}

function openInputPrompt(options) {
  if (!inputModal || !inputModalField) {
    return;
  }

  activeInputPrompt = {
    title: options.title || "입력",
    description: options.description || "값을 입력해주세요.",
    placeholder: options.placeholder || "",
    value: options.value || "",
    onConfirm: options.onConfirm
  };

  inputModalTitle.textContent = activeInputPrompt.title;
  inputModalDescription.textContent = activeInputPrompt.description;
  inputModalField.placeholder = activeInputPrompt.placeholder;
  inputModalField.value = activeInputPrompt.value;
  inputModal.classList.remove("hidden");
  nui("inputMode", { active: true });

  setTimeout(function () {
    inputModalField.focus();
    inputModalField.select();
  }, 0);
}

function closeInputPrompt() {
  if (!inputModal) {
    return;
  }

  inputModal.classList.add("hidden");
  activeInputPrompt = null;
  nui("inputMode", { active: false });
}

function confirmInputPrompt() {
  if (!activeInputPrompt || typeof activeInputPrompt.onConfirm !== "function") {
    closeInputPrompt();
    return;
  }

  var value = (inputModalField && inputModalField.value ? inputModalField.value : "").trim();
  activeInputPrompt.onConfirm(value);
  closeInputPrompt();
}

function onOff(value) {
  return value ? "ON" : "OFF";
}

function findPedComponentState(component) {
  var list = latestState && latestState.pedComponents ? latestState.pedComponents : [];
  for (var i = 0; i < list.length; i++) {
    if (list[i].Component === component || list[i].component === component) {
      return list[i];
    }
  }
  return null;
}

function findPedPropState(prop) {
  var list = latestState && latestState.pedProps ? latestState.pedProps : [];
  for (var i = 0; i < list.length; i++) {
    if (list[i].Prop === prop || list[i].prop === prop) {
      return list[i];
    }
  }
  return null;
}

function pedField(state, pascal, camel, fallback) {
  if (!state) {
    return fallback;
  }
  if (state[pascal] !== undefined && state[pascal] !== null) {
    return state[pascal];
  }
  if (state[camel] !== undefined && state[camel] !== null) {
    return state[camel];
  }
  return fallback;
}

function formatPedCollectionName(collection) {
  var key = collection || "";
  if (Object.prototype.hasOwnProperty.call(pedCollectionNameMap, key)) {
    return key ? (pedCollectionNameMap[key] + " (" + key + ")") : pedCollectionNameMap[key];
  }

  var cleaned = key
    .replace(/^mp_[mf]_freemode_01_/, "")
    .replace(/^mp_[mf]_/, "")
    .replace(/^mp_/, "")
    .replace(/_/g, " ")
    .trim();

  if (!cleaned) {
    return "기본 컬렉션";
  }

  var label = cleaned.replace(/\b\w/g, function (char) {
    return char.toUpperCase();
  });

  return key ? (label + " (" + key + ")") : label;
}

function formatPedComponentMeta(component, textureMode) {
  var state = findPedComponentState(component);
  if (!state) {
    return "";
  }
  if (textureMode) {
    var texture = pedField(state, "Texture", "texture", 0);
    var textureCount = pedField(state, "TextureCount", "textureCount", 0);
    return textureCount > 0 ? ("텍스처 " + (texture + 1) + "/" + textureCount) : "텍스처 없음";
  }

  var drawable = pedField(state, "Drawable", "drawable", 0);
  var drawableCount = pedField(state, "DrawableCount", "drawableCount", 0);
  return drawableCount > 0 ? ("변경형 " + (drawable + 1) + "/" + drawableCount) : "변경형 없음";
}

function formatPedPropMeta(prop, textureMode) {
  var state = findPedPropState(prop);
  if (!state) {
    return "";
  }
  var isClear = !!pedField(state, "IsClear", "isClear", false);
  if (textureMode) {
    if (isClear) {
      return "소품 선택 안 함";
    }
    var texture = pedField(state, "Texture", "texture", 0);
    var textureCount = pedField(state, "TextureCount", "textureCount", 0);
    return textureCount > 0 ? ("텍스처 " + (texture + 1) + "/" + textureCount) : "텍스처 없음";
  }

  var drawable = pedField(state, "Drawable", "drawable", -1);
  var drawableCount = pedField(state, "DrawableCount", "drawableCount", 0);
  return isClear ? ("선택 안 함 / " + (drawableCount + 1)) : ("소품 " + (drawable + 2) + "/" + (drawableCount + 1));
}

function hasPermission(permission) {
  if (!latestState || !latestState.hasAccess) {
    return false;
  }

  var permissions = latestState.permissions || {};
  return !!permissions.HBEverything || !!permissions[permission];
}

function getStructureSignature() {
  if (!latestState) {
    return "";
  }

  return JSON.stringify({
    category: currentCategory,
    spawnerClass: spawnerCurrentClassId,
    playerSection: playerCurrentSection,
    showCoordinates: latestState.showCoordinates,
    showVehicleModelDimensions: latestState.showVehicleModelDimensions,
    showPropModelDimensions: latestState.showPropModelDimensions,
    showPedModelDimensions: latestState.showPedModelDimensions,
    showEntityHandles: latestState.showEntityHandles,
    showEntityModels: latestState.showEntityModels,
    showEntityNetOwners: latestState.showEntityNetOwners,
    entityDisplayRange: latestState.entityDisplayRange,
    entitySpawnerActive: latestState.entitySpawnerActive,
    entitySpawnerModel: latestState.entitySpawnerModel,
    timecycleEnabled: latestState.timecycleEnabled,
    timecycleStrength: latestState.timecycleStrength,
    timecycleName: latestState.timecycleName,
    coords: latestState.coords,
    playerGodMode: latestState.playerGodMode,
    playerInvisible: latestState.playerInvisible,
    playerUnlimitedStamina: latestState.playerUnlimitedStamina,
    playerFastRun: latestState.playerFastRun,
    playerFastSwim: latestState.playerFastSwim,
    playerSuperJump: latestState.playerSuperJump,
    playerNoRagdoll: latestState.playerNoRagdoll,
    playerNeverWanted: latestState.playerNeverWanted,
    playerIgnored: latestState.playerIgnored,
    pedComponents: latestState.pedComponents,
    pedProps: latestState.pedProps,
    weaponUnlimitedAmmo: latestState.weaponUnlimitedAmmo,
    weaponNoReload: latestState.weaponNoReload,
    weaponAutoEquipParachutes: latestState.weaponAutoEquipParachutes,
    weaponUnlimitedParachutes: latestState.weaponUnlimitedParachutes,
    parachuteSmokeColorIndex: latestState.parachuteSmokeColorIndex,
    parachutePrimaryStyleIndex: latestState.parachutePrimaryStyleIndex,
    parachuteReserveStyleIndex: latestState.parachuteReserveStyleIndex,
    weaponLoadoutsSetOnRespawn: latestState.weaponLoadoutsSetOnRespawn,
    weaponDefaultLoadout: latestState.weaponDefaultLoadout,
    inVehicle: latestState.inVehicle,
    vehicleEngineOn: latestState.vehicleEngineOn,
    vehicleKeepClean: latestState.vehicleKeepClean,
    vehicleEngineAlwaysOn: latestState.vehicleEngineAlwaysOn,
    vehicleInfiniteFuel: latestState.vehicleInfiniteFuel,
    vehicleFrozen: latestState.vehicleFrozen,
    personalVehicleModel: latestState.personalVehicleModel,
    hasAccess: latestState.hasAccess,
    permissions: latestState.permissions || {},
    weaponAddonCatalogLoaded: weaponAddonCatalogLoaded,
    weaponAddonCatalogCount: weaponAddonCatalog.length,
    weaponLoadoutCatalogLoaded: weaponLoadoutCatalogLoaded,
    weaponLoadoutCatalogCount: weaponLoadoutCatalog.length,
    spawnerCatalogLoaded: spawnerCatalogLoaded,
    spawnerCatalogCount: spawnerCatalog.length
  });
}

function renderIfNeeded(force) {
  if (!latestState) {
    return;
  }

  var signature = getStructureSignature();
  if (force || signature !== lastStructureSignature) {
    lastStructureSignature = signature;
    render();
    return;
  }

  updateSelectionHighlight();
  updateDynamicLabels();
}

function ensureSpawnerCatalog() {
  if (spawnerCatalogLoaded || spawnerCatalogLoading) {
    return;
  }

  spawnerCatalogLoading = true;
  nui("spawnerCatalog").catch(function () {
    spawnerCatalogLoading = false;
    spawnerCatalogLoaded = true;
    spawnerCatalog = [];
    lastStructureSignature = "";
    if (currentCategory === "spawner") {
      render();
    }
  });

  setTimeout(function () {
    if (spawnerCatalogLoading) {
      spawnerCatalogLoading = false;
      spawnerCatalogLoaded = true;
      spawnerCatalog = [];
      lastStructureSignature = "";
      if (currentCategory === "spawner") {
        render();
      }
    }
  }, 5000);
}

function getSpawnerClassById(classId) {
  for (var i = 0; i < spawnerCatalog.length; i++) {
    if (spawnerCatalog[i].id === classId) {
      return spawnerCatalog[i];
    }
  }

  return null;
}

function getWeaponCategoryById(categoryId) {
  for (var i = 0; i < weaponCategoryCatalog.length; i++) {
    if (weaponCategoryCatalog[i].id === categoryId) {
      return weaponCategoryCatalog[i];
    }
  }

  return null;
}

function ensureWeaponAddonCatalog() {
  if (weaponAddonCatalogLoaded || weaponAddonCatalogLoading) {
    return;
  }

  weaponAddonCatalogLoading = true;
  nui("weaponCatalog").then(function (response) {
    weaponAddonCatalogLoading = false;
    weaponAddonCatalogLoaded = true;
    weaponAddonCatalog = response && response.addonWeapons ? response.addonWeapons : [];
    lastStructureSignature = "";
    if (currentCategory === "player" && playerCurrentSection === "weaponAddonWeapons") {
      render();
    }
  });
}

function ensureWeaponLoadoutCatalog() {
  if (weaponLoadoutCatalogLoaded || weaponLoadoutCatalogLoading) {
    return;
  }

  weaponLoadoutCatalogError = "";
  weaponLoadoutCatalogLoading = true;
  nui("weaponLoadoutCatalog").then(function (response) {
    weaponLoadoutCatalogLoading = false;
    if (!response) {
      weaponLoadoutCatalogLoaded = false;
      weaponLoadoutCatalog = [];
      weaponLoadoutCatalogError = "카탈로그 요청 실패";
      lastStructureSignature = "";
      if (currentCategory === "player" && playerCurrentSection === "weaponLoadoutManage") {
        render();
      }
      return;
    }
    weaponLoadoutCatalogLoaded = true;
    weaponLoadoutCatalog = response && response.loadouts ? response.loadouts : [];
    if (latestState && response) {
      latestState.weaponLoadoutsSetOnRespawn = !!response.setOnRespawn;
      latestState.weaponDefaultLoadout = response.defaultLoadout || "";
    }
    lastStructureSignature = "";
    if (currentCategory === "player" && (playerCurrentSection === "weaponLoadouts" || playerCurrentSection === "weaponLoadoutManage")) {
      render();
    }
  });
}

function ensureSavedPedCatalog() {
  if (savedPedCatalogLoaded || savedPedCatalogLoading) {
    return;
  }

  savedPedCatalogError = "";
  savedPedCatalogLoading = true;
  nui("savedPedCatalog").then(function (response) {
    savedPedCatalogLoading = false;
    if (!response) {
      savedPedCatalogLoaded = false;
      savedPedCatalog = [];
      savedPedCatalogError = "카탈로그 요청 실패";
      lastStructureSignature = "";
      if (currentCategory === "player" && playerCurrentSection === "appearanceSavedPeds") {
        render();
      }
      return;
    }

    savedPedCatalogLoaded = true;
    savedPedCatalog = response && response.peds ? response.peds : [];
    lastStructureSignature = "";
    if (currentCategory === "player" && (playerCurrentSection === "playerAppearance" || playerCurrentSection === "appearanceSavedPeds")) {
      render();
    }
  });
}

function ensurePedCollectionCatalog() {
  if (pedCollectionCatalogLoaded || pedCollectionCatalogLoading) {
    return;
  }

  pedCollectionCatalogError = "";
  pedCollectionCatalogLoading = true;
  nui("pedCollectionCatalog").then(function (response) {
    pedCollectionCatalogLoading = false;
    if (!response) {
      pedCollectionCatalogLoaded = false;
      pedCollectionCatalog = [];
      pedCollectionCatalogError = "컬렉션 요청 실패";
      lastStructureSignature = "";
      if (currentCategory === "player" && playerCurrentSection === "pedCollections") {
        render();
      }
      return;
    }

    pedCollectionCatalogLoaded = true;
    pedCollectionCatalog = response.collections || [];
    lastStructureSignature = "";
    if (currentCategory === "player" && playerCurrentSection === "pedCollections") {
      render();
    }
  });
}

function ensureAddonPedCatalog() {
  if (playerCurrentSection !== "appearanceAddonPeds") {
    if (addonPedCatalogLoading) {
      return;
    }

    if (pedSpawnCatalog[playerCurrentSection] && pedSpawnCatalog[playerCurrentSection].length > 0) {
      addonPedCatalogLoaded = true;
      addonPedCatalogError = "";
      return;
    }

    addonPedCatalogLoading = true;
    addonPedCatalogError = "";
    nui("pedCatalogRequest", { section: playerCurrentSection }).then(function () {
    }).catch(function (error) {
      addonPedCatalogLoading = false;
      addonPedCatalogLoaded = false;
      addonPedCatalogError = (error && error.message) ? error.message : "PED 카탈로그를 요청하지 못했습니다.";
      render();
    });
    return;
  }

  if (addonPedCatalogLoaded || addonPedCatalogLoading) {
    return;
  }

  addonPedCatalogLoading = true;
  addonPedCatalogError = "";

  nui("pedCatalog", { section: playerCurrentSection }).then(function (response) {
    if (!response) {
      addonPedCatalogLoading = false;
      addonPedCatalogLoaded = false;
      addonPedCatalogError = "PED 카탈로그 응답이 비어 있습니다.";
      if (currentCategory === "player" && (
        playerCurrentSection === "appearanceAddonPeds" ||
        playerCurrentSection === "appearanceMainPeds" ||
        playerCurrentSection === "appearanceAnimalPeds" ||
        playerCurrentSection === "appearanceMalePeds" ||
        playerCurrentSection === "appearanceFemalePeds" ||
        playerCurrentSection === "appearanceOtherPeds")) {
        render();
      }
      return;
    }

    addonPedCatalogLoading = false;
    addonPedCatalogLoaded = true;
    var items = response.itemsJson ? parseCatalogItemsJson(response.itemsJson) : normalizeCatalogArray(response.items);
    addonPedCatalog = items;
    lastStructureSignature = "";
    if (currentCategory === "player" && (
      playerCurrentSection === "appearanceAddonPeds" ||
      playerCurrentSection === "appearanceMainPeds" ||
      playerCurrentSection === "appearanceAnimalPeds" ||
      playerCurrentSection === "appearanceMalePeds" ||
      playerCurrentSection === "appearanceFemalePeds" ||
      playerCurrentSection === "appearanceOtherPeds")) {
      render();
    }
  }).catch(function (error) {
    addonPedCatalogLoading = false;
    addonPedCatalogLoaded = false;
    addonPedCatalogError = (error && error.message) ? error.message : "PED 카탈로그를 불러오지 못했습니다.";
    if (currentCategory === "player" && (
      playerCurrentSection === "appearanceAddonPeds" ||
      playerCurrentSection === "appearanceMainPeds" ||
      playerCurrentSection === "appearanceAnimalPeds" ||
      playerCurrentSection === "appearanceMalePeds" ||
      playerCurrentSection === "appearanceFemalePeds" ||
      playerCurrentSection === "appearanceOtherPeds")) {
      render();
    }
  });
}

function getSpawnerRootItems() {
  var items = [
    { id: "spawnByModel", label: "모델명으로 스폰", endpoint: "spawnerAction", extra: { getModelFromInput: true }, kind: "action" }
  ];

  for (var i = 0; i < spawnerCatalog.length; i++) {
    items.push({
      id: spawnerCatalog[i].id,
      label: spawnerCatalog[i].title,
      meta: String(spawnerCatalog[i].vehicles ? spawnerCatalog[i].vehicles.length : 0) + "대",
      kind: "class"
    });
  }

  return items;
}

function getSpawnerClassItems(classId) {
  var selectedClass = getSpawnerClassById(classId);
  if (!selectedClass || !selectedClass.vehicles) {
    return [];
  }

  return selectedClass.vehicles.map(function (vehicle) {
    return {
      id: vehicle.id,
      label: vehicle.label,
      meta: vehicle.model,
      endpoint: "spawnerAction",
      extra: { model: vehicle.model },
      kind: "action"
    };
  });
}

function getItemsForCategory(category) {
  switch (category) {
    case "dev":
      var devItems = [
        { id: "clearArea", label: "주변 정리", endpoint: "devAction", action: "clearArea" },
        { id: "toggleCoords", label: "좌표 표시", endpoint: "devAction", action: "toggleCoords" },
        { id: "toggleNoclip", label: "노클립 켜기/끄기", endpoint: "devAction", action: "toggleNoclip" },
        { id: "teleportWaypoint", label: "웨이포인트로 이동", endpoint: "devAction", action: "teleportWaypoint" },
        { id: "toggleVehicleDimensions", label: "차량 외곽선 표시", endpoint: "devAction", action: "toggleVehicleDimensions" },
        { id: "togglePropDimensions", label: "오브젝트 외곽선 표시", endpoint: "devAction", action: "togglePropDimensions" },
        { id: "togglePedDimensions", label: "보행자 외곽선 표시", endpoint: "devAction", action: "togglePedDimensions" },
        { id: "toggleEntityHandles", label: "엔티티 핸들 표시", endpoint: "devAction", action: "toggleEntityHandles" },
        { id: "toggleEntityModels", label: "엔티티 모델 표시", endpoint: "devAction", action: "toggleEntityModels" },
        { id: "toggleEntityNetOwners", label: "네트워크 소유자 표시", endpoint: "devAction", action: "toggleEntityNetOwners" },
        { id: "entityRangeDown", label: "표시 반경 감소", endpoint: "devAction", action: "entityRangeDown" },
        { id: "entityRangeUp", label: "표시 반경 증가", endpoint: "devAction", action: "entityRangeUp" },
        { id: "timecycleCycle", label: "Change Timecycle", endpoint: "devAction", action: "timecycleCycle" },
        { id: "toggleTimecycle", label: "타임사이클 효과 사용", endpoint: "devAction", action: "toggleTimecycle" },
        { id: "timecycleStrengthCycle", label: "Change Timecycle Strength", endpoint: "devAction", action: "timecycleStrengthCycle" },
        { id: "spawnEntity", label: "엔티티 생성", endpoint: "devAction", action: "spawnEntity", extra: { getEntityModelFromInput: true } },
        { id: "confirmEntity", label: "배치 확정", endpoint: "devAction", action: "confirmEntity" },
        { id: "duplicateEntity", label: "배치 확정 후 복제", endpoint: "devAction", action: "duplicateEntity" },
        { id: "cancelEntity", label: "배치 취소", endpoint: "devAction", action: "cancelEntity" }
      ];
      return devItems.filter(function (item) {
        return item.action === "toggleNoclip" ? hasPermission("HBNoClip") : hasPermission("HBDevTools");
      });
    case "player":
      if (!playerCurrentSection) {
        var playerSections = [];
        if (hasPermission("HBPlayerOptions") || hasPermission("HBNoClip")) {
          playerSections.push({ id: "playerOptions", label: "플레이어 옵션", kind: "playerSection" });
        }
        if (hasPermission("HBPlayerAppearance")) {
          playerSections.push({ id: "playerAppearance", label: "플레이어 외형", kind: "playerSection" });
        }
        if (hasPermission("HBWeaponOptions")) {
          playerSections.push({ id: "weaponOptions", label: "무기 옵션", kind: "playerSection" });
        }
        if (hasPermission("HBWeaponLoadouts")) {
          playerSections.push({ id: "weaponLoadouts", label: "무기 로드아웃", kind: "playerSection" });
        }
        return playerSections.length > 0 ? playerSections : [
          { id: "emptyPlayerPermissions", label: "사용 가능한 항목이 없습니다.", kind: "placeholder" }
        ];
      }

      if (playerCurrentSection === "playerOptions") {
        var playerOptionItems = [
          { id: "toggleNoclip", label: "노클립 켜기/끄기", endpoint: "devAction", action: "toggleNoclip" },
          { id: "toggleGodMode", label: "무적 모드", endpoint: "playerAction", action: "toggleGodMode" },
          { id: "toggleInvisible", label: "Invisible", endpoint: "playerAction", action: "toggleInvisible" },
          { id: "toggleStamina", label: "무제한 스태미나", endpoint: "playerAction", action: "toggleStamina" },
          { id: "toggleFastRun", label: "Fast Run", endpoint: "playerAction", action: "toggleFastRun" },
          { id: "toggleFastSwim", label: "빠른 수영", endpoint: "playerAction", action: "toggleFastSwim" },
          { id: "toggleSuperJump", label: "슈퍼 점프", endpoint: "playerAction", action: "toggleSuperJump" },
          { id: "toggleNoRagdoll", label: "레그돌 비활성화", endpoint: "playerAction", action: "toggleNoRagdoll" },
          { id: "toggleNeverWanted", label: "수배 해제", endpoint: "playerAction", action: "toggleNeverWanted" },
          { id: "toggleIgnored", label: "NPC 무시", endpoint: "playerAction", action: "toggleIgnored" },
          { id: "heal", label: "체력/방어구 회복", endpoint: "playerAction", action: "heal" },
          { id: "clean", label: "플레이어 세척", endpoint: "playerAction", action: "clean" },
          { id: "dry", label: "건조", endpoint: "playerAction", action: "dry" },
          { id: "wet", label: "젖게 하기", endpoint: "playerAction", action: "wet" },
          { id: "suicide", label: "자살", endpoint: "playerAction", action: "suicide" }
        ];
        return playerOptionItems.filter(function (item) {
          return item.action === "toggleNoclip" ? hasPermission("HBNoClip") : hasPermission("HBPlayerOptions");
        });
      }

      if (playerCurrentSection === "playerAppearance") {
        return [
          { id: "pedCustomization", label: "PED 커스터마이징", kind: "playerSection" },
          { id: "pedCollections", label: "PED 컬렉션", kind: "playerSection" },
          { id: "saveCurrentPed", label: "Save PED", endpoint: "playerAction", action: "saveCurrentPed", extra: { getPedSaveNameFromInput: true } },
          { id: "appearanceSavedPeds", label: "저장된 PED", kind: "playerSection" },
          { id: "appearanceSpawnPeds", label: "PED 스폰", kind: "playerSection" },
          { id: "cycleWalkingStyle", label: "Walking Style", endpoint: "playerAction", action: "cycleWalkingStyle" },
          { id: "cycleClothingGlow", label: "Clothing Glow Style", endpoint: "playerAction", action: "cycleClothingGlow" }
        ];
      }

      if (playerCurrentSection === "pedCustomization") {
        var pedCustomizationItems = [];
        for (var pc = 0; pc < pedComponentLabels.length; pc++) {
          pedCustomizationItems.push({
            id: "pedComponentNext_" + pc,
            label: pedComponentLabels[pc] + " 변경형",
            meta: formatPedComponentMeta(pc, false),
            endpoint: "playerAction",
            action: "pedComponentNext",
            extra: { component: pc }
          });
          pedCustomizationItems.push({
            id: "pedComponentTextureNext_" + pc,
            label: pedComponentLabels[pc] + " 텍스처",
            meta: formatPedComponentMeta(pc, true),
            endpoint: "playerAction",
            action: "pedComponentTextureNext",
            extra: { component: pc }
          });
        }
        for (var pp = 0; pp < pedPropLabels.length; pp++) {
          pedCustomizationItems.push({
            id: "pedPropNext_" + pedPropLabels[pp].prop,
            label: pedPropLabels[pp].label + " 변경형",
            meta: formatPedPropMeta(pedPropLabels[pp].prop, false),
            endpoint: "playerAction",
            action: "pedPropNext",
            extra: { prop: pedPropLabels[pp].prop }
          });
          pedCustomizationItems.push({
            id: "pedPropTextureNext_" + pedPropLabels[pp].prop,
            label: pedPropLabels[pp].label + " 텍스처",
            meta: formatPedPropMeta(pedPropLabels[pp].prop, true),
            endpoint: "playerAction",
            action: "pedPropTextureNext",
            extra: { prop: pedPropLabels[pp].prop }
          });
        }
        return pedCustomizationItems;
      }

      if (playerCurrentSection === "pedCollections") {
        if (pedCollectionCatalogLoading) {
          return [
            { id: "loadingPedCollections", label: "불러오는 중", meta: "PED 컬렉션 목록을 불러오는 중입니다.", kind: "placeholder" }
          ];
        }

        if (pedCollectionCatalogError) {
          return [
            { id: "pedCollectionError", label: "컬렉션 요청 실패", meta: pedCollectionCatalogError, kind: "placeholder" }
          ];
        }

        if (!pedCollectionCatalogLoaded || pedCollectionCatalog.length === 0) {
          return [
            { id: "emptyPedCollections", label: "목록 없음", meta: "사용 가능한 PED 컬렉션이 없습니다.", kind: "placeholder" }
          ];
        }

        return pedCollectionCatalog.map(function (collection) {
          var rawCollection = collection.collection || "";
          return {
            id: "pedCollection::" + encodeURIComponent(rawCollection),
            label: formatPedCollectionName(rawCollection),
            meta: rawCollection ? ("#" + collection.index + " | " + rawCollection) : ("#" + collection.index),
            kind: "playerSection"
          };
        });
      }

      if (playerCurrentSection.indexOf("pedCollection::") === 0) {
        var collectionName = decodeURIComponent(playerCurrentSection.replace("pedCollection::", ""));
        var collectionItems = [];
        for (var cc = 0; cc < pedComponentLabels.length; cc++) {
          collectionItems.push({
            id: "pedCollectionComponentNext_" + cc,
            label: pedComponentLabels[cc] + " 변경형",
            meta: formatPedComponentMeta(cc, false),
            endpoint: "playerAction",
            action: "pedCollectionComponentNext",
            extra: { component: cc, collection: collectionName }
          });
          collectionItems.push({
            id: "pedCollectionComponentTextureNext_" + cc,
            label: pedComponentLabels[cc] + " 텍스처",
            meta: formatPedComponentMeta(cc, true),
            endpoint: "playerAction",
            action: "pedCollectionComponentTextureNext",
            extra: { component: cc, collection: collectionName }
          });
        }
        for (var cp = 0; cp < pedPropLabels.length; cp++) {
          collectionItems.push({
            id: "pedCollectionPropNext_" + pedPropLabels[cp].prop,
            label: pedPropLabels[cp].label + " 변경형",
            meta: formatPedPropMeta(pedPropLabels[cp].prop, false),
            endpoint: "playerAction",
            action: "pedCollectionPropNext",
            extra: { prop: pedPropLabels[cp].prop, collection: collectionName }
          });
          collectionItems.push({
            id: "pedCollectionPropTextureNext_" + pedPropLabels[cp].prop,
            label: pedPropLabels[cp].label + " 텍스처",
            meta: formatPedPropMeta(pedPropLabels[cp].prop, true),
            endpoint: "playerAction",
            action: "pedCollectionPropTextureNext",
            extra: { prop: pedPropLabels[cp].prop, collection: collectionName }
          });
        }
        return collectionItems;
      }

      if (playerCurrentSection === "appearanceSavedPeds") {
        var savedPedItems = [];
        if (savedPedCatalogLoading) {
          savedPedItems.push({ id: "loadingSavedPeds", label: "Loading", meta: "Loading saved PED list...", kind: "placeholder" });
          return savedPedItems;
        }

        if (savedPedCatalogError) {
          savedPedItems.push({ id: "savedPedError", label: savedPedCatalogError, meta: "savedPedCatalog 콜백이 응답하지 않았습니다.", kind: "placeholder" });
          return savedPedItems;
        }

        if (savedPedCatalogLoaded && savedPedCatalog.length > 0) {
          for (var s = 0; s < savedPedCatalog.length; s++) {
            savedPedItems.push({
              id: "appearanceSavedPed::" + encodeURIComponent(savedPedCatalog[s].key),
              label: savedPedCatalog[s].name,
              meta: savedPedCatalog[s].model || "저장된 PED",
              kind: "playerSection"
            });
          }
        } else {
          savedPedItems.push({ id: "emptySavedPeds", label: "저장된 PED 없음", meta: "먼저 PED를 저장해주세요.", kind: "placeholder" });
        }

        return savedPedItems;
      }

      if (playerCurrentSection.indexOf("appearanceSavedPed::") === 0) {
        var savedPedKey = decodeURIComponent(playerCurrentSection.replace("appearanceSavedPed::", ""));
        return [
          { id: "spawnSavedPed", label: "저장된 PED 스폰", endpoint: "playerAction", action: "spawnSavedPed", extra: { key: savedPedKey } },
          { id: "deleteSavedPed", label: "저장된 PED 삭제", endpoint: "playerAction", action: "deleteSavedPed", extra: { key: savedPedKey } }
        ];
      }

      if (playerCurrentSection === "appearanceSpawnPeds") {
        return [
          { id: "spawnPedByName", label: "이름으로 스폰", endpoint: "playerAction", action: "spawnPedByName", extra: { getPedModelFromInput: true } },
          { id: "appearanceAddonPeds", label: "애드온 PED", kind: "playerSection" },
          { id: "appearanceMainPeds", label: "주요 PED", kind: "playerSection" },
          { id: "appearanceAnimalPeds", label: "동물", kind: "playerSection" },
          { id: "appearanceMalePeds", label: "남성 PED", kind: "playerSection" },
          { id: "appearanceFemalePeds", label: "여성 PED", kind: "playerSection" },
          { id: "appearanceOtherPeds", label: "기타 PED", kind: "playerSection" }
        ];
      }

      if (playerCurrentSection === "appearanceAddonPeds") {
        if (addonPedCatalogLoading) {
          return [
            { id: "loadingAddonPeds", label: "Loading", meta: "Loading addon PED list...", kind: "placeholder" }
          ];
        }

        if (addonPedCatalogError) {
          return [
            { id: "errorAddonPeds", label: "카탈로그 요청 실패", meta: addonPedCatalogError, kind: "placeholder" }
          ];
        }

        if (!addonPedCatalogLoaded || addonPedCatalog.length === 0) {
          return [
            { id: "emptyAddonPeds", label: "목록 없음", meta: "사용 가능한 애드온 PED가 없습니다.", kind: "placeholder" }
          ];
        }

        return addonPedCatalog.map(function (ped) {
          return {
            id: ped.model,
            label: ped.label || ped.model,
            meta: ped.model,
            endpoint: "playerAction",
            action: "spawnPedByName",
            extra: { model: ped.model }
          };
        });
      }

      if (playerCurrentSection === "appearanceMainPeds" ||
          playerCurrentSection === "appearanceAnimalPeds" ||
          playerCurrentSection === "appearanceMalePeds" ||
          playerCurrentSection === "appearanceFemalePeds" ||
          playerCurrentSection === "appearanceOtherPeds") {
        if (addonPedCatalogLoading) {
          return [
            { id: "loadingBuiltInPeds", label: "Loading", meta: "Loading PED list...", kind: "placeholder" }
          ];
        }

        if (addonPedCatalogError) {
          return [
            { id: "errorBuiltInPeds", label: "카탈로그 요청 실패", meta: addonPedCatalogError, kind: "placeholder" }
          ];
        }
      }

      if (pedSpawnCatalog[playerCurrentSection]) {
        if (!addonPedCatalogLoaded && pedSpawnCatalog[playerCurrentSection].length === 0) {
          return [
            { id: "emptyBuiltInPeds", label: "목록 없음", meta: "사용 가능한 PED 목록이 없습니다.", kind: "placeholder" }
          ];
        }

        return pedSpawnCatalog[playerCurrentSection].map(function (ped) {
          return {
            id: ped.model,
            label: ped.label,
            meta: ped.meta || ped.model,
            endpoint: "playerAction",
            action: "spawnPedByName",
            extra: { model: ped.model }
          };
        });
      }

      if (playerCurrentSection === "weaponOptions") {
        return [
          { id: "getAllWeapons", label: "Give All Weapons", endpoint: "weaponAction", action: "getAllWeapons" },
          { id: "removeAllWeapons", label: "전체 무기 제거", endpoint: "weaponAction", action: "removeAllWeapons" },
          { id: "toggleUnlimitedAmmo", label: "무제한 탄약", endpoint: "weaponAction", action: "toggleUnlimitedAmmo" },
          { id: "toggleNoReload", label: "재장전 없음", endpoint: "weaponAction", action: "toggleNoReload" },
          { id: "refillAllAmmo", label: "Refill All Ammo", endpoint: "weaponAction", action: "refillAllAmmo" },
          { id: "setAllAmmo", label: "전체 탄약 수량 설정", endpoint: "weaponAction", action: "setAllAmmo", extra: { getAmmoCountFromInput: true } },
          { id: "spawnWeaponByName", label: "Give Weapon By Name", endpoint: "weaponAction", action: "spawnWeaponByName", extra: { getWeaponNameFromInput: true } },
          { id: "weaponAddonWeapons", label: "애드온 무기", kind: "playerSection" },
          { id: "weaponParachuteOptions", label: "낙하산 옵션", kind: "playerSection" },
          { id: "weaponCategories", label: "무기 카테고리", kind: "playerSection" }
        ];
      }

      if (playerCurrentSection === "weaponCategories") {
        return weaponCategoryCatalog.map(function (category) {
          return {
            id: category.id,
            label: category.title,
            meta: String(category.weapons.length) + " weapons",
            kind: "playerSection"
          };
        });
      }

      var selectedWeaponCategory = getWeaponCategoryById(playerCurrentSection);
      if (selectedWeaponCategory) {
        return selectedWeaponCategory.weapons.map(function (weapon) {
          return {
            id: weapon.model,
            label: weapon.label,
            meta: weapon.model,
            endpoint: "weaponAction",
            action: "spawnWeaponByName",
            extra: { model: weapon.model }
          };
        });
      }

      if (playerCurrentSection === "weaponAddonWeapons") {
        if (weaponAddonCatalogLoading) {
          return [
            { id: "loadingAddonWeapons", label: "Loading", meta: "Loading addon weapon list...", kind: "placeholder" }
          ];
        }

        if (!weaponAddonCatalogLoaded || weaponAddonCatalog.length === 0) {
          return [
            { id: "emptyAddonWeapons", label: "목록 없음", meta: "사용 가능한 애드온 무기가 없습니다.", kind: "placeholder" }
          ];
        }

        return weaponAddonCatalog.map(function (weapon) {
          return {
            id: weapon.id,
            label: weapon.label || weapon.model,
            meta: weapon.model,
            endpoint: "weaponAction",
            action: "spawnWeaponByName",
            extra: { model: weapon.model }
          };
        });
      }

      if (playerCurrentSection === "weaponParachuteOptions") {
        return [
          { id: "togglePrimaryParachute", label: "주 낙하산 켜기/끄기", endpoint: "weaponAction", action: "togglePrimaryParachute" },
          { id: "enableReserveParachute", label: "Enable Reserve Parachute", endpoint: "weaponAction", action: "enableReserveParachute" },
          { id: "toggleAutoEquipParachutes", label: "자동 낙하산 장착", endpoint: "weaponAction", action: "toggleAutoEquipParachutes" },
          { id: "toggleUnlimitedParachutes", label: "Unlimited Parachutes", endpoint: "weaponAction", action: "toggleUnlimitedParachutes" },
          { id: "cycleParachuteSmoke", label: "Change Smoke Color", endpoint: "weaponAction", action: "cycleParachuteSmoke" },
          { id: "cyclePrimaryParachuteStyle", label: "Change Primary Parachute Style", endpoint: "weaponAction", action: "cyclePrimaryParachuteStyle" },
          { id: "cycleReserveParachuteStyle", label: "Change Reserve Parachute Style", endpoint: "weaponAction", action: "cycleReserveParachuteStyle" }
        ];
      }

      if (playerCurrentSection === "weaponLoadouts") {
        return [
          { id: "saveWeaponLoadout", label: "Save Loadout", endpoint: "weaponLoadoutAction", action: "saveLoadout", extra: { getLoadoutNameFromInput: true } },
          { id: "weaponLoadoutManage", label: "Manage Loadouts", kind: "playerSection" },
          { id: "toggleLoadoutOnRespawn", label: "리스폰 시 기본 로드아웃 복원", endpoint: "weaponLoadoutAction", action: "toggleLoadoutOnRespawn" }
        ];
      }

      if (playerCurrentSection === "weaponLoadoutManage") {
        var loadoutItems = [];
        if (weaponLoadoutCatalogLoading) {
          loadoutItems.push({ id: "loadingWeaponLoadouts", label: "Loading", meta: "Loading saved weapon loadouts...", kind: "placeholder" });
          return loadoutItems;
        }

        if (weaponLoadoutCatalogError) {
          loadoutItems.push({ id: "weaponLoadoutError", label: weaponLoadoutCatalogError, meta: "weaponLoadoutCatalog 콜백이 응답하지 않았습니다.", kind: "placeholder" });
          return loadoutItems;
        }

        if (weaponLoadoutCatalogLoaded && weaponLoadoutCatalog.length > 0) {
          for (var w = 0; w < weaponLoadoutCatalog.length; w++) {
            loadoutItems.push({
              id: "weaponLoadout_" + weaponLoadoutCatalog[w].id,
              label: weaponLoadoutCatalog[w].name,
              meta: String(weaponLoadoutCatalog[w].count) + " weapons - Enter to manage",
              kind: "playerSection"
            });
          }
        } else {
          loadoutItems.push({ id: "emptyWeaponLoadouts", label: "저장된 로드아웃 없음", meta: "먼저 로드아웃을 저장해주세요.", kind: "placeholder" });
        }

        return loadoutItems;
      }

      if (playerCurrentSection.indexOf("weaponLoadout_") === 0) {
        var loadoutName = playerCurrentSection.replace("weaponLoadout_", "");
        return [
          { id: "equipLoadout", label: "불러오기", endpoint: "weaponLoadoutAction", action: "equipLoadout", extra: { name: loadoutName } },
          { id: "setDefaultLoadout", label: "기본 로드아웃으로 설정", endpoint: "weaponLoadoutAction", action: "setDefaultLoadout", extra: { name: loadoutName } },
          { id: "deleteLoadout", label: "로드아웃 삭제", endpoint: "weaponLoadoutAction", action: "deleteLoadout", extra: { name: loadoutName } }
        ];
      }

      return [
        { id: "comingSoon", label: "Coming Soon", meta: "This category will be added later.", kind: "placeholder" }
      ];
    case "vehicle":
      if (!hasPermission("HBVehicleOptions")) {
        return [
          { id: "emptyVehiclePermissions", label: "사용 가능한 항목이 없습니다.", kind: "placeholder" }
        ];
      }
      return [
        { id: "repair", label: "차량 수리", endpoint: "vehicleAction", action: "repair" },
        { id: "clean", label: "세차", endpoint: "vehicleAction", action: "clean" },
        { id: "toggleKeepClean", label: "항상 깨끗하게", endpoint: "vehicleAction", action: "toggleKeepClean" },
        { id: "toggleEngineAlwaysOn", label: "엔진 항상 켜짐", endpoint: "vehicleAction", action: "toggleEngineAlwaysOn" },
        { id: "toggleInfiniteFuel", label: "무한 연료", endpoint: "vehicleAction", action: "toggleInfiniteFuel" },
        { id: "toggleFreeze", label: "차량 고정", endpoint: "vehicleAction", action: "toggleFreeze" },
        { id: "toggleEngine", label: "엔진 켜기/끄기", endpoint: "vehicleAction", action: "toggleEngine" },
        { id: "flip", label: "차량 뒤집힘 복구", endpoint: "vehicleAction", action: "flip" },
        { id: "delete", label: "차량 삭제", endpoint: "vehicleAction", action: "delete" },
        { id: "saveCurrent", label: "Save Current Vehicle", endpoint: "personalVehicleAction", action: "saveCurrent" },
        { id: "spawnSaved", label: "저장된 차량 호출", endpoint: "personalVehicleAction", action: "spawnSaved" },
        { id: "clearSaved", label: "저장 삭제", endpoint: "personalVehicleAction", action: "clearSaved" }
      ];
    case "spawner":
      if (!hasPermission("HBVehicleSpawner")) {
        return [
          { id: "emptySpawnerPermissions", label: "사용 가능한 항목이 없습니다.", kind: "placeholder" }
        ];
      }
      if (spawnerCurrentClassId) {
        return getSpawnerClassItems(spawnerCurrentClassId);
      }
      return getSpawnerRootItems();
    default:
      var rootItems = [];
      if (hasPermission("HBDevTools") || hasPermission("HBNoClip")) {
        rootItems.push({ id: "dev", title: "개발자 도구", meta: "좌표, 노클립, 외곽선, 엔티티 정보, 타임사이클" });
      }
      if (hasPermission("HBPlayerOptions") || hasPermission("HBPlayerAppearance") || hasPermission("HBWeaponOptions") || hasPermission("HBWeaponLoadouts") || hasPermission("HBNoClip")) {
        rootItems.push({ id: "player", title: "플레이어 관련 옵션", meta: "무적 " + onOff(latestState.playerGodMode) + " | 투명 " + onOff(latestState.playerInvisible) });
      }
      if (hasPermission("HBVehicleOptions")) {
        rootItems.push({ id: "vehicle", title: "차량 관련 옵션", meta: latestState.personalVehicleModel ? ("저장됨: " + latestState.personalVehicleModel) : (latestState.inVehicle ? ("차량 탑승 중 | 엔진 " + onOff(latestState.vehicleEngineOn)) : "차량 탑승 중일 때만 일부 옵션이 작동합니다.") });
      }
      if (hasPermission("HBVehicleSpawner")) {
        rootItems.push({ id: "spawner", title: "차량 스포너", meta: "모델명 입력, 클래스 차량 목록" });
      }
      return rootItems.length > 0 ? rootItems : [
        { id: "emptyRootPermissions", title: "사용 가능한 항목이 없습니다.", meta: "권한 설정을 확인해주세요." }
      ];
  }
}

function isToggleItem(itemId) {
  return [
    "toggleCoords",
    "toggleNoclip",
    "toggleVehicleDimensions",
    "togglePropDimensions",
    "togglePedDimensions",
    "toggleEntityHandles",
    "toggleEntityModels",
    "toggleEntityNetOwners",
    "toggleTimecycle",
    "toggleGodMode",
    "toggleInvisible",
    "toggleStamina",
    "toggleFastRun",
    "toggleFastSwim",
    "toggleSuperJump",
    "toggleNoRagdoll",
    "toggleNeverWanted",
    "toggleIgnored",
    "toggleUnlimitedAmmo",
    "toggleNoReload",
    "toggleAutoEquipParachutes",
    "toggleUnlimitedParachutes",
    "toggleLoadoutOnRespawn",
    "toggleKeepClean",
    "toggleEngineAlwaysOn",
    "toggleInfiniteFuel",
    "toggleFreeze",
    "toggleEngine"
  ].indexOf(itemId) !== -1;
}

function isItemActive(itemId) {
  if (!latestState) {
    return false;
  }

  switch (itemId) {
    case "toggleCoords": return !!latestState.showCoordinates;
    case "toggleNoclip": return !!latestState.noclipEnabled;
    case "toggleVehicleDimensions": return !!latestState.showVehicleModelDimensions;
    case "togglePropDimensions": return !!latestState.showPropModelDimensions;
    case "togglePedDimensions": return !!latestState.showPedModelDimensions;
    case "toggleEntityHandles": return !!latestState.showEntityHandles;
    case "toggleEntityModels": return !!latestState.showEntityModels;
    case "toggleEntityNetOwners": return !!latestState.showEntityNetOwners;
    case "toggleTimecycle": return !!latestState.timecycleEnabled;
    case "toggleGodMode": return !!latestState.playerGodMode;
    case "toggleInvisible": return !!latestState.playerInvisible;
    case "toggleStamina": return !!latestState.playerUnlimitedStamina;
    case "toggleFastRun": return !!latestState.playerFastRun;
    case "toggleFastSwim": return !!latestState.playerFastSwim;
    case "toggleSuperJump": return !!latestState.playerSuperJump;
    case "toggleNoRagdoll": return !!latestState.playerNoRagdoll;
    case "toggleNeverWanted": return !!latestState.playerNeverWanted;
    case "toggleIgnored": return !!latestState.playerIgnored;
    case "toggleUnlimitedAmmo": return !!latestState.weaponUnlimitedAmmo;
    case "toggleNoReload": return !!latestState.weaponNoReload;
    case "toggleAutoEquipParachutes": return !!latestState.weaponAutoEquipParachutes;
    case "toggleUnlimitedParachutes": return !!latestState.weaponUnlimitedParachutes;
    case "toggleLoadoutOnRespawn": return !!latestState.weaponLoadoutsSetOnRespawn;
    case "toggleKeepClean": return !!latestState.vehicleKeepClean;
    case "toggleEngineAlwaysOn": return !!latestState.vehicleEngineAlwaysOn;
    case "toggleInfiniteFuel": return !!latestState.vehicleInfiniteFuel;
    case "toggleFreeze": return !!latestState.vehicleFrozen;
    case "toggleEngine": return !!latestState.vehicleEngineOn;
    default: return false;
  }
}

function createActionButton(item) {
  var button = document.createElement("button");
  button.className = "action-button";
  button.type = "button";
  button.setAttribute("data-item-id", item.id);

  var labelEl = document.createElement("span");
  labelEl.className = "action-label";
  labelEl.textContent = item.label;
  button.appendChild(labelEl);

  if (isToggleItem(item.id)) {
    var badge = document.createElement("span");
    badge.className = "toggle-badge";
    button.appendChild(badge);
  }

  button.addEventListener("click", function () {
    executeItem(item);
  });

  updateActionButtonState(button, item);
  return button;
}

function updateActionButtonState(button, item) {
  var labelEl = button.querySelector(".action-label");
  var badgeEl = button.querySelector(".toggle-badge");
  var label = item.label;

  if (latestState) {
    if (item.id === "timecycleCycle") {
      label = "타임사이클 변경: " + (latestState.timecycleName || "default");
    } else if (item.id === "timecycleStrengthCycle") {
      label = "타임사이클 강도 : " + String(latestState.timecycleStrength || 0) + "/20";
    } else if (item.id === "cycleWalkingStyle") {
      label = "걷기 스타일: " + (latestState.playerWalkingStyleName || "기본");
    } else if (item.id === "cycleClothingGlow") {
      label = "발광 의상 스타일: " + (latestState.playerClothingGlowName || "켜짐");
    } else if (item.id === "cycleParachuteSmoke") {
      var smokeNames = ["None", "Red", "Orange", "Yellow", "Blue", "Black"];
      label = "연막 색상 변경: " + smokeNames[Math.max(0, Math.min(smokeNames.length - 1, latestState.parachuteSmokeColorIndex || 0))];
    } else if (item.id === "cyclePrimaryParachuteStyle") {
      label = "Change Primary Parachute Style: " + String((latestState.parachutePrimaryStyleIndex || 0) + 1);
    } else if (item.id === "cycleReserveParachuteStyle") {
      label = "Change Reserve Parachute Style: " + String((latestState.parachuteReserveStyleIndex || 0) + 1);
    }
  }

  if (item.meta && !isToggleItem(item.id)) {
    label = label + " : " + item.meta;
  }

  if (labelEl) {
    labelEl.textContent = label;
  }

  if (!isToggleItem(item.id) || !badgeEl) {
    button.classList.remove("is-active");
    return;
  }

  var active = isItemActive(item.id);
  badgeEl.textContent = active ? "ON" : "OFF";
  badgeEl.classList.toggle("is-on", active);
  badgeEl.classList.toggle("is-off", !active);
  button.classList.toggle("is-active", active);
}

function createCategoryCard(item) {
  var button = document.createElement("button");
  button.className = "category-card";
  button.type = "button";
  button.setAttribute("data-item-id", item.id);

  var titleEl = document.createElement("div");
  titleEl.className = "category-title";
  titleEl.textContent = item.title;

  var metaEl = document.createElement("div");
  metaEl.className = "category-meta";
  metaEl.textContent = item.meta;

  button.appendChild(titleEl);
  button.appendChild(metaEl);
  button.addEventListener("click", function () {
    enterCategory(item.id);
  });
  return button;
}

function createInfoRow(label, value) {
  var row = document.createElement("div");
  row.className = "control-card";

  var top = document.createElement("div");
  top.className = "control-top";

  var labelEl = document.createElement("div");
  labelEl.className = "field-label";
  labelEl.textContent = label;

  var valueEl = document.createElement("div");
  valueEl.className = "field-value";
  valueEl.textContent = value;

  top.appendChild(labelEl);
  top.appendChild(valueEl);
  row.appendChild(top);
  return row;
}

function createBackRow(title, meta) {
  var wrapper = document.createElement("div");
  wrapper.className = "section";

  var top = document.createElement("div");
  top.className = "submenu-header";

  var left = document.createElement("div");
  var titleEl = document.createElement("div");
  titleEl.className = "section-title";
  titleEl.textContent = title;
  left.appendChild(titleEl);

  if (meta) {
    var metaEl = document.createElement("div");
    metaEl.className = "section-meta";
    metaEl.textContent = meta;
    left.appendChild(metaEl);
  }

  top.appendChild(left);
  wrapper.appendChild(top);
  return wrapper;
}

function renderRoot() {
  var list = document.createElement("div");
  list.className = "category-list";
  currentItems = getItemsForCategory("root");

  for (var i = 0; i < currentItems.length; i++) {
    list.appendChild(createCategoryCard(currentItems[i]));
  }

  content.appendChild(list);
}

function renderDev() {
  var section = createBackRow("개발자 도구", "주변 정리, 좌표, 노클립, 외곽선, 엔티티 정보, 타임사이클을 다룹니다.");

  var entityInfo = document.createElement("div");
  entityInfo.className = "section-subgroup";
  entityInfo.appendChild(createInfoRow("현재 표시 반경", String(latestState.entityDisplayRange || 0) + "m"));

  var timecycleInfo = document.createElement("div");
  timecycleInfo.className = "section-subgroup";
  timecycleInfo.appendChild(createInfoRow("타임사이클 사용 상태", onOff(latestState.timecycleEnabled)));

  var spawnerInfo = document.createElement("div");
  spawnerInfo.className = "section-subgroup";
  spawnerInfo.appendChild(createInfoRow("Entity Placement", latestState.entitySpawnerActive ? "Placing" : "Idle"));
  spawnerInfo.appendChild(createInfoRow("Current Entity Model", latestState.entitySpawnerModel || "None"));

  var grid = document.createElement("div");
  grid.className = "button-grid";
  currentItems = getItemsForCategory("dev");

  section.appendChild(entityInfo);
  section.appendChild(timecycleInfo);
  section.appendChild(spawnerInfo);

  for (var i = 0; i < currentItems.length; i++) {
    grid.appendChild(createActionButton(currentItems[i]));
  }

  section.appendChild(grid);
  content.appendChild(section);
}

function renderPlayer() {
  currentItems = getItemsForCategory("player");

  if (!playerCurrentSection) {
    var rootSection = createBackRow(
      "플레이어 관련 옵션",
      "플레이어 옵션, 외형, MP 페드, 무기, 로드아웃을 이곳에서 관리합니다."
    );
    var rootGrid = document.createElement("div");
    rootGrid.className = "button-grid";

    for (var i = 0; i < currentItems.length; i++) {
      rootGrid.appendChild(createActionButton(currentItems[i]));
    }

    rootSection.appendChild(rootGrid);
    content.appendChild(rootSection);
    return;
  }

  var titleMap = {
    playerOptions: "플레이어 옵션",
    playerAppearance: "플레이어 외형",
    pedCustomization: "PED 커스터마이징",
    pedCollections: "PED 컬렉션",
    appearanceSavedPeds: "저장된 PED",
    appearanceSpawnPeds: "PED 스폰",
    appearanceAddonPeds: "애드온 PED",
    appearanceMainPeds: "주요 PED",
    appearanceAnimalPeds: "동물",
    appearanceMalePeds: "남성 PED",
    appearanceFemalePeds: "여성 PED",
    appearanceOtherPeds: "기타 PED",
    weaponOptions: "무기 옵션",
    weaponLoadouts: "무기 로드아웃",
    weaponLoadoutManage: "Manage Loadouts",
    weaponCategories: "무기 카테고리",
    weaponAddonWeapons: "애드온 무기",
    weaponParachuteOptions: "낙하산 옵션",
    weaponLoadouts: "무기 로드아웃"
  };

  var metaMap = {
    playerOptions: "무적, 투명화, 이동/상태 관련 옵션을 조정합니다.",
    playerAppearance: "PED 저장, 저장된 PED, PED 스폰, 걷기 스타일을 관리합니다.",
    pedCustomization: "현재 PED의 의상 변경형과 텍스처를 순환합니다.",
    pedCollections: "기본 게임, DLC, 커스텀 컬렉션 의상을 적용합니다.",
    appearanceSavedPeds: "저장된 PED를 불러오거나 삭제합니다.",
    appearanceSpawnPeds: "모델명 입력 또는 분류로 PED를 스폰합니다.",
    appearanceAddonPeds: "서버 애드온 PED 목록입니다.",
    appearanceMainPeds: "주요 PED 목록입니다.",
    appearanceAnimalPeds: "동물 PED 목록입니다.",
    appearanceMalePeds: "남성 PED 목록입니다.",
    appearanceFemalePeds: "여성 PED 목록입니다.",
    appearanceOtherPeds: "기타 PED 목록입니다.",
    weaponOptions: "기본 무기 관리, 무기 카테고리, 애드온 무기, 낙하산 옵션을 다룹니다.",
    weaponLoadouts: "저장한 무기 로드아웃 기능을 이곳에 연결할 예정입니다.",
    weaponCategories: "무기 그룹별로 선택해서 바로 지급할 수 있습니다.",
    weaponAddonWeapons: "서버 애드온 무기 목록입니다.",
    weaponParachuteOptions: "낙하산 관련 옵션을 관리합니다.",
    weaponLoadouts: "로드아웃 저장과 리스폰 복원 설정을 관리합니다.",
    weaponLoadoutManage: "저장된 무기 로드아웃 목록을 관리합니다."
  };

  var section = createBackRow(titleMap[playerCurrentSection] || "플레이어 관련 옵션", metaMap[playerCurrentSection] || "");
  var grid = document.createElement("div");
  grid.className = "button-grid";

  for (var j = 0; j < currentItems.length; j++) {
    grid.appendChild(createActionButton(currentItems[j]));
  }

  section.appendChild(grid);
  content.appendChild(section);
}

function renderVehicle() {
  var section = createBackRow(
    "차량 관련 옵션",
    latestState.personalVehicleModel
      ? ("저장됨: " + latestState.personalVehicleModel)
      : (latestState.inVehicle
          ? ("차량 탑승 중 | 엔진 " + onOff(latestState.vehicleEngineOn))
          : "차량 탑승 중일 때만 일부 옵션이 작동합니다.")
  );

  var personalInfo = document.createElement("div");
  personalInfo.className = "section-subgroup";
  personalInfo.appendChild(createInfoRow("저장된 개인 차량", latestState.personalVehicleModel || "없음"));

  var grid = document.createElement("div");
  grid.className = "button-grid";
  currentItems = getItemsForCategory("vehicle");

  for (var i = 0; i < currentItems.length; i++) {
    grid.appendChild(createActionButton(currentItems[i]));
  }

  section.appendChild(personalInfo);
  section.appendChild(grid);
  content.appendChild(section);
}

function renderSpawner() {
  var title = "차량 스포너";
  var meta = "모델명 입력, 클래스 차량 목록";
  if (spawnerCurrentClassId) {
    var selectedClass = getSpawnerClassById(spawnerCurrentClassId);
    title = selectedClass ? selectedClass.title : "차량 클래스";
    meta = selectedClass ? (selectedClass.vehicles.length + "대 차량") : "차량 목록";
  }

  var section = createBackRow(title, meta);
  var stack = document.createElement("div");
  stack.className = "stack";

  currentItems = getItemsForCategory("spawner");

  if (!spawnerCurrentClassId) {
    var rootGrid = document.createElement("div");
    rootGrid.className = "button-grid";

    for (var i = 0; i < currentItems.length; i++) {
      if (currentItems[i].kind === "action") {
        rootGrid.appendChild(createActionButton(currentItems[i]));
      } else if (currentItems[i].kind === "class") {
        rootGrid.appendChild(createActionButton(currentItems[i]));
      }
    }

    if (spawnerCatalogLoading) {
      stack.appendChild(createInfoRow("차량 클래스", "목록을 불러오는 중입니다..."));
    } else if (!spawnerCatalogLoaded || spawnerCatalog.length === 0) {
      stack.appendChild(createInfoRow("차량 클래스", "사용 가능한 차량 클래스가 없습니다."));
    } else {
      stack.appendChild(rootGrid);
    }
  } else {
    var vehicleGrid = document.createElement("div");
    vehicleGrid.className = "button-grid";

    for (var j = 0; j < currentItems.length; j++) {
      vehicleGrid.appendChild(createActionButton(currentItems[j]));
    }

    stack.appendChild(vehicleGrid);
  }

  section.appendChild(stack);
  content.appendChild(section);
}

function render() {
  if (!content || !latestState) {
    return;
  }

  content.innerHTML = "";

  if (currentCategory === "spawner") {
    ensureSpawnerCatalog();
  }

  if (currentCategory === "player" && playerCurrentSection === "weaponAddonWeapons") {
    ensureWeaponAddonCatalog();
  }
  if (currentCategory === "player" && playerCurrentSection && (playerCurrentSection === "weaponLoadouts" || playerCurrentSection === "weaponLoadoutManage" || playerCurrentSection.indexOf("weaponLoadout_") === 0)) {
    ensureWeaponLoadoutCatalog();
  }

  switch (currentCategory) {
    case "dev":
      renderDev();
      break;
    case "player":
      renderPlayer();
      break;
    case "vehicle":
      renderVehicle();
      break;
    case "spawner":
      renderSpawner();
      break;
    default:
      renderRoot();
      break;
  }

  clampSelection();
  currentItemNodes = Array.prototype.slice.call(content.querySelectorAll("[data-item-id]"));
  updateSelectionHighlight();
}

function clampSelection() {
  if (currentItems.length === 0) {
    currentSelection = 0;
    return;
  }

  if (currentSelection < 0) {
    currentSelection = 0;
  }

  if (currentSelection >= currentItems.length) {
    currentSelection = currentItems.length - 1;
  }
}

function updateSelectionHighlight() {
  for (var i = 0; i < currentItemNodes.length; i++) {
    if (i === currentSelection) {
      currentItemNodes[i].classList.add("is-selected");
    } else {
      currentItemNodes[i].classList.remove("is-selected");
    }
  }

  scrollSelectionIntoView();
}

function scrollSelectionIntoView() {
  if (!currentItemNodes || currentSelection < 0 || currentSelection >= currentItemNodes.length) {
    return;
  }

  var selectedNode = currentItemNodes[currentSelection];
  if (!selectedNode || typeof selectedNode.scrollIntoView !== "function") {
    return;
  }

  selectedNode.scrollIntoView({
    block: "nearest",
    inline: "nearest",
    behavior: "auto"
  });
}

function updateDynamicLabels() {
  if (currentCategory === "root" || currentCategory === "spawner") {
    return;
  }

  for (var i = 0; i < currentItems.length && i < currentItemNodes.length; i++) {
    updateActionButtonState(currentItemNodes[i], currentItems[i]);
  }
}

function setSelection(nextIndex) {
  if (currentItems.length === 0) {
    currentSelection = 0;
    return;
  }

  var previousIndex = currentSelection;
  currentSelection = nextIndex;

  if (currentSelection < 0) {
    currentSelection = 0;
  } else if (currentSelection >= currentItems.length) {
    currentSelection = currentItems.length - 1;
  }

  if (previousIndex === currentSelection) {
    return;
  }

  if (currentItemNodes[previousIndex]) {
    currentItemNodes[previousIndex].classList.remove("is-selected");
  }

  if (currentItemNodes[currentSelection]) {
    currentItemNodes[currentSelection].classList.add("is-selected");
  }

  scrollSelectionIntoView();
}

function buildSelectionRows() {
  if (!currentItemNodes || currentItemNodes.length === 0) {
    return [];
  }

  var indexedNodes = [];
  for (var i = 0; i < currentItemNodes.length; i++) {
    var node = currentItemNodes[i];
    if (!node || typeof node.getBoundingClientRect !== "function") {
      continue;
    }

    var rect = node.getBoundingClientRect();
    indexedNodes.push({
      index: i,
      top: rect.top,
      left: rect.left
    });
  }

  indexedNodes.sort(function (a, b) {
    if (Math.abs(a.top - b.top) > 6) {
      return a.top - b.top;
    }

    return a.left - b.left;
  });

  var rows = [];
  for (var j = 0; j < indexedNodes.length; j++) {
    var entry = indexedNodes[j];
    var lastRow = rows.length > 0 ? rows[rows.length - 1] : null;

    if (!lastRow || Math.abs(lastRow.top - entry.top) > 6) {
      rows.push({
        top: entry.top,
        items: [entry]
      });
    } else {
      lastRow.items.push(entry);
    }
  }

  for (var k = 0; k < rows.length; k++) {
    rows[k].items.sort(function (a, b) {
      return a.left - b.left;
    });
  }

  return rows;
}

function findSelectionPosition(rows) {
  for (var rowIndex = 0; rowIndex < rows.length; rowIndex++) {
    for (var columnIndex = 0; columnIndex < rows[rowIndex].items.length; columnIndex++) {
      if (rows[rowIndex].items[columnIndex].index === currentSelection) {
        return {
          row: rowIndex,
          column: columnIndex
        };
      }
    }
  }

  return null;
}

function moveSelectionVertical(delta) {
  if (currentItems.length === 0) {
    return;
  }

  var rows = buildSelectionRows();
  if (rows.length === 0) {
    return;
  }

  var position = findSelectionPosition(rows);
  if (!position) {
    return;
  }

  var targetRow = position.row + delta;
  if (targetRow < 0) {
    targetRow = rows.length - 1;
  } else if (targetRow >= rows.length) {
    targetRow = 0;
  }

  var targetItems = rows[targetRow].items;
  var targetColumn = position.column;
  if (targetColumn >= targetItems.length) {
    targetColumn = targetItems.length - 1;
  }

  setSelection(targetItems[targetColumn].index);
}

function moveSelectionHorizontal(delta) {
  if (currentItems.length === 0) {
    return;
  }

  var rows = buildSelectionRows();
  if (rows.length === 0) {
    return;
  }

  var position = findSelectionPosition(rows);
  if (!position) {
    return;
  }

  var rowItems = rows[position.row].items;
  if (rowItems.length <= 1) {
    return;
  }

  var nextColumn = position.column + delta;
  if (nextColumn < 0 || nextColumn >= rowItems.length) {
    return;
  }

  setSelection(rowItems[nextColumn].index);
}

function enterCategory(category) {
  currentCategory = category;
  currentSelection = 0;
  if (category !== "spawner") {
    spawnerCurrentClassId = null;
  }
  if (category !== "player") {
    playerCurrentSection = null;
  }
  lastStructureSignature = "";
  render();
}

function goBack() {
  if (currentCategory === "spawner" && spawnerCurrentClassId) {
    spawnerCurrentClassId = null;
    currentSelection = 0;
    lastStructureSignature = "";
    render();
    return;
  }

  if (currentCategory === "player" && playerCurrentSection) {
    if (playerCurrentSection.indexOf("weaponLoadout_") === 0) {
      playerCurrentSection = "weaponLoadoutManage";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection.indexOf("appearanceSavedPed::") === 0) {
      playerCurrentSection = "appearanceSavedPeds";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection === "weaponLoadoutManage") {
      playerCurrentSection = "weaponLoadouts";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection.indexOf("weaponCategory") === 0 && playerCurrentSection !== "weaponCategories") {
      playerCurrentSection = "weaponCategories";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection === "weaponCategories" || playerCurrentSection === "weaponAddonWeapons" || playerCurrentSection === "weaponParachuteOptions") {
      playerCurrentSection = "weaponOptions";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection === "appearanceAddonPeds" || playerCurrentSection === "appearanceMainPeds" ||
        playerCurrentSection === "appearanceAnimalPeds" || playerCurrentSection === "appearanceMalePeds" ||
        playerCurrentSection === "appearanceFemalePeds" || playerCurrentSection === "appearanceOtherPeds") {
      playerCurrentSection = "appearanceSpawnPeds";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    if (playerCurrentSection === "appearanceSavedPeds" || playerCurrentSection === "appearanceSpawnPeds" ||
        playerCurrentSection === "pedCustomization" || playerCurrentSection === "pedCollections" ||
        playerCurrentSection.indexOf("pedCollection::") === 0) {
      playerCurrentSection = "playerAppearance";
      currentSelection = 0;
      lastStructureSignature = "";
      render();
      return;
    }

    playerCurrentSection = null;
    currentSelection = 0;
    lastStructureSignature = "";
    render();
    return;
  }

  if (currentCategory !== "root") {
    currentCategory = "root";
    currentSelection = 0;
    spawnerCurrentClassId = null;
    lastStructureSignature = "";
    render();
    return;
  }

  nui("close");
}

function executeCurrentSelection() {
  if (currentItems.length === 0) {
    return;
  }

  var item = currentItems[currentSelection];
  if (!item) {
    return;
  }

  if (currentCategory === "root") {
    enterCategory(item.id);
    return;
  }

  executeItem(item);
}

function executeItem(item) {
  if (item.kind === "class") {
    spawnerCurrentClassId = item.id;
    currentSelection = 0;
    lastStructureSignature = "";
    render();
    return;
  }

  if (item.kind === "playerSection") {
    playerCurrentSection = item.id;
    currentSelection = 0;
    if (item.id === "weaponLoadoutManage") {
      weaponLoadoutCatalogLoaded = false;
      weaponLoadoutCatalogLoading = false;
      weaponLoadoutCatalog = [];
      weaponLoadoutCatalogError = "";
      ensureWeaponLoadoutCatalog();
    }
    if (item.id === "appearanceSavedPeds") {
      savedPedCatalogLoaded = false;
      savedPedCatalogLoading = false;
      savedPedCatalog = [];
      savedPedCatalogError = "";
      ensureSavedPedCatalog();
    }
    if (item.id === "pedCollections") {
      pedCollectionCatalogLoaded = false;
      pedCollectionCatalogLoading = false;
      pedCollectionCatalog = [];
      pedCollectionCatalogError = "";
      ensurePedCollectionCatalog();
    }
    if (item.id === "appearanceAddonPeds" ||
        item.id === "appearanceMainPeds" ||
        item.id === "appearanceAnimalPeds" ||
        item.id === "appearanceMalePeds" ||
        item.id === "appearanceFemalePeds" ||
        item.id === "appearanceOtherPeds") {
      addonPedCatalogError = "";
      if (item.id === "appearanceAddonPeds") {
        if (!addonPedCatalogLoaded || addonPedCatalog.length === 0) {
          addonPedCatalogLoaded = false;
          addonPedCatalogLoading = false;
          addonPedCatalog = [];
          ensureAddonPedCatalog();
        }
      } else if (!pedSpawnCatalog[item.id] || pedSpawnCatalog[item.id].length === 0) {
        addonPedCatalogLoaded = false;
        addonPedCatalogLoading = false;
        ensureAddonPedCatalog();
      }
    }
    lastStructureSignature = "";
    render();
    return;
  }

  if (item.kind === "placeholder") {
    return;
  }

  if (item.extra && item.extra.getModelFromInput) {
    openInputPrompt({
      title: "차량 모델명 입력",
      description: "스폰할 차량 모델명을 입력해주세요.",
      placeholder: "예: sultanrs, police3, adder",
      value: vehicleModelInput,
      onConfirm: function (value) {
        vehicleModelInput = value;
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { model: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getEntityModelFromInput) {
    openInputPrompt({
      title: "엔티티 모델명 입력",
      description: "생성할 엔티티 모델명을 입력해주세요.",
      placeholder: "예: prop_beachball_02, a_m_m_bevhills_01, sultanrs",
      value: entityModelInput,
      onConfirm: function (value) {
        entityModelInput = value;
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, model: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getWeaponNameFromInput) {
    openInputPrompt({
      title: "무기 이름 입력",
      description: "지급할 무기 이름을 입력해주세요.",
      placeholder: "예: weapon_carbinerifle 또는 carbinerifle",
      value: "",
      onConfirm: function (value) {
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, model: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getAmmoCountFromInput) {
    openInputPrompt({
      title: "탄약 수량 입력",
      description: "모든 무기의 탄약 수량을 입력해주세요.",
      placeholder: "예: 250",
      value: "",
      onConfirm: function (value) {
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, ammoCount: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getLoadoutNameFromInput) {
    openInputPrompt({
      title: "로드아웃 이름 입력",
      description: "저장할 무기 로드아웃 이름을 입력해주세요.",
      placeholder: "예: 기본 전투 세트",
      value: "",
      onConfirm: function (value) {
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, name: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getPedSaveNameFromInput) {
    openInputPrompt({
      title: "PED 저장 이름 입력",
      description: "저장할 PED 이름을 입력해주세요.",
      placeholder: "e.g. spawn a car",
      value: "",
      onConfirm: function (value) {
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, name: value });
      }
    });
    return;
  }

  if (item.extra && item.extra.getPedModelFromInput) {
    openInputPrompt({
      title: "PED 모델명 입력",
      description: "스폰할 PED 모델명을 입력해주세요.",
      placeholder: "예: a_m_m_bevhills_01",
      value: "",
      onConfirm: function (value) {
        if (!value) {
          return;
        }
        executeItemWithPayload(item, { action: item.action, model: value });
      }
    });
    return;
  }

  var payload = {};
  if (item.action) {
    payload.action = item.action;
  }

  if (item.extra) {
    for (var key in item.extra) {
      if (Object.prototype.hasOwnProperty.call(item.extra, key) &&
          key !== "getModelFromInput" &&
          key !== "getEntityModelFromInput") {
        payload[key] = item.extra[key];
      }
    }
  }

  executeItemWithPayload(item, payload);
}

function executeItemWithPayload(item, payload) {
  if (item && item.endpoint === "weaponLoadoutAction") {
    weaponLoadoutCatalogLoaded = false;
    weaponLoadoutCatalogLoading = false;
    weaponLoadoutCatalog = [];
  }
  if (item && item.endpoint === "playerAction" && (item.action === "saveCurrentPed" || item.action === "deleteSavedPed")) {
    savedPedCatalogLoaded = false;
    savedPedCatalogLoading = false;
    savedPedCatalog = [];
    savedPedCatalogError = "";
  }
  nui(item.endpoint, payload || {}).then(function (response) {
    if (response && response.open !== undefined) {
      applyState(response);
      lastStructureSignature = "";
      if (item && item.endpoint === "weaponLoadoutAction" && item.action === "deleteLoadout" &&
          currentCategory === "player" && playerCurrentSection.indexOf("weaponLoadout_") === 0) {
        playerCurrentSection = "weaponLoadoutManage";
        currentSelection = 0;
        ensureWeaponLoadoutCatalog();
      }
      if (item && item.endpoint === "playerAction" && item.action === "deleteSavedPed" &&
          currentCategory === "player" && playerCurrentSection.indexOf("appearanceSavedPed::") === 0) {
        playerCurrentSection = "appearanceSavedPeds";
        currentSelection = 0;
        ensureSavedPedCatalog();
      }
      render();
    } else if (response && typeof response === "object") {
      latestState = response;
      renderCoordinateOverlay();
      lastStructureSignature = "";
      if (item && item.endpoint === "weaponLoadoutAction" && item.action === "deleteLoadout" &&
          currentCategory === "player" && playerCurrentSection.indexOf("weaponLoadout_") === 0) {
        playerCurrentSection = "weaponLoadoutManage";
        currentSelection = 0;
        ensureWeaponLoadoutCatalog();
      }
      if (item && item.endpoint === "playerAction" && item.action === "deleteSavedPed" &&
          currentCategory === "player" && playerCurrentSection.indexOf("appearanceSavedPed::") === 0) {
        playerCurrentSection = "appearanceSavedPeds";
        currentSelection = 0;
        ensureSavedPedCatalog();
      }
      render();
    }
  });
}

function handleMessageEvent(event) {
  if (!event || !event.data) {
    return;
  }

  var payload = event.data;
  if (typeof payload === "string") {
    try {
      payload = JSON.parse(payload);
    } catch (e) {
      return;
    }
  }

  if (payload && payload.type === "pedCatalogResult") {
    var items = parseCatalogItemsJson(payload.itemsJson);
    addonPedCatalogLoading = false;
    addonPedCatalogLoaded = true;
    addonPedCatalogError = "";

    if (payload.section === "appearanceMainPeds") {
      pedSpawnCatalog.appearanceMainPeds = items;
    } else if (payload.section === "appearanceAnimalPeds") {
      pedSpawnCatalog.appearanceAnimalPeds = items;
    } else if (payload.section === "appearanceMalePeds") {
      pedSpawnCatalog.appearanceMalePeds = items;
    } else if (payload.section === "appearanceFemalePeds") {
      pedSpawnCatalog.appearanceFemalePeds = items;
    } else if (payload.section === "appearanceOtherPeds") {
      pedSpawnCatalog.appearanceOtherPeds = items;
    } else if (payload.section === "appearanceAddonPeds") {
      addonPedCatalog = items;
    }

    lastStructureSignature = "";
    render();
    return;
  }

  if (payload && payload.type === "vehicleCatalogResult") {
    var catalog = null;
    try {
      catalog = payload.catalogJson ? JSON.parse(payload.catalogJson) : null;
    } catch (error) {
      catalog = null;
    }

    var normalized = normalizeVehicleCatalog(catalog);
    spawnerCatalogLoading = false;
    spawnerCatalogLoaded = true;
    spawnerCatalog = normalized.classes;

    lastStructureSignature = "";
    render();
    return;
  }

  applyState(payload);
}

window.addEventListener("message", handleMessageEvent);
window.addEventListener("keydown", function (event) {
  if (activeInputPrompt) {
    if (event.key === "Enter") {
      event.preventDefault();
      confirmInputPrompt();
      return;
    }

    if (event.key === "Escape" || event.key === "Backspace" || event.code === "Backslash" || event.key === "\\") {
      if (event.key !== "Backspace" || !inputModalField || inputModalField.value.length === 0) {
        event.preventDefault();
        closeInputPrompt();
        return;
      }
    }
    return;
  }

  if (app.classList.contains("hidden")) {
    return;
  }

  if (event.key === "ArrowUp") {
    event.preventDefault();
    moveSelectionVertical(-1);
    return;
  }
  if (event.key === "ArrowDown") {
    event.preventDefault();
    moveSelectionVertical(1);
    return;
  }
  if (event.key === "ArrowLeft") {
    event.preventDefault();
    moveSelectionHorizontal(-1);
    return;
  }
  if (event.key === "ArrowRight") {
    event.preventDefault();
    moveSelectionHorizontal(1);
    return;
  }
  if (event.key === "Enter") {
    event.preventDefault();
    executeCurrentSelection();
    return;
  }
  if (event.key === "Backspace") {
    event.preventDefault();
    goBack();
    return;
  }
  if (event.code === "Backslash" || event.key === "\\") {
    event.preventDefault();
    goBack();
    return;
  }
  if (event.key === "Escape") {
    event.preventDefault();
    nui("close");
  }
});

if (inputModalConfirm) {
  inputModalConfirm.addEventListener("click", function () {
    confirmInputPrompt();
  });
}

nui("ready").then(function (payload) {
  applyState(payload);
});

function schedulePoll() {
  var interval = 500;

  if (latestState && latestState.hasAccess === false) {
    interval = 60000;
  } else if (latestState && latestState.showCoordinates) {
    interval = 160;
  }

  if (pollTimer) {
    clearTimeout(pollTimer);
  }

  pollTimer = setTimeout(function () {
    pollState();
  }, interval);
}

function pollState() {
  nui("state").then(function (payload) {
    applyState(payload);
    schedulePoll();
  });
}

schedulePoll();
