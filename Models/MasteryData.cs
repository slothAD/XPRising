using System;
using ProjectM;

namespace OpenRPG.Models;

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

public struct StatConfig(UnitStatType type, double strength, double rate)
{
    public UnitStatType type = type;
    public double strength = strength;
    public double rate = rate;
}