using MelonLoader;
using System;
using System.Collections;
using UnityEngine;

namespace Vibrator_Controller {
    public class VibratorController : MelonMod {

        public static ArrayList toys = new ArrayList();
        string findButton = null;
        bool lockSpeed = false;
        bool requireHold;

        public static QMNestedButton menu;
        public static QMSingleButton LockButtonUI;
        QMSingleButton LockKeyBind;
        QMSingleButton HoldButtonUI;
        QMSingleButton HoldKeyBind;
        public static QMSingleButton addButtonUI;
        QMToggleButton holdToggle;
        QMSingleButton HowToUse;
        KeyCode lockButton;//button to lock speed
        KeyCode holdButton;//button to hold with other controll to use toy (if enabled)
        public static string subMenu;
        int buttonX;
        int buttonY;

        public override void OnApplicationStart() {
            MelonPrefs.RegisterCategory("VibratorController", "Vibrator Controller");
            MelonPrefs.RegisterInt("VibratorController", "lockButton", 0, "Button to lock speed");
            MelonPrefs.RegisterInt("VibratorController", "holdButton", 0, "Button to hold to use toy");
            MelonPrefs.RegisterBool("VibratorController", "Requirehold", false, "If enabled you will need to hold set button to use toy");
            MelonPrefs.RegisterString("VibratorController", "subMenu", "UIElementsMenu", "Menu to put the mod button on");
            MelonPrefs.RegisterInt("VibratorController", "buttonX", 0, "x position to put the mod button");
            MelonPrefs.RegisterInt("VibratorController", "buttonY", 0, "y position to put the mod button");

            lockButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "lockButton");
            holdButton = (KeyCode)MelonPrefs.GetInt("VibratorController", "holdButton");
            requireHold = MelonPrefs.GetBool("VibratorController", "Requirehold");
            subMenu = MelonPrefs.GetString("VibratorController", "subMenu");
            buttonX = MelonPrefs.GetInt("VibratorController", "buttonX");
            buttonY = MelonPrefs.GetInt("VibratorController", "buttonY");
        }

        public override void VRChat_OnUiManagerInit() {
            menu = new QMNestedButton(subMenu, buttonX, buttonY, "Vibrator\nController", "Vibrator Controller Settings");

            LockButtonUI = new QMSingleButton(menu, 1, 0, "Lock Speed\nButton", delegate () {
                if (findButton == "lockButton") {
                    lockButton = KeyCode.None;
                    findButton = null;
                    LockButtonUI.setButtonText("Lock Speed\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", lockButton.GetHashCode());
                    return;
                }
                findButton = "lockButton";
                LockButtonUI.setButtonText("Press Now");
            }, "Click than press button on controller to set button to lock vibraton speed", null, null);

            // LockKey keybind 
            LockKeyBind = new QMSingleButton(menu, 1, 1, "none", new System.Action(() => {

            }), "Shows current Lock Speed Button keybind", null, null);
            LockKeyBind.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            LockKeyBind.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);
            LockKeyBind.setIntractable(false);

            HoldButtonUI = new QMSingleButton(menu, 2, 0, "Hold\nButton", delegate () {
                if (findButton == "holdButton") {
                    holdButton = KeyCode.None;
                    findButton = null;
                    HoldButtonUI.setButtonText("Hold\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", holdButton.GetHashCode());
                    return;
                }
                findButton = "holdButton";
                HoldButtonUI.setButtonText("Press Now");
            }, "Click than press button on controller to set button to hold to use toy", null, null);

            // LockKey keybind 
            HoldKeyBind = new QMSingleButton(menu, 2, 1, "none", new System.Action(() => {

            }), "Shows current Hold Button keybind", null, null);
            HoldKeyBind.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            HoldKeyBind.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);
            HoldKeyBind.setIntractable(false);

            addButtonUI = new QMSingleButton(menu, 3, 0, "Add\nToy", delegate () {

                string text = System.Windows.Forms.Clipboard.GetText();

                if (text.Length != 4) {
                    addButtonUI.setButtonText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    return;
                }

                Client.setupClient();
                Client.send("join " + text);

            }, "Click to paste your friend's Long Distance Control Link code", null, null);

            // How To Use Button
            HowToUse = new QMSingleButton(menu, 3, 1, "How To Use", new System.Action(() => {
                System.Diagnostics.Process.Start("https://github.com/markviews/VRChatVibratorController");
            }), "Opens a documentation by markviews", null, null);
            HowToUse.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            HowToUse.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);

            holdToggle = new QMToggleButton(menu, 5, -1, "Hold on", delegate () {
                HoldButtonUI.setActive(true);
                HoldKeyBind.setActive(true);
                requireHold = true;
                MelonPrefs.SetBool("VibratorController", "Requirehold", true);
            }, "Hold off", delegate () {
                HoldButtonUI.setActive(false);
                HoldKeyBind.setActive(false);
                requireHold = false;
                MelonPrefs.SetBool("VibratorController", "Requirehold", false);
            }, "Require holding a button to use toy?");

            holdToggle.setToggleState(requireHold);
            HoldButtonUI.setActive(requireHold);
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
        public static void message(string msg) {
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
                    }
                    break;
                case "notFound":
                    MelonLogger.Log("Invalid code");
                    addButtonUI.setButtonText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    break;
                case "left":
                    MelonLogger.Log("User disconnected");
                    break;
            }
        }

        public void getButton() {
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

        public void setButton(KeyCode button) {
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