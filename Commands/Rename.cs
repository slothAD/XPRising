using ProjectM;
using RPGMods.Utils;
using System.Text.RegularExpressions;
using Unity.Collections;
using VampireCommandFramework;

namespace RPGMods.Commands
{/*
    public static class Rename
    {
        [Command("rename", usage: "rename <Player Name/SteamID> <New Name>", description: "Rename the specified player.")]
        public static void Initialize( ctx)
        {
            if (ctx.Args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            var playerEntity = ctx.Event.SenderCharacterEntity;
            var userEntity = ctx.Event.SenderUserEntity;
            FixedString64 NewName = ctx.Args[0];
            if (ctx.Args.Length > 1)
            {
                bool isPlayerFound;
                if (ulong.TryParse(ctx.Args[0], out var SteamID)) isPlayerFound = Helper.FindPlayer(SteamID, false, out playerEntity, out userEntity);
                else isPlayerFound = Helper.FindPlayer(ctx.Args[0], false, out playerEntity, out userEntity);

                if (!isPlayerFound)
                {
                    ctx.Reply($"Unable to find the specified player.");
                    return;
                }

                NewName = ctx.Args[1];
            }

            if (Regex.IsMatch(NewName.ToString(), @"[^a-zA-Z0-9]"))
            {
                ctx.Reply("Name can only contain alphanumeric!");
                return;
            }

            //-- The game default max byte length is 20.
            //-- The max legth assignable is actually 61 bytes.
            if (NewName.utf8LengthInBytes > 20)
            {
                ctx.Reply($"New name is too long!");
                return;
            }

            if (Cache.NamePlayerCache.TryGetValue(NewName.ToString().ToLower(), out _))
            {
                ctx.Reply($"Name is already taken!");
                return;
            }

            Helper.RenamePlayer(userEntity, playerEntity, NewName);
            if (userEntity.Equals(ctx.Event.SenderUserEntity))
            {
                ctx.Reply($"Your name has been updated to \"{NewName}\".");
            }
            else
            {
                ctx.Reply($"Player \"{ctx.Args[0]}\" name has been updated to \"{NewName}\".");
            }
        }
    }
    */
   public static class Adminrename
    {
        [Command("adminrename", usage: "adminrename <Player Name/SteamID> <New Name>", description: "Rename the specified player. Careful, the new name isn't parsed to be alphanumeric.", adminOnly: true)]
        public static void Initialize(ChatCommandContext ctx, string oldName, string newName)
        {
            var playerEntity = ctx.Event.SenderCharacterEntity;
            var userEntity = ctx.Event.SenderUserEntity;
            FixedString64 NewName = newName;
            bool isPlayerFound;
            if (ulong.TryParse(oldName, out var SteamID)) isPlayerFound = Helper.FindPlayer(SteamID, false, out playerEntity, out userEntity);
            else isPlayerFound = Helper.FindPlayer(oldName, false, out playerEntity, out userEntity);

            if (!isPlayerFound)
            {
                ctx.Reply($"Unable to find the specified player.");
                return;
            }


            //-- The game default max byte length is 20.
            //-- The max legth assignable is actually 61 bytes.
            if (NewName.utf8LengthInBytes > 20)
            {
                ctx.Reply($"New name is too long!");
                return;
            }

            if (Cache.NamePlayerCache.TryGetValue(NewName.ToString().ToLower(), out _))
            {
                ctx.Reply($"Name is already taken!");
                return;
            }

            Helper.RenamePlayer(userEntity, playerEntity, NewName);
            if (userEntity.Equals(ctx.Event.SenderUserEntity))
            {
                ctx.Reply($"Your name has been updated to \"{NewName}\".");
            }
            else
            {
                ctx.Reply($"Player \"{oldName}\" name has been updated to \"{NewName}\".");
            }
        }
    }
}
