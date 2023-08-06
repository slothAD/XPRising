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
    [CommandGroup(name: "unlock", shortHand:"u")]
    internal class Unlock
    {
        [Command(name: "vbloodability", shortHand: "vba", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all abilities that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockAbility(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Ability);
            ctx.Reply($"All abilities from VBlood has been unlocked for {playerName}");

        }

        [Command(name: "vbloodpassive", shortHand: "vbp", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all passives that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockPassive(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Passive);
            ctx.Reply($"All passives from VBlood has been unlocked for {playerName}");
        }

        [Command(name: "vbloodshapeshift", shortHand: "vbs", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all shapeshifters that drop from killing a VBood for yourself or a player.")]
        public static void VBloodUnlockShapeshift(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Shapeshift);
            ctx.Reply($"All shapeshifters from VBlood has been unlocked for {playerName}");
        }

        [Command(name: "achievements", shortHand: "a", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all shapeshifters that drop from killing a VBood for yourself or a player.")]
        public static void Achievements(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Shapeshift);
            VWorld.Server.GetExistingSystem<DebugEventsSystem>().CompleteAllAchievements(userModel.FromCharacter);
            ctx.Reply($"All shapeshifters from VBlood has been unlocked for {playerName}");
        }

        [Command(name: "research", shortHand: "r", adminOnly: true, usage: "[<PlayerName>]", description: "Unlock all shapeshifters that drop from killing a VBood for yourself or a player.")]
        public static void Research(ChatCommandContext ctx, string playerName = null)
        {
            if (playerName == null)
            {
                playerName = ctx.User.CharacterName.ToString();
            }

            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            var systemBase = VWorld.Server.GetExistingSystem<SystemBase>();
            var buff = BuffUtility.BuffSpawnerSystemData.Create(systemBase);
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buff, userModel.FromCharacter, DebugEventsSystem.VBloodFeatureType.Shapeshift);
            VWorld.Server.GetExistingSystem<DebugEventsSystem>().UnlockAllResearch(userModel.FromCharacter);
            ctx.Reply($"All shapeshifters from VBlood has been unlocked for {playerName}");
        }
    }
}
