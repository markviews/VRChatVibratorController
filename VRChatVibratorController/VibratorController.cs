using Buttplug;
using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UIExpansionKit.API;
using UnityEngine;
using Vibrator_Controller;
using System.Reflection;
using System.Collections.Generic;
using VRChatUtilityKit.Utilities;
using VRC;
using System.Threading.Tasks;

[assembly: MelonInfo(typeof(VibratorController), "Vibrator Controller", "1.4.5", "MarkViews", "https://github.com/markviews/VRChatVibratorController")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit", "VRCWSLibary", "VRChatUtilityKit")]

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private static string findButton = null;
        private static bool useActionMenu, lockSpeed = false, pauseControl = false;
        private static KeyCode lockButton, holdButton;
        private static GameObject quickMenu, menuContent;
        private static MelonPreferences_Category vibratorController;

        private static ButtplugClient bpClient;
        internal static ICustomShowableLayoutedMenu menu;

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
                    new ToyActionMenu();
                } catch (Exception) {
                    MelonLogger.Warning("Failed to add action menu button");
                }
            }

            extractDLL();

            VRCWSIntegration.Init();
            ExpansionKitApi.RegisterWaitConditionBeforeDecorating(CreateButton());
            NetworkEvents.OnPlayerLeft += onPlayerLeft;
        }

        private void onPlayerLeft(Player obj) {
            if (obj.prop_String_0 == VRCWSIntegration.connectedTo)
                foreach (Toy toy in Toy.remoteToys.Select(x=>x.Value)) {
                    toy.disable();
                }
        }

        private void extractDLL() {
            try {
                using (Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Vibrator_Controller.buttplug_rs_ffi.dll"))
                using (BinaryReader r = new BinaryReader(s))
                using (FileStream fs = new FileStream(Environment.CurrentDirectory + @"\buttplug_rs_ffi.dll", FileMode.OpenOrCreate))
                using (BinaryWriter w = new BinaryWriter(fs))
                    w.Write(r.ReadBytes((int)s.Length));
            } catch (Exception) {
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

        internal static void ShowMenu() {
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
                SetupBP();
                return;
            }

            if (bpClient.IsScanning) {
                menu.AddSimpleButton("Scanning..\nPress To\nStop", () => {
                    MelonLogger.Msg("Done Scanning.");
                    bpClient.StopScanningAsync();
                    menu.Hide();
                    ShowMenu();
                });
            } else {
                menu.AddSimpleButton("Scan for\nLocal Toys\n(Bluetooth)", () => {
                    MelonLogger.Msg("Scanning for toys..");
                    bpClient.StartScanningAsync();
                    menu.Hide();
                    ShowMenu();
                });
            }

            foreach (Toy toy in Toy.allToys) {

                if (toy.isLocal()) {

                    string text = toy.name + "\n" + toy.hand;

                    if (!toy.isActive) {
                        text = toy.name + "\n" + "<color=red>Not Shared</color>";
                    }

                    if (toy.supportsBatteryLVL && toy.battery != -1) {
                        text += "\n" + (toy.battery * 100) + "%";

                        toy.device.SendBatteryLevelCmd().ContinueWith(battery => {
                            if (toy.battery != battery.Result) {
                                toy.battery = battery.Result;
                                menu.Hide();
                                ShowMenu();
                            }
                        });
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

        private static void SetupBP() {
            bpClient = new ButtplugClient("VRCVibratorController");
            bpClient.ConnectAsync(new ButtplugEmbeddedConnectorOptions());
            bpClient.DeviceAdded += (object aObj, DeviceAddedEventArgs args) => {
                new Toy(args.Device);
                bpClient.StopScanningAsync();
                new Task(async () =>
                {
                    await AsyncUtils.YieldToMainThread();
                    menu.Hide();
                    ShowMenu();
                });
            };
            
            bpClient.DeviceRemoved += (object aObj, DeviceRemovedEventArgs args) => {
                if (Toy.myToys.ContainsKey(args.Device.Index))
                {
                    Toy.myToys[args.Device.Index].disable();
                }

                new Task(async () =>
                {
                    await AsyncUtils.YieldToMainThread();
                    menu.Hide();
                    ShowMenu();
                });
            };
        }

        public override void OnUpdate() {
            if (findButton != null) getButton();

            if (Input.GetKeyDown(lockButton)) {
                if (lockSpeed) lockSpeed = false;
                else lockSpeed = true;
            }

            foreach (Toy toy in Toy.allToys) {
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
                    int left = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"));
                    int right = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));

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
                    if (toy.supportsTwoVibrators) {
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

            Toy toy = null;
            if (Toy.remoteToys.ContainsKey(msg.ToyID))
            {
                toy = Toy.remoteToys[msg.ToyID];
            }

            switch (msg.Command) {

                //remote toy commands
                case Commands.AddToy:
                    
                    if (msg.ToyID == ulong.MaxValue) {
                        MelonLogger.Error("Connected but no toys found..");
                        return;
                    }

                    MelonLogger.Msg($"Adding : {msg.ToyName} : {msg.ToyID}");
                    new Toy(msg.ToyName, msg.ToyID, msg.ToyMaxSpeed, msg.ToyMaxSpeed2, msg.ToyMaxLinear, msg.ToySupportsRotate);

                    break;
                case Commands.RemoveToy:
                    toy?.disable();

                    break;

                //Local toy commands
                case Commands.SetSpeed:
                    if(toy?.hand == Hand.shared)
                        toy?.setSpeed(msg.Strength);
                    
                    break;
                case Commands.SetSpeedEdge:
                    if (toy?.hand == Hand.shared)
                        toy?.setEdgeSpeed(msg.Strength);
                    
                    break;
                case Commands.SetAir:
                    if (toy?.hand == Hand.shared)
                        toy?.setContraction(msg.Strength);

                    break;
                case Commands.SetRotate:
                    if (toy?.hand == Hand.shared)
                        toy?.rotate();
                    
                    break;
                case Commands.GetToys:
                    MelonLogger.Msg("Control Client connected");
                    //maybe check
                    foreach (KeyValuePair<ulong, Toy> entry in Toy.myToys) {
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