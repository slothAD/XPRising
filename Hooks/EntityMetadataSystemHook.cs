using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using OpenRPG.Systems;

namespace OpenRPG.Hooks
{
    [HarmonyPatch(typeof(EntityMetadataSystem), nameof(EntityMetadataSystem.OnUpdate))]
    public class EntityMetadataSystem_Patch
    {
        public static void Prefix(EntityMetadataSystem __instance)
        {
            //-- Spawned mobs appear here!
            if (!WorldDynamicsSystem.isFactionDynamic) return;

            {
                var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (__instance.EntityManager.HasComponent<Movement>(entity) && __instance.EntityManager.HasComponent<FactionReference>(entity))
                    {
                        WorldDynamicsSystem.MobReceiver(entity);
                    }
                }
            }

            {
                var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (__instance.EntityManager.HasComponent<Movement>(entity) && __instance.EntityManager.HasComponent<FactionReference>(entity))
                    {
                        WorldDynamicsSystem.MobReceiver(entity);
                    }
                }
            }
        }
    }
}