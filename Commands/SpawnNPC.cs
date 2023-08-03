using Bloodstone.API;
using OpenRPG.Utils;
using System;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class SpawnNPC
    {
        [Command("spawnnpc", "<Prefab GUID> [<Amount>] [<Waypoint>]", "Spawns a NPC to a previously created waypoint.")]
        public static void SpawnNPCCommand(ChatCommandContext ctx, int guid, int amount = 1, string waypoint = null)
        {

            if (waypoint == null)
            {

                var pos = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;

                if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, guid, amount, new(pos.x, pos.z), 1, 2, 1800))
                {
                    throw ctx.Error($"Failed to spawn: {waypoint}");
                }

                ctx.Reply($"Spawning {amount} {waypoint} at <{pos.x}, {pos.z}>");
            }
            else
            {
                ulong SteamID = ctx.Event.User.PlatformId;

                if (Database.globalWaypoint.TryGetValue(waypoint, out var WPData))
                {
                    float3 wp = WPData.Location;
                    if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, guid, amount, new(wp.x, wp.y), 1, 2, 1800))
                    {
                        throw ctx.Error($"Could not find specified unit: {guid}");
                    }
                    ctx.Reply($"Spawning {amount} {guid} at <{wp.x}, {wp.y}>");
                    return;
                }

                if (Database.waypoints.TryGetValue(waypoint + "_" + SteamID, out var WPData_))
                {
                    float3 wp = WPData_.Location;
                    if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, guid, amount, new(wp.x, wp.y), 1, 2, 1800))
                    {
                        throw ctx.Error($"Could not find specified unit: {guid}");
                    }
                    ctx.Reply($"Spawning {amount} {guid} at <{wp.x}, {wp.y}>");
                    return;
                }
                throw ctx.Error("This waypoint doesn't exist.");
            }
        }
    }
}