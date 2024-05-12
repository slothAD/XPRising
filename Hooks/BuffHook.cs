using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using OpenRPG.Models;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using OpenRPG.Utils;
using OpenRPG.Systems;
using OpenRPG.Utils.Prefabs;
using ProjectM.Scripting;
using Stunlock.Core;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Hooks;
[HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), nameof(ModifyUnitStatBuffSystem_Spawn.OnUpdate))]
public class ModifyUnitStatBuffSystem_Spawn_Patch
{
    private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance) {
        oldStyleBuffHook(__instance);
        rebuiltBuffHook(__instance);
    }

    private static void oldStyleBuffApplicaiton(Entity entity, EntityManager entityManager) {

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Applying OpenRPG Buffs");
        var owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Owner found, hash: " + owner.GetHashCode());
        if (!entityManager.HasComponent<PlayerCharacter>(owner)) return;

        var playerCharacter = entityManager.GetComponentData<PlayerCharacter>(owner);
        var userEntity = playerCharacter.UserEntity;
        var user = entityManager.GetComponentData<User>(userEntity);

        var buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Buffer acquired, length: " + buffer.Length);

        //Buffer.Clear();
        //Plugin.Log(LogSystem.Buff, LogLevel.Info, "Buffer cleared, to confirm length: " + Buffer.Length);


        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Weapon Mastery System Buff Receiver");
        if (Plugin.WeaponMasterySystemActive) WeaponMasterySystem.BuffReceiver(buffer, owner, user.PlatformId);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Bloodline Buff Receiver");
        if (Plugin.BloodlineSystemActive) BloodlineSystem.BuffReceiver(buffer, owner, user.PlatformId);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Class System Buff Receiver");
        if (ExperienceSystem.LevelRewardsOn && Plugin.ExperienceSystemActive) ExperienceSystem.BuffReceiver(buffer, owner, user.PlatformId);


        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing PowerUp Command");
        if (Database.PowerUpList.TryGetValue(user.PlatformId, out var powerUpData)) {
            buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.MaxHealth,
                Value = powerUpData.MaxHP,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.PhysicalPower,
                Value = powerUpData.PATK,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.SpellPower,
                Value = powerUpData.SATK,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.PhysicalResistance,
                Value = powerUpData.PDEF,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.SpellResistance,
                Value = powerUpData.SDEF,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });
        }

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Done Adding, Buffer length: " + buffer.Length);

    }

    private static void oldStyleBuffHook(ModifyUnitStatBuffSystem_Spawn __instance) {
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Attempting Old Style");

        EntityManager entityManager = __instance.EntityManager;
        NativeArray<Entity> entities = __instance.__query_1735840491_0.ToEntityArray(Allocator.Temp);

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Entities Length of " + entities.Length);

        foreach (var entity in entities) {
            var GUID = entityManager.GetComponentData<PrefabGUID>(entity);
            Plugin.Log(LogSystem.Buff, LogLevel.Info, "GUID of " + GUID.GuidHash);
            if (GUID.GuidHash == Helper.forbiddenBuffGUID) {
                Plugin.Log(LogSystem.Buff, LogLevel.Info, "Forbidden buff found with GUID of " + GUID.GuidHash);
            }
            else if (GUID == Helper.AppliedBuff) {
                oldStyleBuffApplicaiton(entity, entityManager);
            }
        }
    }

    private static void rebuiltBuffHook(ModifyUnitStatBuffSystem_Spawn __instance) {
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Attempting New Style");
        EntityManager em = __instance.EntityManager;
        bool hasSGM = Helper.GetServerGameManager(out var sgm);
        if (!hasSGM) {
            Plugin.Log(LogSystem.Buff, LogLevel.Error, "No Server Game Manager, Something is WRONG.");
            return;
        }

        EntityQuery query = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<PlayerCharacter>(),
                        ComponentType.ReadOnly<IsConnected>()
                    },
            Options = EntityQueryOptions.IncludeDisabled
        });
        var pcArray = query.ToEntityArray(Allocator.Temp);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "got connected Players, array of length " + pcArray.Length);
        foreach (var entity in pcArray) {
            em.TryGetComponentData<PlayerCharacter>(entity, out var pc);
            em.TryGetComponentData<User>(entity, out var userEntity);
            var steamID = userEntity.PlatformId;
            var hasBuffs = Cache.buffData.TryGetValue(steamID, out List<BuffData> bdl);
            if (!hasBuffs)
            {
                Plugin.Log(LogSystem.Buff, LogLevel.Info, $"{steamID} has no buffs");
                continue;
            }

            var Buffer = em.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
            //em.TryGetComponentData<BuffBuffer>(entity, out BuffBuffer buffer2);

            em.TryGetBuffer<ModifyUnitStats>(entity, out var stats);

            Plugin.Log(LogSystem.Buff, LogLevel.Info, "got entities modifyunitystatbuffDOTS buffer of length " + Buffer.Length);
            Plugin.Log(LogSystem.Buff, LogLevel.Info, "got entities modifyunitystatbuff buffer of length " + stats.Length);

            foreach (BuffData bd in bdl) {
                if (bd.isApplied) { continue; }
                ModifyUnitStatBuff_DOTS buff = new ModifyUnitStatBuff_DOTS {
                    StatType = (UnitStatType)bd.targetStat,
                    Value = (float)bd.value,
                    ModificationType = (ModificationType)bd.modificationType,
                    Id = ModificationId.NewId(bd.ID)
                };
                applyBuff(em, buff, sgm, entity);
                //baseStats.PhysicalPower.ApplyModification(sgm, entity, entity, buff.ModificationType, buff.Value);
            }
        }
    }

    private static bool applyBuff(EntityManager em, ModifyUnitStatBuff_DOTS buff, ServerGameManager sgm, Entity e) {
        ModifiableFloat stat = new ModifiableFloat();
        ModifiableInt statInt = new ModifiableInt();
        bool targetIsInt = false;
        bool applied = false;
        UnitStatType tar = buff.StatType;

        if (Helper.baseStatsSet.Contains(tar)) {
            em.TryGetComponentData<UnitStats>(e, out var baseStats);
            if (tar == UnitStatType.PhysicalPower) {
                stat = baseStats.PhysicalPower;
            } else if (tar == UnitStatType.ResourcePower) {
                stat = baseStats.ResourcePower;
            } else if (tar == UnitStatType.SiegePower) {
                stat = baseStats.SiegePower;
            // } else if (tar == UnitStatType.AttackSpeed || tar == UnitStatType.PrimaryAttackSpeed) {
            //     stat = baseStats.AttackSpeed; // Looks like AttackSpeed is no longer modifiable
            } else if (tar == UnitStatType.FireResistance) {
                statInt = baseStats.FireResistance; targetIsInt = true;
            } else if (tar == UnitStatType.GarlicResistance) {
                statInt = baseStats.GarlicResistance; targetIsInt = true;
            } else if (tar == UnitStatType.SilverResistance) {
                statInt = baseStats.SilverResistance; targetIsInt = true;
            } else if (tar == UnitStatType.HolyResistance) {
                statInt = baseStats.HolyResistance; targetIsInt = true;
            } else if (tar == UnitStatType.SunResistance) {
                statInt = baseStats.SunResistance; targetIsInt = true;
            } else if (tar == UnitStatType.SpellResistance) {
                stat = baseStats.SpellResistance;
            } else if (tar == UnitStatType.PhysicalResistance) {
                stat = baseStats.PhysicalResistance;
            } else if (tar == UnitStatType.PhysicalCriticalStrikeChance) {
                stat = baseStats.PhysicalCriticalStrikeChance;
            } else if (tar == UnitStatType.PhysicalCriticalStrikeDamage) {
                stat = baseStats.PhysicalCriticalStrikeDamage;
            } else if (tar == UnitStatType.SpellCriticalStrikeChance) {
                stat = baseStats.SpellCriticalStrikeChance;
            } else if (tar == UnitStatType.SpellCriticalStrikeDamage) {
                stat = baseStats.SpellCriticalStrikeDamage;
            } else if (tar == UnitStatType.PassiveHealthRegen) {
                stat = baseStats.PassiveHealthRegen;
            } else if (tar == UnitStatType.PvPResilience) {
                statInt = baseStats.PvPResilience; targetIsInt = true;
            } else if (tar == UnitStatType.ResourceYield) {
                stat = baseStats.ResourceYieldModifier;
            } else if (tar == UnitStatType.PvPResilience) {
                statInt = baseStats.PvPResilience; targetIsInt = true;
            } else if (tar == UnitStatType.ReducedResourceDurabilityLoss) {
                stat = baseStats.ReducedResourceDurabilityLoss;
            }
        }
        else if (tar == UnitStatType.MaxHealth) {
            // TODO check if these changes are valid..
            // they probably aren't but we aren't using this part of the buff hook any?
            em.TryGetComponentData<Health>(e, out Health health);
            //health.MaxHealth.UpdateValue(sgm, e, e, buff.ModificationType, buff.Value);
            ModifiableFloat.ModifyValue(ref health.MaxHealth._Value, ref health.MaxHealth._Value, buff.ModificationType, buff.Value);
        }
        else if (tar == UnitStatType.MovementSpeed) {
            em.TryGetComponentData<Movement>(e, out Movement speed);
            // speed.Speed.UpdateValue(sgm, e, e, buff.ModificationType, buff.Value);
            ModifiableFloat.ModifyValue(ref speed.Speed._Value, ref speed.Speed._Value, buff.ModificationType, buff.Value);
        }
        try {
            if (targetIsInt) {
                // statInt.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                ModifiableInt.ModifyValue(ref statInt._Value, ref statInt._Value, buff.ModificationType, (int)buff.Value);
            } else {
                // stat.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                ModifiableFloat.ModifyValue(ref stat._Value, ref stat._Value, buff.ModificationType, buff.Value);
            }
            applied = true;
        } catch {
            Plugin.Log(LogSystem.Buff, LogLevel.Info, "Failed to apply buff to statID: " + tar);
        }
        return applied;
    }
}

