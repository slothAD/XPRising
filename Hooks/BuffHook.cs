using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using RPGMods.Utils;
using RPGMods.Systems;
using static ProjectM.UI.PowerSelectionMenu;
using System.Collections.Generic;
using ProjectM.Scripting;

namespace RPGMods.Hooks
{
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

        public static bool buffLogging = true;

        private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance)
        {

            EntityManager em = __instance.EntityManager;
            bool hasSGM = Helper.GetServerGameManager(out ServerGameManager sgm);
            if (!hasSGM)
            {
                Plugin.Logger.LogInfo("No Server Game Manager, Something is WRONG.");
                return;

            }

            EntityQuery query = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<PlayerCharacter>(),
                            ComponentType.ReadOnly<IsConnected>()
                        },
                Options = EntityQueryOptions.IncludeDisabled
            });
            foreach (var entity in query.ToEntityArray(Allocator.Temp))
            {
                em.TryGetComponentData<PlayerCharacter>(entity, out PlayerCharacter pc);
                em.TryGetComponentData<User>(entity, out User userEntity);
                ulong SteamID = userEntity.PlatformId;
                bool hasBuffs = Cache.buffData.TryGetValue(SteamID, out List<BuffData> bdl);
                if (!hasBuffs) { continue; }

                var Buffer = em.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                //em.TryGetComponentData<BuffBuffer>(entity, out BuffBuffer buffer2);

                em.TryGetBuffer<ModifyUnitStats>(entity, out var stats);

                if (buffLogging) Plugin.Logger.LogInfo("got entities modifyunitystatbuff buffer of length " + Buffer.Length);

                foreach (BuffData bd in bdl)
                {
                    if (bd.isApplied) { continue; }
                    ModifyUnitStatBuff_DOTS buff = new ModifyUnitStatBuff_DOTS
                    {
                        StatType = (UnitStatType)bd.targetStat,
                        Value = (float)bd.value,
                        ModificationType = (ModificationType)bd.modificationType,
                        Id = ModificationId.NewId(bd.ID)
                    };
                    applyBuff(baseStats, buff,sgm,entity);
                    //baseStats.PhysicalPower.ApplyModification(sgm, entity, entity, buff.ModificationType, buff.Value);
                }


            }
        }

        public static void applyBuff(EntityManager em, ModifyUnitStatBuff_DOTS buff, ServerGameManager sgm, Entity e)
        {
            ModifiableFloat stat;
            UnitStatType tar = buff.StatType;

            if (Helper.baseStatsSet.Contains((int)tar)){
                em.TryGetComponentData<UnitStats>(e, out var baseStats);
                if (tar == UnitStatType.PhysicalPower){
                    baseStats.PhysicalPower.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.ResourcePower){
                    baseStats.ResourcePower.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.SiegePower){
                    baseStats.SiegePower.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.AttackSpeed || tar == UnitStatType.PrimaryAttackSpeed){
                    baseStats.AttackSpeed.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.FireResistance){
                    baseStats.FireResistance.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.GarlicResistance){
                    baseStats.GarlicResistance.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.SilverResistance){
                    baseStats.SilverResistance.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.HolyResistance){
                    baseStats.HolyResistance.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.SunResistance){
                    baseStats.SunResistance.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.SpellResistance){
                    baseStats.SpellResistance.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PhysicalResistance){
                    baseStats.PhysicalResistance.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PhysicalCriticalStrikeChance) {
                    baseStats.PhysicalCriticalStrikeChance.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PhysicalCriticalStrikeDamage) {
                    baseStats.PhysicalCriticalStrikeDamage.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.SpellCriticalStrikeChance) {
                    baseStats.SpellCriticalStrikeChance.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.SpellCriticalStrikeDamage) {
                    baseStats.SpellCriticalStrikeDamage.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PassiveHealthRegen) {
                    baseStats.PassiveHealthRegen.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PvPResilience) {
                    baseStats.PvPResilience.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.ResourceYield) {
                    baseStats.ResourceYieldModifier.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
                else if (tar == UnitStatType.PvPResilience) {
                    baseStats.PvPResilience.ApplyModification(sgm, e, e, buff.ModificationType, (int)buff.Value);
                }
                else if (tar == UnitStatType.ReducedResourceDurabilityLoss) {
                    baseStats.ReducedResourceDurabilityLoss.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
                }
            }
            else if(tar == UnitStatType.MaxHealth) {

                em.TryGetComponentData<Health>(e, out Health health);
                health.MaxHealth.ApplyModification(sgm, e, e, buff.ModificationType, buff.Value);
            }

        }

    }

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    public class BuffSystem_Spawn_Server_Patch
    {
        private static void Prefix(BuffSystem_Spawn_Server __instance)
        {

            if (PvPSystem.isPunishEnabled || SiegeSystem.isSiegeBuff || PermissionSystem.isVIPSystem || PvPSystem.isHonorSystemEnabled)
            {
                NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                    //if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(entities[i], GUID);
                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.HonorBuffReceiver(entity, GUID);
                    if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
                    if (PvPSystem.isPunishEnabled) PvPSystem.BuffReceiver(entity, GUID);
                    if (SiegeSystem.isSiegeBuff) SiegeSystem.BuffReceiver(entity, GUID);
                }
            }
        }

        private static void Postfix(BuffSystem_Spawn_Server __instance)
        {

            if (PvPSystem.isPunishEnabled || HunterHuntedSystem.isActive || WeaponMasterSystem.isMasteryEnabled)
            {
                NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (!__instance.EntityManager.HasComponent<InCombatBuff>(entity)) continue;
                    Entity e_Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!__instance.EntityManager.HasComponent<PlayerCharacter>(e_Owner)) continue;
                    Entity e_User = __instance.EntityManager.GetComponentData<PlayerCharacter>(e_Owner).UserEntity;

                    if (HunterHuntedSystem.isActive)
                    {
                        HunterHuntedSystem.HeatManager(e_User);
                        HunterHuntedSystem.HumanAmbusher(e_User, e_Owner, true);
                        HunterHuntedSystem.BanditAmbusher(e_User, e_Owner, true);
                    }
                    if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.LoopMastery(e_User, e_Owner);
                    if (PvPSystem.isPunishEnabled && !ExperienceSystem.isEXPActive) PvPSystem.OnCombatEngaged(entity, e_Owner);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ModifyBloodDrainSystem_Spawn), nameof(ModifyBloodDrainSystem_Spawn.OnUpdate))]
    public class ModifyBloodDrainSystem_Spawn_Patch
    {
        private static void Prefix(ModifyBloodDrainSystem_Spawn __instance)
        {

            if (PermissionSystem.isVIPSystem || PvPSystem.isHonorSystemEnabled)
            {
                NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                    //if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(entities[i], GUID);
                    if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.HonorBuffReceiver(entity, GUID);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
    public class Destroy_TravelBuffSystem_Patch
    {
        private static void Postfix(Destroy_TravelBuffSystem __instance)
        {
            var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                //-- Most likely it's a new player!
                if (GUID.Equals(Database.Buff.AB_Interact_TombCoffinSpawn_Travel))
                {
                    var Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!__instance.EntityManager.HasComponent<PlayerCharacter>(Owner)) return;

                    var userEntity = __instance.EntityManager.GetComponentData<PlayerCharacter>(Owner).UserEntity;
                    var playerName = __instance.EntityManager.GetComponentData<User>(userEntity).CharacterName.ToString();

                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.NewPlayerReceiver(userEntity, Owner, playerName);
                    else Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                }
            }
        }
    }
}