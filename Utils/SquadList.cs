using System;
using ProjectM;
using RPGMods.Systems;
using ProjectM.Network;
using Unity.Entities;
using Unity.Transforms;

namespace RPGMods.Utils
{
    public class SquadList {
        public struct Unit {
            public string name { get; }
            public int level { get; }

            public Unit(string n, int l) {
                name = n;
                level = l;
            }
        }
        private static Unit[] bandit_units = {
            new("CHAR_Bandit_Wolf", 14),
            new("CHAR_Bandit_Worker_Gatherer", 14),
            new("CHAR_Bandit_Worker_Miner", 14),
            new("CHAR_Bandit_Worker_Woodcutter", 14),
            new("CHAR_Bandit_Hunter", 16),
            new("CHAR_Bandit_Thug", 16),
            new("CHAR_Bandit_Thief", 18),
            new("CHAR_Bandit_Mugger", 20),
            new("CHAR_Bandit_Trapper", 20),
            new("CHAR_Bandit_Deadeye", 26),
            new("CHAR_Bandit_Stalker", 30),
            new("CHAR_Bandit_Bomber", 32),
            // "CHAR_Bandit_Deadeye_Frostarrow_VBlood", // 20
            // "CHAR_Bandit_Foreman_VBlood", // 20
            // "CHAR_Bandit_StoneBreaker_VBlood", // 20
            // "CHAR_Bandit_Stalker_VBlood", // 27
            // "CHAR_Bandit_Bomber_VBlood", // 30
            // "CHAR_Bandit_Deadeye_Chaosarrow_VBlood", // 30
            // "CHAR_Bandit_Tourok_VBlood", // 37
        };

        private static Unit[] church = {
            new("CHAR_ChurchOfLight_Miner_Standard", 42),
            new("CHAR_ChurchOfLight_Archer", 56),
            new("CHAR_ChurchOfLight_SlaveRuffian", 60),
            new("CHAR_ChurchOfLight_Cleric", 62),
            new("CHAR_ChurchOfLight_Footman", 62),
            new("CHAR_ChurchOfLight_Rifleman", 62),
            new("CHAR_ChurchOfLight_SlaveMaster_Enforcer", 64),
            new("CHAR_ChurchOfLight_SlaveMaster_Sentry", 64),
            new("CHAR_ChurchOfLight_Knight_2H", 68),
            new("CHAR_ChurchOfLight_Knight_Shield", 68),
            new("CHAR_ChurchOfLight_CardinalAide", 70),
            new("CHAR_ChurchOfLight_Lightweaver", 72),
            new("CHAR_ChurchOfLight_Paladin", 74),
            new("CHAR_ChurchOfLight_Priest", 74),
        };

        private static Unit[] church_elite = {
            new("CHAR_Paladin_DivineAngel", 80),
            // "CHAR_ChurchOfLight_Sommelier_VBlood", // 70
            // "CHAR_ChurchOfLight_Cardinal_VBlood", // 74
            // "CHAR_ChurchOfLight_Overseer_VBlood", // 66
            // "CHAR_ChurchOfLight_Paladin_VBlood", // 80
        };

        private static Unit[] church_extra = {
            new("CHAR_Militia_EyeOfGod", 0) // Spell effect?
        };

        private static Unit[] cultist_units = {
            new("CHAR_Cultist_Pyromancer", 60),
            new("CHAR_Cultist_Slicer", 60),
        };

        private static Unit[] cursed_units = {
            new("CHAR_Cursed_MonsterToad", 61),
            new("CHAR_Cursed_ToadSpitter", 61),
            new("CHAR_Cursed_Witch_Exploding_Mosquito", 61),
            new("CHAR_Cursed_MonsterToad_Minion", 62),
            new("CHAR_Cursed_Mosquito", 62),
            new("CHAR_Cursed_Wolf", 62),
            new("CHAR_Cursed_WormTerror", 62),
            new("CHAR_Cursed_Bear_Standard", 64),
            new("CHAR_Cursed_Nightlurker", 64),
            new("CHAR_Cursed_Witch", 72),
            new("CHAR_Cursed_Bear_Spirit", 80),
            new("CHAR_Cursed_Wolf_Spirit", 80),
            // "CHAR_Cursed_MountainBeast_VBlood", // 83
            // "CHAR_Cursed_ToadKing_VBlood", // 64
            // "CHAR_Cursed_Witch_VBlood", // 77
        };

