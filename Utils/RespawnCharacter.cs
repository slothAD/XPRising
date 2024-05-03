using BepInEx.Logging;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Utils
{
    public class RespawnCharacter
    {
        public static void Respawn(Entity victimEntity, PlayerCharacter player, Entity userEntity) {
            Plugin.Log(LogSystem.Death, LogLevel.Info, "attempting auto respawn");
            var bufferSystem = Plugin.Server.GetOrCreateSystem<EntityCommandBufferSystem>();

            Plugin.Log(LogSystem.Death, LogLevel.Info, "buffer system obtained");
            unsafe
            {
                var playerLocation = player.LastValidPosition;
                Plugin.Log(LogSystem.Death, LogLevel.Info, "got last valid position");
                var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(userEntity).Position;
                Plugin.Log(LogSystem.Death, LogLevel.Info, "got current positon");
                float3 spawnAt;
                if(math.abs(location.x - playerLocation.x) < 0.1 && math.abs(location.y - playerLocation.y) < 0.1) {
                    Plugin.Log(LogSystem.Death, LogLevel.Info, "current xy and last valid are less than 0.1 diff using last valid");
                    spawnAt = new float3(playerLocation.x, playerLocation.y, location.z);
                } else {
                    Plugin.Log(LogSystem.Death, LogLevel.Info, "current and last valid very different, using last valid at height 6");
                    spawnAt = new float3(playerLocation.x, playerLocation.y, 6);
                }

                Plugin.Log(LogSystem.Death, LogLevel.Info, "setting spawn location as nullable unboxed");
                var spawnLocation = new Il2CppSystem.Nullable_Unboxed<float3>(spawnAt);
                Plugin.Log(LogSystem.Death, LogLevel.Info, "getting server bootstrap");
                var server = Plugin.Server.GetOrCreateSystem<ServerBootstrapSystem>();


                Plugin.Log(LogSystem.Death, LogLevel.Info, "respawning character");

                server.RespawnCharacter(bufferSystem.CreateCommandBuffer(), userEntity, customSpawnLocation: spawnLocation, previousCharacter: victimEntity, fadeOutEntity: userEntity);
            }
        }
    }
}
