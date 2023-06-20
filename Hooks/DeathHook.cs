using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGMods.Commands;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;

namespace RPGMods.Hooks {
[HarmonyPatch]
public class DeathEventListenerSystem_Patch
{
    public static bool deathLogging = false;
    [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
    [HarmonyPostfix]
    public static void Postfix(DeathEventListenerSystem __instance)
    {
        //if (__instance._DeathEventQuery != null)
        {
            NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
            foreach (DeathEvent ev in deathEvents)
            {
                //-- Just track whatever died...
                if (WorldDynamicsSystem.isFactionDynamic) WorldDynamicsSystem.MobKillMonitor(ev.Died);

                //-- Player Creature Kill Tracking
                var killer = ev.Killer;
                
                // If the entity killing is a minion, switch the killer to the owner of the minion.
                if (__instance.EntityManager.HasComponent<Minion>(killer)) {
                    if (deathLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Minion killed entity. Getting owner...");
                    if (__instance.EntityManager.TryGetComponentData<EntityOwner>(killer, out var entityOwner)) {
                        killer = entityOwner.Owner;
                        if (deathLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Owner found, switching killer to owner.");
                    }
                }
                
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(killer) && __instance.EntityManager.HasComponent<Movement>(ev.Died))
                {
                    if (ExperienceSystem.isEXPActive) ExperienceSystem.EXPMonitor(killer, ev.Died);
                    if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerKillEntity(killer, ev.Died);
                    if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.UpdateMastery(killer, ev.Died);
                    if (Bloodlines.areBloodlinesEnabled) Bloodlines.UpdateBloodline(killer, ev.Died);
                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.MobKillMonitor(killer, ev.Died);

                }
                //-- ----------------------

                //-- Auto Respawn & HunterHunted System Begin
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(ev.Died))
                {
                    if (HunterHuntedSystem.isActive) HunterHuntedSystem.PlayerDied(ev.Died);
                    
                    PlayerCharacter player = __instance.EntityManager.GetComponentData<PlayerCharacter>(ev.Died);
                    Entity userEntity = player.UserEntity;
                    User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                    ulong SteamID = user.PlatformId;
                    //-- ----------------------------------

                    //-- Check for AutoRespawn
                    if (user.IsConnected)
                    {
                        bool isServerWide = Database.autoRespawn.ContainsKey(1);
                        bool doRespawn;
                        if (!isServerWide)
                        {
                            doRespawn = Database.autoRespawn.ContainsKey(SteamID);
                        }
                        else { doRespawn = true; }

                        if (doRespawn)
                        {
                            Utils.RespawnCharacter.Respawn(ev.Died, player, userEntity);
                        }
                    }
                    //-- ---------------------
                }
                //-- ----------------------------------------
            }
        }
    }
}
}