        private static Unit[] farmlands = {
            new("CHAR_Farmlands_HostileVillager_Female_FryingPan", 28),
            new("CHAR_Farmlands_HostileVillager_Female_Pitchfork", 28),
            new("CHAR_Farmlands_HostileVillager_Male_Club", 28),
            new("CHAR_Farmlands_HostileVillager_Male_Shovel", 28),
            new("CHAR_Farmlands_HostileVillager_Male_Torch", 28),
            new("CHAR_Farmlands_HostileVillager_Male_Unarmed", 28),
            new("CHAR_Farmlands_Woodcutter_Standard", 34),
            new("CHAR_Farmland_Wolf", 40),
        };

        private static Unit[] farmNonHostile = {
            new("CHAR_Farmlands_Villager_Female_Sister", 20),
            new("CHAR_Farmlands_Villager_Female", 26),
            new("CHAR_Farmlands_Villager_Male", 26),
            new("CHAR_Farmlands_Farmer", 34),
        };

        private static Unit[] farmFood = {
            new("CHAR_Farmlands_SheepOld", 10),
            new("CHAR_Farmlands_SmallPig", 20),
            new("CHAR_Farmlands_Pig", 24),
            new("CHAR_Farmlands_Cow", 30),
            new("CHAR_Farmlands_Sheep", 36),
            new("CHAR_Farmlands_Ram", 38),
        };

        private static Unit[] forest = {
            new("CHAR_Forest_Wolf", 10),
            new("CHAR_Forest_AngryMoose", 16),
            new("CHAR_Forest_Bear_Standard", 18),
            // "CHAR_Forest_Wolf_VBlood", // 16
            // "CHAR_Forest_Bear_Dire_Vblood", // 35
        };

        private static Unit[] gloomrot = {
            new("CHAR_Gloomrot_Pyro", 56),
            new("CHAR_Gloomrot_Batoon", 58),
            new("CHAR_Gloomrot_Railgunner", 58),
            new("CHAR_Gloomrot_Tazer", 58),
            new("CHAR_Gloomrot_Technician", 58),
            new("CHAR_Gloomrot_Technician_Labworker", 58),
            new("CHAR_Gloomrot_TractorBeamer", 58),
            new("CHAR_Gloomrot_SentryOfficer", 60),
            new("CHAR_Gloomrot_SentryTurret", 60),
            new("CHAR_Gloomrot_SpiderTank_Driller", 60),
            new("CHAR_Gloomrot_AceIncinerator", 74),
            new("CHAR_Gloomrot_SpiderTank_LightningRod", 74),
            new("CHAR_Gloomrot_SpiderTank_Gattler", 77),
            new("CHAR_Gloomrot_SpiderTank_Zapper", 77),
            // "CHAR_Gloomrot_Iva_VBlood", // 60
            // "CHAR_Gloomrot_Monster_VBlood", // 83
            // "CHAR_Gloomrot_Purifier_VBlood", // 60
            // "CHAR_Gloomrot_RailgunSergeant_VBlood", // 77
            // "CHAR_Gloomrot_TheProfessor_VBlood", // 74
            // "CHAR_Gloomrot_Voltage_VBlood", // 60
        };

        private static Unit[] harpy = {
            new("CHAR_Harpy_Dasher", 66),
            new("CHAR_Harpy_FeatherDuster", 66),
            new("CHAR_Harpy_Sorceress", 68),
            new("CHAR_Harpy_Scratcher", 70),
            // "CHAR_Harpy_Matriarch_VBlood", // 68
        };
        
