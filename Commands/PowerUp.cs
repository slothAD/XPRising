using ProjectM.Network;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    public static class PowerUp
    {
        [Command("powerup", "pu", "<player_name> <add>|<remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>", "Buff specified player with the specified value.", adminOnly:true)]
        public static void powerUP(ChatCommandContext ctx, string name, string flag, float MaxHP = 0, float PATK = 0, float SATK = 0, float PDEF = 0, float SDEF = 0){
            
            if (!Helper.FindPlayer(name, false, out var playerEntity, out var userEntity))
            {
                ctx.Reply("Specified player not found.");
                return;
            }
            ulong SteamID;
            if (Plugin.Server.EntityManager.TryGetComponentData<User>(userEntity, out var user) )
            {
                SteamID = user.PlatformId;
            }
            else if(Plugin.Server.EntityManager.TryGetComponentData<User>(ctx.Event.SenderUserEntity, out var u2))
            {
                SteamID = u2.PlatformId;
            }
            else
            {
                ctx.Reply("Steam ID for " + name + " could not be found!");
                SteamID=0;
                flag = "remove";
            }

            if (flag.ToLower().Equals("remove"))
            {
                Database.PowerUpList.Remove(SteamID);
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                ctx.Reply("PowerUp removed from specified player.");
                return;
            }


            if (flag.ToLower().Equals("add"))
            {

                var PowerUpData = new PowerUpData()
                {
                    Name = name,
                    MaxHP = MaxHP,
                    PATK = PATK,
                    PDEF = PDEF,
                    SATK = SATK,
                    SDEF = SDEF
                };

                Database.PowerUpList[SteamID] = PowerUpData;
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                ctx.Reply("PowerUp added to specified player.");
            }
            else
            {

                ctx.Reply("flag needs to be add or remove");
            }
            return;
        }

        public static void SavePowerUp()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/powerup.json", JsonSerializer.Serialize(Database.PowerUpList, Database.JSON_options));
        }

        public static void LoadPowerUp()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/powerup.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/powerup.json");
                stream.Dispose();
            }
            string content = File.ReadAllText("BepInEx/config/RPGMods/Saves/powerup.json");
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
