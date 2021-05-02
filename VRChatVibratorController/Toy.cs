using MelonLoader;
using PlagueButtonAPI;
using System.Collections;
using UnityEngine;

namespace Vibrator_Controller
{
    class Toy
    {
        internal static ArrayList toys = new ArrayList();

        internal string hand = "none";
        private static int x = -3;
        internal ButtonAPI.PlagueButton button;
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

        internal Toy(string name, string id)
        {
            this.id = id;
            this.name = name;

            button = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, name + "\nClick to\nSet", "Click to set controll mode", (ButtonAPI.HorizontalPosition)x++, ButtonAPI.VerticalPosition.BottomButton, ButtonAPI.MakeEmptyPage("SubMenu_1").transform, delegate (bool a)
            {
                changeHand();
            }, Color.white, Color.magenta, null, true, false, false, false, null, true);

            toys.Add(this);

            GameObject slider = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/User Volume/VolumeSlider");
            GameObject quickmenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");

            GameObject speedSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
            speedSlider = speedSliderObject.GetComponent<UnityEngine.UI.Slider>();
            speedSlider.maxValue = 10;
            speedSlider.wholeNumbers = true;
            speedSlider.value = 0;
            speedSliderText = speedSlider.transform.Find("Fill Area/VolumeNumberText").GetComponent<UnityEngine.UI.Text>();
            speedSliderText.text = name + " Speed: 0%";
            speedSliderObject.SetActive(false);

            if (name.Equals("Max"))
            {
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
            }
            else if (name.Equals("Nora"))
            {
                rotateButton = ButtonAPI.CreateButton(ButtonAPI.ButtonType.Default, "Rotate", "Rotate Nora", ButtonAPI.HorizontalPosition.LeftOfMenu, ButtonAPI.VerticalPosition.BelowBottomButton, ButtonAPI.ShortcutMenuTransform, delegate (bool a)
                {
                    rotate();
                }, Color.white, Color.magenta, null, true, false, false, false, null, true);
                rotateButton.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 190);
                rotateButton.gameObject.SetActive(false);
            }
            else if (name.Equals("Edge"))
            {
                speedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                GameObject edgeSliderObject = GameObject.Instantiate(slider, quickmenu.transform, true);
                edgeSliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 160);
                edgeSlider = edgeSliderObject.GetComponent<UnityEngine.UI.Slider>();
                edgeSlider.maxValue = 10;
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
            MelonLogger.Msg("Disabled toy: " + id);
            hand = "none";
            button.SetText(name + "\nClick to\nSet");
            button.gameObject.SetActive(false);
            fixSliders();
        }

        internal void enable()
        {
            isActive = true;
            MelonLogger.Msg("Enabled toy: " + id);
            button.gameObject.SetActive(true);
        }

        internal void showSlider(bool toggle)
        {
            speedSlider.gameObject.SetActive(toggle);
            if (maxSlider != null) maxSlider.gameObject.SetActive(toggle);
        }

        internal int lastSpeed = 0;

        internal void setSpeed(int speed)
        {
            if (speed != lastSpeed)
            {
                lastSpeed = speed;
                Client.send("speed " + id + " " + speed + (name is "Edge" ? " 1" : ""));
                speedSliderText.text = name + " Speed: " + (speed * 10) + "%";
            }
        }

        internal int lastEdgeSpeed = 0;

        internal void setEdgeSpeed(float speed)
        {
            if (speed != lastEdgeSpeed)
            {
                lastEdgeSpeed = (int)speed;
                Client.send("speed " + id + " " + speed + " 2");
                edgeSliderText.text = name + " Speed: " + (speed * 10) + "%";
            }
        }

        internal int contraction = 0;

        internal void setContraction(int speed = -1)
        {
            if (speed == -1)
            {
                if (contraction != maxSlider.value)
                {
                    contraction = (int)maxSlider.value;
                    Client.send("air " + id + " " + contraction);
                    maxSliderText.text = "Max Contraction: " + contraction;
                }
            }
            else
            {
                if (speed != maxSlider.value)
                {
                    maxSlider.value = speed;
                    contraction = speed;
                    Client.send("air " + id + " " + maxSlider.value);
                    maxSliderText.text = "Max Contraction: " + maxSlider.value;
                    
                    MelonLogger.Warning("Max Contraction: " + maxSlider.value);
                }
            }
        }

        internal void rotate()
        {
            Client.send("rotate " + id);
        }

        internal void fixSliders()
        {
            float sliderY = 0;
            foreach (Toy toy in toys)
            {

                if (!toy.hand.Equals("none"))
                {
                    toy.speedSlider.transform.localPosition = new Vector3(-348.077f, 343.046f - sliderY, 0);
                    toy.speedSlider.gameObject.SetActive(true);

                    switch (toy.name)
                    {
                        case "Edge":
                            toy.speedSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1260, -1340 - sliderY);
                            toy.edgeSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(-410, -1340 - sliderY);
                            toy.edgeSlider.gameObject.SetActive(true);
                            break;
                        case "Max":
                            toy.maxSlider.transform.localPosition = new Vector3(492.955f, 343.046f - sliderY, 0);
                            toy.maxSlider.gameObject.SetActive(true);
                            break;
                        case "Nora":
                            toy.rotateButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(1330, -1340 - sliderY);
                            toy.rotateButton.gameObject.SetActive(true);
                            break;
                    }

                    sliderY += 160;
                }
                else
                {
                    toy.speedSlider.gameObject.SetActive(false);
                    if (toy.speedSlider != null) toy.speedSlider.gameObject.SetActive(false);
                    if (toy.maxSlider != null) toy.maxSlider.gameObject.SetActive(false);
                    if (toy.rotateButton != null) toy.rotateButton.gameObject.SetActive(false);
                }

            }
            BoxCollider collider = GameObject.Find("UserInterface/QuickMenu").GetComponent<BoxCollider>();
            collider.size = new Vector3(collider.size.x, collider.size.y + sliderY, collider.size.z);
        }

        internal void changeHand()
        {
            switch (hand)
            {
                case "none":
                    hand = "left";
                    button.SetText(name + "\nLeft Trigger");
                    break;
                case "left":
                    hand = "right";
                    button.SetText(name + "\nRight Trigger");
                    break;
                case "right":
                    hand = "either";
                    button.SetText(name + "\nEither Trigger");
                    break;
                case "either":
                    hand = "both";
                    if (!name.Equals("Edge"))
                    {
                        changeHand();
                        break;
                    }
                    button.SetText(name + "\nLeft/Right\n(for edge)");
                    break;
                case "both":
                    hand = "slider";
                    button.SetText(name + "\nSlider only");
                    break;
                case "slider":
                    hand = "none";
                    button.SetText(name + "\nClick to\nSet");
                    break;
            }
            fixSliders();
        }


    }
}
