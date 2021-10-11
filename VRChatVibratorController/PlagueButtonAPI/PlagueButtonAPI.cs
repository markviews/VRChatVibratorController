using MelonLoader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace PlagueButtonAPI {
    #region PlagueButtonAPI - Created By Plague
    internal class ButtonAPI {
        #region Creditation And Disclaimer
#pragma warning disable 414

        private static readonly string Creditation =
        "Plague Button API" +
        "https://VRCAntiCrash.com" +
        "Copyright Reserved" +
        "Use-Only Licensed" +
        "https://github.com/PlagueVRC/PlagueButtonAPI" +
        "Removal Or Modification Of This String Breaches The License." +
        "This String Is To Be Preserved AS IS.";

#pragma warning restore 414
        #endregion Creditation And Disclaimer

        #region PlagueButton Class

        internal class PlagueButton {
            #region Constructors
            public PlagueButton(GameObject gameObject = null, Button button = null, Text text = null, UiTooltip tooltip = null, Image image = null, RectTransform rect = null, Color? OffColour = null, Color? OnColour = null, Color? BorderColour = null, bool ToggleState = false, float xPos = 0f, float yPos = 0f) {
                if (gameObject != null) {
                    this.gameObject = gameObject;
                }

                if (button != null) {
                    this.button = button;
                }

                if (text != null) {
                    this.text = text;
                }

                if (tooltip != null) {
                    this.tooltip = tooltip;
                }

                if (image != null) {
                    this.image = image;
                }

                if (rect != null) {
                    this.rect = rect;
                }

                //Read Only - They Can Only Be Set Here!
                if (OffColour != null) {
                    this.OffColour = (Color)OffColour;
                }

                if (OnColour != null) {
                    this.OnColour = (Color)OnColour;
                }

                this.BorderColour = BorderColour;
                this.ToggleState = ToggleState;
                this.xPos = xPos;
                this.yPos = yPos;
            }

            public PlagueButton(Color? OffColour = null, Color? OnColour = null, Color? BorderColour = null, bool ToggleState = false, float xPos = 0f, float yPos = 0f) {
                //Read Only - They Can Only Be Set Here!
                if (OffColour != null) {
                    this.OffColour = (Color)OffColour;
                }

                if (OnColour != null) {
                    this.OnColour = (Color)OnColour;
                }

                this.BorderColour = BorderColour;
                this.ToggleState = ToggleState;
                this.xPos = xPos;
                this.yPos = yPos;
            }
            #endregion

            #region Reference Objects

            private GameObject _gameObject;
            internal GameObject gameObject {
                get => _gameObject;
                set {
                    if (_gameObject == null) {
                        _gameObject = value;
                    }
                }
            }

            private Button _button;
            internal Button button {
                get => _button;
                set {
                    if (_button == null) {
                        _button = value;
                    }
                }
            }

            private Text _text;
            internal Text text {
                get => _text;
                set {
                    if (_text == null) {
                        _text = value;
                    }
                }
            }

            private UiTooltip _tooltip;
            internal UiTooltip tooltip {
                get => _tooltip;
                set {
                    if (_tooltip == null) {
                        _tooltip = value;
                    }
                }
            }

            private Image _image;
            internal Image image {
                get => _image;
                set {
                    if (_image == null) {
                        _image = value;
                    }
                }
            }

            private RectTransform _rect;
            internal RectTransform rect {
                get => _rect;
                set {
                    if (_rect == null) {
                        _rect = value;
                    }
                }
            }

            #endregion

            #region Read Only Objects

            internal readonly Color OffColour;

            internal readonly Color OnColour;

            internal readonly Color? BorderColour;

            internal readonly bool ToggleState;

            internal readonly float xPos;
            internal readonly float yPos;

            #endregion
        }

        #endregion

        #region internal Variables

        internal static Transform ShortcutMenuTransform = null;

        internal static Transform NewElementsMenuTransform = null;

        internal static QuickMenu QuickMenuObj = null;

        internal static Transform UserInteractMenuTransform = null;

        internal static Transform CustomTransform = null;

        internal static System.Collections.Generic.List<PlagueButton> ButtonsFromThisMod = new System.Collections.Generic.List<PlagueButton>();

        #endregion internal Variables

        #region Main Functions

        #region Slider Creation

        internal class SliderRef {
            internal GameObject SliderObject;
            internal Text SliderText;
        }

        /// <summary>
        /// Creates A Slider At The Given Location. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        /// <param name="Parent">The Parent To Place This Slider In, You Can Use ButtonAPI.ShortcutMenuTransform As A Example Transform.</param>
        /// <param name="OnChanged">The Delegate To Call Upon The Slider Being Changed, This Is Used As: delegate(float val) { }</param>
        /// <param name="X">The Horizontal Position Of The Slider.</param>
        /// <param name="Y">The Vertical Position Of The Slider.</param>
        /// <param name="Text">The Text To Place Above The Slider.</param>
        /// <param name="InitialValue">The Initial Value Set On The Slider.</param>
        /// <param name="MaxValue">The Max Value The Slider Can Go.</param>
        /// <param name="MinValue">The Minimum Value The Slider Can Go.</param>
        /// <returns>ButtonAPI.SliderRef</returns>
        internal static SliderRef CreateSlider(Transform Parent, Action<float> OnChanged, float X, float Y, string Text,
            float InitialValue, float MaxValue, float MinValue) {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ButtonAPIHandler.InitTransforms();

            //Prevent Weird Bugs Due To A Invalid Parent - Set It To The Main QuickMenu
            if (Parent == null) {
                Parent = CustomTransform;
            }

            //Template Button For Positioning
            var gameObject = CreateButton(ButtonType.Default, "slider_element_" + X + Y,
                "", X, Y, null, delegate (bool a) { }, Color.white, Color.magenta, Color.magenta, true, false, false,
                false, null).gameObject;

            gameObject.SetActive(value: false);

            //Slider
            var transform = UnityEngine.Object.Instantiate(
                VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Settings/AudioDevicePanel/VolumeSlider"), ShortcutMenuTransform);

            transform.transform.localScale = new Vector3(1f, 1f, 1f);
            transform.transform.localPosition = gameObject.gameObject.transform.localPosition;

            UnityEngine.Object.Destroy(gameObject);

            transform.transform.localPosition -= new Vector3(70f, 0);

            transform.GetComponentInChildren<RectTransform>().anchorMin += new Vector2(0.06f, 0f);
            transform.GetComponentInChildren<RectTransform>().anchorMax += new Vector2(0.1f, 0f);

            transform.GetComponentInChildren<Slider>().value = InitialValue;
            transform.GetComponentInChildren<Slider>().maxValue = MaxValue;
            transform.GetComponentInChildren<Slider>().minValue = MinValue;
            transform.GetComponentInChildren<Slider>().Set(InitialValue);

            var info = Assembly.GetExecutingAssembly().GetCustomAttribute<MelonInfoAttribute>();

            //Change Internal Names & Sanitize Them
            transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "Slider_" + X + "_" + Y + "_" + Parent.name.Replace(" ", "_");
            transform.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "Slider_" + X + "_" + Y + "_" + Parent.name.Replace(" ", "_");

            //Text
            var gameObject2 = new GameObject("Text");
            gameObject2.transform.SetParent(ShortcutMenuTransform, worldPositionStays: false);
            var text2 = gameObject2.AddComponent<Text>();
            text2.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text2.fontSize = 64;
            text2.text = Text + " (" + InitialValue.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")) + ")";
            text2.transform.localPosition = transform.transform.localPosition;
            text2.transform.localPosition += new Vector3(0, 75, 0);
            text2.enabled = true;
            text2.GetComponent<RectTransform>().sizeDelta = new Vector2(text2.fontSize * Text.Length, 100f);
            text2.alignment = TextAnchor.MiddleCenter;

            //Change Internal Names & Sanitize Them
            text2.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "Slider_" + X + "_" + Y + "_" + Parent.name.Replace(" ", "_");
            text2.transform.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "Slider_" + X + "_" + Y + "_" + Parent.name.Replace(" ", "_");

            gameObject2.transform.SetParent(Parent, worldPositionStays: true);
            transform.SetParent(Parent, true);

            transform.GetComponentInChildren<Slider>().onValueChanged = new Slider.SliderEvent();

            transform.GetComponentInChildren<Slider>().onValueChanged.AddListener(new Action<float>((val) => {
                try {
                    OnChanged.Invoke(val);
                } catch (Exception ex) {
                    MelonLogger.Error("An Exception Occured In The OnClick Of The " + Text + " Slider -->\n" + ex);
                }
            }));

            //Update Text
            transform.GetComponentInChildren<Slider>().onValueChanged.AddListener((Action<float>)delegate (float val) {
                text2.text = Text + " (" + val.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")) + ")";

                transform.GetComponentInChildren<Slider>().transform.Find("Fill Area/Label").GetComponent<Text>().text = RangeConv(Convert.ToInt32(val), MinValue, MaxValue, 0, 100) + "%";
            });

            transform.GetComponentInChildren<Slider>().transform.Find("Fill Area/Label").GetComponent<Text>().text = RangeConv(Convert.ToInt32(InitialValue), MinValue, MaxValue, 0, 100) + "%";

            return new SliderRef { SliderObject = transform.gameObject, SliderText = text2 };
        }

        private static int RangeConv(float input, float MinPossibleInput, float MaxPossibleInput, float MinConv, float MaxConv) {
            return (int)((((input - MinPossibleInput) * (MaxConv - MinConv)) / (MaxPossibleInput - MinPossibleInput)) + MinConv);
        }

        #endregion

        #region InputField Creation

        /// <summary>
        /// Creates A Input Field And Returns The Object Of The Input Field Made. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        /// <param name="PlaceHolderText">The Text To Show Before Anything Is Typed By The User.</param>
        /// <param name="Y">The Vertical Position Of The InputField</param>
        /// <param name="Parent">The Parent To Place This InputField In, You Can Use ButtonAPI.ShortcutMenuTransform As A Example Transform.</param>
        /// <param name="TextChanged">The Delegate To Call Upon Text Being Changed, This Is Used As: delegate(string text) { }</param>
        /// <param name="OnEnterKeyPressed">What Is Called When The Enter Key Is Pressed With The Control Visible, This Is Used As: delegate() { }</param>
        /// <param name="OnCloseMenu">What Is Called When The QuickMenu Is Closed With The Control Visible, This Is Used As: delegate() { }</param>
        /// <returns>UnityEngine.UI.InputField</returns>
        internal static InputField CreateInputField(string PlaceHolderText, VerticalPosition Y, Transform Parent, Action<string> TextChanged, Action OnEnterKeyPressed = null, Action OnCloseMenu = null) {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ButtonAPIHandler.InitTransforms();

            //Prevent Weird Bugs Due To A Invalid Parent - Set It To The Main QuickMenu
            if (Parent == null) {
                Parent = CustomTransform;
            }

            //Prevent Weird Bugs Due To A Invalid Parent - Set It To The Main QuickMenu
            if (Parent == null) {
                Parent = QuickMenu.prop_QuickMenu_0.transform;
            }

            //Get The Transform Of InputField Of The Input Popup - Which We Are Going To Use As Our Template
            var inputfield = UnityEngine.Object.Instantiate(VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.field_Public_VRCUiPopupInput_0.GetComponentInChildren<InputField>());

            var info = Assembly.GetExecutingAssembly().GetCustomAttribute<MelonInfoAttribute>();

            //Change Internal Names & Sanitize Them
            inputfield.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "InputField_" + (float)Y + "_" + Parent.name.Replace(" ", "_");
            inputfield.transform.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + "InputField_" + (float)Y + "_" + Parent.name.Replace(" ", "_");

            var SliderFreezer = inputfield.gameObject.AddComponent<FreezeControls>();

            if (OnEnterKeyPressed != null) {
                SliderFreezer.OnEnterKeyPressed = OnEnterKeyPressed;
            }

            if (OnCloseMenu != null) {
                SliderFreezer.OnExit = OnCloseMenu;
            }

            inputfield.placeholder.GetComponent<Text>().text = PlaceHolderText;

            inputfield.placeholder.GetComponent<Text>().alignment = TextAnchor.UpperLeft;

            inputfield.textComponent.alignment = TextAnchor.UpperLeft;

            var tempcolorblock = inputfield.colors;

            tempcolorblock.normalColor = Color.magenta;
            tempcolorblock.highlightedColor = Color.magenta;
            tempcolorblock.pressedColor = Color.magenta;

            inputfield.colors = tempcolorblock;

            //InputField Position Calculation
            var num =
                (GameObject.Find("/UserInterface/QuickMenu").GetComponent<QuickMenu>().transform.Find("UserInteractMenu/ForceLogoutButton").localPosition.x -
                 GameObject.Find("/UserInterface/QuickMenu").GetComponent<QuickMenu>().transform.Find("UserInteractMenu/BanButton").localPosition.x) / 3.9f;

            //Define Position To Place This InputField In The Parent, Appended To By The TopOrBottom Switch
            inputfield.transform.localPosition = new Vector3(-1185f, inputfield.transform.localPosition.y + num * ((float)Y - 1.57f), inputfield.transform.localPosition.z);

            //Alignment Due To Scaling
            inputfield.GetComponent<RectTransform>().sizeDelta = new Vector2(1675f, 410f);

            inputfield.transform.localPosition -= new Vector3(-1185f, 130f, 0);

            //Define Where To Put This InputField
            inputfield.transform.SetParent(ShortcutMenuTransform, worldPositionStays: false);

            inputfield.onValueChanged = new InputField.OnChangeEvent();
            inputfield.onValueChanged.AddListener(new Action<string>((text) => {
                try {
                    TextChanged.Invoke(text);
                } catch (Exception ex) {
                    MelonLogger.Error("An Exception Occured In The OnValueChanged Of A InputField -->\n" + ex);
                }
            }));

            inputfield.transform.SetParent(Parent, worldPositionStays: true);

            return inputfield;
        }

        #endregion

        #region Text Creation

        /// <summary>
        /// Creates Text With A Lot Of Customization And Returns The PlagueButton Of The Text Made. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        ///
        ///     <example>
        ///     Here Is An Example Of How To Use This:
        ///         <code>
        ///         ButtonAPI.CreateText(ButtonAPI.ButtonType.Toggle, ButtonAPI.SizeType.ButtonSize, "Toggle Pickups", "Toggles All Pickups In The Current Instance.", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.TopButton, null, true, true, delegate (bool a)
        ///            {
        ///                //Do Something Here
        ///            }, false, Color.magenta, Color.white);
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="ButtonType">
        /// The Type Of Text You Wish To Create.
        /// </param>
        /// <param name="SizeType">
        /// The Size Type OF The Text.
        /// </param>
        /// <param name="Text">
        /// The Main Text In The Text.
        /// </param>
        /// <param name="ToolTip">
        /// The Text That Appears At The Top Of The Menu When You Hover Over The Text.
        /// </param>
        /// <param name="X">
        /// The Horizontal Position Of The Text.
        /// </param>
        /// <param name="Y">
        /// The Vertical Position Of The Text.
        /// </param>
        /// <param name="Parent">
        /// The Transform Of The GameObject You Wish To Put Your Text In (You Can Set This As Just "null" For The Main ShortcutMenu).
        /// </param>
        /// <param name="Clickable">
        /// Whether The Text Should Be Clickable Or Just Plain Text.
        /// </param>
        /// <param name="TextListener">
        /// What You Want The Text To Do When You Click It - Must Be delegate(bool nameofboolhere) {  }.
        /// </param>
        /// <param name="OffColour">
        /// The Colour You Want The Main Text Of The Text You Defined Earlier To Change Into If This Text Is Toggled Off.
        /// </param>
        /// <param name="OnColour">
        /// The Colour You Want The Main Text Of The Text You Defined Earlier To Change Into If This Text Is Toggled On.
        /// </param>
        /// <param name="CurrentToggleState">
        /// The Toggle State You Want The Text To Be On Creation.
        /// </param>
        /// <param name="ChangeColourOnClick">
        /// Only Set This To False If You Are Setting The Text's Text Colour In The TextListener - Or The Toggling Will Break!
        /// </param>
        internal static PlagueButton CreateText(ButtonType ButtonType, SizeType SizeType, string Text, string ToolTip, HorizontalPosition X,
            VerticalPosition Y, Transform Parent, bool Clickable, bool ChangeColourOnClick, Action<bool> TextListener, bool CurrentToggleState, Color OnColour, Color OffColour) {
            var button = CreateText(ButtonType, SizeType, Text, ToolTip, (float)X,
             (float)Y, Parent, Clickable, ChangeColourOnClick, TextListener, CurrentToggleState, OnColour, OffColour);

            return button;
        }

        /// <summary>
        /// Creates Text With A Lot Of Customization And Returns The PlagueButton Of The Text Made. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        ///
        ///     <example>
        ///     Here Is An Example Of How To Use This:
        ///         <code>
        ///         ButtonAPI.CreateText(ButtonAPI.ButtonType.Toggle, ButtonAPI.SizeType.ButtonSize, "Toggle Pickups", "Toggles All Pickups In The Current Instance.", ButtonAPI.HorizontalPosition.FirstButtonPos, ButtonAPI.VerticalPosition.TopButton, null, true, true, delegate (bool a)
        ///            {
        ///                //Do Something Here
        ///            }, false, Color.magenta, Color.white);
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="ButtonType">
        /// The Type Of Text You Wish To Create.
        /// </param>
        /// <param name="SizeType">
        /// The Size Type OF The Text.
        /// </param>
        /// <param name="Text">
        /// The Main Text In The Text.
        /// </param>
        /// <param name="ToolTip">
        /// The Text That Appears At The Top Of The Menu When You Hover Over The Text.
        /// </param>
        /// <param name="X">
        /// The Horizontal Position Of The Text.
        /// </param>
        /// <param name="Y">
        /// The Vertical Position Of The Text.
        /// </param>
        /// <param name="Parent">
        /// The Transform Of The GameObject You Wish To Put Your Text In (You Can Set This As Just "null" For The Main ShortcutMenu).
        /// </param>
        /// <param name="Clickable">
        /// Whether The Text Should Be Clickable Or Just Plain Text.
        /// </param>
        /// <param name="TextListener">
        /// What You Want The Text To Do When You Click It - Must Be delegate(bool nameofboolhere) {  }.
        /// </param>
        /// <param name="OffColour">
        /// The Colour You Want The Main Text Of The Text You Defined Earlier To Change Into If This Text Is Toggled Off.
        /// </param>
        /// <param name="OnColour">
        /// The Colour You Want The Main Text Of The Text You Defined Earlier To Change Into If This Text Is Toggled On.
        /// </param>
        /// <param name="CurrentToggleState">
        /// The Toggle State You Want The Text To Be On Creation.
        /// </param>
        /// <param name="ChangeColourOnClick">
        /// Only Set This To False If You Are Setting The Text's Text Colour In The TextListener - Or The Toggling Will Break!
        /// </param>
        internal static PlagueButton CreateText(ButtonType ButtonType, SizeType SizeType, string Text, string ToolTip, float X,
            float Y, Transform Parent, bool Clickable, bool ChangeColourOnClick, Action<bool> TextListener, bool CurrentToggleState, Color OnColour, Color OffColour) {
            var button = CreateButton(ButtonType, Text, ToolTip, X, Y, Parent, TextListener, OffColour, OnColour, null, false, false, false, CurrentToggleState, null, true);

            UnityEngine.Object.Destroy(button.image);

            if (SizeType == SizeType.QuickMenuSize) {
                button.rect.sizeDelta += new Vector2(5000f, 0f);
            }

            if (!Clickable) {
                UnityEngine.Object.Destroy(button.button);
            }

            button.text.alignment = TextAnchor.MiddleCenter;

            return button;
        }

        #endregion

        #region Button Creation

        /// <summary>
        /// Creates A Button With A Lot Of Customization And Returns The PlagueButton Of The Button Made. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        ///
        ///     <example>
        ///     Here Is An Example Of How To Use This:
        ///         <code>
        ///         ButtonAPI.CreateButton(ButtonAPI.ButtonType.Toggle, "Toggle Pickups", "Toggles All Pickups In The Current Instance.", ButtonAPI.HorizontalPosition.FirstButtonPos,      ButtonAPI.VerticalPosition.TopButton, null, delegate (bool a)
        ///            {
        ///                //Do Something Here
        ///            }, Color.white, Color.magenta, null, false, false, true);
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="ButtonType">
        /// The Type Of Button You Wish To Create.
        /// </param>
        /// <param name="Text">
        /// The Main Text In The Button
        /// </param>
        /// <param name="ToolTip">
        /// The Text That Appears At The Top Of The Menu When You Hover Over The Button.
        /// </param>
        /// <param name="X">
        /// The Horizontal Position Of The Button.
        /// </param>
        /// <param name="Y">
        /// The Vertical Position Of The Button.
        /// </param>
        /// <param name="Parent">
        /// The Transform Of The GameObject You Wish To Put Your Button In (You Can Set This As Just "null" For The Main ShortcutMenu).
        /// </param>
        /// <param name="ButtonListener">
        /// What You Want The Button To Do When You Click It - Must Be delegate(bool nameofboolhere) {  }.
        /// </param>
        /// <param name="ToggledOffTextColour">
        /// The Colour You Want The Main Text Of The Button You Defined Earlier To Change Into If This Button Is Toggled Off.
        /// </param>
        /// <param name="ToggledOnTextColour">
        /// The Colour You Want The Main Text Of The Button You Defined Earlier To Change Into If This Button Is Toggled On.
        /// </param>
        /// <param name="BorderColour">
        /// The Colour You Want The Border Of The Button To Be (You Can Set This As Just "null" For The Default Colour That The ShortcutMenu Currently Is!).
        /// </param>
        /// <param name="FullSizeButton">
        /// If You Want This Button To Be A Full Size Normal Button, Or Half Sized (False) - Default Is Half Sized.
        /// </param>
        /// <param name="BottomHalf">
        /// If You Want This Button To Be On The Bottom Half Of The VericalPosition You Chose Or The Top - Default Is Bottom Half.
        /// </param>
        /// <param name="HalfHorizontally">
        /// If You Want This Button To Have It's Size Cut In Half Horizontally.
        /// </param>
        /// <param name="CurrentToggleState">
        /// The Toggle State You Want The Button To Be On Creation.
        /// </param>
        /// <param name="SpriteForButton">
        /// The Image Sprite You Want To Apply To The Button.
        /// </param>
        /// <param name="ChangeColourOnClick">
        /// Only Set This To False If You Are Setting The Button's Text Colour In The ButtonListener - Or The Toggling Will Break!
        /// </param>
        /// <param name="ConditionalOrSinglePressKeyBind">
        /// A KeyCode Of A Conditional Such As Ctrl Or A Single Press KeyBind
        /// </param>
        /// <param name="OptionalKeyBind">
        /// Optional KeyCode Of The KeyBind To Be Pressed With The Conditional Just Before
        /// </param>
        internal static PlagueButton CreateButton(ButtonType ButtonType, string Text, string ToolTip, HorizontalPosition X,
            VerticalPosition Y, Transform Parent, Action<bool> ButtonListener, Color ToggledOffTextColour,
            Color ToggledOnTextColour, Color? BorderColour, bool FullSizeButton = false, bool BottomHalf = true,
            bool HalfHorizontally = false, bool CurrentToggleState = false, Sprite SpriteForButton = null,
            bool ChangeColourOnClick = true, KeyCode? ConditionalOrSinglePressKeyBind = null, KeyCode? OptionalKeyBind = null) {
            //Prevent Weird Bugs Due To A Invalid Parent - Set It To The Main QuickMenu
            if (Parent == null) {
                Parent = CustomTransform;
            }

            var button = CreateButton(ButtonType, Text, ToolTip, (float)X, (float)Y, Parent, ButtonListener, ToggledOffTextColour, ToggledOnTextColour, BorderColour, FullSizeButton, BottomHalf, HalfHorizontally, CurrentToggleState, SpriteForButton, ChangeColourOnClick, ConditionalOrSinglePressKeyBind, OptionalKeyBind);

            //Return The GameObject For Handling It Elsewhere
            return button;
        }

        /// <summary>
        /// Creates A Button With A Lot Of Customization And Returns The PlagueButton Of The Button Made. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        ///     <para>
        ///     As You Type Arguments Within This Method You Will See What Each Argument Does Here.
        ///     </para>
        ///
        ///     <example>
        ///     Here Is An Example Of How To Use This:
        ///         <code>
        ///         ButtonAPI.CreateButton(ButtonAPI.ButtonType.Toggle, "Toggle Pickups", "Toggles All Pickups In The Current Instance.", ButtonAPI.HorizontalPosition.FirstButtonPos,      ButtonAPI.VerticalPosition.TopButton, null, delegate (bool a)
        ///            {
        ///                //Do Something Here
        ///            }, Color.white, Color.magenta, null, false, false, true);
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="ButtonType">
        /// The Type Of Button You Wish To Create.
        /// </param>
        /// <param name="Text">
        /// The Main Text In The Button
        /// </param>
        /// <param name="ToolTip">
        /// The Text That Appears At The Top Of The Menu When You Hover Over The Button.
        /// </param>
        /// <param name="X">
        /// The Horizontal Position Of The Button.
        /// </param>
        /// <param name="Y">
        /// The Vertical Position Of The Button.
        /// </param>
        /// <param name="Parent">
        /// The Transform Of The GameObject You Wish To Put Your Button In (You Can Set This As Just "null" For The Main ShortcutMenu).
        /// </param>
        /// <param name="ButtonListener">
        /// What You Want The Button To Do When You Click It - Must Be delegate(bool nameofboolhere) {  }.
        /// </param>
        /// <param name="ToggledOffTextColour">
        /// The Colour You Want The Main Text Of The Button You Defined Earlier To Change Into If This Button Is Toggled Off.
        /// </param>
        /// <param name="ToggledOnTextColour">
        /// The Colour You Want The Main Text Of The Button You Defined Earlier To Change Into If This Button Is Toggled On.
        /// </param>
        /// <param name="BorderColour">
        /// The Colour You Want The Border Of The Button To Be (You Can Set This As Just "null" For The Default Colour That The ShortcutMenu Currently Is!).
        /// </param>
        /// <param name="FullSizeButton">
        /// If You Want This Button To Be A Full Size Normal Button, Or Half Sized (False) - Default Is Half Sized.
        /// </param>
        /// <param name="BottomHalf">
        /// If You Want This Button To Be On The Bottom Half Of The VericalPosition You Chose Or The Top - Default Is Bottom Half.
        /// </param>
        /// <param name="HalfHorizontally">
        /// If You Want This Button To Have It's Size Cut In Half Horizontally.
        /// </param>
        /// <param name="CurrentToggleState">
        /// The Toggle State You Want The Button To Be On Creation.
        /// </param>
        /// <param name="SpriteForButton">
        /// The Image Sprite You Want To Apply To The Button.
        /// </param>
        /// <param name="ChangeColourOnClick">
        /// Only Set This To False If You Are Setting The Button's Text Colour In The ButtonListener - Or The Toggling Will Break!
        /// </param>
        /// <param name="ConditionalOrSinglePressKeyBind">
        /// A KeyCode Of A Conditional Such As Ctrl Or A Single Press KeyBind
        /// </param>
        /// <param name="OptionalKeyBind">
        /// Optional KeyCode Of The KeyBind To Be Pressed With The Conditional Just Before
        /// </param>
        internal static PlagueButton CreateButton(ButtonType ButtonType, string Text, string ToolTip, float X, float Y,
            Transform Parent, Action<bool> ButtonListener, Color ToggledOffTextColour, Color ToggledOnTextColour,
            Color? BorderColour, bool FullSizeButton = false, bool BottomHalf = true, bool HalfHorizontally = false,
            bool CurrentToggleState = false, Sprite SpriteForButton = null, bool ChangeColourOnClick = true, KeyCode? ConditionalOrSinglePressKeyBind = null, KeyCode? OptionalKeyBind = null) {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ButtonAPIHandler.InitTransforms();

            //Prevent Weird Bugs Due To A Invalid Parent - Set It To The Main QuickMenu
            if (Parent == null) {
                Parent = CustomTransform;
            }

            //Get The Transform Of The Settings Button - Which We Are Going To Use As Our Template
            var transform = UnityEngine.Object
                .Instantiate(GameObject.Find("/UserInterface/QuickMenu").GetComponent<QuickMenu>().transform.Find("ShortcutMenu/SettingsButton").gameObject)
                .transform;

            var plagueButton = new PlagueButton(ToggledOffTextColour,
                ToggledOnTextColour,
                BorderColour,
                CurrentToggleState, X, Y);

            //Button Position Calculation
            var num =
                (GameObject.Find("/UserInterface/QuickMenu").GetComponent<QuickMenu>().transform.Find("UserInteractMenu/ForceLogoutButton").localPosition.x -
                GameObject.Find("/UserInterface/QuickMenu").GetComponent<QuickMenu>().transform.Find("UserInteractMenu/BanButton").localPosition.x) / 3.9f;

            var info = Assembly.GetExecutingAssembly().GetCustomAttribute<MelonInfoAttribute>();

            //Change Internal Names & Sanitize Them
            transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + Text.Replace(" ", "_".Replace(",", "_").Replace(":", "_") + "_Button_" + X + "_" + Y + "_" + Parent.name);
            transform.transform.name = "PlagueButtonAPI_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + Text.Replace(" ", "_".Replace(",", "_").Replace(":", "_Button_") + "_" + X + "_" + Y + "_" + Parent.name);

            //Define Position To Place This Button In The Parent, Appended To Later
            if (BottomHalf || FullSizeButton) {
                if (Parent == UserInteractMenuTransform) {
                    transform.localPosition = new Vector3(transform.localPosition.x + num * X,
                        transform.localPosition.y + num * (Y - 2.95f), transform.localPosition.z);
                } else {
                    transform.localPosition = new Vector3(transform.localPosition.x + num * X,
                        transform.localPosition.y + num * (Y - 1.95f), transform.localPosition.z);
                }
            } else {
                if (Parent == UserInteractMenuTransform) {
                    transform.localPosition = new Vector3(transform.localPosition.x + num * X,
                        transform.localPosition.y + num * (Y - 2.45f), transform.localPosition.z);
                } else {
                    transform.localPosition = new Vector3(transform.localPosition.x + num * X,
                        transform.localPosition.y + num * (Y - 1.45f), transform.localPosition.z);
                }
            }

            //Define Where To Put This Button Temporarily
            transform.SetParent(ShortcutMenuTransform, worldPositionStays: false);

            //Set Text, Tooltip & Colours
            transform.GetComponentInChildren<Text>().supportRichText = true;
            transform.GetComponentInChildren<Text>().text = Text;
            transform.GetComponentInChildren<UiTooltip>().field_Public_String_0 = ToolTip;
            transform.GetComponentInChildren<UiTooltip>().field_Public_String_1 = ToolTip;

            if (CurrentToggleState && ButtonType != ButtonType.Default) {
                transform.GetComponentInChildren<Text>().color = ToggledOnTextColour;
            } else {
                transform.GetComponentInChildren<Text>().color = ToggledOffTextColour;
            }

            //Set The Button's Border Colour
            if (BorderColour != null) {
                transform.GetComponentInChildren<Image>().color = (Color)BorderColour;
            }

            //Size Scaling & Repositioning
            if (!FullSizeButton) {
                transform.localPosition +=
                    new Vector3(0f, transform.GetComponent<RectTransform>().sizeDelta.y / 5f, 0f);
                transform.localPosition -=
                    new Vector3(0f, transform.GetComponent<RectTransform>().sizeDelta.y / 2f, 0f);
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(
                    transform.GetComponent<RectTransform>().sizeDelta.x,
                    transform.GetComponent<RectTransform>().sizeDelta.y / 2f);
            } else {
                transform.localPosition -= new Vector3(0f, 20f, 0f);
            }

            if (HalfHorizontally) {
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(
                    transform.GetComponent<RectTransform>().sizeDelta.x / 2f,
                    transform.GetComponent<RectTransform>().sizeDelta.y);

                transform.localPosition -= new Vector3(transform.GetComponent<RectTransform>().sizeDelta.x / 2f, 0f, 0f);
            }

            if (SpriteForButton != null) {
                transform.GetComponentInChildren<Image>().sprite = SpriteForButton;
            }

            //Remove Any Previous Events
            transform.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();

            //Listener Redirection - To Get Around AddListener Not Passing A State Bool Due To Being A onClick Event
            transform.GetComponent<Button>().onClick.AddListener(new Action(() => {
                try {
                    if (ButtonType == ButtonType.Toggle) {
                        ButtonListener?.Invoke(transform.GetComponentInChildren<Text>().color != ToggledOnTextColour);
                    } else {
                        ButtonListener?.Invoke(true);
                    }
                } catch (Exception ex) {
                    MelonLogger.Error("An Exception Occured In The OnClick Of The " + Text + (ButtonType == ButtonType.Toggle ? " Toggle" : " Button") + " -->\n" + ex);
                }
            }));

            if (ButtonType == ButtonType.Toggle) {
                //Set The Text Colour To The Toggle State, ToggledOnTextColour Being Toggled On
                transform.GetComponent<Button>().onClick.AddListener(new Action(() => {
                    if (transform.GetComponentInChildren<Text>().color == ToggledOnTextColour) {
                        transform.GetComponentInChildren<Text>().color = ToggledOffTextColour;
                    } else {
                        transform.GetComponentInChildren<Text>().color = ToggledOnTextColour;
                    }
                }));
            }

            //Update
            plagueButton.gameObject = transform.gameObject;
            plagueButton.button = transform.GetComponent<Button>();
            plagueButton.text = transform.GetComponentInChildren<Text>();
            plagueButton.tooltip = transform.GetComponentInChildren<UiTooltip>();
            plagueButton.image = transform.GetComponentInChildren<Image>();
            plagueButton.rect = transform.GetComponent<RectTransform>();

            ButtonsFromThisMod.Add(plagueButton);

            //Define Where To Put This Button
            transform.SetParent(Parent, worldPositionStays: true);

            if (ConditionalOrSinglePressKeyBind != null) {
                RegisteredKeyBinds.Add(plagueButton.button, Tuple.Create((KeyCode)ConditionalOrSinglePressKeyBind, OptionalKeyBind));
            }

            //Return The GameObject For Handling It Elsewhere
            return plagueButton;
        }

        #endregion Button Creation

        #region Sub Menu Creation And Handling

        /// <summary>
        /// Creates A Empty Page For Adding Buttons To, If The Page Already Exists, This Will Return It. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="name">
        /// The Name You Want To Give The Page/Find Internally.
        /// </param>
        /// <param name="OptionalTitleText">Optional Text To Display At The Top Of The Page.</param>
        /// <param name="OptionalTitleTextTooltip">Optional Text To Display When Hovering Over The Text Defined Just Before This.</param>
        /// <param name="OptionalTitleTextOnColour">Optional Toggled On Colour Of The Text Defined Previous.</param>
        /// <param name="OptionalTitleTextOffColour">Optional Toggled Off Colour Of The Text Defined Previous.</param>
        /// <param name="OptionalTitleTextOnClick">Optional Function To Run On Selecting The Text Defined Previous</param>
        /// <returns></returns>
        internal static GameObject MakeEmptyPage(string name, string OptionalTitleText = "", string OptionalTitleTextTooltip = "", Color? OptionalTitleTextOnColour = null, Color? OptionalTitleTextOffColour = null, Action<bool> OptionalTitleTextOnClick = null) {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
                MelonLogger.Msg("Your Empty Page Name Cannot Be Empty!");
                return null;
            }

            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ButtonAPIHandler.InitTransforms();

            var info = Assembly.GetExecutingAssembly().GetCustomAttribute<MelonInfoAttribute>();

            //If This Page Already Exists, Return It
            for (var i = 0; i < SubMenus.Count; i++) {
                var menu = SubMenus[i];

                if (menu.name == "PlagueButtonAPI_SubMenu_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + name) {
                    return menu;
                }
            }

            //Clone The ShortcutMenu
            var transform = UnityEngine.Object.Instantiate(ShortcutMenuTransform.gameObject).transform;

            //Change Internal Names
            transform.transform.name = "PlagueButtonAPI_SubMenu_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + name;
            transform.name = "PlagueButtonAPI_SubMenu_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + name;

            //Remove All Buttons
            for (var i = 0; i < transform.childCount; i++) {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }

            //Organise Hierarchy Ree
            if (NewElementsMenuTransform.Find("PlagueButtonAPI") == null) {
                var obj = new GameObject("PlagueButtonAPI");

                obj.transform.SetParent(NewElementsMenuTransform);
            }

            //Make This Page We Cloned A Child Of The NewElementsMenuTransform
            transform.SetParent(NewElementsMenuTransform.Find("PlagueButtonAPI"), worldPositionStays: false);

            //Make This Page We Cloned Inactive By Default
            transform.gameObject.SetActive(value: false);

            //Add It To The Handler
            SubMenus.Add(transform.gameObject);

            //Title Text
            if (!string.IsNullOrEmpty(OptionalTitleText)) {
                CreateText(ButtonType.Toggle, SizeType.QuickMenuSize, OptionalTitleText, OptionalTitleTextTooltip, (float)HorizontalPosition.SecondButtonPos + 0.5f, (float)VerticalPosition.AboveMenu - 0.5f, transform, (OptionalTitleTextOnClick != null), false, OptionalTitleTextOnClick, false, OptionalTitleTextOnColour ?? (OptionalTitleTextOffColour ?? Color.white), OptionalTitleTextOffColour ?? (OptionalTitleTextOnColour ?? Color.white));
            }

            //Return The GameObject For Handling It Elsewhere
            return transform.gameObject;
        }

        /// <summary>
        /// Finds A SubMenu Inside Said Transform Created By My Button API. This Method Will Not Create One Under This Name If Not Found. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="name">
        /// The Name OF The SubMenu To Find.
        /// </param>
        /// <param name="WhereTheSubMenuIsInside">
        /// Where You Placed The SubMenu, Such As The ShortcutMenu Or UserInteractMenu.
        /// </param>
        internal static GameObject FindSubMenu(string name, Transform WhereTheSubMenuIsInside) {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ButtonAPIHandler.InitTransforms();

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
                MelonLogger.Msg("Your SubMenu Name Cannot Be Empty!");
                return null;
            }

            var info = Assembly.GetExecutingAssembly().GetCustomAttribute<MelonInfoAttribute>();

            //Find The SubMenu And Return It
            return WhereTheSubMenuIsInside.Find("PlagueButtonAPI_SubMenu_" + info.Name.Replace(" ", "_") + " By " + info.Author.Replace(" ", "_") + "_" + name).gameObject;
        }

        /// <summary>
        /// Enters The Submenu. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="menu">
        /// The GameObject Of The SubMenu You Want To Enter.
        /// </param>
        internal static void EnterSubMenu(GameObject menu) {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            if (ShortcutMenuTransform.gameObject.active) {
                ShortcutMenuTransform.gameObject.SetActive(false);
            }

            if (UserInteractMenuTransform.gameObject.active) {
                UserInteractMenuTransform.gameObject.SetActive(false);
            }

            if (CustomTransform.gameObject.active) {
                CustomTransform.gameObject.SetActive(false);
            }

            for (var i = 0; i < SubMenus.Count; i++) {
                var Menu = SubMenus[i];
                Menu.SetActive(false);
            }

            if (menu != null) {
                menu.SetActive(true);
            }
        }

        /// <summary>
        /// Closes All SubMenus Created
        /// </summary>
        internal static void CloseAllSubMenus() {
            var QuickMenuObj = GameObject.Find("UserInterface/QuickMenu/");

            if (!QuickMenuObj.GetComponent<ButtonAPIHandler>()) {
                QuickMenuObj.AddComponent<ButtonAPIHandler>();
            }

            ShortcutMenuTransform.gameObject.SetActive(false);
            UserInteractMenuTransform.gameObject.SetActive(false);
            CustomTransform.gameObject.SetActive(false);

            for (var i = 0; i < SubMenus.Count; i++) {
                var Menu = SubMenus[i];
                Menu.SetActive(false);
            }
        }

        #endregion Sub Menu Creation And Handling

        #endregion Main Functions

        #region Internal Enumerations

        /// <summary>
        /// The Horizontal Position Of The Button You Are Creating.
        /// </summary>
        internal enum HorizontalPosition {
            ThreeLeftOfMenu = -6,

            TwoLeftOfMenu = -5,

            LeftOfMenu = -4,

            FirstButtonPos = -3,

            SecondButtonPos = -2,

            ThirdButtonPos = -1,

            FourthButtonPos = 0,

            RightOfMenu = 1,

            TwoRightOfMenu = 2,

            ThreeRightOfMenu = 3
        }

        /// <summary>
        /// The Vertical Position Of The Button You Are Creating.
        /// </summary>
        internal enum VerticalPosition {
            TwoAboveMenu = 6,

            AboveMenu = 5,

            AboveTopButton = 4,

            TopButton = 3,

            SecondButton = 2,

            BottomButton = 1,

            BelowBottomButton = 0,

            TwoBelowBottomButton = 0
        }

        /// <summary>
        /// The Type Of Button You Are Creating.
        /// </summary>
        internal enum ButtonType {
            Default,
            Toggle
        }

        /// <summary>
        /// The Type Of Text Label You Want To Make
        /// </summary>
        internal enum SizeType {
            ButtonSize,
            QuickMenuSize
        }

        #endregion Internal Enumerations

        #region Internal Functions - Not For The End User

        //Any Created Sub Menus By The User Are Stored Here
        internal static List<GameObject> SubMenus = new List<GameObject>();

        /// <summary>
        /// Internal, Do Not Use This!
        /// </summary>
        internal static Dictionary<Button, Tuple<KeyCode, KeyCode?>> RegisteredKeyBinds = new Dictionary<Button, Tuple<KeyCode, KeyCode?>>();

        #endregion Internal Functions - Not For The End User
    }

    #region Extension Methods
    internal static class ButtonAPIExtensions {
        /// <summary>
        /// Sets The Buttons Toggle State. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Wish To Set The Toggle State Of.
        /// </param>
        /// <param name="StateToSetTo">
        /// The Toggle State You Wish To Set This Button To.
        /// </param>
        internal static void SetToggleState(this ButtonAPI.PlagueButton button, bool StateToSetTo) {
            if (button.text != null) {
                button.text.color = button.text.color == button.OnColour ? button.OffColour : button.OnColour;
            }
        }

        /// <summary>
        /// Gets The Buttons Toggle State. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Wish To Get The Toggle State Of.
        /// </param>
        internal static bool GetToggleState(this ButtonAPI.PlagueButton button) {
            return button.ToggleState;
        }

        /// <summary>
        /// Sets The Buttons Text. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Wish To Set The Text Of.
        /// </param>
        /// <param name="text">
        /// The Text You Want To Place On The Button.
        /// </param>
        internal static void SetText(this ButtonAPI.PlagueButton button, string text) {
            if (button.text != null) {
                button.text.text = text;
            }
        }

        /// <summary>
        /// Gets The Buttons Text. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Wish To Get The Text Of.
        /// </param>
        internal static string GetText(this ButtonAPI.PlagueButton button) {
            return button.text != null ? button.text.text : "";
        }

        /// <summary>
        /// Sets The Buttons Tooltip Text. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Wish To Set The Tooltip Of.
        /// </param>
        /// <param name="text">
        /// The Text You Want To Place On The Button's Tooltip.
        /// </param>
        internal static void SetTooltip(this ButtonAPI.PlagueButton button, string text) {
            if (button.tooltip != null) {
                button.tooltip.field_Public_String_0 = text;
                button.tooltip.field_Public_String_1 = text;
            }
        }

        /// <summary>
        /// Gets The Buttons Tooltip Text. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The GameObject Of The Button You Wish To Get The Tooltip Of.
        /// </param>
        internal static string GetTooltip(this ButtonAPI.PlagueButton button) {
            return button.tooltip != null ? button.tooltip.field_Public_String_0 : "";
        }

        /// <summary>
        /// Sets A Button To Be Interactable Or Not. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button To Set The Interactivity Of.
        /// </param>
        /// <param name="state">
        /// If You Want The Button To Be Interactable.
        /// </param>
        internal static void SetInteractivity(this ButtonAPI.PlagueButton button, bool state) {
            if (button.button != null) {
                button.button.interactable = state;
            }
        }

        /// <summary>
        /// Gets If A Button Is Interactable Or Not. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button To Get The Interactivity Of.
        /// </param>
        internal static bool GetInteractivity(this ButtonAPI.PlagueButton button) {
            return button.button == null || button.button.interactable;
        }

        /// <summary>
        /// Sets The Sprite Of A Given Button. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button To Set The Sprite Of.
        /// </param>
        /// <param name="sprite">
        /// The Image Sprite To Apply.
        /// </param>
        internal static void SetSprite(this ButtonAPI.PlagueButton button, Sprite sprite) {
            if (button.image != null) {
                button.image.sprite = sprite;
            }
        }

        /// <summary>
        /// Returns The Sprite Of A Given Button's GameObject. | Created By Plague | Discord Server: http://VRCAntiCrash.com
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button To Pull The Sprite From.
        /// </param>
        internal static Sprite GetSprite(this ButtonAPI.PlagueButton button) {
            return button.image != null ? button.image.sprite : null;
        }

        /// <summary>
        /// Destroys The Button
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button You Want To Destroy
        /// </param>
        /// <returns>
        /// A Bool Indicating If Destroying Was Successful, Or The Button Didn't Exist To Destroy In The First Place.
        /// </returns>
        internal static bool Destroy(this ButtonAPI.PlagueButton button) {
            if (ButtonAPI.ButtonsFromThisMod.Contains(button)) {
                ButtonAPI.ButtonsFromThisMod.Remove(button);
            }

            if (button.gameObject != null) {
                UnityEngine.Object.Destroy(button.gameObject);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves A Component On This Button's GameObject, If It Is Not Found In The Root, It Will Check In Children.
        /// </summary>
        /// <typeparam name="T">
        /// The Component To Retrieve
        /// </typeparam>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <returns>
        /// Tuple - The Bool Being If It Was Found In Root, False Otherwise
        /// </returns>
        internal static Tuple<bool, T> GetComponent<T>(this ButtonAPI.PlagueButton button) {
            var InRoot = false;

            var ReturnableType = button.gameObject.GetComponent<T>();

            if (ReturnableType == null) {
                ReturnableType = button.gameObject.GetComponentInChildren<T>();
            } else {
                InRoot = true;
            }

            return Tuple.Create(InRoot, ReturnableType);
        }

        /// <summary>
        /// Adds A Component To The Root Of The Button's GameObject.
        /// </summary>
        /// <typeparam name="T">
        /// Your Type Which Will Be The Component
        /// </typeparam>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <returns>
        /// The Component Added, Or Null If It Failed
        /// </returns>
        internal static T AddComponent<T>(this ButtonAPI.PlagueButton button) where T : Component {
            try {
                return button.gameObject?.AddComponent<T>();
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Gets If The Button Is Currently Active
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <returns>
        /// A Boolean Indicating Active State
        /// </returns>
        internal static bool IsActive(this ButtonAPI.PlagueButton button) {
            return button.gameObject != null && button.gameObject.active;
        }

        /// <summary>
        /// Sets The Button's Active State, This Only Sets The Button's Main GameObject State, Not Its Children.
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <param name="state">
        /// The State You Want To Set It To
        /// </param>
        internal static void SetActive(this ButtonAPI.PlagueButton button, bool state) {
            button.gameObject?.SetActive(state);
        }

        /// <summary>
        /// Sets The Button & Its Children's Active States
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <param name="state">
        /// The State You Want To Set It To
        /// </param>
        internal static void SetActiveRecursively(this ButtonAPI.PlagueButton button, bool state) {
            button.gameObject?.SetActiveRecursively(state);
        }

        /// <summary>
        /// VRChat's Layers
        /// </summary>
        internal enum VRCLayer {
            Default,
            TransparentFX,
            IgnoreRaycast,
            Empty1,
            Water,
            UI,
            Empty2,
            Empty3,
            Interactive,
            Player,
            PlayerLocal,
            Enviroment,
            UiMenu,
            Pickup,
            PickupNoEnviroment,
            StereoLeft,
            StereoRight,
            Walkthrough,
            MirrorReflection,
            reserved2,
            reserved3,
            reserved4,
            PostProcessing,
            Empty4,
            Empty5,
            Empty6,
            Empty7,
            Empty8,
            Empty9,
            Empty10,
            Empty11,
            Empty12
        }

        /// <summary>
        /// Sets The Layer(s) Of The Button
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <param name="layers">
        /// The Array Of VRCLayer(s) You Decide, Made With new VRCLayer[] { VRCLayer.LayerHere }
        /// </param>
        internal static void SetLayers(this ButtonAPI.PlagueButton button, VRCLayer[] layers) {
            if (button.gameObject == null) {
                return;
            }

            var FinalLayer = 0;

            for (var i = 0; i < layers.Length; i++) {
                if (FinalLayer == 0) {
                    FinalLayer = (1 << (int)layers[i]);
                } else {
                    FinalLayer = (FinalLayer | (1 << (int)layers[i]));
                }
            }

            button.gameObject.layer = FinalLayer;

            foreach (var trans in button.gameObject.GetComponentsInChildren<Transform>(true)) {
                if (trans != null && trans.gameObject != null) {
                    trans.gameObject.layer = FinalLayer;
                }
            }
        }

        /// <summary>
        /// Gets The Layer(s) This Button Is On
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <returns>
        /// Integer[] Representing The Layer's Original Layer Numbers.
        /// </returns>
        internal static VRCLayer[] GetLayers(this ButtonAPI.PlagueButton button) {
            var LayersFound = new List<VRCLayer>();

            for (var i = 0; i < Enum.GetValues(typeof(VRCLayer)).Length; i++) {
                if ((button.gameObject.layer | 1 << i) > 0) {
                    LayersFound.Add((VRCLayer)i);
                }
            }

            return LayersFound.ToArray();
        }

        /// <summary>
        /// Sets The Button's Tag
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        /// <param name="tag">
        /// The String Of The Tag You Want To Set.
        /// </param>
        internal static void SetTag(this ButtonAPI.PlagueButton button, string tag) {
            if (button.gameObject != null) {
                button.gameObject.tag = tag;
            }
        }

        /// <summary>
        /// Gets The Button's Tag
        /// </summary>
        /// <param name="button">
        /// The PlagueButton Of The Button
        /// </param>
        internal static string GetTag(this ButtonAPI.PlagueButton button) {
            return button.gameObject != null ? button.gameObject.tag : "";
        }
    }
    #endregion

    #region Custom Components

    [RegisterTypeInIl2Cpp]
    internal class ButtonAPIHandler : MonoBehaviour {
        public ButtonAPIHandler(IntPtr instance) : base(instance) { }

        private static float HandlerRoutineDelay = 0f;

        /// <summary>
        /// Initiates The Transform Object References - Do Not Call This!
        /// </summary>
        internal static void InitTransforms() {
            if (ButtonAPI.ShortcutMenuTransform == null) {
                ButtonAPI.ShortcutMenuTransform = GameObject.Find("/UserInterface/QuickMenu/ShortcutMenu").transform;

                ButtonAPI.QuickMenuObj = ButtonAPI.ShortcutMenuTransform.parent.GetComponent<QuickMenu>();

                if (ButtonAPI.CustomTransform == null) {
                    ButtonAPI.CustomTransform = ButtonAPI.ShortcutMenuTransform;
                }
            }

            if (ButtonAPI.NewElementsMenuTransform == null) {
                ButtonAPI.NewElementsMenuTransform =
                    GameObject.Find("/UserInterface/QuickMenu/QuickMenu_NewElements").transform;
            }

            if (ButtonAPI.UserInteractMenuTransform == null) {
                ButtonAPI.UserInteractMenuTransform =
                    GameObject.Find("/UserInterface/QuickMenu/UserInteractMenu").transform;
            }
        }

        void Update() {
            //If User Has Loaded A World
            if (RoomManager.prop_Boolean_3) // This Seems To Go Splat Sometimes?
            {
                if (ButtonAPI.SubMenus != null && ButtonAPI.SubMenus.Count > 0 && Time.time > HandlerRoutineDelay) {
                    HandlerRoutineDelay = Time.time + 0.2f;

                    if (ButtonAPI.QuickMenuObj == null || ButtonAPI.ShortcutMenuTransform == null || ButtonAPI.UserInteractMenuTransform == null || ButtonAPI.CustomTransform == null) {
                        MelonLogger.Error("[PlagueButtonAPI] A NullRef Was Prevented In SubMenuHandler()! Either Recompile Your Mod Or Talk To Plague! NOTE: The ButtonAPI Will Attempt To Auto-Fix This For You, This May Or May Not Work!");
                        InitTransforms();
                        return;
                    }

                    for (var i = 0; i < ButtonAPI.SubMenus.Count; i++) {
                        var Menu = ButtonAPI.SubMenus[i];

                        if (Menu.activeSelf) // Is In This SubMenu
                        {
                            //If QuickMenu Was Closed
                            if (!ButtonAPI.QuickMenuObj.prop_Boolean_0) {
                                //Hide SubMenu
                                Menu.SetActive(false);
                            }

                            //If QuickMenu Is Open Normally When In A SubMenu (Aka When It Shouldn't Be) - This Fixes The Menu Breaking When A Player Joins
                            else if (ButtonAPI.ShortcutMenuTransform.gameObject.active || ButtonAPI.UserInteractMenuTransform.gameObject.active || ButtonAPI.CustomTransform.gameObject.active) {
                                ButtonAPI.ShortcutMenuTransform.gameObject.SetActive(false);
                                ButtonAPI.UserInteractMenuTransform.gameObject.SetActive(false);
                                ButtonAPI.CustomTransform.gameObject.SetActive(false);
                            }

                            break;
                        }
                    }
                }
            }

            if (ButtonAPI.RegisteredKeyBinds.Count > 0) {
                foreach (var ButtonAndBinds in ButtonAPI.RegisteredKeyBinds) {
                    if (ButtonAndBinds.Value.Item2 != null) {
                        if (Input.GetKey(ButtonAndBinds.Value.Item1) && Input.GetKeyDown((KeyCode)ButtonAndBinds.Value.Item2)) {
                            ButtonAndBinds.Key?.onClick?.Invoke();
                        }
                    } else {
                        if (Input.GetKeyDown(ButtonAndBinds.Value.Item1)) {
                            ButtonAndBinds.Key?.onClick?.Invoke();
                        }
                    }
                }
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    internal class FreezeControls : MonoBehaviour {
        public FreezeControls(IntPtr instance) : base(instance) { }

        internal Action OnExit;
        internal Action OnEnterKeyPressed;

        void OnEnable() {
            VRCInputManager.Method_Public_Static_Void_Boolean_PDM_0(true);
        }

        void OnDisable() {
            VRCInputManager.Method_Public_Static_Void_Boolean_PDM_0(false);

            OnExit?.Invoke();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                VRCInputManager.Method_Public_Static_Void_Boolean_PDM_0(false);
                VRCUiManager.prop_VRCUiManager_0.Method_Public_Virtual_New_Void_Boolean_0();
            } else if (Input.GetKeyDown(KeyCode.Return)) {
                OnEnterKeyPressed?.Invoke();
            }
        }
    }

    #endregion
    #endregion
}