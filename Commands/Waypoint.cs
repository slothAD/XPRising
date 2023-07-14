using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace RPGMods.Commands {
    [CommandGroup("waypoint", "wp")]
    public static class Waypoint {
        public static int WaypointLimit = 3;

        [Command("go", "g", "<waypoint Name>", "Teleports you to the specified waypoint", adminOnly: false)]
        public static void goToWaypoint(ChatCommandContext ctx, string waypoint) {
            var PlayerEntity = ctx.Event.SenderCharacterEntity;
            var SteamID = ctx.Event.User.PlatformId;
            if (Helper.IsPlayerInCombat(PlayerEntity)) {
                ctx.Reply("Unable to use waypoint! You're in combat!");
                return;
            }

            if (Database.waypointDBNew.TryGetValue(waypoint, out var WPData)) {
                Helper.TeleportTo(ctx, WPData);
                return;
            }

            if (Database.waypointDBNew.TryGetValue(waypoint + "_" + SteamID, out var WPData_)) {
                if (WaypointLimit <= 0 && !ctx.Event.User.IsAdmin) {
                    ctx.Reply("Personal Waypoints are forbidden to you.");
                    if(Database.waypointDBNew.Remove(waypoint + "_" + SteamID)) {
                        ctx.Reply("The forbidden waypoint has been destroyed.");
                    }
                    return;
                }
                Helper.TeleportTo(ctx, WPData_);
                return;
            }
            ctx.Reply("Waypoint not found.");
        }


        [Command("set", "s", "<waypoint name>", "Creates the specified personal waypoint", adminOnly: false)]
        public static void setWaypoint(ChatCommandContext ctx, string name) {
            if(WaypointLimit <= 0 && !ctx.Event.User.IsAdmin) {
                ctx.Reply("You may not create waypoints.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;
            if (Database.waypointDBNew.TryGetValue(name, out _)) {
                ctx.Reply($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
                return;
            }
            if (Database.waypoints_owned.TryGetValue(SteamID, out var total) && !ctx.Event.User.IsAdmin) {
                if (total >= WaypointLimit) {
                    ctx.Reply("You already have reached your total waypoint limit.");
                    return;
                }
            }
            string wp_name = name + "_" + SteamID;
            if (Database.waypointDBNew.TryGetValue(name, out _)) {
                ctx.Reply($"You already have a waypoint with the same name.");
                return;
            }
            var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var f3_location = new Tuple<float, float, float>(location.x, location.y, location.z);
            AddWaypoint(SteamID, f3_location, wp_name, name, false);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command("set global", "sg", "<waypoint name>", "Creates the specified global waypoint", adminOnly: true)]
        public static void setGlobalWaypoint(ChatCommandContext ctx, string name) {
            var SteamID = ctx.Event.User.PlatformId;
            var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var f3_location = new Tuple<float, float, float>(location.x, location.y, location.z);
            AddWaypoint(SteamID, f3_location, name, name, true);
            ctx.Reply("Successfully added Waypoint.");
        }

        [Command("remove global", "rg", "<waypoint name>", "Removes the specified global waypoint", adminOnly: true)]
        public static void removeGlobalWaypoint(ChatCommandContext ctx, string name) {
            var SteamID = ctx.Event.User.PlatformId;
            RemoveWaypoint(SteamID, name, true);
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command("remove", "r", "<waypoint name>", "Removes the specified personal waypoint", adminOnly: false)]
        public static void removeWaypoint(ChatCommandContext ctx, string name) {
            var SteamID = ctx.Event.User.PlatformId;
            string wp_name = name + "_" + SteamID;
            if (!Database.waypointDBNew.TryGetValue(wp_name, out _)) {
                ctx.Reply($"You do not have any waypoint with this name.");
                return;
            }
            RemoveWaypoint(SteamID, wp_name, false);
            ctx.Reply("Successfully removed Waypoint.");
        }

        [Command("list", "l", "", "lists waypoints available to you", adminOnly: false)]
        public static void listWaypoints(ChatCommandContext ctx) {
            var SteamID = ctx.Event.User.PlatformId;
            int total_wp = 0;
            int count = 0;
            int wpPerMsg = 5;
            string reply = "";
            foreach (var wp in Database.waypointDBNew) {
                if(!wp.Key.Contains("_")) {
                    if (count < wpPerMsg) {
                        reply += $" - <color=#ffff00>{wp.Key}</color> [<color=#00dd00>Global</color>]";
                        count++;
                    } else {
                        ctx.Reply(reply);
                        reply = "";
                        count = 0;
                    }
                    total_wp++;
                }

                if (wp.Key.Contains(SteamID.ToString())) {
                    if (count < wpPerMsg) {
                        string easyName = wp.Key.Substring(0, wp.Key.IndexOf("_"+SteamID));
                        reply += $" - <color=#ffff00>{easyName}</color>";
                        count++;
                    } else {
                        ctx.Reply(reply);
                        reply = "";
                        count = 0;
                    }
                    total_wp++;
                }
            }
            if (count > 0) {
                ctx.Reply(reply);
            }
            if (total_wp == 0) ctx.Reply("No waypoint available.");

        }

        public static void AddWaypoint(ulong owner, Tuple<float, float, float> location, string name, string true_name, bool isGlobal) {
            //var WaypointData = new WaypointData(true_name, owner, location);
            if (isGlobal) Database.waypointDBNew[true_name] = location;
            else Database.waypointDBNew[name] = location;
            if (!isGlobal && Database.waypoints_owned.TryGetValue(owner, out var total)) {
                Database.waypoints_owned[owner] = total + 1;
            } else if(!isGlobal) Database.waypoints_owned[owner] = 1;
        }

        public static void RemoveWaypoint(ulong owner, string name, bool global) {
            if (global) {
                Database.waypointDBNew.Remove(name);
            } else {
                Database.waypoints_owned[owner] -= 1;
                if (Database.waypoints_owned[owner] < 0) Database.waypoints_owned[owner] = 0;
                Database.waypointDBNew.Remove(name);
            }
        }

        public static void LoadWaypoints() {
            //LoadWaypointsOld();
            LoadWaypointsNewMethod();
        }
        public static void LoadWaypointsOld() {
            string specificName = "waypoints.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificName);

            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificName);
            try {
                Database.waypoints = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                if (Database.waypoints == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.waypoints = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                }
                Plugin.Logger.LogWarning(DateTime.Now + ": Bloodline DB Populated.");
            } catch {
                Database.waypoints = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning(DateTime.Now + ": Bloodline DB Created.");
            }

            specificName = "global_waypoints.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificName);

            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificName);
            try {
                Database.globalWaypoint = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                if (Database.globalWaypoint == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.globalWaypoint = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                }
                Plugin.Logger.LogWarning(DateTime.Now + ": GlobalWaypoints DB Populated.");
            } catch {
                Database.globalWaypoint = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning(DateTime.Now + ": GlobalWaypoints DB Created.");
            }

            specificName = "total_waypoints.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificName);

            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificName);
            try {
                Database.waypoints_owned = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                if (Database.waypoints_owned == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.waypoints_owned = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                }
                Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint Count DB Populated.");
            } catch {
                Database.waypoints_owned = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint Count DB Created.");
            }

        }
        public static void LoadWaypointsNewMethod() {
            /*
            Database.waypoints = Helper.LoadDB<string, WaypointData>("waypoints.json");
            Plugin.Logger.LogWarning(DateTime.Now + ": Waypoints DB Populated.");

            Database.globalWaypoint = Helper.LoadDB<string, WaypointData>("global_waypoints.json");
            Plugin.Logger.LogWarning(DateTime.Now + ": GlobalWaypoints DB Populated.");
            */
            Database.waypoints_owned = Helper.LoadDB<ulong, int>("total_waypoints.json");
            Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint Count DB Populated.");

            Database.waypointDBNew = Helper.LoadDB<string, Tuple<float, float, float>>("waypointsNewDB.json");
            Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint New DB Populated.");

        }

        public static void SaveWaypoints(string saveFolder) {
            saveOwned(saveFolder);
            //Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint Count DB Saved.");
            saveWPNew(saveFolder);
            //Plugin.Logger.LogWarning(DateTime.Now + ": Waypoint new DB saved.");
        }
        public static void saveOwned(string saveFolder) {
            File.WriteAllText(saveFolder + "total_waypoints.json", JsonSerializer.Serialize(Database.waypoints_owned, Database.JSON_options));
        }
        public static void saveWaypoints(string saveFolder) {
            File.WriteAllText(saveFolder + "waypoints.json", JsonSerializer.Serialize(Database.waypoints, Database.JSON_options));
        }
        public static void saveWPNew(string saveFolder) {
            File.WriteAllText(saveFolder + "waypointsNewDB.json", JsonSerializer.Serialize(Database.waypointDBNew, Database.JSON_options));
            
        }
        public static void saveGlobalWaypoints(string saveFolder) {
            File.WriteAllText(saveFolder + "global_waypoints.json", JsonSerializer.Serialize(Database.globalWaypoint, Database.JSON_options));
        }
    }
}
