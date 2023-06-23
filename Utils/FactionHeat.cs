using System;
using System.Linq;
using RPGMods.Systems;
using Unity.Entities;
using Faction = RPGMods.Utils.Prefabs.Faction;

namespace RPGMods.Utils;

public static class FactionHeat {
    public static readonly Faction[] ActiveFactions = {
        Faction.Militia,
        Faction.Bandits,
        Faction.Undead,
        Faction.Gloomrot,
        Faction.Critters,
        Faction.Werewolf
    };
    
    private static readonly string[] ColourGradient = { "fef001", "ffce03", "fd9a01", "fd6104", "ff2c05", "f00505" };

    public static readonly int[] HeatLevels = { 150, 250, 500, 1000, 1500, 3000 };
    
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
                heatValue = 10;
                activeFaction = Faction.Gloomrot;
                break;
            // Nature
            case Faction.Bear:
            case Faction.Critters:
            case Faction.Wolves:
                heatValue = 10;
                activeFaction = Faction.Critters;
                break;
            // Undead
            case Faction.Undead:
                heatValue = 3;
                activeFaction = Faction.Undead;
                break;
            // Werewolves
            case Faction.Werewolf:
            case Faction.WerewolfHuman:
                heatValue = 20;
                activeFaction = Faction.Werewolf;
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

        if (isVBlood) heatValue *= HunterHuntedSystem.vBloodMultiplier;
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
        
        var squadMessage = SquadList.SpawnSquad(userEntity, playerEntity, faction, wantedLevel);
        Output.SendLore(userEntity, $"<color=#{ColourGradient[wantedLevel]}>{squadMessage}</color>");
    }
}