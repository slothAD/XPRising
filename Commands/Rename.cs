/*using OpenRPG.Utils;
using System.Text.RegularExpressions;
using Unity.Collections;

namespace OpenRPG.Commands
{
    [Command("rename", Usage = "rename <Player Name/SteamID> <New Name>", Description = "Rename the specified player.")]
    public static class Rename
    {
        public static void Initialize(ChatCommandContext ctx)
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
                    throw ctx.Error( $"Unable to find the specified player.");
                    return;
                }

                NewName = ctx.Args[1];
            }

            if (Regex.IsMatch(NewName.ToString(), @"[^a-zA-Z0-9]"))
            {
                throw ctx.Error( "Name can only contain alphanumeric!");
                return;
            }

            //-- The game default max byte length is 20.
            //-- The max legth assignable is actually 61 bytes.
            if (NewName.utf8LengthInBytes > 20)
            {
                throw ctx.Error( $"New name is too long!");
                return;
            }

            if (Cache.NamePlayerCache.TryGetValue(NewName.ToString().ToLower(), out _))
            {
                throw ctx.Error( $"Name is already taken!");
                return;
            }

            Helper.RenamePlayer(userEntity, playerEntity, NewName);
            if (userEntity.Equals(ctx.Event.SenderUserEntity))
            {
                ctx.Reply( $"Your name has been updated to \"{NewName}\".");
            }
            else
            {
                ctx.Reply( $"Player \"{ctx.Args[0]}\" name has been updated to \"{NewName}\".");
            }
        }
    }

    [Command("adminrename", Usage = "adminrename <Player Name/SteamID> <New Name>", Description = "Rename the specified player. Careful, the new name isn't parsed to be alphanumeric.")]
    public static class Adminrename
    {
        public static void Initialize(ChatCommandContext ctx)
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
                    throw ctx.Error( $"Unable to find the specified player.");
                    return;
                }

                NewName = ctx.Args[1];
            }

            //-- The game default max byte length is 20.
            //-- The max legth assignable is actually 61 bytes.
            if (NewName.utf8LengthInBytes > 20)
            {
                throw ctx.Error( $"New name is too long!");
                return;
            }

            if (Cache.NamePlayerCache.TryGetValue(NewName.ToString().ToLower(), out _))
            {
                throw ctx.Error( $"Name is already taken!");
                return;
            }

            Helper.RenamePlayer(userEntity, playerEntity, NewName);
            if (userEntity.Equals(ctx.Event.SenderUserEntity))
            {
                ctx.Reply( $"Your name has been updated to \"{NewName}\".");
            }
            else
            {
                ctx.Reply( $"Player \"{ctx.Args[0]}\" name has been updated to \"{NewName}\".");
            }
        }
    }
}
*/