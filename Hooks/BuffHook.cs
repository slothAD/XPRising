using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using OpenRPG.Utils;
using OpenRPG.Systems;
using OpenRPG.Utils.Prefabs;
using ProjectM.Scripting;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Hooks;
[HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), nameof(ModifyUnitStatBuffSystem_Spawn.OnUpdate))]
public class ModifyUnitStatBuffSystem_Spawn_Patch
{
    #region GodMode & Other Buff
    private static ModifyUnitStatBuff_DOTS Cooldown = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.CooldownModifier,
        Value = 0,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunCharge = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunChargeTime,
        Value = 50000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Hazard = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ImmuneToHazards,
        Value = 1,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Speed = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MovementSpeed,
        Value = 15,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.FireResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HolyResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SilverResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS GResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.GarlicResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS RPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourcePower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PHRegen = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PassiveHealthRegen,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HRecovery = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HealthRecovery,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS MaxHP = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MaxHealth,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS MaxYield = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourceYield,
        Value = 10,
        ModificationType = ModificationType.Multiply,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS DurabilityLoss = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ReducedResourceDurabilityLoss,
        Value = -10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };
    #endregion
    
    private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance) {
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Entered Buff System, attempting Old Style");
        oldStyleBuffHook(__instance);
        //Plugin.SystemLog(LogSystem.Buff, LogLevel.Info, "Old Style Done, attemping New Style, just cause");
        //rebuiltBuffHook(__instance);
    }

    public static void oldStyleBuffApplicaiton(Entity entity, EntityManager entityManager) {

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Applying OpenRPG Buffs");
        Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Owner found, hash: " + Owner.GetHashCode());
        if (!entityManager.HasComponent<PlayerCharacter>(Owner)) return;

        PlayerCharacter playerCharacter = entityManager.GetComponentData<PlayerCharacter>(Owner);
        Entity User = playerCharacter.UserEntity;
        User Data = entityManager.GetComponentData<User>(User);

        var Buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Buffer acquired, length: " + Buffer.Length);

        //Buffer.Clear();
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Buffer cleared, to confirm length: " + Buffer.Length);


        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Weapon Mastery System Buff Reciever");
        if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(Buffer, Owner, Data.PlatformId);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Bloodline Buff Reciever");
        if (Bloodlines.areBloodlinesEnabled) Bloodlines.BuffReceiver(Buffer, Owner, Data.PlatformId);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Class System Buff Reciever");
        if (ExperienceSystem.LevelRewardsOn && ExperienceSystem.isEXPActive) ExperienceSystem.BuffReceiver(Buffer, Owner, Data.PlatformId);


        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing PowerUp Command");
        if (Database.PowerUpList.TryGetValue(Data.PlatformId, out var powerUpData)) {
            Buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.MaxHealth,
                Value = powerUpData.MaxHP,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            Buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.PhysicalPower,
                Value = powerUpData.PATK,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            Buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.SpellPower,
                Value = powerUpData.SATK,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            Buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.PhysicalResistance,
                Value = powerUpData.PDEF,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });

            Buffer.Add(new ModifyUnitStatBuff_DOTS() {
                StatType = UnitStatType.SpellResistance,
                Value = powerUpData.SDEF,
                ModificationType = ModificationType.Add,
                Id = ModificationId.NewId(0)
            });
        }



        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing NoCD Command");
        if (Database.nocooldownlist.ContainsKey(Data.PlatformId)) {
            Buffer.Add(Cooldown);
        }

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Sun Immunity Command");
        if (Database.sunimmunity.ContainsKey(Data.PlatformId)) {
            Buffer.Add(SunCharge);
            Buffer.Add(Hazard);
            Buffer.Add(SunResist);
        }

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing Speeding Command");
        if (Database.speeding.ContainsKey(Data.PlatformId)) {
            Buffer.Add(Speed);
        }

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Now doing GodMode Command");
        if (Database.godmode.ContainsKey(Data.PlatformId)) {
            Buffer.Add(PResist);
            Buffer.Add(FResist);
            Buffer.Add(HResist);
            Buffer.Add(SResist);
            Buffer.Add(SunResist);
            Buffer.Add(GResist);
            Buffer.Add(SPResist);
            Buffer.Add(PPower);
            Buffer.Add(RPower);
            Buffer.Add(SPPower);
            Buffer.Add(MaxYield);
            Buffer.Add(MaxHP);
            Buffer.Add(Hazard);
            Buffer.Add(SunCharge);
            Buffer.Add(DurabilityLoss);
        }

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Done Adding, Buffer length: " + Buffer.Length);

    }

    public static void oldStyleBuffHook(ModifyUnitStatBuffSystem_Spawn __instance) {

        EntityManager entityManager = __instance.EntityManager;
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

        Plugin.Log(LogSystem.Buff, LogLevel.Info, "Entities Length of " + entities.Length);

        foreach (var entity in entities) {
            var GUID = entityManager.GetComponentData<PrefabGUID>(entity);
            Plugin.Log(LogSystem.Buff, LogLevel.Info, "GUID of " + GUID.GuidHash);
            if (GUID.GuidHash == Helper.forbiddenBuffGUID) {
                Plugin.Log(LogSystem.Buff, LogLevel.Info, "Forbidden buff found with GUID of " + GUID.GuidHash);
                continue;
            }
            else if (GUID.GuidHash == Helper.buffGUID) {
                oldStyleBuffApplicaiton(entity, entityManager);
            }
        }
    }
    public static void rebuiltBuffHook(ModifyUnitStatBuffSystem_Spawn __instance) {
        EntityManager em = __instance.EntityManager;
        bool hasSGM = Helper.GetServerGameManager(out ServerGameManager sgm);
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
        NativeArray<Entity> pcArray = query.ToEntityArray(Allocator.Temp);
        Plugin.Log(LogSystem.Buff, LogLevel.Info, "got connected Players, array of length " + pcArray.Length);
        foreach (var entity in pcArray) {
            em.TryGetComponentData<PlayerCharacter>(entity, out var pc);
            em.TryGetComponentData<User>(entity, out var userEntity);
            ulong SteamID = userEntity.PlatformId;
            bool hasBuffs = Cache.buffData.TryGetValue(SteamID, out List<BuffData> bdl);
            if (!hasBuffs) { continue; }

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

    public static bool applyBuff(EntityManager em, ModifyUnitStatBuff_DOTS buff, ServerGameManager sgm, Entity e) {
        ModifiableFloat stat = new ModifiableFloat();
        ModifiableInt statInt = new ModifiableInt();
        bool targetIsInt = false;
        bool applied = false;
        UnitStatType tar = buff.StatType;

        if (Helper.baseStatsSet.Contains((int)tar)) {
            em.TryGetComponentData<UnitStats>(e, out var baseStats);
            if (tar == UnitStatType.PhysicalPower) {
                stat = baseStats.PhysicalPower;
                //baseStats.PhysicalPower.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
            } else if (tar == UnitStatType.ResourcePower) {
                stat = baseStats.ResourcePower;
            } else if (tar == UnitStatType.SiegePower) {
                stat = baseStats.SiegePower;
                //baseStats.SiegePower.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
            } else if (tar == UnitStatType.AttackSpeed || tar == UnitStatType.PrimaryAttackSpeed) {
                stat = baseStats.AttackSpeed;
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
        } else if (tar == UnitStatType.MaxHealth) {
            em.TryGetComponentData<Health>(e, out Health health);
            health.MaxHealth.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
        } else if (tar == UnitStatType.MovementSpeed) {
            em.TryGetComponentData<Movement>(e, out Movement speed);
            speed.Speed.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
        }
        try {
            if (targetIsInt) {
                statInt.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
            } else {
                stat.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
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
    private static void Prefix(BuffSystem_Spawn_Server __instance) {
        if (PermissionSystem.isVIPSystem) {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities) {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
            }
        }
    }

    private static void Postfix(BuffSystem_Spawn_Server __instance) {

        if (WeaponMasterSystem.isMasteryEnabled) {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities) {
                if (!__instance.EntityManager.HasComponent<InCombatBuff>(entity)) continue;
                Entity e_Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!__instance.EntityManager.HasComponent<PlayerCharacter>(e_Owner)) continue;
                Entity e_User = __instance.EntityManager.GetComponentData<PlayerCharacter>(e_Owner).UserEntity;

                if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.LoopMastery(e_User, e_Owner);
            }
        }
    }
}

[HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
public class DebugBuffSystem_Patch
{
    private static void Prefix(BuffDebugSystem __instance)
    {
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
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
            if (HunterHuntedSystem.isActive) HunterHuntedSystem.CheckForAmbush(ownerEntity);
        } else if (combatEnd) {
            Plugin.Log(LogSystem.Buff, LogLevel.Info, $"{steamID}: Combat end");
            Cache.playerCombatEnd[steamID] = DateTime.Now;
        }
    }
}

[HarmonyPatch(typeof(ModifyBloodDrainSystem_Spawn), nameof(ModifyBloodDrainSystem_Spawn.OnUpdate))]
public class ModifyBloodDrainSystem_Spawn_Patch {
    private static void Prefix(ModifyBloodDrainSystem_Spawn __instance) {

        if (PermissionSystem.isVIPSystem) {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities) {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                //if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(entities[i], GUID);
                if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
            }
        }
    }
}