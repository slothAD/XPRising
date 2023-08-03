using ProjectM.Network;
using OpenRPG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class PowerUp
    {
        [Command("powerup add", usage: "pu <player_name> <add>|<remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>", description: "Buff specified player with the specified value.")]
        public static void PowerUpAdd(ChatCommandContext ctx, string playerName, string type, int MaxHP = 0, int PATK = 0, int SATK = 0, int PDEF = 0, int SDEF = 0)
        {

            string PlayerName = playerName.ToLower();
            if (!Helper.FindPlayer(PlayerName, false, out var playerEntity, out var userEntity))
            {
                throw ctx.Error("Specified player not found.");
            }

            ulong SteamID = Plugin.Server.EntityManager.GetComponentData<User>(userEntity).PlatformId;

            if (type.ToLower().Equals("remove"))
            {
                Database.PowerUpList.Remove(SteamID);
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                ctx.Reply("PowerUp removed from specified player.");
                return;
            }

            if (MaxHP == 0 && PATK == 0 && PDEF == 0 && SATK == 0 && SDEF == 0 )
            {
                throw ctx.Error("Missing Arguments.");
            }

            if (type.ToLower().Equals("add"))
            {
                var PowerUpData = new PowerUpData()
                {
                    Name = PlayerName,
                    MaxHP = MaxHP,
                    PATK = PATK,
                    PDEF = PDEF,
                    SATK = SATK,
                    SDEF = SDEF
                };

                Database.PowerUpList[SteamID] = PowerUpData;
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                ctx.Reply("PowerUp added to specified player.");
                return;
            }
        }

        public static void SavePowerUp()
        {
            File.WriteAllText(Plugin.PowerUpJson, JsonSerializer.Serialize(Database.PowerUpList, Database.JSON_options));
        }

        public static void LoadPowerUp()
        {
            if (!File.Exists(Plugin.PowerUpJson))
            {
                var stream = File.Create(Plugin.PowerUpJson);
                stream.Dispose();
            }
            string content = File.ReadAllText(Plugin.PowerUpJson);
            try
            {
                Database.PowerUpList = JsonSerializer.Deserialize<Dictionary<ulong, PowerUpData>>(content);
                Plugin.Logger.LogWarning("PowerUp DB Populated.");
            }
            catch
            {
                Database.PowerUpList = new ();
                Plugin.Logger.LogWarning("PowerUp DB Created.");
            }
        }
    }
}
