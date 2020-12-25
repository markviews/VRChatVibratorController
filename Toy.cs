using UnityEngine;

namespace Vibrator_Controller {
    class Toy {
        internal string hand = "none";
        private static int x = 1;
        private QMSingleButton button;
        private float lastSpeed = 0;
        internal float contraction = -1;
        private string name;
        private string id;
        internal UnityEngine.UI.Slider speedSlider;//slider for vibrator speed
        internal UnityEngine.UI.Text speedSliderText;
        internal UnityEngine.UI.Slider maxSlider;//slider for max's contractions
        internal UnityEngine.UI.Text maxSliderText;


        internal Toy(string name, string id) {
            this.id = id;
            this.name = name;
            button = new QMSingleButton(Interface.menu, x++, 2, name + "\nClick to\nSet", delegate () {
                changeHand();
            }, "Click to set controll mode", null, null);
            VibratorController.toys.Add(this);

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
                fixSliders();
            }
        }

        internal void showSlider(bool toggle) {
            speedSlider.gameObject.SetActive(toggle);
            if (maxSlider != null) maxSlider.gameObject.SetActive(toggle);
        }

        internal void setSpeed(float speed) {
            speed = (int)(speed * 10);
            if (speed != lastSpeed) {
                lastSpeed = speed;
                Client.send("speed " + id + " " + speed);
                if (hand.Equals("slider"))
                    speedSliderText.text = name + " Speed: " + (speed * 10) + "%";
            }
        }

        internal void fixSliders() {
            float sliderY = 0;
            int sliders = 0;
            foreach (Toy toy in VibratorController.toys) {

                if (toy.hand.Equals("slider") || toy.name.Equals("Max")) {
                    sliders++;
                    if (toy.hand.Equals("slider")) {
                        toy.speedSlider.gameObject.SetActive(true);
                        toy.speedSlider.transform.localPosition = new Vector3(-348.077f, 343.046f - sliderY, 0);
                    }
                    if (toy.name.Equals("Max") && !toy.hand.Equals("none")) {
                        toy.maxSlider.gameObject.SetActive(true);
                        toy.maxSlider.transform.localPosition = new Vector3(492.955f, 343.046f - sliderY, 0);
                    }
                    sliderY += 160;
                }

                if (!toy.hand.Equals("slider"))
                    toy.speedSlider.gameObject.SetActive(false);

            }
            float add = 160 * sliders;
            BoxCollider collider = GameObject.Find("UserInterface/QuickMenu").GetComponent<BoxCollider>();
            collider.size = new Vector3(collider.size.x, collider.size.y + add, collider.size.z);
        }

        internal void changeHand() {
            switch (hand) {
                case "none":
                    hand = "left";
                    button.setButtonText(name + "\nLeft Trigger");
                    VibratorController.LockButtonUI.setActive(true);//in case this was disabled
                    if (maxSlider != null) maxSlider.gameObject.SetActive(true);//in case this was disabled
                    break;
                case "left":
                    hand = "right";
                    button.setButtonText(name + "\nRight Trigger");
                    break;
                case "right":
                    hand = "either";
                    button.setButtonText(name + "\nEither Trigger");
                    break;
                case "either":
                    hand = "slider";
                    button.setButtonText(name + "\nSlider");

                    //disable "Lock Speed Button" button when this is on for all connected toys
                    foreach (Toy toy in VibratorController.toys)
                        if (toy.hand != "slider" || toy.hand != "none") break;
                    VibratorController.LockButtonUI.setActive(false);
                    break;
                case "slider":
                    hand = "none";
                    button.setButtonText(name + "\nClick to\nSet");
                    VibratorController.LockButtonUI.setActive(true);//in case this was disabled
                    if (maxSlider != null) maxSlider.gameObject.SetActive(false);//hide 'Max' slider
                    break;
            }
            fixSliders();
        }


    }
}
