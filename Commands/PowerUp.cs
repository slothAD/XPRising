using ProjectM.Network;
using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class PowerUp {
        public static bool powerupLogging = true;
        [Command("powerup", "pu", "<player_name> <add>|<remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>", "Buff specified player with the specified value.", adminOnly:true)]
        public static void PowerUpCommand(ChatCommandContext ctx, string name, string flag, float MaxHP = 0, float PATK = 0, float SATK = 0, float PDEF = 0, float SDEF = 0){

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Beginning PowerUp Command");

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Arguments are as follows: " + name + ", " + flag + ", " + MaxHP + ", " + PATK + ", " + SATK + ", " + PDEF + ", " + SDEF + ", ");

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Now trying to find player");

            if (!Helper.FindPlayer(name, false, out var playerEntity, out var userEntity))
            {
                throw ctx.Error("Specified player not found.");
            }
            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Player " + name + " Found");
            ulong steamID;
            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Trying to get steam ID");
            if (Plugin.Server.EntityManager.TryGetComponentData<User>(userEntity, out var user) ){
                steamID = user.PlatformId;
            }
            else if(Plugin.Server.EntityManager.TryGetComponentData<User>(ctx.Event.SenderUserEntity, out var u2)){
                steamID = u2.PlatformId;
            }
            else{
                ctx.Reply("Steam ID for " + name + " could not be found!");
                steamID=0;
                flag = "remove";
            }

            if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Checking Flags");
            if (flag.ToLower().Equals("remove")) {
                if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Flag is Remove");
                Database.PowerUpList.Remove(steamID);
                Helper.ApplyBuff(userEntity, playerEntity, Helper.appliedBuff);
                ctx.Reply("PowerUp removed from specified player.");
                return;
            }


            if (flag.ToLower().Equals("add")) {
                if (powerupLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Flag is Add");

                var powerUpData = new PowerUpData(){
                    Name = name,
                    MaxHP = MaxHP,
                    PATK = PATK,
                    PDEF = PDEF,
                    SATK = SATK,
                    SDEF = SDEF
                };

                Database.PowerUpList[steamID] = powerUpData;
                Helper.ApplyBuff(userEntity, playerEntity, Helper.appliedBuff);
                ctx.Reply("PowerUp added to specified player.");
            }
            else{

                ctx.Reply("flag needs to be add or remove");
            }
            return;
        }
    }
}
