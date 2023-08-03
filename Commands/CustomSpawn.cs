using ProjectM;
using OpenRPG.Utils;
using Unity.Transforms;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class CustomSpawnNPC
    {
        [Command("customspawn", usage: "<Prefab Name> [<BloodType> <BloodQuality> <BloodConsumeable(1/0)> <Duration>]", description: "Spawns a modified NPC at your current position.")]
        public static void CustomSpawnNPCCommand(ChatCommandContext ctx, string namePrefab, string bloodType, int qualityInt,  bool bloodconsume, int durationInt)
        {

            PrefabGUID type = new PrefabGUID((int)Helper.BloodType.Frailed);
            float quality = qualityInt;
            float duration = durationInt;

            if (quality < 0) quality = 0;
            if (quality > 100) quality = 100;

            type = new PrefabGUID((int)Helper.GetBloodTypeFromName(bloodType));

    
            var pos = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            if (!Helper.SpawnNPCIdentify(out var npc_id, namePrefab, pos, 1, 2, duration))
            {
                ctx.Reply($"Could not find specified unit: {namePrefab}");
                return;
            }

            var Options = new SpawnOptions(true, type, quality, bloodconsume, false, default, true);
            var NPCData = Cache.spawnNPC_Listen[npc_id];
            NPCData.Options = Options;
            if (NPCData.EntityIndex != 0) NPCData.Process = true;

            Cache.spawnNPC_Listen[npc_id] = NPCData;

            ctx.Reply($"Spawning CustomNPC {namePrefab} at your position with LifeTime of {duration}s");
            
        }
    }
}