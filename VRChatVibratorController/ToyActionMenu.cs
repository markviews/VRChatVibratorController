using ActionMenuUtils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ActionMenuApi;
using ActionMenuApi.Types;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Vibrator_Controller {
    class ToyActionMenu {
        private static AssetBundle iconsAssetBundle = null;
        private static Texture2D logo;

        private static int[] available_purcent = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        private static Dictionary<int, Texture2D> purcent_icons = new Dictionary<int, Texture2D>();

        private static string[] available_toys = { "Ambi", "Osci", "Edge", "Domi", "Hush", "Nora", "Lush", "Max", "Diamo" };
        private static Dictionary<string, Texture2D> toy_icons = new Dictionary<string, Texture2D>();

        private static ActionMenuAPI actionMenuApi;

        internal ToyActionMenu() {
            try {
                //Adapted from knah's JoinNotifier mod found here: https://github.com/knah/VRCMods/blob/master/JoinNotifier/JoinNotifierMod.cs 
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Vibrator_Controller.icons"))
                using (var tempStream = new MemoryStream((int)stream.Length)) {
                    stream.CopyTo(tempStream);

                    iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }

                logo = iconsAssetBundle.LoadAsset_Internal("Assets/logo.png", Il2CppType.Of<Texture2D>())
                    .Cast<Texture2D>();
                logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;

                foreach (string toy_name in available_toys) {
                    var logo = iconsAssetBundle
                        .LoadAsset_Internal($"Assets/{toy_name.ToLower()}-x64.png", Il2CppType.Of<Texture2D>())
                        .Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    toy_icons.Add(toy_name, logo);
                }

                foreach (int purcent in available_purcent) {
                    var logo = iconsAssetBundle.LoadAsset_Internal($"Assets/{purcent}.png", Il2CppType.Of<Texture2D>())
                        .Cast<Texture2D>();
                    logo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    purcent_icons.Add(purcent, logo);
                }
            } catch (Exception e) {
                MelonLogger.Warning(
                    "Consider checking for newer version as mod possibly no longer working, Exception occured OnAppStart(): " +
                    e.Message);
            }

            // actionMenuApi = new ActionMenuAPI();

            SetupButtons();
        }

        private static void SetupButtons() {
            AMAPI.AddSubMenuToMenu(ActionMenuPageType.Main,
                "Vibrator Controller",
                delegate {
                    foreach (Toy toy in Toy.toys) {
                        try {
                            if (toy.isActive) ToysMenu(toy);
                        } catch (Exception e) {
                            MelonLogger.Warning($"Error with toy {toy.name}: " + e.Message);
                            throw;
                        }
                    }
                },
                logo
            );

            return;
            actionMenuApi.AddPedalToExistingMenu(ActionMenuAPI.ActionMenuPageType.Main, delegate {
                actionMenuApi.CreateSubMenu(delegate {
                    //// Test displaying all existing toys icons
                    // foreach (string toy_name in available_toys)
                    // {
                    //     ToysMenu("Nora", toy_icons[toy_name]);
                    // }

                    foreach (Toy toy in Toy.toys) {
                        if (toy.isActive) ToysMenu(toy);
                    }
                });
            }, "Vibrator Controller", logo);

            MelonLogger.Msg("ActionMenu Generated");
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

            return;
            actionMenuApi.AddPedalToCustomMenu(() => {
                actionMenuApi.CreateSubMenu(delegate {
                    foreach (int purcent in Enumerable.Reverse(available_purcent).ToArray()) {
                        PercentageMenu(toy, purcent, purcent_icons[purcent]);
                    }
                });
            }, $"{toy.name}: {toy.lastSpeed * 10}%", toy_icons[toy.name]);
        }

        private static void VibrateRadial(Toy toy, string text = "") {
            AMAPI.AddRadialPedalToSubMenu(text,
                f => {
                    int roundedPercent = (int)Math.Round(f * 100);

                    if (toy.lastSpeed != roundedPercent / 10) {
                        toy.setSpeed(roundedPercent / 10);
                    }
                }, toy.lastSpeed * 10, toy_icons[toy.name]);
        }

        private static void EdgeRadials(Toy toy) {
            VibrateRadial(toy, $"{toy.name} 2");

            AMAPI.AddRadialPedalToSubMenu($"{toy.name} 1",
                f =>
                {
                    int roundedPercent = (int) Math.Round(f * 100);

                    if (toy.lastEdgeSpeed != roundedPercent / 10) {
                        toy.setEdgeSpeed(roundedPercent / 10);
                    }
                }, toy.lastEdgeSpeed * 10, toy_icons[toy.name]);
        }

        private static void MaxRadials(Toy toy) {
            VibrateRadial(toy, $"{toy.name} Vibration");

            AMAPI.AddRadialPedalToSubMenu($"{toy.name} Contraction",
                f => {
                    int contractionLevel = (int)Math.Round(f * 100) / 33;

                    if (toy.contraction != contractionLevel) {
                        toy.setContraction(contractionLevel);
                    }
                }, toy.lastSpeed * 10, toy_icons[toy.name]);
        }

        private static void NoraRadials(Toy toy) {
            VibrateRadial(toy, $"{toy.name} Vibration");

            AMAPI.AddButtonPedalToSubMenu($"{toy.name} Rotate", () => { toy.rotate(); }, toy_icons[toy.name]);
        }

        // TODO find a way to use radial button
        // See with gompo#6956 if he found anything interesting for that

        private static void PercentageMenu(Toy toy, int purcent, Texture2D logo = null) {
            actionMenuApi.AddPedalToCustomMenu(() => { toy.setSpeed(purcent / 10); }, "", logo);
        }
    }
}