        private static Unit[] militia_units = {
            new("CHAR_Militia_Hound", 36),
            new("CHAR_Militia_Light", 36),
            new("CHAR_Militia_Torchbearer", 36),
            new("CHAR_Militia_InkCrawler", 38),
            new("CHAR_Militia_Guard", 40),
            new("CHAR_Militia_Bomber", 47),
            new("CHAR_Militia_Longbowman", 42),
            new("CHAR_Militia_Nun", 42),
            new("CHAR_Militia_Miner_Standard", 50),
            new("CHAR_Militia_Heavy", 54),
            new("CHAR_Militia_Devoted", 56),
            new("CHAR_Militia_Crossbow", 70),
            // "CHAR_Militia_BishopOfDunley_VBlood", // 57
            // "CHAR_Militia_Glassblower_VBlood", // 47
            // "CHAR_Militia_Guard_VBlood", // 44
            // "CHAR_Militia_Hound_VBlood", // 48
            // "CHAR_Militia_HoundMaster_VBlood", // 48
            // "CHAR_Militia_Leader_VBlood", // 57
            // "CHAR_Militia_Longbowman_LightArrow_Vblood", // 40
            // "CHAR_Militia_Nun_VBlood", // 44
            // "CHAR_Militia_Scribe_VBlood", // 47
        };

        private static Unit[] vhunter = {
            new("CHAR_VHunter_Leader_VBlood", 44),
            new("CHAR_VHunter_Jade_VBlood", 57),
        };

        private static Unit[] wtf = {
            new("CHAR_Scarecrow", 54),
            new("CHAR_ChurchOfLight_Sommelier_BarrelMinion", 50),
            new("CHAR_Farmlands_HostileVillager_Werewolf", 20), // level 20?
            // "CHAR_Poloma_VBlood", // 35 - geomancer
            // "CHAR_Villager_CursedWanderer_VBlood", // 62
            // "CHAR_Villager_Tailor_VBlood", // 40
        };

        private static Unit[] spiders = {
            new("CHAR_Spider_Forestling", 20),
            new("CHAR_Spider_Forest", 26),
            new("CHAR_Spider_Baneling", 56),
            new("CHAR_Spider_Spiderling", 56),
            new("CHAR_Spider_Melee", 58),
            new("CHAR_Spider_Range", 58),
            new("CHAR_Spider_Broodmother", 60),
            // "CHAR_Spider_Queen_VBlood", // 58
        };

        private static Unit[] golems = { // Nature?
            new("CHAR_IronGolem", 36),
            new("CHAR_StoneGolem", 36),
            new("CHAR_CopperGolem", 42),
            new("CHAR_RockElemental", 50),
            new("CHAR_Treant", 57),
        };

        private static Unit[] mutants = {
            new("CHAR_Mutant_RatHorror", 58),
            new("CHAR_Mutant_FleshGolem", 60),
            new("CHAR_Mutant_Wolf", 64),
            new("CHAR_Mutant_Spitter", 70),
            new("CHAR_Mutant_Bear_Standard", 74),
        };

