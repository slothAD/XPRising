using System;
using ProjectM;

namespace RPGMods.Utils;

public static class Faction {
    public enum Type {
        Bandit = -413163549,
        Bears = 1344481611,
        Cursed = 1522496317,
        Farmland = 1057375699,
        Fauna = 10678632,
        Flora = -1414061934,
        Forest = -1632009503,
        Geomancer = 1597367490,
        Harpy = 1731533561,
        Horse = -1430861195,
        HumanCleric = 2395673,
        HumanGloom = -1632475814,
        Merchant = 887347866,
        Mutant = -210606557,
        PCSummoned = 1106458752,
        Prisoner = 671871002,
        RockElemental = 1513046884,
        ShadyMerchants = 30052367,
        Town = 1094603131,
        Undead = 929074293,
        Unknown = 0,
        VampireHunter = 2120169232,
        Werewolves = -2024618997,
        Winter = -535162217,
        Wolves = -1671358863,
    }
    // TODO investigate what this one was
    // [Warning:RPGMods - Gloomrot] Entity: 286320185 Unknown faction: 1977351396
    public static Type ConvertGuidToFaction(PrefabGUID guid) {
        if (Enum.IsDefined(typeof(Type), guid.GetHashCode())) return (Type)guid.GetHashCode();
        return Type.Unknown;
    }
}