using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System;
using Unity.Transforms;
using ProjectM;
using VampireCommandFramework;
using Bloodstone.API;
using Unity.Entities;

namespace OpenRPG.Commands
{
    
    public static class PlayerInfo
    {

        private static EntityManager entityManager = VWorld.Server.EntityManager;

        [Command("playerinfo", shortHand: "pi", adminOnly: false, usage: "[<PlayerName>]", description: "Display the player information details.")]
        public static void PlayerInfoCommand(ChatCommandContext ctx, string playerName = null)
        {

            ulong SteamID = ctx.Event.User.PlatformId;
            string Name = ctx.Event.User.CharacterName.ToString();
            string CharacterEntity = ctx.Event.SenderCharacterEntity.Index.ToString() + ":" + ctx.Event.SenderCharacterEntity.Version.ToString();
            string UserEntity = ctx.Event.SenderUserEntity.Index.ToString() + ":" + ctx.Event.SenderUserEntity.Version.ToString();
            var ping = entityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            var position = entityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity).Value;

            if (playerName == null)
            {
                ctx.Reply($"Name: {Utils.Color.White(Name)}");
                ctx.Reply($"SteamID: {Utils.Color.White(SteamID.ToString())}");
                ctx.Reply($"Latency: {Utils.Color.White(ping.ToString())}s");
                ctx.Reply($"-- Position --");
                ctx.Reply($"X: {Utils.Color.White(Math.Round(position.x, 2).ToString())} " +
                    $"Y: {Utils.Color.White(Math.Round(position.y, 2).ToString())} " +
                    $"Z: {Utils.Color.White(Math.Round(position.z, 2).ToString())}");
                ctx.Reply($"-- Entities --");
                ctx.Reply($"Char Entity: {Utils.Color.White(CharacterEntity)}");
                ctx.Reply($"User Entity: {Utils.Color.White(UserEntity)}");

                return;
            }

            if (!Helper.FindPlayer(playerName, false, out var playerEntity, out var userEntity))
            {
                throw ctx.Error("Player not found.");
            }

            var userData = entityManager.GetComponentData<User>(userEntity);

            SteamID = userData.PlatformId;
            Name = userData.CharacterName.ToString();
            CharacterEntity = playerEntity.Index.ToString() + ":" + playerEntity.Version.ToString();
            UserEntity = userEntity.Index.ToString() + ":" + userEntity.Version.ToString();
            ping = (int)entityManager.GetComponentData<Latency>(playerEntity).Value;
            position = entityManager.GetComponentData<Translation>(playerEntity).Value;

            Database.PvPStats.TryGetValue(SteamID, out var pvpStats);
            Database.player_experience.TryGetValue(SteamID, out var exp);

            ctx.Reply($"Name: {Utils.Color.White(Name)}");
            ctx.Reply($"SteamID: {Utils.Color.White(SteamID.ToString())}");
            ctx.Reply($"Latency: {Utils.Color.White(ping.ToString())}s");
            ctx.Reply($"-- Position --");
            ctx.Reply($"X: {Utils.Color.White(Math.Round(position.x, 2).ToString())} " +
                $"Y: {Utils.Color.White(Math.Round(position.y, 2).ToString())} " +
                $"Z: {Utils.Color.White(Math.Round(position.z, 2).ToString())}");
            ctx.Reply($"-- {Utils.Color.White("Entities")} --");
            ctx.Reply($"Char Entity: {Utils.Color.White(CharacterEntity)}");
            ctx.Reply($"User Entity: {Utils.Color.White(UserEntity)}");
            ctx.Reply($"-- {Utils.Color.White("Experience")} --");
            ctx.Reply($"Level: {Utils.Color.White(ExperienceSystem.convertXpToLevel(exp).ToString())} [{Utils.Color.White(exp.ToString())}]");
            ctx.Reply($"-- {Utils.Color.White("PvP Stats")} --");

            if (PvPSystem.isHonorSystemEnabled)
            {
                Database.SiegeState.TryGetValue(SteamID, out var siegeState);
                Cache.HostilityState.TryGetValue(playerEntity, out var hostilityState);

                double tLeft = 0;
                if (siegeState.IsSiegeOn)
                {
                    TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                    tLeft = Math.Round(TimeLeft.TotalHours, 2);
                }

                string hostilityText = hostilityState.IsHostile ? "Aggresive" : "Passive";
                string siegeText = siegeState.IsSiegeOn ? "Sieging" : "Defensive";

                ctx.Reply($"Reputation: {Utils.Color.White(pvpStats.Reputation.ToString())}");
                ctx.Reply($"Hostility: {Utils.Color.White(hostilityText)}");
                ctx.Reply($"Siege: {Utils.Color.White(siegeText)}");
                ctx.Reply($"-- Time Left: {Utils.Color.White(tLeft.ToString())} hour(s)");
            }

            ctx.Reply($"K/D: {Utils.Color.White(pvpStats.KD.ToString())} " +
                $"Kill: {Utils.Color.White(pvpStats.Kills.ToString())} " +
                $"Death: {Utils.Color.White(pvpStats.Deaths.ToString())}");
        }

        /*       [Command("myinfo", shortHand: "aaaaaaaaaa", adminOnly: false, usage: "", description: "Display your information details.")]
               public static void MyInfoCommand(ChatCommandContext ctx)
               {
                   ulong SteamID = ctx.Event.User.PlatformId;
                   string Name = ctx.Event.User.CharacterName.ToString();
                   string CharacterEntity = ctx.Event.SenderCharacterEntity.Index.ToString() + ":" + ctx.Event.SenderCharacterEntity.Version.ToString();
                   string UserEntity = ctx.Event.SenderUserEntity.Index.ToString() + ":" + ctx.Event.SenderUserEntity.Version.ToString();
                   var ping = entityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
                   var position = entityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity).Value;

                   ctx.Reply($"Name: {Utils.Color.White(Name)}");
                   ctx.Reply($"SteamID: {Utils.Color.White(SteamID.ToString())}");
                   ctx.Reply($"Latency: {Utils.Color.White(ping.ToString())}s");
                   ctx.Reply($"-- Position --");
                   ctx.Reply($"X: {Utils.Color.White(Math.Round(position.x, 2).ToString())} " +
                       $"Y: {Utils.Color.White(Math.Round(position.y, 2).ToString())} " +
                       $"Z: {Utils.Color.White(Math.Round(position.z, 2).ToString())}");
                   ctx.Reply($"-- Entities --");
                   ctx.Reply($"Char Entity: {Utils.Color.White(CharacterEntity)}");
                   ctx.Reply($"User Entity: {Utils.Color.White(UserEntity)}");
               }
        */
    }
}