        private static Unit[] undead_minions = {
            new("CHAR_Undead_SkeletonSoldier_TombSummon", 1),
            new("CHAR_Undead_SkeletonSoldier_Withered", 1),
            new("CHAR_Undead_SkeletonCrossbow_Graveyard", 2),
            new("CHAR_Undead_RottingGhoul", 4),
            new("CHAR_Undead_ArmoredSkeletonCrossbow_Farbane", 18),
            new("CHAR_Undead_SkeletonCrossbow_GolemMinion", 18),
            new("CHAR_Undead_SkeletonCrossbow_Farbane_OLD", 20),
            new("CHAR_Undead_SkeletonSoldier_Armored_Farbane", 20),
            new("CHAR_Undead_SkeletonSoldier_GolemMinion", 20),
            new("CHAR_Undead_SkeletonApprentice", 22),
            new("CHAR_Undead_UndyingGhoul", 25),
            new("CHAR_Undead_Priest", 27),
            new("CHAR_Undead_Ghoul_TombSummon", 30),
            new("CHAR_Undead_FlyingSkull", 32),
            new("CHAR_Undead_Assassin", 35),
            new("CHAR_Undead_ArmoredSkeletonCrossbow_Dunley", 38),
            new("CHAR_Undead_SkeletonGolem", 38),
            new("CHAR_Undead_Ghoul_Armored_Farmlands", 40),
            new("CHAR_Undead_SkeletonSoldier_Armored_Dunley", 40),
            new("CHAR_Undead_SkeletonSoldier_Infiltrator", 40),
            new("CHAR_Undead_Guardian", 42),
            new("CHAR_Undead_Necromancer", 46),
            new("CHAR_Undead_Necromancer_TombSummon", 46),
            new("CHAR_Undead_SkeletonMage", 44),
            new("CHAR_Unholy_Baneling", 58),
            new("CHAR_Undead_CursedSmith_FloatingWeapon_Base", 60),
            new("CHAR_Undead_CursedSmith_FloatingWeapon_Mace", 60),
            new("CHAR_Undead_CursedSmith_FloatingWeapon_Slashers", 60),
            new("CHAR_Undead_CursedSmith_FloatingWeapon_Spear", 60),
            new("CHAR_Undead_CursedSmith_FloatingWeapon_Sword", 60),
            new("CHAR_Undead_ShadowSoldier", 60), // hit and disappear
            new("CHAR_Undead_SkeletonSoldier_Base", 60),
            new("CHAR_Undead_GhostMilitia_Crossbow", 63),
            new("CHAR_Undead_GhostMilitia_Light", 63),
            new("CHAR_Undead_ZealousCultist_Ghost", 64),
            new("CHAR_Undead_GhostAssassin", 65),
            new("CHAR_Undead_GhostBanshee", 65),
            new("CHAR_Undead_GhostBanshee_TombSummon", 65),
            new("CHAR_Undead_GhostGuardian", 65),
            // "CHAR_Undead_BishopOfDeath_VBlood", // 27
            // "CHAR_Undead_BishopOfShadows_VBlood", // 47
            // "CHAR_Undead_CursedSmith_VBlood", // 65
            // "CHAR_Undead_Infiltrator_VBlood", // 47
            // "CHAR_Undead_Leader_Vblood", // 44
            // "CHAR_Undead_Priest_VBlood", // 35
            // "CHAR_Undead_ZealousCultist_VBlood",
        };

        private static Unit[] werewolves = {
            new("CHAR_Werewolf", 62),
            new("CHAR_WerewolfChieftain_VBlood", 64),
        };
        
        private static Unit[] winter = {
            new("CHAR_Winter_Wolf", 50),
            new("CHAR_Winter_Moose", 52),
            new("CHAR_Winter_Bear_Standard", 54),
            // "CHAR_Wendigo_VBlood", // 57
            // "CHAR_Winter_Yeti_VBlood" // 74
        };

