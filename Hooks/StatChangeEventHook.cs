using HarmonyLib;
using Unity.Entities;
using System;
using ProjectM.Gameplay.Systems;

namespace RPGMods.Hooks {
    public delegate void OnUpdateEventHandler(World world);
    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    public class SCSHook {
        public static event OnUpdateEventHandler OnUpdate;
        private static void Postfix(StatChangeSystem __instance) {
            try {
                OnUpdate?.Invoke(__instance.World);
            } catch (Exception e) {
                Plugin.Logger.LogError(e);
            }
        }
    }
}