[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public class BuffSystem_Spawn_Server_Patch {
    private static void Postfix(BuffSystem_Spawn_Server __instance) {

        if (Plugin.WeaponMasterySystemActive) {
            NativeArray<Entity> entities = __instance.__query_401358634_0.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities) {
                if (!__instance.EntityManager.HasComponent<InCombatBuff>(entity)) continue;
                Entity e_Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!__instance.EntityManager.HasComponent<PlayerCharacter>(e_Owner)) continue;
                Entity e_User = __instance.EntityManager.GetComponentData<PlayerCharacter>(e_Owner).UserEntity;

                WeaponMasterySystem.LoopMastery(e_User, e_Owner);
            }
        }
    }
}

[HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
public class DebugBuffSystem_Patch
{
    private static void Prefix(BuffDebugSystem __instance)
    {
        NativeArray<Entity> entities = __instance.__query_401358786_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities) {
            var guid = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);

            var combatStart = false;
            var combatEnd = false;
            var newPlayer = false;
            switch (guid.GuidHash)
            {
                case (int)Buffs.Buff_InCombat:
                    combatStart = true;
                    break;
                case (int)Buffs.Buff_OutOfCombat:
                    combatEnd = true;
                    break;
                case (int)Effects.AB_Interact_TombCoffinSpawn_Travel:
                    newPlayer = true;
                    break;
                default:
                    continue;
            }

            // Get entity owner: This will be the entity that actually gets the buff
            var ownerEntity = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
            // If the owner is not a player character, ignore this entity
            if (!__instance.EntityManager.TryGetComponentData(ownerEntity, out PlayerCharacter playerCharacter)) continue;
            
            var userEntity = playerCharacter.UserEntity;
            var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
            var steamID = userData.PlatformId;
            
            if (newPlayer) Helper.UpdatePlayerCache(userEntity, userData);
            if (combatStart || combatEnd) TriggerCombatUpdate(ownerEntity, steamID, combatStart, combatEnd);
        }
    }

    private static void TriggerCombatUpdate(Entity ownerEntity, ulong steamID, bool combatStart, bool combatEnd)
    {
        // Update player combat status
        // Notes:
        // - only update combatStart if we are not already in combat. It gets sent multiple times as
        //   mobs refresh their combat state with the PC
        // - Buff_OutOfCombat only seems to be sent once.
        var inCombat = Cache.GetCombatStart(steamID) > Cache.GetCombatEnd(steamID);
        if (combatStart && !inCombat) {
            Plugin.Log(LogSystem.Buff, LogLevel.Info, $"{steamID}: Combat start");
            Cache.playerCombatStart[steamID] = DateTime.Now;

            // Actions to check on combat start
            if (Plugin.WantedSystemActive) WantedSystem.CheckForAmbush(ownerEntity);
        } else if (combatEnd) {
            Plugin.Log(LogSystem.Buff, LogLevel.Info, $"{steamID}: Combat end");
            Cache.playerCombatEnd[steamID] = DateTime.Now;
        }
    }
}