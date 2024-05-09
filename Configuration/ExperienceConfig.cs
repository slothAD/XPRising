using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using OpenRPG.Systems;
using OpenRPG.Utils;

namespace OpenRPG.Configuration;

public static class ExperienceConfig
{
    private static ConfigFile _configFile; 
    
    public static void Initialize()
    {
        Plugin.Log(Plugin.LogSystem.Core, LogLevel.Info, "Loading XP config");
        var configPath = AutoSaveSystem.ConfirmFile(AutoSaveSystem.ConfigPath, "ExperienceConfig.cfg");
        _configFile = new ConfigFile(configPath, true);
        
        // Currently, we are never updating and saving the config file in game, so just load the values.
        ExperienceSystem.ShouldAllowGearLevel = _configFile.Bind("Experience", "Allow Gear Level", false, "Enable/disable gear level adjustment.").Value;
        ExperienceSystem.LevelRewardsOn = _configFile.Bind("Experience", "Enable Level Rewards", false, "Enable rewards per level.").Value;
        ExperienceSystem.EasyLvl15 = _configFile.Bind("Experience", "Easy lvl 15", true, "Makes level 15 much easier to reach so players dont get held up by the quest on it.").Value;

        ExperienceSystem.MaxLevel = _configFile.Bind("Experience", "Max Level", 80, "_configFileure the experience system max level.").Value;
        ExperienceSystem.ExpMultiplier = _configFile.Bind("Experience", "Multiplier", 1.0f, "Multiply the EXP gained by player.\n" +
                "Ex.: 0.7f -> Will reduce the EXP gained by 30%\nFormula: UnitKilledLevel * EXPMultiplier").Value;
        ExperienceSystem.VBloodMultiplier = _configFile.Bind("Experience", "VBlood Multiplier", 15f, "Multiply EXP gained from VBlood kills.\n" +
                "Formula: EXPGained * VBloodMultiplier * EXPMultiplier").Value;
        ExperienceSystem.ExpConstant = _configFile.Bind("Experience", "Constant", 0.2f, "Increase or decrease the required EXP to level up.\n" +
                "Formula: (level/constant)^2\n" +
                "EXP Table & Formula: https://bit.ly/3npqdJw").Value;
        ExperienceSystem.GroupMaxDistance = _configFile.Bind("Experience", "Group Range", 50f, "Set the maximum distance an ally (player) has to be from the player for them to share EXP with the player").Value;
        var groupLevelScheme = _configFile.Bind("Experience", "Group level Scheme", 2, "Used to determine the group player level.\n" +
                " Options: Off = 0, Average of player levels = 1, Max player level in group = 2, Each player determines their own level = 3, Use the level of the killer = 4." +
                " See documentation for more information.").Value;
        if (Enum.IsDefined(typeof(ExperienceSystem.GroupLevelScheme), groupLevelScheme)) {
                ExperienceSystem.CurrentGroupLevelScheme = (ExperienceSystem.GroupLevelScheme)groupLevelScheme;
        }

        ExperienceSystem.PvpXpLossPercent = _configFile.Bind("Rates, Experience", "PvP XP Loss Percent", 0f, "Sets the percentage of XP to the next level lost on a PvP death").Value;
        ExperienceSystem.PveXpLossPercent = _configFile.Bind("Rates, Experience", "PvE XP Loss Percent", 10f, "Sets the percentage of XP to the next level lost on a PvE death").Value;
    }
}