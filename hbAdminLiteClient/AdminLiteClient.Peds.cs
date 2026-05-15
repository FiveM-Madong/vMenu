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
        private void SaveCurrentPed(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowFeed("PED 저장 이름을 입력해주세요.");
                return;
            }

            var trimmedName = name.Trim().Replace("|", " ").Replace("\r", " ").Replace("\n", " ");
            var key = $"hb_adminlite_saved_ped_{trimmedName}";
            var modelHash = (uint)GetEntityModel(Game.PlayerPed.Handle);
            SetResourceKvp(key, modelHash.ToString(CultureInfo.InvariantCulture));
            AddSavedPedToIndex(key, trimmedName);
            ShowFeed($"PED {trimmedName} 저장 완료");
        }

        private void SpawnPedByName(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                ShowFeed("PED 모델명을 입력해주세요.");
                return;
            }

            var hash = (uint)GetHashKey(modelName.Trim());
            if (!IsModelInCdimage(hash) || !IsModelAPed(hash))
            {
                ShowFeed("유효한 PED 모델이 아닙니다.");
                return;
            }

            _ = SetPlayerPedModelAsync(hash);
        }

        private void SpawnSavedPed(string keyOrName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyOrName))
                {
                    ShowFeed("저장된 PED를 찾지 못했습니다.");
                    return;
                }

                var key = ResolveExistingSavedPedKey(keyOrName);
                var raw = string.IsNullOrWhiteSpace(key) ? null : GetResourceKvpString(key);
                if (string.IsNullOrWhiteSpace(raw))
                {
                    ShowFeed("저장된 PED를 찾지 못했습니다.");
                    return;
                }

                var modelHash = ExtractSavedPedModelHash(raw);
                if (modelHash == 0)
                {
                    ShowFeed("저장된 PED 데이터가 올바르지 않습니다.");
                    return;
                }

                _ = SetPlayerPedModelAsync(modelHash);
            }
            catch (Exception ex)
            {
                ShowFeed($"저장된 PED 불러오기 실패: {ex.Message}");
            }
        }

        private static uint ExtractSavedPedModelHash(string raw)
        {
            if (uint.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var legacyModelHash))
            {
                return legacyModelHash;
            }

            var match = Regex.Match(raw ?? "", "\"ModelHash\"\\s*:\\s*(\\d+)", RegexOptions.CultureInvariant);
            if (match.Success && uint.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var jsonModelHash))
            {
                return jsonModelHash;
            }

            return 0;
        }

        private void DeleteSavedPed(string keyOrName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyOrName))
                {
                    ShowFeed("저장된 PED를 찾지 못했습니다.");
                    return;
                }

                var key = ResolveExistingSavedPedKey(keyOrName) ?? keyOrName.Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    ShowFeed("저장된 PED를 찾지 못했습니다.");
                    return;
                }

                var displayName = GetSavedPedDisplayName(key);
                DeleteResourceKvp(key);
                RemoveSavedPedFromIndex(key);
                ShowFeed($"PED {displayName} 삭제 완료");
            }
            catch (Exception ex)
            {
                ShowFeed($"저장된 PED 삭제 실패: {ex.Message}");
            }
        }

        private void CyclePedComponent(int component)
        {
            component = MathUtil.Clamp(component, 0, 11);
            var ped = Game.PlayerPed.Handle;
            var max = GetNumberOfPedDrawableVariations(ped, component);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 변경형이 없습니다.");
                return;
            }

            var next = (GetPedDrawableVariation(ped, component) + 1) % max;
            SetPedComponentVariation(ped, component, next, 0, 0);
        }

        private void CyclePedComponentTexture(int component)
        {
            component = MathUtil.Clamp(component, 0, 11);
            var ped = Game.PlayerPed.Handle;
            var drawable = GetPedDrawableVariation(ped, component);
            var max = GetNumberOfPedTextureVariations(ped, component, drawable);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 텍스처가 없습니다.");
                return;
            }

            var current = Math.Max(0, GetPedTextureVariation(ped, component));
            SetPedComponentVariation(ped, component, drawable, (current + 1) % max, 0);
        }

        private void CyclePedProp(int prop)
        {
            var ped = Game.PlayerPed.Handle;
            var max = GetNumberOfPedPropDrawableVariations(ped, prop);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 소품이 없습니다.");
                return;
            }

            var current = GetPedPropIndex(ped, prop) + 1;
            var next = (current + 1) % (max + 1);
            if (next == 0)
            {
                ClearPedProp(ped, prop);
                return;
            }

            SetPedPropIndex(ped, prop, next - 1, 0, true);
        }

        private void CyclePedPropTexture(int prop)
        {
            var ped = Game.PlayerPed.Handle;
            var drawable = GetPedPropIndex(ped, prop);
            if (drawable < 0)
            {
                ShowFeed("먼저 소품을 선택하세요.");
                return;
            }

            var max = GetNumberOfPedPropTextureVariations(ped, prop, drawable);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 텍스처가 없습니다.");
                return;
            }

            var current = Math.Max(0, GetPedPropTextureIndex(ped, prop));
            SetPedPropIndex(ped, prop, drawable, (current + 1) % max, true);
        }

        private void CyclePedCollectionComponent(int component, string collection)
        {
            component = MathUtil.Clamp(component, 0, 11);
            var ped = Game.PlayerPed.Handle;
            var safeCollection = NormalizePedCollection(collection);
            var total = GetNumberOfPedCollectionDrawableVariations(ped, component, safeCollection);
            if (total <= 0)
            {
                ShowFeed("이 컬렉션에는 사용 가능한 변경형이 없습니다.");
                return;
            }

            var currentGlobal = GetPedDrawableVariation(ped, component);
            var currentCollection = GetPedCollectionNameFromDrawable(ped, component, currentGlobal) ?? "";
            var currentLocal = string.Equals(currentCollection, safeCollection, StringComparison.Ordinal)
                ? GetPedCollectionLocalIndexFromDrawable(ped, component, currentGlobal)
                : -1;

            for (var offset = 1; offset <= total; offset++)
            {
                var next = (currentLocal + offset) % total;
                if (IsPedCollectionComponentVariationValid(ped, component, safeCollection, next, 0) &&
                    !IsPedCollectionComponentVariationGen9Exclusive(ped, component, safeCollection, next))
                {
                    SetPedCollectionComponentVariation(ped, component, safeCollection, next, 0, 0);
                    return;
                }
            }

            ShowFeed("선택 가능한 변경형이 없습니다.");
        }

        private void CyclePedCollectionComponentTexture(int component, string collection)
        {
            component = MathUtil.Clamp(component, 0, 11);
            var ped = Game.PlayerPed.Handle;
            var safeCollection = NormalizePedCollection(collection);
            var currentGlobal = GetPedDrawableVariation(ped, component);
            var currentCollection = GetPedCollectionNameFromDrawable(ped, component, currentGlobal) ?? "";
            if (!string.Equals(currentCollection, safeCollection, StringComparison.Ordinal))
            {
                CyclePedCollectionComponent(component, safeCollection);
                return;
            }

            var local = GetPedCollectionLocalIndexFromDrawable(ped, component, currentGlobal);
            var max = GetNumberOfPedCollectionTextureVariations(ped, component, safeCollection, local);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 텍스처가 없습니다.");
                return;
            }

            var current = Math.Max(0, GetPedTextureVariation(ped, component));
            SetPedCollectionComponentVariation(ped, component, safeCollection, local, (current + 1) % max, 0);
        }

        private void CyclePedCollectionProp(int prop, string collection)
        {
            var ped = Game.PlayerPed.Handle;
            var safeCollection = NormalizePedCollection(collection);
            var total = GetNumberOfPedCollectionPropDrawableVariations(ped, prop, safeCollection);
            if (total <= 0)
            {
                ShowFeed("이 컬렉션에는 사용 가능한 소품이 없습니다.");
                return;
            }

            var currentGlobal = GetPedPropIndex(ped, prop);
            var currentCollection = currentGlobal >= 0 ? (GetPedCollectionNameFromProp(ped, prop, currentGlobal) ?? "") : "";
            var currentDisplay = string.Equals(currentCollection, safeCollection, StringComparison.Ordinal)
                ? GetPedCollectionLocalIndexFromProp(ped, prop, currentGlobal) + 1
                : 0;
            var nextDisplay = (currentDisplay + 1) % (total + 1);

            if (nextDisplay == 0)
            {
                ClearPedProp(ped, prop);
                return;
            }

            SetPedCollectionPropIndex(ped, prop, safeCollection, nextDisplay - 1, 0, true);
        }

        private void CyclePedCollectionPropTexture(int prop, string collection)
        {
            var ped = Game.PlayerPed.Handle;
            var safeCollection = NormalizePedCollection(collection);
            var currentGlobal = GetPedPropIndex(ped, prop);
            if (currentGlobal < 0)
            {
                ShowFeed("먼저 소품을 선택하세요.");
                return;
            }

            var currentCollection = GetPedCollectionNameFromProp(ped, prop, currentGlobal) ?? "";
            if (!string.Equals(currentCollection, safeCollection, StringComparison.Ordinal))
            {
                CyclePedCollectionProp(prop, safeCollection);
                return;
            }

            var local = GetPedCollectionLocalIndexFromProp(ped, prop, currentGlobal);
            var max = GetNumberOfPedCollectionPropTextureVariations(ped, prop, safeCollection, local);
            if (max <= 0)
            {
                ShowFeed("사용 가능한 텍스처가 없습니다.");
                return;
            }

            var current = Math.Max(0, GetPedPropTextureIndex(ped, prop));
            SetPedCollectionPropIndex(ped, prop, safeCollection, local, (current + 1) % max, true);
        }

        private static string NormalizePedCollection(string collection)
        {
            return string.Equals(collection, "base", StringComparison.OrdinalIgnoreCase) ? "" : (collection ?? "");
        }

        private static List<PedComponentSummary> BuildPedComponentSummary()
        {
            var result = new List<PedComponentSummary>();
            var ped = Game.PlayerPed.Handle;
            for (var component = 0; component < 12; component++)
            {
                var drawable = GetPedDrawableVariation(ped, component);
                var drawableCount = Math.Max(0, GetNumberOfPedDrawableVariations(ped, component));
                var texture = Math.Max(0, GetPedTextureVariation(ped, component));
                var textureCount = drawable >= 0 ? Math.Max(0, GetNumberOfPedTextureVariations(ped, component, drawable)) : 0;
                result.Add(new PedComponentSummary
                {
                    Component = component,
                    Drawable = drawable,
                    DrawableCount = drawableCount,
                    Texture = texture,
                    TextureCount = textureCount
                });
            }

            return result;
        }

        private static List<PedComponentSummary> SafeBuildPedComponentSummary()
        {
            try
            {
                var playerPed = Game.PlayerPed;
                if (playerPed == null || !playerPed.Exists())
                {
                    return new List<PedComponentSummary>();
                }

                return BuildPedComponentSummary();
            }
            catch
            {
                return new List<PedComponentSummary>();
            }
        }

        private static List<PedPropSummary> BuildPedPropSummary()
        {
            var result = new List<PedPropSummary>();
            var ped = Game.PlayerPed.Handle;
            var props = new[] { 0, 1, 2, 6, 7 };
            foreach (var prop in props)
            {
                var drawable = GetPedPropIndex(ped, prop);
                var drawableCount = Math.Max(0, GetNumberOfPedPropDrawableVariations(ped, prop));
                var texture = drawable >= 0 ? Math.Max(0, GetPedPropTextureIndex(ped, prop)) : 0;
                var textureCount = drawable >= 0 ? Math.Max(0, GetNumberOfPedPropTextureVariations(ped, prop, drawable)) : 0;
                result.Add(new PedPropSummary
                {
                    Prop = prop,
                    Drawable = drawable,
                    DrawableCount = drawableCount,
                    Texture = texture,
                    TextureCount = textureCount,
                    IsClear = drawable < 0
                });
            }

            return result;
        }

        private static List<PedPropSummary> SafeBuildPedPropSummary()
        {
            try
            {
                var playerPed = Game.PlayerPed;
                if (playerPed == null || !playerPed.Exists())
                {
                    return new List<PedPropSummary>();
                }

                return BuildPedPropSummary();
            }
            catch
            {
                return new List<PedPropSummary>();
            }
        }

        private List<SavedPedIndexEntry> GetSavedPedIndex()
        {
            var raw = GetResourceKvpString("hb_adminlite_saved_ped_index");
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<SavedPedIndexEntry>();
            }

            var entries = new List<SavedPedIndexEntry>();
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
                    entries.Add(new SavedPedIndexEntry { Key = key, Name = name });
                }
            }

            return entries;
        }

        private void SaveSavedPedIndex(List<SavedPedIndexEntry> entries)
        {
            var builder = new System.Text.StringBuilder();
            var normalized = new List<SavedPedIndexEntry>();
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

            SetResourceKvp("hb_adminlite_saved_ped_index", builder.ToString());
        }

        private void AddSavedPedToIndex(string key, string name)
        {
            var entries = GetSavedPedIndex();
            SavedPedIndexEntry existing = null;
            foreach (var entry in entries)
            {
                if (entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase) || entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    existing = entry;
                    break;
                }
            }

            if (existing == null)
            {
                entries.Add(new SavedPedIndexEntry { Key = key, Name = name });
            }
            else
            {
                existing.Key = key;
                existing.Name = name;
            }

            SaveSavedPedIndex(entries);
        }

        private void RemoveSavedPedFromIndex(string key)
        {
            var entries = GetSavedPedIndex();
            entries.RemoveAll(entry => entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            SaveSavedPedIndex(entries);
        }

        private string ResolveExistingSavedPedKey(string keyOrName)
        {
            var trimmed = keyOrName.Trim();
            var entries = GetSavedPedIndex();
            foreach (var entry in entries)
            {
                if (entry.Key.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                    entry.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Key;
                }
            }

            var directKey = $"hb_adminlite_saved_ped_{trimmed}";
            return string.IsNullOrWhiteSpace(GetResourceKvpString(directKey)) ? null : directKey;
        }

        private string GetSavedPedDisplayName(string key)
        {
            var entries = GetSavedPedIndex();
            foreach (var entry in entries)
            {
                if (entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Name;
                }
            }

            return key.Replace("hb_adminlite_saved_ped_", "");
        }

        private async Task SetPlayerPedModelAsync(uint modelHash)
        {
            if (!IsModelInCdimage(modelHash) || !IsModelAPed(modelHash))
            {
                ShowFeed("유효한 PED 모델이 아닙니다.");
                return;
            }

            RequestModel(modelHash);
            while (!HasModelLoaded(modelHash))
            {
                await Delay(0);
            }

            SetPlayerModel(Game.Player.Handle, modelHash);
            await Delay(0);
            await RestorePedAfterModelChangeAsync(modelHash);
            ApplyPlayerState();
            SetModelAsNoLongerNeeded(modelHash);
            ShowFeed("PED를 변경했습니다.");
        }

        private async void SetWalkingStyleLocal(string walkingStyle)
        {
            if (IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_f_freemode_01")) || IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01")))
            {
                var isPedMale = IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01"));
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, 1f);
                ClearPedAlternateWalkAnim(Game.PlayerPed.Handle, 1f);
                string animDict = null;
                if (walkingStyle == "Injured") animDict = isPedMale ? "move_m@injured" : "move_f@injured";
                else if (walkingStyle == "Tough Guy") animDict = isPedMale ? "move_m@tough_guy@" : "move_f@tough_guy@";
                else if (walkingStyle == "Femme") animDict = isPedMale ? "move_m@femme@" : "move_f@femme@";
                else if (walkingStyle == "Gangster") animDict = isPedMale ? "move_m@gangster@a" : "move_f@gangster@ng";
                else if (walkingStyle == "Posh") animDict = isPedMale ? "move_m@posh@" : "move_f@posh@";
                else if (walkingStyle == "Sexy") animDict = isPedMale ? null : "move_f@sexy@a";
                else if (walkingStyle == "Business") animDict = isPedMale ? null : "move_f@business@a";
                else if (walkingStyle == "Drunk") animDict = isPedMale ? "move_m@drunk@a" : "move_f@drunk@a";
                else if (walkingStyle == "Hipster") animDict = isPedMale ? "move_m@hipster@a" : null;

                if (animDict != null)
                {
                    if (!HasAnimDictLoaded(animDict))
                    {
                        RequestAnimDict(animDict);
                        while (!HasAnimDictLoaded(animDict))
                        {
                            await Delay(0);
                        }
                    }

                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, animDict, "idle", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, animDict, "walk", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, animDict, "run", 1f, true);
                }
            }
        }


        private object BuildAddonPedCatalog()
        {
            var peds = new List<object>();
            try
            {
                var raw = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(raw);
                if (parsed != null && parsed.TryGetValue("peds", out var addonPeds))
                {
                    for (var i = 0; i < addonPeds.Count; i++)
                    {
                        var model = addonPeds[i];
                        if (string.IsNullOrWhiteSpace(model))
                        {
                            continue;
                        }

                        peds.Add(new
                        {
                            id = model,
                            label = model,
                            model
                        });
                    }
                }
            }
            catch
            {
            }

            return peds;
        }

    }
}
