using HarmonyLib;
using ProjectM;
using ProjectM.Auth;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Terrain;
using RPGMods.Systems;
using RPGMods.Utils;
using Stunlock.Network;
using System;
using System.Reflection;

namespace RPGMods.Hooks {
    /*
    [HarmonyPatch(typeof(LoadPersistenceSystemV2), nameof(LoadPersistenceSystemV2.SetLoadState))]
    public class PersistenceSystem_Patch
    {
        public static void Prefix(ServerStartupState.State loadState, LoadPersistenceSystemV2 __instance)
        {
            if (loadState == ServerStartupState.State.SuccessfulStartup)
            {
                Plugin.Initialize();
            }
        }
    }*/


    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    public class ServerGameSetting_Patch {
        private static bool isInitialized = false;
        public static void Postfix() {
            if (isInitialized == false) {
                Plugin.Initialize();
                isInitialized = true;
            }
        }
    }
    /*
    [HarmonyPatch(typeof(HandleGameplayEventsSystem), nameof(HandleGameplayEventsSystem.OnUpdate))]
    public class InitializationPatch
    {
        [HarmonyPostfix]
        public static void RPGMods_Initialize_Method()
        {
            Plugin.Initialize();
            Plugin.harmony.Unpatch(typeof(HandleGameplayEventsSystem).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("RPGMods_Initialize_Method"));
        }
    }*/

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch {
        public static void Postfix() {
            Plugin.Initialize();
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch {
        public static void Prefix() {
            AutoSaveSystem.SaveDatabase();
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class OnUserConnected_Patch {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId) {
            try {
                var em = __instance.EntityManager;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userEntity = serverClient.UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                if (!isNewVampire) {
                    var playerName = userData.CharacterName.ToString();
                    Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                    if (WeaponMasterSystem.isDecaySystemEnabled && WeaponMasterSystem.isMasteryEnabled) {
                        WeaponMasterSystem.DecayMastery(userEntity);
                    }
                }
            } catch { }
        }
    }
    /*
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.BeginSetupServer))]
    public static class BeginSetupServer_Patch
    {
        private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
        {
            Plugin.Initialize();
        }
    }*/
}