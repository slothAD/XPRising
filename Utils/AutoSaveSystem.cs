using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using BepInEx.Logging;
using OpenRPG.Systems;
using ProjectM;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Utils
{
    //-- AutoSave is now directly hooked into the Server game save activity.
    public static class AutoSaveSystem
    {
        // Config paths
        public static readonly string BasePath = Paths.ConfigPath ?? Path.Combine("BepInEx", "config");
        public static readonly string ConfigPath = Path.Combine(BasePath, "OpenRPG");
        public static readonly string SavesPath = Path.Combine(ConfigPath, "Data");
        public static readonly string BackupsPath = Path.Combine(SavesPath, "Backup");
        
        // Config files
        public static readonly string PowerUpJson = "power_up.json";
        public static readonly string WaypointCountJson = "waypoint_count.json";
        public static readonly string GlobalWaypointsJson = "waypoints.json";
        public static readonly string WeaponMasteryJson = "weapon_mastery.json";
        public static readonly string PlayerLogoutJson = "player_logout.json";
        public static readonly string WeaponMasteryConfigJson = "weapon_mastery_config.json";
        public static readonly string PlayerLogConfigJson = "playerLogConfig.json";
        public static readonly string BloodlinesJson = "bloodlines.json";
        public static readonly string BloodlineConfigJson = "bloodline_config.json";
        public static readonly string PlayerExperienceJson = "player_experience.json";
        public static readonly string PlayerAbilityPointsJson = "player_ability_points.json";
        public static readonly string PlayerLevelStatsJson = "player_level_stats.json";
        public static readonly string ExperienceClassStatsJson = "experience_class_stats.json";
        public static readonly string CommandPermissionJson = "command_permission.json";
        public static readonly string UserPermissionJson = "user_permission.json";
        
        private static int _saveCount = 0;
        private static int _autoSaveCount = 0;
        public static int AutoSaveFrequency = 1;
        public static int BackupFrequency = 0;
        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            IncludeFields = true,
            Converters =
            {
                new PrefabGuidConverter()
            }
        };
        private static readonly JsonSerializerOptions PrettyJsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Converters =
            {
                new PrefabGuidConverter()
            }
        };

        private enum LoadMethod
        {
            Both,
            Main,
            Backup,
            None,
        }

        // Returns true on all succeeding, false on any errors
        public static bool SaveDatabase(bool forceSave, bool forceBackup = false)
        {
            var anyErrors = false;
            if (forceSave)
            {
                anyErrors |= !InternalSaveDatabase(SavesPath);
                if (forceBackup) anyErrors |= !InternalSaveDatabase(BackupsPath);
            }
            else
            {
                // TODO test time between logs so that we can a comment for it in the config
                Plugin.Log(LogSystem.Core, LogLevel.Info, "AUTO-SAVING NOW: TODO REMOVE THIS", true);
                
                var autoSave = _saveCount % AutoSaveFrequency == 0;
                if (autoSave)
                {
                    anyErrors |= !InternalSaveDatabase(SavesPath);
                    var saveBackup = _autoSaveCount % BackupFrequency == 0;
                    if (forceBackup || saveBackup) anyErrors |= !InternalSaveDatabase(BackupsPath);
                    
                    // Just ensure that it wraps around. No need to support ludicrously high save count numbers
                    _autoSaveCount = (_autoSaveCount + 1) % 100;
                }
                
                // Just ensure that it wraps around. No need to support ludicrously high save count numbers
                _saveCount = (_saveCount + 1) % 100;
            }

            return !anyErrors;
        }
        
        // Returns true on all succeeding, false on any errors
        private static bool InternalSaveDatabase(string saveFolder)
        {
            var anyErrors = false;
            anyErrors |= !SaveDB(saveFolder, CommandPermissionJson, Database.command_permission, PrettyJsonOptions);
            anyErrors |= !SaveDB(saveFolder, UserPermissionJson, Database.user_permission, PrettyJsonOptions);
            anyErrors |= !SaveDB(saveFolder, WaypointCountJson, Database.waypoints_owned, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, GlobalWaypointsJson, Database.waypoints, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, PowerUpJson, Database.PowerUpList, JsonOptions);

            //-- System Related
            anyErrors |= !SaveDB(saveFolder, PlayerLogoutJson, Database.player_logout, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, PlayerExperienceJson, Database.player_experience, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, PlayerLogConfigJson, Database.playerLogConfig, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, PlayerAbilityPointsJson, Database.player_abilityIncrease, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, PlayerLevelStatsJson, Database.player_level_stats, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, ExperienceClassStatsJson, Database.experience_class_stats, PrettyJsonOptions);
            anyErrors |= !SaveDB(saveFolder, WeaponMasteryJson, Database.player_weaponmastery, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, WeaponMasteryConfigJson, Database.masteryStatConfig, PrettyJsonOptions);
            anyErrors |= !SaveDB(saveFolder, BloodlinesJson, Database.playerBloodline, JsonOptions);
            anyErrors |= !SaveDB(saveFolder, BloodlineConfigJson, Database.bloodlineStatConfig, PrettyJsonOptions);

            Plugin.Log(LogSystem.Core, LogLevel.Info, $"All databases saved to: {saveFolder}");
            return !anyErrors;
        }

        public static void LoadOrInitialiseDatabase()
        {
            InternalLoadDatabase(true, LoadMethod.Both);
        }

        public static bool LoadDatabase(bool loadBackup)
        {
            return InternalLoadDatabase(false, loadBackup ? LoadMethod.Backup : LoadMethod.Main);
        }

        public static bool WipeDatabase()
        {
            return InternalLoadDatabase(true, LoadMethod.None);
        }
        
        private static bool InternalLoadDatabase(bool useInitialiser, LoadMethod loadMethod)
        {
            var anyErrors = false;
            //-- Commands Related
            Database.command_permission = LoadDB(CommandPermissionJson, loadMethod, useInitialiser, Database.command_permission, out var success, PermissionSystem.DefaultCommandPermissions);
            anyErrors |= !success;
            Database.user_permission = LoadDB(UserPermissionJson, loadMethod, useInitialiser, Database.user_permission, out success);
            anyErrors |= !success;
            Database.waypoints_owned = LoadDB(WaypointCountJson, loadMethod, useInitialiser, Database.waypoints_owned, out success);
            anyErrors |= !success;
            Database.waypoints = LoadDB(GlobalWaypointsJson, loadMethod, useInitialiser, Database.waypoints, out success);
            anyErrors |= !success;
            Database.PowerUpList = LoadDB(PowerUpJson, loadMethod, useInitialiser, Database.PowerUpList, out success);
            anyErrors |= !success;

            //-- System Related
            Database.player_logout = LoadDB(PlayerLogoutJson, loadMethod, useInitialiser, Database.player_logout, out success);
            anyErrors |= !success;
            Database.player_experience = LoadDB(PlayerExperienceJson, loadMethod, useInitialiser, Database.player_experience, out success);
            anyErrors |= !success;
            Database.playerLogConfig = LoadDB(PlayerLogConfigJson, loadMethod, useInitialiser, Database.playerLogConfig, out success);
            anyErrors |= !success;
            Database.player_abilityIncrease = LoadDB(PlayerAbilityPointsJson, loadMethod, useInitialiser, Database.player_abilityIncrease, out success);
            anyErrors |= !success;
            Database.player_level_stats = LoadDB(PlayerLevelStatsJson, loadMethod, useInitialiser, Database.player_level_stats, out success);
            anyErrors |= !success;
            Database.experience_class_stats = LoadDB(ExperienceClassStatsJson, loadMethod, useInitialiser, Database.experience_class_stats, out success, ExperienceSystem.DefaultExperienceClassStats);
            anyErrors |= !success;
            Database.player_weaponmastery = LoadDB(WeaponMasteryJson, loadMethod, useInitialiser, Database.player_weaponmastery, out success);
            anyErrors |= !success;
            Database.masteryStatConfig = LoadDB(WeaponMasteryConfigJson, loadMethod, useInitialiser, Database.masteryStatConfig, out success, WeaponMasterySystem.DefaultMasteryConfig);
            anyErrors |= !success;
            Database.playerBloodline = LoadDB(BloodlinesJson, loadMethod, useInitialiser, Database.playerBloodline, out success);
            anyErrors |= !success;
            Database.bloodlineStatConfig = LoadDB(BloodlineConfigJson, loadMethod, useInitialiser, Database.bloodlineStatConfig, out success, BloodlineSystem.DefaultBloodlineConfig);
            anyErrors |= !success;

            Plugin.Log(LogSystem.Core, LogLevel.Info, "All database data is now loaded.", true);
            return !anyErrors;
        }
        
        
        
        private static bool SaveDB<TData>(string saveFolder, string specificFile, TData data, JsonSerializerOptions options)
        {
            try
            {
                var outputFile = Path.Combine(saveFolder, specificFile);
                File.WriteAllText(outputFile, JsonSerializer.Serialize(data, options));
                Plugin.Log(LogSystem.Core, LogLevel.Info, $"{specificFile} Saved.");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log(LogSystem.Core, LogLevel.Error, $"Could not save DB {specificFile}: {e.Message}", true);
                return false;
            }
        }

        private static TData LoadDB<TData>(string specificFile, LoadMethod loadMethod, bool useInitialiser, TData currentRef, out bool success, Func<TData> initialiser = null) where TData : class, new()
        {
            success = false;
            TData data;
            switch (loadMethod)
            {
                case LoadMethod.Main:
                    if (LoadDB(SavesPath, specificFile, out data))
                    {
                        success = true;
                        return data;
                    }
                    break;
                case LoadMethod.Backup:
                    if (LoadDB(BackupsPath, specificFile, out data))
                    {
                        success = true;
                        return data;
                    }
                    break;
                case LoadMethod.Both:
                default:
                    // attempt to load the main save first
                    if (LoadDB(SavesPath, specificFile, out data) ||
                        LoadDB(BackupsPath, specificFile, out data))
                    {
                        success = true;
                        return data;
                    }
                    break;
                case LoadMethod.None:
                    success = true;
                    Plugin.Log(LogSystem.Core, LogLevel.Info, $"Initialising DB for {specificFile}");
                    return initialiser == null ? new TData() : initialiser();
            }

            // If nothing loaded correctly, check if we should use the initialiser or just return the current value.
            if (!useInitialiser) return currentRef;
            
            Plugin.Log(LogSystem.Core, LogLevel.Warning, $"Initialising DB for {specificFile}");
            return initialiser == null ? new TData() : initialiser();
        }

        private static bool LoadDB<TData>(string folder, string specificFile, out TData data) where TData : class, new()
        {
            // Default JSON content to valid json so that they have a chance to be serialised without errors when loading for the first time.
            var genericType = typeof(TData);
            var isJsonListData = genericType.IsGenericType &&
                                 (genericType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
                                  genericType.GetGenericTypeDefinition().IsAssignableFrom(typeof(HashSet<>)));
            var defaultContents = isJsonListData ? "[]" : "{}";
            try {
                var saveFile = ConfirmFile(folder, specificFile, defaultContents);
                var json = File.ReadAllText(saveFile);
                data = JsonSerializer.Deserialize<TData>(json, JsonOptions);
                Plugin.Log(LogSystem.Core, LogLevel.Info, $"Main DB Loaded for {specificFile}");
                return true;
            } catch (Exception e) {
                Plugin.Log(LogSystem.Core, LogLevel.Error, $"Could not load main {specificFile}: {e.Message}", true);
                data = new TData();
                return false;
            }
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
