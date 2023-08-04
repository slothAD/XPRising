using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class PvP
    {
        [Command("pvp", shortHand: "pvp", adminOnly: false, usage: "[<on>|<off>|<top> <PlayerName>]", description: "Display your PvP statistics or toggle PvP/Castle Siege state")]
        public static void PvPCommand(ChatCommandContext ctx, string type = null, string playerName = null)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            if (type == null)
            {
                Database.PvPStats.TryGetValue(SteamID, out var PvPStats);

                ctx.Reply($"Name: {Utils.Color.White(CharName)}");
                if (PvPSystem.isHonorSystemEnabled)
                {
                    Database.SiegeState.TryGetValue(SteamID, out var siegeState);
                    Cache.HostilityState.TryGetValue(charEntity, out var hostilityState);

                    double tLeft = 0;
                    if (siegeState.IsSiegeOn)
                    {
                        TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                        tLeft = Math.Round(TimeLeft.TotalHours, 2);
                        if (PvPStats.Reputation <= -20000)
                        {
                            tLeft = -1;
                        }
                    }

                    string hostilityText = hostilityState.IsHostile ? "Aggresive" : "Passive";
                    string siegeText = siegeState.IsSiegeOn ? "Sieging" : "Defensive";

                    Cache.ReputationLog.TryGetValue(SteamID, out var RepLog);
                    TimeSpan ReputationSpan = DateTime.Now - RepLog.TimeStamp;

                    var TimeLeftUntilRefresh = PvPSystem.HonorGainSpanLimit - ReputationSpan.TotalMinutes;
                    if (TimeLeftUntilRefresh > 0)
                    {
                        TimeLeftUntilRefresh = Math.Round(TimeLeftUntilRefresh, 2);
                    }
                    else
                    {
                        TimeLeftUntilRefresh = 0;
                        RepLog.TotalGained = 0;
                    }

                    int HonorGainLeft = PvPSystem.MaxHonorGainPerSpan - RepLog.TotalGained;

                    ctx.Reply($"Reputation: {Utils.Color.White(PvPStats.Reputation.ToString())}");
                    ctx.Reply($"-- Time Left Until Refresh: {Utils.Color.White(TimeLeftUntilRefresh.ToString())} minute(s)");
                    ctx.Reply($"-- Available Reputation Gain: {Utils.Color.White(HonorGainLeft.ToString())} point(s)");
                    ctx.Reply($"Hostility: {Utils.Color.White(hostilityText)}");
                    ctx.Reply($"Siege: {Utils.Color.White(siegeText)}");
                    ctx.Reply($"-- Time Left: {Utils.Color.White(tLeft.ToString())} hour(s)");
                }
                ctx.Reply($"K/D: {Utils.Color.White(PvPStats.KD.ToString())} [{Utils.Color.White(PvPStats.Kills.ToString())}/{Utils.Color.White(PvPStats.Deaths.ToString())}]");
            }
            else
            {
                var isPvPShieldON = false;

                if (type.ToLower().Equals("on")) isPvPShieldON = false;
                else if (type.ToLower().Equals("off")) isPvPShieldON = true;

                if (playerName == null)
                {
                    if (type.ToLower().Equals("top"))

                    {
                        if (PvPSystem.isLadderEnabled)
                        {
                            _ = PvPSystem.TopRanks(ctx);
                            return;
                        }
                        else
                        {
                            throw ctx.Error("Leaderboard is not enabled.");
                        }
                    }

                    if (PvPSystem.isHonorSystemEnabled)
                    {
                        if (Helper.IsPlayerInCombat(charEntity))
                        {
                            throw ctx.Error($"Unable to change state, you are in combat!");
                        }

                        Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
                        Database.SiegeState.TryGetValue(SteamID, out var siegeState);

                        if (type.ToLower().Equals("on"))
                        {
                            PvPSystem.HostileON(SteamID, charEntity, userEntity);
                            ctx.Reply("Entering aggresive state!");
                            return;
                        }
                        else if (type.ToLower().Equals("off"))
                        {
                            if (PvPStats.Reputation <= -1000)
                            {
                                throw ctx.Error($"You're [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], aggresive state is enforced.");
                            }

                            if (siegeState.IsSiegeOn)
                            {
                                throw ctx.Error($"You're in siege mode, aggressive state is enforced.");
                            }
                            PvPSystem.HostileOFF(SteamID, charEntity);
                            ctx.Reply("Entering passive state!");
                            return;
                        }
                    }
                    else
                    {
                        if (!PvPSystem.isPvPToggleEnabled)
                        {
                            throw ctx.Error("PvP toggling is not enabled!");
                        }
                        if (Helper.IsPlayerInCombat(charEntity))
                        {
                            throw ctx.Error($"Unable to change PvP Toggle, you are in combat!");
                        }
                        Helper.SetPvPShield(charEntity, isPvPShieldON);
                        string s = isPvPShieldON ? "OFF" : "ON";
                        ctx.Reply($"PvP is now {s}");
                        return;
                    }
                    return;
                }
                else if (playerName != null && ctx.Event.User.IsAdmin)
                {

                    if (PvPSystem.isHonorSystemEnabled)
                    {
                        string name = playerName;
                        if (Helper.FindPlayer(name, false, out Entity targetChar, out Entity targetUser))
                        {
                            SteamID = Plugin.Server.EntityManager.GetComponentData<User>(targetUser).PlatformId;
                            Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
                            if (type.ToLower().Equals("on"))
                            {
                                PvPSystem.HostileON(SteamID, targetChar, targetUser);
                                ctx.Reply($"Vampire \"{name}\" is now in aggresive state!");
                                return;
                            }
                            else if (type.ToLower().Equals("off"))
                            {
                                if (PvPStats.Reputation <= -1000)
                                {
                                    throw ctx.Error($"Vampire \"{name}\" is [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], aggresive state is enforced.");
                                }
                                PvPSystem.HostileOFF(SteamID, targetChar);
                                ctx.Reply($"Vampire \"{name}\" is now in passive state!");
                                return;
                            }
                            return;
                        }
                        else
                        {
                            throw ctx.Error($"Unable to find the specified player!");
                        }
                    }
                    else
                    {
                        string name = playerName;
                        if (Helper.FindPlayer(name, false, out Entity targetChar, out _))
                        {
                            Helper.SetPvPShield(targetChar, isPvPShieldON);
                            string s = isPvPShieldON ? "OFF" : "ON";
                            ctx.Reply($"Player \"{name}\" PvP is now {s}");
                            return;
                        }
                        else
                        {
                            throw ctx.Error($"Unable to find the specified player!");
                        }
                    }
                }
            }
        }

        [Command("pvp req", shortHand: "aaaaaaaaaa", adminOnly: false, usage: "<ammount> <PlayerName", description: "Display your PvP statistics or toggle PvP/Castle Siege state")]
        public static void PvPReqCommand(ChatCommandContext ctx, int amount, string name)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;



            if (amount > 9999) amount = 9999;
            if (Helper.FindPlayer(name, false, out _, out var targetUser))
            {
                SteamID = Plugin.Server.EntityManager.GetComponentData<User>(targetUser).PlatformId;
            }
            else
            {
                throw ctx.Error($"Unable to find the specified player!");
            }

            Database.PvPStats.TryGetValue(SteamID, out var PvPData);
            PvPData.Reputation = amount;
            Database.PvPStats[SteamID] = PvPData;
            ctx.Reply($"Player \"{name}\" reputation is now set to {amount}");


        }
    }
}
