using MelonLoader;
using PlagueButtonAPI;
using System.Collections;
using UnityEngine;
using Buttplug;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Vibrator_Controller
{
    public enum Hand
    {
        none, shared, left, right, both, either, slider
    }
    public class Toy
    {
        internal static ArrayList toys = new ArrayList();
        internal static Dictionary<string, Toy> sharedToys = new Dictionary<string, Toy>();//id, Toy

        internal Hand hand = Hand.none;
        internal string name;
        internal string id;
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

        internal Toy(ButtplugClientDevice device) {
            int count = 1;
            id = device.Name.Replace(" ", "");
            while (sharedToys.ContainsKey(id)) {
                id = device.Name.Replace(" ", "") + count++;
            }

            hand = Hand.shared;
            name = device.Name;
            this.device = device;

            MelonLogger.Msg("Device connected: " + name + " [" + id + "]");
            CreateSlider();

            toys.Add(this);
            sharedToys.Add(id, this);
        }

        internal Toy(string name, string id) {
            toys.Add(this);
            this.name = name;
            this.id = id;
            CreateSlider();
        }

        private void CreateSlider() {
            GameObject slider = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/User Volume/VolumeSlider");
            GameObject quickmenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");

            GameObject speedSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
            speedSlider = speedSliderObject.GetComponent<UnityEngine.UI.Slider>();
            speedSlider.maxValue = 20;
            speedSlider.wholeNumbers = true;
            speedSlider.value = 0;
            speedSliderText = speedSlider.transform.Find("Fill Area/VolumeNumberText").GetComponent<UnityEngine.UI.Text>();
            speedSliderText.text = name + " Speed: 0%";
            speedSliderObject.SetActive(false);

            if (name.Equals("Max")) {
                GameObject maxSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
                maxSliderObject.transform.localScale = new Vector3(0.7f, 1, 1);
                maxSlider = maxSliderObject.GetComponent<UnityEngine.UI.Slider>();
                maxSlider.maxValue = 3;
                maxSlider.wholeNumbers = true;
                maxSlider.value = 0;
                Transform textTransform = maxSlider.transform.Find("Fill Area/VolumeNumberText");
                textTransform.localScale = new Vector3(1, 1, 1);
                maxSliderText = textTransform.GetComponent<UnityEngine.UI.Text>();
                maxSliderText.text = "Max Contraction: 0";
                maxSliderObject.SetActive(false);
            } else if (name.Equals("Nora")) {
                rotateButton = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Rotate", "Rotate Nora", ButtonAPI.HorizontalPosition.LeftOfMenu, ButtonAPI.VerticalPosition.BelowBottomButton, ButtonAPI.ShortcutMenuTransform, delegate (bool a) {
                    rotate();
                }, Color.white, Color.magenta, null, true, false, false, false, null, true);
                rotateButton.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 190);
                rotateButton.gameObject.SetActive(false);
            } else if (name.Equals("Edge")) {
                speedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                GameObject edgeSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
                edgeSliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                edgeSlider = edgeSliderObject.GetComponent<UnityEngine.UI.Slider>();
                edgeSlider.maxValue = 20;
                edgeSlider.wholeNumbers = true;
                edgeSlider.value = 0;
                Transform textTransform = edgeSlider.transform.Find("Fill Area/VolumeNumberText");
                textTransform.localScale = new Vector3(1, 1, 1);
                edgeSliderText = textTransform.GetComponent<UnityEngine.UI.Text>();
                edgeSliderText.text = "Edge Speed: 0%";
                edgeSliderObject.SetActive(false);
            }
        }

        internal void disable()
        {
            isActive = false;
            MelonLogger.Msg("Disabled toy: " + name);

            if (!isLocal()) {
                hand = Hand.none;
            }

            fixSlider();
        }

        internal void enable()
        {
            isActive = true;
            MelonLogger.Msg("Enabled toy: " + name);
        }

        internal void setSpeed(int speed)
        {
            if (speed != lastSpeed)
            {
                lastSpeed = speed;
                speedSliderText.text = name + " Speed: " + (speed * 10) + "%";
                //MelonLogger.Msg("Speed " + speed);

                if (isLocal()) {
                    try {
                        device.SendVibrateCmd((double)speed / 10);
                        //MelonLogger.Msg("set device speed to " + ((double)speed / 10));
                    } catch (ButtplugDeviceException) {
                        MelonLogger.Error("Toy not connected");
                    }
                } else {
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetSpeed, this, speed));
                }

            }
        }

        internal void setEdgeSpeed(int speed)
        {
            if (speed != lastEdgeSpeed)
            {
                lastEdgeSpeed = (int)speed;
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetSpeedEdge, this, speed));
                edgeSliderText.text = name + " Speed: " + (speed * 10) + "%";
            }
        }

        internal void setContraction(int speed = -1)
        {
            if (speed == -1)
            {
                if (lastContraction != maxSlider.value)
                {
                    lastContraction = (int)maxSlider.value;
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetAir, this, lastContraction));
                    maxSliderText.text = "Max Contraction: " + lastContraction;
                }
            }
            else
            {
                if (speed != maxSlider.value)
                {
                    maxSlider.value = speed;
                    lastContraction = speed;
                    VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetAir, this, (int)maxSlider.value));
                    maxSliderText.text = "Max Contraction: " + maxSlider.value;
                    
                    //MelonLogger.Msg("Max Contraction: " + maxSlider.value);
                }
            }
        }

        internal void rotate()
        {
            VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.SetRotate, this));
        }

        private void fixSlider()
        {
            //MelonLogger.Msg("fixSlider " + name + " " + hand);

            float sliderY = 0;

            if (hand != Hand.none && hand!=Hand.shared)
            {
                speedSlider.transform.localPosition = new Vector3(-348.077f, 343.046f - sliderY, 0);
                speedSlider.gameObject.SetActive(true);

                //MelonLogger.Msg("fixSlider " + name + " enabled slider");

                switch (name)
                {
                    case "Edge":
                        speedSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1260, -1340 - sliderY);
                        edgeSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-410, -1340 - sliderY);
                        edgeSlider.gameObject.SetActive(true);
                        break;
                    case "Max":
                        maxSlider.transform.localPosition = new Vector3(492.955f, 343.046f - sliderY, 0);
                        maxSlider.gameObject.SetActive(true);
                        break;
                    case "Nora":
                        rotateButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(1330, -1340 - sliderY);
                        rotateButton.gameObject.SetActive(true);
                        break;
                }

                sliderY += 160;
            }
            else
            {
                speedSlider.gameObject.SetActive(false);
                if (speedSlider != null) speedSlider.gameObject.SetActive(false);
                if (maxSlider != null) maxSlider.gameObject.SetActive(false);
                if (rotateButton != null) rotateButton.gameObject.SetActive(false);
            }

            BoxCollider collider = GameObject.Find("UserInterface/QuickMenu").GetComponent<BoxCollider>();
            collider.size = new Vector3(collider.size.x, collider.size.y + sliderY, collider.size.z);
        }

        internal void changeHand()
        {
            hand++;
            if (hand > Enum.GetValues(typeof(Hand)).Cast<Hand>().Max())
                hand = 0;


            if (hand == Hand.shared && !isLocal())
                hand++; 
            if (hand == Hand.both && !name.Equals("Edge"))
                hand++;
            /*
            switch (hand)
            {
                case Hand.none:
                    hand = Hand.left;
                    break;
                case Hand.left:
                    if (isLocal()) {
                        hand = Hand.shared;
                        break;
                    }
                    hand = Hand.right;
                    break;
                case Hand.shared:
                    hand = Hand.right;
                    break;
                case Hand.right:
                    hand = Hand.either;
                    break;
                case Hand.either:
                    hand = Hand.both;
                    if (!name.Equals("Edge")) {
                        changeHand();
                        break;
                    }
                    break;
                case Hand.both:
                    hand = Hand.slider;
                    break;
                case Hand.slider:
                    hand = Hand.none;
                    break;
            }*/
            fixSlider();
        }

        //returns true if this is a local bluetooth device (controlled by someone else)
        internal bool isLocal() {
            return device != null;
        }

    }
}
