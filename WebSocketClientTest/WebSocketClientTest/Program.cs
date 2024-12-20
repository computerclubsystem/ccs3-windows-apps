﻿using Microsoft.VisualBasic;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketClientTest.Messages;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest {
    internal class Program {
        static ClientWebSocket ws;
        static CancellationTokenSource cts = new CancellationTokenSource();
        static CancellationToken token;
        static string serverUri;
        static DesktopManager dm;
        static IntPtr defaultDesktopPtr;
        static IntPtr securedDesktopPtr;
        static JsonSerializerOptions jsonSerializerOptions;
        static void Main(string[] args) {
            jsonSerializerOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            dm = new DesktopManager();

            defaultDesktopPtr = dm.GetDefaultDesktopPointer();
            securedDesktopPtr = dm.CreateDesktop("Secured");

            token = cts.Token;
            serverUri = args[0];
            ConnectWebSocket(serverUri);
            while (true) {
                Console.WriteLine("1 to send short message, 2 to send 5 MB message");
                string line = Console.ReadLine();
                try {
                    if (line == "1") {
                        byte[] data = Encoding.UTF8.GetBytes("Message from C# client");
                        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(data);
                        ws.SendAsync(memory, WebSocketMessageType.Binary, true, token);
                    } else if (line == "2") {
                        byte[] data = new byte[5 * 1024 * 1024];
                        Random.Shared.NextBytes(data);
                        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(data);
                        ws.SendAsync(memory, WebSocketMessageType.Binary, true, token);
                    }
                } catch (Exception ex) {
                    Log("Cannot send data: " + ex.ToString());
                }
            }
        }

        static async void ConnectWebSocket(string uriString) {
            //ClientWebSocket ws = new ClientWebSocket();
            Uri uri = new Uri(uriString);
            while (true) {
                try {
                    var hhh = new HttpClientHandler();
                    ws = new ClientWebSocket();
                    X509Store store = new X509Store("MY", StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection collection = store.Certificates;
                    var ccs3DeviceCerts = collection.Find(X509FindType.FindByIssuerName, "CCS3 Root CA", false);
                    var ccs3DeviceTimeValidCerts = ccs3DeviceCerts.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                    var firstCcs3DeviceCert = ccs3DeviceTimeValidCerts.MaxBy(x => x.NotAfter); // .OrderByDescending(x => x.NotAfter).FirstOrDefault();
                    if (firstCcs3DeviceCert != null) {
                        // The current process must have access to the certificate private key
                        // otherwise the connection will fail
                        bool hasAccessToThePrivateKey;
                        try {
                            var pk = firstCcs3DeviceCert.GetRSAPrivateKey();
                            hasAccessToThePrivateKey = pk != null;
                            ws.Options.ClientCertificates = new X509Certificate2Collection(firstCcs3DeviceCert);
                            Log("Using certificate with thumbprint " + firstCcs3DeviceCert.Thumbprint);
                        } catch (Exception ex) {
                            Log("Access to the certificate private key is denied. Can't use it. Start the app as admin. " + ex.Message);
                        }
                        //ws.Options.ClientCertificates.Add(firstCcs3DeviceCert);
                    } else {
                        Log("Certificate with issuer name ccs3-device not found");
                    }
                    //X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select", "Select a certificate from the following list to get information on that certificate", X509SelectionFlag.MultiSelection);
                    //fcollection.Find(X509FindType.FindByIssuerName, "", true);
                    ws.Options.RemoteCertificateValidationCallback = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => {
                        return true;
                    };
                    Log("Connecting to " + uriString);
                    await ws.ConnectAsync(uri, token);
                    Log("Connected");
                    break;
                } catch (Exception ex) {
                    Log("Cannot connect. " + ex.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }
            StartReceiving();

            // TODO: Move this send
            //DeviceAuthMessage deviceAuthMessage = DeviceAuthMessageFactory.Create();
            //deviceAuthMessage.Body = new DeviceAuthMessageBody();
            //deviceAuthMessage.Body.DeviceId = deviceId;
            //deviceAuthMessage.Body.SoftwareVersion = "1.2.3";
            //SendMessage(deviceAuthMessage);
        }
        static async void StartReceiving() {
            //List<byte> message = new List<byte>();
            byte[] buffer = new byte[1 * 1024 * 1024];
            while (true) {
                Memory<byte> memory = new(buffer);
                try {
                    var result = await ws.ReceiveAsync(memory, token);
                    Log("Received " + result.Count + " bytes. End of message: " + result.EndOfMessage + ". Message type: " + result.MessageType);
                    if (result.Count > 0 && result.MessageType != WebSocketMessageType.Close) {
                        if (result.EndOfMessage) {
                            byte[] data = memory[..result.Count].ToArray();
                            string stringData = Encoding.UTF8.GetString(data);
                            Log("Received: " + stringData);
                            Message<object> msg = Deserialize(stringData);
                            string msgType = msg.Header.Type;
                            if (msgType == MessageType.DeviceSetStatus) {
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
                        } else {
                            // TODO: Collect bytes until the whole message is received
                            //message.AddRange(memory.Slice(0, result.Count).ToArray());
                        }
                    } else {
                        // Socket was closed - reconnect
                        Log("The socket has been closed, reconnecting");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        ConnectWebSocket(serverUri);
                        break;
                    }
                } catch (Exception ex) {
                    Log("Error on receiving: " + ex.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    ConnectWebSocket(serverUri);
                    break;
                }
            }
        }

        private static void ProcessDeviceSetStatusMessage(Message<object> msg) {
            DeviceSetStatusMessage message = new DeviceSetStatusMessage {
                Header = msg.Header,
                Body = DeserializeBody<DeviceSetStatusMessageBody>(msg.Body)
            };
            if (message.Body.State == DeviceState.Disabled) {
                Log("Switching to default desktop");
                SwitchToDesktop(defaultDesktopPtr);
            } else if (message.Body.State == DeviceState.Enabled) {
                Log("Switching to secured desktop");
                SwitchToDesktop(securedDesktopPtr);
            }
        }

        static async void SendMessage<TBody>(Message<TBody> message) {
            try {
                string json = Serialize(message);
                Log("Seinding: " + json);
                byte[] data = Encoding.UTF8.GetBytes(json);
                ReadOnlyMemory<byte> rom = new ReadOnlyMemory<byte>(data);
                await ws.SendAsync(rom, WebSocketMessageType.Binary, true, token);
            } catch (Exception ex) {
                Log("Cannot send " + message + " " + ex.ToString());
            }
        }

        static string Serialize(object obj) {
            string json = JsonSerializer.Serialize(obj, jsonSerializerOptions);
            return json;
        }

        static Message<object> Deserialize(string data) {
            Message<object> msg = JsonSerializer.Deserialize<Message<object>>(data, jsonSerializerOptions);
            return msg;
        }

        static TBody DeserializeBody<TBody>(object bodyObject) {
            // TODO: Not an good way to get generic body type
            TBody body = JsonSerializer.Deserialize<TBody>(Serialize(bodyObject), jsonSerializerOptions);
            return body;
        }

        static void Log(string message) {
            Console.WriteLine(DateTime.Now.ToString() + ": " + message);
        }

        static void SwitchToDesktop(IntPtr desktopPointer) {
#if !CCS3_NO_DESKTOP_SWITCH
                dm.SwitchToDesktop(desktopPointer);
#endif
        }
    }
}
