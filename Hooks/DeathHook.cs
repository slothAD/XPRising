using HarmonyLib;
using System;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Commands;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Collections;
using Unity.Entities;
using OpenRPG.Configuration;
using Unity.Transforms;

namespace OpenRPG.Hooks {
    [HarmonyPatch]
    public class DeathEventListenerSystem_Patch {
        [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
        [HarmonyPostfix]
        public static void Postfix(DeathEventListenerSystem __instance) {
            if (Helper.deathLogging) Plugin.LogInfo("beginning Death Tracking");
            NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
            if (Helper.deathLogging) Plugin.LogInfo("Death events converted successfully, length is " + deathEvents.Length);
            foreach (DeathEvent ev in deathEvents) {
                if (Helper.deathLogging) Plugin.LogInfo("Death Event occured");
                //-- Just track whatever died...
                if (WorldDynamicsSystem.isFactionDynamic) WorldDynamicsSystem.MobKillMonitor(ev.Died);

                //-- Player Creature Kill Tracking
                var killer = ev.Killer;

                // If the entity killing is a minion, switch the killer to the owner of the minion.
                if (__instance.EntityManager.HasComponent<Minion>(killer)) {
                    if (Helper.deathLogging) Plugin.LogInfo($"Minion killed entity. Getting owner...");
                    if (__instance.EntityManager.TryGetComponentData<EntityOwner>(killer, out var entityOwner)) {
                        killer = entityOwner.Owner;
                        if (Helper.deathLogging) Plugin.LogInfo($"Owner found, switching killer to owner.");
                    }
                }

                if (__instance.EntityManager.HasComponent<PlayerCharacter>(killer) && __instance.EntityManager.HasComponent<Movement>(ev.Died)) {
                    if (Helper.deathLogging) Plugin.LogInfo("Killer is a player, running xp and heat and the like");
                    
                    if ((ExperienceSystem.isEXPActive || HunterHuntedSystem.isActive) && ExperienceSystem.EntityProvidesExperience(ev.Died)) {
                        var isVBlood = Plugin.Server.EntityManager.TryGetComponentData(ev.Died, out BloodConsumeSource bS) && bS.UnitBloodType.Equals(Helper.vBloodType);

                        var useGroup = ExperienceSystem.groupLevelScheme != ExperienceSystem.GroupLevelScheme.None && ExperienceSystem.GroupModifier > 0;

                        var triggerLocation = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ev.Died);                        
                        var closeAllies = Alliance.GetClosePlayers(
                            triggerLocation.Position, killer, ExperienceSystem.GroupMaxDistance, true, useGroup, Helper.deathLogging);

                        // If you get experience for the kill, you get heat for the kill
                        if (ExperienceSystem.isEXPActive) ExperienceSystem.EXPMonitor(closeAllies, ev.Died, isVBlood);
                        if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerKillEntity(closeAllies, ev.Died, isVBlood);
                    }
                    
                    if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.UpdateMastery(killer, ev.Died);
                    if (Bloodlines.areBloodlinesEnabled) Bloodlines.UpdateBloodline(killer, ev.Died);

                }

                //-- Auto Respawn & HunterHunted System Begin
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died)) {
                    if (Helper.deathLogging) Plugin.LogInfo("the dead person is a player, running xp loss and heat dumping");
                    if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerDied(ev.Died);
                    if (ExperienceSystem.isEXPActive && ExperienceSystem.xpLostOnRelease) {
                        ExperienceSystem.deathXPLoss(ev.Died, ev.Killer);
                    }

                    PlayerCharacter player = __instance.EntityManager.GetComponentData<PlayerCharacter>(ev.Died);
                    Entity userEntity = player.UserEntity;
                    User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                    ulong SteamID = user.PlatformId;

                    //-- Check for AutoRespawn
                    if (user.IsConnected) {
                        bool isServerWide = Database.autoRespawn.ContainsKey(1);
                        bool doRespawn = isServerWide || Database.autoRespawn.ContainsKey(SteamID);

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