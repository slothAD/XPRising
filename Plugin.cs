using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using VampireCommandFramework;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using OpenRPG.Commands;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Entities;
using UnityEngine;
using VRising.GameData;
using OpenRPG.Configuration;
using OpenRPG.Components.RandomEncounters;
using ProjectM;

namespace OpenRPG
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin
    {
        public static Harmony harmony;

        internal static Plugin Instance { get; private set; }

        private static ConfigEntry<int> WaypointLimit;

        private static ConfigEntry<bool> EnableWorldDynamics;
        private static ConfigEntry<bool> WDGrowOnKill;

        private static ConfigEntry<int> buffID;
        private static ConfigEntry<int> forbiddenBuffID;
        private static ConfigEntry<int> appliedBuff;
        private static ConfigEntry<bool> humanReadablePercentageStats;
        private static ConfigEntry<bool> inverseMultiplersDisplayReduction;
        private static ConfigEntry<bool> disableCommandAdminRequirement;

        public static bool isInitialized = false;

        private static ManualLogSource _logger;
        private static World _serverWorld;
        public static World Server
        {
            get
            {
                if (_serverWorld != null) return _serverWorld;

                _serverWorld = GetWorld("Server")
                    ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
                return _serverWorld;
            }
        }

        public static bool IsServer => Application.productName == "VRisingServer";

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                {
                    return world;
                }
            }

            return null;
        }

        public void InitConfig()
        {
            // TODO move this config to the same place as the rest
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 2, "Set a waypoint limit for per non-admin user.");

            buffID = Config.Bind("Buff System", "Buff GUID", Helper.buffGUID, "The GUID of the buff you want to hijack for the buffs from mastery, bloodlines, and everything else from this mod\nDefault is now boneguard set bonus 2, 1409441911 is cloak, but you can set anything else too");
            forbiddenBuffID = Config.Bind("Buff System", "Forbidden Buff GUID", Helper.forbiddenBuffGUID, "The GUID of the buff that prohibits you from getting mastery buffs\nDefault is boneguard set bonus 1, so you cant double up.");
            appliedBuff = Config.Bind("Buff System", "Applied Buff", Helper.buffGUID, "The GUID of the buff that gets applied when mastery, bloodline, etc changes. Doesnt need to be the same as the Buff GUID.");
            humanReadablePercentageStats = Config.Bind("Buff System", "Human Readable Percentage Stats", false, "Determines if rates for percentage stats should be read as out of 100 instead of 1, off by default for compatability.");
            inverseMultiplersDisplayReduction = Config.Bind("Buff System", "Inverse Multipliers Display Reduction", true, "Determines if inverse multiplier stats display their reduction, or the final value.");

            EnableWorldDynamics = Config.Bind("World Dynamics", "Enable Faction Dynamics", false, $"All other faction dynamics data & config is within {AutoSaveSystem.WorldDynamicsJson} file.");
            WDGrowOnKill = Config.Bind("World Dynamics", "Factions grow on kill", false, "Inverts the faction dynamic system, so that they grow stronger when killed and weaker over time.");
            
            disableCommandAdminRequirement = Config.Bind("Admin", "Disable command admin requirement", false, "Disables all \"isAdmin\" checks for running commands.");
        }

        public override void Load()
        {
            // Ensure the logger is accessible in static contexts.
            _logger = base.Log;
            if(!IsServer)
            {
                Plugin.Log(LogSystem.Plugin, LogLevel.Warning, $"This is a server plugin. Not continuing to load on client.", true);
                return;
            }
            
            InitConfig();
            CommandRegistry.RegisterAll();
            GameData.OnInitialize += GameDataOnInitialize;
            GameData.OnDestroy += GameDataOnDestroy;
            Instance = this;
            RandomEncounters.Load();
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Plugin.Log(LogSystem.Plugin, LogLevel.Info, $"Plugin is loaded", true);
        }

        private static void GameDataOnInitialize(World world)
        {
            RandomEncounters.GameData_OnInitialize();
            RandomEncounters._encounterTimer = new Timer();
            if (RandomEncountersConfig.Enabled.Value)
            {
                RandomEncounters.StartEncounterTimer();
            }
            
            Initialize();
        }

        private static void GameDataOnDestroy()
        {
        }

        public override bool Unload()
        {
            Config.Clear();
            harmony.UnpatchSelf();
            return true;
        }

        public static void Initialize()
        {
            Plugin.Log(LogSystem.Plugin, LogLevel.Warning, $"Trying to Initialize {MyPluginInfo.PLUGIN_NAME}: isInitialized == {isInitialized}", isInitialized);
            if (isInitialized) return;
            Plugin.Log(LogSystem.Plugin, LogLevel.Info, $"Initializing {MyPluginInfo.PLUGIN_NAME}...", true);
            
            //-- Initialize System
            Helper.GetServerGameSettings(out Helper.SGS);
            Helper.GetServerGameManager(out Helper.SGM);
            Helper.GetUserActivityGridSystem(out Helper.UAGS);

            DebugLoggingConfig.Initialize();
            VipConfig.Initialize();
            WantedConfig.Initialize();
            ExperienceConfig.Initialize();
            MasteryConfig.Initialize();
            BloodlineConfig.Initialize();

            //-- Apply configs
            Waypoint.WaypointLimit = WaypointLimit.Value;
            
            Plugin.Log(LogSystem.Plugin, LogLevel.Info, "Loading world dynamics config");
            WorldDynamicsSystem.isFactionDynamic = EnableWorldDynamics.Value;
            WorldDynamicsSystem.growOnKill = WDGrowOnKill.Value;

            Helper.buffGUID = buffID.Value;
            Helper.AppliedBuff = new PrefabGUID(appliedBuff.Value);
            Helper.forbiddenBuffGUID = forbiddenBuffID.Value;
            Helper.humanReadablePercentageStats = humanReadablePercentageStats.Value;
            Helper.inverseMultipersDisplayReduction = inverseMultiplersDisplayReduction.Value;
            
            Plugin.Log(LogSystem.Plugin, LogLevel.Info, "Initialising player cache and internal database...");
            Helper.CreatePlayerCache();
            AutoSaveSystem.LoadDatabase();
            
            // Validate any potential change in permissions
            var commands = Command.GetAllCommands();
            Command.ValidatedCommandPermissions(commands);
            // Note for devs: To regenerate Command.md and PermissionSystem.DefaultCommandPermissions, uncomment the following:
            // Command.GenerateCommandMd(commands);
            // Command.GenerateDefaultCommandPermissions(commands);
            
            Plugin.Log(LogSystem.Plugin, LogLevel.Info, $"Setting CommandRegistry middleware");
            if (disableCommandAdminRequirement.Value)
            {
                Plugin.Log(LogSystem.Plugin, LogLevel.Info, "Removing admin privilege requirements");
                CommandRegistry.Middlewares.Clear();                
            }
            CommandRegistry.Middlewares.Add(new Command.PermissionMiddleware());

            Plugin.Log(LogSystem.Plugin, LogLevel.Info, "Finished initialising", true);

            isInitialized = true;
        }

        public enum LogSystem
        {
            Bloodline,
            Buff,
            Death,
            Faction,
            Mastery,
            Plugin,
            PowerUp,
            RandomEncounter,
            SquadSpawn,
            Wanted,
            Xp
        }
        
        public new static void Log(LogSystem system, LogLevel logLevel, string message, bool forceLog = false)
        {
            var isLogging = forceLog || DebugLoggingConfig.IsLogging(system);
            if (isLogging) _logger.Log(logLevel, $"{DateTime.Now.ToString("u")}: [{Enum.GetName(system)}] {message}");
        }
    }
}
