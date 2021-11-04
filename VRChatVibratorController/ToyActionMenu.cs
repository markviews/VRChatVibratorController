using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using ActionMenuApi.Api;
using System.Linq;

namespace Vibrator_Controller {
    class ToyActionMenu {

        private static AssetBundle iconsAssetBundle = null;
        private static Texture2D logo;
        private static int[] available_purcent = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        private static Dictionary<int, Texture2D> purcent_icons = new Dictionary<int, Texture2D>();
        private static string[] available_toys = { "Ambi", "Osci", "Edge", "Domi", "Hush", "Nora", "Lush", "Max", "Diamo" };
        private static Dictionary<string, Texture2D> toy_icons = new Dictionary<string, Texture2D>();

        internal ToyActionMenu() {
            try {
                //Adapted from knah's JoinNotifier mod found here: https://github.com/knah/VRCMods/blob/master/JoinNotifier/JoinNotifierMod.cs 
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Vibrator_Controller.icons"))
                using (var tempStream = new MemoryStream((int)stream.Length)) {
                    stream.CopyTo(tempStream);
                    iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }

                logo = iconsAssetBundle.LoadAsset_Internal("Assets/logo.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;

                foreach (string toy_name in available_toys) {
                    var logo = iconsAssetBundle.LoadAsset_Internal($"Assets/{toy_name.ToLower()}-x64.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    toy_icons.Add(toy_name, logo);
                }

                foreach (int purcent in available_purcent) {
                    var logo = iconsAssetBundle.LoadAsset_Internal($"Assets/{purcent}.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    purcent_icons.Add(purcent, logo);
                }
            } catch (Exception e) {
                MelonLogger.Warning("Consider checking for newer version as mod possibly no longer working, Exception occured OnAppStart(): " + e.Message);
            }

            SetupButtons();
        }

        private static void SetupButtons() {
            VRCActionMenuPage.AddSubMenu(ActionMenuPage.Main, "Vibrator Controller", delegate {
                foreach (Toy toy in Toy.allToys)
                {
                    try
                    {
                        if (toy.isActive && toy.hand != Hand.shared) ToysMenu(toy);
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Warning($"Error with toy {toy.name}: " + e.Message);
                    }
                }
            }, logo);
        }

        private static void ToysMenu(Toy toy) {
            switch (toy.name) {
                case "Edge":
                    EdgeRadials(toy);
                    break;
                case "Max":
                    MaxRadials(toy);
                    break;
                case "Nora":
                    NoraRadials(toy);
                    break;
                default:
                    VibrateRadial(toy, toy.name);
                    break;
            }
        }

        private static void VibrateRadial(Toy toy, string text = "") {
            CustomSubMenu.AddRadialPuppet(text, f => {
                int roundedPercent = (int)Math.Round(f * 100);
                toy.setSpeed(roundedPercent / (100/toy.maxSpeed)); //0-10
            }, ((float)toy.lastSpeed) / toy.maxSpeed, GetTextureForToy(toy));
        }

        private static Texture2D GetTextureForToy(Toy toy) {
            string name = toy.name;

            if (toy_icons.ContainsKey(name))
                return toy_icons[name];

            return null;
        }

        private static void EdgeRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " 2");

            CustomSubMenu.AddRadialPuppet(toy.name + " 1", f => {
                int roundedPercent = (int)Math.Round(f * 100);
                toy.setEdgeSpeed(roundedPercent / (100 / toy.maxSpeed2)); //0-10
            }, ((float)toy.lastEdgeSpeed) / toy.maxSpeed2, GetTextureForToy(toy));
        }

        private static void MaxRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " Vibration");

            CustomSubMenu.AddRadialPuppet($"{toy.name} Contraction", f => {
                int contractionLevel = (int)Math.Round(f * 100) / (100 / toy.maxLinear);
                if (toy.lastContraction != contractionLevel) {
                    toy.setContraction(contractionLevel);
                }
            }, ((float)toy.lastSpeed / toy.maxSpeed), GetTextureForToy(toy));
        }

        private static void NoraRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " Vibration");

            CustomSubMenu.AddButton(toy.name + " Rotate", () => { toy.rotate(); }, GetTextureForToy(toy));
        }

    }
}