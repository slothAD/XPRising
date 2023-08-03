using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using OpenRPG.Systems;

namespace OpenRPG.Hooks
{
    //-- Can Intercept Entity Spawn Here! Nice!
    //var GUID = Helper.GetPrefabGUID(entity);
    //var Name = Helper.GetPrefabName(GUID);
    //Plugin.Logger.LogWarning($"{entity} - {Name}");
    //foreach (var t in __instance.EntityManager.GetComponentTypes(entity))
    //{
    //    Plugin.Logger.LogWarning($"--{t}");
    //}

    [HarmonyPatch(typeof(EntityMetadataSystem), nameof(EntityMetadataSystem.OnUpdate))]
    public class EntityMetadataSystem_Patch
    {
        public static void Prefix(EntityMetadataSystem __instance)
        {
            //-- Spawned mobs appear here!
            if (!WorldDynamicsSystem.isFactionDynamic) return;

            var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var entity in entities)
            {
                if (__instance.EntityManager.HasComponent<Movement>(entity) && __instance.EntityManager.HasComponent<FactionReference>(entity))
                {
                    WorldDynamicsSystem.MobReceiver(entity);
                }
            }

            var entities1 = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var entity in entities1)
            {
                if (__instance.EntityManager.HasComponent<Movement>(entity) && __instance.EntityManager.HasComponent<FactionReference>(entity))
                {
                    WorldDynamicsSystem.MobReceiver(entity);
                }
            }
            
        }
    }
}