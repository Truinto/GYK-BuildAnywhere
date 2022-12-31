# Changelog

## [1.0.4]
- update

## [1.0.3]
- fixed hotkeys not working
- removed BuildModeExpandView, which didn't work
- fixed boolean settings are now correctly read

## [1.0.1]
- added extend build mode camera bounds (BuildModeExpandView)
- added hotkey to teleport to mouse cursor (default: G)
- added new recipe 100 firewood at once (PGB_global AND PGB_MoreZombieRecipies)
- added option to build anything at any blueprint desk (default off)
- added setting to change Teleportstone hotkey (default: H)

## [1.0.0]
- Ignore building zone (default key: C)
- Ignore building overlap (default key: V)
- Fishing minigame always succeeds (FishingAlwaysSuccess)
- No delay before fish bite (FishAlwaysBiting)
- No input needed to reel in fish (PullAutomatically)
- Fishing lures get set to full durability after use (NoDurabiliyLoss)
- All items set to full durability after use (NoItemDurability)
- Autopsy doesn't ask for confirmation (AutopsyNoConfirm)
(section below needs PGB_global AND PGB_MoreZombieRecipies)
- Zombie Workbench: added Churchbench recipies
- Random text generator: added Writing desk II recipies
- Preparation bench has zero time extraction/insertion
- using fuel in furnaces generates ash
- marble sculptor is slightly easier to craft
- stone workplace new recipies stone to sand, and marble sculptor for each quality
- gold nuggest have the same drop rate as silver
- peat can be set enqueued (click infinite before closing the window)
- candle III will last indefinitely in candle holder III (doesn't affect the others)
- saw can process firewood and splinters
- possible fix for 'plant' sermon buff
- furnace II burn wood to ash
- possible fix for 'farmer' perk
- new Zombie Brewery recipies for Mead
- remove cooldown from teleport rune
- split tavern recipies into separate ones, which support enqueued work
- more eggs (basket holds more eggs)
- more water (well pump pumps more)
- merchant will buy gold ingots
- priest will buy/sell faith
- can grow cannabis in zombie farm (visually wheat)
- linked kitchen buildings with the main yard and garden, mainly for the well pump (PGB_global AND PGB_LinkStorageWellPump)
- Furnaces and similiar automatic processing buildings drop into storages, like zombies do (AutoDropIntoStorage)
- Zombies drop tech orbs (AutoDropIntoStorage AND ZombieDropTechOrbs)
- Player crafts drop into storages, except if the output doesn't stack (AutoDropIntoStorage AND AutoDropIntoStoragePlayerStacks), experimental, default off
- All crafts drop into storages (AutoDropIntoStorage AND AutoDropIntoStoragePlayerAll), experimental, default off
- note: If you have no storage slots left, these options might delete the item. I did code it so it would drop it, when no storage is found, but in testing this did not always work.
- Better corpses while you have the Prayer for repose buff (CorpseBuff)
- Buff timer never run out (InfiniteBuffs)
- hotkey H opens teleport menu (like hearthstone does), tends to crash when used right after loading a save