using System;
using System.Linq;
using Unity.Entities;
using Faction = RPGMods.Utils.Prefabs.Faction;

namespace RPGMods.Utils;

public static class FactionHeat {
    public static readonly Faction[] ActiveFactions = { Faction.Militia, Faction.Bandits, Faction.Undead };
    
    private static readonly string[] ColourGradient = { "fef001", "ffce03", "fd9a01", "fd6104", "ff2c05", "f00505" };

    private struct UnitSpawn {
        public int group { get; }
        public int minCount { get; }
        public int maxCount { get; }

        public UnitSpawn(int g, int min, int max) {
            group = g;
            minCount = min;
            maxCount = max;
        }
        
        public UnitSpawn(int g, int count) {
            group = g;
            minCount = count;
            maxCount = count;
        }
    }
    
    private struct AmbushLevel {
        public string Message { get; }
        public UnitSpawn[][] SquadOptions { get; }

        public AmbushLevel(string ambush, UnitSpawn[][] squad) {
            Message = ambush;
            SquadOptions = squad;
        }
    }

    private static readonly AmbushLevel[] BanditAmbushLevels =
    {
        new("The bandits are ambushing you!", new[] { new[] { new UnitSpawn(0, 3)} }),
        new("A small bandit squad is ambushing you!", new[] { new[] { new UnitSpawn(0, 5)} }),
        new("A large bandit squad is ambushing you!", new[] { new[] { new UnitSpawn(0, 10, 15)} }),
        new("The bandits are ambushing you with as many people they can spare!", new[] { new[] { new UnitSpawn(0, 20, 25)} })
    };
    
    private static readonly AmbushLevel[] HumanAmbushLevels =
    {
        new("A militia squad is ambushing you!", new[] { new[] { new UnitSpawn(1, 5, 10)} }),
        new("A squad of soldiers is ambushing you!", new[] { new[] { new UnitSpawn(2, 10, 15)} }),
        new("An ambush squad from the Church has been sent to kill you!", new[] { new[] { new UnitSpawn(3, 10, 15)} }),
        new("The Vampire Hunters are ambushing you!", new[] { new[] { new UnitSpawn(4, 15, 20)}, new[] { new UnitSpawn(4, 9), new UnitSpawn(5, 1)} }),
        new("An extermination squad has found you and wants you DEAD.", new[] { new[] { new UnitSpawn(4, 10, 20), new UnitSpawn(5, 2)} })
    };
    
    private static readonly AmbushLevel[] UndeadAmbushLevels =
    {
        new("An undead squad is ambushing you!", new[] { new[] { new UnitSpawn(6, 5, 10)} }),
        new("More undead are ambushing you!", new[] { new[] { new UnitSpawn(6, 10, 15)} }),
        new("Strong undead have been summoned at you!", new[] { new[] { new UnitSpawn(7, 10, 15)} }),
        new("Undead leaders have been summoned at you!", new[] { new[] { new UnitSpawn(7, 15, 20)}, new[] { new UnitSpawn(7, 9), new UnitSpawn(8, 1)} }),
        new("The undead are here to kill you!", new[] { new[] { new UnitSpawn(7, 5, 10), new UnitSpawn(9, 1)} })
    };

    private static readonly int[] HeatLevels = { 150, 250, 500, 1000, 1500, 3000 };

    private static readonly AmbushLevel[] UnknownAmbushLevels = {new("", Array.Empty<UnitSpawn[]>())};

    private static AmbushLevel GetFactionAmbushLevel(Faction faction, int wantedLevel) {
        if (wantedLevel == 0) return UnknownAmbushLevels[0];
        var factionStates = faction switch {
                Faction.Militia => HumanAmbushLevels,
                Faction.Bandits => BanditAmbushLevels,
                Faction.Undead => UndeadAmbushLevels,
                _ => UnknownAmbushLevels
            };
        
        return factionStates[Math.Min(wantedLevel, factionStates.Length) - 1];
    }

    private static int vBloodMultiplier = 20;
    public static void GetActiveFactionHeatValue(Faction faction, bool isVBlood, out int heatValue, out Faction activeFaction) {
        switch (faction) {
            // Bandit
            case Faction.Traders_T01:
                heatValue = 200; // Don't kill the merchants
                activeFaction = Faction.Bandits;
                break;
            case Faction.Bandits:
                heatValue = 10;
                activeFaction = Faction.Bandits;
                break;
            // Human
            case Faction.Militia:
                heatValue = 10;
                activeFaction = Faction.Militia;
                break;
            case Faction.ChurchOfLum_SpotShapeshiftVampire:
                heatValue = 25;
                activeFaction = Faction.Militia;
                break;
            case Faction.Traders_T02:
                heatValue = 200; // Don't kill the merchants
                activeFaction = Faction.Militia;
                break;
            case Faction.ChurchOfLum:
                heatValue = 15;
                activeFaction = Faction.Militia;
                break;
            // Human: gloomrot
            case Faction.Gloomrot:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Unknown;
                break;
            // Nature
            case Faction.Bear:
            case Faction.Critters:
            case Faction.Wolves:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Unknown;
                break;
            // Undead
            case Faction.Undead:
                heatValue = 3;
                activeFaction = Faction.Undead;
                break;
            // Werewolves
            case Faction.Werewolf:
            case Faction.WerewolfHuman:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Unknown;
                break;
            case Faction.VampireHunters:
                heatValue = 2;
                activeFaction = Faction.VampireHunters;
                break;
            // Do nothing
            case Faction.ChurchOfLum_Slaves:
            case Faction.ChurchOfLum_Slaves_Rioters:
            case Faction.Cursed:
            case Faction.Elementals:
            case Faction.Ignored:
            case Faction.Harpy:
            case Faction.Mutants:
            case Faction.NatureSpirit:
            case Faction.Plants:
            case Faction.Players:
            case Faction.Players_Castle_Prisoners:
            case Faction.Players_Mutant:
            case Faction.Players_Shapeshift_Human:
            case Faction.Spiders:
            case Faction.Unknown:
            case Faction.Wendigo:
            case Faction.World_Prisoners:
                heatValue = 0;
                activeFaction = Faction.Unknown;
                break;
            default:
                Plugin.Logger.LogWarning($"Faction not handled for active faction: {Enum.GetName(faction)}");
                heatValue = 0;
                activeFaction = Faction.Unknown;
                break;
        }

        if (isVBlood) heatValue *= vBloodMultiplier;
    }

    public static string GetFactionStatus(Faction faction, int heat) {
        var output = $"{Enum.GetName(faction)}: ";
        return HeatLevels.Aggregate(output, (current, t) => current + (heat < t ? "☆" : "★"));
    }

    public static int GetWantedLevel(int heat) {
        for (var i = 0; i < HeatLevels.Length; i++) {
            if (HeatLevels[i] > heat) return i;
        }

        return HeatLevels.Length;
    }

    public static void Ambush(Entity userEntity, Entity playerEntity, Faction faction, int wantedLevel) {
        if (wantedLevel < 1) return;
        
        SquadList.SpawnSquad(userEntity, playerEntity, faction, wantedLevel);
        var ambushLevel = GetFactionAmbushLevel(faction, wantedLevel);
        Output.SendLore(userEntity, $"<color=#{ColourGradient[wantedLevel]}>{ambushLevel.Message}</color>");
    }
}