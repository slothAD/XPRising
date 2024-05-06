using BepInEx.Logging;
using HarmonyLib;
using OpenRPG.Configuration;
using ProjectM;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Collections;
using Unity.Transforms;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Hooks {
    [HarmonyPatch]
    public class DeathEventListenerSystem_Patch {
        [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
        public static void Postfix(DeathEventListenerSystem __instance) {
            NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
            foreach (DeathEvent ev in deathEvents) {
                Plugin.Log(LogSystem.Death, LogLevel.Info, $"Death Event occured for: {ev.Died}");

                //-- Player Creature Kill Tracking
                var killer = ev.Killer;
                Plugin.Log(LogSystem.Death, LogLevel.Info, () => $"[{ev.Source},{ev.Killer},{ev.Died}] => [{Helper.GetPrefabName(ev.Source)},{Helper.GetPrefabName(ev.Killer)},{Helper.GetPrefabName(ev.Died)}]");

                // If the killer is the victim, then we can skip trying to add xp, heat, mastery, bloodline.
                if (!killer.Equals(ev.Died) && ExperienceSystem.EntityProvidesExperience(ev.Died))
                {
                    // If the entity killing is a minion, switch the killer to the owner of the minion.
                    if (__instance.EntityManager.HasComponent<Minion>(killer))
                    {
                        Plugin.Log(LogSystem.Death, LogLevel.Info, $"Minion killed entity. Getting owner...");
                        if (__instance.EntityManager.TryGetComponentData<EntityOwner>(killer, out var entityOwner))
                        {
                            killer = entityOwner.Owner;
                            Plugin.Log(LogSystem.Death, LogLevel.Info, $"Owner found, switching killer to owner.");
                        }
                    }

                    if (__instance.EntityManager.HasComponent<PlayerCharacter>(killer) &&
                        __instance.EntityManager.HasComponent<Movement>(ev.Died))
                    {
                        Plugin.Log(LogSystem.Death, LogLevel.Info,
                            $"Killer ({killer}) is a player, running xp and heat and the like");

                        if (ExperienceSystem.isEXPActive || HunterHuntedSystem.isActive)
                        {
                            var isVBlood =
                                Plugin.Server.EntityManager.TryGetComponentData(ev.Died, out BloodConsumeSource bS) &&
                                bS.UnitBloodType.Equals(Helper.vBloodType);

                            var useGroup =
                                ExperienceSystem.groupLevelScheme != ExperienceSystem.GroupLevelScheme.None &&
                                ExperienceSystem.GroupModifier > 0;

                            var triggerLocation = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ev.Died);
                            var closeAllies = Alliance.GetClosePlayers(
                                triggerLocation.Position, killer, ExperienceSystem.GroupMaxDistance, true, useGroup,
                                LogSystem.Death);

                            // If you get experience for the kill, you get heat for the kill
                            if (ExperienceSystem.isEXPActive) ExperienceSystem.EXPMonitor(closeAllies, ev.Died, isVBlood);
                            if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerKillEntity(closeAllies, ev.Died, isVBlood);
                        }

                        if (WeaponMasterySystem.IsMasteryEnabled) WeaponMasterySystem.UpdateMastery(killer, ev.Died);
                        if (BloodlineSystem.IsBloodlineSystemEnabled) BloodlineSystem.UpdateBloodline(killer, ev.Died);
                    }
                }

                //-- HunterHunted System Begin
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died)) {
                    Plugin.Log(LogSystem.Death, LogLevel.Info, $"the deceased ({ev.Died}) is a player, running xp loss and heat dumping");
                    if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerDied(ev.Died);
                    if (ExperienceSystem.isEXPActive) ExperienceSystem.deathXPLoss(ev.Died, ev.Killer);
                }
            }
            
            // TODO this should integrate iterating into the loop above
            //-- Random Encounters
            if (deathEvents.Length > 0 && RandomEncountersConfig.Enabled.Value && Plugin.isInitialized)
                RandomEncountersSystem.ServerEvents_OnDeath(__instance, deathEvents);
            //-- ----------------------------------------
        }
    }
}