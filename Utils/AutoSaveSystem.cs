using RPGMods.Commands;
using RPGMods.Hooks;
using RPGMods.Systems;
using System;
using UnityEngine.Rendering.HighDefinition;

namespace RPGMods.Utils
{
    public static class AutoSaveSystem
    {
        //-- AutoSave is now directly hooked into the Server game save activity.
        public const string mainSaveFolder = "BepInEx/config/RPGMods/Saves/";
        public const string backupSaveFolder = "BepInEx/config/RPGMods/Saves/Backup/";
        private static int saveCount = 0;
        public static int backupFrequency = 5;
        public static bool saveLogging = true;
        public static void SaveDatabase(){
            saveCount++;
            string saveFolder = mainSaveFolder;
            if(saveCount % backupFrequency == 0) {
                saveFolder = backupSaveFolder;
            }
            //PermissionSystem.SaveUserPermission(); //-- Nothing new to save.
            GodMode.SaveGodMode(saveFolder);
            if(saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": God Mode Saved.");


            SunImmunity.SaveImmunity(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Sun Immunity Saved.");
            Waypoint.SaveWaypoints(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Waypoints Saved.");
            NoCooldown.SaveCooldown(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": No CD Saved.");
            Speed.SaveSpeed(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Speed Saved.");
            //AutoRespawn.SaveAutoRespawn();
            //Kit.SaveKits();   //-- Nothing to save here for now.
            PowerUp.SavePowerUp(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Powerup Saved.");

            //-- System Related
            ExperienceSystem.SaveEXPData(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Experience Saved.");
            //PvPSystem.SavePvPStat();
            WeaponMasterSystem.SaveWeaponMastery(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Mastery Saved.");
            Bloodlines.saveBloodlines(saveFolder);
            if (saveLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Bloodlines Saved.");
            //BanSystem.SaveBanList();
            //WorldDynamicsSystem.SaveFactionStats(saveFolder);
            //WorldDynamicsSystem.SaveIgnoredMobs(saveFolder);
            //VampireDownedServerEventSystem_Patch.saveKillMap();

            Plugin.Logger.LogInfo(DateTime.Now+": All databases saved to JSON file.");
        }

        public static void LoadDatabase()
        {
            //-- Commands Related
            PermissionSystem.LoadPermissions();
            NoCooldown.LoadNoCooldown();
            GodMode.LoadGodMode();
            PowerUp.LoadPowerUp();

            SunImmunity.LoadSunImmunity();
            Speed.LoadSpeed();

            
            Waypoint.LoadWaypoints();
            //AutoRespawn.LoadAutoRespawn();
            //Kit.LoadKits();


            //-- System Related
            PvPSystem.LoadPvPStat();
            ExperienceSystem.LoadEXPData();
            WeaponMasterSystem.LoadWeaponMastery();
            Bloodlines.loadBloodlines();
            //BanSystem.LoadBanList();
            WorldDynamicsSystem.LoadFactionStats();
            WorldDynamicsSystem.LoadIgnoredMobs();
            //VampireDownedServerEventSystem_Patch.loadKillMap();

            Plugin.Logger.LogInfo("All database is now loaded.");
        }
    }
}
