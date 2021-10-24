using Buttplug;
using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using Vibrator_Controller;
using System.Reflection;
using System.Collections.Generic;
using VRChatUtilityKit.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine.Events;
using VRC;

[assembly: MelonInfo(typeof(VibratorController), "Vibrator Controller", "1.4.4", "MarkViews", "https://github.com/markviews/VRChatVibratorController")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit", "VRCWSLibary", "VRChatUtilityKit")]

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private static string findButton = null;
        private static bool useActionMenu, lockSpeed = false, pauseControl = false;
        private static KeyCode lockButton, holdButton;
        private static GameObject quickMenu, menuContent;
        private static MelonPreferences_Category vibratorController;
        private static ToyActionMenu toyActionMenu;

        private static bool scanning = false;
        private static ButtplugClient bpClient;
        private static ICustomShowableLayoutedMenu menu;

        public override void OnApplicationStart() {
            vibratorController = MelonPreferences.CreateCategory("VibratorController");

            MelonPreferences.CreateEntry(vibratorController.Identifier, "lockButton", 0, "Button to lock speed");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "holdButton", 0, "Button to hold to use toy");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "ActionMenu", true, "action menu integration");

            lockButton = (KeyCode)MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "lockButton");
            holdButton = (KeyCode)MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "holdButton");
            useActionMenu = MelonPreferences.GetEntryValue<bool>(vibratorController.Identifier, "ActionMenu");

            if (useActionMenu && MelonHandler.Mods.Any(mod => mod.Info.Name == "ActionMenuApi")) {
                try {
                    toyActionMenu = new ToyActionMenu();
                } catch (Exception) {
                    MelonLogger.Warning("Failed to add action menu button");
                }
            }

            extractDLL();

            VRCWSIntegration.Init();
            ExpansionKitApi.RegisterWaitConditionBeforeDecorating(CreateButton());
            VRCUtils.OnUiManagerInit += init;
            NetworkEvents.OnPlayerLeft += onPlayerLeft;
        }

        private void onPlayerLeft(Player obj)
        {
            if(obj.prop_String_0 == VRCWSIntegration.connectedTo)
            foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys)
            {
                Toy toy = entry.Value;
                toy.disable();
            }
        }

        private void init()
        {
            var baseUIElement = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo/Buttons/RightSideButtons/RightUpperButtonColumn/PlaylistsButton").gameObject;

            var gameObject = GameObject.Instantiate(baseUIElement, baseUIElement.transform.parent, true);
            gameObject.name = "Get Toys";

            var uitext = gameObject.GetComponentInChildren<Text>();
            uitext.text = "Get Toys";

            var button = gameObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            var action = new Action(delegate ()
            {
                MelonLogger.Msg($"Connecting to user");
                VRCWSIntegration.connectedTo = VRCUtils.ActiveUserInUserInfoMenu.id;
                foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys)
                {
                    Toy toy = entry.Value;
                    toy.disable();
                }
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.GetToys));
            });
            button.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(action));
        }

        private void extractDLL()
        {
            try
            {
                using (Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Vibrator_Controller.buttplug_rs_ffi.dll"))
                using (BinaryReader r = new BinaryReader(s))
                using (FileStream fs = new FileStream(Environment.CurrentDirectory + @"\buttplug_rs_ffi.dll", FileMode.OpenOrCreate))
                using (BinaryWriter w = new BinaryWriter(fs))
                    w.Write(r.ReadBytes((int)s.Length));
            }
            catch (Exception)
            {
                MelonLogger.Msg("Couldnt extract buttplug_rs_ffi. Maybe a second process is already using that file");
            }
        }

        private IEnumerator CreateButton() {
            while (QuickMenu.prop_QuickMenu_0 == null) yield return null;

            quickMenu = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            menuContent = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop");


            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Get\nToys", () => {
                VRCWSIntegration.connectedTo = VRCUtils.ActivePlayerInQuickMenu.prop_String_0;
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.GetToys));
            });

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Vibrator\nController", () => {
                ShowMenu();
            });
        }

        private static void ShowMenu() {
            menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            menu.AddSimpleButton(findButton == "lockButton" ? "Press Now" : "Lock Speed\nButton\n" + lockButton.ToString(), () => {
                if (findButton == "lockButton") {
                    lockButton = KeyCode.None;
                    findButton = null;
                    MelonPreferences.SetEntryValue(vibratorController.Identifier, "lockButton", lockButton.GetHashCode());
                } else {
                    findButton = "lockButton";
                }
                menu.Hide();
                ShowMenu();
            });

            menu.AddSimpleButton(findButton == "holdButton" ? "Press Now" : "Hold\nButton\n" + holdButton.ToString(), () => {
                if (findButton == "holdButton") {
                    holdButton = KeyCode.None;
                    findButton = null;
                    MelonPreferences.SetEntryValue(vibratorController.Identifier, "holdButton", holdButton.GetHashCode());
                } else {
                    findButton = "holdButton";
                }
                menu.Hide();
                ShowMenu();
            });

            if (bpClient == null) {
                bpClient = new ButtplugClient("VRCVibratorController");
                bpClient.ConnectAsync(new ButtplugEmbeddedConnectorOptions()).ContinueWith((t) => {
                    menu.Hide();
                    ShowMenu();
                });
                bpClient.DeviceAdded += (object aObj, DeviceAddedEventArgs args) => { new Toy(args.Device); bpClient.StopScanningAsync(); scanning = false; };
                bpClient.DeviceRemoved += (object aObj, DeviceRemovedEventArgs args) => { 
                    foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys) {
                        Toy toy = entry.Value;

                        if (toy.device == args.Device) {
                            Toy.sharedToys.Remove(toy.id);

                            if (Toy.toys.Contains(toy)) 
                                Toy.toys.Remove(toy);

                            break;
                        }
                    }
                    ShowMenu();
                };
                return;
            }

            if (scanning) {
                menu.AddSimpleButton("Scanning..\nPress To\nStop", () => {
                    MelonLogger.Msg("Done Scanning.");
                    scanning = false;
                    bpClient.StopScanningAsync();
                    menu.Hide();
                    ShowMenu();
                });
            } else {
                menu.AddSimpleButton("Scan for\nLocal Toys\n(Bluetooth)", () => {
                    MelonLogger.Msg("Scanning for toys..");
                    scanning = true;
                    bpClient.StartScanningAsync();
                    menu.Hide();
                    ShowMenu();
                });
            }

            //if (bpClient != null && bpClient.Devices != null && bpClient.Devices.Length > 0)
            //    Console.WriteLine("Devices: " + bpClient.Devices.Length);

            foreach (Toy toy in Toy.toys) {

                if (toy.isLocal()) {
                    string text = toy.name + "\n" + toy.hand;

                    if (toy.hand == Hand.shared) {
                        text = toy.name + "\nShared";
                    }

                    menu.AddSimpleButton(text, () => {
                        toy.changeHand();
                        menu.Hide();
                        ShowMenu();
                    });
                } else {
                    menu.AddSimpleButton(toy.name + "\n" + toy.hand, () => {
                        toy.changeHand();
                        menu.Hide();
                        ShowMenu();
                    });
                }

            }

            menu.Show();
        }

        public override void OnUpdate() {
            if (findButton != null) getButton();

            if (Input.GetKeyDown(lockButton)) {
                if (lockSpeed) lockSpeed = false;
                else lockSpeed = true;
            }

            foreach (Toy toy in Toy.toys) {
                if (toy.hand == Hand.shared) return;
                if (menuOpen()) {
                    toy.setSpeed((int)toy.speedSlider.value);

                    if (toy.maxSlider != null)
                        toy.setContraction();
                    if (toy.edgeSlider != null) {
                        if (toy.lastEdgeSpeed != toy.edgeSlider.value)
                            toy.setEdgeSpeed((int)toy.edgeSlider.value);
                    }
                    pauseControl = true;
                } else {
                    if (lockSpeed) return;
                    if (holdButton != KeyCode.None && !pauseControl)
                        if (!Input.GetKey(holdButton)) {
                            toy.setSpeed(0);
                            return;
                        }
                    int left = (int)(20 * Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"));
                    int right = (int)(20 * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));

                    if (pauseControl) {
                        if (left != 0 || right != 0) {
                            Console.WriteLine(left + " " + right);
                            pauseControl = false;
                        } else return;
                    }

                    switch (toy.hand) {
                        case Hand.left:
                            right = left;
                            break;
                        case Hand.right:
                            left = right;
                            break;
                        case Hand.either:
                            if (left > right) right = left;
                            else left = right;
                            break;
                        case Hand.both:
                            break;
                    }
                    if (toy.name == "Edge") {
                        toy.setEdgeSpeed(right);
                    }
                    toy.setSpeed(left);
                }
            }
        }

        private static bool menuOpen() {
            if (quickMenu.active || menuContent.active)
                return true;
            return false;
        }

        //message from server
        internal static void message(VibratorControllerMessage msg, string userID) {


            switch (msg.Command) {
                
                //remote toy commands
                case Commands.AddToy:

                    if (msg.ToyID == "") {
                        MelonLogger.Error("Connected but no toys found..");
                        return;
                    }
                    
                    foreach (Toy toy in Toy.toys)
                        if (toy.id.Contains(msg.ToyID)) {
                            toy.enable();
                            return;
                        }

                    MelonLogger.Msg($"Adding : {msg.ToyName} : {msg.ToyID}");
                    new Toy(msg.ToyName, msg.ToyID);
                       
                    break;
                case Commands.RemoveToy:
                    
                    foreach (Toy toy in Toy.toys) {
                        if (toy.id.Contains(msg.ToyID)) {
                            toy.disable();//TODO display this somehow
                            break;
                        }
                    }
                    break;

                //Local toy commands
                case Commands.SetSpeed:
                    if (Toy.sharedToys.ContainsKey(msg.ToyID))
                    {
                        Toy toy = Toy.sharedToys[msg.ToyID];
                        toy.setSpeed(msg.Strength);
                    }
                    break;
                case Commands.SetSpeedEdge:
                    if (Toy.sharedToys.ContainsKey(msg.ToyID))
                    {
                        Toy toy = Toy.sharedToys[msg.ToyID];
                        toy.setEdgeSpeed(msg.Strength);
                    }
                    break;
                case Commands.SetAir:
                    if (Toy.sharedToys.ContainsKey(msg.ToyID)) {
                        Toy toy = Toy.sharedToys[msg.ToyID];
                        toy.setContraction(msg.Strength);
                        
                    }
                    break;
                case Commands.SetRotate:
                    if (Toy.sharedToys.ContainsKey(msg.ToyID)) {
                        Toy toy = Toy.sharedToys[msg.ToyID];
                        toy.rotate();
                    }
                    break;
                case Commands.GetToys:
                    MelonLogger.Msg("Control Client connected");
                    //maybe check
                    foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys) {
                        VRCWSIntegration.connectedTo = userID;
                        VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.AddToy, entry.Value));
                    }
                    
                    break;
            }
        }

        private void getButton() {
            //A-Z
            for (int i = 97; i <= 122; i++)
                if (Input.GetKey((KeyCode)i)) {
                    setButton((KeyCode)i);
                    return;
                }

            //left vr controller buttons
            if (Input.GetKey(KeyCode.JoystickButton0)) setButton(KeyCode.JoystickButton0);
            else if (Input.GetKey(KeyCode.JoystickButton1)) setButton(KeyCode.JoystickButton1);
            else if (Input.GetKey(KeyCode.JoystickButton2)) setButton(KeyCode.JoystickButton2);
            else if (Input.GetKey(KeyCode.JoystickButton3)) setButton(KeyCode.JoystickButton3);
            else if (Input.GetKey(KeyCode.JoystickButton8)) setButton(KeyCode.JoystickButton8);
            else if (Input.GetKey(KeyCode.JoystickButton9)) setButton(KeyCode.JoystickButton9);

            //right vr controller buttons
            else if (Input.GetKey(KeyCode.Joystick1Button0)) setButton(KeyCode.Joystick1Button0);
            else if (Input.GetKey(KeyCode.Joystick1Button1)) setButton(KeyCode.Joystick1Button1);
            else if (Input.GetKey(KeyCode.Joystick1Button2)) setButton(KeyCode.Joystick1Button2);
            else if (Input.GetKey(KeyCode.Joystick1Button3)) setButton(KeyCode.Joystick1Button3);
            else if (Input.GetKey(KeyCode.Joystick1Button8)) setButton(KeyCode.Joystick1Button8);
            else if (Input.GetKey(KeyCode.Joystick1Button9)) setButton(KeyCode.Joystick1Button9);
        }

        private void setButton(KeyCode button) {
            if (findButton.Equals("lockButton")) {
                lockButton = button;
                MelonPreferences.SetEntryValue(vibratorController.Identifier, "lockButton", button.GetHashCode());
            } else if (findButton.Equals("holdButton")) {
                holdButton = button;
                MelonPreferences.SetEntryValue(vibratorController.Identifier, "holdButton", button.GetHashCode());
            }
            findButton = null;
        }
    }
}