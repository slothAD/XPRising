using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using VampireCommandFramework;

namespace RPGMods.Commands {
    public static class Kit {
        private static List<ItemKit> kits;

        [Command("kit", usage:"kit <Name>", description:"Gives you a previously specified set of items.")]
        public static void Initialize(ChatCommandContext ctx, string name) {

            try {
                ItemKit kit = kits.First(x => x.Name.ToLower() == name.ToLower());
                foreach (var guid in kit.PrefabGUIDs) {
                    Helper.AddItemToInventory(ctx, new PrefabGUID(guid.Key), guid.Value);
                }
                ctx.Reply($"You got the kit: <color=#ffff00>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
            } catch {
                ctx.Reply($"Kit doesn't exist.");
                return;
            }
        }

        public static void LoadKits() {
            if (!File.Exists("BepInEx/config/RPGMods/kits.json")) {
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/kits.json");
            try {
                kits = JsonSerializer.Deserialize<List<ItemKit>>(json);
                Plugin.Logger.LogWarning("Kits DB Populated.");
            } catch {
                kits = new List<ItemKit>();
                Plugin.Logger.LogWarning("Kits DB Created.");
            }
        }

        public static void SaveKits() {
            var options = new JsonSerializerOptions() {
                WriteIndented = true,
                IncludeFields = true
            };
            File.WriteAllText("BepInEx/config/RPGMods/kits.json", JsonSerializer.Serialize(kits, options));
        }
    }
}
