# RPGMods - Gloomrot Update
### Server Only Mod
Server only mod for RPG systems, which also includes ChatCommands with bug fixes.
Read the changelog for extra details.
My Fork adds a number of config options to mastery and allows you to invert the dynamic faction system, making them stronger when killed.
#### [Video Demo of Experience & Mastery](https://streamable.com/k2p3bm)

## Important Changes
If you are updating from before 1.7.0 and you had custom rates for bloodlines or mastery, you will need to manually move them to their new names of Rates, Bloodline and Rates, Mastery which will be created when you next launch, this was done to make the config more readable in the future.

## Gloomrot changes
Many commands are now in community commands, Use that!

<details>
1.7.5 Several Heat fixes, returns everything but cdr to normal stats rather than inverse multipliers, and updates default configs.

1.7.4 removes the PvP stuff and fixes a major dumb bug from 1.7.3 DO NOT use 1.7.3. PvPMods coming soon with the pvp content, currently on patreon.

1.7.3 adds some bug fixes, waypoints should be linked to the config properly for example.

1.7.2 brings back waypoints, along with some bug fixes.

1.7.0 has a bevy of new configuration options for how to handle XP loss under the new Rates, Experience section, and merged in an XP sharing rework from aontas.
It also includes config options for what buff to hijack, and what buff to apply when your mastery or bloodline changes, found in the Buff System section.
It also changes the names of Mastery rates and Bloodline Rates config options to Rates, Mastery and Rates, Bloodline respectively

1.6.3 has several bug fixes, some formatting updates and bug fixes from Aontas, and a compatability fix for serverlaunchfix by Deca
It also has new debug log config options, turn them on if you need to give me the logs
</details>

## Experience System
<details>
<summary>Experience System</summary>
Disable the VRising Gear Level system and replace it with a traditional RPG experience system,\
complete with exp sharing between clan members or other players designated as allies.
</details>
Now with a class system, currently undocumented.

#### Group XP
Killing with other vampires can provide group XP. This is governed by `Group Modifier`, `Group Level Scheme` and `Ally Max Distance`.
<details>
<summary>Group XP options</summary>

A vampire is considered to be in your group if they are in the same clan and within the distance specified by `Ally Max Distance`.

Group XP is modified by the `Group Modifier` and `Group Level Scheme`.

Given a scenario of 2 allied vampires close together, PC 1 (lvl 10), PC 2 (lvl 20), where PC 1 kills the mob, the following table shows the level used to calculate each players XP:

| Scheme | Name        | PC 1 | PC 2 |
|--------|-------------|------|------|
| 0      | None        | 10   | N/A  |
| 1      | Average     | 15   | 15   |
| 2      | Max         | 20   | 20   |
| 3      | Each player | 10   | 20   |
| 4      | Killer      | 10   | 10   |

Notes:
- `0`: Effectively disables group XP. Each vampire only gets XP for mobs that they get the killing blow on
- `1`: Higher level vampires get more XP when grouped with lower level vampires
- `2`: Lower level players are penalised when playing with higher level players
- `3`: Each player gets XP based on their own level (Default behaviour)
- `4`: Each player gets XP based on who killed the mob (Previous version behaviour)

</details>

## Mastery System
<details>
<summary>Mastery System</summary>
> ### Weapon Mastery
Mastering a weapon will now progressively give extra bonuses to the character's stats, all of which are configurable.\
Weapon mastery will increase when the weapon is used to kill a creature, and while in combat to a maximum of 60 seconds. (0.001%/Sec)\
Spell mastery can only increase and take effect when no weapon is equipped. Unless changed in the configuration options. \
New Growth Subsystem. Off by default, but turn it on with efficency, and when you reset a mastery it will be faster to level up, or slower, however you configure it.

> ### Mastery Decay
When the vampire goes to sleep (offline), all their mastery will continuously decay per minute passed while offline.\
This decay will keep on counting even while the server is offline.

> ### Efficency System
Off by default, when a vampire feels ready, they can type .mastery reset all, or .mastery reset sword or any other weapon type, to reset their mastery values to 0, but make their mastery that much more effective in the future. Thus a vampire who reaches 100% mastery in sword, then types .mastery reset sword will be reset back to 0% mastery, but when calculating the bonus from mastery, will now be considered to have twice as much mastery as they currently do, so if they reach 100% mastery, they will get a bonus like they have 200%, if they reset again at this point, it will go up to 300% efficency, thus a mastery of 50% would now be like 150% and 10% would be 30% and so on. The Efficency is specifc to each weapon type, so you could have 1000% efficency with swords, 250% with unarmed and 100% with axes.

