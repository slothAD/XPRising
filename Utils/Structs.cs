using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Faction = OpenRPG.Utils.Prefabs.Faction;

namespace OpenRPG.Utils
{
    public struct BuffData
    {
        public string source;
        public int targetStat;
        public int modificationType;
        public double value;
        public int ID;
        public bool isApplied;

    }

    public struct PowerUpData
    {
        public string Name { get; set; }
        public float MaxHP { get; set; }
        public float PATK { get; set; }
        public float PDEF { get; set; }
        public float SATK { get; set; }
        public float SDEF { get; set; }
    }
    public struct StatsBonus()
    {
        public int Level_Int { get; set; } = 0;
        public float HP_Float { get; set; } = 0;
        public float PhysicalPower_Float { get; set; } = 0;
        public float PhysicalResistance_Float { get; set; } = 0;
        public float PhysicalCriticalStrikeChance_Float { get; set; } = 0;
        public float PhysicalCriticalStrikeDamage_Float { get; set; } = 0;
        public float SpellPower_Float { get; set; } = 0;
        public float SpellResistance_Float { get; set; } = 0;
        public float SpellCriticalStrikeChance_Float { get; set; } = 0;
        public float SpellCriticalStrikeDamage_Float { get; set; } = 0;
        public float DamageVsPlayerVampires_Float { get; set; } = 0;
        public float ResistVsPlayerVampires_Float { get; set; } = 0;
        public int FireResistance_Int { get; set; } = 0;
    }

    public struct FactionData(Prefabs.Faction faction)
    {
        public string Name { get; set; } = $"Faction_{Enum.GetName(faction)}";
        public bool Active { get; set; } = false;
        public int Level { get; set; } = 0;
        public int MaxLevel { get; set; } = 0;
        public int MinLevel { get; set; } = 0;
        public int ActivePower { get; set; } = 0;
        public int StoredPower { get; set; } = 0;
        public int DailyPower { get; set; } = 0;
        public int RequiredPower { get; set; } = 0;
        public StatsBonus FactionBonus { get; set; } = new();
    }

    public struct PlayerHeatData {
        public struct Heat {
            public int level { get; set; }
            public DateTime lastAmbushed { get; set; }
        }
        
        public Dictionary<Faction, Heat> heat { get; } = new();
        public DateTime lastCooldown { get; set; }
        public bool isLogging { get; set; }

        public PlayerHeatData() {
            foreach (Faction faction in FactionHeat.ActiveFactions) {
                heat[faction] = new();
            }
        }
    }

    public struct PlayerGroup()
    {
        public HashSet<Entity> Allies { get; } = new();
        public HashSet<Entity> Enemies { get; } = new();
        public DateTime TimeStamp { get; } = DateTime.Now;
    }

    public struct PlayerData(
        FixedString64 characterName,
        ulong steamID,
        bool isOnline,
        Entity userEntity,
        Entity charEntity)
    {
        public FixedString64 CharacterName { get; set; } = characterName;
        public ulong SteamID { get; set; } = steamID;
        public bool IsOnline { get; set; } = isOnline;
        public Entity UserEntity { get; set; } = userEntity;
        public Entity CharEntity { get; set; } = charEntity;
    }
    
    public struct WaypointData(float x, float y, float z)
    {
        public float x = x;
        public float y = y;
        public float z = z;

        public WaypointData(float3 location) : this(location.x, location.y, location.z) {}

        public float3 ToFloat3()
        {
            return new float3(x, y, z);
        }
    }

    public struct StatConfig(UnitStatType type, double strength, double rate)
    {
        public UnitStatType type = type;
        public double strength = strength;
        public double rate = rate;
    }

    public struct MasteryData()
    {
        public double Mastery = 0;
        public double Effectiveness = 1;
        public double Growth = 1;

        public MasteryData ResetMastery(double maxMastery, double maxEffectiveness, double growthPerEffectiveness, double maxGrowth, double minGrowth)
        {
            Effectiveness = Math.Min(maxEffectiveness, Effectiveness + Mastery / maxMastery);
            Mastery = 0;

            var percentageEffectiveness = Effectiveness / maxEffectiveness;
            if (growthPerEffectiveness >= 0) {
                Growth = Math.Min(maxGrowth, Growth + (percentageEffectiveness * growthPerEffectiveness));
            }
            else {
                Growth = Math.Max(minGrowth, Growth * (1 - (percentageEffectiveness / (percentageEffectiveness - growthPerEffectiveness))));
            }

            return this;
        }

        public override string ToString()
        {
            return $"[{Mastery:F3},{Effectiveness:F3},{Growth:F3}]";
        }
    }

    public struct BanData(
        DateTime banUntil = default(DateTime),
        string reason = "Invalid",
        string bannedBy = "Default",
        ulong steamID = 0)
    {
        public DateTime BanUntil { get; set; } = banUntil;
        public string Reason { get; set; } = reason;
        public string BannedBy { get; set; } = bannedBy;
        public ulong SteamID { get; set; } = steamID;
    }

    public struct SpawnOptions
    {
        public bool ModifyBlood { get; set; }
        public PrefabGUID BloodType { get; set; }
        public float BloodQuality { get; set; }
        public bool BloodConsumeable { get; set; }
        public bool ModifyStats { get; set; }
        public UnitStats UnitStats { get; set; }
        public bool Process { get; set; }

        public SpawnOptions(bool modifyBlood = false, PrefabGUID bloodType = default, float bloodQuality = 0, bool bloodConsumeable = true, bool modifyStats = false, UnitStats unitStats = default, bool process = false)
        {
            ModifyBlood = modifyBlood;
            BloodType = bloodType;
            BloodQuality = bloodQuality;
            BloodConsumeable = bloodConsumeable;
            ModifyStats = modifyStats;
            UnitStats = unitStats;
            Process = process;
        }
    }

    public struct SpawnNPCListen
    {
        public float Duration { get; set; }
        public int EntityIndex { get; set; }
        public int EntityVersion { get; set; }
        public SpawnOptions Options { get; set; }
        public bool Process { get; set; }

        public SpawnNPCListen(float duration = 0.0f, int entityIndex = 0, int entityVersion = 0, SpawnOptions options = default, bool process = true)
        {
            Duration = duration;
            EntityIndex = entityIndex;
            EntityVersion = entityVersion;
            Options = options;
            Process = process;
        }

        public Entity getEntity()
        {
            Entity entity = new Entity()
            {
                Index = this.EntityIndex,
                Version = this.EntityVersion,
            };
            return entity;
        }
    }

    public sealed class SizedDictionaryAsync<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {

        private int maxSize;
        private Queue<TKey> keys;

        public SizedDictionaryAsync(int size)
        {
            maxSize = size;
            keys = new Queue<TKey>();
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException();
            base.TryAdd(key, value);
            keys.Enqueue(key);
            if (keys.Count > maxSize) base.TryRemove(keys.Dequeue(), out _);
        }

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException();
            if (!keys.Contains(key)) return false;
            var newQueue = new Queue<TKey>();
            while (keys.Count > 0)
            {
                var thisKey = keys.Dequeue();
                if (!thisKey.Equals(key)) newQueue.Enqueue(thisKey);
            }
            keys = newQueue;
            return base.TryRemove(key, out _);
        }
    }
}