        // Units friendly to players
        private static Unit[] servants = {
            new("CHAR_Bandit_Bomber_Servant", 32),
            new("CHAR_Bandit_Deadeye_Servant", 26),
            new("CHAR_Bandit_Hunter_Servant", 16),
            new("CHAR_Bandit_Miner_Standard_Servant", 14),
            new("CHAR_Bandit_Mugger_Servant", 20),
            new("CHAR_Bandit_Stalker_Servant", 30),
            new("CHAR_Bandit_Thief_Servant", 18),
            new("CHAR_Bandit_Thug_Servant", 16),
            new("CHAR_Bandit_Trapper_Servant", 20),
            new("CHAR_Bandit_Woodcutter_Standard_Servant", 14),
            new("CHAR_Bandit_Worker_Gatherer_Servant", 14),
            new("CHAR_ChurchOfLight_Archer_Servant", 56),
            new("CHAR_ChurchOfLight_Cleric_Servant", 62),
            new("CHAR_ChurchOfLight_Footman_Servant", 62),
            new("CHAR_ChurchOfLight_Knight_2H_Servant", 68),
            new("CHAR_ChurchOfLight_Knight_Shield_Servant", 68),
            new("CHAR_ChurchOfLight_Lightweaver_Servant", 72),
            new("CHAR_ChurchOfLight_Miner_Standard_Servant", 42),
            new("CHAR_ChurchOfLight_Paladin_Servant", 74),
            new("CHAR_ChurchOfLight_Priest_Servant", 74),
            new("CHAR_ChurchOfLight_Rifleman_Servant", 62),
            new("CHAR_ChurchOfLight_SlaveMaster_Enforcer_Servant", 64),
            new("CHAR_ChurchOfLight_SlaveMaster_Sentry_Servant", 64),
            new("CHAR_ChurchOfLight_SlaveRuffian_Servant", 60),
            new("CHAR_ChurchOfLight_Villager_Female_Servant", 50),
            new("CHAR_ChurchOfLight_Villager_Male_Servant", 50),
            new("CHAR_Farmlands_Farmer_Servant", 34),
            new("CHAR_Farmlands_Nun_Servant", 46),
            new("CHAR_Farmlands_Villager_Female_Servant", 26),
            new("CHAR_Farmlands_Villager_Female_Sister_Servant", 20),
            new("CHAR_Farmlands_Villager_Male_Servant", 26),
            new("CHAR_Farmlands_Woodcutter_Standard_Servant", 34),
            new("CHAR_Militia_BellRinger_Servant", 36),
            new("CHAR_Militia_Bomber_Servant", 47),
            new("CHAR_Militia_Crossbow_Servant", 36),
            new("CHAR_Militia_Devoted_Servant", 56),
            new("CHAR_Militia_Guard_Servant", 40),
            new("CHAR_Militia_Heavy_Servant", 54),
            new("CHAR_Militia_Light_Servant", 36),
            new("CHAR_Militia_Torchbearer_Servant", 36),
            new("CHAR_Militia_Longbowman_Servant", 42),
            new("CHAR_Militia_Miner_Standard_Servant", 40),
            new("CHAR_Gloomrot_AceIncinerator_Servant", 72),
            new("CHAR_Gloomrot_Batoon_Servant", 58),
            new("CHAR_Gloomrot_Pyro_Servant", 56),
            new("CHAR_Gloomrot_Railgunner_Servant", 58),
            new("CHAR_Gloomrot_SentryOfficer_Servant", 60),
            new("CHAR_Gloomrot_Tazer_Servant", 58),
            new("CHAR_Gloomrot_Technician_Labworker_Servant", 58),
            new("CHAR_Gloomrot_Technician_Servant", 58),
            new("CHAR_Gloomrot_TractorBeamer_Servant", 58),
            new("CHAR_Gloomrot_Villager_Female_Servant", 50),
            new("CHAR_Gloomrot_Villager_Male_Servant", 50),
            new("CHAR_NecromancyDagger_SkeletonBerserker_Armored_Farbane", 20), // Giant skeleton - friendly
            new("CHAR_Paladin_FallenAngel", 80),
            new("CHAR_Spectral_Guardian", 1),
            new("CHAR_Spectral_SpellSlinger", 60),
            new("CHAR_Unholy_DeathKnight", 60),
            new("CHAR_Unholy_FallenAngel", 0), // Why is this at 0?
        };

        private static EntityManager entityManager = Plugin.Server.EntityManager;
        private static Entity empty_entity = new();
        private static Random generate = new();
        private static bool showDebugLogs = true;

        public static ref Unit[] GetGroupUnits(int group) {
            switch (group) {
                case 0:
                    return ref bandit_units;
                case 1:
                    return ref church;
                case 2:
                    return ref church_elite;
                case 3:
                    return ref cultist_units;
                case 4:
                    return ref cursed_units;
                case 5:
                    return ref farmlands;
                case 6:
                    return ref forest;
                case 7:
                    return ref gloomrot;
                case 8:
                    return ref harpy;
                case 9:
                    return ref militia_units;
                case 10:
                    return ref vhunter;
                case 11:
                    return ref wtf;
                case 12:
                    return ref spiders;
                case 13:
                    return ref golems;
                case 14:
                    return ref mutants;
                case 15:
                    return ref undead_minions;
                case 16:
                    return ref werewolves;
                case 17:
                    return ref winter;
                case 18:
                    return ref servants;
            }

            return ref bandit_units;
        }