> ### Growth System
Off by default, and only works if efficency is also on, but when you reset, the Growth system will change how fast you get mastery in the future based on what you reset at, so if the growth is set to 1, and you reset with 50% mastery, you will now gain new mastery 50% faster, if you were instead to then reset with another 50% mastery, you would now gain mastery twice as fast. The growth is specific for each weapon, same as with efficency. If the config for growth is set to -1, then it will act as a divisior on the amount gained, so if you reset with 100% mastery and growth config at -1, you would gain half as much mastery, if you reset again at 100% mastery, it would be a third, and so on.

</details>

## Bloodline System
<details>
<summary>Bloodline System</summary>
> ### Bloodlines
Killing enemies while you have a blood type will progress the strength of your bloodline.\
As your bloodline grows in strength it will provide scaling benefits to your stats, fully configurably\
By default merciless bloodlines are enabled, which means to progress your bloodlines strength\
You need to have and kill a target with blood of higher quality than your bloodlines strength.\
Vblood always provides progress even in merciless mode\
You can customise the benefits, including the minimum bloodline strength for a benefit, in the config\
You can change the names of the bloodlines in the config as well.\
The default names are based on the vampire characters of some of my long time supporters, and are as below\
<details>
<summary>Bloodline Names</summary>
Dracula the Progenitor, dracula for short, is the frailed bloodline.
<details>
<summary>Lore</summary>
Its Dracula, not from one of my supporters, but the iconic vampire of Bram Stroker. I considered using Carmilla or Varney, who are older vampire stories, but Dracula is the classic.
</details>
Arwen the Godeater, arwen for short, is the creature bloodline.
<details>
<summary>Lore</summary>
Arwen, Third child of Semika, an asendent kitsune, and her lover, Bei, a cultivator who rebelled against the heavens, was born afflicted with vamprism, after her father sought it out to keep his youth alongside his lover, who had recently become ascended to divinity. When the thirf great reset happened, she took to the disapperance of her parents by indulging in her every desire. She quickly grew bored of the now empty city in which she grew up, and headed into the wider world to try and seek more entertainment. In her adventures in this newly changed world, her madness grew, and after breaking a bone once, she insisted on having them replaced with metal, eventually settling on a complex silver alloy, similar to the modern darksilver. The newfound prowress these bones gave her allowed her to maintain a hold on the deity of the archives, as she drained him dry, gaining her the title of godeater.
</details>
Ilvris Dragonblood, ilvris for short, is the warrior bloodline.
<details>
<summary>Lore</summary>
An elf born after the fifth great reset in the fae realm, ilvris was always jealous of dragons, as part of her trials to earn a last name, she hunted down and killed three dragons, bathing in their blood to absorb their powers. Shortly after completing the ceremonies to obtain her name however, she was captured by an elder wyrm, and subjected to a powerful mind control. To her further misfortune, after being returned to her lands, she was captured by the elven overlord of the time, and was pronounced dead, with no heirs. However when Hadubert and his study visited the palace and fought the overlord, they set her free, and together they destroyed the elves, and destroyed the nascent avatar of kivtavash, the deity of hedonism, self-perfection, and freedom. The traveled together for a time, alongside the new bearer of the divine resurrection, and in seeking the power to overthrow the dragons, ilvris requested hadubert turn her into a vampire breaking the mind control. Eventually they hunted down the elder wyrm that mind controlled ilvris, and killed him by crashing his flying city into the citadel of the architect behind the confict in the first place.
</details>
Aya the Shadowlord, aya for short, is the rogue bloodline.
<details>
<summary>Lore</summary>
Aya was a young prodigy of a clan of shadow mages shortly before the first great reset. When her clan was destroyed, her grandfather gave her his life-saving charm, a single use random teleport. Thinking herself saved, she was dismayed to find herself thrown directly into a cage, and then promptly sold to a vampire clan. Fortunately, as she aged, the young master of the vampire clan took a liking to her, and elevated her from servant and meal, to a proper vampire. When Semika, Atrata, and the rest of their adventuring party attacked the city, she took the chance to flee towards her family, to see who may have survived, and give them a proper burial. However, she made it less than half way when she was badly injured by a vampire hunter she encountered, and was then sealed in her coffin, as they lacked the ability to truly slay her. This turned out to be her great fortune, as this allowed her to survive the third great reset, and her coffin was found within a decade by Arwen, who set her free, and invited Aya into her home. Aya then served Rin, Arwens eldest sister, as spymaster, for the next decade, until the fourth great reset occured when magic was slain.
</details>
Nytheria the Destroyer, nytheria for short, is the brute bloodline.
<details>
<summary>Lore</summary>
One of the twin children of Atrata, the fist of the soul, and Grunayrum, Dragonlord of the moons, Nytheria was discarded for being only half dragon, despite their incredible strength. When the third great reset happened, very little changed for them, but as they sought out adventure, their might continuted to grow. They contracted vampirism from Arwen, during their time as lovers, and the further increase to their strength lead to their now accidentally destroying things, such as trying to grab a sword and crushing the hilt instead. Fortunately, they had always favored their natural weapons, but their newfound capacity for destruction lead to their epithet. But their destuction of Mount Xuanyu is what made their name known for the millenia to come.
</details>
Hadubert the Inferno, hadubert for short, is the scholar bloodline.
<details>
<summary>Lore</summary>
Hadubert, Student of the school of warcraft and wizardry, always had a penchant for two things, fire, and magical reasearch. While his incredible proficency in the first lead to his epithet, the second lead him to accompanying the chosen vessel of resurrection, and to creating vampirism in his world after the fifth great reset. After the first vessel burst, and he slew the nascent avatar alongside ilvris, and the vessels lover, he delved deep into magical research, to see if he could prevent the death of the next bearer, and reduce the harm the avatar would do. He succeeded, at a price, his research resulted in him having an eternal desire for blood, but also immense power and durability. Fortunately his research did work, and his efforts ensured the vessel survived the new birth, and they sustained the avatar on the souls of dragons, before crashing the dragons flying city into the citadel of the masterminds behind the process of the divine resurrection, the very school that raised him.
</details>
Rei the Binder, rei for short, is the worker bloodline.
<details>
<summary>Lore</summary>
Fourth Child of Semika and bei, and younger sister of Arwen the Godeater, Rei was always exceptionally kind. Though the vampiric nature she inherited left her constantly craving the blood of the living, she chose, rather than to drink from people, to ensure her meals were always ethically sourced. Her choice, rather than finding consenting people to drink from, was to bind demons as her meals. She took to commanding them as well, using them for a huge variety of tasks, while always convincing them that it was just what they wanted to do in the first place. Eventually, she used her demonic hordes to help Arwen drain the deity of archives, and used them to kill magic itself, allowing the god of change to return, and causing the fourth great reset.
</details>
</details>
The bloodline for frailed blood provides a portion of the benefits of your other bloodlines.\
The command is .bloodline or .bl\


