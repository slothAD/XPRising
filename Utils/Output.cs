using ProjectM;
using ProjectM.Network;
using OpenRPG.Hooks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using static VCF.Core.Basics.RoleCommands;
using Bloodstone.API;

namespace OpenRPG.Utils
{
    public static class Output
    {
        /*
        public static void CustomErrorMessage(ChatCommandContext ctx, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Vworld.Server, ctx.Event.User, $"<color=#ff0000>{message}</color>");
        }

        public static void CustomErrorMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000>{message}</color>");
        }

        public static void SendSystemMessage(ChatCommandContext ctx, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Vworld.Server, ctx.Event.User, $"{message}");
        }

        public static void SendSystemMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"{message}");
        }

        public static void InvalidCommand(VChatEvent ev)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000>Invalid command.</color>");
        }

        public static void InvalidArguments(ChatCommandContext ctx)
        {
            ServerChatUtils.SendSystemMessageToClient(Vworld.Server, ctx.Event.User, $"<color=#ff0000>Invalid command parameters. Check {ctx.Prefix}help [<command>] for more information.</color>");
        }

        public static void MissingArguments(ChatCommandContext ctx)
        {
            ServerChatUtils.SendSystemMessageToClient(Vworld.Server, ctx.Event.User, $"<color=#ff0000>Missing command parameters. Check {ctx.Prefix}help [<command>] for more information.</color>");
        }
        */

        public static void SendLore(Entity userEntity, string message)
        {
            ProjectM.Network.User user = VWorld.Server.EntityManager.GetComponentData<ProjectM.Network.User>(userEntity);
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
        }
    }
}
