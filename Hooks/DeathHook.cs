
using HarmonyLib;
using System;
using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;

namespace RPGMods.Hooks {
    [HarmonyPatch]
    public class DeathEventListenerSystem_Patch {
        [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
        [HarmonyPostfix]
        public static void Postfix(DeathEventListenerSystem __instance) {
            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": beginning Death Tracking");
            NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Death events converted successfully, length is " + deathEvents.Length);
            foreach (DeathEvent ev in deathEvents) {
                if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Death Event occured");
                //-- Just track whatever died...
                //if (WorldDynamicsSystem.isFactionDynamic) WorldDynamicsSystem.MobKillMonitor(ev.Died);

                //-- Player Creature Kill Tracking
                var killer = ev.Killer;

                // If the entity killing is a minion, switch the killer to the owner of the minion.
                if (__instance.EntityManager.HasComponent<Minion>(killer)) {
                    if (Helper.deathLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Minion killed entity. Getting owner...");
                    if (__instance.EntityManager.TryGetComponentData<EntityOwner>(killer, out var entityOwner)) {
                        killer = entityOwner.Owner;
                        if (Helper.deathLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Owner found, switching killer to owner.");
                    }
                }

                if (__instance.EntityManager.HasComponent<PlayerCharacter>(killer) && __instance.EntityManager.HasComponent<Movement>(ev.Died)) {
                    if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Killer is a player, running xp and heat and the like");

                    if (ExperienceSystem.isEXPActive || HunterHuntedSystem.isActive) {
                        var ignoreKill = false;
                        //-- Check victim is not a summon
                        if (Plugin.Server.EntityManager.HasComponent<Minion>(ev.Died)) {
                            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Minion killed, ignoring");
                            ignoreKill = true;
                        }
            
                        //-- Check victim has a level
                        if (!Plugin.Server.EntityManager.HasComponent<UnitLevel>(ev.Died)) {
                            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Has no level, ignoring");
                            ignoreKill = true;
                        }

                        if (!ignoreKill) {
                            if (Helper.deathLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Killer entity: {killer}");
                            
                            bool isVBlood;
                            if (Plugin.Server.EntityManager.TryGetComponentData(ev.Died, out BloodConsumeSource bloodSource)) {
                                isVBlood = bloodSource.UnitBloodType.Equals(Helper.vBloodType);
                            }
                            else {
                                isVBlood = false;
                            }

                            var useGroup = ExperienceSystem.groupLevelScheme != ExperienceSystem.GroupLevelScheme.None && ExperienceSystem.GroupModifier == 0;
                            var closeAllies = Alliance.GetCloseAllies(
                                ev.Died, killer, ExperienceSystem.GroupMaxDistance, useGroup, Helper.deathLogging);

                            // If you get experience for the kill, you get heat for the kill
                            if (ExperienceSystem.isEXPActive) ExperienceSystem.EXPMonitor(closeAllies, ev.Died, isVBlood);
                            if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerKillEntity(closeAllies, ev.Died, isVBlood);
                        }
                    }

                    if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.UpdateMastery(killer, ev.Died);
                    if (Bloodlines.areBloodlinesEnabled) Bloodlines.UpdateBloodline(killer, ev.Died);
                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.MobKillMonitor(killer, ev.Died);
                }

                //-- Auto Respawn & HunterHunted System Begin
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died)) {
                    if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": the dead person is a player, running xp loss and heat dumping");
                    if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerDied(ev.Died);

                    PlayerCharacter player = __instance.EntityManager.GetComponentData<PlayerCharacter>(ev.Died);
                    Entity userEntity = player.UserEntity;
                    User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                    ulong SteamID = user.PlatformId;
                    //-- ----------------------------------

                    if (ExperienceSystem.isEXPActive && ExperienceSystem.xpLostOnRelease) {
                        if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Ready to check Kill Map");
                        if (Database.killMap.TryGetValue(ev.Died, out Entity trueKiller)) {
                            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Kill Map has killer");
                            ExperienceSystem.deathXPLoss(ev.Died, trueKiller);
                        } else {
                            if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Player not found in the kill map");
                            ExperienceSystem.deathXPLoss(ev.Died, ev.Killer);
                        }
                    }

                    //-- Check for AutoRespawn
                    if (user.IsConnected) {
                        bool isServerWide = Database.autoRespawn.ContainsKey(1);
                        bool doRespawn;
                        if (!isServerWide) {
                            doRespawn = Database.autoRespawn.ContainsKey(SteamID);
                        } else { doRespawn = true; }

                        if (doRespawn) {
                            Utils.RespawnCharacter.Respawn(ev.Died, player, userEntity);
                        }
                    }

                    //-- ----------------------------------------
                }
            }
        }
    }
}