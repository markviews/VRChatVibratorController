using Buttplug;
using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UIExpansionKit.API;
using UnityEngine;
using Vibrator_Controller;
using System.Reflection;
using System.Collections.Generic;
using VRC;
using Friend_Notes;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine.UI;
using VRCWSLibary;
using System.Runtime.InteropServices;

[assembly: MelonInfo(typeof(VibratorController), "Vibrator Controller", "1.5.3", "MarkViews", "https://github.com/markviews/VRChatVibratorController")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit", "VRCWSLibary", "VRChatUtilityKit")]

namespace Vibrator_Controller {
    internal class VibratorController : MelonMod {

        private static bool useActionMenu = false;
        public static int buttonStep;
        private static GameObject quickMenu { get; set; }
        public static TabButton TabButton { get; private set; }
        private static ToggleButton search;
        private static Label networkStatus;
        private static Label buttplugError;

        private static MelonPreferences_Category vibratorController;
        private static ButtplugClient bpClient;


        public static AssetBundle iconsAssetBundle;
        public static Texture2D logo;
        public static int[] available_purcent = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        public static Dictionary<int, Texture2D> purcent_icons = new Dictionary<int, Texture2D>();
        public static string[] available_toys = { "Ambi", "Osci", "Edge", "Domi", "Hush", "Nora", "Lush", "Max", "Diamo" };
        public static Dictionary<string, Texture2D> toy_icons = new Dictionary<string, Texture2D>();

        public static bool VGBPresent = false;

        //https://gitlab.com/jacefax/vibegoesbrrr/-/blob/master/VibeGoesBrrrMod.cs#L27
        static public class NativeMethods
        {
            public static string TempPath
            {
                get
                {
                    string tempPath = Path.Combine(Path.GetTempPath(), $"VibratorController-1");
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }
                    return tempPath;
                }
            }

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            public static string LoadUnmanagedLibraryFromResource(Assembly assembly, string libraryResourceName, string libraryName)
            {
                string assemblyPath = Path.Combine(TempPath, libraryName);

                MelonLogger.Msg($"Unpacking and loading {libraryName}");

                using (Stream s = assembly.GetManifestResourceStream(libraryResourceName))
                {
                    var data = new BinaryReader(s).ReadBytes((int)s.Length);
                    File.WriteAllBytes(assemblyPath, data);
                }

                LoadLibrary(assemblyPath);

                return assemblyPath;
            }
        }

        static VibratorController()
        {
            //Clean up old file, call as earlöy as possible so file isnt loaded by VibeGoBrr
            if (File.Exists(Environment.CurrentDirectory + @"\buttplug_rs_ffi.dll"))
                File.Delete(Environment.CurrentDirectory + @"\buttplug_rs_ffi.dll");
            try
            {
                //Adapted from knah's JoinNotifier mod found here: https://github.com/knah/VRCMods/blob/master/JoinNotifier/JoinNotifierMod.cs 
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Vibrator_Controller.icons"))
                using (var tempStream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(tempStream);
                    iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }

                logo = iconsAssetBundle.LoadAsset_Internal("Assets/logo.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;

                foreach (string toy_name in available_toys)
                {
                    var logo = iconsAssetBundle.LoadAsset_Internal($"Assets/{toy_name.ToLower()}-x64.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    toy_icons.Add(toy_name, logo);
                }

                foreach (int purcent in available_purcent)
                {
                    var logo = iconsAssetBundle.LoadAsset_Internal($"Assets/{purcent}.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    purcent_icons.Add(purcent, logo);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning("Consider checking for newer version as mod possibly no longer working, Exception occured OnAppStart(): " + e.Message);
            }
        }

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "VibeGoesBrrr"))
            {
                MelonLogger.Warning("VibeGoesBrrr detected. Disabling Vibrator Controller since these mods are incompatible");
                return;
            }

            
            NativeMethods.LoadUnmanagedLibraryFromResource(Assembly.GetExecutingAssembly(), "Vibrator_Controller.buttplug_rs_ffi.dll", "buttplug_rs_ffi.dll");

            vibratorController = MelonPreferences.CreateCategory("VibratorController");

            MelonPreferences.CreateEntry(vibratorController.Identifier, "ActionMenu", true, "action menu integration");
            MelonPreferences.CreateEntry(vibratorController.Identifier, "buttonStep", 5, "What % to change when pressing button");

            useActionMenu = MelonPreferences.GetEntryValue<bool>(vibratorController.Identifier, "ActionMenu");
            buttonStep = MelonPreferences.GetEntryValue<int>(vibratorController.Identifier, "buttonStep");

            if (useActionMenu && MelonHandler.Mods.Any(mod => mod.Info.Name == "ActionMenuApi")) {
                try {
                    new ToyActionMenu();
                } catch (Exception) {
                    MelonLogger.Warning("Failed to add action menu button");
                }
            }

            VRCWSIntegration.Init();
            MelonCoroutines.Start(UiManagerInitializer());
            CreateButton();

            VRCUtils.OnUiManagerInit += createMenu;
        }

        public IEnumerator UiManagerInitializer() {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;

            quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)");

            NetworkManagerHooks.Initialize();
            NetworkManagerHooks.OnLeave += onPlayerLeft;
        }

        private void CreateButton() {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Get\nToys", () => {
                string name = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_SelectedUser_Local").GetComponent<VRC.UI.Elements.Menus.SelectedUserMenuQM>().field_Private_IUser_0.prop_String_0;
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(name, Commands.GetToys));
            });

        }

        private void onPlayerLeft(Player obj) {
            foreach (Toy toy in Toy.remoteToys.Where(x=>x.Value.connectedTo == obj.prop_String_0).Select(x=>x.Value)) {
                toy.disable();
            }
        }

        internal static void createMenu()
        {
            MelonLogger.Msg("Creating BP client");
            SetupBP();

            MelonLogger.Msg("Creating Menu");
            search = new ToggleButton((state) =>
            {
                if (state)
                {
                    search.Text = "Scanning...";
                    bpClient.StartScanningAsync();
                }
                else
                {
                    search.Text = "Scan for toys";
                    bpClient.StopScanningAsync();
                }
            },
            CreateSpriteFromTexture2D(logo), null, "Scan for toys", "BPToggle", "Scan for connected toys", "Scaning for connected toys");
            networkStatus = new Label("Network", Client.ClientAvailable() ? "Connected" : "Not\nConnected", "networkstatus");
            networkStatus.TextComponent.fontSize = 24;
            buttplugError = new Label("Buttplug", "No Error", "status");
            buttplugError.TextComponent.fontSize = 24;
            Client.GetClient().ConnectRecieved += async() => {
                await AsyncUtils.YieldToMainThread();
                networkStatus.SubtitleText = Client.ClientAvailable() ? "Connected" : "Not\nConnected"; 
            };
            TabButton = new TabButton(CreateSpriteFromTexture2D(logo), "Vibrator Controller", "VibratorControllerMenu", "Vibrator Controller", "Vibrator Controller Menu");
            TabButton.SubMenu
              .AddButtonGroup(new ButtonGroup("ControlsGrp", "Controls", new List<IButtonGroupElement>()
              {search, networkStatus, buttplugError, new SingleButton(() => { ResetBP(); }, CreateSpriteFromTexture2D(logo), "Reset Connector", "reset", "Resets the underlyung buttplug connector")
            }));

            //Control all toys (vibrate only)
            new Toy("All Toys", 1000, "all", 20, 0, 0, false, TabButton.SubMenu);

            //activate scroll
            TabButton.SubMenu.ToggleScrollbar(true);
        }

        public static Sprite CreateSpriteFromTexture2D(Texture2D texture)
        {
            if (texture == null) 
                return null;
            Rect size = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.CreateSprite(texture, size, pivot, 100, 0, SpriteMeshType.Tight, Vector4.zero, false);
        }

        private static void SetupBP() {
            bpClient = new ButtplugClient("VRCVibratorController");
            bpClient.ConnectAsync(new ButtplugEmbeddedConnectorOptions());
            bpClient.DeviceAdded += async(object aObj, DeviceAddedEventArgs args) => {
                await AsyncUtils.YieldToMainThread();
                new Toy(args.Device, TabButton.SubMenu);
            };
            
            bpClient.DeviceRemoved += async(object aObj, DeviceRemovedEventArgs args) => {
                await AsyncUtils.YieldToMainThread();
                if (Toy.myToys.ContainsKey(args.Device.Index))
                {
                    Toy.myToys[args.Device.Index].disable();
                }
            };

            bpClient.ErrorReceived += async(object aObj, ButtplugExceptionEventArgs args) =>
            {
                MelonLogger.Msg($"Buttplug Client recieved error: {args.Exception.Message}");
                await AsyncUtils.YieldToMainThread();

                buttplugError.SubtitleText = "Error occured";
            };
        }


        private static void ResetBP()
        {
            MelonLogger.Msg("Resetting Buttplug Connector");
            if (bpClient != null)
            {
                bpClient.DisconnectAsync().Wait();
                bpClient = null;
                Toy.allToys.ForEach(x => x.disable());
            }
            buttplugError.SubtitleText = "No Error";
            SetupBP();
        }

        public override void OnUpdate() {
            

            foreach (Toy toy in Toy.allToys) {
                if (toy.hand == Hand.shared || toy.hand == Hand.none || toy.hand == Hand.actionmenu) return;
                
                if (menuOpen()) return;

                int left = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"));
                int right = (int)(toy.maxSpeed * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));

                switch (toy.hand) {
                    case Hand.left:
                        right = left;
                        break;
                    case Hand.right:
                        left = right;
                        break;
                    case Hand.either:
                        if (left > right) right = left;
                        else left = right;
                        break;
                    case Hand.both:
                        break;
                }
                if (toy.supportsTwoVibrators) {
                    toy.setEdgeSpeed(right);
                }
                toy.setSpeed(left);
            }
        }

        //message from server
        internal static async void message(VibratorControllerMessage msg, string userID) {
            await AsyncUtils.YieldToMainThread();

            switch (msg.Command)
            {
                case Commands.GetToys:
                    handleGetToys(userID);
                    break;
                case Commands.ToyUpdate:
                    handleToyUpdate(msg, userID);
                    break;
                case Commands.SetSpeeds:
                    handleSetSpeeds(msg);
                    break;
            }
        }

        private static void handleSetSpeeds(VibratorControllerMessage msg)
        {
            foreach (var toymessage in msg.messages.Select(x => x.Value))
            {
                if (!Toy.myToys.ContainsKey(toymessage.ToyID))
                    continue;

                Toy toy = Toy.myToys[toymessage.ToyID];

                switch (toymessage.Command)
                {
                    //Local toy commands
                    case Commands.SetSpeed:
                        if (toy?.hand == Hand.shared)
                            toy?.setSpeed(toymessage.Strength);

                        break;
                    case Commands.SetSpeedEdge:
                        if (toy?.hand == Hand.shared)
                            toy?.setEdgeSpeed(toymessage.Strength);

                        break;
                    case Commands.SetAir:
                        if (toy?.hand == Hand.shared)
                            toy?.setContraction(toymessage.Strength);

                        break;
                    case Commands.SetRotate:
                        if (toy?.hand == Hand.shared)
                            toy?.rotate();

                        break;
                }
            }
        }

        private static void handleToyUpdate(VibratorControllerMessage msg, string userID)
        {
            foreach (var toy in msg.messages.Select(x => x.Value))
            {
                switch (toy.Command)
                {

                    //remote toy commands
                    case Commands.AddToy:

                        MelonLogger.Msg($"Adding : {toy.ToyName} : {toy.ToyID}");
                        new Toy(toy.ToyName, toy.ToyID, userID, toy.ToyMaxSpeed, toy.ToyMaxSpeed2, toy.ToyMaxLinear, toy.ToySupportsRotate, TabButton.SubMenu);

                        break;
                    case Commands.RemoveToy:

                        if (Toy.remoteToys.ContainsKey(toy.ToyID))
                            Toy.remoteToys[toy.ToyID].disable();
                        break;
                }
            }
        }

        private static void handleGetToys(string userID)
        {
            MelonLogger.Msg("Control Client requested toys");
            VibratorControllerMessage messageToSend = null;
            foreach (KeyValuePair<ulong, Toy> entry in Toy.myToys.Where(x => x.Value.hand == Hand.shared))
            {
                entry.Value.connectedTo = userID;
                if (messageToSend == null)
                    messageToSend = new VibratorControllerMessage(userID, Commands.AddToy, entry.Value);
                else
                    messageToSend.Merge(new VibratorControllerMessage(userID, Commands.AddToy, entry.Value));

            }

            if (messageToSend != null)
                VRCWSIntegration.SendMessage(messageToSend);
        }

        private static bool menuOpen() {
            if (quickMenu == null) {
                quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)");
                return true;
            }

            if (quickMenu.activeSelf) {
                return true;
            }
                
            return false;
        }
    }
}