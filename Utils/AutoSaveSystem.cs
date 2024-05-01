using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using OpenRPG.Systems;
using ProjectM;

namespace OpenRPG.Utils
{
    //-- AutoSave is now directly hooked into the Server game save activity.
    public static class AutoSaveSystem
    {
        // Config paths
        public static readonly string BasePath = Paths.ConfigPath ?? Path.Combine("BepInEx", "config");
        public static readonly string ConfigPath = Path.Combine(BasePath, "OpenRPG");
        public static readonly string SavesPath = Path.Combine(ConfigPath, "Saves");
        public static readonly string BackupsPath = Path.Combine(SavesPath, "Backup");
        
        // Config files
        public static readonly string AutoRespawnJson = "auto_respawn.json";
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
        public static readonly string WeaponMasteryDecayJson = "mastery_decay.json";
        public static readonly string PlayerLogMasteryJson = "player_log_mastery.json";
        public static readonly string BloodlinesJson = "bloodlines.json";
        public static readonly string BloodlineDelayJson = "bloodline_decay.json";
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
        public static bool saveLogging = false;
        
        
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
            SaveDB(saveFolder, AutoRespawnJson, Database.autoRespawn, JSON_options);
            SaveDB(saveFolder, KitsJson, Database.kits, JSON_options);
            SaveDB(saveFolder, PowerUpJson, Database.PowerUpList, JSON_options);

            //-- System Related
            SaveDB(saveFolder, PlayerExperienceJson, Database.player_experience, JSON_options);
            SaveDB(saveFolder, PlayerLogExperienceJson, Database.player_log_exp, JSON_options);
            SaveDB(saveFolder, PlayerAbilityPointsJson, Database.player_abilityIncrease, JSON_options);
            SaveDB(saveFolder, PlayerLevelStatsJson, Database.player_level_stats, JSON_options);
            SaveDB(saveFolder, ExperienceClassStatsJson, Database.experience_class_stats, Pretty_JSON_options);
            SaveDB(saveFolder, WeaponMasteryJson, Database.player_weaponmastery, JSON_options);
            SaveDB(saveFolder, WeaponMasteryDecayJson, Database.player_decaymastery_logout, JSON_options);
            SaveDB(saveFolder, PlayerLogMasteryJson, Database.player_log_mastery, JSON_options);
            SaveDB(saveFolder, BloodlinesJson, Database.playerBloodline, JSON_options);
            SaveDB(saveFolder, BloodlineDelayJson, Database.playerDecayBloodlineLogout, JSON_options);
            SaveDB(saveFolder, PlayerLogBloodlinesJson, Database.playerLogBloodline, JSON_options);
            SaveDB(saveFolder, UserBanList, Database.user_banlist, Pretty_JSON_options);
            SaveDB(saveFolder, WorldDynamicsJson, Database.FactionStats, Pretty_JSON_options);
            SaveDB(saveFolder, IgnoredMonstersJson, Database.IgnoredMonstersGUID, JSON_options);

            Plugin.LogInfo($"All databases saved to: {saveFolder}");
        }

