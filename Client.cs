using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrator_Controller {
    class Client {

        //Source https://codereview.stackexchange.com/questions/41591/websockets-client-code-and-making-it-production-ready
        static ClientWebSocket webSocket;

        public static async void setupClient() {
            if (webSocket != null) return;
            try {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("ws://control.markstuff.net:8080"), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket));
            } catch (Exception ex) {
                Console.WriteLine("Exception: {0}", ex);
            }
        }

        public static async Task send(string msg) {
            MelonLogger.Log("Sending: " + msg);
            byte[] buffer = Encoding.Default.GetBytes(msg);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task Receive(ClientWebSocket webSocket) {
            byte[] buffer = new byte[256];
            Array.Clear(buffer, 0, buffer.Length);
            while (webSocket.State == WebSocketState.Open) {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                } else {
                    String text = Encoding.ASCII.GetString(buffer);
                    MelonLogger.Log("Receaved: " + text);
                    VibratorController.message(text);
                }
            }
        }
    }
}
