using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace RPGMods.Utils;

public static class FactionHeat {
    public static readonly Faction.Type[] ActiveFactions = { Faction.Type.Militia, Faction.Type.Bandits, Faction.Type.Undead };
    
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

    private static AmbushLevel GetFactionAmbushLevel(Faction.Type type, int wantedLevel) {
        if (wantedLevel == 1) return UnknownAmbushLevels[0];
        var factionStates = type switch {
                Faction.Type.Militia => HumanAmbushLevels,
                Faction.Type.Bandits => BanditAmbushLevels,
                Faction.Type.Undead => UndeadAmbushLevels,
                _ => UnknownAmbushLevels
            };
        
        return factionStates[Math.Max(wantedLevel, factionStates.Length) - 1];
    }

    private static int vBloodMultiplier = 20;
    public static void GetActiveFactionHeatValue(Faction.Type faction, bool isVBlood, out int heatValue, out Faction.Type activeFaction) {
        switch (faction) {
            // Bandit
            case Faction.Type.Traders_T01:
                heatValue = 200; // Don't kill the merchants
                activeFaction = Faction.Type.Bandits;
                break;
            case Faction.Type.Bandits:
                heatValue = 10;
                activeFaction = Faction.Type.Bandits;
                break;
            // Human
            case Faction.Type.Militia:
                heatValue = 10;
                activeFaction = Faction.Type.Militia;
                break;
            case Faction.Type.ChurchOfLum_SpotShapeshiftVampire:
                heatValue = 25;
                activeFaction = Faction.Type.Militia;
                break;
            case Faction.Type.Traders_T02:
                heatValue = 200; // Don't kill the merchants
                activeFaction = Faction.Type.Militia;
                break;
            case Faction.Type.ChurchOfLum:
                heatValue = 15;
                activeFaction = Faction.Type.Militia;
                break;
            // Human: gloomrot
            case Faction.Type.Gloomrot:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Type.Unknown;
                break;
            // Nature
            case Faction.Type.Bear:
            case Faction.Type.Critters:
            case Faction.Type.Wolves:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Type.Unknown;
                break;
            // Undead
            case Faction.Type.Undead:
                heatValue = 3;
                activeFaction = Faction.Type.Undead;
                break;
            // Werewolves
            case Faction.Type.Werewolf:
            case Faction.Type.WerewolfHuman:
                // TODO
                heatValue = 0;
                activeFaction = Faction.Type.Unknown;
                break;
            case Faction.Type.VampireHunters:
                heatValue = 10;
                activeFaction = Faction.Type.VampireHunters;
                break;
            // Do nothing
            case Faction.Type.ChurchOfLum_Slaves:
            case Faction.Type.ChurchOfLum_Slaves_Rioters:
            case Faction.Type.Cursed:
            case Faction.Type.Elementals:
            case Faction.Type.Ignored:
            case Faction.Type.Harpy:
            case Faction.Type.Mutants:
            case Faction.Type.NatureSpirit:
            case Faction.Type.Plants:
            case Faction.Type.Players:
            case Faction.Type.Players_Castle_Prisoners:
            case Faction.Type.Players_Mutant:
            case Faction.Type.Players_Shapeshift_Human:
            case Faction.Type.Spiders:
            case Faction.Type.Unknown:
            case Faction.Type.Wendigo:
            case Faction.Type.World_Prisoners:
                heatValue = 0;
                activeFaction = Faction.Type.Unknown;
                break;
            default:
                Plugin.Logger.LogWarning($"Faction not handled for active faction: {Enum.GetName(faction)}");
                heatValue = 0;
                activeFaction = Faction.Type.Unknown;
                break;
        }

        if (isVBlood) heatValue *= vBloodMultiplier;
    }

    public static string GetFactionStatus(Faction.Type type, int heat) {
        var output = $"{Enum.GetName(type)}: ";
        return HeatLevels.Aggregate(output, (current, t) => current + (heat < t ? "☆" : "★"));
    }

    public static int GetWantedLevel(int heat) {
        for (var i = 0; i < HeatLevels.Length; i++) {
            if (HeatLevels[i] > heat) return i;
        }

        return HeatLevels.Length;
    }

    public static void Ambush(Entity userEntity, Entity playerEntity, Faction.Type type, int heatLevel, Random rand) {
        var wantedLevel = GetWantedLevel(heatLevel);
        if (wantedLevel == 0) return;
        
        SquadList.SpawnSquad(playerEntity, type, wantedLevel);
        var ambushLevel = GetFactionAmbushLevel(type, wantedLevel);
        Output.SendLore(userEntity, $"<color=#{ColourGradient[wantedLevel]}>{ambushLevel.Message}</color>");
        
        // var ambushLevel = GetFactionAmbushLevel(type, wantedLevel);
        //
        // var squadOptions = ambushLevel.SquadOptions;
        // if (squadOptions.Length > 0) {
        //     var squadIndex = rand.Next(0, squadOptions.Length);
        //     var squadDetails = squadOptions[squadIndex];
        //
        //     foreach (var spawn in squadDetails) {
        //         SquadList.SpawnSquad(playerEntity, type, rand.Next(spawn.minCount, spawn.maxCount));
        //     }
        //     Output.SendLore(userEntity, $"<color=#{ColourGradient[wantedLevel]}>{ambushLevel.Message}</color>");
        // }
    }
}