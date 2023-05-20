using RPGMods.Utils;
using System;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    
    public static class SpawnNPC
    {
        [Command("spawnnpc", "spn", "<Prefab Name/GUID> [<Amount>]", "Spawns a NPC at you.", adminOnly: true)]
        public static void Initialize(ChatCommandContext ctx, string name, int count)
        {
            bool isUsingGUID = int.TryParse(name, out var GUID);

            
            var pos = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            if (isUsingGUID)
            {
                if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, GUID, count, new(pos.x, pos.y, pos.z), 1, 2, 1800))
                {
                    ctx.Reply($"Failed to spawn: {name}");
                    return;
                }
            }
            else
            {
                if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(pos.x, pos.y, pos.z), 1, 2, 1800))
                {
                    ctx.Reply($"Could not find specified unit: {name}");
                    return;
                }
            }
            ctx.Reply($"Spawning {count} {name} at <{pos.x}, {pos.y}, {pos.z}>");
            /*
            else if (ctx.Args.Length >= 2)
            {
                name = ctx.Args[0];
                waypoint = ctx.Args.Last().ToLower();
                ulong SteamID = ctx.Event.User.PlatformId;

                if (Database.globalWaypoint.TryGetValue(waypoint, out var WPData))
                {
                    float3 wp = WPData.Location;
                    if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(wp.x, wp.y), 1, 2, 1800))
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                        return;
                    }
                    Output.SendSystemMessage(ctx, $"Spawning {count} {name} at <{wp.x}, {wp.y}>");
                    return;
                }

                if (Database.waypoints.TryGetValue(waypoint+"_"+SteamID, out var WPData_))
                {
                    float3 wp = WPData_.Location;
                    if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(wp.x, wp.y), 1, 2, 1800))
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                        return;
                    }
                    Output.SendSystemMessage(ctx, $"Spawning {count} {name} at <{wp.x}, {wp.y}>");
                    return;
                }
                Output.CustomErrorMessage(ctx, "This waypoint doesn't exist.");
            }*/
        }
    }
}