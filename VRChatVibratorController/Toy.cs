using MelonLoader;
using PlagueButtonAPI;
using System.Collections;
using UnityEngine;
using Buttplug;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Vibrator_Controller {
    public enum Hand {
        none, shared, left, right, both, either, slider
    }
    public class Toy {
        internal static Dictionary<ulong, Toy> remoteToys { get; set; } = new Dictionary<ulong, Toy>();
        internal static Dictionary<ulong, Toy> myToys { get; set; } = new Dictionary<ulong, Toy>();

        internal static List<Toy> allToys => remoteToys.Select(x=>x.Value).Union(myToys.Select(x => x.Value)).ToList();

        internal Hand hand = Hand.none;
        internal string name;
        internal ulong id;
        internal bool isActive = true;

        internal UnityEngine.UI.Slider speedSlider;//slider for vibrator speed
        internal UnityEngine.UI.Text speedSliderText;
        internal UnityEngine.UI.Slider maxSlider;//slider for max's contractions
        internal UnityEngine.UI.Text maxSliderText;
        internal UnityEngine.UI.Slider edgeSlider;//slider for edge's 2nd speed
        internal UnityEngine.UI.Text edgeSliderText;
        internal ButtonAPI.PlagueButton rotateButton;
        internal ButtplugClientDevice device;
        internal int lastSpeed = 0, lastEdgeSpeed = 0, lastContraction = 0;

        /* 
         * TODO
         * support for toys with rotate, 2 vibrators, linear functions. (just need to set variables below)
         */

        internal bool supportsRotate = false, supportsLinear = false, supportsTwoVibrators = false, supportsBatteryLVL = false;
        internal int maxSpeed = 20, maxSpeed2 = -1, maxLinear = -1;
        internal double battery = -1;

        internal Toy(ButtplugClientDevice device) {
            id = device.Index;
            hand = Hand.shared;
            name = device.Name;

            //remove company name
            if (name.Split(' ').Length > 1) name = name.Split(' ')[1];

            if (myToys.ContainsKey(id))
            {
                MelonLogger.Msg("Device reconnected: " + name + " [" + id + "]");
                myToys[id].name = name; //id should be uniquie but just to be sure
                myToys[id].enable();
                return;
            }


            this.device = device;

            MelonLogger.Msg("Device connected: " + name + " [" + id + "]");

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.LinearCmd))
                supportsLinear = true;

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.RotateCmd))
                supportsRotate = true;

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.BatteryLevelCmd)) {
                supportsBatteryLVL = true;
                device.SendBatteryLevelCmd().ContinueWith(battery => { 
                    this.battery = battery.Result;
                    VibratorController.menu.Hide();
                    VibratorController.ShowMenu();
                });
            }
                
            //prints info about the device
            foreach (KeyValuePair<ServerMessage.Types.MessageAttributeType, ButtplugMessageAttributes> entry in device.AllowedMessages)
                MelonLogger.Msg("[" + id + "] Allowed Message: " + entry.Key);

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.VibrateCmd)) {
                ButtplugMessageAttributes attributes = device.AllowedMessages[ServerMessage.Types.MessageAttributeType.VibrateCmd];

                if (attributes.ActuatorType != null && attributes.ActuatorType.Length > 0)
                    MelonLogger.Msg("[" + id +  "] ActuatorType " + string.Join(", ", attributes.ActuatorType));

                if (attributes.StepCount != null && attributes.StepCount.Length > 0) {
                    MelonLogger.Msg("[" + id + "] StepCount " + string.Join(", ", attributes.StepCount));
                    maxSpeed = (int)attributes.StepCount[0];
                }
                    
                if (attributes.Endpoints != null && attributes.Endpoints.Length > 0)
                    MelonLogger.Msg("[" + id + "] Endpoints " + string.Join(", ", attributes.Endpoints));

                if (attributes.MaxDuration != null && attributes.MaxDuration.Length > 0)
                    MelonLogger.Msg("[" + id + "] MaxDuration " + string.Join(", ", attributes.MaxDuration));

                if (attributes.Patterns != null && attributes.Patterns.Length > 0)
                    foreach (string[] pattern in attributes.Patterns)
                        MelonLogger.Msg("[" + id + "] Pattern " + string.Join(", ", pattern));
            }

            CreateSlider();


            myToys.Add(id, this);
        }

        internal Toy(string name, ulong id, int maxSpeed, int maxSpeed2, int maxLinear, bool supportsRotate) {
            

            if (remoteToys.ContainsKey(id))
            {
                MelonLogger.Msg("Device reconnected: " + name + " [" + id + "]");
                if (maxSpeed2 != -1) myToys[id].supportsTwoVibrators = true;
                if (maxLinear != -1) myToys[id].supportsLinear = true;
                myToys[id].name = name;
                myToys[id].supportsRotate = supportsRotate;
                myToys[id].maxSpeed = maxSpeed;
                myToys[id].maxSpeed2 = maxSpeed2;
                myToys[id].maxLinear = maxLinear;
                myToys[id].enable();
                MelonLogger.Msg($"Reconnected toy Name: {name}, ID: {id} Max Speed: {maxSpeed}" + (supportsTwoVibrators ? $", Max Speed 2: {maxSpeed2}" : "") + (supportsLinear ? $", Max Linear Speed: {maxLinear}" : "") + (supportsRotate ? $", Supports Rotation" : ""));
                return;
            }

            if (maxSpeed2 != -1) supportsTwoVibrators = true;
            if (maxLinear != -1) supportsLinear = true;

            this.supportsRotate = supportsRotate;
            this.maxSpeed = maxSpeed;
            this.maxSpeed2 = maxSpeed2;
            this.maxLinear = maxLinear;
            this.name = name;
            this.id = id;
            
            MelonLogger.Msg($"Added toy Name: {name}, ID: {id} Max Speed: {maxSpeed}" + (supportsTwoVibrators ? $", Max Speed 2: {maxSpeed2}" : "") + (supportsLinear ? $", Max Linear Speed: {maxLinear}" : "") + (supportsRotate ? $", Supports Rotation" : ""));

            remoteToys.Add(id, this);
            CreateSlider();
        }

        private void CreateSlider() {
            GameObject slider = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/User Volume/VolumeSlider");
            GameObject quickmenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");

            GameObject speedSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
            speedSlider = speedSliderObject.GetComponent<UnityEngine.UI.Slider>();
            speedSlider.maxValue = maxSpeed;
            speedSlider.wholeNumbers = true;
            speedSlider.value = 0;
            speedSliderText = speedSlider.transform.Find("Fill Area/VolumeNumberText").GetComponent<UnityEngine.UI.Text>();
            speedSliderText.text = name + " Speed: 0%";
            speedSliderObject.SetActive(false);

            if (supportsLinear) {
                GameObject maxSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
                maxSliderObject.transform.localScale = new Vector3(0.7f, 1, 1);
                maxSlider = maxSliderObject.GetComponent<UnityEngine.UI.Slider>();
                maxSlider.maxValue = maxLinear;
                maxSlider.wholeNumbers = true;
                maxSlider.value = 0;
                Transform textTransform = maxSlider.transform.Find("Fill Area/VolumeNumberText");
                textTransform.localScale = new Vector3(1, 1, 1);
                maxSliderText = textTransform.GetComponent<UnityEngine.UI.Text>();
                maxSliderText.text = "Contraction: 0";
                maxSliderObject.SetActive(false);
            }

            if (supportsRotate) {
                rotateButton = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Rotate", "Rotate", ButtonAPI.HorizontalPosition.LeftOfMenu, ButtonAPI.VerticalPosition.BelowBottomButton, ButtonAPI.ShortcutMenuTransform, delegate (bool a) {
                    rotate();
                }, Color.white, Color.magenta, null, true, false, false, false, null, true);
                rotateButton.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 190);
                rotateButton.gameObject.SetActive(false);
            }

            if (supportsTwoVibrators) {
                speedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                GameObject edgeSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
                edgeSliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                edgeSlider = edgeSliderObject.GetComponent<UnityEngine.UI.Slider>();
                edgeSlider.maxValue = maxSpeed2;
                edgeSlider.wholeNumbers = true;
                edgeSlider.value = 0;
                Transform textTransform = edgeSlider.transform.Find("Fill Area/VolumeNumberText");
                textTransform.localScale = new Vector3(1, 1, 1);
                edgeSliderText = textTransform.GetComponent<UnityEngine.UI.Text>();
                edgeSliderText.text = "Speed: 0%";
                edgeSliderObject.SetActive(false);
            }
        }

        internal void disable() {
            if (isActive) {
                isActive = false;
                MelonLogger.Msg("Disabled toy: " + name);
                hand = Hand.none;
                fixSlider();
                if (isLocal())
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.RemoveToy, this));
            }
        }

        internal void enable() {
            if (!isActive) {
                isActive = true;
                MelonLogger.Msg("Enabled toy: " + name);
            }
        }

        internal void setSpeed(int speed) {
            if (speed != lastSpeed) {
                lastSpeed = speed;
                speedSliderText.text = $"{name} Speed: {(double)speed / maxSpeed * 100}%";
                Console.WriteLine(isLocal() + "   " + speed);
                if (isLocal()) {
                    try {
                        device.SendVibrateCmd((double)speed / maxSpeed);
                        //MelonLogger.Msg("set device speed to " + ((double)speed / maxSpeed));
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetSpeed, this, speed));
                }

            }
        }

        internal void setEdgeSpeed(int speed) {
            if (speed != lastEdgeSpeed) {
                lastEdgeSpeed = speed;
                edgeSliderText.text = name + " Speed: " + (((double)speed /maxSpeed2) * 100) + "%";

                if (isLocal()) {
                    try {
                        //TODO fix this. i'm not sure how to vibrate just the second motor
                        device.SendVibrateCmd((double)speed / maxSpeed2);
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetSpeedEdge, this, speed));
                }


            }
        }

        internal void setContraction(int speed = -1) {
            if (speed == -1) {
                if (lastContraction != maxSlider.value) {
                    lastContraction = (int)maxSlider.value;
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetAir, this, lastContraction));
                    maxSliderText.text = "Contraction: " + lastContraction;
                }
            } else {
                if (speed != maxSlider.value) {
                    maxSlider.value = speed;
                    lastContraction = speed;
                    maxSliderText.text = "Contraction: " + maxSlider.value;

                    //MelonLogger.Msg("Contraction: " + maxSlider.value);

                    if (isLocal()) {
                        try {
                            //moves to new position in 1 second
                            device.SendLinearCmd(1000, (double)speed / maxLinear);
                        } catch (ButtplugDeviceException) {
                            MelonLogger.Error("Toy not connected");
                        }
                    } else {
                        VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetAir, this, (int)maxSlider.value));
                    }

                }
            }
        }

        internal bool clockwise = false;
        internal void rotate() {

            if (isLocal()) {
                try {
                    clockwise = !clockwise;
                    device.SendRotateCmd(lastSpeed, clockwise);
                } catch (ButtplugDeviceException) {
                    MelonLogger.Error("Toy not connected");
                }
            } else {
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetRotate, this));
            }
            
        }

        private void fixSlider() {
            //MelonLogger.Msg("fixSlider " + name + " " + hand);

            float sliderY = 0;

            if (hand != Hand.none && hand != Hand.shared) {
                speedSlider.transform.localPosition = new Vector3(-348.077f, 343.046f - sliderY, 0);
                speedSlider.gameObject.SetActive(true);

                if (maxSlider != null) {
                    maxSlider.transform.localPosition = new Vector3(492.955f, 343.046f - sliderY, 0);
                    maxSlider.gameObject.SetActive(true);
                }

                if (rotateButton != null) {
                    rotateButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(1330, -1340 - sliderY);
                    rotateButton.gameObject.SetActive(true);
                }

                if (edgeSlider != null) {
                    speedSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1260, -1340 - sliderY);
                    edgeSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-410, -1340 - sliderY);
                    edgeSlider.gameObject.SetActive(true);
                }

                sliderY += 160;
            } else {
                speedSlider.gameObject.SetActive(false);
                if (speedSlider != null) speedSlider.gameObject.SetActive(false);
                if (maxSlider != null) maxSlider.gameObject.SetActive(false);
                if (rotateButton != null) rotateButton.gameObject.SetActive(false);
            }

            BoxCollider collider = GameObject.Find("UserInterface/QuickMenu").GetComponent<BoxCollider>();
            collider.size = new Vector3(collider.size.x, collider.size.y + sliderY, collider.size.z);
        }

        internal void changeHand() {
            if (!isActive) return;

            hand++;
            if (hand > Enum.GetValues(typeof(Hand)).Cast<Hand>().Max())
                hand = 0;

            if (hand == Hand.shared && !isLocal())
                hand++;
            if (hand == Hand.both && !supportsTwoVibrators)
                hand++;

            if (isLocal()) {
                if (hand == Hand.shared) {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.AddToy, this));
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.RemoveToy, this));
                }
            }

            fixSlider();
        }

        //returns true if this is a local bluetooth device (controlled by someone else)
        internal bool isLocal() {
            return device != null;
        }

    }
}