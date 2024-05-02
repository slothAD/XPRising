using ProjectM;
using Unity.Entities;

namespace OpenRPG.Utils
{
    public static class Output
    {
        public static void CustomErrorMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000>{message}</color>");
        }
        
        public static void SendSystemMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"{message}");
        }

        public static void InvalidCommand(VChatEvent ev)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000>Invalid command.</color>");
        }
        
        public static void SendLore(Entity userEntity, string message)
        {
            var user = Plugin.Server.EntityManager.GetComponentData<ProjectM.Network.User>(userEntity);
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, user, message);
        }
    }
}
