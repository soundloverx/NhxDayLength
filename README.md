# NhxDayLength

Private mod for our Valheim server, not published on Thunderstore. It sets how long the day and the night last.

## Day and night

The game's day/night cycle lasts 30 minutes: 21 minutes of daylight and 9 of night. NhxDayLength makes them two settings, so you can, for example, have longer days without longer nights.

- Daylight is from sunrise to sunset, and night from sunset to sunrise. The sun and the moon move at whatever speed fits.
- Everything that depends on the time of day follows: monsters that only come out at night, the cold at night, the music, the "Day N" message at sunrise.
- Sleeping still skips to the next morning in 12 seconds, but with the sun crossing the sky at a steady speed, so even a short night takes most of the 12 seconds. As in the game, you can go to bed from the afternoon, the second half of the daylight, so with longer days that's later.
- How long things take doesn't change: growing plants, smelters, kilns, fermenters, cooking, food, Rested and the time between raids. With longer days, more of all that happens in a day. As in the game, sleeping also fast-forwards plants, smelters, kilns, fermenters and cooking, so with longer days, going to bed in the afternoon skips more of them.

## Changing the lengths on a world in progress

The game counts days from the world's clock: the day number is the time since the world was created, divided by the length of a day plus a night. So when that total changes, the day number and the time of day change at once. Going from 30 to 60 minutes on day 300, for example, makes it day 150, at some other time of day, and the sky moves to it over a few seconds.

This only happens once per change. The day number is only used for the "Day N" message, the day written on death markers on the map, and which hint Hugin gives. Changing the day and night so that their total stays the same (24 + 6 instead of 21 + 9) keeps the day number, and only moves the time of day a little.

## Config

`NhxDayLength.cfg`

| Setting | Default | Description |
|----|----|----|
| Day Length | 21 | Minutes of daylight, from sunrise to sunset, 1 to 240. |
| Night Length | 9 | Minutes of night, from sunset to sunrise, 1 to 240. |

The server's settings apply to every player, and only admins can change them. Changes made in the game with the Configuration Manager take effect immediately, and an admin's reach every player when the Configuration Manager is closed. After editing the config file, restart the server.

## Compatibility

- Must be installed on the server and on every player's game, in the same version.
- Requires Jotunn.
- Don't use it with another mod that changes the day length.
- Mods that count in days follow the new length: Jewelcrafting's gem respawn time, and the shipment times shown by More World Locations.
