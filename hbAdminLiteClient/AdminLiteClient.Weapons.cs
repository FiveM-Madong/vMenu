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
        private void ExecuteWeaponAction(string action, string weaponName = null, int? ammoCount = null)
        {
            switch (action)
            {
                case "getAllWeapons":
                    GiveAllWeapons();
                    ShowFeed("모든 무기를 지급했습니다.");
                    break;
                case "removeAllWeapons":
                    RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
                    ShowFeed("모든 무기를 제거했습니다.");
                    break;
                case "toggleUnlimitedAmmo":
                    weaponUnlimitedAmmo = !weaponUnlimitedAmmo;
                    if (Game.PlayerPed.Weapons?.Current != null && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                    {
                        Game.PlayerPed.Weapons.Current.InfiniteAmmo = weaponUnlimitedAmmo;
                    }
                    break;
                case "toggleNoReload":
                    weaponNoReload = !weaponNoReload;
                    break;
                case "refillAllAmmo":
                    RefillAllAmmo();
                    ShowFeed("모든 무기의 탄약을 채웠습니다.");
                    break;
                case "setAllAmmo":
                    if (!ammoCount.HasValue || ammoCount.Value < 0)
                    {
                        ShowFeed("탄약 수량이 올바르지 않습니다.");
                        break;
                    }

                    SetAllAmmo(ammoCount.Value);
                    ShowFeed($"모든 무기의 탄약을 {ammoCount.Value}발로 설정했습니다.");
                    break;
                case "spawnWeaponByName":
                    if (string.IsNullOrWhiteSpace(weaponName))
                    {
                        ShowFeed("무기 이름을 입력해주세요.");
                        break;
                    }

                    if (!GiveWeaponByName(weaponName))
                    {
                        ShowFeed("무기 지급에 실패했습니다. 이름을 확인해주세요.");
                    }
                    break;
            }
        }

        private void ExecuteWeaponLoadoutAction(string action, string name = null)
        {
            try
            {
                switch (action)
                {
                    case "toggleLoadoutOnRespawn":
                        weaponLoadoutsSetOnRespawn = !weaponLoadoutsSetOnRespawn;
                        SetResourceKvp("hb_adminlite_weapon_loadout_on_respawn", weaponLoadoutsSetOnRespawn ? "true" : "false");
                        ShowFeed($"리스폰 시 기본 로드아웃 복원 {(weaponLoadoutsSetOnRespawn ? "ON" : "OFF")}");
                        break;
                    case "saveLoadout":
                        SaveCurrentWeaponLoadout(name);
                        break;
                    case "equipLoadout":
                        EquipWeaponLoadout(name);
                        break;
                    case "deleteLoadout":
                        DeleteWeaponLoadout(name);
                        break;
                    case "setDefaultLoadout":
                        SetDefaultWeaponLoadout(name);
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowFeed($"로드아웃 처리 실패: {ex.Message}");
            }
        }

        private Dictionary<string, WeaponLoadoutRecord> GetSavedWeaponLoadouts()
        {
            var result = new Dictionary<string, WeaponLoadoutRecord>();
            var index = GetWeaponLoadoutIndex();

            if (index.Count > 0)
            {
                foreach (var entry in index)
                {
                    var raw = GetResourceKvpString(entry.Key);
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    try
                    {
                        var weapons = JsonConvert.DeserializeObject<List<WeaponLoadoutWeaponRecord>>(raw);
                        result[entry.Key] = new WeaponLoadoutRecord { Name = entry.Name, Weapons = weapons ?? new List<WeaponLoadoutWeaponRecord>() };
                    }
                    catch
                    {
                        result[entry.Key] = new WeaponLoadoutRecord { Name = entry.Name, Weapons = new List<WeaponLoadoutWeaponRecord>() };
                    }
                }

                return result;
            }

            void CollectFromPrefix(string prefix)
            {
                var searchHandle = StartFindKvp(prefix);
                while (true)
                {
                    var key = FindKvp(searchHandle);
                    if (string.IsNullOrEmpty(key))
                    {
                        break;
                    }

                    if (key.EndsWith("_on_respawn", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var rawValue = GetResourceKvpString(key);
                    if (string.IsNullOrWhiteSpace(rawValue))
                    {
                        continue;
                    }

                    try
                    {
                        var weapons = JsonConvert.DeserializeObject<List<WeaponLoadoutWeaponRecord>>(rawValue);
                        result[key] = new WeaponLoadoutRecord { Name = GetWeaponLoadoutDisplayName(key), Weapons = weapons ?? new List<WeaponLoadoutWeaponRecord>() };
                    }
                    catch
                    {
                        result[key] = new WeaponLoadoutRecord { Name = GetWeaponLoadoutDisplayName(key), Weapons = new List<WeaponLoadoutWeaponRecord>() };
                    }
                }

                EndFindKvp(searchHandle);
            }

            CollectFromPrefix("hb_adminlite_weapon_loadout_");

            return result;
        }

        private void SaveCurrentWeaponLoadout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFeed("로드아웃 이름을 입력해주세요.");
                return;
            }

            var trimmedName = name.Trim();
            var key = $"hb_adminlite_weapon_loadout_{trimmedName}";
            if (!SaveWeaponLoadoutCompat(key))
            {
                throw new InvalidOperationException("무기 로드아웃 저장 실패");
            }
            AddWeaponLoadoutToIndex(key, trimmedName);
            ShowFeed($"무기 로드아웃 {trimmedName} 저장 완료");
        }

        private void EquipWeaponLoadout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFeed("로드아웃 이름이 올바르지 않습니다.");
                return;
            }

            var key = ResolveExistingWeaponLoadoutKey(name);
            var raw = string.IsNullOrWhiteSpace(key) ? null : GetResourceKvpString(key);
            if (string.IsNullOrWhiteSpace(raw))
            {
                ShowFeed("저장된 로드아웃을 찾지 못했습니다.");
                return;
            }

            try
            {
                var weaponHashes = ParseWeaponHashesFromLoadout(raw);
                RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
                foreach (var weaponHash in weaponHashes)
                {
                    if (weaponHash == 0 || !IsWeaponValid(weaponHash))
                    {
                        continue;
                    }

                    var ammo = 9999;
                    GetMaxAmmo(Game.PlayerPed.Handle, weaponHash, ref ammo);
                    if (ammo <= 0)
                    {
                        ammo = 9999;
                    }
                    GiveWeaponToPed(Game.PlayerPed.Handle, weaponHash, ammo, false, false);
                }

                ShowFeed($"로드아웃 {GetWeaponLoadoutDisplayName(key)} 장착 완료");
            }
            catch
            {
                ShowFeed("로드아웃 장착에 실패했습니다.");
            }
        }

        private void DeleteWeaponLoadout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFeed("로드아웃 이름이 올바르지 않습니다.");
                return;
            }

            var key = ResolveExistingWeaponLoadoutKey(name);
            if (string.IsNullOrWhiteSpace(key))
            {
                ShowFeed("저장된 로드아웃을 찾지 못했습니다.");
                return;
            }

            DeleteResourceKvp(key);
            RemoveWeaponLoadoutFromIndex(key);
            if ((GetResourceKvpString("hb_adminlite_weapon_default_loadout") ?? "") == key)
            {
                DeleteResourceKvp("hb_adminlite_weapon_default_loadout");
            }

            ShowFeed($"로드아웃 {GetWeaponLoadoutDisplayName(key)} 삭제 완료");
        }

        private void SetDefaultWeaponLoadout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFeed("로드아웃 이름이 올바르지 않습니다.");
                return;
            }

            var key = ResolveExistingWeaponLoadoutKey(name);
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(GetResourceKvpString(key)))
            {
                ShowFeed("저장된 로드아웃을 찾지 못했습니다.");
                return;
            }

            SetResourceKvp("hb_adminlite_weapon_default_loadout", key);
            ShowFeed($"기본 로드아웃을 {GetWeaponLoadoutDisplayName(key)}(으)로 설정했습니다.");
        }

        private void RestoreDefaultWeaponLoadout()
        {
            var defaultKey = GetResourceKvpString("hb_adminlite_weapon_default_loadout");
            if (!string.IsNullOrWhiteSpace(defaultKey))
            {
                EquipWeaponLoadout(defaultKey);
            }
        }

        private static string NormalizeWeaponLoadoutKey(string name)
        {
            var trimmed = name.Trim();
            if (trimmed.StartsWith("hb_adminlite_weapon_loadout_", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return $"hb_adminlite_weapon_loadout_{trimmed}";
        }

        private string ResolveExistingWeaponLoadoutKey(string name)
        {
            var trimmed = name.Trim();
            var entries = GetWeaponLoadoutIndex();
            foreach (var entry in entries)
            {
                if (entry != null &&
                    ((!string.IsNullOrWhiteSpace(entry.Key) && entry.Key.Equals(trimmed, StringComparison.OrdinalIgnoreCase)) ||
                     (!string.IsNullOrWhiteSpace(entry.Name) && entry.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase))))
                {
                    return entry.Key;
                }
            }

            var candidates = new[]
            {
                trimmed,
                $"hb_adminlite_weapon_loadout_{trimmed}"
            };

            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(GetResourceKvpString(candidate)))
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool SaveWeaponLoadoutCompat(string saveName)
        {
            if (string.IsNullOrWhiteSpace(saveName))
            {
                return false;
            }

            var ownedHashes = GetOwnedWeaponHashesNoLinq();
            var json = new System.Text.StringBuilder();
            json.Append("[");
            for (var i = 0; i < ownedHashes.Count; i++)
            {
                var hash = ownedHashes[i];
                if (i > 0)
                {
                    json.Append(",");
                }

                json.Append("{");
                json.Append("\"Hash\":").Append(hash).Append(",");
                json.Append("\"Name\":\"").Append(EscapeJson(GetWeaponDisplayName(hash))).Append("\",");
                json.Append("\"Components\":{},");
                json.Append("\"Perm\":0,");
                json.Append("\"SpawnName\":\"").Append(EscapeJson(GetWeaponSpawnName(hash))).Append("\",");
                json.Append("\"CurrentAmmo\":").Append(GetAmmoInPedWeapon(Game.PlayerPed.Handle, hash)).Append(",");
                json.Append("\"CurrentTint\":").Append(GetPedWeaponTintIndex(Game.PlayerPed.Handle, hash));
                json.Append("}");
            }
            json.Append("]");

            var serialized = json.ToString();
            SetResourceKvp(saveName, serialized);
            return (GetResourceKvpString(saveName) ?? "{}") == serialized;
        }

        private List<uint> ParseWeaponHashesFromLoadout(string raw)
        {
            var result = new List<uint>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return result;
            }

            const string marker = "\"Hash\":";
            var start = 0;
            while (true)
            {
                var markerIndex = raw.IndexOf(marker, start, StringComparison.Ordinal);
                if (markerIndex < 0)
                {
                    break;
                }

                var valueStart = markerIndex + marker.Length;
                var valueEnd = valueStart;
                while (valueEnd < raw.Length && char.IsDigit(raw[valueEnd]))
                {
                    valueEnd++;
                }

                if (valueEnd > valueStart)
                {
                    var slice = raw.Substring(valueStart, valueEnd - valueStart);
                    if (uint.TryParse(slice, out var hash))
                    {
                        result.Add(hash);
                    }
                }

                start = valueEnd;
            }

            return result;
        }

        private List<uint> GetOwnedWeaponHashesNoLinq()
        {
            var owned = new List<uint>();
            foreach (WeaponHash weaponHash in Enum.GetValues(typeof(WeaponHash)))
            {
                var hash = (uint)weaponHash;
                if (hash == 0 || !IsWeaponValid(hash))
                {
                    continue;
                }

                if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                {
                    owned.Add(hash);
                }
            }

            return owned;
        }

        private string GetWeaponSpawnName(uint hash)
        {
            foreach (var entry in ValidWeapons.weaponNames)
            {
                if ((uint)GetHashKey(entry.Key) == hash)
                {
                    return entry.Key;
                }
            }

            return $"weapon_{hash}";
        }

        private string GetWeaponDisplayName(uint hash)
        {
            foreach (var entry in ValidWeapons.weaponNames)
            {
                if ((uint)GetHashKey(entry.Key) == hash)
                {
                    return entry.Value;
                }
            }

            return hash.ToString();
        }

        private Dictionary<string, uint> GetWeaponComponentsForHash(uint hash)
        {
            var components = new Dictionary<string, uint>();
            foreach (var comp in ValidWeapons.GetWeaponComponents())
            {
                var componentHash = (uint)GetHashKey(comp.Key);
                if (DoesWeaponTakeWeaponComponent(hash, componentHash))
                {
                    components[comp.Value] = componentHash;
                }
            }

            return components;
        }

        private static string GetWeaponLoadoutDisplayName(string key)
        {
            return key
                .Replace("hb_adminlite_weapon_loadout_", "");
        }

        private List<WeaponLoadoutIndexEntry> GetWeaponLoadoutIndex()
        {
            var raw = GetResourceKvpString("hb_adminlite_weapon_loadout_index");
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<WeaponLoadoutIndexEntry>();
            }

            var entries = new List<WeaponLoadoutIndexEntry>();
            var lines = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var separator = line.IndexOf('|');
                if (separator <= 0 || separator >= line.Length - 1)
                {
                    continue;
                }

                var key = line.Substring(0, separator).Trim();
                var name = line.Substring(separator + 1).Trim();
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(name))
                {
                    entries.Add(new WeaponLoadoutIndexEntry
                    {
                        Key = key,
                        Name = name
                    });
                }
            }

            return entries;
        }

        private void SaveWeaponLoadoutIndex(List<WeaponLoadoutIndexEntry> entries)
        {
            var normalized = new List<WeaponLoadoutIndexEntry>();
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Name))
                {
                    continue;
                }

                var exists = false;
                foreach (var existing in normalized)
                {
                    if (existing.Key.Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    normalized.Add(entry);
                }
            }

            var builder = new System.Text.StringBuilder();
            for (var i = 0; i < normalized.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(normalized[i].Key);
                builder.Append('|');
                builder.Append(normalized[i].Name.Replace("|", "/").Replace("\r", " ").Replace("\n", " "));
            }

            SetResourceKvp("hb_adminlite_weapon_loadout_index", builder.ToString());
        }

        private void AddWeaponLoadoutToIndex(string key, string name)
        {
            var entries = GetWeaponLoadoutIndex();
            WeaponLoadoutIndexEntry existing = null;
            foreach (var entry in entries)
            {
                if (entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                    entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    existing = entry;
                    break;
                }
            }

            if (existing == null)
            {
                entries.Add(new WeaponLoadoutIndexEntry
                {
                    Key = key,
                    Name = name
                });
            }
            else
            {
                existing.Key = key;
                existing.Name = name;
            }

            SaveWeaponLoadoutIndex(entries);
        }

        private void RemoveWeaponLoadoutFromIndex(string key)
        {
            var entries = GetWeaponLoadoutIndex();
            entries.RemoveAll(entry => entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            SaveWeaponLoadoutIndex(entries);
        }

        private bool TryHandleExtendedWeaponAction(string action)
        {
            switch (action)
            {
                case "togglePrimaryParachute":
                    TogglePrimaryParachute();
                    return true;
                case "enableReserveParachute":
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                    ShowFeed("예비 낙하산을 활성화했습니다.");
                    return true;
                case "toggleAutoEquipParachutes":
                    weaponAutoEquipParachutes = !weaponAutoEquipParachutes;
                    ShowFeed($"자동 낙하산 장착 {(weaponAutoEquipParachutes ? "ON" : "OFF")}");
                    return true;
                case "toggleUnlimitedParachutes":
                    weaponUnlimitedParachutes = !weaponUnlimitedParachutes;
                    if (weaponUnlimitedParachutes)
                    {
                        EnsurePrimaryParachute();
                        SetPlayerHasReserveParachute(Game.Player.Handle);
                    }
                    ShowFeed($"무제한 낙하산 {(weaponUnlimitedParachutes ? "ON" : "OFF")}");
                    return true;
                case "cycleParachuteSmoke":
                    CycleParachuteSmoke();
                    return true;
                case "cyclePrimaryParachuteStyle":
                    parachutePrimaryStyleIndex = (parachutePrimaryStyleIndex + 1) % 8;
                    SetPlayerParachuteTintIndex(Game.Player.Handle, parachutePrimaryStyleIndex);
                    ShowFeed($"주 낙하산 스타일 {parachutePrimaryStyleIndex + 1}번");
                    return true;
                case "cycleReserveParachuteStyle":
                    parachuteReserveStyleIndex = (parachuteReserveStyleIndex + 1) % 8;
                    SetPlayerReserveParachuteTintIndex(Game.Player.Handle, parachuteReserveStyleIndex);
                    ShowFeed($"예비 낙하산 스타일 {parachuteReserveStyleIndex + 1}번");
                    return true;
                default:
                    return false;
            }
        }

        private void TogglePrimaryParachute()
        {
            var parachuteHash = (uint)GetHashKey("gadget_parachute");
            if (HasPedGotWeapon(Game.PlayerPed.Handle, parachuteHash, false))
            {
                RemoveWeaponFromPed(Game.PlayerPed.Handle, parachuteHash);
                ShowFeed("주 낙하산을 제거했습니다.");
            }
            else
            {
                GiveWeaponToPed(Game.PlayerPed.Handle, parachuteHash, 0, false, false);
                ShowFeed("주 낙하산을 지급했습니다.");
            }
        }

        private void EnsurePrimaryParachute()
        {
            var parachuteHash = (uint)GetHashKey("gadget_parachute");
            if (!HasPedGotWeapon(Game.PlayerPed.Handle, parachuteHash, false))
            {
                GiveWeaponToPed(Game.PlayerPed.Handle, parachuteHash, 0, false, false);
            }
        }

        private void CycleParachuteSmoke()
        {
            var colors = new[]
            {
                new[] { 0, 0, 0 },
                new[] { 255, 0, 0 },
                new[] { 255, 165, 0 },
                new[] { 255, 255, 0 },
                new[] { 0, 120, 255 },
                new[] { 0, 0, 0 }
            };

            var names = new[] { "없음", "빨강", "주황", "노랑", "파랑", "검정" };
            parachuteSmokeColorIndex = (parachuteSmokeColorIndex + 1) % colors.Length;
            var color = colors[parachuteSmokeColorIndex];
            SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, parachuteSmokeColorIndex != 0);
            SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, color[0], color[1], color[2]);
            ShowFeed($"낙하산 연막 색상 {names[parachuteSmokeColorIndex]}");
        }

        private object BuildAddonWeaponCatalog()
        {
            var weapons = new List<object>();
            try
            {
                var raw = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(raw);
                if (parsed != null && parsed.TryGetValue("weapons", out var addonWeapons))
                {
                    foreach (var weapon in addonWeapons)
                    {
                        if (string.IsNullOrWhiteSpace(weapon))
                        {
                            continue;
                        }

                        weapons.Add(new
                        {
                            id = weapon,
                            label = GetLabelText(weapon),
                            model = weapon
                        });
                    }
                }
            }
            catch
            {
            }

            return weapons;
        }


    }
}
