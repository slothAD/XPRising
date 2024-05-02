using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands{
    [CommandGroup("mastery", "m")]
    public static class Mastery{
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command("get", "g", "[weaponType]", "Display your current mastery progression for your equipped or specified weapon type")]
        public static void getMastery(ChatCommandContext ctx, string weaponType = "") {
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

            int type;
            if (string.IsNullOrEmpty(weaponType))
            {
                var equipedType = WeaponMasterSystem.GetWeaponType(ctx.Event.SenderCharacterEntity);
                type = (int)equipedType + 1;
            }
            else if (!WeaponMasterSystem.nameMap.TryGetValue(weaponType.ToLower(), out type))
            {
                ctx.Reply($"Mastery type not found! did you typo?");
                return;
            }
            
            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");

            ctx.Reply(getMasteryDataStringForType(SteamID, type));
        }

        [Command("get-all", "ga", "", "Display your current mastery progression in everything")]
        public static void getAllMastery(ChatCommandContext ctx) {
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

            for (var weapon = 0; weapon < WeaponMasterSystem.masteryStats.Length; weapon++) {
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
                double val = WeaponMasterSystem.calcBuffValue(weapon, masteryPercent, SteamID, i);
                if (Helper.inverseMultipersDisplayReduction && Helper.inverseMultiplierStats.Contains(WeaponMasterSystem.masteryStats[weapon][i])) {
                    val = 1 - val;
                }
                print += $"{val:F3}";
                print += "</color>";
            }
            print += $") Effectiveness: {effectiveness * 100}%, Growth: {growth * 100}%";
            return print ;
        }

        [Command("add", "a", "<weaponType> <amount>", "Adds the amount to the mastery of the specified weaponType", adminOnly: true)]
        public static void addMastery(ChatCommandContext ctx, string weaponType, double amount){
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            var CharName = ctx.Event.User.CharacterName.ToString();
            var UserEntity = ctx.Event.SenderUserEntity;
            var CharEntity = ctx.Event.SenderCharacterEntity;

            var MasteryType = weaponType.ToLower();
            if (!WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out var type)) {
                ctx.Reply($"Mastery type not found! did you typo?");
                return;
            }

            WeaponMasterSystem.modMastery(SteamID, type, amount);
            ctx.Reply($"{MasteryType} Mastery for \"{CharName}\" adjusted by <color=#fffffffe>{amount:F2}%</color>");
            Helper.ApplyBuff(UserEntity, CharEntity, Helper.AppliedBuff);
        }
        
        [Command("set", "s", "<playerName> <weaponType> <masteryValue>", "Sets the specified player's mastery to a specific value", adminOnly: true)]
        public static void setMastery(ChatCommandContext ctx, string name, string weaponType, double value) {
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

            var MasteryType = weaponType.ToLower();
            if (!WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out var type)) {
                ctx.Reply($"Mastery type not found! did you typo?");
                return;
            }

            WeaponMasterSystem.modMastery(SteamID, type, -100000);
            WeaponMasterSystem.modMastery(SteamID, type, (value));
            ctx.Reply($"{MasteryType} Mastery for \"{name}\" set to <color=#fffffffe>{value:F2}%</color>");
        }

        [Command("log", "l", "", "Toggles logging of mastery gain.", adminOnly: false)]
        public static void logMastery(ChatCommandContext ctx, string flag)
        {
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            var currentValue = Database.player_log_mastery.GetValueOrDefault(SteamID, false);
            Database.player_log_mastery.Remove(SteamID);
            if (currentValue)
            {
                Database.player_log_mastery.Add(SteamID, false);
                ctx.Reply($"Mastery gain is no longer being logged.");
            }
            else
            {
                Database.player_log_mastery.Add(SteamID, true);
                ctx.Reply("Mastery gain is now being logged.");
            }
        }


        [Command("reset", "r", "<weaponType>", "Resets a mastery to gain more power with it.", adminOnly: false)]
        public static void resetMastery(ChatCommandContext ctx, string weaponType)
        {
            if (!WeaponMasterSystem.isMasteryEnabled){
                ctx.Reply("Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            var MasteryType = weaponType.ToLower();
            if (!WeaponMasterSystem.nameMap.TryGetValue(MasteryType, out var type)) {
                ctx.Reply($"{MasteryType} Mastery type not found! did you typo?");
                return;
            }
            ctx.Reply("Resetting " + MasteryType + " Mastery");
            WeaponMasterSystem.resetMastery(SteamID, type);
        }
    }
}
