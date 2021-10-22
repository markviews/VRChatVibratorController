using Buttplug;
using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using Vibrator_Controller;
using System.Reflection;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(VibratorController), "Vibrator Controller", "1.4.3", "MarkViews", "https://github.com/markviews/VRChatVibratorController")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit")]

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private string findButton = null;
        private bool useActionMenu, lockSpeed = false, pauseControl = false;
        private KeyCode lockButton, holdButton;
        private GameObject quickMenu, menuContent;
        private MelonPreferences_Category vibratorController;
        private ToyActionMenu toyActionMenu;

        private bool scanning = false;
        private ButtplugClient bpClient;

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

            Client.Setup();
            ExpansionKitApi.RegisterWaitConditionBeforeDecorating(CreateButton());
        }

        private void extractDLL() {
            using (Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Vibrator_Controller.buttplug_rs_ffi.dll"))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + @"\buttplug_rs_ffi.dll", FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs)) {
                w.Write(r.ReadBytes((int)s.Length));
            }
        }

        public IEnumerator CreateButton() {
            while (QuickMenu.prop_QuickMenu_0 == null) yield return null;

            quickMenu = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            menuContent = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Vibrator\nController", () => {
                ShowMenu();
            });
        }

        public void ShowMenu() {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

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

            menu.AddSimpleButton("Add Remote\nToy\n(Code)", () => {
                menu.Hide();
                BuiltinUiUtils.ShowInputPopup("Enter Code", "", InputField.InputType.Standard, false, "Confirm", (text, _, __) => {
                    text = text.Trim();
                    if (text.Length == 4) {
                        Client.currentlyConnectedCode = text;
                        Client.reconnectTries = 0;
                        Client.Send("join " + text);
                    }
                });
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
                            break;
                        }
                    }
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
                menu.AddSimpleButton(toy.name + "\n" + toy.hand, () => {
                    toy.changeHand();
                    menu.Hide();
                    ShowMenu();
                });
            }

            foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys) {
                Toy toy = entry.Value;

                if (toy.isActive) {
                    menu.AddSimpleButton(toy.device.Name + "\n(Shared)", () => {
                        toy.disable();
                        menu.Hide();
                        ShowMenu();
                    });
                } else {
                    menu.AddSimpleButton(toy.device.Name + "\n(Not Shared)", () => {
                        toy.enable();
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
                if (menuOpen()) {
                    toy.setSpeed((int)toy.speedSlider.value);

                    if (toy.maxSlider != null)
                        toy.setContraction();
                    if (toy.edgeSlider != null) {
                        if (toy.lastEdgeSpeed != toy.edgeSlider.value)
                            toy.setEdgeSpeed(toy.edgeSlider.value);
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
                        case "left":
                            right = left;
                            break;
                        case "right":
                            left = right;
                            break;
                        case "either":
                            if (left > right) right = left;
                            else left = right;
                            break;
                        case "both":
                            break;
                    }
                    if (toy.name == "Edge") {
                        toy.setEdgeSpeed(right);
                    }
                    toy.setSpeed(left);
                }
            }
        }

        internal bool menuOpen() {
            if (quickMenu.active || menuContent.active)
                return true;
            return false;
        }

        //message from server
        internal static void message(string msg) {
            String[] args = msg.Replace(((char)0).ToString(), "").Split(' ');
            switch (args[0]) {
                
                //remote toy commands
                case "toys"://toys name:id name:id name:id (depreciated)
                case "add"://add name:id name:id name:id
                    if (args[1] == "") {
                        MelonLogger.Error("Connected but no toys found..");
                        return;
                    }
                    for (int i = 1; i < args.Length; i++) {
                        string[] toyData = args[i].Split(':');

                        if (toyData.Length >= 2) {
                            string name = toyData[0];
                            string id = toyData[1];

                            foreach (Toy toy in Toy.toys)
                                if (toy.id.Contains(id)) {
                                    toy.enable();
                                    return;
                                }

                            MelonLogger.Msg("Adding: " + name + ":" + id);
                            new Toy(name, id);
                        }

                    }
                    break;
                case "remove"://remove name:id name:id name:id
                    if (args.Length >= 2) {
                        string[] toyData = args[1].Split(':');
                        string name = toyData[0];
                        string id = toyData[1];
                        foreach (Toy toy in Toy.toys) {
                            if (toy.id.Contains(id)) {
                                toy.disable();//TODO display this somehow
                                break;
                            }
                        }
                        
                    }
                    break;
                case "notFound"://*no args
                    MelonLogger.Error("Invalid code");
                    //addToyText.text = "Add\nToys\n<color=#FF0000>Invalid Code</color>";
                    break;


                //other user disconnect from the server (not this user)
                //user on webpage sharing toy to to this user disconnected or
                //user with mod controlling this user disconnected
                case "left"://*no args
                    MelonLogger.Warning("Control Client disconnected");

                    foreach (Toy toy in Toy.toys) {
                        toy.disable();
                    }

                    foreach (KeyValuePair<string, Toy> entry in Toy.sharedToys) {
                        Toy toy = entry.Value;
                        toy.disable();
                    }
                    break;


                //Local toy commands
                case "speed"://speed id speed edgeSpeed
                    if (args.Length >= 3) {
                        string id = args[1];
                        int speed, edgeSpeed;

                        try {
                            speed = Int32.Parse(args[2]);
                            if (args.Length >= 4)
                                edgeSpeed = Int32.Parse(args[3]);
                        } catch (FormatException) {break;}

                        if (Toy.sharedToys.ContainsKey(id)) {
                            Toy toy = Toy.sharedToys[id];
                            toy.setSpeed(speed);
                        }
                    }
                    break;
                case "air"://air id speed
                    if (args.Length >= 3) {
                        string id = args[1];
                        int speed;

                        try {
                            speed = Int32.Parse(args[2]);
                        } catch (FormatException) { break; }

                        if (Toy.sharedToys.ContainsKey(id)) {
                            Toy toy = Toy.sharedToys[id];
                            toy.setContraction(speed);
                        }
                    }
                    break;
                case "rotate"://rotate id
                    if (args.Length >= 2) {
                        string id = args[1];

                        if (Toy.sharedToys.ContainsKey(id)) {
                            Toy toy = Toy.sharedToys[id];
                            toy.rotate();
                        }
                    }
                    break;
                case "id"://id id
                    if (args.Length >= 2) {
                        string id = args[1];

                    }
                    break;
                case "joined"://*no args
                    MelonLogger.Msg("Control Client connected");
                    break;
            }
        }

        internal void getButton() {
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

        internal void setButton(KeyCode button) {
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