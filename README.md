# VRChat Lovense Remote
Lets you control your friends lovense using your vr controllers.
<br>Mod menu can be found under `UI Elements` by default, read below for instructions to change it's location.

### How to add toy to mod
1. Copy control link code that your friend sent to you to your clipboard (the whole link, like https://c.lovense.com/c/01abc2)
2. Click Add Toy in the in game menu

### How to get control link code
0. Person that has the toy should follow these steps
1. Download / open the Lovense Remote app (the blue one)
2. Pair device
3. Click `Long Distance`
4. Click `+`
5. Click `Control Link` (the link will be copied to clipboard)
6. Send the link to your friend with the mod (DO NOT open the link in a web browser, it will take control and the mod won't be able to)

### Mod menu location
You can change the menu location by changing `buttonX` and `buttonY` to desired coordinates and `subMenu` for in `modprefs.ini` to:
`ShortcutMenu` (Main menu), `UserInteractMenu` (Menu when you select someone), `UserIconCameraMenu` (VRC+ menu to take pictures), `EmoteMenu`, `EmojiMenu`, `CameraMenu`, `UIElementsMenu`, or `AvatarStatsMenu`

### Customizable controls setup
1. Click `Hold Button` or `Lock Speed Button` in the mod menu
2. Press a button on your keyboard or controller
* Hold Button:  If set to `Hold On` you will need to hold the button you set along with the trigger to use toy
* Lock Speed Button: Press the button you set to keep the current vibration speed until you press it or the set controller trigger again.

#### Planned Features
* Stroke control: speed based on how fast you stroke your controllers (with customizable thresholds)
* Slider control: speed based on slider

#### Credits
DubyaDude for [RubyButtonAPI](https://github.com/DubyaDude/RubyButtonAPI)
