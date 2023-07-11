using HarmonyLib;
using ProjectM;
using System.Text.Json;
using System.IO;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using System;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
    public class VampireDownedServerEventSystem_Patch
    {
        public static void Postfix(VampireDownedServerEventSystem __instance)
        {
            //if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

            EntityManager em = __instance.EntityManager;
            var EventsQuery = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

            foreach(var entity in EventsQuery)
            {
                VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, em, out var Victim);

                try {
                    em.TryGetComponentData<VampireDownedBuff>(entity, out VampireDownedBuff deathBuff);

                    //Entity Source = em.GetComponentData<VampireDownedBuff>(entity).Source;
                    Entity Source = deathBuff.Source;
                    VampireDownedServerEventSystem.TryFindRootOwner(Source, 1, em, out var Killer);
                    if (ExperienceSystem.isEXPActive) {
                        if (ExperienceSystem.xpLostOnDown) {
                            if (ExperienceSystem.xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": XP Lost on down, doing it now");
                            ExperienceSystem.deathXPLoss(Victim, Source);
                        } if (ExperienceSystem.xpLostOnRelease) {
                            if (ExperienceSystem.xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": XP Lost on release, adding to the map");
                            Database.killMap.Remove(Victim);
                            Database.killMap.Add(Victim, Source);
                        }
                    }

                    //-- Update PvP Stats & Check
                    if (em.HasComponent<PlayerCharacter>(Killer) && em.HasComponent<PlayerCharacter>(Victim) && !Killer.Equals(Victim)){
                        PvPSystem.Monitor(Killer, Victim);
                        if (PvPSystem.isPunishEnabled) PvPSystem.PunishCheck(Killer, Victim);
                    }
                }
                catch {
                    // The above code currently results in the following error:
                    //
                    // Attempting to call method 'Unity.Entities.EntityManager::GetComponentData<ProjectM.VampireDownedBuff>'
                    // for which no ahead of time (AOT) code was generated.
                    //
                    // As PvP support is not yet complete, ignore this error as it is not required for PvE support.
                }
            }
        }

        public static string specificFile = "KillMap.json";
        public static void saveKillMap() {
            File.WriteAllText(AutoSaveSystem.mainSaveFolder + specificFile, JsonSerializer.Serialize(Database.killMap, Database.JSON_options));
        }

        public static void loadKillMap() {
            Database.killMap = new Dictionary<Entity, Entity>();
            Plugin.Logger.LogWarning("KillData DB Created.");
            /*
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificFile);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificFile);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificFile);
            try {
                Database.killMap = JsonSerializer.Deserialize<Dictionary<Entity, Entity>>(json);
                if (Database.killMap == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificFile);
                    Database.killMap = JsonSerializer.Deserialize<Dictionary<Entity, Entity>>(json);
                }
                Plugin.Logger.LogWarning("KillData DB Populated.");
            } catch {
                Database.killMap = new Dictionary<Entity, Entity>();
                Plugin.Logger.LogWarning("KillData DB Created.");
            }
            */
        }
    }
}