        private static ArraySegment<Unit> GetUnitsForLevel(Unit[] units, int playerLevel) {
            var maxUnitLevel = playerLevel + 10;
            // Assuming that any unit array has at least 1 element...
            var i = 1;
            // This assumes that the units are in level order...
            for (; i < units.Length && units[i].level > maxUnitLevel; i++) { }
            return new ArraySegment<Unit>(units, 0, i);
        }
        // This should only really handle the "active" factions
        private static ArraySegment<Unit> GetFactionUnits(Faction.Type faction, int playerLevel, int wantedLevel, bool vblood) {
            switch (faction) {
                case Faction.Type.Bandit:
                    return GetUnitsForLevel(bandit_units, playerLevel);
                case Faction.Type.Undead:
                    return GetUnitsForLevel(undead_minions, playerLevel);
                case Faction.Type.Farmland:
                    // Just because this would be hilarious
                    if (generate.Next(100) < 5) {
                        return new ArraySegment<Unit>(farmFood);
                    }
                    if (playerLevel >= 42 && wantedLevel > 3) {
                        return GetUnitsForLevel(church, playerLevel);
                    } else if (wantedLevel > 1) {
                        return GetUnitsForLevel(militia_units, playerLevel);
                    }
                    return GetUnitsForLevel(farmlands, playerLevel);
                case Faction.Type.HumanCleric:
                    return GetUnitsForLevel(church, playerLevel);
                case Faction.Type.HumanGloom:
                    return GetUnitsForLevel(gloomrot, playerLevel);
                case Faction.Type.Bears:
                case Faction.Type.Cursed:
                case Faction.Type.Fauna:
                case Faction.Type.Flora:
                case Faction.Type.Forest:
                case Faction.Type.Geomancer:
                case Faction.Type.Harpy:
                case Faction.Type.Horse:
                case Faction.Type.Merchant:
                case Faction.Type.Mutant:
                case Faction.Type.PCSummoned:
                case Faction.Type.Prisoner:
                case Faction.Type.RockElemental:
                case Faction.Type.ShadyMerchants:
                case Faction.Type.Town:
                case Faction.Type.Unknown:
                case Faction.Type.VampireHunter:
                case Faction.Type.Werewolves:
                case Faction.Type.Winter:
                case Faction.Type.Wolves:
                default:
                    Plugin.Logger.LogWarning($"{Enum.GetName(faction)} units not yet suppported");
                    return GetUnitsForLevel(bandit_units, playerLevel);
            }
        }

        public static void SpawnSquad(Entity player, Faction.Type faction, int wantedLevel) {
            var steamID = entityManager.GetComponentData<User>(player).PlatformId;
            var playerLevel = 0;
            if (Database.player_experience.TryGetValue(steamID, out int exp))
            {
                playerLevel = ExperienceSystem.convertXpToLevel(exp);
            }
            var units = GetFactionUnits(faction, playerLevel, wantedLevel, false);

            var remainingSquadValue = wantedLevel * 5;

            var position = entityManager.GetComponentData<LocalToWorld>(player).Position;
            while (remainingSquadValue > 0)
            {
                var unit = units[generate.Next(0, units.Count)];
                var unitValue = Math.Max(unit.level - playerLevel, 1);
                var possibleSpawnLimit = Math.Max(remainingSquadValue / unitValue, 1);
                var unitSpawn = generate.Next(1, possibleSpawnLimit);

                var isFound = Prefabs.Units.TryGetValue(unit.name, out var unitPrefabGuid);
                if (isFound) {
                    Plugin.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unitPrefabGuid, position,
                        unitSpawn, 1, 5, HunterHuntedSystem.ambush_despawn_timer);
                    remainingSquadValue -= unitSpawn * unitValue;
                    if (showDebugLogs) Plugin.Logger.LogWarning($"Spawning: {unitSpawn}*{unit.name}");
                }
                else {
                    Plugin.Logger.LogWarning($"Unit not found in database: {unit.name}");
                }
            }
        }
        
        public static void SpawnSquad(Entity player, int group, int maxUnits)
        {
            var remainingUnits = maxUnits;
        
            var f3pos = entityManager.GetComponentData<LocalToWorld>(player).Position;
            while (remainingUnits > 0)
            {
                var unitSpawn = generate.Next(1, Math.Min(remainingUnits, 3));
                var units = GetGroupUnits(group);
                var unitName = units[generate.Next(0, units.Length)].name;
        
                var isFound = Prefabs.Units.TryGetValue(unitName, out var unit);
                if (isFound) {
                    Plugin.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, f3pos,
                        unitSpawn, 1, 5, HunterHuntedSystem.ambush_despawn_timer);
                    remainingUnits -= unitSpawn;
                    if (showDebugLogs) Plugin.Logger.LogWarning($"Spawning: {unitName}");
                }
                else {
                    Plugin.Logger.LogWarning($"Unit not found in database: {unitName}");
                }
            }
        }
    }
}
