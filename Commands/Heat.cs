using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System;
using Unity.Entities;
using ProjectM;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("heat")]
    public static class Heat
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command(name: "info", shortHand: "i", adminOnly: false, usage: "", description: "Shows your current wanted level.")]
        public static void HeatCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;

            if (!HunterHuntedSystem.isActive)
            {
                throw ctx.Error("HunterHunted system is not enabled.");
            }

            string CharName = ctx.Event.User.CharacterName.ToString();

            HunterHuntedSystem.HeatManager(userEntity);

            Cache.heatlevel.TryGetValue(SteamID, out var human_heatlevel);
            if (human_heatlevel >= 1500) Output.SendLore(userEntity,$"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>YOU ARE A MENACE...</color>");
            else if (human_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>The Vampire Hunters are hunting you...</color>");
            else if (human_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>Humans elite squads are hunting you...</color>");
            else if (human_heatlevel >= 250) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c4515cff>Humans soldiers are hunting you...</color>");
            else if (human_heatlevel >= 150) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c9999eff>The humans are hunting you...</color>");
            else Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#ffffffff>You're currently anonymous...</color>");

            Cache.bandit_heatlevel.TryGetValue(SteamID, out var bandit_heatlevel);
            if (bandit_heatlevel >= 650) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c90e21ff>The bandits really wants you dead...</color>");
            else if (bandit_heatlevel >= 450) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c90e21ff>A large bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 250) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c4515cff>A small bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 150) Output.SendLore(userEntity,$"<color=#ff0000ff>[Bandits]</color> <color=#c9999eff>The bandits are hunting you...</color>");
            else Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#ffffffff>The bandits doesn't recognize you...</color>");

        }

        [Command(name: "set", shortHand: "s", adminOnly: false, usage: "<ValueHeatHumans> <ValueHeatBandits> <PlayerName>", description: "Sets a player's wanted level")]
        public static void HeatSetCommand(ChatCommandContext ctx,int valueHuman, int valueBandit, string playerName)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;

            if (!HunterHuntedSystem.isActive)
            {
                throw ctx.Error("HunterHunted system is not enabled.");
            }

            string CharName = ctx.Event.User.CharacterName.ToString();
   
            if (Helper.FindPlayer(playerName, true, out var targetEntity, out var targetUserEntity))
            {
                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                CharName = playerName;
                userEntity = targetUserEntity;
                charEntity = targetEntity;
            }
            else
            {
                throw ctx.Error($"Could not find specified player \"{playerName}\".");
            }

            Cache.heatlevel[SteamID] = valueHuman;
            Cache.bandit_heatlevel[SteamID] = valueBandit;
            ctx.Reply($"Player \"{CharName}\" heat value changed.");
            ctx.Reply($"Human: <color=#ffff00>{Cache.heatlevel[SteamID]}</color> | Bandit: <color=#ffff00>{Cache.bandit_heatlevel[SteamID]}</color>");
            HunterHuntedSystem.HeatManager(userEntity);


            /*
            // TODO: I don't know what this code does exactly
            if (ctx.Args.Length == 1 && user.IsAdmin)
            {
                if (!ctx.Args[0].Equals("debug") && ctx.Args.Length != 2) return;

                Cache.player_last_ambushed.TryGetValue(SteamID, out var last_ambushed);
                TimeSpan since_ambush = DateTime.Now - last_ambushed;
                int NextAmbush = (int)(HunterHuntedSystem.ambush_interval - since_ambush.TotalSeconds);
                if (NextAmbush < 0) NextAmbush = 0;

                ctx.Reply( $"Next Possible Ambush in {Color.White(NextAmbush.ToString())}s");
                ctx.Reply( $"Ambush Chance: {Color.White(HunterHuntedSystem.ambush_chance.ToString())}%");
                ctx.Reply( $"Human: {Color.White(human_heatlevel.ToString())} | Bandit: {Color.White(bandit_heatlevel.ToString())}");
            }*/
        }
    }
}
