using System;
using ProjectM.Network;
using Unity.Entities;

namespace RPGMods.Utils;

public static class FactionHeat {
    // Ideally this would be a higher level enum to have consistent faction support across features
    public enum Faction {
        Human,
        Bandit,
        Undead,
    }
    
    private static readonly string[] ColourGradient = { "fef001", "ffce03", "fd9a01", "fd6104", "ff2c05", "f00505" };

    private struct FactionState {
        public int HeatLevel { get; }
        public int UpperHeat { get; }
        public string StateMessage { get; }
        public string AmbushMessage { get; }

        public FactionState(int level, int heat, string state, string ambush) {
            HeatLevel = level;
            UpperHeat = heat;
            StateMessage = state;
            AmbushMessage = ambush;
        }
    }

    private static readonly FactionState[] BanditHeatLevels =
    {
        new(0, 150, "The bandits do not recognise you...", ""),
        new(1, 250, "The bandits are hunting you...", "The bandits are ambushing you!"),
        new(2, 450, "Small bandit squads are hunting you...", "A small bandit squad is ambushing you!"),
        new(3, 650, "Large bandit squads are hunting you...", "A large bandit squad is ambushing you!"),
        new(4, 1000, "The bandits really wants you dead...", "The bandits are ambushing you with as many people they can spare!")
    };
    
    private static readonly FactionState[] HumanHeatLevels =
    {
        new(0, 150, "The humans do not recognise you...", ""),
        new(1, 250, "The humans are hunting you...", "A militia squad is ambushing you!"),
        new(2, 500, "Humans soldiers are hunting you...", "A squad of soldiers is ambushing you!"),
        new(3, 1000, "Humans elite squads are hunting you...", "An ambush squad from the Church has been sent to kill you!"),
        new(4, 1500, "The Vampire Hunters are hunting you...", "The Vampire Hunters are ambushing you!"),
        new(5, 2000, "YOU ARE A MENACE...", "An extermination squad has found you and wants you DEAD.")
    };

    private static FactionState GetFactionState(Faction faction, int heat) {
        var factionStates = faction switch {
                Faction.Human => HumanHeatLevels,
                _ => BanditHeatLevels
            };
        
        foreach (var t in factionStates) {
            if (t.UpperHeat > heat) return t;
        }

        // Otherwise just return highest status
        return factionStates[^1];
    }

    public static string GetFactionStatus(Faction faction, int heat) {
        return GetFactionState(faction, heat).StateMessage;
    }

    public static void Ambush(Entity userEntity, Entity playerEntity, Faction faction, int heatLevel, Random rand) {
        var factionState = GetFactionState(faction, heatLevel);

        if (factionState.HeatLevel == 0) return;

        switch (faction) {
            case Faction.Human:
                HumanAmbush(userEntity, playerEntity, factionState, rand);
                break;
            case Faction.Bandit:
                BanditAmbush(userEntity, playerEntity, factionState, rand);
                break;
            case Faction.Undead:
                break;
            default:
                Plugin.Logger.LogWarning($"Ambush not yet supported for this faction: {Enum.GetName(faction)}");
                break;
        }
    }

    private static void BanditAmbush(Entity userEntity, Entity playerEntity, FactionState factionState, Random rand) {
        switch (factionState.HeatLevel) {
            case 4:
                SquadList.SpawnSquad(playerEntity, 0, rand.Next(20, 25));
                break;
            case 3:
                SquadList.SpawnSquad(playerEntity, 0, rand.Next(10, 15));
                break;
            case 2:
                SquadList.SpawnSquad(playerEntity, 0, 5);
                break;
            case 1:
                SquadList.SpawnSquad(playerEntity, 0, 3);
                break;
            default:
                // Do nothing
                return;
        }

        Output.SendLore(userEntity, $"<color=#{ColourGradient[factionState.HeatLevel]}>{factionState.AmbushMessage}</color>");
    }
    
    private static void HumanAmbush(Entity userEntity, Entity playerEntity, FactionState factionState, Random rand) {
        switch (factionState.HeatLevel) {
            case 5:
                SquadList.SpawnSquad(playerEntity, 4, rand.Next(10, 20));
                SquadList.SpawnSquad(playerEntity, 5, 2);
                break;
            case 4:
                if (rand.Next(0, 100) < 50) {
                    SquadList.SpawnSquad(playerEntity, 5, 1);
                    SquadList.SpawnSquad(playerEntity, 4, 9);
                }
                else {
                    SquadList.SpawnSquad(playerEntity, 4, rand.Next(15, 20));
                }
                break;
            case 3:
                SquadList.SpawnSquad(playerEntity, 3, rand.Next(10, 15));
                break;
            case 2:
                SquadList.SpawnSquad(playerEntity, 2, rand.Next(10, 15));
                break;
            case 1:
                SquadList.SpawnSquad(playerEntity, 1, rand.Next(5, 10));
                break;
            default:
                // Do nothing
                return;
        }

        Output.SendLore(userEntity, $"<color=#{ColourGradient[factionState.HeatLevel]}>{factionState.AmbushMessage}</color>");
    }
}