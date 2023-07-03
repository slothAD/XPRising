
﻿using HarmonyLib;
﻿using System;
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
            {
                if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": beginning Death Tracking");
                NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
                if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Death events converted successfully, length is " + deathEvents.Length);
                foreach (DeathEvent ev in deathEvents) {
                    if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Death Event occured");
                    //-- Just track whatever died...
                    if (WorldDynamicsSystem.isFactionDynamic) WorldDynamicsSystem.MobKillMonitor(ev.Died);

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
                        if (ExperienceSystem.isEXPActive) ExperienceSystem.EXPMonitor(killer, ev.Died);
                        if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerUpdateHeat(killer, ev.Died);
                        if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.UpdateMastery(killer, ev.Died);
                        if (Bloodlines.areBloodlinesEnabled) Bloodlines.UpdateBloodline(killer, ev.Died);
                        if (PvPSystem.isHonorSystemEnabled) PvPSystem.MobKillMonitor(killer, ev.Died);

                    }

                    //-- ----------------------
                    //-- Player Killed Tracking

                    //-- Auto Respawn & HunterHunted System Begin
                    if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died)) {
                        if (Helper.deathLogging) Plugin.Logger.LogInfo(DateTime.Now + ": the dead person is a player, running xp loss and heat dumping");
                        PlayerCharacter player = __instance.EntityManager.GetComponentData<PlayerCharacter>(ev.Died);
                        Entity userEntity = player.UserEntity;
                        User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                        ulong SteamID = user.PlatformId;
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
                        //-- Reset the heat level of the player
                        if (HunterHuntedSystem.isActive) {
                            if (ExperienceSystem.xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": resetting heat");
                            Cache.bandit_heatlevel[SteamID] = 0;
                            Cache.heatlevel[SteamID] = 0;
                        }
                    }
                    //-- ----------------------------------------
                }
            }
        }
    }
}