TerraJump
===

A simple jump pad plugin for TShock.

## Usage

To use TerraJump you need to build a jump pad like so

![correctly built jump pad](https://user-images.githubusercontent.com/19757593/106363847-53774100-632b-11eb-99ae-d9ecc5d9f153.png)

Build it out of the block you set in the configuration.

The last thing you need, it the `terrajump.use` permission.

Happy jumping!

## Commands

TerraJump adds two commands you can use:
- `/jump` - Shoots you into the air just like a jump pad would.
- `/terrajump` - Command used to configure the plugin in-game.

### TerraJump command usage

`/terrajump [subcommands]`

- `/terrajump toggle` - Used to toggle the entire plugin
- `/terrajump reload` - Used to reload plugin configuration
- `/terrajump edit [subcommands]` - Used to edit jump pad block and force
  - `/terrajump edit tile [tile id]` - Check or set the tile id used to create the jump pads
  - `/terrajump edit force [number]` - Check or set the jump pad force. Must be 10 or higher
- `/terrajump disbale [subcommands]` - Used to disable jump pads
  - `/terrajump disable self` - Used to disable all jump pads only for yourself
  - `/terrajump disable pad` - Used to disable a specific jump pad for yourself
  - `/terrajump disable global` - Used to disable a specific jump pad for everyone

## Permissions

Permission used by the plugin:
- `terrajump.use` - Allows you to use the jump pads and the `/jump` command
- `terrajump` - Allows you to use the `/terrajump` with no subcommands
- `terrajump.disable` - Allows you to use `/terrajump disable self` and `/terrajump disable pad` commands
- `terrajump.admin.toggle` - Allows you to use the `/terrajump toggle` command
- `terrajump.admin.reload` - Allows you to use the `/terrajump reload` command
- `terrajump.admin.edit` - Allows you to use the `/terrajump edit` command alongside all its subcommands
- `terrajump.admin.disable` - Allows you to use the `/terrajump disable global` command
