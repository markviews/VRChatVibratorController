using MelonLoader;
using Newtonsoft.Json;
using System.Collections;
using VRCWSLibary;

namespace Vibrator_Controller {
    public enum Commands {
        AddToy, RemoveToy, SetSpeed, SetSpeedEdge, SetAir, SetRotate, GetToys
    }

    public class VibratorControllerMessage {
        public VibratorControllerMessage() { }
        public VibratorControllerMessage(Commands command) { Command = command; }
        public VibratorControllerMessage(Commands command, Toy toy) { Command = command; ToyID = toy.id; ToyName = toy.name; ToyMaxSpeed = toy.maxSpeed; ToyMaxSpeed2 = toy.maxSpeed2; ToyMaxLinear = toy.maxLinear; ToySupportsRotate = toy.supportsRotate; }
        public VibratorControllerMessage(Commands command, Toy toy, int strength) { Command = command; ToyID = toy.id; ToyName = toy.name; Strength = strength; }
        public Commands Command { get; set; }
        public string ToyID { get; set; }
        public string ToyName { get; set; }
        public int Strength { get; set; }
        public int ToyMaxSpeed { get; set; }
        public int ToyMaxSpeed2 { get; set; }
        public int ToyMaxLinear { get; set; }
        public bool ToySupportsRotate { get; set; }
    }
    public class VRCWSIntegration {
        public static string connectedTo;

        private static Client client;
        private static MelonPreferences_Entry<bool> onlyTrusted;

        public static void Init() {
            var category = MelonPreferences.CreateCategory("VibratorController");
            onlyTrusted = category.CreateEntry("Only Trusted", false);
            MelonCoroutines.Start(LoadClient());
        }

        public static void SendMessage(VibratorControllerMessage message) {
            if (client == null || connectedTo == null)
                return;
            client.Send(new Message() { Method = "VibratorControllerMessage", Target = connectedTo, Content = JsonConvert.SerializeObject(message) });
        }

        private static IEnumerator LoadClient() {
            while (!Client.ClientAvailable())
                yield return null;


            client = Client.GetClient();

            onlyTrusted.OnValueChanged += (_, newValue) => {
                client.RemoveEvent("VibratorControllerMessage");
                client.RegisterEvent("VibratorControllerMessage", (msg) => {
                    EventCall(msg);
                }, signatureRequired: newValue);
            };


            client.RegisterEvent("VibratorControllerMessage", (msg) => {
                EventCall(msg);
            }, signatureRequired: onlyTrusted.Value);

        }

        private static long lastTick = 0;

        private static void EventCall(Message msg) {
            //MelonLogger.Msg($"VibratorControllerMessage recieved");
            //MelonLogger.Msg(msg);

            if (msg.TimeStamp.Ticks > lastTick) {
                lastTick = msg.TimeStamp.Ticks;
                VibratorController.message(msg.GetContentAs<VibratorControllerMessage>() ?? new VibratorControllerMessage(), msg.Target); //ignore null message
            }
        }


    }
}