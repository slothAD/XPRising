using OpenRPG.Utils;
using System;
using OpenRPG.Models;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace OpenRPG.Commands {
    [CommandGroup("waypoint", "wp")]
    public static class Waypoint {
        public static int WaypointLimit = 3;

        [Command("go", "g", "<waypoint name>", "Teleports you to the specified waypoint", adminOnly: false)]
        public static void GoToWaypoint(ChatCommandContext ctx, string waypoint) {
            var steamID = ctx.Event.User.PlatformId;
            if (Cache.PlayerInCombat(steamID) && !ctx.IsAdmin) {
                ctx.Reply("Unable to use waypoint! You're in combat!");
                return;
            }

            if (Database.waypoints.TryGetValue(waypoint, out var wpData)) {
                Helper.TeleportTo(ctx, wpData.ToFloat3());
                return;
            }

            if (Database.waypoints.TryGetValue(waypoint + "_" + steamID, out wpData)) {
                if (WaypointLimit <= 0 && !ctx.IsAdmin) {
                    ctx.Reply("Personal Waypoints are forbidden to you.");
                    if(Database.waypoints.Remove(waypoint + "_" + steamID)) {
                        ctx.Reply("The forbidden waypoint has been destroyed.");
                    }
                    return;
                }
                Helper.TeleportTo(ctx, wpData.ToFloat3());
                return;
            }
            ctx.Reply("Waypoint not found.");
        }


        [Command("set", "s", "<waypoint name>", "Creates the specified personal waypoint", adminOnly: false)]
        public static void SetWaypoint(ChatCommandContext ctx, string name) {
            if(WaypointLimit <= 0 && !ctx.IsAdmin) {
                ctx.Reply("You may not create waypoints.");
                return;
            }
            var steamID = ctx.Event.User.PlatformId;
            if (Database.waypoints.TryGetValue(name, out _)) {
                ctx.Reply($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
                return;
            }
            if (Database.waypoints_owned.TryGetValue(steamID, out var total) && !ctx.IsAdmin) {
                if (total >= WaypointLimit) {
                    ctx.Reply("You already have reached your total waypoint limit.");
                    return;
                }
            }
            string waypointName = name + "_" + steamID;
            if (Database.waypoints.TryGetValue(name, out _)) {
                ctx.Reply($"You already have a waypoint with the same name.");
                return;
            }
            var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            AddWaypoint(steamID, location, waypointName, name, false);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command("set global", "sg", "<waypoint name>", "Creates the specified global waypoint", adminOnly: true)]
        public static void SetGlobalWaypoint(ChatCommandContext ctx, string name) {
            var steamID = ctx.Event.User.PlatformId;
            var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            AddWaypoint(steamID, location, name, name, true);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command("remove global", "rg", "<waypoint name>", "Removes the specified global waypoint", adminOnly: true)]
        public static void RemoveGlobalWaypoint(ChatCommandContext ctx, string name) {
            var steamID = ctx.Event.User.PlatformId;
            RemoveWaypoint(steamID, name, true);
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command("remove", "r", "<waypoint name>", "Removes the specified personal waypoint", adminOnly: false)]
        public static void RemoveWaypoint(ChatCommandContext ctx, string name) {
            var steamID = ctx.Event.User.PlatformId;
            string waypointName = name + "_" + steamID;
            if (!Database.waypoints.TryGetValue(waypointName, out _)) {
                ctx.Reply($"You do not have any waypoint with this name.");
                return;
            }
            RemoveWaypoint(steamID, waypointName, false);
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command("list", "l", "", "lists waypoints available to you", adminOnly: false)]
        public static void ListWaypoints(ChatCommandContext ctx) {
            var steamID = ctx.Event.User.PlatformId;
            int totalWaypoints = 0;
            int count = 0;
            int wpPerMsg = 5;
            string reply = "";
            foreach (var wp in Database.waypoints) {
                if(!wp.Key.Contains("_")) {
                    if (count < wpPerMsg) {
                        reply += $" - <color=#ffff00>{wp.Key}</color> [<color=#00dd00>Global</color>]";
                        count++;
                    } else {
                        ctx.Reply(reply);
                        reply = "";
                        count = 0;
                    }
                    totalWaypoints++;
                }

                if (wp.Key.Contains(steamID.ToString())) {
                    if (count < wpPerMsg) {
                        string easyName = wp.Key.Substring(0, wp.Key.IndexOf("_", StringComparison.Ordinal));
                        reply += $" - <color=#ffff00>{easyName}</color>";
                        count++;
                    } else {
                        ctx.Reply(reply);
                        reply = "";
                        count = 0;
                    }
                    totalWaypoints++;
                }
            }
            if (count > 0) {
                ctx.Reply(reply);
            }
            if (totalWaypoints == 0) ctx.Reply("No waypoint available.");

        }

        private static void AddWaypoint(ulong owner, float3 location, string name, string trueName, bool isGlobal) {
            if (isGlobal) Database.waypoints[trueName] = new WaypointData(location);
            else Database.waypoints[name] = new WaypointData(location);
            if (!isGlobal && Database.waypoints_owned.TryGetValue(owner, out var total)) {
                Database.waypoints_owned[owner] = total + 1;
            } else if(!isGlobal) Database.waypoints_owned[owner] = 1;
        }

        private static void RemoveWaypoint(ulong owner, string name, bool global) {
            if (global) {
                Database.waypoints.Remove(name);
            } else {
                Database.waypoints_owned[owner] -= 1;
                if (Database.waypoints_owned[owner] < 0) Database.waypoints_owned[owner] = 0;
                Database.waypoints.Remove(name);
            }
        }
    }
}
