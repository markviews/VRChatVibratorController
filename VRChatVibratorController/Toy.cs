using MelonLoader;
using Buttplug;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.XR;
using VRChatUtilityKit.Ui;
using UnityEngine;
using System.Threading.Tasks;
using VRChatUtilityKit.Utilities;
using VRC;

namespace Vibrator_Controller {
    public enum Hand {
        none, shared, left, right, both, either, actionmenu
    }
    public class Toy {
        internal static Dictionary<ulong, Toy> remoteToys { get; set; } = new Dictionary<ulong, Toy>();
        internal static Dictionary<ulong, Toy> myToys { get; set; } = new Dictionary<ulong, Toy>();
        internal static List<Toy> allToys => remoteToys.Select(x=>x.Value).Union(myToys.Select(x => x.Value)).ToList();

        internal SingleButton changeMode;
        internal SingleButton inc;
        internal SingleButton dec;
        internal Label label;

        internal ButtonGroup toys;
        internal SubMenu menu;

        internal Hand hand = Hand.none;
        internal string name;
        internal ulong id;
        internal bool isActive = true;

        internal ButtplugClientDevice device;
        internal string connectedTo;

        internal int lastSpeed = 0, lastEdgeSpeed = 0, lastContraction = 0;

        internal bool supportsRotate = false, supportsLinear = false, supportsTwoVibrators = false, supportsBatteryLVL = false;
        internal int maxSpeed = 20, maxSpeed2 = -1, maxLinear = -1;
        internal double battery = -1;

