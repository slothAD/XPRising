using HarmonyLib;
using ProjectM;
using OpenRPG.Systems;
using Unity.Collections;
using Unity.Entities;

namespace OpenRPG.Hooks
{
    [HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
    public class VampireDownedServerEventSystem_Patch
    {
        public static void Postfix(VampireDownedServerEventSystem __instance)
        {
            EntityManager em = __instance.EntityManager;
            var EventsQuery = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

            foreach(var entity in EventsQuery)
            {
                VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, em, out var victim);

                if (ExperienceSystem.isEXPActive && ExperienceSystem.xpLostOnDown)
                {
                    try
                    {
                        // This line currently throws an AOT error. Is there another way to get the source?
                        em.TryGetComponentData(entity, out VampireDownedBuff deathBuff);
                        VampireDownedServerEventSystem.TryFindRootOwner(deathBuff.Source, 1, em, out var killer);
                        if (ExperienceSystem.xpLogging) Plugin.LogInfo("XP Lost on down");
                        ExperienceSystem.deathXPLoss(victim, killer);
                    }
                    catch
                    {
                        // The above code currently results in the following error:
                        //
                        // Attempting to call method 'Unity.Entities.EntityManager::GetComponentData<ProjectM.VampireDownedBuff>'
                        // for which no ahead of time (AOT) code was generated.

                        // Just assume that the killer is the victim
                        if (ExperienceSystem.xpLogging) Plugin.LogInfo("XP Lost on down");
                        ExperienceSystem.deathXPLoss(victim, victim);
                    }
                }
            }
        }
    }
}
