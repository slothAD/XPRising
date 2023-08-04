using ProjectM;
using OpenRPG.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class Kit
    {
        private static List<ItemKit> kits;

        [Command("kit", shortHand: "kit", adminOnly: false, usage: "<Name>", description: "Gives you a previously specified set of items.")]
        public static void Initialize(ChatCommandContext ctx, string name)
        {
            try
            {
                ItemKit kit = kits.First(x => x.Name.ToLower() == name.ToLower());
                foreach (var guid in kit.PrefabGUIDs)
                {
                    Helper.AddItemToInventory(ctx, new PrefabGUID(guid.Key), guid.Value);
                }
                ctx.Reply($"You got the kit: <color=#ffff00>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
            }
            catch
            {
               throw  ctx.Error($"Kit doesn't exist.");
            }
        }

        public static void LoadKits()
        {

            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.KitsJson))
            {
                var stream = File.Create(Plugin.KitsJson);
                stream.Dispose();
            }
            string json = File.ReadAllText(Plugin.KitsJson);
            try
            {
                kits = JsonSerializer.Deserialize<List<ItemKit>>(json);
                Plugin.Logger.LogWarning("Kits DB Populated.");
            }
            catch
            {
                kits = new List<ItemKit>();
                Plugin.Logger.LogWarning("Kits DB Created.");
            }
        }

        public static void SaveKits()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            File.WriteAllText(Plugin.KitsJson, JsonSerializer.Serialize(kits, options));
        }
    }
}