> ### Bloodline Decay
Though the option is currently present, decay is not yet implemented for bloodlines.

> ### Efficency System
On by default, when a vampire feels ready, they can type .bloodline reset <bloodline name>, The bloodline name can be the current names, the default names, or the blood type names, to reset their bloodline strength to 0, but make their bloodline that much more effective in the future. Thus a vampire who reaches 100% Dracula bloodline, then types .bl reset dracula will be reset back to 0% strength, but when calculating the bonus from the bloodline, will now be considered to have twice as much strength as they currently do, for the purposes of the power of the effect only so if they reach 25% strength, they will get a bonus like they have 50%, but not get the bonus unlocked when they hit 50%. if they reset again at this point, it will go up to 250% efficency, thus a bloodline of 50% would now be like 125% and 10% would be 25% and so on. The Efficency is specifc to each bloodline, so you could have 500% efficency with Hadubert's bloodline, 250% with Dracula's and 100% with Ilvris' bloodline.

> ### Growth System
On by default, and only works if efficency is also on, but when you reset, the Growth system will change how fast you get bloodline strength in the future based on what you reset at, so if the growth is set to 1, and you reset with 50% strength, you will now gain new strength 50% faster, if you were instead to then reset with another 50% strength, you would now gain strength twice as fast. The growth is specific for each bloodline, same as with efficency. If the config for growth is set to -1, then it will act as a divisior on the amount gained, so if you reset with 100% strength and growth config at -1, you would gain half as much strength, if you reset again at 100% strength, it would be a third, and so on.

