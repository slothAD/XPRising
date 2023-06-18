using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGMods.Commands{
    [CommandGroup("mastery", "m")]
    public static class Mastery{
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static bool detailedStatements = true;


        [Command("get", "g", "", "Display your current mastery progression")]
        public static void getMastery(ChatCommandContext ctx) {
            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");
            if (!WeaponMasterSystem.isMasteryEnabled) {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist) {
                ctx.Reply("You haven't even tried to master anything...");
                return;
            }

            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");

            int weapon;

            if (detailedStatements) {
                for (weapon = 0; weapon < WeaponMasterSystem.masteryStats.Length; weapon++) {
                    ctx.Reply(getMasteryDataStringForType(SteamID, weapon));
                }
            } else {
                weapon = (int)WeaponMasterSystem.GetWeaponType(ctx.Event.SenderCharacterEntity) + 1;
                ctx.Reply(getMasteryDataStringForType(SteamID, weapon));
                ctx.Reply(getMasteryDataStringForType(SteamID, 0));
            }
        }

        [Command("get all", "g a", "", "Display your current mastery progression in everything")]
        public static void getAllMastery(ChatCommandContext ctx) {
            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");
            if (!WeaponMasterSystem.isMasteryEnabled) {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist) {
                ctx.Reply("You haven't even tried to master anything...");
                return;
            }

            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");

            int weapon;

            for (weapon = 0; weapon < WeaponMasterSystem.masteryStats.Length; weapon++) {
                ctx.Reply(getMasteryDataStringForType(SteamID, weapon));
            }
        }

        public static string getMasteryDataStringForType(ulong SteamID, int weapon){
            string name;
            double masteryPercent;
            double effectiveness;
            double growth;
            string print;
            bool ed = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd);

            name = WeaponMasterSystem.typeToName(weapon);
            masteryPercent = WeaponMasterSystem.masteryDataByType(weapon, SteamID);
            effectiveness = 1;
            growth = 1;
            if (ed)
            {
                effectiveness = wd.efficency[weapon];
                growth = wd.growth[weapon];
            }
            print = $"{name}:<color=#fffffffe> {masteryPercent:F2}%</color> (";
            for (int i = 0; i < WeaponMasterSystem.masteryStats[weapon].Length; i++)
            {
                if (i > 0)
                    print += ",";
                print += Helper.statTypeToString((UnitStatType)WeaponMasterSystem.masteryStats[weapon][i]);
                print += " <color=#75FF33>";
                print += $"{WeaponMasterSystem.calcBuffValue(weapon, masteryPercent, SteamID, i):F2}";
                print += "</color>";
            }
            print += $") Effectiveness: {effectiveness * 100}%, Growth: {growth * 100}%";
            return print ;
        }

        [Command("add", "a", "weaponType, amount", "Adds the amount to the mastery of the specifed weaponType", adminOnly: true)]
        public static void addMastery(ChatCommandContext ctx, string name, double amount){
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            string CharName = ctx.Event.User.CharacterName.ToString();
            var UserEntity = ctx.Event.SenderUserEntity;
            var CharEntity = ctx.Event.SenderCharacterEntity;

            string MasteryType = name.ToLower();
            int type = 0;
            bool typeFound = WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out type);
            if (!typeFound) {
                ctx.Reply($"{name.ToUpper()} Mastery type not found! did you typo?");
                return;
            }

            WeaponMasterSystem.modMastery(SteamID, type, (amount));
            ctx.Reply($"{name.ToUpper()} Mastery for \"{CharName}\" adjusted by <color=#fffffffe>{amount:F2}%</color>");
            Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }
        
        [Command("set", "s", "[playerName, XP]", "Sets the specified players current xp to a specific value", adminOnly: true)]
        public static void setMastery(ChatCommandContext ctx, string name, string MasteryType, double value) {
            if (!WeaponMasterSystem.isMasteryEnabled) {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            ulong SteamID;
            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity)) {
                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
            } else {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }

            MasteryType = MasteryType.ToLower();
            bool typeFound = WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out int type);
            if (!typeFound) {
                ctx.Reply($"{MasteryType} Mastery type not found! did you typo?");
                return;
            }

            WeaponMasterSystem.modMastery(SteamID, type, -100000);
            WeaponMasterSystem.modMastery(SteamID, type, (value));
            ctx.Reply($"{MasteryType} Mastery for \"{name}\" set to <color=#fffffffe>{value:F2}%</color>");
        }

        [Command("log", "l", "<On, Off>", "Turns on or off logging of mastery gain.", adminOnly: false)]
        public static void logMastery(ChatCommandContext ctx, string flag)
        {
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            if (flag.ToLower().Equals("on"))
            {
                Database.player_log_mastery.Remove(SteamID);
                Database.player_log_mastery.Add(SteamID, true);
                ctx.Reply("Mastery gain is now being logged.");
                return;
            }
            else if (flag.ToLower().Equals("off"))
            {
                Database.player_log_mastery.Remove(SteamID);
                Database.player_log_mastery.Add(SteamID, false);
                ctx.Reply($"Mastery gain is no longer being logged.");
                return;
            }
        }


        [Command("reset", "r", "[weaponType]", "Resets a mastery to gain more power with it.", adminOnly: false)]
        public static void resetMastery(ChatCommandContext ctx, string name)
        {
            if (!WeaponMasterSystem.isMasteryEnabled){
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            string MasteryType = name.ToLower();
            bool typeFound = WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out int type);
            if (!typeFound) {
                ctx.Reply($"{name.ToUpper()} Mastery type not found! did you typo?");
                return;
            }
            ctx.Reply("Resetting " + WeaponMasterSystem.typeToName(type) + " Mastery");
            WeaponMasterSystem.resetMastery(SteamID, type);
        }
    }

}
