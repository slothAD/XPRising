using ProjectM;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System;
using System.Collections.Generic;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class Siege
    {
        
        private static Dictionary<ulong, DateTime> SiegeConfirm = new();

        [Command("siege", shortHand: "siege", adminOnly: false, usage: "[<on>|<off>]", description: "Display all players currently in siege mode, or engage siege mode.")]
        public static void Initialize(ChatCommandContext ctx, string value = null)
        {
            if (PvPSystem.isHonorSystemEnabled == false || PvPSystem.isHonorBenefitEnabled == false)
            {
                throw ctx.Error("Honor system is not enabled.");
            }

            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
            Database.SiegeState.TryGetValue(SteamID, out var siegeState);

            if (value == null)
            {
                if (siegeState.IsSiegeOn)
                {
                    if (PvPStats.Reputation <= -20000)
                    {
                        throw ctx.Error( $"You're [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], siege mode is enforced.");
                    }
                    TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                    double tLeft = Math.Round(TimeLeft.TotalHours, 2);

                    ctx.Reply( $"Siege mode will end in {Utils.Color.White(tLeft.ToString())} hour(s)");
                }
                else
                {
                    ctx.Reply($"You're currently in defensive mode.");
                }

                _ = PvPSystem.SiegeList(ctx);
                return;
            }

            if (value.ToLower().Equals("on"))
            {
                bool doConfirm = SiegeConfirm.TryGetValue(SteamID, out DateTime TimeStamp);
                if (doConfirm)
                {
                    TimeSpan span = DateTime.Now - TimeStamp;
                    if (span.TotalSeconds > 60)
                    {
                        doConfirm = false;
                    }
                }

                if (!doConfirm)
                {
                    if (Database.SiegeState.TryGetValue(SteamID, out var siegeData))
                    {
                        if (siegeData.IsSiegeOn)
                        {
                            throw ctx.Error( "You're already in active siege mode.");
                        }
                    }

                    ctx.Reply( "Are you sure you want to enter castle siege mode?");
                    TimeSpan TimeLeft = DateTime.Now.AddMinutes(PvPSystem.SiegeDuration) - DateTime.Now;
                    double calcHours = Math.Round(TimeLeft.TotalHours, 2);
                    ctx.Reply( "You and your allies will not be able to exit siege mode for (" + calcHours + ") hours once you start.");
                    ctx.Reply("Type \" .rpg siege on\" again to confirm.");

                    SiegeConfirm.Add(SteamID, DateTime.Now);
                    return;
                }
                else
                {
                    PvPSystem.SiegeON(SteamID, charEntity, userEntity);
                    SiegeConfirm.Remove(SteamID);
                    ctx.Reply( "Active siege mode engaged.");
                    return;
                }
            }
            else if (value.ToLower().Equals("off"))
            {
                Helper.GetAllies(charEntity, out var allies);
                if (allies.AllyCount > 0)
                {
                    allies.Allies.Add(userEntity, charEntity);
                    foreach(var ally in allies.Allies)
                    {
                        Cache.HostilityState.TryGetValue(ally.Value, out var hostilityState);
                        Database.PvPStats.TryGetValue(hostilityState.SteamID, out var stats);
                        if (stats.Reputation <= -20000)
                        {
                            PvPStats.Reputation = -20000;
                            break;
                        }
                    }
                }

                if (PvPStats.Reputation <= -20000)
                {
                    throw ctx.Error( $"You or your allies are [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], siege mode is enforced.");
                }
                TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                
                if (TimeLeft.TotalSeconds <= 0)
                {
                    PvPSystem.SiegeOFF(SteamID, charEntity);
                    ctx.Reply( "Defensive siege mode engaged.");
                    return;
                }
                else
                {
                    double tLeft = Math.Round(TimeLeft.TotalHours, 2);
                    ctx.Reply( $"Siege mode cannot be ended until {Utils.Color.White(tLeft.ToString())} more hour(s)");
                    return;
                }
            }
            else if (value.ToLower().Equals("global") && ctx.Event.User.IsAdmin)
            {
                if (PvPSystem.Interlocked.isSiegeOn)


                {
                    PvPSystem.Interlocked.isSiegeOn = false;
                    ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, "Server wide siege mode has been deactivated!");
                }
                else
                {
                    PvPSystem.Interlocked.isSiegeOn = true;
                    ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, "Server wide siege mode is now active!");
                }
            }
            else
            {
                throw ctx.Error("Invalid arguments");
            }
        }
    }
}
