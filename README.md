# VRChat Vibrator Controller

### Mod almost ready for use.

Lets you control your friends lovense using your vr controllers.
<br>Mod menu can be found under `UI Elements` by default, read below for instructions to change it's location.

## How to use
1. Ask friend to follow instructions below to get a code
2. Copy the code from your friend to your clipboard
3. Click `Add Toy` in the mod menu

## How to get a code
* Friend that has toy should follow these instructions
1. Connect your phone to the same wifi as your computer
2. Connect your toy to the Lovense Connect app (the pink one)
3. Open the [mod webpage](https://remote.markstuff.net/)
4. Click `Search for toys`

## Mod menu location
You can change the menu location by changing `buttonX` and `buttonY` to desired coordinates and `subMenu` in `modprefs.ini` to:
`ShortcutMenu` (Main menu), `UserInteractMenu` (Menu when you select someone), `UserIconCameraMenu` (VRC+ menu to take pictures), `EmoteMenu`, `EmojiMenu`, `CameraMenu`, `UIElementsMenu`, or `AvatarStatsMenu`

## Customizable controls setup
1. Click `Hold Button` or `Lock Speed Button` in the mod menu
2. Press a button on your keyboard or controller
* Hold Button:  If set to `Hold On` you will need to hold the button you set along with the trigger to use toy
* Lock Speed Button: Press the button you set to keep the current vibration speed until you press it again

## Planned Features
* Stroke control: speed based on how fast you stroke your controllers (with customizable thresholds)
* Input textbox instead of scuffed clipboard input
* If friend with toy has this mod they won't have to open the webpage
* More than one connection per mod user (You can currently only connect to one person at a time)

This mod is not developed by or associated with Lovense or Hytto.

## Credits
DubyaDude for [RubyButtonAPI](https://github.com/DubyaDude/RubyButtonAPI)
<br>abbeybabbey for UI improvments
