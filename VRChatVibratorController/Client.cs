using MelonLoader;
using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

namespace Vibrator_Controller {
    class Client {

        internal static string currentlyConnectedCode;
        internal static int reconnectTries = 0;
        private static WebSocket ws;
        private static bool connected = false;

        internal static void Setup() {
            ws = new WebSocket("ws://control.markstuff.net:8080");
            ws.OnMessage += (sender, e) => VibratorController.message(e.Data);
            ws.OnOpen += (sender, e) => { 
                connected = true;
                MelonLogger.Msg("Connected to server");
                if (currentlyConnectedCode != null) {
                    Send("join " + currentlyConnectedCode);
                } else {
                    Send("new");
                }
            };
            ws.OnError += Reconnect;
            ws.OnClose += Reconnect;
        }

        internal static void Send(string text) {
            if (ws != null && connected) {
                ws.Send(text);
                MelonLogger.Msg("Sent: " + text);
            } else
                Reconnect(null, null);
        }

        private static void Reconnect(object sender, EventArgs e) {
            connected = false;
            reconnectTries += 1;

            if (reconnectTries < 10) 
                MelonCoroutines.Start(Retry());
            else if (reconnectTries == 10)
                MelonLogger.Msg("Failed to reconnect 10 times.. press AddToy and type code to reconnect");
        }

        private static IEnumerator Retry() {
            yield return new WaitForSeconds(1);
            if (ws == null || !connected)
                ws.Connect();
        }

    }

}