        public static void LoadDatabase()
        {
            //-- Commands Related
            Database.command_permission = LoadDB(CommandPermissionJson, PermissionSystem.DefaultCommandPermissions);
            Database.user_permission = LoadDB<Dictionary<ulong, int>>(UserPermissionJson);
            Database.sunimmunity = LoadDB<Dictionary<ulong, bool>>(SunImmunityJson);
            Database.waypoints_owned = LoadDB<Dictionary<ulong, int>>(WaypointCountJson);
            Database.waypoints = LoadDB<Dictionary<string, Tuple<float, float, float>>>(GlobalWaypointsJson);
            Database.godmode = LoadDB<Dictionary<ulong, bool>>(GodModeJson);
            Database.speeding = LoadDB<Dictionary<ulong, bool>>(SpeedingJson);
            Database.nocooldownlist = LoadDB<Dictionary<ulong, bool>>(NoCooldownJson);
            Database.autoRespawn = LoadDB<Dictionary<ulong, bool>>(AutoRespawnJson);
            Database.kits = LoadDB<List<ItemKit>>(KitsJson);
            Database.PowerUpList = LoadDB<Dictionary<ulong, PowerUpData>>(PowerUpJson);

            //-- System Related
            Database.player_experience = LoadDB<Dictionary<ulong, int>>(PlayerExperienceJson);
            Database.player_log_exp = LoadDB<Dictionary<ulong, bool>>(PlayerLogExperienceJson);
            Database.player_abilityIncrease = LoadDB<Dictionary<ulong, int>>(PlayerAbilityPointsJson);
            Database.player_level_stats = LoadDB<LazyDictionary<ulong, LazyDictionary<UnitStatType,float>>>(PlayerLevelStatsJson);
            Database.experience_class_stats = LoadDB(ExperienceClassStatsJson, ExperienceSystem.DefaultExperienceClassStats);
            Database.player_weaponmastery = LoadDB<Dictionary<ulong, WeaponMasterData>>(WeaponMasteryJson);
            Database.player_decaymastery_logout = LoadDB<Dictionary<ulong, DateTime>>(WeaponMasteryDecayJson);
            Database.player_log_mastery = LoadDB<Dictionary<ulong, bool>>(PlayerLogMasteryJson);
            Database.playerBloodline = LoadDB<Dictionary<ulong, BloodlineData>>(BloodlinesJson);
            Database.playerDecayBloodlineLogout = LoadDB<Dictionary<ulong, DateTime>>(BloodlineDelayJson);
            Database.playerLogBloodline = LoadDB<Dictionary<ulong, bool>>(PlayerLogBloodlinesJson);
            Database.user_banlist = LoadDB<Dictionary<ulong, BanData>>(UserBanList);
            Database.FactionStats = LoadDB(WorldDynamicsJson, WorldDynamicsSystem.DefaultFactionStats);
            Database.IgnoredMonstersGUID = LoadDB(IgnoredMonstersJson, WorldDynamicsSystem.DefaultIgnoredMonsters);

            Plugin.LogInfo("All database data is now loaded.");
        }
        
        private static void SaveDB<TData>(string saveFolder, string specificFile, TData data, JsonSerializerOptions options)
        {
            var outputFile = Path.Combine(saveFolder, specificFile);
            File.WriteAllText(outputFile, JsonSerializer.Serialize(data, options));
            if (saveLogging) Plugin.LogInfo($"{specificFile} Saved.");
        }

        private static TData LoadDB<TData>(string specificFile, Func<TData> initialiser = null) where TData : new() {
            TData data;
            ConfirmFile(SavesPath, specificFile);
            ConfirmFile(BackupsPath, specificFile);
            try {
                var json = File.ReadAllText(Path.Combine(SavesPath, specificFile));
                data = JsonSerializer.Deserialize<TData>(json, JSON_options);
                if (data == null) {
                    Plugin.LogWarning($"Failed loading main backup for {specificFile}, attempting loading backup");
                    json = File.ReadAllText(Path.Combine(BackupsPath, specificFile));
                    data = JsonSerializer.Deserialize<TData>(json, JSON_options);
                }
                if (saveLogging) Plugin.LogInfo($"DB Loaded for {specificFile}");
            } catch (Exception e) {
                Plugin.LogError($"Could not load {specificFile}: {e.Message}");
                data = initialiser == null ? new TData() : initialiser();
                Plugin.LogWarning($"DB Created for {specificFile}");
            }
            return data;
        }
        
        private static void ConfirmFile(string address, string file) {
            try {
                Directory.CreateDirectory(address);
            }
            catch (Exception e) {
                Plugin.LogError("Error creating directory at " + address + "\n Error is: " + e.Message);
            }
            try
            {
                var fileAddress = Path.Combine(address, file);
                // If the file does not exist, create a new empty file there
                if (!File.Exists(fileAddress)) {
                    var stream = File.Create(fileAddress);
                    stream.Dispose();
                }
            } catch (Exception e) {
                Plugin.LogError("Error creating file at " + address + "\n Error is: " + e.Message);
            }
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
