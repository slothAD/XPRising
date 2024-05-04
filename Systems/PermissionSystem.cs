using System;
using ProjectM;
using ProjectM.Network;
using OpenRPG.Utils;
using System.Collections.Generic;
using System.Linq;
using OpenRPG.Utils.Prefabs;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Systems
{
    public static class PermissionSystem
    {
        public static bool isVIPSystem = true;
        public static bool isVIPWhitelist = true;
        public static int VIP_Permission = 50;

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

        public static int HighestPrivilege = 100;
        public static int LowestPrivilege = 0;
        public static bool IsUserVIP(ulong steamID)
        {
            bool isVIP = GetUserPermission(steamID) >= VIP_Permission;
            return isVIP;
        }

        public static int GetUserPermission(ulong steamID)
        {
            return Database.user_permission.GetValueOrDefault(steamID, LowestPrivilege);
        }

        public static int GetCommandPermission(string command)
        {
            return Database.command_permission.GetValueOrDefault(command, HighestPrivilege);
        }

        private static object SendPermissionList(ChatCommandContext ctx, List<string> messages)
        {
            foreach(var m in messages)
            {
                ctx.Reply(m);
            }
            return new object();
        }

        public static void UserPermissionList(ChatCommandContext ctx)
        {
            var sortedPermission = Database.user_permission.ToList();
            // Sort by privilege descending
            sortedPermission.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            ctx.Reply($"===================================");
            if (sortedPermission.Count == 0) ctx.Reply($"<color=#fffffffe>No permissions</color>");
            else
            {
                foreach (var (item, index) in sortedPermission.Select((item, index) => (item, index)))
                {
                    ctx.Reply($"{index}. <color=#fffffffe>{Helper.GetNameFromSteamID(item.Key)} : {item.Value}</color>");
                }
            }
            ctx.Reply($"===================================");
        }
        
        public static void CommandPermissionList(ChatCommandContext ctx)
        {
            var sortedPermission = Database.command_permission.ToList();
            // Sort by command name
            sortedPermission.Sort((pair1, pair2) => String.Compare(pair1.Key, pair2.Key, StringComparison.CurrentCultureIgnoreCase));
            ctx.Reply($"===================================");
            if (sortedPermission.Count == 0) ctx.Reply($"<color=#fffffffe>No commands</color>");
            else
            {
                foreach (var (item, index) in sortedPermission.Select((item, index) => (item, index)))
                {
                    ctx.Reply($"{index}. <color=#fffffffe>{item.Key} : {item.Value}</color>");
                }
            }
            ctx.Reply($"===================================");
        }

        public static void BuffReceiver(Entity buffEntity, PrefabGUID GUID)
        {
            if (!GUID.GuidHash.Equals(Buffs.Buff_OutOfCombat) && !em.HasComponent<InCombatBuff>(buffEntity)) return;
            var Owner = em.GetComponentData<EntityOwner>(buffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity;
            var SteamID = em.GetComponentData<User>(userEntity).PlatformId;

            if (IsUserVIP(SteamID))
            {
                var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
                //-- Out of Combat Buff
                if (GUID.GuidHash.Equals(Buffs.Buff_OutOfCombat))
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
            var permissions = new Dictionary<string, int>()
            {
                {"autorespawn", 100},
                {"autorespawn-all", 100},
                {"ban info", 0},
                {"ban player", 100},
                {"ban unban", 100},
                {"bloodline add", 100},
                {"bloodline get", 0},
                {"bloodline get-all", 0},
                {"bloodline log", 0},
                {"bloodline reset", 0},
                {"bloodline set", 100},
                {"experience ability", 0},
                {"experience ability reset", 50},
                {"experience ability show", 0},
                {"experience get", 0},
                {"experience log", 0},
                {"experience set", 100},
                {"godmode", 100},
                {"kick", 100},
                {"kit", 100},
                {"load", 100},
                {"mastery add", 100},
                {"mastery get", 0},
                {"mastery get-all", 0},
                {"mastery log", 0},
                {"mastery reset", 0},
                {"mastery set", 100},
                {"nocooldown", 100},
                {"permission", 100},
                {"permission set command", 100},
                {"permission set user", 100},
                {"playerinfo", 0},
                {"powerdown", 100},
                {"powerup", 100},
                {"re disable", 100},
                {"re enable", 100},
                {"re me", 100},
                {"re player", 100},
                {"re start", 100},
                {"save", 100},
                {"speed", 100},
                {"sunimmunity", 100},
                {"unlock achievements", 100},
                {"unlock research", 100},
                {"unlock vbloodability", 100},
                {"unlock vbloodpassive", 100},
                {"unlock vbloodshapeshift", 100},
                {"wanted fixminions", 100},
                {"wanted get", 0},
                {"wanted log", 0},
                {"wanted set", 100},
                {"wanted trigger", 100},
                {"waypoint go", 100},
                {"waypoint list", 0},
                {"waypoint remove", 100},
                {"waypoint remove global", 100},
                {"waypoint set", 100},
                {"waypoint set global", 100},
                {"worlddynamics ignore", 100},
                {"worlddynamics info", 0},
                {"worlddynamics load", 100},
                {"worlddynamics save", 100},
                {"worlddynamics unignore", 100}
            };
            return permissions;
        }
    }
}
