using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using BepInEx.Logging;
using OpenRPG.Commands;
using OpenRPG.Models;
using OpenRPG.Systems;
using ProjectM;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Utils
{
    using WeaponMasteryData = LazyDictionary<WeaponMasterySystem.MasteryType, MasteryData>;
    using BloodlineMasteryData = LazyDictionary<BloodlineSystem.BloodType, MasteryData>;
    
    //-- AutoSave is now directly hooked into the Server game save activity.
    public static class AutoSaveSystem
    {
        // Config paths
        public static readonly string BasePath = Paths.ConfigPath ?? Path.Combine("BepInEx", "config");
        public static readonly string ConfigPath = Path.Combine(BasePath, "OpenRPG");
        public static readonly string SavesPath = Path.Combine(ConfigPath, "Data");
        public static readonly string BackupsPath = Path.Combine(SavesPath, "Backup");
        
        // Config files
        public static readonly string GodModeJson = "god_mode.json";
        public static readonly string KitsJson = "kits.json";
        public static readonly string NoCooldownJson = "no_cooldown.json";
        public static readonly string PowerUpJson = "power_up.json";
        public static readonly string SpeedingJson = "speeding.json";
        public static readonly string SunImmunityJson = "sun_immunity.json";
        public static readonly string WaypointCountJson = "waypoint_count.json";
        public static readonly string GlobalWaypointsJson = "waypoints.json";
        public static readonly string WorldDynamicsJson = "world_dynamics.json";
        public static readonly string IgnoredMonstersJson = "ignored_monsters.json";
        public static readonly string UserBanList = "user_banlist.json";
        public static readonly string WeaponMasteryJson = "weapon_mastery.json";
        public static readonly string PlayerLogoutJson = "player_logout.json";
        public static readonly string WeaponMasteryConfigJson = "weapon_mastery_config.json";
        public static readonly string PlayerLogMasteryJson = "player_log_mastery.json";
        public static readonly string BloodlinesJson = "bloodlines.json";
        public static readonly string BloodlineConfigJson = "bloodline_config.json";
        public static readonly string PlayerLogBloodlinesJson = "player_log_bloodlines.json";
        public static readonly string PlayerExperienceJson = "player_experience.json";
        public static readonly string PlayerLogExperienceJson = "player_log_exp.json";
        public static readonly string PlayerAbilityPointsJson = "player_ability_points.json";
        public static readonly string PlayerLevelStatsJson = "player_level_stats.json";
        public static readonly string ExperienceClassStatsJson = "experience_class_stats.json";
        public static readonly string CommandPermissionJson = "command_permission.json";
        public static readonly string UserPermissionJson = "user_permission.json";
        
        private static int saveCount = 0;
        public static int backupFrequency = 5;
        
        private static JsonSerializerOptions JSON_options = new()
        {
            WriteIndented = false,
            IncludeFields = true,
            Converters =
            {
                new PrefabGuidConverter()
            }
        };
        private static JsonSerializerOptions Pretty_JSON_options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Converters =
            {
                new PrefabGuidConverter()
            }
        };
        
        public static void SaveDatabase()
        {
            saveCount++;
            var saveBackup = saveCount % backupFrequency == 0;
            var saveFolder = saveBackup ? BackupsPath : SavesPath;
            
            SaveDB(saveFolder, CommandPermissionJson, Database.command_permission, Pretty_JSON_options);
            SaveDB(saveFolder, UserPermissionJson, Database.user_permission, Pretty_JSON_options);
            SaveDB(saveFolder, SunImmunityJson, Database.sunimmunity, JSON_options);
            SaveDB(saveFolder, WaypointCountJson, Database.waypoints_owned, JSON_options);
            SaveDB(saveFolder, GlobalWaypointsJson, Database.waypoints, JSON_options);
            SaveDB(saveFolder, NoCooldownJson, Database.nocooldownlist, JSON_options);
            SaveDB(saveFolder, GodModeJson, Database.godmode, JSON_options);
            SaveDB(saveFolder, SpeedingJson, Database.speeding, JSON_options);
            SaveDB(saveFolder, KitsJson, Database.kits, JSON_options);
            SaveDB(saveFolder, PowerUpJson, Database.PowerUpList, JSON_options);

            //-- System Related
            SaveDB(saveFolder, PlayerLogoutJson, Database.player_logout, JSON_options);
            SaveDB(saveFolder, PlayerExperienceJson, Database.player_experience, JSON_options);
            SaveDB(saveFolder, PlayerLogExperienceJson, Database.player_log_exp, JSON_options);
            SaveDB(saveFolder, PlayerAbilityPointsJson, Database.player_abilityIncrease, JSON_options);
            SaveDB(saveFolder, PlayerLevelStatsJson, Database.player_level_stats, JSON_options);
            SaveDB(saveFolder, ExperienceClassStatsJson, Database.experience_class_stats, Pretty_JSON_options);
            SaveDB(saveFolder, WeaponMasteryJson, Database.player_weaponmastery, JSON_options);
            SaveDB(saveFolder, WeaponMasteryConfigJson, Database.masteryStatConfig, Pretty_JSON_options);
            SaveDB(saveFolder, PlayerLogMasteryJson, Database.player_log_mastery, JSON_options);
            SaveDB(saveFolder, BloodlinesJson, Database.playerBloodline, JSON_options);
            SaveDB(saveFolder, BloodlineConfigJson, Database.bloodlineStatConfig, Pretty_JSON_options);
            SaveDB(saveFolder, PlayerLogBloodlinesJson, Database.playerLogBloodline, JSON_options);
            SaveDB(saveFolder, UserBanList, Database.user_banlist, Pretty_JSON_options);
            SaveDB(saveFolder, WorldDynamicsJson, Database.FactionStats, Pretty_JSON_options);
            SaveDB(saveFolder, IgnoredMonstersJson, Database.IgnoredMonstersGUID, JSON_options);

            Plugin.Log(LogSystem.Core, LogLevel.Info, $"All databases saved to: {saveFolder}");
        }

        public static void LoadDatabase()
        {
            //-- Commands Related
            Database.command_permission = LoadDB(CommandPermissionJson, PermissionSystem.DefaultCommandPermissions);
            Database.user_permission = LoadDB<Dictionary<ulong, int>>(UserPermissionJson);
            Database.sunimmunity = LoadDB<Dictionary<ulong, bool>>(SunImmunityJson);
            Database.waypoints_owned = LoadDB<Dictionary<ulong, int>>(WaypointCountJson);
            Database.waypoints = LoadDB<Dictionary<string, WaypointData>>(GlobalWaypointsJson);
            Database.godmode = LoadDB<Dictionary<ulong, bool>>(GodModeJson);
            Database.speeding = LoadDB<Dictionary<ulong, bool>>(SpeedingJson);
            Database.nocooldownlist = LoadDB<Dictionary<ulong, bool>>(NoCooldownJson);
            Database.kits = LoadDB(KitsJson, Kit.DefaultItemKit);
            Database.PowerUpList = LoadDB<Dictionary<ulong, PowerUpData>>(PowerUpJson);

            //-- System Related
            Database.player_logout = LoadDB<Dictionary<ulong, DateTime>>(PlayerLogoutJson);
            Database.player_experience = LoadDB<Dictionary<ulong, int>>(PlayerExperienceJson);
            Database.player_log_exp = LoadDB<Dictionary<ulong, bool>>(PlayerLogExperienceJson);
            Database.player_abilityIncrease = LoadDB<Dictionary<ulong, int>>(PlayerAbilityPointsJson);
            Database.player_level_stats = LoadDB<LazyDictionary<ulong, LazyDictionary<UnitStatType,float>>>(PlayerLevelStatsJson);
            Database.experience_class_stats = LoadDB(ExperienceClassStatsJson, ExperienceSystem.DefaultExperienceClassStats);
            Database.player_weaponmastery = LoadDB<LazyDictionary<ulong, WeaponMasteryData>>(WeaponMasteryJson);
            Database.masteryStatConfig = LoadDB(WeaponMasteryConfigJson, WeaponMasterySystem.DefaultMasteryConfig);
            Database.player_log_mastery = LoadDB<Dictionary<ulong, bool>>(PlayerLogMasteryJson);
            Database.playerBloodline = LoadDB<LazyDictionary<ulong, BloodlineMasteryData>>(BloodlinesJson);
            Database.bloodlineStatConfig = LoadDB(BloodlineConfigJson, BloodlineSystem.DefaultBloodlineConfig);
            Database.playerLogBloodline = LoadDB<Dictionary<ulong, bool>>(PlayerLogBloodlinesJson);
            Database.user_banlist = LoadDB<Dictionary<ulong, BanData>>(UserBanList);
            Database.FactionStats = LoadDB(WorldDynamicsJson, WorldDynamicsSystem.DefaultFactionStats);
            Database.IgnoredMonstersGUID = LoadDB(IgnoredMonstersJson, WorldDynamicsSystem.DefaultIgnoredMonsters);

            Plugin.Log(LogSystem.Core, LogLevel.Info, "All database data is now loaded.", true);
        }
        
        private static void SaveDB<TData>(string saveFolder, string specificFile, TData data, JsonSerializerOptions options)
        {
            try
            {
                var outputFile = Path.Combine(saveFolder, specificFile);
                File.WriteAllText(outputFile, JsonSerializer.Serialize(data, options));
                Plugin.Log(LogSystem.Core, LogLevel.Info, $"{specificFile} Saved.");
            }
            catch (Exception e)
            {
                Plugin.Log(LogSystem.Core, LogLevel.Error, $"Could not save DB {specificFile}: {e.Message}", true);
            }
        }

        private static TData LoadDB<TData>(string specificFile, Func<TData> initialiser = null) where TData : class, new()
        {
            // Default JSON content to valid json so that they have a chance to be serialised without errors when loading for the first time.
            var genericType = typeof(TData);
            var isJsonListData = genericType.IsGenericType &&
                                 (genericType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
                                  genericType.GetGenericTypeDefinition().IsAssignableFrom(typeof(HashSet<>)));
            var defaultContents = isJsonListData ? "[]" : "{}";
            try {
                var saveFile = ConfirmFile(SavesPath, specificFile, defaultContents);
                var json = File.ReadAllText(saveFile);
                var data = JsonSerializer.Deserialize<TData>(json, JSON_options);
                Plugin.Log(LogSystem.Core, LogLevel.Info, $"Main DB Loaded for {specificFile}");
                return data;
            } catch (Exception e) {
                Plugin.Log(LogSystem.Core, LogLevel.Error, $"Could not load main {specificFile}: {e.Message}", true);
            }
            
            try {
                var backupFile = ConfirmFile(BackupsPath, specificFile, defaultContents);
                var json = File.ReadAllText(backupFile);
                var data = JsonSerializer.Deserialize<TData>(json, JSON_options);
                Plugin.Log(LogSystem.Core, LogLevel.Info, $"Backup DB Loaded for {specificFile}");
                return data;
            } catch (Exception e) {
                Plugin.Log(LogSystem.Core, LogLevel.Error, $"Could not load backup {specificFile}: {e.Message}", true);
            }
            
            Plugin.Log(LogSystem.Core, LogLevel.Warning, $"Initialising DB for {specificFile}");
            return initialiser == null ? new TData() : initialiser();
        }
        
        public static string ConfirmFile(string address, string file, string defaultContents = "") {
            try {
                Directory.CreateDirectory(address);
            }
            catch (Exception e) {
                throw new Exception("Error creating directory at " + address + "\n Error is: " + e.Message);
            }
            var fileAddress = Path.Combine(address, file);
            try
            {
                // If the file does not exist, create a new empty file there
                if (!File.Exists(fileAddress))
                {
                    File.WriteAllText(fileAddress, defaultContents);
                }
            } catch (Exception e) {
                throw new Exception("Error creating file at " + fileAddress + "\n Error is: " + e.Message);
            }

            return fileAddress;
        }
    }
    
    public class PrefabGuidConverter : JsonConverter<PrefabGUID>
    {
        public override PrefabGUID Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            new PrefabGUID(reader.GetInt32()!);

        public override void Write(
            Utf8JsonWriter writer,
            PrefabGUID guid,
            JsonSerializerOptions options) =>
            writer.WriteNumberValue(guid.GuidHash);
    }
}
