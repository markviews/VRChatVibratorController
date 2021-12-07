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
                String name = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_SelectedUser_Local").GetComponent<VRC.UI.Elements.Menus.SelectedUserMenuQM>().field_Private_IUser_0.prop_String_0;
                VRCWSIntegration.connectedTo = name;
                VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.GetToys));
            });

        }

        private void onPlayerLeft(Player obj) {
            if (obj.prop_String_0 == VRCWSIntegration.connectedTo)
                foreach (Toy toy in Toy.remoteToys.Select(x=>x.Value)) {
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

            //new Toy("Edge", 100, 20, 20, 0, false, TabButton.SubMenu);
            //new Toy("Edge", 200, 20, 0, 0, false, TabButton.SubMenu);

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
            Toy toy = null;
            if (Toy.myToys.ContainsKey(msg.ToyID))
            {
                toy = Toy.myToys[msg.ToyID];
            }

            switch (msg.Command) {

                //remote toy commands
                case Commands.AddToy:
                    
                    if (msg.ToyID == ulong.MaxValue) {
                        MelonLogger.Error("Connected but no toys found..");
                        return;
                    }

                    MelonLogger.Msg($"Adding : {msg.ToyName} : {msg.ToyID}");
                    new Toy(msg.ToyName, msg.ToyID, msg.ToyMaxSpeed, msg.ToyMaxSpeed2, msg.ToyMaxLinear, msg.ToySupportsRotate, TabButton.SubMenu);

                    break;
                case Commands.RemoveToy:
                    if (Toy.remoteToys.ContainsKey(msg.ToyID))
                    {
                        toy = Toy.remoteToys[msg.ToyID];
                    }
                    toy?.disable();

                    break;

                //Local toy commands
                case Commands.SetSpeed:
                    if(toy?.hand == Hand.shared)
                        toy?.setSpeed(msg.Strength);
                    
                    break;
                case Commands.SetSpeedEdge:
                    if (toy?.hand == Hand.shared)
                        toy?.setEdgeSpeed(msg.Strength);
                    
                    break;
                case Commands.SetAir:
                    if (toy?.hand == Hand.shared)
                        toy?.setContraction(msg.Strength);

                    break;
                case Commands.SetRotate:
                    if (toy?.hand == Hand.shared)
                        toy?.rotate();
                    
                    break;
                case Commands.GetToys:
                    MelonLogger.Msg("Control Client connected");
                    //maybe check
                    foreach (KeyValuePair<ulong, Toy> entry in Toy.myToys.Where(x=>x.Value.hand == Hand.shared)) {
                        VRCWSIntegration.connectedTo = userID;
                        VRCWSIntegration.SendMessage(new VibratorControllerMessage(Commands.AddToy, entry.Value));
                    }

                    break;
            }
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