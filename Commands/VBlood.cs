using Bloodstone.API;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using VRising.GameData.Models;
using VRising.GameData;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup(name: "vblood", shortHand:"vb")]
    internal class VBlood
    {
        [Command(name: "unlockability", shortHand: "ua", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all abilities that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockAbility(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.User.CharacterName.ToString());
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Ability);

        }

        [Command(name: "unlockPassive", shortHand: "up", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all passives that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockPassive(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.User.CharacterName.ToString());
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Passive);
        }

        [Command(name: "unlockshapeshift", shortHand: "us", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all shapeshifters that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockShapeshift(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.User.CharacterName.ToString());
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Shapeshift);
        }
    }
}
