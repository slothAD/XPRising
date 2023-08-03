using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class Mastery
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command("mastery", usage:"mastery [<log> <on>|<off>]", description: "Display your current mastery progression, or toggle the gain notification.")]
        public static void MasteryCommand(ChatCommandContext ctx)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                throw ctx.Error("Weapon Mastery system is not enabled.");
            }
            var SteamID = ctx.Event.User.PlatformId;

            bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
            if (!isDataExist)
            {
                throw ctx.Error("You haven't even tried to master anything...");
            }

            ctx.Reply("-- <color=#ffffffff>Weapon Mastery</color> --");
            ctx.Reply($"Sword:<color=#fffffffe> {(double)MasteryData.Sword * 0.001}%</color> (ATK <color=#75FF33>↑</color>, SPL <color=#75FF33>↑</color>)");
            ctx.Reply($"Spear:<color=#fffffffe> {(double)MasteryData.Spear * 0.001}%</color> (ATK <color=#75FF33>↑↑</color>)");
            ctx.Reply($"Axes:<color=#fffffffe> {(double)MasteryData.Axes * 0.001}%</color> (ATK <color=#75FF33>↑</color>, HP <color=#75FF33>↑</color>)");
            ctx.Reply($"Scythe:<color=#fffffffe> {(double)MasteryData.Scythe * 0.001}%</color> (ATK <color=#75FF33>↑</color>, CRIT <color=#75FF33>↑</color>)");
            ctx.Reply($"Slashers:<color=#fffffffe> {(double)MasteryData.Slashers * 0.001}%</color> (CRIT <color=#75FF33>↑</color>, MOV <color=#75FF33>↑</color>)");
            ctx.Reply($"Mace:<color=#fffffffe> {(double)MasteryData.Mace * 0.001}%</color> (HP <color=#75FF33>↑↑</color>)");
            ctx.Reply($"None:<color=#fffffffe> {(double)MasteryData.None * 0.001}%</color> (ATK <color=#75FF33>↑↑</color>, MOV <color=#75FF33>↑↑</color>)");
            ctx.Reply($"Spell:<color=#fffffffe> {(double)MasteryData.Spell * 0.001}%</color> (CD <color=#75FF33>↓↓</color>)");
            ctx.Reply($"Crossbow:<color=#fffffffe> {(double)MasteryData.Crossbow * 0.001}%</color> (CRIT <color=#75FF33>↑↑</color>)");
            //ctx.Reply( $"Fishing Pole: <color=#ffffffff>{(double)MasteryData.FishingPole * 0.001}%</color> (??? ↑↑)");
            
        }

        [Command("mastery log", usage: "<True|False>", description: "Display your current mastery progression, or toggle the gain notification.")]
        public static void MasteryLogCommand(ChatCommandContext ctx, bool log)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                throw ctx.Error("Weapon Mastery system is not enabled.");
            }
            var SteamID = ctx.Event.User.PlatformId;

            Database.player_log_mastery[SteamID] = log;

            if (log)
            {
                ctx.Reply($"Mastery gain is now logged.");
            }
            else
            {
                ctx.Reply($"Mastery gain is no longer being logged.");
            }
                
        }

        [Command("mastery set", usage: " <type> <value> [<PlayerName>]", description: "Display your current mastery progression, or toggle the gain notification.")]
        public static void MasterySetCommand(ChatCommandContext ctx, string type, int value, string playerName = null)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                throw ctx.Error("Weapon Mastery system is not enabled.");
            }

            var SteamID = ctx.Event.User.PlatformId;
            string CharName = ctx.Event.User.CharacterName.ToString();
            var UserEntity = ctx.Event.SenderUserEntity;
            var CharEntity = ctx.Event.SenderCharacterEntity;

            if (playerName != null)
            {
                string name = playerName;
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                    CharName = name;
                    UserEntity = targetUserEntity;
                    CharEntity = targetEntity;
                }
                else
                {
                    throw ctx.Error($"Could not find specified player \"{name}\".");
                }
            }

            string MasteryType = type;
            if (MasteryType.Equals("sword")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Sword, value);
            else if (MasteryType.Equals("none")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.None, value);
            else if (MasteryType.Equals("spear")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Spear, value);
            else if (MasteryType.Equals("crossbow")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Crossbow, value);
            else if (MasteryType.Equals("slashers")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Slashers, value);
            else if (MasteryType.Equals("scythe")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Scythe, value);
            else if (MasteryType.Equals("fishingpole")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.FishingPole, value);
            else if (MasteryType.Equals("mace")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Mace, value);
            else if (MasteryType.Equals("axes")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Axes, value);
            else
            {
                throw ctx.Error($"Could not find specified MasteryType \"{type}\".");
            }
            ctx.Reply($"{type} Mastery for \"{CharName}\" adjusted by <color=#fffffffe>{value * 0.001}%</color>");
            Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }
    }
}
