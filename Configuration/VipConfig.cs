using BepInEx.Configuration;
using BepInEx.Logging;
using OpenRPG.Systems;
using OpenRPG.Utils;

namespace OpenRPG.Configuration;

public static class VipConfig
{
    private static ConfigFile _configFile; 
    
    public static void Initialize()
    {
        Plugin.Log(Plugin.LogSystem.Plugin, LogLevel.Info, "Loading VIP config");
        var configPath = AutoSaveSystem.ConfirmFile(AutoSaveSystem.ConfigPath, "VipConfig.cfg");
        _configFile = new ConfigFile(configPath, true);
        
        // Currently, we are never updating and saving the config file in game, so just load the values.
        PermissionSystem.isVIPSystem = _configFile.Bind("VIP", "Enable VIP System", false, "Enable the VIP System.").Value;
        PermissionSystem.isVIPWhitelist = _configFile.Bind("VIP", "Enable VIP Whitelist", false, "Enable the VIP user to ignore server capacity limit.").Value;
        PermissionSystem.VIP_Permission = _configFile.Bind("VIP", "Minimum VIP Permission", 50, "The minimum permission level required for the user to be considered as VIP.").Value;

        PermissionSystem.VIP_InCombat_DurabilityLoss = _configFile.Bind("VIP.InCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is in combat. -1.0 to disable.\n" +
            "Does not affect durability loss on death.").Value;
        PermissionSystem.VIP_InCombat_GarlicResistance = _configFile.Bind("VIP.InCombat", "Garlic Resistance Multiplier", -1.0, "Multiply garlic resistance when user is in combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_InCombat_SilverResistance = _configFile.Bind("VIP.InCombat", "Silver Resistance Multiplier", -1.0, "Multiply silver resistance when user is in combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_InCombat_MoveSpeed = _configFile.Bind("VIP.InCombat", "Move Speed Multiplier", -1.0, "Multiply move speed when user is in combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_InCombat_ResYield = _configFile.Bind("VIP.InCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.").Value;

        PermissionSystem.VIP_OutCombat_DurabilityLoss = _configFile.Bind("VIP.OutCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is out of combat. -1.0 to disable.\n" +
            "Does not affect durability loss on death.").Value;
        PermissionSystem.VIP_OutCombat_GarlicResistance = _configFile.Bind("VIP.OutCombat", "Garlic Resistance Multiplier", 2.0, "Multiply garlic resistance when user is out of combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_OutCombat_SilverResistance = _configFile.Bind("VIP.OutCombat", "Silver Resistance Multiplier", 2.0, "Multiply silver resistance when user is out of combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_OutCombat_MoveSpeed = _configFile.Bind("VIP.OutCombat", "Move Speed Multiplier", 1.25, "Multiply move speed when user is out of combat. -1.0 to disable.").Value;
        PermissionSystem.VIP_OutCombat_ResYield = _configFile.Bind("VIP.OutCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.").Value;
    }
}