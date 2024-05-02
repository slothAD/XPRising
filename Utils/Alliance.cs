using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace OpenRPG.Utils; 

public class Alliance {
    public struct ClosePlayer {
        public Entity userEntity;
        public User userComponent;
        public int currentXp;
        public int playerLevel;
        public ulong steamID;
        public float3 position;
        public bool isTrigger;
    }
    
    private static bool ConvertToClosePlayer(Entity entity, float3 position, bool logging, out ClosePlayer player) {
        if (!Plugin.Server.EntityManager.TryGetComponentData(entity, out PlayerCharacter pc)) {
            if (logging) Plugin.LogInfo("Player Character Component unavailable, available components are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
            player = new ClosePlayer();
            return false;
        } 
        var user = pc.UserEntity;
        if (!Plugin.Server.EntityManager.TryGetComponentData(user, out User userComponent)) {
            if (logging) Plugin.LogInfo("User Component unavailable, available components from pc.UserEntity are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(user));
            // Can't really do anything at this point
            player = new ClosePlayer();
            return false;
        }
                        
        var steamID = userComponent.PlatformId;
        var playerLevel = 0;
        if (Database.player_experience.TryGetValue(steamID, out int currentXp))
        {
            playerLevel = ExperienceSystem.convertXpToLevel(currentXp);
        }
                        
        player = new ClosePlayer() {
            currentXp = currentXp,
            playerLevel = playerLevel,
            steamID = steamID,
            userEntity = user,
            userComponent = userComponent,
            position = position
        };
        return true;
    }
    
    // Determines the units close to the entity in question.
    // This will always include the entity that triggered this call, even if they are greater than groupMaxDistance away.
    public static List<ClosePlayer> GetClosePlayers(float3 position, Entity triggerEntity, float groupMaxDistance,
        bool areAllies, bool useGroup, bool logging) {
        var maxDistanceSq = groupMaxDistance * groupMaxDistance;
        //-- Must be executed from main thread
        if (logging) Plugin.LogInfo("Fetching allies...");
        List<ClosePlayer> closePlayers = new();
        if (!useGroup) {
            // If we are not using the group, then the trigger entity is the only ally
            if (ConvertToClosePlayer(triggerEntity, position, logging, out var closePlayer)) {
                closePlayer.isTrigger = true;
                closePlayers.Add(closePlayer);
            }
        }
        else {
            GetPlayerTeams(triggerEntity, logging, out var playerGroup);
            
            if (logging) Plugin.LogInfo($"Getting close players");

            var playerList = areAllies ? playerGroup.Allies : playerGroup.Enemies;
            
            foreach (var player in playerList) {
                if (logging) Plugin.LogInfo("Iterating over players, entity is " + player.GetHashCode());
                var isTrigger = triggerEntity.Equals(player);
                if (logging && isTrigger) Plugin.LogInfo("Entity is trigger");
                var playerPosition = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(player).Position;
                
                if (!isTrigger) {
                    if (logging) Plugin.LogInfo("Got entity Position");
                    var distance = math.distancesq(position.xz, playerPosition.xz);
                    if (logging)
                        Plugin.LogInfo("DistanceSq is " + distance + ", Max DistanceSq is " +
                                              maxDistanceSq);
                    if (!(distance <= maxDistanceSq)) continue;
                }

                if (logging) Plugin.LogInfo("Converting entity to player...");

                if (ConvertToClosePlayer(player, playerPosition, logging, out var closePlayer)) {
                    closePlayer.isTrigger = isTrigger;
                    closePlayers.Add(closePlayer);
                }
            }
        }
        
        //-- ---------------------------------
        if (logging) Plugin.LogInfo($"Close players fetched (are Allies: {areAllies}), Total player count of {closePlayers.Count}");
        return closePlayers;
    }
    
    // Get allies/enemies for PlayerCharacter, cached for 30 seconds
    // The list of allies includes PlayerCharacter.
    private static readonly int CacheAgeLimit = 30;
    
    private static EntityQuery ConnectedPlayerCharactersQuery = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[]
        {
            ComponentType.ReadOnly<PlayerCharacter>(),
            ComponentType.ReadOnly<IsConnected>()
        },
        Options = EntityQueryOptions.IncludeDisabled
    });
    public static void GetPlayerTeams(Entity playerCharacter, bool logging, out PlayerGroup playerGroup) {
        if (Cache.PlayerAllies.TryGetValue(playerCharacter, out playerGroup)) {
            if (logging) Plugin.LogInfo($"Player found in cache, cache timestamp is {playerGroup.TimeStamp}");
            var cacheAge = DateTime.Now - playerGroup.TimeStamp;
            if (cacheAge.TotalSeconds < CacheAgeLimit) return;
            if (logging) Plugin.LogInfo($"Cache is too old, refreshing cached data");
        }

        playerGroup = new PlayerGroup();
        
        if (!Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(playerCharacter)) {
            if (logging) {
                Plugin.LogInfo($"Entity is not user: {playerCharacter}");
                Plugin.LogInfo($"Components for Player Character are: {Plugin.Server.EntityManager.Debug.GetEntityInfo(playerCharacter)}");
            }
            return;
        }
        
        // Check if the player has a team
        var hasTeam = false;
        var teamValue = 0;
        if (Plugin.Server.EntityManager.TryGetComponentData(playerCharacter, out Team playerTeam)) {
            if (logging) Plugin.LogInfo($"Player Character found team: {playerTeam.Value} - Faction Index: {playerTeam.FactionIndex}");
            hasTeam = true;
            teamValue = playerTeam.Value;
        }
        else {
            if (logging) Plugin.LogInfo($"Player Character has no team: all other PCs are marked as enemies.");
        }

        if (logging) Plugin.LogInfo($"Beginning To Parse Player Group");

        var playerEntityBuffer = ConnectedPlayerCharactersQuery.ToEntityArray(Allocator.Temp);
        if (logging) Plugin.LogInfo($"got connected PC entities buffer of length {playerEntityBuffer.Length}");
        
        foreach (var entity in playerEntityBuffer) {
            if (logging) Plugin.LogInfo("got Entity " + entity);
            if (Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(entity)) {
                if (logging) Plugin.LogInfo("Entity is User " + entity);
                if (entity.Equals(playerCharacter)) {
                    if (logging) Plugin.LogInfo("Entity is self");
                    // We are our own ally.
                    playerGroup.Allies.Add(entity);
                    continue;
                }

                // If the playerCharacter doesn't have a team, then all other PC entities are enemies
                if (!hasTeam) {
                    if (logging) Plugin.LogInfo($"Entity defaults to enemy: {entity}");
                    playerGroup.Enemies.Add(entity);
                }

                var allies = false;
                try {
                    if (logging) Plugin.LogInfo("Trying to get entity teams");
                    if (Plugin.Server.EntityManager.TryGetComponentData(entity, out Team entityTeam))
                    {
                        // Team has been found
                        if (logging) Plugin.LogInfo($"Team Value:{entityTeam.Value} - Faction Index: {entityTeam.FactionIndex}");
                        
                        // Check if the playerCharacter is on the same team as entity
                        allies = entityTeam.Value == teamValue;
                    }
                    else {
                        if (logging) {
                            Plugin.LogInfo($"Could not get team for entity: {entity}");
                            Plugin.LogInfo("Components for entity are: " +
                                                  Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
                        }
                    }
                }
                catch (Exception e) {
                    if (logging)
                        Plugin.LogInfo("GetPlayerTeams failed " + e.Message);
                }

                if (allies) {
                    if (logging)
                        Plugin.LogInfo($"Allies: {playerCharacter} - {entity}");
                    playerGroup.Allies.Add(entity);
                }
                else {
                    if (logging)
                        Plugin.LogInfo($"Enemies: {playerCharacter} - {entity}");
                    playerGroup.Enemies.Add(entity);
                }
            }
            else {
                // Should never get here as the query should only return PlayerCharacter entities
                if (logging) Plugin.LogInfo("No Associated User!");
            }
        }
        
        Cache.PlayerAllies[playerCharacter] = playerGroup;
    }
}