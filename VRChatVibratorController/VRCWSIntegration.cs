using MelonLoader;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using VRCWSLibary;

namespace Vibrator_Controller {
    public enum Commands {
        AddToy, RemoveToy, SetSpeed, SetSpeedEdge, SetAir, SetRotate, GetToys, SetSpeeds, ToyUpdate
    }

    public class ToyMessage
    {
        public Commands Command { get; set; }
        public ulong ToyID { get; set; }
        public string ToyName { get; set; }
        public int Strength { get; set; }
        public int ToyMaxSpeed { get; set; }
        public int ToyMaxSpeed2 { get; set; }
        public int ToyMaxLinear { get; set; }
        public bool ToySupportsRotate { get; set; }
    }

    public class VibratorControllerMessage
    {
        public VibratorControllerMessage() { }
        public VibratorControllerMessage(string target, Commands command) { 
            Target = target; 
            Command = command; 
        }
        public VibratorControllerMessage(string target, Commands command, Toy toy) { 
            Target = target;

            messages[toy.id + ":" + command] = new ToyMessage()
            {
                Command = command,
                ToyID = toy.id,
                ToyName = toy.name,
                ToyMaxSpeed = toy.maxSpeed,
                ToyMaxSpeed2 = toy.maxSpeed2,
                ToyMaxLinear = toy.maxLinear,
                ToySupportsRotate = toy.supportsRotate
            };
            Command = Commands.ToyUpdate;

        }
        public VibratorControllerMessage(string target, Commands command, Toy toy, int strength) { 
            Target = target;
            messages[toy.id +":"+ command] = new ToyMessage()
            {
                Command = command,
                ToyID = toy.id,
                ToyName = toy.name,
                Strength = strength
            };
            Command = Commands.SetSpeeds;
        }

        public string Target { get; set; }
        public Commands Command { get; set; }
        public Dictionary<string, ToyMessage> messages = new Dictionary<string, ToyMessage>();

        //Merge the parameter into the current one
        public void Merge(VibratorControllerMessage otherMessage)
        {
            foreach (var message in otherMessage.messages)
            {
                this.messages[message.Key] = message.Value;
            }
        }

    }
    public class VRCWSIntegration {

        private static Client client;
        private static MelonPreferences_Entry<bool> onlyTrusted;

        private static Dictionary<string, VibratorControllerMessage> messagesToSendPerTarget = new Dictionary<string, VibratorControllerMessage>();


        public static void Init() {
            var category = MelonPreferences.CreateCategory("VibratorController");
            onlyTrusted = category.CreateEntry("Only Trusted", false);
            MelonCoroutines.Start(LoadClient());
            Timer timer = new Timer(200);
            timer.Elapsed += (_,__) => {
                if (client == null)
                    return;

                lock (messagesToSendPerTarget)
                {
                    foreach (var message in messagesToSendPerTarget)
                    {
                        client.Send(new Message() { Method = "VibratorControllerMessage", Target = message.Value.Target, Content = JsonConvert.SerializeObject(message.Value) });
                    }
                    messagesToSendPerTarget.Clear();
                }
            };
            timer.Enabled = true;
        }

        public static void SendMessage(VibratorControllerMessage message) {
            if (client == null || message.Target == null)
                return;
            if (message.Command == Commands.SetSpeeds)
            {
                lock (messagesToSendPerTarget)
                {
                    if (messagesToSendPerTarget.ContainsKey(message.Target))
                        messagesToSendPerTarget[message.Target].Merge(message);
                    else
                        messagesToSendPerTarget[message.Target] = message;
                }
                return;
            }

            client.Send(new Message() { Method = "VibratorControllerMessage", Target = message.Target, Content = JsonConvert.SerializeObject(message) });
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
                var messagecontent = msg.GetContentAs<VibratorControllerMessage>();
                if(messagecontent != null)
                    VibratorController.message(messagecontent, msg.Target);
            }
        }


    }
}