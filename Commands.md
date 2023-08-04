
|Command         |Short                      | Params | Optional Params     | Usage | Description                                                                               | Admin
|----------------|-------------------------------|---------------|----------------------|----------------------|-------------------------------------|------------------------------------------------------|
|`.autorespawn` |  `.aresp`   | NO APPLY | `<PlayerName>`  |  `.autorespawn [<PlayerName>] ` | Toggle auto respawn on the same position on death |  No, Only if PlayerName is specified                   |
|`.ban info` |  `.ban info`   | `<PlayerName>` | NO APPLY  |  `.ban info <PlayerName> ` | Check the status of specified player |  No                |
|`.ban` |  `.ban `   | `<PlayerName> <days> <"<reason><"` | NO APPLY  |  `.ban <PlayerName> <playername> <days> "<reason>"` | Ban a player, 0 days is permanent. |  Yes                 |
|`.unban` |  `.uban `   | `<PlayerName>` | NO APPLY  |  `.uban <PlayerName>` | Unban the specified player. |  Yes                 |
|`.unban` |  `.uban `   | `<PlayerName>` | NO APPLY  |  `.uban <PlayerName>` | Unban the specified player. |  Yes                 |
|`.experience log` |  `.xp l `   | `<True|False>` | NO APPLY  |  `.xp log <True|False>` | Toggle the exp gain notification. |  Yes                 |
|`.experience set` |  `.xp s `   | `<PlayerName> <Value>` | NO APPLY  |  `.xp set <PlayerName> <Value>` | Sets the specified players current xp to a specific value. |  Yes                 |
|`.experience` |  `.xp `   | NO APPLY | NO APPLY  |  `.xp` | Shows your currect experience and progression to next level" |  No                 |
|`.godmode` |  `.gm `   | NO APPLY | NO APPLY  |  `.gm` | Toggles god mode. |  Yes                 |
|`.health` |  `.h `   | `<percentage>` | `[<PlayerName>]` |  `.h <percentage> [<PlayerName>]` | Sets your current Health or a specific player. |  Yes                 |
|`.heat` |  `.heat `   |  NO APPLY | NO APPLY |  `.heat` | Shows your current wanted level. |  No                 |
|`.heat set` |  `.heat set`   |  `<ValueHeatHumans> <ValueHeatBandits> <PlayerName>` | NO APPLY |  `.heat set <ValueHeatHumans> <ValueHeatBandits> <PlayerName>` | Sets a player's wanted level. |  Yes                 |
|`.heat set` |  `.heat set`   |  `<ValueHeatHumans> <ValueHeatBandits> <PlayerName>` | NO APPLY |  `.heat set <ValueHeatHumans> <ValueHeatBandits> <PlayerName>` | Sets a player's wanted level. |  Yes                 |
|`.kick` |  `.kick`   |  `<PlayerName>` | NO APPLY |  `.kick <PlayerName>` | Kick the specified player out of the server. |  Yes                 |
|`.kit` |  `.kit`   |  `<Name>` | NO APPLY |  `.kit <Name>` | Gives you a previously specified set of items. |  Through configuration                 |
|`.mastery` |  `.m`   |  NO APPLY | NO APPLY |  `.mast` | Display your current mastery progression. |  No                |
|`.mastery log` |  `.m l `   | `<True|False>` | NO APPLY  |  `.mast log <True|False>` | Toggle the mastery gain notification. |  Yes                 |
|`.mastery set` |  `.m s `   | `<type> <value>` | `<PlayerName>`  |  `.mast set <type> <value> [<PlayerName>]` | Mastery setting for yourself or a specific player by type. The types are sword, spear, crossbow, slashers, scythe, fishingpole, mace or axes |  Yes                 |
|`.ping` |  `.ping`   | NO APPLY | NO APPLY  |  `.ping` | Shows your latency. |  No                 |
|`.playerinfo` |  `.pi`   | NO APPLY | `<PlayerName>`   |  `.pinfo [<PlayerName>]` | Display your or another the player information details. |  No                 |
|`.powerup` |  `.pu`   | `<PlayerName> <add|remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>` | NO APPLY   |  `.pu <PlayerName> <add|remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>` | Buff specified player with the specified value. |  Yes                 |
|`.punish` |  `.punish`   | `<PlayerName>` | `<remove(True/False>`   |  `.punish <PlayerName> [<remove(True/False>]` | Manually punish someone or lift their debuff. |  Yes                 |
|`.pvp` |  `.pvp`   | `<PlayerName>` | `<on>|<off>|<top> <PlayerName>`   |  `.pvp [<on>|<off>|<top> <PlayerName>]` | Display your PvP statistics or toggle PvP/Castle Siege state. |  No, Only if PlayerName is specified                 |
|`.save` |  `.sv`   | NO APPLY | `"<Name>"`   |  `.sv ["<name>"]` | Force the server to save the game as well as write OpenRPG DB to a json file.   | Yes              |
|`.shutdown` |  `.shutdown`   | NO APPLY | NO APPLY   |  `.shutdown` | Trigger the exit signal & shutdown the server.   | Yes              |
|`.siege` |  `.siege`   | NO APPLY | `<on>|<off>`   |  `.siege [<on>|<off>]` | Display all players currently in siege mode, or engage siege mode.   | No              |
|`.speed` |  `.speed`   | NO APPLY | NO APPLY   |  `.speed` | Toggles increased movement speed.   | Yes              |
|`.sunimmunity` |  `.si`   | NO APPLY | NO APPLY   |  `.si` | Toggles sun immunity.   | Yes              |
|`.waypoint` |  `.wp`   | `"<Name>"` | NO APPLY   |  `.wp "<Name>"` | Teleports you to the specific waypoint.   | No              |
|`.waypoint set` |  `.wp s`   | `"<Name>"` | NO APPLY   |  `.wp set "<Name>"` | Creates the specified personal waypoint.   | Through configuration              |
|`.waypoint remove` |  `.wp r`   | `"<Name>"` | NO APPLY   |  `.wp remove "<Name>"` | Removes the specified personal waypoint.   | Through configuration              |
|`.waypoint set global` |  `.wp sg`   | `"<Name>"` | NO APPLY   |  `.wp set global"<Name>"` | Creates the specified global waypoint.   | Yes              |
|`.waypoint remove global` |  `.wp rg`   | `"<Name>"` | NO APPLY   |  `.wp remove global"<Name>"` | Removes the specified global waypoint.   | Yes              |
|`.waypoint list` |  `.wp l`   | NO APPLY | NO APPLY   |  `.wp list` | Lists waypoints available to you.   | No              |
|`.worlddynamics` |  `.wd`   | NO APPLY | `<faction>`   |  `.wd [<faction>]` | List all or specific faction stats.   | No              |
|`.worlddynamics ignore` |  `.wd ignore`   | `<NpcPrefabName>` | NO APPLY   |  `.wd ignore <NpcPrefabName>` | Ignores a specified mob for buffing.   | Yes              |
|`.worlddynamics unignore` |  `.wd unignore`   | `<NpcPrefabName>` | NO APPLY   |  `.wd unignore <NpcPrefabName>` | Removes a mob from the world dynamics ignore list..   | Yes              |
|`.worlddynamics save` |  `.wd save`   | NO APPLY | NO APPLY  |  `.wd save` | Save to the json file.   | Yes              |
|`.worlddynamics load` |  `.wd load`   | NO APPLY | NO APPLY  |  `.wd load` | Load from the json file.   | Yes              |