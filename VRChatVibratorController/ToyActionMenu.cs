using ActionMenuUtils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Vibrator_Controller
{
    class ToyActionMenu
    {
        private static AssetBundle iconsAssetBundle = null;
        private static Texture2D lovenseLogo;

        private static int[] available_purcent = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        private static Dictionary<int, Texture2D> purcent_icons = new Dictionary<int, Texture2D>();

        private static string[] available_toys = { "Ambi", "Osci", "Edge", "Domi", "Hush", "Nora", "Lush" };
        private static Dictionary<string, Texture2D> toy_icons = new Dictionary<string, Texture2D>();

        private static ActionMenuAPI actionMenuApi;

        internal ToyActionMenu()
        {

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

                lovenseLogo = iconsAssetBundle.LoadAsset_Internal("Assets/lovense-logo.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                lovenseLogo.hideFlags |= HideFlags.DontUnloadUnusedAsset;

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

            actionMenuApi = new ActionMenuAPI();

            SetupButtons();
        }

        private static void SetupButtons()
        {
            actionMenuApi.AddPedalToExistingMenu(ActionMenuAPI.ActionMenuPageType.Main, delegate
            {
                actionMenuApi.CreateSubMenu(delegate {
                    //// Test displaying all existing toys icons
                    // foreach (string toy_name in available_toys)
                    // {
                    //     ToysMenu("Nora", toy_icons[toy_name]);
                    // }

                    foreach (Toy toy in Toy.toys)
                    {
                        ToysMenu(toy);
                    }
                });
            }, "Lovense", lovenseLogo);

            MelonLogger.Msg("ActionMenu Generated");
        }

        private static void ToysMenu(Toy toy)
        {
            actionMenuApi.AddPedalToCustomMenu(() =>
            {
                actionMenuApi.CreateSubMenu(delegate {
                    foreach (int purcent in Enumerable.Reverse(available_purcent).ToArray())
                    {
                        PucentageMenu(toy, purcent, purcent_icons[purcent]);
                    }
                });
            }, $"{toy.name}: {toy.lastSpeed * 10}%", toy_icons[toy.name]);
        }

        // TODO find a way to use radial button
        // See with gompo#6956 if he found anything interesting for that

        private static void PucentageMenu(Toy toy, int purcent, Texture2D logo = null)
        {
            actionMenuApi.AddPedalToCustomMenu(() =>
            {
                toy.setSpeed(purcent / 10);
            }, "", logo);
        }
    }
}
