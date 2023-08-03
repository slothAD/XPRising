using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System.Linq;
using VampireCommandFramework;
using System;

namespace OpenRPG.Commands
{
    
    [CommandGroup("rpg")]
    public static class Permission
    {
        [Command("permission", usage: "<list>|<save>|<reload>|<set> <0-100> <playername>", description: "Manage commands and user permissions level.")]
        public static void Initialize(ChatCommandContext ctx, string option, int level = 0, string playerName = null )
        {

            if (level == 0 && playerName == null)
            {
                if (option.ToLower().Equals("list"))
                {
                    _ = PermissionSystem.PermissionList(ctx);
                }
                else if (option.ToLower().Equals("save"))
                {
                    PermissionSystem.SaveUserPermission();
                    ctx.Reply("Saved user permission to JSON file.");
                }
                else if (option.ToLower().Equals("reload"))
                {
                    PermissionSystem.LoadPermissions();
                    ctx.Reply( "Reloaded permission from JSON file.");
                }
                else
                {
                   throw ctx.Error($"Option {option} dont exist");
                }
                return;
            }

            if (level == 0 || playerName == null)
            {
                throw ctx.Error($"Missing Arguments");
            }

            if (option.ToLower().Equals("set")) {
                
                if (level < 0 || level > 100)
                {
                    throw ctx.Error($"Level {level} invalid");
                }
                


                bool tryFind = Helper.FindPlayer(playerName, false, out _, out var target_userEntity);
                if (!tryFind)
                {
                    throw ctx.Error($"Could not find specified player \"{playerName}\".");
                }
                ulong SteamID = Plugin.Server.EntityManager.GetComponentData<User>(target_userEntity).PlatformId;

                if (level == 0) Database.user_permission.Remove(SteamID);
                else Database.user_permission[SteamID] = level;

                ctx.Reply( $"Player \"{playerName}\" permission is now set to <color=#fffffffe>{level}</color>.");
            }
            else
            {
                throw ctx.Error($"Invalid arguments for option {option}");
            }
        }
    }
}
