using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrator_Controller
{
    class Client
    {
        public static string currentlyConnectedCode; 
        //Source https://codereview.stackexchange.com/questions/41591/websockets-client-code-and-making-it-production-ready
        static ClientWebSocket webSocket;

        internal static async void setupClient(string[] queue)
        {
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("wss://control.markstuff.net:8080"), CancellationToken.None);
                foreach (string item in queue)
                {
                    await send(item);
                }
                await Receive(webSocket);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error occured on setupClient method:\n{ex}");
            }
        }

        

        internal static async Task send(string msg)
        {
            //MelonLogger.Log("Sending: [" + msg + "]");
            if (webSocket == null || (webSocket.State != WebSocketState.Open && currentlyConnectedCode == null))
            {
                setupClient(new string[]{ msg });
                return;
            }
            MelonDebug.Msg(webSocket.State);
            if (webSocket.State != WebSocketState.Open && currentlyConnectedCode != null)
            {
                setupClient(new string[] {"join "+ currentlyConnectedCode, msg });
                return;
            }
            byte[] buffer = new byte[20];
            buffer = Encoding.Unicode.GetBytes(msg);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[256];
            while (webSocket.State == WebSocketState.Open)
            {
                Array.Clear(buffer, 0, buffer.Length);
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    String text = Encoding.ASCII.GetString(buffer);
                    VibratorController.message(text);
                }
            }
        }
    }
}
