using MelonLoader;
using System;
using System.Collections;
using UnityEngine;

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        internal static ArrayList toys = new ArrayList();
        internal static string findButton = null;
        internal static bool lockSpeed = false;
        internal static bool requireHold;
        internal static QMSingleButton LockButtonUI;
        internal static QMSingleButton LockKeyBind;
        internal static QMSingleButton HoldButtonUI;
        internal static QMSingleButton HoldKeyBind;
        internal static QMSingleButton addButtonUI;
        internal static KeyCode lockButton;//button to lock speed
        internal static KeyCode holdButton;//button to hold with other controll to use toy (if enabled)

        public override void OnApplicationStart() {
            MelonPrefs.RegisterCategory("VibratorController", "Vibrator Controller");
            MelonPrefs.RegisterInt("VibratorController", "lockButton", 0, "Button to lock speed");
            MelonPrefs.RegisterInt("VibratorController", "holdButton", 0, "Button to hold to use toy");
            MelonPrefs.RegisterBool("VibratorController", "Requirehold", false, "If enabled you will need to hold set button to use toy");
            MelonPrefs.RegisterString("VibratorController", "subMenu", "UIElementsMenu", "Menu to put the mod button on");
            MelonPrefs.RegisterInt("VibratorController", "buttonX", 0, "x position to put the mod button");
            MelonPrefs.RegisterInt("VibratorController", "buttonY", 0, "y position to put the mod button");

            VibratorController.lockButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "lockButton");
            VibratorController.holdButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "holdButton");
            VibratorController.requireHold = MelonPrefs.GetBool("VibratorController", "Requirehold");
            Interface.subMenu = MelonPrefs.GetString("VibratorController", "subMenu");
            Interface.buttonX = MelonPrefs.GetInt("VibratorController", "buttonX");
            Interface.buttonY = MelonPrefs.GetInt("VibratorController", "buttonY");
        }

        public override void VRChat_OnUiManagerInit() {
            Interface.setupUI();
        }

        public override void OnUpdate() {
            if (HoldKeyBind != null) {
                HoldKeyBind.setButtonText(holdButton.ToString());
            }

            if (LockKeyBind != null) {
                LockKeyBind.setButtonText(lockButton.ToString());
            }

            if (findButton != null) getButton();

            if (Input.GetKeyDown(lockButton)) {
                if (lockSpeed) lockSpeed = false;
                else lockSpeed = true;
            }

            if (lockSpeed) return;

            foreach (Toy toy in toys) {

                if (toy.maxSlider != null) {
                    if (toy.contraction != toy.maxSlider.value) {
                        toy.contraction = toy.maxSlider.value;
                        toy.maxSliderText.text = "Max Contraction: " + toy.contraction;
                        //toy.send((int)toy.lastSpeed, toy.contraction);
                    }
                }

                if (toy.hand != "slider" && requireHold)
                    if (!Input.GetKey(holdButton)) {
                        toy.setSpeed(0);
                    }

                float speed = 0;
                switch (toy.hand) {
                    case "none":
                        break;
                    case "left":
                        speed = Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger");
                        break;
                    case "right":
                        speed = Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger");
                        break;
                    case "either":
                        float left = Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger");
                        float right = Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger");
                        if (left > right) speed = left;
                        else speed = right;
                        break;
                    case "slider":
                        speed = toy.speedSlider.value / 10;
                        break;
                }
                toy.setSpeed(speed);
            }
        }

        //message from server
        internal static void message(string msg) {
            MelonLogger.Log(msg);
            String[] args = msg.Split(' ');
            switch (args[0]) {
                case "toys":
                case "add":
                    for (int i = 1; i < args.Length; i++) {
                        string[] toyData = args[i].Split(':');
                        string name = toyData[0];
                        string id = toyData[1];
                        MelonLogger.Log("Adding: " + name + ":" + id);
                        new Toy(name, id);
                    }
                    break;
                case "remove": {
                        string[] toyData = args[1].Split(':');
                        string name = toyData[0];
                        string id = toyData[1];
                        MelonLogger.Log("Removing: " + name + ":" + id);
                        //TODO add this feature
                    }
                    break;
                case "notFound":
                    MelonLogger.Log("Invalid code");
                    addButtonUI.setButtonText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    //TODO fix button after a second
                    break;
                case "left":
                    MelonLogger.Log("User disconnected");
                    //TODO display this somehow
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
                LockButtonUI.setButtonText("Lock Speed\nButton Set");
                MelonPrefs.SetInt("VibratorController", "lockButton", button.GetHashCode());
            } else if (findButton.Equals("holdButton")) {
                holdButton = button;
                HoldButtonUI.setButtonText("Hold\nButton Set");
                MelonPrefs.SetInt("VibratorController", "holdButton", button.GetHashCode());
            }
            findButton = null;
        }

    }
}