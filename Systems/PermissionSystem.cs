using ProjectM;
using ProjectM.Network;
using OpenRPG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Systems
{
    public static class PermissionSystem
    {
        public static bool isVIPSystem = true;
        public static bool isVIPWhitelist = true;
        public static int VIP_Permission = 10;

        public static double VIP_OutCombat_ResYield = -1.0;
        public static double VIP_OutCombat_DurabilityLoss = -1.0;
        public static double VIP_OutCombat_MoveSpeed = -1.0;
        public static double VIP_OutCombat_GarlicResistance = -1.0;
        public static double VIP_OutCombat_SilverResistance = -1.0;

        public static double VIP_InCombat_ResYield = -1.0;
        public static double VIP_InCombat_DurabilityLoss = -1.0;
        public static double VIP_InCombat_MoveSpeed = -1.0;
        public static double VIP_InCombat_GarlicResistance = -1.0;
        public static double VIP_InCombat_SilverResistance = -1.0;

        private static EntityManager em = Plugin.Server.EntityManager;

        public static bool IsUserVIP(ulong steamID)
        {
            bool isVIP = GetUserPermission(steamID) >= VIP_Permission;
            return isVIP;
        }

        public static int GetUserPermission(ulong steamID)
        {
            bool isExist = Database.user_permission.TryGetValue(steamID, out var permission);
            if (isExist) return permission;
            return 0;
        }

        public static int GetCommandPermission(string command)
        {
            var isExist = Database.command_permission.TryGetValue(command, out int requirement);
            if (isExist) return requirement;
            else
            {
                Database.command_permission[command] = 100;
            }
            return 100;
        }

        public static bool PermissionCheck(ulong steamID, string command)
        {
            bool isAllowed = GetUserPermission(steamID) >= GetCommandPermission(command);
            return isAllowed;
        }

        private static object SendPermissionList(ChatCommandContext ctx, List<string> messages)
        {
            foreach(var m in messages)
            {
                ctx.Reply(m);
            }
            return new object();
        }

        public static async Task PermissionList(ChatCommandContext ctx)
        {
            await Task.Yield();

            List<string> messages = new List<string>();

            var SortedPermission = Database.user_permission.ToList();
            SortedPermission.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            var ListPermission = SortedPermission;
            messages.Add($"===================================");
            if (ListPermission.Count == 0) messages.Add($"<color=#fffffffe>No Result</color>");
            else
            {
                int i = 0;
                foreach (var result in ListPermission)
                {
                    i++;
                    messages.Add($"{i}. <color=#fffffffe>{Helper.GetNameFromSteamID(result.Key)} : {result.Value}</color>");
                }
            }
            messages.Add($"===================================");

            TaskRunner.Start(taskWorld => SendPermissionList(ctx, messages), false);
        }

        public static void BuffReceiver(Entity buffEntity, PrefabGUID GUID)
        {
            if (!GUID.Equals(Database.Buff.OutofCombat) && !em.HasComponent<InCombatBuff>(buffEntity)) return;
            var Owner = em.GetComponentData<EntityOwner>(buffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity;
            var SteamID = em.GetComponentData<User>(userEntity).PlatformId;

            if (IsUserVIP(SteamID))
            {
                var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
                //-- Out of Combat Buff
                if (GUID.Equals(Database.Buff.OutofCombat))
                {
                    if (VIP_OutCombat_ResYield > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ResourceYield,
                            Value = (float)VIP_OutCombat_ResYield,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_DurabilityLoss > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ReducedResourceDurabilityLoss,
                            Value = (float)VIP_OutCombat_DurabilityLoss,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_MoveSpeed > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)VIP_OutCombat_MoveSpeed,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_GarlicResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.GarlicResistance,
                            Value = (float)VIP_OutCombat_GarlicResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_SilverResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.SilverResistance,
                            Value = (float)VIP_OutCombat_SilverResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                }
                //-- In Combat Buff
                else if (em.HasComponent<InCombatBuff>(buffEntity))
                {
                    if (VIP_InCombat_ResYield > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ResourceYield,
                            Value = (float)VIP_InCombat_ResYield,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_DurabilityLoss > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ReducedResourceDurabilityLoss,
                            Value = (float)VIP_InCombat_DurabilityLoss,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_MoveSpeed > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)VIP_InCombat_MoveSpeed,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_GarlicResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.GarlicResistance,
                            Value = (float)VIP_InCombat_GarlicResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_SilverResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.SilverResistance,
                            Value = (float)VIP_InCombat_SilverResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                }
            }
        }

        public static Dictionary<string, int> DefaultCommandPermissions()
        {
            var permissions = new Dictionary<string, int>();
            permissions["help"] = 0;
            permissions["ping"] = 0;
            permissions["myinfo"] = 0;
            permissions["pvp"] = 0;
            permissions["pvp_args"] = 100;
            permissions["siege"] = 0;
            permissions["siege_args"] = 100;
            permissions["wanted"] = 0;
            permissions["wanted_args"] = 100;
            permissions["experience"] = 0;
            permissions["experience_args"] = 100;
            permissions["mastery"] = 0;
            permissions["mastery_args"] = 100;
            permissions["autorespawn"] = 100;
            permissions["autorespawn_args"] = 100;
            permissions["waypoint"] = 100;
            permissions["waypoint_args"] = 100;
            permissions["ban"] = 100;
            permissions["bloodpotion"] = 100;
            permissions["blood"] = 100;
            permissions["customspawn"] = 100;
            permissions["give"] = 100;
            permissions["godmode"] = 100;
            permissions["health"] = 100;
            permissions["kick"] = 100;
            permissions["kit"] = 100;
            permissions["nocooldown"] = 100;
            permissions["permission"] = 100;
            permissions["playerinfo"] = 100;
            permissions["punish"] = 100;
            permissions["rename"] = 100;
            permissions["adminrename"] = 100;
            permissions["resetcooldown"] = 100;
            permissions["save"] = 100;
            permissions["shutdown"] = 100;
            permissions["spawnnpc"] = 100;
            permissions["speed"] = 100;
            permissions["sunimmunity"] = 100;
            permissions["teleport"] = 100;
            permissions["worlddynamics"] = 100;
            return permissions;
        }
    }
}
