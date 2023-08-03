/*using ProjectM;
using ProjectM.Network;
using OpenRPG.Utils;
using VampireCommandFramework;
using Unity.Collections;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class BloodSet
    {
        [Command("blood", usage: "<Type> [<Quality>] [<Value>]", description: "Sets your current Blood Type, Quality and Value")]
        public static void BloodSetCommand(ChatCommandContext ctx, string typeName, int qualityInt, int value)
        {

            float quality = qualityInt;

            PrefabGUID type = Helper.GetSourceTypeFromName(typeName);

            if (qualityInt < 0) quality = 0;
            if (qualityInt > 100) quality = 100;

            var BloodEvent = new ChangeBloodDebugEvent()
            {
                Amount = value,
                //Quality = quality,
                //Source = type
            };
            Plugin.Server.GetExistingSystem<DebugEventsSystem>().ChangeBloodEvent(ctx.Event.User.Index, ref BloodEvent);
            ctx.Reply($"Changed Blood Type to <color=#ffff00>{typeName}</color> with <color=#ffff00>{quality}</color>% quality");

        }
    }
}*/
