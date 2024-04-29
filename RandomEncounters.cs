using OpenRPG.Components.RandomEncounters;
using OpenRPG.Configuration;
using OpenRPG.Systems;
using OpenRPG.Utils.RandomEncounters;
using System;

namespace OpenRPG
{
    internal static class RandomEncounters
    {

        public static Timer _encounterTimer;

        public static void Load()
        {
            GameFrame.Initialize();
        }

        internal static void GameData_OnInitialize()
        {
            Plugin.Logger.LogInfo("Loading main data RandomEncounters");
            DataFactory.Initialize();
            Plugin.Logger.LogInfo("Binding configuration RandomEncounters");
            RandomEncountersConfig.Initialize();
        }

        public static void StartEncounterTimer()
        {
            _encounterTimer.Start(
                _ =>
                {
                    Plugin.Logger.LogInfo($"{DateTime.Now.ToString()} Starting an encounter.");
                    RandomEncountersSystem.StartEncounter();
                },
                input =>
                {
                    if (input is not int onlineUsersCount)
                    {
                        Plugin.Logger.LogError("Encounter timer delay function parameter is not a valid integer");
                        return TimeSpan.MaxValue;
                    }
                    if (onlineUsersCount < 1)
                    {
                        onlineUsersCount = 1;
                    }
                    var seconds = new Random().Next(RandomEncountersConfig.EncounterTimerMin.Value, RandomEncountersConfig.EncounterTimerMax.Value);
                    Plugin.Logger.LogInfo($"{DateTime.Now.ToString()} Next encounter will start in {seconds / onlineUsersCount} seconds.");
                    return TimeSpan.FromSeconds(seconds) / onlineUsersCount;
                });
        }

        public static void Unload()
        {
            _encounterTimer?.Stop();
            GameFrame.Uninitialize();
            Plugin.Logger.LogInfo($"RandomEncounters unloaded!");
        }
    }
}
