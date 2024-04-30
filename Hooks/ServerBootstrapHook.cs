using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Stunlock.Network;

namespace OpenRPG.Hooks
{
    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    public class ServerGameSetting_Patch
    {
        public static void Postfix()
        {
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch
    {
        public static void Postfix()
        {
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            // Save before we quit the server
            AutoSaveSystem.SaveDatabase();
            TaskRunner.Destroy();
            RandomEncounters.Unload();
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
                // TODO test this
                Plugin.LogInfo($"Vampire joined: {userData.PlatformId} {userData.CharacterName}");

                if (!isNewVampire)
                {
                    var playerName = userData.CharacterName.ToString();
                    Helper.UpdatePlayerCache(userEntity, playerName, playerName);
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