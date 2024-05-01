using System;
using OpenRPG.Utils;
using Unity.Entities;
using ProjectM.Network;

namespace OpenRPG.Systems
{
    public static class BanSystem
    {
        public static EntityManager em = Plugin.Server.EntityManager;
        public static bool IsUserBanned(ulong steamID, out BanData banData)
        {
            var isExist = Database.user_banlist.TryGetValue(steamID, out banData);
            if (isExist)
            {
                var CurrentTime = DateTime.Now;
                if (CurrentTime <= banData.BanUntil)
                {
                    return true;
                }
                else
                {
                    Database.user_banlist.Remove(steamID);
                    return false;
                }
            }
            return false;
        }

        public static bool BanUser(Entity userEntity, Entity targetUserEntity, int duration, string reason, out BanData banData)
        {
            banData = new BanData();
            var targetUserData = em.GetComponentData<User>(targetUserEntity);
            if (targetUserData.IsAdmin) return false;
            if (PermissionSystem.GetUserPermission(targetUserData.PlatformId) >= PermissionSystem.GetCommandPermission("ban player")) return false;

            DateTime banUntil;
            if (duration == 0)
            {
                banUntil = DateTime.MaxValue;
            }
            else
            {
                bool isExist = Database.user_banlist.TryGetValue(targetUserData.PlatformId, out var prevBanData);
                if (isExist) banUntil = prevBanData.BanUntil.AddDays(duration);
                else banUntil = DateTime.Now.AddDays(duration);
            }

            var userData = em.GetComponentData<User>(userEntity);
            banData.BanUntil = banUntil;
            banData.Reason = reason;
            banData.BannedBy = userData.CharacterName.ToString();
            banData.SteamID = userData.PlatformId;

            Database.user_banlist[targetUserData.PlatformId] = banData;
            return true;
        }

        public static bool UnbanUser(Entity userEntity)
        {
            var userData = em.GetComponentData<User>(userEntity);
            return Database.user_banlist.Remove(userData.PlatformId);
        }
    }
}
