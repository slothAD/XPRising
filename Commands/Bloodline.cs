using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("bloodline, bl", Usage = "bloodline [<log> <on>|<off>] [<reset> all|(bloodline)]", Description = "Display your current bloodline progression, toggle the gain notification, or reset your bloodline to gain effectiveness.")]
    public static class Bloodline
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static bool detailedStatements = false;
        public static void Initialize(Context ctx) {
            if (!Bloodlines.areBloodlinesEnabled) {
                Output.CustomErrorMessage(ctx, "Bloodline system is not enabled.");
                return;
            }
            /*else {
                Output.CustomErrorMessage(ctx, "The Bloodline system command is not yet coded.");
                return;
            }*/
            var SteamID = ctx.Event.User.PlatformId;

            if (ctx.Args.Length > 1) {
                if (ctx.Args[0].ToLower().Equals("set") && ctx.Args.Length >= 3) {
                    bool isAllowed = true;//ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "mastery_args");
                    if (!isAllowed) return;
                    if (double.TryParse(ctx.Args[2], out double value)) {
                        string CharName = ctx.Event.User.CharacterName.ToString();
                        var UserEntity = ctx.Event.SenderUserEntity;
                        var CharEntity = ctx.Event.SenderCharacterEntity;
                        if (ctx.Args.Length == 4) {
                            string name = ctx.Args[3];
                            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity)) {
                                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                                CharName = name;
                                UserEntity = targetUserEntity;
                                CharEntity = targetEntity;
                            }
                            else {
                                Output.CustomErrorMessage(ctx, $"Could not find specified player \"{name}\".");
                                return;
                            }
                        }
                        string MasteryType = ctx.Args[1].ToLower();
                        int type;
                        if(!Bloodlines.nameMap.TryGetValue(ctx.Args[1], out type)) {
                            if (MasteryType.Equals("dracula")) type = 0;
                            else if (MasteryType.Equals("arwen")) type = 1;
                            else if (MasteryType.Equals("ilvris")) type = 2;
                            else if (MasteryType.Equals("aya")) type = 3;
                            else if (MasteryType.Equals("nytheria")) type = 4;
                            else if (MasteryType.Equals("hadubert")) type = 5;
                            else if (MasteryType.Equals("rei")) type = 6;
                            else {
                                Output.InvalidArguments(ctx);
                                return;
                            }
                        }
                        Bloodlines.modBloodline(SteamID, type, value);
                        Output.SendSystemMessage(ctx, $"{Bloodlines.names[type]}'s bloodline for \"{CharName}\" adjusted by <color=#fffffffe>{value}%</color>");
                        Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                        return;

                    }
                    else {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }
                if (ctx.Args[0].ToLower().Equals("log")) {
                    if (ctx.Args[1].ToLower().Equals("on")) {
                        Database.playerLogBloodline[SteamID] = true;
                        Output.SendSystemMessage(ctx, $"Bloodline gain is now logged.");
                        return;
                    }
                    else if (ctx.Args[1].ToLower().Equals("off")) {
                        Database.playerLogBloodline[SteamID] = false;
                        Output.SendSystemMessage(ctx, $"Bloodline gain is no longer being logged.");
                        return;
                    }
                    else {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }

                if (ctx.Args[0].ToLower().Equals("reset") && ctx.Args.Length >= 2) {

                    string MasteryType = ctx.Args[1].ToLower();
                    int type;
                    if (!Bloodlines.nameMap.TryGetValue(ctx.Args[1], out type)) {
                        if (MasteryType.Equals("dracula")) type = 0;
                        else if (MasteryType.Equals("arwen")) type = 1;
                        else if (MasteryType.Equals("ilvris")) type = 2;
                        else if (MasteryType.Equals("aya")) type = 3;
                        else if (MasteryType.Equals("nytheria")) type = 4;
                        else if (MasteryType.Equals("hadubert")) type = 5;
                        else if (MasteryType.Equals("rei")) type = 6;
                        else {
                            Output.InvalidArguments(ctx);
                            return;
                        }
                    }
                    Output.CustomErrorMessage(ctx, "Resetting " + Bloodlines.names[type] + "'s Bloodline");
                    Bloodlines.resetBloodline(SteamID, type);
                }
            }
            else {
                bool isDataExist = Database.playerBloodline.TryGetValue(SteamID, out var MasteryData);
                if (!isDataExist) {
                    Output.CustomErrorMessage(ctx, "You haven't developed any bloodline...");
                    return;
                }

                Output.SendSystemMessage(ctx, "-- <color=#ffffffff>Bloodlines</color> --");


                if (ctx.Event.SenderCharacterEntity != null) {
                    int bl;
                    string name;
                    double masteryPercent;
                    string print;
                    BloodlineData bld = Bloodlines.getBloodlineData(SteamID);
                    if (detailedStatements) {
                        for (bl = 0; bl < Bloodlines.stats.Length; bl++) {
                            if (bl >= Bloodlines.stats.Length)
                                Output.SendSystemMessage(ctx, $"Bloodline type {bl} beyond bloodline type limit of {Bloodlines.stats.Length - 1}");
                            name = Bloodlines.names[bl];
                            masteryPercent = bld.strength[bl];
                            print = $"{name}:<color=#fffffffe> {masteryPercent}%</color> (";
                            for (int i = 0; i < Bloodlines.stats[bl].Length; i++) {
                                if (bld.strength[bl] >= Bloodlines.minStrengths[bl][i]) {
                                    if (i > 0)
                                        print += ",";
                                    print += Helper.statTypeToString((UnitStatType)Bloodlines.stats[bl][i]);
                                    print += " <color=#75FF33>";
                                    print += Bloodlines.calcBuffValue(bl, SteamID, i);
                                    print += "</color>";
                                }
                            }
                            print += $") Effectiveness: {bld.efficency[bl] * 100}%";
                            Output.SendSystemMessage(ctx, print);
                        }
                    }

                    else {
                        Blood blood = entityManager.GetComponentData<Blood>(ctx.Event.SenderCharacterEntity);
                        Bloodlines.bloodlineMap.TryGetValue(blood.BloodType, out bl);
                        if (bl >= Bloodlines.stats.Length)
                            Output.SendSystemMessage(ctx, $"Bloodline type {bl} beyond bloodline type limit of {Bloodlines.stats.Length - 1}");
                        name = Bloodlines.names[bl];
                        masteryPercent = bld.strength[bl];
                        print = $"{name}:<color=#fffffffe> {masteryPercent}%</color> (";
                        for (int i = 0; i < Bloodlines.stats[bl].Length; i++) {
                            if (bld.strength[bl] >= Bloodlines.minStrengths[bl][i]) {
                                if (i > 0)
                                    print += ",";
                                print += Helper.statTypeToString((UnitStatType)Bloodlines.stats[bl][i]);
                                print += " <color=#75FF33>";
                                print += Bloodlines.calcBuffValue(bl, SteamID, i);
                                print += "</color>";
                            }
                        }
                        print += $") Effectiveness: {bld.efficency[bl] * 100}%";
                        Output.SendSystemMessage(ctx, print);
                    }
                }
            }
        }
    }
}
