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
            Plugin.Log(LogSystem.Death, LogLevel.Info, $"Attempting auto respawn: {player.Name}");
            var bufferSystem = Plugin.Server.GetOrCreateSystem<EntityCommandBufferSystem>();

            // Warning: If you are trying to use this, remember that the ground uses location.xz and the height is location.y
            var location = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(userEntity).Position;

            var spawnLocation = new Il2CppSystem.Nullable_Unboxed<float3>(location);
            var server = Plugin.Server.GetOrCreateSystem<ServerBootstrapSystem>();

            Plugin.Log(LogSystem.Death, LogLevel.Info, $"respawning {player.Name} at: {spawnLocation.Value} (current location: {location})");

            server.RespawnCharacter(bufferSystem.CreateCommandBuffer(), userEntity, customSpawnLocation: spawnLocation, previousCharacter: victimEntity, fadeOutEntity: userEntity);
        }
    }
}
