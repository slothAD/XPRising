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
        ExperienceSystem.isEXPActive = _configFile.Bind("Experience", "Enable", true, "Enable/disable the the Experience System.").Value;
        ExperienceSystem.ShouldAllowGearLevel = _configFile.Bind("Experience", "Allow Gear Level", false, "Enable/disable gear level adjustment.").Value;
        ExperienceSystem.LevelRewardsOn = _configFile.Bind("Experience", "Enable Level Rewards", false, "Enable rewards per level.").Value;
        ExperienceSystem.easyLvl15 = _configFile.Bind("Experience", "Easy lvl 15", true, "Makes level 15 much easier to reach so players dont get held up by the quest on it.").Value;

        ExperienceSystem.MaxLevel = _configFile.Bind("Experience", "Max Level", 80, "_configFileure the experience system max level.").Value;
        ExperienceSystem.EXPMultiplier = _configFile.Bind("Experience", "Multiplier", 1.0f, "Multiply the EXP gained by player.\n" +
                "Ex.: 0.7f -> Will reduce the EXP gained by 30%\nFormula: UnitKilledLevel * EXPMultiplier").Value;
        ExperienceSystem.VBloodMultiplier = _configFile.Bind("Experience", "VBlood Multiplier", 15f, "Multiply EXP gained from VBlood kills.\n" +
                "Formula: EXPGained * VBloodMultiplier * EXPMultiplier").Value;
        ExperienceSystem.EXPConstant = _configFile.Bind("Experience", "Constant", 0.2f, "Increase or decrease the required EXP to level up.\n" +
                "Formula: (level/constant)^2\n" +
                "EXP Table & Formula: https://bit.ly/3npqdJw").Value;
        ExperienceSystem.GroupModifier = _configFile.Bind("Experience", "Group Modifier", 0.75, "Set the modifier for EXP gained for each ally(player) in vicinity.\n" +
                "Example if you have 2 ally nearby, EXPGained = ((EXPGained * Modifier)*Modifier)").Value;
        ExperienceSystem.GroupMaxDistance = _configFile.Bind("Experience", "Group Range", 50f, "Set the maximum distance an ally (player) has to be from the player for them to share EXP with the player").Value;
        var groupLevelScheme = _configFile.Bind("Experience", "Group level Scheme", 2, "_configFileure the group levelling scheme. See documentation.").Value;
        if (Enum.IsDefined(typeof(ExperienceSystem.GroupLevelScheme), groupLevelScheme)) {
                ExperienceSystem.groupLevelScheme = (ExperienceSystem.GroupLevelScheme)groupLevelScheme;
        }

        ExperienceSystem.pvpXPLoss = _configFile.Bind("Rates, Experience", "PvP XP Loss", 0f, "Sets the flat XP Lost on a PvP death").Value;
        ExperienceSystem.pvpXPLossPerLevel = _configFile.Bind("Rates, Experience", "PvP XP Loss per Level", 0f, "Sets the XP Lost per level of the dying player on a PvP death").Value;
        ExperienceSystem.pvpXPLossPercent = _configFile.Bind("Rates, Experience", "PvP XP Loss Percent", 0f, "Sets the percentage of XP to the next level lost on a PvP death").Value;
        ExperienceSystem.pvpXPLossPercentPerLevel = _configFile.Bind("Rates, Experience", "PvP XP Loss Percent per Level", 0f, "Sets the percentage of XP to the next level lost per level of the dying player on a PvP death").Value;
        ExperienceSystem.pvpXPLossMultPerLvlDiff = _configFile.Bind("Rates, Experience", "PvP XP Loss Per lvl Diff", 0f, "Adds this times the number of levels higher than your killer you are as an additional percent to your xp lost on a PvP death.").Value;
        ExperienceSystem.pvpXPLossMultPerLvlDiffSq = _configFile.Bind("Rates, Experience", "PvP XP Loss Per lvl Diff squared", 0f, "Adds this times the square of the number of levels higher than your killer you are as an additional percent to your xp lost on a PvP death.").Value;
            
        ExperienceSystem.pveXPLoss = _configFile.Bind("Rates, Experience", "PvE XP Loss", 0f, "Sets the flat XP Lost on a PvE death").Value;
        ExperienceSystem.pveXPLossPerLevel = _configFile.Bind("Rates, Experience", "PvE XP Loss per Level", 0f, "Sets the XP Lost per level of the dying player on a PvE death").Value;
        ExperienceSystem.pveXPLossPercent = _configFile.Bind("Rates, Experience", "PvE XP Loss Percent", 10f, "Sets the percentage of XP to the next level lost on a PvE death").Value;
        ExperienceSystem.pveXPLossPercentPerLevel = _configFile.Bind("Rates, Experience", "PvE XP Loss Percent per Level", 0f, "Sets the percentage of XP to the next level lost per level of the dying player on a PvE death").Value;
        ExperienceSystem.pveXPLossMultPerLvlDiff = _configFile.Bind("Rates, Experience", "PvE XP Loss Mult Per lvl Diff", 0f, "Adds this times the number of levels higher than your killer you are as an additional percent to your xp lost on a PvE death.").Value;
        ExperienceSystem.pveXPLossMultPerLvlDiffSq = _configFile.Bind("Rates, Experience", "PvE XP Loss Per lvl Diff squared", 0f, "Adds this times the square of the number of levels higher than your killer you are as an additional percent to your xp lost on a PvE death.").Value;

        ExperienceSystem.xpLostOnDown = _configFile.Bind("Rates, Experience", "XP Lost on Down", false, "Vampires are treated as dead for the XP system when they are downed.").Value;
        ExperienceSystem.xpLostOnRelease = _configFile.Bind("Rates, Experience", "XP Lost on Release", true, "Vampires are treated as dead for the XP system when they release, incentivising saving allies.").Value;
    }
}