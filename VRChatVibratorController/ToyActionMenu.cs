using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using ActionMenuApi.Api;

namespace Vibrator_Controller {
    class ToyActionMenu {


        internal ToyActionMenu() {
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
            }, VibratorController.logo);
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
            }, ((float)toy.lastSpeed) / toy.maxSpeed, toy.GetTexture());
        }

        

        private static void EdgeRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " 2");

            CustomSubMenu.AddRadialPuppet(toy.name + " 1", f => {
                int roundedPercent = (int)Math.Round(f * 100);
                toy.setEdgeSpeed(roundedPercent / (100 / toy.maxSpeed2)); //0-10
            }, ((float)toy.lastEdgeSpeed) / toy.maxSpeed2, toy.GetTexture());
        }

        private static void MaxRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " Vibration");

            CustomSubMenu.AddRadialPuppet($"{toy.name} Contraction", f => {
                int contractionLevel = (int)Math.Round(f * 100) / (100 / toy.maxLinear);
                if (toy.lastContraction != contractionLevel) {
                    toy.setContraction(contractionLevel);
                }
            }, ((float)toy.lastSpeed / toy.maxSpeed), toy.GetTexture());
        }

        private static void NoraRadials(Toy toy) {
            VibrateRadial(toy, toy.name + " Vibration");

            CustomSubMenu.AddButton(toy.name + " Rotate", () => { toy.rotate(); }, toy.GetTexture());
        }

    }
}