using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("bloodline", "bl")]
    public static class Bloodline
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static bool detailedStatements = false;
        
        [Command("get", "g", "", "Display your current bloodline progression")]
        public static void getBloodline(ChatCommandContext ctx) {
            if (!Bloodlines.areBloodlinesEnabled) {
                ctx.Reply("Bloodline system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.playerBloodline.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist) {
                ctx.Reply("You haven't developed any bloodline...");
                return;
            }

            ctx.Reply("-- <color=#ffffffff>Bloodlines</color> --");
            int bl;
            string name;
            double masteryPercent;
            string print;
            BloodlineData bld = Bloodlines.getBloodlineData(SteamID);
            if (detailedStatements) {
                for (bl = 0; bl < Bloodlines.stats.Length; bl++) {
                    if (bl >= Bloodlines.stats.Length)
                        ctx.Reply($"Bloodline type {bl} beyond bloodline type limit of {Bloodlines.stats.Length - 1}");
                    name = Bloodlines.names[bl];
                    masteryPercent = bld.strength[bl];
                    print = $"{name}:<color=#fffffffe> {masteryPercent:F3}%</color> (";
                    for (int i = 0; i < Bloodlines.stats[bl].Length; i++) {
                        if (bld.strength[bl] >= Bloodlines.minStrengths[bl][i]) {
                            if (i > 0)
                                print += ",";
                            print += Helper.statTypeToString((UnitStatType)Bloodlines.stats[bl][i]);
                            print += " <color=#75FF33>";
                            double val = Bloodlines.calcBuffValue(bl, SteamID, i);
                            if (Helper.inverseMultipersDisplayReduction && Helper.inverseMultiplierStats.Contains(Bloodlines.stats[bl][i])) {
                                val = 1 - val;
                            }
                            print += $"{val:F3}";
                            print += "</color>";
                        }
                    }
                    print += $") Effectiveness: {bld.efficency[bl] * 100}%";
                    ctx.Reply(print);
                }
            } else {
                Blood blood = entityManager.GetComponentData<Blood>(ctx.Event.SenderCharacterEntity);
                Bloodlines.bloodlineMap.TryGetValue(blood.BloodType, out bl);
                if (bl >= Bloodlines.stats.Length)
                    ctx.Reply($"Bloodline type {bl} beyond bloodline type limit of {Bloodlines.stats.Length - 1}");
                name = Bloodlines.names[bl];
                masteryPercent = bld.strength[bl];
                print = $"{name}:<color=#fffffffe> {masteryPercent:F3}%</color> (";
                for (int i = 0; i < Bloodlines.stats[bl].Length; i++) {
                    if (bld.strength[bl] >= Bloodlines.minStrengths[bl][i]) {
                        if (i > 0)
                            print += ",";
                        print += Helper.statTypeToString((UnitStatType)Bloodlines.stats[bl][i]);
                        print += " <color=#75FF33>";
                        print += $"{Bloodlines.calcBuffValue(bl, SteamID, i):F3}";
                        print += "</color>";
                    }
                }
                print += $") Effectiveness: {bld.efficency[bl] * 100}%";
                ctx.Reply(print);
            }
        }
        
        [Command("get all", "ga", "", "Display all your bloodline progressions")]
        public static void getAllBloodlines(ChatCommandContext ctx) {
            if (!Bloodlines.areBloodlinesEnabled) {
                ctx.Reply("Bloodline system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.playerBloodline.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist) {
                ctx.Reply("You haven't developed any bloodline...");
                return;
            }

            ctx.Reply("-- <color=#ffffffff>Bloodlines</color> --");
            int bl;
            string name;
            double masteryPercent;
            string print;
            BloodlineData bld = Bloodlines.getBloodlineData(SteamID);
            for (bl = 0; bl < Bloodlines.stats.Length; bl++) {
                if (bl >= Bloodlines.stats.Length)
                    ctx.Reply($"Bloodline type {bl} beyond bloodline type limit of {Bloodlines.stats.Length - 1}");
                name = Bloodlines.names[bl];
                masteryPercent = bld.strength[bl];
                print = $"{name}:<color=#fffffffe> {masteryPercent:F3}%</color> (";
                for (int i = 0; i < Bloodlines.stats[bl].Length; i++) {
                    if (bld.strength[bl] >= Bloodlines.minStrengths[bl][i]) {
                        if (i > 0)
                            print += ",";
                        print += Helper.statTypeToString((UnitStatType)Bloodlines.stats[bl][i]);
                        print += " <color=#75FF33>";
                        double val = Bloodlines.calcBuffValue(bl, SteamID, i);
                        if (Helper.inverseMultipersDisplayReduction && Helper.inverseMultiplierStats.Contains(Bloodlines.stats[bl][i])) {
                            val = 1 - val;
                        }
                        print += $"{val:F3}";
                        print += "</color>";
                    }
                }
                print += $") Effectiveness: {bld.efficency[bl] * 100}%";
                ctx.Reply(print);
            }
        }

        [Command("add", "a", "[BloodlineName, amount]", "Adds amount to the specified bloodline. able to use default names, bloodtype names, or the configured names.", adminOnly: true)]
        public static void addBloodlineValue(ChatCommandContext ctx, string MasteryType, double amount)
        {
            ulong SteamID;
            string name = ctx.Event.User.CharacterName.ToString();
            var UserEntity = ctx.Event.SenderUserEntity;
            var CharEntity = ctx.Event.SenderCharacterEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            int type;
            if (!Bloodlines.nameMap.TryGetValue(MasteryType, out type))
            {
                MasteryType = MasteryType.ToLower();
                if (MasteryType.Equals("dracula")) type = 0;
                else if (MasteryType.Equals("arwen")) type = 1;
                else if (MasteryType.Equals("ilvris")) type = 2;
                else if (MasteryType.Equals("aya")) type = 3;
                else if (MasteryType.Equals("nytheria")) type = 4;
                else if (MasteryType.Equals("hadubert")) type = 5;
                else if (MasteryType.Equals("rei")) type = 6;
                else if (MasteryType.Equals("semika")) type = 7;
                else
                {
                    ctx.Reply("Invalid Arguments");
                    return;
                }
            }
            Bloodlines.modBloodline(SteamID, type, amount);
            ctx.Reply($"{Bloodlines.names[type]}'s bloodline for \"{name}\" adjusted by <color=#fffffffe>{amount}%</color>");
            Helper.ApplyBuff(UserEntity, CharEntity, Helper.appliedBuff);
        }

        [Command("set", "s", "[playerName, bloodline, value]", "Sets the specified players bloodline to a specific value", adminOnly: true)]
        public static void setBloodline(ChatCommandContext ctx, string name, string type, double value) {
            if (!Bloodlines.areBloodlinesEnabled) {
                ctx.Reply("Bloodline system is not enabled.");
                return;
            }
            ulong SteamID;
            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity)) {
                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
            } else {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }

            bool typeFound = Bloodlines.nameMap.TryGetValue(type, out int index);
            if (!typeFound) {
                ctx.Reply($"{type} Bloodline not found! did you typo?");
                return;
            }
            Bloodlines.modBloodline(SteamID, index, -100000);
            Bloodlines.modBloodline(SteamID, index, value);
            ctx.Reply(name + "'s " + type + " bloodline set to " + value);
        }

        [Command("log", "l", "<On, Off>", "Turns on or off logging of bloodlineXP gain.", adminOnly: false)]
        public static void logBloodline(ChatCommandContext ctx, string flag)
        {
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            if (flag.ToLower().Equals("on"))
            {
                Database.playerLogBloodline.Remove(SteamID);
                Database.playerLogBloodline.Add(SteamID, true);
                ctx.Reply("Bloodline gain is now being logged.");
                return;
            }
            else if (flag.ToLower().Equals("off"))
            {
                Database.playerLogBloodline.Remove(SteamID);
                Database.playerLogBloodline.Add(SteamID, false);
                ctx.Reply($"Bloodline gain is no longer being logged.");
                return;
            }
        }

        [Command("reset", "r", "[BloodlineName]", "Resets a bloodline to gain more power with it.", adminOnly: false)]
        public static void resetBloodline(ChatCommandContext ctx, string BloodlineName)
        {
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            int type;
            if (!Bloodlines.nameMap.TryGetValue(BloodlineName, out type))
            {
                BloodlineName = BloodlineName.ToLower();
                if (BloodlineName.Equals("dracula")) type = 0;
                else if (BloodlineName.Equals("arwen")) type = 1;
                else if (BloodlineName.Equals("ilvris")) type = 2;
                else if (BloodlineName.Equals("aya")) type = 3;
                else if (BloodlineName.Equals("nytheria")) type = 4;
                else if (BloodlineName.Equals("hadubert")) type = 5;
                else if (BloodlineName.Equals("semika")) type = 7;
                else
                {
                    ctx.Reply("Invalid Arguments");
                    return;
                }
            }
            ctx.Reply("Resetting " + Bloodlines.names[type] + "'s Bloodline");
            Bloodlines.resetBloodline(SteamID, type);
        }
    }
}
