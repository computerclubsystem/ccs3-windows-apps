using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ccs3ClientApp;

public class WebSocketConnector {
    public event EventHandler<ConnectedEventArgs>? Connected;
    public event EventHandler<DisconnectedEventArgs>? Disconnected;
    public event EventHandler<ConnectErrorEventArgs>? ConnectError;
    public event EventHandler<ValidatingRemoteCertificateArgs>? ValidatingRemoteCertificate;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<ReceiveErrorEventArgs>? ReceiveError;
    public event EventHandler<SendDataErrorEventArgs>? SendDataError;

    private WebSocketConnectorConfig _config;
    private WebSocketConnectorState _state;

    public WebSocketConnector() {
        _state = new WebSocketConnectorState();
    }

    public void Initialize(WebSocketConnectorConfig config) {
        _config = config;
    }

    public void Start() {
        ConnectWebSocket(_config.ServerUri);
    }

    private void AbortCurrentWebSocket() {
        if (_state.WebSocket is not null) {
            try {
                _state.WebSocket.Abort();
            } catch { }
        }
    }

    private async void ConnectWebSocket(Uri uri) {
        while (true) {
            try {
                _state.WebSocket = new ClientWebSocket();
                if (_config.ClientCertificate is not null) {
                    _state.WebSocket.Options.ClientCertificates.Add(_config.ClientCertificate);
                }
                _state.WebSocket.Options.RemoteCertificateValidationCallback = ValidateRemoteCertificate;
                await _state.WebSocket.ConnectAsync(uri, _config.CancellationToken);
                Connected?.Invoke(this, new ConnectedEventArgs());
                break;
            } catch (Exception ex) {
                ConnectError?.Invoke(this, new ConnectErrorEventArgs { Exception = ex });
                AbortCurrentWebSocket();
                await DelayReconnect();
            }
        }
        StartReceiving();
    }

    private async void StartReceiving() {
        //List<byte> message = new List<byte>();
        byte[] buffer = new byte[1 * 1024 * 1024];
        while (true) {
            Memory<byte> memory = new(buffer);
            try {
                var result = await _state.WebSocket.ReceiveAsync(memory, _config.CancellationToken);
                if (result.Count > 0 && result.MessageType != WebSocketMessageType.Close) {
                    if (result.EndOfMessage) {
                        var receivedMemory = memory[..result.Count];
                        DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = receivedMemory });
                        //byte[] data = memory[..result.Count].ToArray();

                        //string stringData = Encoding.UTF8.GetString(data);
                        //Message<object> msg = Deserialize(stringData);
                        //string msgType = msg.Header.Type;
                        //if (msgType == MessageType.DeviceSetStatus) {
                        //    ProcessDeviceSetStatusMessage(msg);
                        //}

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
                } else if (result.Count == 0 || result.MessageType == WebSocketMessageType.Close) {
                    // Socket was closed - reconnect
                    Disconnected?.Invoke(this, new DisconnectedEventArgs());
                    AbortCurrentWebSocket();
                    await DelayReconnect();
                    ConnectWebSocket(_config.ServerUri);
                    break;
                }
            } catch (Exception ex) {
                ReceiveError?.Invoke(this, new ReceiveErrorEventArgs { Exception = ex });
                AbortCurrentWebSocket();
                await DelayReconnect();
                ConnectWebSocket(_config.ServerUri);
                break;
            }
        }
    }

    private async Task DelayReconnect() {
        await Task.Delay(_config.ReconnectDelay, _config.CancellationToken);
    }

    public async Task<bool> SendData(ReadOnlyMemory<byte> bytes) {
        try {
            await _state.WebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _config.CancellationToken);
            return true;
        } catch (Exception ex) {
            SendDataError?.Invoke(this, new SendDataErrorEventArgs { Exception = ex });
            return false;
        }
    }

    private bool ValidateRemoteCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) {
        if (_config.TrustAllServerCertificates == true) {
            return true;
        }
        ValidatingRemoteCertificate?.Invoke(
            this,
            new ValidatingRemoteCertificateArgs {
                Certificate = certificate,
                Chain = chain,
                SslPolicyError = sslPolicyErrors
            }
        );
        string serverCertificateHash = certificate!.GetCertHashString();
        bool thumbprintsAreSame = string.Equals(serverCertificateHash, _config.ServerCertificateThumbprint, StringComparison.OrdinalIgnoreCase);
        return thumbprintsAreSame;
    }

    private class WebSocketConnectorState {
        public ClientWebSocket WebSocket { get; set; }
    }
}

public class ConnectErrorEventArgs : EventArgs {
    public Exception Exception { get; set; }
}

public class ConnectedEventArgs : EventArgs {
}

public class DisconnectedEventArgs : EventArgs {
}

public class SendDataErrorEventArgs : EventArgs {
    public Exception Exception { get; set; }
}

public class ValidatingRemoteCertificateArgs : EventArgs {
    public X509Certificate? Certificate { get; set; }
    public X509Chain? Chain { get; set; }
    public SslPolicyErrors SslPolicyError { get; set; }
}

public class DataReceivedEventArgs : EventArgs {
    public Memory<byte> Data { get; set; }
}

public class ReceiveErrorEventArgs : EventArgs {
    public Exception Exception { get; set; }
}

public class WebSocketConnectorConfig {
    public X509Certificate2? ClientCertificate { get; set; }
    public Uri ServerUri { get; set; }
    public string? ServerCertificateThumbprint { get; set; }
    public bool TrustAllServerCertificates { get; set; }
    public TimeSpan ReconnectDelay { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
