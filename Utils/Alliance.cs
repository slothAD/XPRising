using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace RPGMods.Utils; 

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
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Player Character Component unavailable, available components are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
            player = new ClosePlayer();
            return false;
        } 
        var user = pc.UserEntity;
        if (!Plugin.Server.EntityManager.TryGetComponentData(user, out User userComponent)) {
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": User Component unavailable, available components from pc.UserEntity are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(user));
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
        if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Fetching allies...");
        List<ClosePlayer> closePlayers = new();
        if (!useGroup) {
            // If we are not using the group, then the trigger entity is the only ally
            //var position = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(triggerEntity).Position;
            if (ConvertToClosePlayer(triggerEntity, position, logging, out var closePlayer)) {
                closePlayer.isTrigger = true;
                closePlayers.Add(closePlayer);
            }
        }
        else {
            GetPlayerTeams(triggerEntity, logging, out var playerGroup);
            
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Getting close players");

            var playerList = areAllies ? playerGroup.Allies : playerGroup.Enemies;
            
            foreach (var player in playerList) {
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Iterating over players, entity is " + player.GetHashCode());
                var isTrigger = triggerEntity.Equals(player);
                if (logging && isTrigger) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is trigger");
                var playerPosition = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(player).Position;
                
                if (!isTrigger) {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Got entity Position");
                    var distance = math.distancesq(position.xz, playerPosition.xz);
                    if (logging)
                        Plugin.Logger.LogInfo(DateTime.Now + ": DistanceSq is " + distance + ", Max DistanceSq is " +
                                              maxDistanceSq);
                    if (!(distance <= maxDistanceSq)) continue;
                }

                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Converting entity to player...");

                if (ConvertToClosePlayer(player, playerPosition, logging, out var closePlayer)) {
                    closePlayer.isTrigger = isTrigger;
                    closePlayers.Add(closePlayer);
                }
            }
        }
        
        //-- ---------------------------------
        if (logging) Plugin.Logger.LogInfo(DateTime.Now + $": Close players fetched (are Allies: {areAllies}), Total player count of {closePlayers.Count}");
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
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Player found in cache, cache timestamp is {playerGroup.TimeStamp}");
            var cacheAge = DateTime.Now - playerGroup.TimeStamp;
            if (cacheAge.TotalSeconds < CacheAgeLimit) return;
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Cache is too old, refreshing cached data");
        }

        playerGroup = new PlayerGroup();
        
        if (!Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(playerCharacter)) {
            if (logging) {
                Plugin.Logger.LogInfo($"{DateTime.Now}: Entity is not user: {playerCharacter}");
                Plugin.Logger.LogInfo($"{DateTime.Now}: Components for Player Character are: {Plugin.Server.EntityManager.Debug.GetEntityInfo(playerCharacter)}");
            }
            return;
        }
        
        // Check if the player has a team
        var hasTeam = false;
        var teamValue = 0;
        if (Plugin.Server.EntityManager.TryGetComponentData(playerCharacter, out Team playerTeam)) {
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Player Character found team: {playerTeam.Value} - Faction Index: {playerTeam.FactionIndex}");
            hasTeam = true;
            teamValue = playerTeam.Value;
        }
        else {
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Player Character has no team: all other PCs are marked as enemies.");
        }

        if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Beginning To Parse Player Group");

        var playerEntityBuffer = ConnectedPlayerCharactersQuery.ToEntityArray(Allocator.Temp);
        if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: got connected PC entities buffer of length {playerEntityBuffer.Length}");
        
        foreach (var entity in playerEntityBuffer) {
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": got Entity " + entity);
            if (Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(entity)) {
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is User " + entity);
                if (entity.Equals(playerCharacter)) {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is self");
                    // We are our own ally.
                    playerGroup.Allies.Add(entity);
                    continue;
                }

                // If the playerCharacter doesn't have a team, then all other PC entities are enemies
                if (!hasTeam) {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + $": Entity defaults to enemy: {entity}");
                    playerGroup.Enemies.Add(entity);
                }

                var allies = false;
                try {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Trying to get entity teams");
                    if (Plugin.Server.EntityManager.TryGetComponentData(entity, out Team entityTeam))
                    {
                        // Team has been found
                        if (logging) Plugin.Logger.LogInfo(DateTime.Now + $": Team Value:{entityTeam.Value} - Faction Index: {entityTeam.FactionIndex}");
                        
                        // Check if the playerCharacter is on the same team as entity
                        allies = entityTeam.Value == teamValue;
                    }
                    else {
                        if (logging) {
                            Plugin.Logger.LogInfo(DateTime.Now + $": Could not get team for entity: {entity}");
                            Plugin.Logger.LogInfo(DateTime.Now + ": Components for entity are: " +
                                                  Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
                        }
                    }
                }
                catch (Exception e) {
                    if (logging)
                        Plugin.Logger.LogInfo(DateTime.Now + ": GetPlayerTeams failed " + e.Message);
                }

                if (allies) {
                    if (logging)
                        Plugin.Logger.LogInfo($"{DateTime.Now}: Allies: {playerCharacter} - {entity}");
                    playerGroup.Allies.Add(entity);
                }
                else {
                    if (logging)
                        Plugin.Logger.LogInfo($"{DateTime.Now}: Enemies: {playerCharacter} - {entity}");
                    playerGroup.Enemies.Add(entity);
                }
            }
            else {
                // Should never get here as the query should only return PlayerCharacter entities
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": No Associated User!");
            }
        }
        
        Cache.PlayerAllies[playerCharacter] = playerGroup;
    }
}