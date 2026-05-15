using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace hbAdminLiteServer
{
    public partial class AdminLiteServer : BaseScript
    {
        private void RequestPlayerList([FromSource] Player source)
        {
            if (!HasAdminAccess(source, HbPermission.HBOnlinePlayers))
            {
                source.TriggerEvent("hb_adminlite:receivePlayerList", "[]");
                return;
            }

            var list = new List<PlayerSummary>();
            foreach (var player in Players)
            {
                if (!int.TryParse(player.Handle, out var serverId))
                {
                    continue;
                }

                list.Add(new PlayerSummary
                {
                    ServerId = serverId,
                    Name = player.Name,
                });
            }

            list = list.OrderBy(p => p.Name).ToList();

            source.TriggerEvent("hb_adminlite:receivePlayerList", JsonConvert.SerializeObject(list));
        }

        private async Task RequestPedCatalog([FromSource] Player source, string section)
        {
            if (!HasAdminAccess(source, HbPermission.HBPlayerAppearance))
            {
                await SendPedCatalogInChunks(source, section ?? "", "[]");
                return;
            }

            string raw = null;
            var resourceName = GetCurrentResourceName();

            try
            {
                raw = LoadResourceFile(resourceName, "ped_catalog.json");
                if (string.IsNullOrWhiteSpace(raw))
                {
                    raw = LoadResourceFile(resourceName, "config/ped_catalog.json");
                }
            }
            catch
            {
                raw = null;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                await SendPedCatalogInChunks(source, section ?? "", "[]");
                return;
            }

            var key = section switch
            {
                "appearanceMainPeds" => "mainPeds",
                "appearanceAnimalPeds" => "animalPeds",
                "appearanceMalePeds" => "malePeds",
                "appearanceFemalePeds" => "femalePeds",
                "appearanceOtherPeds" => "otherPeds",
                _ => ""
            };

            if (string.IsNullOrWhiteSpace(key))
            {
                await SendPedCatalogInChunks(source, section ?? "", "[]");
                return;
            }

            var itemsJson = ExtractSectionJson(raw, key);
            await SendPedCatalogInChunks(source, section ?? "", itemsJson);
        }

        private async Task RequestVehicleCatalog([FromSource] Player source)
        {
            if (!HasAdminAccess(source, HbPermission.HBVehicleSpawner))
            {
                await SendVehicleCatalogInChunks(source, "{\"classes\":[]}");
                return;
            }

            string raw = null;
            var resourceName = GetCurrentResourceName();

            try
            {
                raw = LoadResourceFile(resourceName, "vehicle_catalog.json");
                if (string.IsNullOrWhiteSpace(raw))
                {
                    raw = LoadResourceFile(resourceName, "config/vehicle_catalog.json");
                }
            }
            catch
            {
                raw = null;
            }

            await SendVehicleCatalogInChunks(source, string.IsNullOrWhiteSpace(raw) ? "{\"classes\":[]}" : raw);
        }

        private static async Task SendPedCatalogInChunks(Player source, string section, string itemsJson)
        {
            var payload = string.IsNullOrWhiteSpace(itemsJson) ? "[]" : itemsJson;
            const int chunkSize = 1500;
            var totalChunks = Math.Max(1, (int)Math.Ceiling((double)payload.Length / chunkSize));

            source.TriggerEvent("hb_adminlite:receivePedCatalogStart", section ?? "", totalChunks);
            await BaseScript.Delay(0);

            for (var index = 0; index < totalChunks; index++)
            {
                var start = index * chunkSize;
                var length = Math.Min(chunkSize, payload.Length - start);
                var chunk = payload.Substring(start, length);
                source.TriggerEvent("hb_adminlite:receivePedCatalogChunk", section ?? "", index, chunk);
                await BaseScript.Delay(0);
            }

            source.TriggerEvent("hb_adminlite:receivePedCatalogEnd", section ?? "");
        }

        private static async Task SendVehicleCatalogInChunks(Player source, string catalogJson)
        {
            var payload = string.IsNullOrWhiteSpace(catalogJson) ? "{\"classes\":[]}" : catalogJson;
            const int chunkSize = 1500;
            var totalChunks = Math.Max(1, (int)Math.Ceiling((double)payload.Length / chunkSize));

            source.TriggerEvent("hb_adminlite:receiveVehicleCatalogStart", totalChunks);
            await BaseScript.Delay(0);

            for (var index = 0; index < totalChunks; index++)
            {
                var start = index * chunkSize;
                var length = Math.Min(chunkSize, payload.Length - start);
                var chunk = payload.Substring(start, length);
                source.TriggerEvent("hb_adminlite:receiveVehicleCatalogChunk", index, chunk);
                await BaseScript.Delay(0);
            }

            source.TriggerEvent("hb_adminlite:receiveVehicleCatalogEnd");
        }

        private static string ExtractSectionJson(string raw, string key)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(key))
            {
                return "[]";
            }

            try
            {
                var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\\[(?<body>[\\s\\S]*?)\\]\\s*(,|\\})";
                var match = Regex.Match(raw, pattern, RegexOptions.Singleline);
                if (!match.Success)
                {
                    return "[]";
                }

                return "[" + match.Groups["body"].Value + "]";
            }
            catch
            {
                return "[]";
            }
        }

    }
}