</details>

## HunterHunted System
<details>
<summary>Heat System</summary>
A new system where every NPC you kill contributes to a wanted level system,\
if you kill too many NPCs from that faction, eventually your wanted level will rise higher and higher.\

The higher your wanted level is, a more difficult squad of ambushers will be sent by that faction to kill you.\
Wanted level will eventually cooldown the longer you go without killing NPCs from that faction,\
space your kills so you don't get hunted by an extremely elite group of assassins.\

Another way of lowering your wanted level is to kill Vampire Hunters.

Otherwise, if you are dead for any reason at all, your wanted level will reset back to anonymous.\
```
Note:
- Ambush may only occur when the player is in combat.
- All mobs spawned by this system is assigned to Faction_VampireHunters
```
</details>

## World Dynamics
Each factions in the world will continously gain strength for every in-game day cycle.\
Vampires will need to regularly cull these factions mobs to prevent or weaken the faction.\
For each mobs killed, the faction growth will be hampered, if enough are killed, the faction may even weaken.

Every faction strength gain and stat buff can be manually configured, by the server admin via config & json file.

<details>
<summary>Faction Stats Details</summary>

Use [Gaming.Tools](https://gaming.tools/v-rising) to look up NPCs faction.
```json
//-- DO NOT COPY PASTE - JUST EDIT THE FILE BUILD BY THE AUTOMATICALLY
//-- INFO:
//-- - Dynamic value: can and will change during gameplay.
//-- - Static value: will not change during game play.
//-- - FactionBonus: this section is all static.

"-413163549": {
    "Name": "Faction_Bandits",
    "Active": false,        //-- Set to true to activate this faction
    "Level": 0,             //-- Dynamic value.
    "MaxLevel": 0,          //-- Static value. Faction will never go above this level.
    "MinLevel": 0,          //-- Static value. Faction will never go below this level.
    "ActivePower": 0,       //-- Dynamic value. Current active power that will get exported to stored power.
    "StoredPower": 0,       //-- Dynamic value. Once it reach required power, faction level up. If it reach < 0, faction level down.
    "DailyPower": 0,        //-- Static value. Active power will be set to this for every in-game day cycle.
    "RequiredPower": 0,     //-- Static value. Stored power need to reach this value for faction to level up.
    "FactionBonus": {
        "Level_Int": 0,                             //-- Stats bonus that will be given to the faction mobs. Formula: OriginalValue + (Value * Level)
        "HP_Float": 0,                              //-- Leave at 0 to not give bonus. Negative to debuff when level up, buff when level down. Postitive to buff when level up, debuff when level down.
        "PhysicalPower_Float": 0,
        "PhysicalResistance_Float": 0,              //-- Unit will be invulnerable to physical damage if this reach 1
        "PhysicalCriticalStrikeChance_Float": 0,
        "PhysicalCriticalStrikeDamage_Float": 0,
        "SpellPower_Float": 0,
        "SpellResistance_Float": 0,                 //-- Unit will be invulnerable to spell damage if this reach 1
        "SpellCriticalStrikeChance_Float": 0,
        "SpellCriticalStrikeDamage_Float": 0,
        "DamageVsPlayerVampires_Float": 0,          
        "ResistVsPlayerVampires_Float": 0,          //-- Unit will be invulnerable to player if this reach 1
        "FireResistance_Int": 0
    }
}
```

</details>

<details>
<summary>Ignored Monsters</summary>

Use [Gaming.Tools](https://gaming.tools/v-rising) to look up NPCs GUID.
You can add some monster to the ignored list with their Prefab Name.
```json
[
  "CHAR_Undead_Banshee",
  "CHAR_Cultist_Pyromancer"
]
```

</details>


## Command Permission & VIP Login Whitelist
Commands are configured to require a minimum level of permission for the user to be able to use them.\
When there's no minimum permission set in the command_permission.json, it will default to a minimum requirement of permission lv. 100.

VIP System, when enabled, will enable the user with permission level higher or equal to the minimum requirement set in the config,\
to be able to bypass server capacity.

Permission levels range from 0 to 100.\
With 0 as the default permission for users (lowest),\
and 100 as the highest permission (admin).

## Custom Ban System
You can now ban a player for the specified duration in days using the .ban/.unban command.\
`WARNING` If you remove RPGMods, all the banned users via the command will no longer be banned!

## Localization System
Removed as it was causing issues in some other localities... Isn't programming for a global audience fun?
<details>
<summary>Old Description</summary>
Now allows all text from RPGMods to be customized to your language, a Language.Json file will be generated in the Bepinex/Config/RPGMods subfolder, to provide a translation, where it has something like {"\" not found.", "\" not found."} change it to something like {\" not found.", "\" 見つけありません"} to change the displayed text.
</details>

## Config
<details>
<summary>Basic</summary>

- `WayPoint Limits` [default `3`]\
Set a waypoint limit per user.

</details>

<details>
<summary>VIP</summary>

- `Enable VIP System` [default `false`]\
Enable the VIP System.
- `Enable VIP Whitelist` [default `false`]\
Enable the VIP user to ignore server capacity limit.
- `Minimum VIP Permission` [default `10`]\
The minimum permission level required for the user to be considered as VIP.

<details>
<summary>-- VIP.InCombat Buff</summary>

- `Durability Loss Multiplier` [default `0.5`]\
Multiply durability loss when user is in combat. -1.0 to disable.\
Does not affect durability loss on death.
- `Garlic Resistance Multiplier` [default `-1.0`]\
Multiply garlic resistance when user is in combat. -1.0 to disable.
- `Silver Resistance Multiplier` [default `-1.0`]\
Multiply silver resistance when user is in combat. -1.0 to disable.
- `Move Speed Multiplier` [default `-1.0`]\
Multiply move speed when user is in combat. -1.0 to disable.
- `Resource Yield Multiplier` [default `2.0`]\
Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.

</details>

<details>
<summary>-- VIP.OutCombat Buff</summary>

- `Durability Loss Multiplier` [default `0.5`]\
Multiply durability loss when user is out of combat. -1.0 to disable.\
Does not affect durability loss on death.
- `Garlic Resistance Multiplier` [default `2.0`]\
Multiply garlic resistance when user is out of combat. -1.0 to disable.
- `Silver Resistance Multiplier` [default `2.0`]\
Multiply silver resistance when user is out of combat. -1.0 to disable.
- `Move Speed Multiplier` [default `1.25`]\
Multiply move speed when user is out of combat. -1.0 to disable.
- `Resource Yield Multiplier` [default `2.0`]\
Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.

</details>

</details>


<details>
<summary>HunterHunted</summary>

- `Enable` [default `true`]\
Enable/disable the HunterHunted system.
- `Heat Cooldown Value` [default `35`]\
Set the reduction value for player heat for every cooldown interval.
- `Bandit Heat Cooldown Value` [default `35`]\
Set the reduction value for player heat from the bandits faction for every cooldown interval.
- `Cooldown Interval` [default `60`]\
Set every how many seconds should the cooldown interval trigger.
- `Ambush Interval` [default `300`]\
Set how many seconds player can be ambushed again since last ambush.
- `Ambush Chance` [default `50`]\
Set the percentage that an ambush may occur for every cooldown interval.
- `Ambush Despawn Timer` [default `300`]\
Despawn the ambush squad after this many second if they are still alive. Ex.: -1 -> Never Despawn.

</details>

<details>
<summary>Experience</summary>

- `Enable` [default `true`]\
Enable/disable the Experience system.
- `Max Level` [default `80`]\
Configure the experience system max level..
- `Multiplier` [default `1`]\
Multiply the experience gained by the player.
- `VBlood Multiplier` [default `15`]\
Multiply the experience gained from VBlood kills.
- `EXP Lost / Death` [default `0.10`]\
Percentage of experience the player lost for every death by NPC, no EXP is lost for PvP.
- `Constant` [default `0.2`]\
Increase or decrease the required EXP to level up.\
[EXP Table & Formula](https://bit.ly/3npqdJw)
- `Group Modifier` [default `0.75`]\
Set the modifier for EXP gained for each ally(player) in vicinity.\
Example if you have 2 ally nearby, EXPGained = ((EXPGained * Modifier)*Modifier)
- `Ally Max Distance` [default `50`]\
Set the maximum distance an ally(player) has to be from the player for them to share EXP with the player
- `Group Level Scheme` [default `3`]\
Set the group levelling scheme for allied players. See experience section for scheme options.

</details>

<details>
<summary>Mastery</summary>

- `Enable Weapon Mastery` [default `true`]\
Enable/disable the weapon mastery system.
- `Enable Mastery Decay` [default `true`]\
Enable/disable the decay of weapon mastery when the user is offline.
- `Max Mastery Value` [default `100000`]\
Configure the maximum mastery the user can atain. (100000 is 100%)
- `Mastery Value/Combat Ticks` [default `5`]\
Configure the amount of mastery gained per combat ticks. (5 -> 0.005%)
- `Max Combat Ticks` [default `12`]\
Mastery will no longer increase after this many ticks is reached in combat. (1 tick = 5 seconds)
- `Mastery Multiplier` [default `1`]\
Multiply the gained mastery value by this amount.
- `VBlood Mastery Multiplier` [default `15`]\
Multiply Mastery gained from VBlood kill.
- `Decay Interval` [default `60`]\
Every amount of seconds the user is offline by the configured value will translate as 1 decay tick.
- `Decay Value` [default `1`]\
Mastery will decay by this amount for every decay tick. (1 -> 0.001%)
- `X Stats`
The stat IDs that the mastery of a given weapon should boost, as shown on the table below. the amount of entries here MUST match the amount in the paired X Rates
- `X Rates`
The amount of a stat per mastery percentage, except in the case of CDR where it is the amount of mastery percentage to be 50% cdr

Stat IDs copied from the code.
PhysicalPower = 0,
ResourcePower = 1,
SiegePower = 2,
ResourceYield = 3,
MaxHealth = 4,
MovementSpeed = 5,
CooldownModifier = 7,
PhysicalResistance = 8,
FireResistance = 9,
HolyResistance = 10,
SilverResistance = 11,
SunChargeTime = 12,
EnergyGain = 17,
MaxEnergy = 18,
SunResistance = 19,
GarlicResistance = 20,
Vision = 22,
SpellResistance = 23,
Radial_SpellResistance = 24,
SpellPower = 25,
PassiveHealthRegen = 26,
PhysicalLifeLeech = 27,
SpellLifeLeech = 28,
PhysicalCriticalStrikeChance = 29,
PhysicalCriticalStrikeDamage = 30,
SpellCriticalStrikeChance = 31,
SpellCriticalStrikeDamage = 32,
AttackSpeed = 33,
DamageVsUndeads = 38,
DamageVsHumans = 39,
DamageVsDemons = 40,
DamageVsMechanical = 41,
DamageVsBeasts = 42,
DamageVsCastleObjects = 43,
DamageVsPlayerVampires = 44,
ResistVsUndeads = 45,
ResistVsHumans = 46,
ResistVsDemons = 47,
ResistVsMechanical = 48,
ResistVsBeasts = 49,
ResistVsCastleObjects = 50,
ResistVsPlayerVampires = 51,
DamageVsWood = 52,
DamageVsMineral = 53,
DamageVsVegetation = 54,
DamageVsLightArmor = 55,
DamageVsHeavyArmor = 56,
DamageVsMagic = 57,
ReducedResourceDurabilityLoss = 58,
PrimaryAttackSpeed = 59,
ImmuneToHazards = 60,
PrimaryLifeLeech = 61,
HealthRecovery = 62

</details>


## Chat Commands
Use .help to get a list of all commands available to you, and details on them.
<details>
<summary>kit</summary>

`kit <name>`\
Gives you a previously specified set of items.\
&ensp;&ensp;**Example:** `kit starterset`

<details>
<summary>-- How does kit work?</summary>

&ensp;&ensp;You will get a new config file located in `BepInEx/config/RPGMods/kits.json`
```json
[
  {
    "Name": "Kit1",
    "PrefabGUIDs": {
      "820932258": 50,
      "2106123809": 20
    }
  },
  {
    "Name": "Kit2",
    "PrefabGUIDs": {
      "820932258": 50,
      "2106123809": 20
    }
  }
]
```

</details>

</details>


</details>

## More Information
<details>
<summary>Changelog</summary>

`1.5.0`
- Some bug fixes
- Bloodline system finally added, see above for details

`1.4.2`
- Some bug fixes
- Added the Class system by `SALTYFLEA#3772`
- This version isnt on my patreon first because i didnt make the main changes.

`1.4.1`
- Actually updated the changelog.

`1.4.0`
- assorted bug fixes, like mastery going below 0 from decay, or being able to exceed the cap.
- New Localization overhaul, use the new Language.json file to translate to your language

`1.3.2`
- assorted bug fixes
- Fixed an issue where certain localizations would not read the weapon mastery configs correctly.

`1.2.7a`
- bug fixes

`1.2.7`
- Activated the efficency and growth subsystems for mastery.
- fixed an issue with the dynamic faction system, accidentally only saved when units deleveled.

`1.2.6`
- Made mastery buffs fully configurable.

`1.2.5`
- Added several config options to the mastery system centered around the spell mastery.
- Added config option to invert the dynamic faction system, making factions grow as they are killed and weaken as time passes.

`1.2.4`
- Arguments parse protection for customspawn command.
- Fixed error with spawning horses using customspawn command.
- New initialization method to fix crash with a 100% fresh server with no save.

`1.2.3`
- Added config option to announce all grief kills.
- Added config option to exclude killing of offline player from PvP Punishment.
- Fixed unintended effect that causes vermin nest & tomb to have no spawn limit.

`1.2.2`
- Added anti-cheese system for PvP Punishment without EXP System.
- Added a config to disable the honor title only with benefits, etc still active.
- Found an issue with heatspawn faction not applied, no longer this will be an issue.
- Fixed customspawn command, stupid mistake was made, fixed it was.

`1.2.1`
- Added mob ignore feature for faction buff.
- Added mob ignore command for faction buffs.
- Added power up command.

`1.2.0`
- Added an initial version for world dynamics.
- Added worlddynamics commands.

`1.1.3`
- Hotfix for crash when user is not within a clan.

`1.1.2`
- Bug fix for exception error on trying to get disabled/offline allies location.

`1.1.1`
- Attempt at fixing proximity glow bug where the mod can't decide if they're close or far.
- Clan members are now factored in for honor system siege.
- Bug fix for dreaded player being able to manually turn siege off.

`1.1.0`
- Added duration option for customspawn command.
- Added honor system and a ton of other mechanics it entails.
- Added siege command.
- Added rename & adminrename commands.
- Added playerinfo & myinfo commands to help server admins with some debugging.
- Fixed hunter hunted not spawning anything on low heat level.
- Give command will now refuse to run if no arguments is given.
- SpawnNPC on waypoint now properly accept the spawn counts.
- Implemented allies caching for better performance.
- Bug fix with the exp gain for killing lower level mobs.
- HunterHunted ambush group are now part of vampire hunters faction.

`1.0.2`
- Added customspawn command.
- Added property to compile with wetstone or not.
- Added shutdown command.
- Bugfix for on defeat message.
- Added kits json save/load log message.
- Minor adjustments.

`1.0.1`
- Added optional wetstone dependency for compiling.
- Added compabilities with wetstone reload function.

`1.0.0`
- Removed wetstone dependency.

</details>

<details>
<summary>Developer & Contributors</summary>

### [Discord](https://discord.gg/XY5bNtNm4w)
### Current Developer
- `Dresmyr` - Also known as Shou (like the english word show), Darkon47 on Github.
If you enjoy the work I have put into this mod, subscribe to my patreon at https://www.patreon.com/user/membership?u=92238426

- `Aontas` Redid heat for gloomrot and has done a fair bit of the xp reworks.

### Original Developer
- `Kaltharos#0001`

### Contributors
#### Without these people, this project will just be a dream. (In no particular order)
- `Dimentox#1154`
- `Nopey#1337`
- `syllabicat#0692`
- `errox#7604`
- `SALTYFLEA#3772` Added the class system, currently otherwise undocumented.

</details>

<details>
<summary>Known Issues</summary>

### General


</details>

<details>
<summary>Planned Features</summary>

- More optimization! It never hurts to optimize! (not from me)
- Average reputation of clan members. (Not from me)
- More dynamic events. (Not from me)
- Kits Option: Limited Uses. (On hold)
- Explore team/alliance in VRising. (On hold)
- Need a better name tagging sytem. (On hold)

</details>