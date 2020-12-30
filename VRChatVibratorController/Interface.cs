using MelonLoader;
using System;
using System.Threading;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Vibrator_Controller {
    class Interface {

        internal static QMNestedButton menu;
        internal static QMSingleButton HowToUse;
        internal static QMToggleButton holdToggle;
        internal static int buttonX;
        internal static int buttonY;
        internal static string subMenu;

        internal static void setupUI() {
            menu = new QMNestedButton(subMenu, buttonX, buttonY, "Vibrator\nController", "Vibrator Controller Settings");

            VibratorController.LockButtonUI = new QMSingleButton(menu, 1, 0, "Lock Speed\nButton", delegate () {
                if (VibratorController.findButton == "lockButton") {
                    VibratorController.lockButton = KeyCode.None;
                    VibratorController.findButton = null;
                    VibratorController.LockKeyBind.setButtonText("");
                    VibratorController.LockButtonUI.setButtonText("Lock Speed\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", VibratorController.lockButton.GetHashCode());
                    return;
                }
                VibratorController.findButton = "lockButton";
                VibratorController.LockButtonUI.setButtonText("Press Now");
            }, "Click than press button on controller to set button to lock vibraton speed", null, null);

            // LockKey keybind
            VibratorController.LockKeyBind = new QMSingleButton(menu, 1, 1, "none", new Action(() => {
            }), "Shows current Lock Speed Button keybind", null, null);
            VibratorController.LockKeyBind.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            VibratorController.LockKeyBind.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);
            VibratorController.LockKeyBind.setIntractable(false);
            VibratorController.LockKeyBind.setButtonText(VibratorController.lockButton.ToString());

            VibratorController.HoldButtonUI = new QMSingleButton(menu, 2, 0, "Hold\nButton", delegate () {
                if (VibratorController.findButton == "holdButton") {
                    VibratorController.holdButton = KeyCode.None;
                    VibratorController.findButton = null;
                    VibratorController.HoldKeyBind.setButtonText("");
                    VibratorController.HoldButtonUI.setButtonText("Hold\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", VibratorController.holdButton.GetHashCode());
                    return;
                }
                VibratorController.findButton = "holdButton";
                VibratorController.HoldButtonUI.setButtonText("Press Now");
            }, "Click than press button on controller to set button to hold to use toy", null, null);

            // LockKey keybind
            VibratorController.HoldKeyBind = new QMSingleButton(menu, 2, 1, "none", new Action(() => {
            }), "Shows current Hold Button keybind", null, null);
            VibratorController.HoldKeyBind.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            VibratorController.HoldKeyBind.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);
            VibratorController.HoldKeyBind.setIntractable(false);
            VibratorController.HoldKeyBind.setButtonText(VibratorController.holdButton.ToString());

            VibratorController.addButtonUI = new QMSingleButton(menu, 3, 0, "Add\nToy", delegate () {

                string text = System.Windows.Forms.Clipboard.GetText();

                if (text.Length != 4) {
                    VibratorController.addButtonUI.setButtonText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    return;
                }

                Client.send("join " + text);

            }, "Click to paste your friend's code", null, null);

            // How To Use Button
            HowToUse = new QMSingleButton(menu, 3, 1, "How To Use", new Action(() => {
                System.Diagnostics.Process.Start("https://github.com/markviews/VRChatVibratorController");
            }), "Opens a documentation by markviews", null, null);
            HowToUse.getGameObject().GetComponent<RectTransform>().sizeDelta /= new Vector2(1f, 2.0175f);
            HowToUse.getGameObject().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 96f);

            holdToggle = new QMToggleButton(menu, 5, -1, "Hold on", delegate () {
                VibratorController.requireHold = true;
                MelonPrefs.SetBool("VibratorController", "Requirehold", true);
            }, "Hold off", delegate () {
                VibratorController.requireHold = false;
                MelonPrefs.SetBool("VibratorController", "Requirehold", false);
            }, "Require holding a button to use toy?");

            holdToggle.setToggleState(VibratorController.requireHold);
        }

    }
}
