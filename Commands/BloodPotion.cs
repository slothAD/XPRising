using ProjectM;
using OpenRPG.Utils;
using System;
using Unity.Entities;
using VampireCommandFramework;
using Unity.Collections;
using Bloodstone.API;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class BloodPotion
    {
        [Command("bloodpotion", usage: "<Type> [<Quality>]", description: "Creates a Potion with specified Blood Type, Quality and Value", adminOnly: false)]
        public static void BloodPotionComand(ChatCommandContext ctx, string typeName, int qualityInt)
        {

            //Helper.BloodType type = Helper.BloodType.Warrior;
            //float quality = 100;

            Helper.BloodType type = Helper.GetBloodTypeFromName(typeName);

            float quality = qualityInt;
            if (qualityInt < 0) quality = 0;
            if (qualityInt > 100) quality = 100;

            Entity entity = Helper.AddItemToInventory(ctx, new PrefabGUID(828432508), 1);
            var blood = VWorld.Server.EntityManager.GetComponentData<StoredBlood>(entity);
            blood.BloodQuality = quality;
            blood.BloodType = new PrefabGUID((int)type);
            VWorld.Server.EntityManager.SetComponentData(entity, blood);

            ctx.Reply($"Got Blood Potion Type <color=#ffff00>{type}</color> with <color=#ffff00>{quality}</color>% quality");

        }
    }
}