        internal Toy(ButtplugClientDevice device, SubMenu menu)
        {
            this.menu = menu;
            id = (device.Index + (ulong) Player.prop_Player_0.prop_String_0.GetHashCode()) % long.MaxValue;
            hand = Hand.shared;
            name = device.Name;
            this.device = device;

            //remove company name
            if (name.Split(' ').Length > 1) name = name.Split(' ')[1];

            if (myToys.ContainsKey(id))
            {
                MelonLogger.Msg("Device reconnected: " + name + " [" + id + "]");
                myToys[id].name = name; //id should be uniquie but just to be sure
                myToys[id].device = device;
                myToys[id].enable();
                return;
            }



            MelonLogger.Msg("Device connected: " + name + " [" + id + "]");

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.LinearCmd))
                supportsLinear = true;

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.RotateCmd))
                supportsRotate = true;


            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.BatteryLevelCmd))
            {
                supportsBatteryLVL = true;
                UpdateBattery();

            }

            //prints info about the device
            foreach (KeyValuePair<ServerMessage.Types.MessageAttributeType, ButtplugMessageAttributes> entry in device.AllowedMessages)
                MelonLogger.Msg("[" + id + "] Allowed Message: " + entry.Key);

            if (device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.VibrateCmd))
            {
                ButtplugMessageAttributes attributes = device.AllowedMessages[ServerMessage.Types.MessageAttributeType.VibrateCmd];

                if (attributes.ActuatorType != null && attributes.ActuatorType.Length > 0)
                    MelonLogger.Msg("[" + id + "] ActuatorType " + string.Join(", ", attributes.ActuatorType));

                if (attributes.StepCount != null && attributes.StepCount.Length > 0)
                {
                    MelonLogger.Msg("[" + id + "] StepCount " + string.Join(", ", attributes.StepCount));
                    maxSpeed = (int)attributes.StepCount[0];
                }
                if (attributes.StepCount != null && attributes.StepCount.Length == 2)
                {
                    supportsTwoVibrators = true;
                    maxSpeed2 = (int)attributes.StepCount[1];
                }

                if (attributes.Endpoints != null && attributes.Endpoints.Length > 0)
                    MelonLogger.Msg("[" + id + "] Endpoints " + string.Join(", ", attributes.Endpoints));

                if (attributes.MaxDuration != null && attributes.MaxDuration.Length > 0)
                    MelonLogger.Msg("[" + id + "] MaxDuration " + string.Join(", ", attributes.MaxDuration));

                if (attributes.Patterns != null && attributes.Patterns.Length > 0)
                    foreach (string[] pattern in attributes.Patterns)
                        MelonLogger.Msg("[" + id + "] Pattern " + string.Join(", ", pattern));
            }

            myToys.Add(id, this);
            createMenu();

            if (hand == Hand.shared)
            {
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.AddToy, this));
            }
        }

        internal Toy(string name, ulong id, string connectedTo, int maxSpeed, int maxSpeed2, int maxLinear, bool supportsRotate, SubMenu menu)
        {
            this.menu = menu;
            if (remoteToys.ContainsKey(id))
            {
                MelonLogger.Msg("Device reconnected: " + name + " [" + id + "]");
                if (maxSpeed2 != -1) remoteToys[id].supportsTwoVibrators = true;
                if (maxLinear != -1) remoteToys[id].supportsLinear = true;
                remoteToys[id].name = name;
                remoteToys[id].connectedTo = connectedTo;
                remoteToys[id].supportsRotate = supportsRotate;
                remoteToys[id].maxSpeed = maxSpeed;
                remoteToys[id].maxSpeed2 = maxSpeed2;
                remoteToys[id].maxLinear = maxLinear;
                remoteToys[id].enable();
                MelonLogger.Msg($"Reconnected toy Name: {remoteToys[id].name}, ID: {remoteToys[id].id} Max Speed: {remoteToys[id].maxSpeed}" + (remoteToys[id].supportsTwoVibrators ? $", Max Speed 2: {remoteToys[id].maxSpeed2}" : "") + (remoteToys[id].supportsLinear ? $", Max Linear Speed: {remoteToys[id].maxLinear}" : "") + (remoteToys[id].supportsRotate ? $", Supports Rotation" : ""));
                return;
            }

            if (maxSpeed2 != -1) supportsTwoVibrators = true;
            if (maxLinear != -1) supportsLinear = true;

            this.supportsRotate = supportsRotate;
            this.maxSpeed = maxSpeed;
            this.maxSpeed2 = maxSpeed2;
            this.maxLinear = maxLinear;
            this.name = name;
            this.connectedTo = connectedTo;
            this.id = id;

            MelonLogger.Msg($"Added toy Name: {name}, ID: {id} Max Speed: {maxSpeed}" + (supportsTwoVibrators ? $", Max Speed 2: {maxSpeed2}" : "") + (supportsLinear ? $", Max Linear Speed: {maxLinear}" : "") + (supportsRotate ? $", Supports Rotation" : ""));

            remoteToys.Add(id, this);
            createMenu();

        }

        private void createMenu()
        {
            toys = new ButtonGroup("Toy" + id, name);
            int step = (int)(maxSpeed * ((float)VibratorController.buttonStep / 100));

                
            changeMode = new SingleButton(() => changeHand(), VibratorController.CreateSpriteFromTexture2D(GetTexture()), $"Mode\n{hand}", "mode", "Change Mode");
            inc = new SingleButton(() => { if (lastSpeed + step <= maxSpeed) setSpeed(lastSpeed + step); }, VibratorController.CreateSpriteFromTexture2D(GetTexture()), "Inc", "inc", "Increment Speed");
            dec = new SingleButton(() => { if (lastSpeed - step >= 0) setSpeed(lastSpeed - step); }, VibratorController.CreateSpriteFromTexture2D(GetTexture()), "Dec", "dec", "Decrement Speed");
            label = new Label($"Current Speed: {lastSpeed}", "Battery not available" , "BatteryStatus");
            
            label.TextComponent.fontSize = 24;
            toys.AddButton(changeMode);
            toys.AddButton(inc);
            toys.AddButton(dec);
            toys.AddButton(label);

            menu.AddButtonGroup(toys);


            //fix if added after init phase
            toys.gameObject.transform.localScale = Vector3.one;
            toys.Header.gameObject.transform.localScale = Vector3.one;

            toys.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            toys.Header.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var pos = toys.gameObject.transform.localPosition;
            var pos2 = toys.Header.gameObject.transform.localPosition;

            toys.gameObject.transform.localPosition = new Vector3(0, pos.y, 0);
            toys.Header.gameObject.transform.localPosition = new Vector3(0, pos2.y, 0);

        }

        public Texture2D GetTexture()
        {
            if (connectedTo == "all")
                return VibratorController.logo;
            if (VibratorController.toy_icons.ContainsKey(name))
                return VibratorController.toy_icons[name];

            return null;
        }

        internal void disable() {
            if (isActive) {
                isActive = false;
                MelonLogger.Msg("Disabled toy: " + name);
                hand = Hand.none;
                toys.rectTransform.gameObject.active = false;
                toys.Header.gameObject.active = false;
                if (isLocal()) {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.RemoveToy, this));
                }
                    
            }
        }

        internal void enable() {
            if (!isActive) {
                isActive = true;
                toys.rectTransform.gameObject.active = true;
                toys.Header.gameObject.active = true;

                if (device != null && device.AllowedMessages.ContainsKey(ServerMessage.Types.MessageAttributeType.BatteryLevelCmd))
                {
                    supportsBatteryLVL = true;
                    UpdateBattery();
                }
                if (isLocal() && hand == Hand.shared)
                {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.AddToy, this));
                }
                MelonLogger.Msg("Enabled toy: " + name);
            }
        }

        internal void setSpeed(int speed) {
            if (speed != lastSpeed) {
                lastSpeed = speed;
                label.Text = $"Current Speed: {speed}";

                if (connectedTo == "all")
                {
                    foreach (var toy in allToys.Where(x=>x.connectedTo != "all"))
                    {
                        toy.setSpeed(speed);
                        if(toy.supportsTwoVibrators)
                            toy.setEdgeSpeed(speed);
                    }
                    return;
                }
                if (isLocal()) {
                    try
                    {
                        if(supportsTwoVibrators)
                            device.SendVibrateCmd(new List<double> { (double)lastSpeed / maxSpeed, (double)lastEdgeSpeed / maxSpeed2 });
                        else
                            device.SendVibrateCmd((double)speed / maxSpeed);

                        //MelonLogger.Msg("set device speed to " + ((double)speed / maxSpeed));
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.SetSpeed, this, speed));
                }
            }
        }

        internal void setEdgeSpeed(int speed) {
            if (speed != lastEdgeSpeed) {
                lastEdgeSpeed = speed;

                if (isLocal()) {
                    try {
                        device.SendVibrateCmd(new List<double> { (double)lastSpeed / maxSpeed, (double)lastEdgeSpeed / maxSpeed2 });
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.SetSpeedEdge, this, speed));
                }


            }
        }

        internal void setContraction(int speed) {
            if (lastContraction != speed) {
                lastContraction = speed;

                if (isLocal()) {
                    try {
                        //moves to new position in 1 second
                        device.SendLinearCmd(1000, (double)speed / maxLinear);
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.SetAir, this, speed));
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
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.SetRotate, this));
            }
            
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

            if (!XRDevice.isPresent && (hand == Hand.both || hand == Hand.either || hand == Hand.left || hand == Hand.right))
                hand = Hand.actionmenu;

            if (isLocal()) {
                if (hand == Hand.shared) {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.AddToy, this));
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(connectedTo, Commands.RemoveToy, this));
                }
            }
            changeMode.Text = "Mode\n"+ hand;
        }

        //returns true if this is a local bluetooth device (controlled by someone else)
        internal bool isLocal() {
            return device != null;
        }

        internal async void UpdateBattery()
        {
            try
            {
                while (isActive)
                {
                    battery = await device.SendBatteryLevelCmd();
                    await AsyncUtils.YieldToMainThread();
                    if (label != null)
                        label.SubtitleText = $"Battery: {battery * 100}";
                    await Task.Delay(1000 * 10);
                }
            }
            catch (Exception)
            {
                //maybe device dissconnected during cmd
            }
            
        }

    }
}