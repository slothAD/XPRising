using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Stunlock.Network;

namespace OpenRPG.Hooks
{
    // TODO does this need to run as well as the GameBootstrap below?
    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    public class ServerGameSetting_Patch
    {
        private static bool isInitialized = false;
        public static void Postfix()
        {
            if (isInitialized == false)
            {
                Plugin.Initialize();
                isInitialized = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch
    {
        public static void Postfix()
        {
            Plugin.Initialize();
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class OnUserConnected_Patch
    {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            try
            {
                var em = __instance.EntityManager;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userEntity = serverClient.UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                if (!isNewVampire)
                {
                    {
                        var playerName = userData.CharacterName.ToString();
                        Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                    }
                    if (WeaponMasterSystem.isDecaySystemEnabled && WeaponMasterSystem.isMasteryEnabled)
                    {
                        WeaponMasterSystem.DecayMastery(userEntity);
                    }
                }
            }
            catch { }
        }
    }
}