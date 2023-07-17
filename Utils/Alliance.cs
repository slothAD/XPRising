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
    public struct CloseAlly {
        public Entity userEntity;
        public User userComponent;
        public int currentXp;
        public int playerLevel;
        public ulong steamID;
        public float3 position;
        public bool isTrigger;
    }
    
    private static bool ConvertToAlly(Entity entity, float3 position, bool logging, out CloseAlly ally) {
        if (!Plugin.Server.EntityManager.TryGetComponentData(entity, out PlayerCharacter pc)) {
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Player Character Component unavailable, available components are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
            ally = new CloseAlly();
            return false;
        } 
        var user = pc.UserEntity;
        if (!Plugin.Server.EntityManager.TryGetComponentData(user, out User userComponent)) {
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": User Component unavailable, available components from pc.UserEntity are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(user));
            // Can't really do anything at this point
            ally = new CloseAlly();
            return false;
        }
                        
        var steamID = userComponent.PlatformId;
        var playerLevel = 0;
        if (Database.player_experience.TryGetValue(steamID, out int currentXp))
        {
            playerLevel = ExperienceSystem.convertXpToLevel(currentXp);
        }
                        
        ally = new CloseAlly() {
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
    public static List<CloseAlly> GetCloseAllies(Entity closeToEntity, Entity triggerEntity, float groupMaxDistance, bool useGroup, bool logging) {
        var maxDistanceSq = groupMaxDistance * groupMaxDistance;
        //-- Must be executed from main thread
        if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Fetching allies...");
        List<CloseAlly> closeAllies = new();
        if (!useGroup) {
            // If we are not using the group, then the trigger entity is the only ally
            var position = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(triggerEntity).Position;
            if (ConvertToAlly(triggerEntity, position, logging, out var ally)) {
                ally.isTrigger = true;
                closeAllies.Add(ally);
            }
        }
        else {
            GetAllies(triggerEntity, logging, out var playerGroup);
            
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Getting close allies");
            
            var victimLocation = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(closeToEntity);
            foreach (var ally in playerGroup.Allies) {
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Iterating over allies, entity is " + ally.GetHashCode());
                var isTrigger = triggerEntity.Equals(ally.Key);
                if (logging && isTrigger) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is trigger");
                var position = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(ally.Value).Position;
                
                if (!isTrigger) {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Got entity Position");
                    var distance = math.distancesq(victimLocation.Position.xz, position.xz);
                    if (logging)
                        Plugin.Logger.LogInfo(DateTime.Now + ": DistanceSq is " + distance + ", Max DistanceSq is " +
                                              maxDistanceSq);
                    if (!(distance <= maxDistanceSq)) continue;
                }

                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Converting entity to ally...");

                if (ConvertToAlly(ally.Key, position, logging, out var closeAlly)) {
                    closeAlly.isTrigger = isTrigger;
                    closeAllies.Add(closeAlly);
                }
            }
        }
        
        //-- ---------------------------------
        if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Allies Fetched, Total ally count of " + closeAllies.Count);
        return closeAllies;
    }
    
    // Get allies for PlayerCharacter (ie, every vampire in the clan), cached for 5 minutes
    // The list of allies includes PlayerCharacter.
    private static readonly int CacheAgeLimit = 300;
    public static void GetAllies(Entity playerCharacter, bool logging, out PlayerGroup playerGroup) {
        if (!Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(playerCharacter)) {
            if (logging) {
                Plugin.Logger.LogInfo($"{DateTime.Now}: Entity is not user: {playerCharacter}");
                Plugin.Logger.LogInfo($"{DateTime.Now}: Components for Player Character are: {Plugin.Server.EntityManager.Debug.GetEntityInfo(playerCharacter)}");
            }

            playerGroup = new PlayerGroup {
                Allies = new Dictionary<Entity, Entity>()
            };
            return;
        }

        if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Beginning To Parse Player Group");
        if (Cache.PlayerAllies.TryGetValue(playerCharacter, out playerGroup)) {
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Allies Found in Cache, timestamp is {playerGroup.TimeStamp}");
            var cacheAge = DateTime.Now - playerGroup.TimeStamp;
            if (cacheAge.TotalSeconds < CacheAgeLimit) return;
            if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: Refreshing cached allies");
        }
        
        // Check if the player has a team
        if (!Plugin.Server.EntityManager.TryGetComponentData(playerCharacter, out Team playerTeam)) {
            if (logging) {
                Plugin.Logger.LogInfo($"{DateTime.Now}: Could not get team for Player Character: {playerCharacter}");
                Plugin.Logger.LogInfo($"{DateTime.Now}: Components for Player Character are: {Plugin.Server.EntityManager.Debug.GetEntityInfo(playerCharacter)}");
            }

            playerGroup = new PlayerGroup {
                Allies = new Dictionary<Entity, Entity>()
            };
            return;
        } else if (logging) {
            Plugin.Logger.LogInfo($"{DateTime.Now}: Player Character Found Value: {playerTeam.Value} - Faction Index: {playerTeam.FactionIndex}");
        }

        playerGroup.TimeStamp = DateTime.Now;

        Dictionary<Entity, Entity> group = new();

        var query = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new[]
                {
                    ComponentType.ReadOnly<PlayerCharacter>(),
                    ComponentType.ReadOnly<IsConnected>()
                },
            Options = EntityQueryOptions.IncludeDisabled
        });
        var allyBuffer = query.ToEntityArray(Allocator.Temp);
        if (logging) Plugin.Logger.LogInfo($"{DateTime.Now}: got connected PC entities buffer of length {allyBuffer.Length}");
        
        foreach (var entity in allyBuffer) {
            if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": got Entity " + entity);
            if (Plugin.Server.EntityManager.HasComponent<PlayerCharacter>(entity)) {
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is User " + entity);
                if (entity.Equals(playerCharacter)) {
                    if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": Entity is self");
                    // We are an ally of ourself.
                    group[entity] = entity;
                    continue;
                }

                bool allies = false;
                try {
                    if (logging)
                        Plugin.Logger.LogInfo(DateTime.Now + ": Trying to get teams ");
                    bool teamFound = Plugin.Server.EntityManager.TryGetComponentData(entity, out Team entityTeam);
                    if (logging) {
                        if (teamFound)
                            Plugin.Logger.LogInfo(DateTime.Now + ": Team Value:" + entityTeam.Value +
                                                  " - Faction Index: " + entityTeam.FactionIndex);
                        else {
                            Plugin.Logger.LogInfo(DateTime.Now + ": Could not get team for entity: " + entity);
                            Plugin.Logger.LogInfo(DateTime.Now + ": Components for entity are: " +
                                                  Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
                        }
                    }

                    allies = teamFound && entityTeam.Value == playerTeam.Value;
                }
                catch (Exception e) {
                    if (logging)
                        Plugin.Logger.LogInfo(DateTime.Now + ": IsAllies Failed " + e.Message);
                }

                if (allies) {
                    if (logging)
                        Plugin.Logger.LogInfo($"{DateTime.Now}: Allies: {playerCharacter} - {entity}");
                    group[entity] = entity;
                }
                else {
                    if (logging)
                        Plugin.Logger.LogInfo($"{DateTime.Now}: Not allies: {playerCharacter} - {entity}");

                }
            }
            else {
                if (logging) Plugin.Logger.LogInfo(DateTime.Now + ": No Associated User!");
            }
        }


        playerGroup.Allies = group;
        playerGroup.AllyCount = group.Count;
        Cache.PlayerAllies[playerCharacter] = playerGroup;
    }
}