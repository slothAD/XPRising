using Bloodstone.API;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class Waypoint
    {
        public static int WaypointLimit = 3;
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command(name: "waypoint", shortHand: "wp", adminOnly: false, usage: "<Name>", description: "Teleports you to the specific waypoint.")]
        public static void WaypoinCommand(ChatCommandContext ctx, string name)
        {
            var PlayerEntity = ctx.Event.SenderCharacterEntity;
            var SteamID = ctx.Event.User.PlatformId;
            if (Helper.IsPlayerInCombat(PlayerEntity))
            {
                throw ctx.Error("Unable to use waypoint! You're in combat!");
            }

            
            if (Database.globalWaypoint.TryGetValue(name, out var WPData))
            {
                Helper.TeleportTo(ctx, WPData.Location);
                return;
            }

            if (Database.waypoints.TryGetValue(name + "_" + SteamID, out var WPData_))
            {
                Helper.TeleportTo(ctx, WPData_.Location);
                return;
            }
        }

        [Command(name: "waypoint set", shortHand: "wp s", adminOnly: false, usage: "<Name>", description: "Creates the specified personal waypoint")]
        public static void WaypointSetCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Database.waypoints_owned.TryGetValue(SteamID, out var total) && !ctx.Event.User.IsAdmin && total >= Waypoint.WaypointLimit)
            {
                if (total >= WaypointLimit)
                {
                    throw ctx.Error("You already have reached your total waypoint limit.");
                }
            }
            var wp_true_name = name + "_" + SteamID;
            if (Database.waypoints.TryGetValue(name, out _))
            {
                throw ctx.Error($"You already have a waypoint with the same name.");
            }

            float3 location = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var f2_location = new float3(location.x, location.y, location.z);
            AddWaypoint(SteamID, f2_location, name, wp_true_name, false);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command(name: "waypoint set global", shortHand: "wp sg", adminOnly: false, usage: "<Name>", description: "Creates the specified global waypoint")]
        public static void WaypointSetGlobalCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Database.globalWaypoint.TryGetValue(name, out _))
            {
                throw ctx.Error($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
            }

            var wp_true_name = name + "_" + SteamID;
            if (Database.waypoints.TryGetValue(name, out _))
            {
                throw ctx.Error($"You already have a waypoint with the same name.");
            }

            float3 location = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var f2_location = new float3(location.x, location.y, location.z);
            AddWaypoint(SteamID, f2_location, name, wp_true_name, true);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command(name: "waypoint remove global", shortHand: "wp rg", adminOnly: false, usage: "<Name>", description: "Removes the specified global waypoint")]
        public static void WaypointremoveGlobalCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (!Database.globalWaypoint.TryGetValue(name, out _))
            {
                throw ctx.Error($"Global \"{name}\" waypoint not found.");
            }
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command(name: "waypoint remove", shortHand: "wp r", adminOnly: false, usage: "<Name>", description: "Removes the specified personal waypoint")]
        public static void WaypointRemoveCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            var wp_name = name + "_" + SteamID;
            if (!Database.waypoints.TryGetValue(wp_name, out _))
            {
                throw ctx.Error($"You do not have any waypoint with this name.");
            }
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command(name: "waypoint list", shortHand: "wp l", adminOnly: false, usage: "",  description: "Lists waypoints available to you")]
        public static void WaypointCommand(ChatCommandContext ctx)
        {
            int total_wp = 0;
            foreach (var global_wp in Database.globalWaypoint)
            {
                ctx.Reply($" - <color=#ffff00>{global_wp.Key}</color> [<color=#00dd00>Global</color>]");
                total_wp++;
            }
            foreach (var wp in Database.waypoints)
            {
                ctx.Reply($" - <color=#ffff00>{wp.Value.Name}</color>");
                total_wp++;
            }
            if (total_wp == 0) throw ctx.Error("No waypoint available.");
        }

        public static void AddWaypoint(ulong owner, float3 location, string name, string true_name, bool isGlobal)
        {
            var WaypointData = new WaypointData(true_name, owner, location);
            if (isGlobal) Database.globalWaypoint[name] = WaypointData;
            else Database.waypoints[name] = WaypointData;
            if (!isGlobal && Database.waypoints_owned.TryGetValue(owner, out var total))
            {
                Database.waypoints_owned[owner] = total + 1;
            }
            else Database.waypoints_owned[owner] = 0;
        }

        public static void RemoveWaypoint(ulong owner, string name, bool global)
        {
            if (global)
            {
                Database.globalWaypoint.Remove(name);
            }
            else
            {
                Database.waypoints_owned[owner] -= 1;
                if (Database.waypoints_owned[owner] < 0) Database.waypoints_owned[owner] = 0;
                Database.waypoints.Remove(name);
            }
        }

        public static void LoadWaypoints()
        {
            if (!File.Exists(Plugin.WaypointsJson))
            {
                var stream = File.Create(Plugin.WaypointsJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.WaypointsJson);
            try
            {
                Database.waypoints = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                Plugin.Logger.LogWarning("Waypoints DB Populated");
            }
            catch
            {
                Database.waypoints = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning("Waypoints DB Created");
            }

            if (!File.Exists(Plugin.GlobalWaypointsJson))
            {
                var stream = File.Create(Plugin.GlobalWaypointsJson);
                stream.Dispose();
            }

            json = File.ReadAllText(Plugin.GlobalWaypointsJson);
            try
            {
                Database.globalWaypoint = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                Plugin.Logger.LogWarning("GlobalWaypoints DB Populated");
            }
            catch
            {
                Database.globalWaypoint = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning("GlobalWaypoints DB Created");
            }

            if (!File.Exists(Plugin.TotalWaypointsJson))
            {
                var stream = File.Create(Plugin.TotalWaypointsJson);
                stream.Dispose();
            }

            json = File.ReadAllText(Plugin.TotalWaypointsJson);
            try
            {
                Database.waypoints_owned = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("TotalWaypoints DB Populated");
            }
            catch
            {
                Database.waypoints_owned = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("TotalWaypoints DB Created");
            }
        }

        public static void SaveWaypoints()
        {
            File.WriteAllText(Plugin.WaypointsJson, JsonSerializer.Serialize(Database.waypoints, Database.JSON_options));
            File.WriteAllText(Plugin.GlobalWaypointsJson, JsonSerializer.Serialize(Database.globalWaypoint, Database.JSON_options));
            File.WriteAllText(Plugin.TotalWaypointsJson, JsonSerializer.Serialize(Database.waypoints_owned, Database.JSON_options));
        }
    }
}
