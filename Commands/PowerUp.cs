using ProjectM.Network;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    public static class PowerUp {
        public static bool powerupLogging = true;
        [Command("powerup", "pu", "<player_name> <add>|<remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>", "Buff specified player with the specified value.", adminOnly:true)]
        public static void powerUP(ChatCommandContext ctx, string name, string flag, float MaxHP = 0, float PATK = 0, float SATK = 0, float PDEF = 0, float SDEF = 0){

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Beginning PowerUp Command");

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Arguments are as follows: " + name + ", " + flag + ", " + MaxHP + ", " + PATK + ", " + SATK + ", " + PDEF + ", " + SDEF + ", ");

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Now trying to find player");

            if (!Helper.FindPlayer(name, false, out var playerEntity, out var userEntity))
            {
                ctx.Reply("Specified player not found.");
                return;
            }
            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Player " + name + " Found");
            ulong SteamID;
            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Trying to get steam ID");
            if (Plugin.Server.EntityManager.TryGetComponentData<User>(userEntity, out var user) ){
                SteamID = user.PlatformId;
            }
            else if(Plugin.Server.EntityManager.TryGetComponentData<User>(ctx.Event.SenderUserEntity, out var u2)){
                SteamID = u2.PlatformId;
            }
            else{
                ctx.Reply("Steam ID for " + name + " could not be found!");
                SteamID=0;
                flag = "remove";
            }

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Checking Flags");
            if (flag.ToLower().Equals("remove")) {
                if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Flag is Remove");
                Database.PowerUpList.Remove(SteamID);
                Helper.ApplyBuff(userEntity, playerEntity, Helper.appliedBuff);
                ctx.Reply("PowerUp removed from specified player.");
                return;
            }


            if (flag.ToLower().Equals("add")) {
                if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Flag is Add");

                var PowerUpData = new PowerUpData(){
                    Name = name,
                    MaxHP = MaxHP,
                    PATK = PATK,
                    PDEF = PDEF,
                    SATK = SATK,
                    SDEF = SDEF
                };

                Database.PowerUpList[SteamID] = PowerUpData;
                Helper.ApplyBuff(userEntity, playerEntity, Helper.appliedBuff);
                ctx.Reply("PowerUp added to specified player.");
            }
            else{

                ctx.Reply("flag needs to be add or remove");
            }
            return;
        }

        public static void SavePowerUp(string saveFolder)
        {
            File.WriteAllText(saveFolder+"powerup.json", JsonSerializer.Serialize(Database.PowerUpList, Database.JSON_options));
        }

        public static void LoadPowerUp() {
            string specificName = "powerup.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            if (!File.Exists(AutoSaveSystem.mainSaveFolder+ specificName))
            {
                var stream = File.Create(AutoSaveSystem.mainSaveFolder+specificName);
                stream.Dispose();
            }
            string content = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.PowerUpList = JsonSerializer.Deserialize<Dictionary<ulong, PowerUpData>>(content);
                if(Database.PowerUpList == null) {
                    content = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.PowerUpList = JsonSerializer.Deserialize<Dictionary<ulong, PowerUpData>>(content);
                }
                Plugin.Logger.LogWarning("PowerUp DB Populated.");
            }
            catch
            {
                Database.PowerUpList = new Dictionary<ulong, PowerUpData>();
                Plugin.Logger.LogWarning("PowerUp DB Created.");
            }
        }
    }
}
