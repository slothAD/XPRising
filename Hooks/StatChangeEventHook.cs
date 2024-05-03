using HarmonyLib;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using System;
using BepInEx.Logging;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Hooks
{
    public delegate void OnUpdateEventHandler(World world);
    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    public class SCSHook {
        public static event OnUpdateEventHandler OnUpdate;
        private static void Postfix(StatChangeSystem __instance) {
            try {
                OnUpdate?.Invoke(__instance.World);
            } catch (Exception e) {
                Plugin.Log(LogSystem.Plugin, LogLevel.Error, $"SCSHook: {e}");
            }
        }
    }
}
