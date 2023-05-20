using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using Unity.Transforms;

namespace RPGMods.Commands
{/*
    [Command("playerinfo, i", Usage = "playerinfo <Name>", Description = "Display the player information details.")]
    public static class PlayerInfo
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length < 1) 
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (!Helper.FindPlayer(ctx.Args[0], false, out var playerEntity, out var userEntity))
            {
                ctx.Reply("Player not found."); 
                return;
            }

            var userData = ctx.EntityManager.GetComponentData<User>(userEntity);

            ulong SteamID = userData.PlatformId;
            string Name = userData.CharacterName.ToString();
            string CharacterEntity = playerEntity.Index.ToString() + ":" + playerEntity.Version.ToString();
            string UserEntity = userEntity.Index.ToString() + ":" + userEntity.Version.ToString();
            var ping = (int) ctx.EntityManager.GetComponentData<Latency>(playerEntity).Value;
            var position = ctx.EntityManager.GetComponentData<Translation>(playerEntity).Value;

            Database.PvPStats.TryGetValue(SteamID, out var pvpStats);
            Database.player_experience.TryGetValue(SteamID, out var exp);

            ctx.Reply($"Name: {Color.White(Name)}");
            ctx.Reply($"SteamID: {Color.White(SteamID.ToString())}");
            ctx.Reply($"Latency: {Color.White(ping.ToString())}s");
            ctx.Reply($"-- Position --");
            ctx.Reply($"X: {Color.White(Math.Round(position.x,2).ToString())} " +
                $"Y: {Color.White(Math.Round(position.y,2).ToString())} " +
                $"Z: {Color.White(Math.Round(position.z,2).ToString())}");
            ctx.Reply($"-- {Color.White("Entities")} --");
            ctx.Reply($"Char Entity: {Color.White(CharacterEntity)}");
            ctx.Reply($"User Entity: {Color.White(UserEntity)}");
            ctx.Reply($"-- {Color.White("Experience")} --");
            ctx.Reply($"Level: {Color.White(ExperienceSystem.convertXpToLevel(exp).ToString())} [{Color.White(exp.ToString())}]");
            ctx.Reply($"-- {Color.White("PvP Stats")} --");

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

                ctx.Reply($"Reputation: {Color.White(pvpStats.Reputation.ToString())}");
                ctx.Reply($"Hostility: {Color.White(hostilityText)}");
                ctx.Reply($"Siege: {Color.White(siegeText)}");
                ctx.Reply($"-- Time Left: {Color.White(tLeft.ToString())} hour(s)");
            }

            ctx.Reply($"K/D: {Color.White(pvpStats.KD.ToString())} " +
                $"Kill: {Color.White(pvpStats.Kills.ToString())} " +
                $"Death: {Color.White(pvpStats.Deaths.ToString())}");
        }
    }

    [Command("myinfo, me", Usage = "myinfo", Description = "Display your information details.")]
    public static class MyInfo
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            string Name = ctx.Event.User.CharacterName.ToString();
            string CharacterEntity = ctx.Event.SenderCharacterEntity.Index.ToString() + ":" + ctx.Event.SenderCharacterEntity.Version.ToString();
            string UserEntity = ctx.Event.SenderUserEntity.Index.ToString() + ":" + ctx.Event.SenderUserEntity.Version.ToString();
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            var position = ctx.EntityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity).Value;

            ctx.Reply($"Name: {Color.White(Name)}");
            ctx.Reply($"SteamID: {Color.White(SteamID.ToString())}");
            ctx.Reply($"Latency: {Color.White(ping.ToString())}s");
            ctx.Reply($"-- Position --");
            ctx.Reply($"X: {Color.White(Math.Round(position.x,2).ToString())} " +
                $"Y: {Color.White(Math.Round(position.y,2).ToString())} " +
                $"Z: {Color.White(Math.Round(position.z,2).ToString())}");
            ctx.Reply($"-- Entities --");
            ctx.Reply($"Char Entity: {Color.White(CharacterEntity)}");
            ctx.Reply($"User Entity: {Color.White(UserEntity)}");
        }
    }*/
}
