using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrator_Controller {
    class Client {

        internal static string currentlyConnectedCode;
        private static ClientWebSocket webSocket;

        //Source https://codereview.stackexchange.com/questions/41591/websockets-client-code-and-making-it-production-ready

        private static void setupClient() {
            try {
                webSocket = new ClientWebSocket();
                webSocket.ConnectAsync(new Uri("wss://control.markstuff.net:8080"), CancellationToken.None).ContinueWith(new Action<Task>((t) => {
                    send("join " + currentlyConnectedCode);
                    MelonLogger.Msg("Connected to server");
                    listen(webSocket).Start();
                }));
            } catch (Exception e) {
                MelonLogger.Error("Disconnected from server. " + e.Message  + "\n" + e.StackTrace);
                webSocket = null;
            }
        }

        internal static void send(string msg) {
            try {
                if (webSocket == null) {
                    if (currentlyConnectedCode != null) setupClient();
                    return;
                }

                if (webSocket.State == WebSocketState.Open)
                    webSocket.SendAsync(new ArraySegment<byte>(Encoding.Unicode.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);
            } catch (Exception e) {
                MelonLogger.Error("A " + e);
            }
        }

        private static async Task listen(ClientWebSocket webSocket) {
            try {
                byte[] buffer = new byte[256];
                while (webSocket.State == WebSocketState.Open) {
                    Array.Clear(buffer, 0, buffer.Length);

                     await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(new Action<Task<WebSocketReceiveResult>>((t) => {
                        try {
                             if (t.Result.MessageType == WebSocketMessageType.Close)
                                 webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                             else VibratorController.message(Encoding.ASCII.GetString(buffer));
                         } catch (Exception e) {
                             MelonLogger.Error("B " + e);
                         }
                     }));
                }
            } catch (Exception e) {
                MelonLogger.Error("C " + e);
            }
        }

    }
}
