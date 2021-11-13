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
using VRC;
using Friend_Notes;

[assembly: MelonInfo(typeof(VibratorController), "Vibrator Controller", "1.4.9", "MarkViews", "https://github.com/markviews/VRChatVibratorController")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit", "VRCWSLibary", "VRChatUtilityKit")]

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private static string findButton = null;
        private static bool useActionMenu, lockSpeed = false, pauseControl = false;
        private static int buttonStep;
        private static KeyCode lockButton, holdButton;
        private static GameObject quickMenu;
        private static MelonPreferences_Category vibratorController;
        private static ButtplugClient bpClient;

        public override void OnApplicationStart() {
            vibratorController = MelonPreferences.CreateCategory("VibratorController");

            MelonPreferences.CreateEntry(vibratorController.Identifier, "lockButton", 0, "Button to lock speed");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "holdButton", 0, "Button to hold to use toy");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "ActionMenu", true, "action menu integration");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "buttonStep", 5, "What % to change when pressing button");

            lockButton = (KeyCode)MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "lockButton");
            holdButton = (KeyCode)MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "holdButton");
            useActionMenu = MelonPreferences.GetEntryValue<bool>(vibratorController.Identifier, "ActionMenu");
            buttonStep = MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "buttonStep");

            if (useActionMenu && MelonHandler.Mods.Any(mod => mod.Info.Name == "ActionMenuApi")) {
                try {
                    new ToyActionMenu();
                } catch (Exception) {
                    MelonLogger.Warning("Failed to add action menu button");
                }
            }

            extractDLL();
            VRCWSIntegration.Init();
            MelonCoroutines.Start(UiManagerInitializer());
            CreateButton();
        }

        public IEnumerator UiManagerInitializer() {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;

            quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)");

            NetworkManagerHooks.Initialize();
            NetworkManagerHooks.OnLeave += onPlayerLeft;
        }

        private void CreateButton() {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Get\nToys", () => {
                String name = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_SelectedUser_Local").GetComponent<VRC.UI.Elements.Menus.SelectedUserMenuQM>().field_Private_IUser_0.prop_String_0;
                VRCWSIntegration.connectedTo = name;
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.GetToys));
            });

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Vibrator\nController", () => {
                ShowMenu();
            });
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

        internal static void ShowMenu() {
            ICustomShowableLayoutedMenu menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

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

            menu.AddSpacer();

            foreach (Toy toy in Toy.allToys) {
                string text = toy.name + "\n" + toy.hand;

                if (toy.isLocal()) {
                    text = toy.name + "\n" + toy.hand;

                    if (!toy.isActive)
                        text = toy.name + "\n" + "<color=red>Not Shared</color>";

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
                } else {
                    text = toy.name + "\n" + toy.hand;
                }

                int step = (int)(toy.maxSpeed * ((float)buttonStep / 100));

                menu.AddSimpleButton("-", () => {
                    if (toy.lastSpeed - step >= 0) {
                        toy.setSpeed(toy.lastSpeed - step);
                        menu.Hide();
                        ShowMenu();
                    }
                });
                menu.AddSimpleButton("+", () => {
                    if (toy.lastSpeed + step <= toy.maxSpeed) {
                        toy.setSpeed(toy.lastSpeed + step);
                        menu.Hide();
                        ShowMenu();
                    }
                });
                menu.AddLabel($"{(double)toy.lastSpeed / toy.maxSpeed * 100}%");

                menu.AddSimpleButton(text, () => {
                    toy.changeHand();
                    menu.Hide();
                    ShowMenu();
                });

            }

            menu.Show();
        }

        private static void SetupBP() {
            bpClient = new ButtplugClient("VRCVibratorController");
            bpClient.ConnectAsync(new ButtplugEmbeddedConnectorOptions());
            bpClient.DeviceAdded += (object aObj, DeviceAddedEventArgs args) => {
                new Toy(args.Device);
                bpClient.StopScanningAsync();
            };
            
            bpClient.DeviceRemoved += (object aObj, DeviceRemovedEventArgs args) => {
                if (Toy.myToys.ContainsKey(args.Device.Index))
                {
                    Toy.myToys[args.Device.Index].disable();
                }
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
                if (lockSpeed) return;
                if (menuOpen()) return;

                if (holdButton != KeyCode.None && !pauseControl)
                    if (!Input.GetKey(holdButton)) {
                        toy.setSpeed(0);
                        return;
                    }

                int left = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"));
                int right = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));

                if (pauseControl) {
                    if (left != 0 || right != 0) {
                        //Console.WriteLine(left + " " + right);
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

        //message from server
        internal static void message(VibratorControllerMessage msg, string userID) {

            Toy toy = null;
            if (Toy.myToys.ContainsKey(msg.ToyID))
            {
                toy = Toy.myToys[msg.ToyID];
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

        private static bool menuOpen() {
            if (quickMenu == null) {
                quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)");
                return true;
            }

            if (quickMenu.activeSelf) {
                return true;
            }
                
            return false;
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