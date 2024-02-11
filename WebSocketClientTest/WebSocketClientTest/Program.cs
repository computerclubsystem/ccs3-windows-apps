using Microsoft.VisualBasic;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketClientTest.Messages;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest
{
    internal class Program
    {
        static ClientWebSocket ws;
        static CancellationTokenSource cts = new CancellationTokenSource();
        static CancellationToken token;
        static string serverUri;
        static string deviceId;
        static DesktopManager dm;
        static IntPtr defaultDesktopPtr;
        static IntPtr securedDesktopPtr;
        static JsonSerializerOptions jsonSerializerOptions;
        static void Main(string[] args)
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            dm = new DesktopManager();

            defaultDesktopPtr = dm.GetDefaultDesktopPointer();
            securedDesktopPtr = dm.CreateDesktop("Secured");

            token = cts.Token;
            serverUri = args[0];
            deviceId = args[1];
            ConnectWebSocket(serverUri);
            while (true)
            {
                Console.WriteLine("1 to send short message, 2 to send 5 MB message");
                string line = Console.ReadLine();
                try
                {
                    if (line == "1")
                    {
                        byte[] data = Encoding.UTF8.GetBytes("Message from C# client");
                        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(data);
                        ws.SendAsync(memory, WebSocketMessageType.Binary, true, token);
                    }
                    else if (line == "2")
                    {
                        byte[] data = new byte[5 * 1024 * 1024];
                        Random.Shared.NextBytes(data);
                        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(data);
                        ws.SendAsync(memory, WebSocketMessageType.Binary, true, token);
                    }
                }
                catch (Exception ex)
                {
                    Log("Cannot send data: " + ex.ToString());
                }
            }
        }

        static async void ConnectWebSocket(string uriString)
        {
            //ClientWebSocket ws = new ClientWebSocket();
            Uri uri = new Uri(uriString);
            while (true)
            {
                try
                {
                    ws = new ClientWebSocket();
                    ws.Options.RemoteCertificateValidationCallback = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
                    {
                        return true;
                    };
                    Log("Connecting to " + uriString);
                    await ws.ConnectAsync(uri, token);
                    Log("Connected");
                    break;
                }
                catch (Exception ex)
                {
                    Log("Cannot connect. " + ex.ToString());
                }
            }
            StartReceiving();

            // TODO: Move this send
            DeviceAuthMessage joinMessage = DeviceAuthMessageFactory.Create();
            joinMessage.Body = new DeviceAuthMessageBody();
            joinMessage.Body.DeviceId = deviceId;
            SendMessage(joinMessage);
        }
        static async void StartReceiving()
        {
            //List<byte> message = new List<byte>();
            byte[] buffer = new byte[1 * 1024 * 1024];
            while (true)
            {
                Memory<byte> memory = new(buffer);
                try
                {
                    var result = await ws.ReceiveAsync(memory, token);
                    Log("Received " + result.Count + " bytes. End of message: " + result.EndOfMessage);
                    if (result.EndOfMessage)
                    {
                        byte[] data = memory[..result.Count].ToArray();
                        string stringData = Encoding.UTF8.GetString(data);
                        Log("Received: " + stringData);
                        Message<object> msg = Deserialize(stringData);
                        string msgType = msg.Header.Type;
                        if (msgType == MessageType.DeviceAuthResut)
                        {
                            ProcessDeviceAuthResultMessage(msg);
                        }
                        else if (msgType == MessageType.DeviceSetStatus)
                        {
                            ProcessDeviceSetStatusMessage(msg);
                        }

                        //                        if (stringData.IndexOf("switch-to-default-desktop") >= 0)
                        //                        {
                        //                            Log("Switching to default desktop");
                        //#if !CCS3_NO_DESKTOP_SWITCH
                        //                            dm.SwitchToDesktop(defaultDesktopPtr);
                        //#endif
                        //                        }
                        //                        else if (stringData.IndexOf("switch-to-secured-desktop") >= 0)
                        //                        {
                        //                            Log("Switching to secured desktop");
                        //#if !CCS3_NO_DESKTOP_SWITCH
                        //                            dm.SwitchToDesktop(securedDesktopPtr);
                        //#endif
                        //                        }
                    }
                    else
                    {
                        // TODO: Collect bytes until the whole message is received
                        //message.AddRange(memory.Slice(0, result.Count).ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Log("Error on receiving: " + ex.ToString());
                    ConnectWebSocket(serverUri);
                    break;
                }
            }
        }

        private static void ProcessDeviceAuthResultMessage(Message<object> msg)
        {
            DeviceAuthResultMessage message = new DeviceAuthResultMessage
            {
                Header = msg.Header,
                // TODO: Find better way to convert to the real message body type
                Body = DeserializeBody<DeviceAuthResultMessageBody>(msg.Body)
            };
        }

        private static void ProcessDeviceSetStatusMessage(Message<object> msg)
        {
            DeviceSetStatusMessage message = new DeviceSetStatusMessage
            {
                Header = msg.Header,
                Body = DeserializeBody<DeviceSetStatusMessageBody>(msg.Body)
            };
            if (message.Body.AccessType == DeviceStatusAccessType.Disabled)
            {
                Log("Switching to default desktop");
                SwitchToDesktop(defaultDesktopPtr);
            }
            else if (message.Body.AccessType == DeviceStatusAccessType.Enabled)
            {
                Log("Switching to secured desktop");
                SwitchToDesktop(securedDesktopPtr);
            }
        }

        static async void SendMessage<TBody>(Message<TBody> message)
        {
            try
            {
                string json = Serialize(message);
                Log("Seinding: " + json);
                byte[] data = Encoding.UTF8.GetBytes(json);
                ReadOnlyMemory<byte> rom = new ReadOnlyMemory<byte>(data);
                await ws.SendAsync(rom, WebSocketMessageType.Binary, true, token);
            }
            catch (Exception ex)
            {
                Log("Cannot send " + message + " " + ex.ToString());
            }
        }

        static string Serialize(object obj)
        {
            string json = JsonSerializer.Serialize(obj, jsonSerializerOptions);
            return json;
        }

        static Message<object> Deserialize(string data)
        {
            Message<object> msg = JsonSerializer.Deserialize<Message<object>>(data, jsonSerializerOptions);
            return msg;
        }

        static TBody DeserializeBody<TBody>(object bodyObject)
        {
            // TODO: Not an good way to get generic body type
            TBody body = JsonSerializer.Deserialize<TBody>(Serialize(bodyObject), jsonSerializerOptions);
            return body;
        }

        static void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + ": " + message);
        }

        static void SwitchToDesktop(IntPtr desktopPointer)
        {
#if !CCS3_NO_DESKTOP_SWITCH
                dm.SwitchToDesktop(desktopPointer);
#endif
        }
    }
}
