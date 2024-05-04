using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using OpenRPG.Systems;
using OpenRPG.Utils;

namespace OpenRPG.Configuration;

public static class BloodlineConfig
{
    private static ConfigFile _configFile;
    
    public static void Initialize()
    {
        Plugin.Log(Plugin.LogSystem.Plugin, LogLevel.Info, "Loading Bloodline config");
        var configPath = AutoSaveSystem.ConfirmFile(AutoSaveSystem.ConfigPath, "BloodlineConfig.cfg");
        _configFile = new ConfigFile(configPath, true);
        
        // Currently, we are never updating and saving the config file in game, so just load the values.
        BloodlineSystem.IsBloodlineSystemEnabled = _configFile.Bind("Bloodlines", "Enable Bloodlines", true, "Enables the Effectiveness mastery subsystem, which lets you reset your mastery to gain a multiplier to the effects of the matching mastery.").Value;
        BloodlineSystem.MercilessBloodlines = _configFile.Bind("Bloodlines", "Merciless Bloodlines", true, "Causes bloodlines to only grow when you kill something with a matching bloodline of higher strength, finally, a reward when you accidentally kill that 100% blood you found").Value;
        BloodlineSystem.DraculaGetsAll = _configFile.Bind("Bloodlines", "Dracula inherits all bloodlines", true, "Determines if Dracula (Frail) blood should inherit a portion of the other bloodlines.").Value;
        BloodlineSystem.GrowthMultiplier = _configFile.Bind("Bloodlines", "Bloodline growth multiplier", 1.0, "The multiplier applied to all bloodline gains.").Value;
        BloodlineSystem.VBloodMultiplier = _configFile.Bind("Bloodlines", "Bloodline VBlood Multiplier", 25.0, "The multiplier applied to the effective level of VBlood enemies for bloodline gains.").Value;
        BloodlineSystem.EffectivenessSubSystemEnabled = _configFile.Bind("Bloodlines", "Enable Effectiveness Subsystem", true, "Enables the Effectiveness bloodline subsystem, which lets you reset your bloodline to gain a multiplier to the effects of the matching bloodline.").Value;
        BloodlineSystem.GrowthSubsystemEnabled = _configFile.Bind("Bloodlines", "Enable Growth Subsystem", true, "Enables the Growth Bloodline subsystem, same as the one for mastery").Value;
        BloodlineSystem.MaxBloodlineStrength = _configFile.Bind("Bloodlines", "Maximum Strength", 100.0, "The maximum strength for a bloodline in percentage.").Value;
        BloodlineSystem.MaxBloodlineEffectiveness = _configFile.Bind("Bloodlines", "Maximum Effectiveness", 5.0, "The maximum bloodline effectiveness where 1 is 100%.").Value;
        BloodlineSystem.MinBloodlineGrowth = _configFile.Bind("Bloodlines", "Minimum Growth Rate", 0.1, "The minimum growth rate, where 1 is 100%").Value;
        BloodlineSystem.MaxBloodlineGrowth = _configFile.Bind("Bloodlines", "Maximum Growth Rate", 10.0, "the maximum growth rate where 1 is 100%").Value;
        BloodlineSystem.GrowthPerEffectiveness = _configFile.Bind("Bloodlines", "Growth per effectiveness", -1.0, "The amount of growth gained per point of effectiveness gained, if negative will reduce accordingly (gaining 100% effectiveness with -1 here will halve your current growth)").Value;
    }
}