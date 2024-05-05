using BepInEx.Configuration;
using BepInEx.Logging;
using OpenRPG.Systems;
using OpenRPG.Utils;

namespace OpenRPG.Configuration;

public static class MasteryConfig
{
    private static ConfigFile _configFile;
    
    public static void Initialize()
    {
        Plugin.Log(Plugin.LogSystem.Core, LogLevel.Info, "Loading Weapon Mastery config");
        var configPath = AutoSaveSystem.ConfirmFile(AutoSaveSystem.ConfigPath, "MasteryConfig.cfg");
        _configFile = new ConfigFile(configPath, true);
        
        // Currently, we are never updating and saving the config file in game, so just load the values.
        const string masteryConfigDocumentation =
            "This file contains the configuration for the buffs that get applied for increasing mastery in a Mastery type. The format consists of a Type => Stat => growth rate. " +
            "The stat IDs for what a Mastery type should boost should be able to handle any number of stats. See the documentation for a list of stat IDs. " + 
            "The growth rate describes the amount per point of mastery the stat should be boosted by. Some stats, like crit, have 1 as 100%, and CDR is % mastery to reach 50% cdr, so configure appropriately.";
        // Bind some dummy value so the documentation is written to the config file.
        _configFile.Bind("Documentation, weapon_mastery_config.json", "Config", 0, masteryConfigDocumentation);
        
        WeaponMasterySystem.IsMasteryEnabled = _configFile.Bind("Mastery", "Enable Weapon Mastery", true, "Enable/disable the weapon mastery system.").Value;
        WeaponMasterySystem.MaxMastery = _configFile.Bind("Mastery", "Max Mastery Value", 100d, "Configure the maximum mastery the user can attain. (100000 is 100%)").Value;
        WeaponMasterySystem.MasteryCombatTick = _configFile.Bind("Mastery", "Mastery Value/Combat Ticks", 5, "Configure the amount of mastery gained per combat ticks. (5 -> 0.005%)").Value;
        WeaponMasterySystem.MaxCombatTick = _configFile.Bind("Mastery", "Max Combat Ticks", 12, "Mastery will no longer increase after this many ticks is reached in combat. (1 tick = 5 seconds)").Value;
        WeaponMasterySystem.MasteryMultiplier = _configFile.Bind("Mastery", "Mastery Multiplier", 1d, "Multiply the gained mastery value by this amount.").Value;
        WeaponMasterySystem.VBloodMultiplier = _configFile.Bind("Mastery", "VBlood Mastery Multiplier", 15d, "Multiply Mastery gained from VBlood kill.").Value;
        
        WeaponMasterySystem.IsDecaySystemEnabled = _configFile.Bind("Mastery", "Enable Mastery Decay", false, "Enable/disable the decay of weapon mastery over time.").Value;
        WeaponMasterySystem.DecayInterval = _configFile.Bind("Mastery", "Decay Interval", 60, "Amount of seconds per decay tick.").Value;
        WeaponMasterySystem.OfflineDecayValue = _configFile.Bind("Mastery", "Decay Value", 1, "Mastery will decay by this amount for every decay tick.(1 -> 0.001%)").Value;
        WeaponMasterySystem.OnlineDecayValue = WeaponMasterySystem.OfflineDecayValue;
        
        WeaponMasterySystem.EffectivenessSubSystemEnabled = _configFile.Bind("Mastery", "Enable Effectiveness Subsystem", true, "Enables the Effectiveness mastery subsystem, which lets you reset your mastery to gain a multiplier to the effects of the matching mastery.").Value;
        WeaponMasterySystem.MaxEffectiveness = _configFile.Bind("Mastery", "Maximum Effectiveness", 5f, "The maximum mastery effectiveness where 1 is 100%.").Value;
        WeaponMasterySystem.GrowthSubSystemEnabled = _configFile.Bind("Mastery", "Enable Growth Subsystem", true, "Enables the growth subsystem, when you reset mastery either increases or decreases your matching mastery growth rate, depending on _configFile.").Value;
        WeaponMasterySystem.GrowthPerEffectiveness = _configFile.Bind("Mastery", "Growth per effectiveness", -1f, "The amount of growth gained per point of effectiveness gained, if negative will reduce accordingly (gaining 100% effectiveness with -1 here will halve your current growth)").Value;
        WeaponMasterySystem.MinGrowth = _configFile.Bind("Mastery", "Minimum Growth Rate", 0.1f, "The minimum growth rate, where 1 is 100%").Value;
        WeaponMasterySystem.MaxGrowth = _configFile.Bind("Mastery", "Maximum Growth Rate", 10f, "the maximum growth rate where 1 is 100%").Value;
        
        WeaponMasterySystem.SpellMasteryNeedsUnarmedToUse = _configFile.Bind("Mastery", "Unarmed Only Spell Mastery Use", false, "Gain the benefits of spell mastery only when you have no weapon equipped.").Value;
        WeaponMasterySystem.SpellMasteryNeedsUnarmedToLearn = _configFile.Bind("Mastery", "Unarmed Only Spell Mastery Learning", true, "Progress spell mastery only when you have no weapon equipped.").Value;
        WeaponMasterySystem.LinearCdr = _configFile.Bind("Mastery", "Linear Mastery CDR", true, "Changes CDR from mastery to provide a linear increase to spells able to be cast in a given time by making the cdr diminishing.").Value;
        WeaponMasterySystem.CdrStacks = _configFile.Bind("Mastery", "Mastery CDR stacks", true, "Allows mastery cdr to stack with that from other sources, the reduction is multiplicative. E.G. Mist signet (10% cdr) and 100% mastery (50% cdr) will result in 55% total cdr, or 120%ish faster cooldowns.").Value;
    }
}