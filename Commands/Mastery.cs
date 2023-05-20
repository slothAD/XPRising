using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    //[Command("mastery, m", Usage = "mastery [<log> <on>|<off>] [<reset> all|(mastery type)]", Description = "Display your current mastery progression, toggle the gain notification, or reset your mastery to gain effectiveness.")]

    [CommandGroup("mastery", "m")]
    public static class Mastery
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static bool detailedStatements = true;


        [Command("get", "g", "", "Display your current mastery progression")]
        public static void getMastery(ChatCommandContext ctx)
        {
            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist)
            {
                ctx.Reply("You haven't even tried to master anything...");
                return;
            }

            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");

            int weapon;

            if (detailedStatements){
                for (weapon = 0; weapon < WeaponMasterSystem.masteryStats.Length; weapon++){
                    ctx.Reply(getMasteryDataStringForType(SteamID, weapon));
                }
            }
            else{
                weapon = (int)WeaponMasterSystem.GetWeaponType(ctx.Event.SenderCharacterEntity) + 1;
                ctx.Reply(getMasteryDataStringForType(SteamID, weapon));
                ctx.Reply(getMasteryDataStringForType(SteamID, 0));
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
            print = $"{name}:<color=#fffffffe> {masteryPercent}%</color> (";
            for (int i = 0; i < WeaponMasterSystem.masteryStats[weapon].Length; i++)
            {
                if (i > 0)
                    print += ",";
                print += Helper.statTypeToString((UnitStatType)WeaponMasterSystem.masteryStats[weapon][i]);
                print += " <color=#75FF33>";
                print += WeaponMasterSystem.calcBuffValue(weapon, masteryPercent, SteamID, i);
                print += "</color>";
            }
            print += $") Effectiveness: {effectiveness * 100}%, Growth: {growth * 100}%";
            return print ;
        }

        [Command("add", "a", "weaponType, amount", "Adds the amount to the mastery of the specifed weaponType", adminOnly: true)]
        public static void addMastery(ChatCommandContext ctx, string name, double amount)
        {
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
            if (MasteryType.Equals("sword")) type = (int)WeaponType.Sword + 1;
            else if (MasteryType.Equals("none") || MasteryType.Equals("unarmed")) type = (int)WeaponType.None + 1;
            else if (MasteryType.Equals("spear")) type = (int)WeaponType.Spear + 1;
            else if (MasteryType.Equals("crossbow")) type = (int)WeaponType.Crossbow + 1;
            else if (MasteryType.Equals("slashers")) type = (int)WeaponType.Slashers + 1;
            else if (MasteryType.Equals("scythe")) type = (int)WeaponType.Scythe + 1;
            else if (MasteryType.Equals("fishingpole")) type = (int)WeaponType.FishingPole + 1;
            else if (MasteryType.Equals("mace")) type = (int)WeaponType.Mace + 1;
            else if (MasteryType.Equals("axes")) type = (int)WeaponType.Axes + 1;
            else if (MasteryType.Equals("spell")) type = 0;

            WeaponMasterSystem.SetMastery(SteamID, type, (int)(amount * 1000));
            ctx.Reply($"{name.ToUpper()} Mastery for \"{CharName}\" adjusted by <color=#fffffffe>{amount}%</color>");
            Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.BloodSight);
        }

        [Command("log", "l", "<On, Off>", "Turns on or off logging of mastery gain.", adminOnly: false)]
        public static void logBloodline(ChatCommandContext ctx, string flag)
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
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            string MasteryType = name.ToLower();
            int type = 0;
            if (MasteryType.Equals("sword")) type = (int)WeaponType.Sword + 1;
            else if (MasteryType.Equals("none") || MasteryType.Equals("unarmed")) type = (int)WeaponType.None + 1;
            else if (MasteryType.Equals("spear")) type = (int)WeaponType.Spear + 1;
            else if (MasteryType.Equals("crossbow")) type = (int)WeaponType.Crossbow + 1;
            else if (MasteryType.Equals("slashers")) type = (int)WeaponType.Slashers + 1;
            else if (MasteryType.Equals("scythe")) type = (int)WeaponType.Scythe + 1;
            else if (MasteryType.Equals("fishingpole")) type = (int)WeaponType.FishingPole + 1;
            else if (MasteryType.Equals("mace")) type = (int)WeaponType.Mace + 1;
            else if (MasteryType.Equals("axes")) type = (int)WeaponType.Axes + 1;
            else if (MasteryType.Equals("spell")) type = 0;
            else if (MasteryType.Equals("all")) type = -1;
            ctx.Reply("Resetting " + WeaponMasterSystem.typeToName(type) + " Mastery");
            WeaponMasterSystem.resetMastery(SteamID, type);
        }
    }

}
