using Il2CppSystem.Collections.Generic;
using MelonLoader;
using PlagueButtonAPI;
using System;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace Vibrator_Controller {
    class Interface {
        internal static int buttonX;
        internal static int buttonY;
        internal static string subMenu;

        internal static void setupUI() {
            ButtonAPI.CustomTransform = GameObject.Find("/UserInterface/QuickMenu/" + subMenu).transform;
            ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Vibrator\nController", "Vibrator Controller Settings", (ButtonAPI.HorizontalPosition)buttonX - 4, (ButtonAPI.VerticalPosition)buttonY + 3, null, delegate (bool a) {
                ButtonAPI.EnterSubMenu(ButtonAPI.MakeEmptyPage("SubMenu_1"));
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            VibratorController.LockButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Back", "exit this menu", ButtonAPI.HorizontalPosition.RightOfMenu, ButtonAPI.VerticalPosition.BottomButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                ButtonAPI.EnterSubMenu(GameObject.Find("/UserInterface/QuickMenu/" + subMenu));
            }, Color.yellow, Color.magenta, null, true, false, false, false, null, true);

            //Lock button
            VibratorController.LockButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Lock Speed\nButton", "Click than press button on controller to set button to lock vibraton speed (click twice to disable)", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                if (VibratorController.findButton == "lockButton") {
                    VibratorController.lockButton = KeyCode.None;
                    VibratorController.findButton = null;
                    VibratorController.LockKeyBind.SetText("");
                    VibratorController.LockButtonUI.SetText("Lock Speed\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", VibratorController.lockButton.GetHashCode());
                    return;
                }
                VibratorController.findButton = "lockButton";
                VibratorController.LockButtonUI.SetText("Press Now");
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            VibratorController.LockKeyBind = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "", "Lock Speed Keybind", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
            }, Color.white, Color.grey, null, false, false, false, false, null, false);
            if (VibratorController.lockButton != 0)
                VibratorController.LockKeyBind.SetText(VibratorController.lockButton.ToString());

            //Hold button
            VibratorController.HoldButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Hold\nButton", "Click than press button on controller to set button to hold to use toy (click twice to disable)", ButtonAPI.HorizontalPosition.SecondButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                if (VibratorController.findButton == "holdButton") {
                    VibratorController.holdButton = KeyCode.None;
                    VibratorController.findButton = null;
                    VibratorController.HoldKeyBind.SetText("");
                    VibratorController.HoldButtonUI.SetText("Hold\nButton\nCleared");
                    MelonPrefs.SetInt("VibratorController", "lockButton", VibratorController.holdButton.GetHashCode());
                    return;
                }
                VibratorController.findButton = "holdButton";
                VibratorController.HoldButtonUI.SetText("Press Now");
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            VibratorController.HoldKeyBind = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "", "Hold Keybind", ButtonAPI.HorizontalPosition.SecondButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
            }, Color.white, Color.grey, null, false, false, false, false, null, false);
            if (VibratorController.holdButton != 0)
            VibratorController.HoldKeyBind.SetText(VibratorController.holdButton.ToString());

            //Add toy
            VibratorController.addButtonUI = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Add\nToy", "Click to pair with a friend's toy", ButtonAPI.HorizontalPosition.ThirdButtonPos, ButtonAPI.VerticalPosition.TopButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                InputPopup("", delegate (string text) {
                    if (text.Length != 4) {
                        VibratorController.addButtonUI.SetText("Add\nToys\n<color=#FF0000>Invalid Code</color>");
                    } else {
                        Client.send("join " + text);
                    }
                });
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            //How to use
            ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "How To Use", "Opens instructions to use", ButtonAPI.HorizontalPosition.ThirdButtonPos, ButtonAPI.VerticalPosition.SecondButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a) {
                System.Diagnostics.Process.Start("https://github.com/markviews/VRChatVibratorController");
            }, Color.white, Color.grey, null, false, false, false, false, null, false);

        }

        //thanks to Plague#2850 for helping me with this
        internal static void InputPopup(string title, Action<string> okaction) {
            VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0
                .Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_0(
                    title, "", InputField.InputType.Standard, false, "Confirm",
                    DelegateSupport.ConvertDelegate<Il2CppSystem.Action<string, List<KeyCode>, Text>>(
                        (Action<string, List<KeyCode>, Text>)delegate (string s, List<KeyCode> k, Text t) {
                            okaction(s);
                        }), null, "...");
        }


    }
}
