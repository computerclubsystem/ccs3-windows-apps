using Ccs3ClientApp.Messages;
using Ccs3ClientApp.Messages.Declarations;
using Ccs3ClientApp.Messages.LocalClient;
using Ccs3ClientApp.Messages.LocalClient.Declarations;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Ccs3ClientApp {
    public partial class MainForm : Form {
        private MainFormState _state;
        private System.Windows.Forms.Timer _timer;

        public MainForm() {
            InitializeComponent();
            _state = new MainFormState();
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        // TODO: If we want to remove the close button
        //private const int CP_NOCLOSE_BUTTON = 0x200;
        //protected override CreateParams CreateParams {
        //    get {
        //        CreateParams myCp = base.CreateParams;
        //        myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
        //        return myCp;
        //    }
        //}

        private void _timer_Tick(object? sender, EventArgs e) {
            ProcessPingToServer();
            ProcessCurrentStatus();
        }

        private void CreateRestrictedDesktop() {
            //_state.DesktopService.CloseDesktop(_state.DesktopService.GetDefaultDesktopPointer());
            IntPtr restrictedAccessDesktopPointer = _state.DesktopService.CreateRestrictedAccessDesktop(_state.RestrictedAccessDesktopName);
            if (restrictedAccessDesktopPointer != IntPtr.Zero) {
                // TODO: Use cancellation token
                Task.Factory.StartNew(() => {
                    _state.DesktopService.SetCurrentThreadDesktop(restrictedAccessDesktopPointer);
                    RestrictedAccessDesktopForm radForm = new();
                    Application.Run(radForm);
                });
                Debug.WriteLine(string.Format("Restricted desktop has been created: {0}", _state.DesktopService.GetRestrictedAccessDesktopPointer()));
            } else {
                var lastError = _state.DesktopService.GetLastError();
                Debug.WriteLine(string.Format("Cannot create restricted access desktop. Error {0}", lastError));
            }
        }

        private void testButtonSwitchToRestrictedAccessDesktopFor3Seconds_Click(object sender, EventArgs e) {
            _state.DesktopService.SwitchToRestrictedAccessDesktop();
            Task.Run(() => {
                //_state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetRestrictedAccessDesktopPointer());
                Thread.Sleep(TimeSpan.FromSeconds(3));
                _state.DesktopService.SwitchToDefaultDesktop();
            });
        }

        private void MainForm_Load(object sender, EventArgs e) {
            notifyIconMain.Visible = true;
            notifyIconMain.Text = "Ccs3 Client App";
            notifyIconMain.Click += NotifyIconMain_Click;
            notifyIconMain.Icon = this.Icon;
            notifyIconMain.ShowBalloonTip(3000, "Ccs3 Client App", "От тук може да видите информация за текущата сесия", ToolTipIcon.Info);
            lblRemainingTimeValue.Text = "";
            Text = "Ccs3 Client App " + typeof(MainForm).Assembly.GetName().Version.ToString();
            Initialize();
        }

        private void NotifyIconMain_Click(object? sender, EventArgs e) {
            Show();
            if (this.WindowState == FormWindowState.Minimized) {
                this.WindowState = FormWindowState.Normal;
            }
            Activate();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                // Just hide the window - the user should be able to open it again from the systray icon
                Hide();
                e.Cancel = true;
                return;
            };

            IntPtr radPointer = _state.DesktopService.OpenDesktop(_state.RestrictedAccessDesktopName);
            if (radPointer != IntPtr.Zero) {
                _state.DesktopService.CloseDesktopPointer(radPointer);
            }
        }

        private void Initialize() {
            _state = new MainFormState();
            _state.CancellationToken = new CancellationToken();
            _state.DesktopService = new RestrictedAccessDesktopService();
            _state.JsonSerializerOptions = CreateJsonSerializerOptions();
            _state.LastServerPingSentAt = DateTimeOffset.Now;
            string? localServiceUriString = Environment.GetEnvironmentVariable("CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL", EnvironmentVariableTarget.Machine) ?? "https://localhost:30000";
            bool localServiceUriParsed = Uri.TryCreate(localServiceUriString, UriKind.Absolute, out Uri? localServiceUri);
            if (localServiceUri is not null) {
                if (localServiceUri.Scheme != Uri.UriSchemeWss) {
                    // The scheme is not wss - use wss
                    UriBuilder ub = new(localServiceUri);
                    ub.Scheme = Uri.UriSchemeWss;
                    localServiceUri = ub.Uri;
                }
                //// TODO: Add command line connection token parameter
                //UriBuilder ub = new(localServiceUri);
                //var query = HttpUtility.ParseQueryString(ub.Query);
                //query.Set("connection-token", "connection-token-value");
                //ub.Query = query.ToString();
                //localServiceUri = ub.Uri;
                _state.LocalServiceUri = localServiceUri;
                _state.LocalServiceConnector = new WebSocketConnector();
                WebSocketConnectorConfig connectorCfg = new() {
                    CancellationToken = _state.CancellationToken,
                    // TODO: For now we will not use ClientApp certificate
                    ClientCertificate = null,
                    // TODO: For now we will trust all server certificates
                    ServerCertificateThumbprint = null,
                    TrustAllServerCertificates = true,
                    ReconnectDelay = TimeSpan.FromSeconds(3),
                    ServerUri = localServiceUri,
                };
                _state.LocalServiceConnector.Initialize(connectorCfg);
                _state.LocalServiceConnector.Connected += LocalServiceConnector_Connected;
                _state.LocalServiceConnector.ConnectError += LocalServiceConnector_ConnectError;
                _state.LocalServiceConnector.Disconnected += LocalServiceConnector_Disconnected;
                _state.LocalServiceConnector.ValidatingRemoteCertificate += LocalServiceConnector_ValidatingRemoteCertificate;
                _state.LocalServiceConnector.DataReceived += LocalServiceConnector_DataReceived;
                _state.LocalServiceConnector.ReceiveError += LocalServiceConnector_ReceiveError;
                _state.LocalServiceConnector.SendDataError += LocalServiceConnector_SendDataError;
                _state.LocalServiceConnector.Start();
            } else {
                // TODO: Cannot parse local service Uri string
            }
            CreateRestrictedDesktop();
        }

        private void LocalServiceConnector_SendDataError(object? sender, SendDataErrorEventArgs e) {
        }

        private void LocalServiceConnector_ReceiveError(object? sender, ReceiveErrorEventArgs e) {
        }

        private void LocalServiceConnector_DataReceived(object? sender, DataReceivedEventArgs e) {
            if (e.Data.Length == 0) {
                return;
            }
            try {
                string stringData = Encoding.UTF8.GetString(e.Data.ToArray());
                try {
                    ProcessServiceConnectorReceivedMessage(stringData);
                } catch (Exception ex) {

                }
            } catch (Exception ex) {

            }
        }

        private void ProcessServiceConnectorReceivedMessage(string stringData) {
            LocalClientPartialMessage partialMsg = DeserializeLocalClientPartialMessage(stringData);
            if (partialMsg?.Header?.Type == null) {
                // TODO: Can't process the message
                return;
            }

            string msgType = partialMsg.Header.Type;
            switch (msgType) {
                case LocalClientNotificationMessageType.Configuration:
                    //// TODO: Deserialize to concrete message, not to LocalClientNotificationMessage<TBody>
                    //var genericMsg = DeserializeLocalClientNotificationMessage<LocalClientConfigurationNotificationMessageBody>(stringData);
                    //LocalClientConfigurationNotificationMessage msg = new() {
                    //    Header = genericMsg.Header,
                    //    Body = genericMsg.Body,
                    //};
                    //ProcessLocalClientConfigurationNotificationMessage(msg);
                    var configurationNotificationMsg = DeserializeLocalClientNotificationMessage<LocalClientConfigurationNotificationMessage, LocalClientConfigurationNotificationMessageBody>(stringData);
                    ProcessLocalClientConfigurationNotificationMessage(configurationNotificationMsg);
                    break;
                case LocalClientNotificationMessageType.Status:
                    var statusNotificationMsg = DeserializeLocalClientNotificationMessage<LocalClientStatusNotificationMessage, LocalClientStatusNotificationMessageBody>(stringData);
                    ProcessLocalClientStatusNotificationMessage(statusNotificationMsg);
                    break;
                default:
                    // TODO: Unknown message type
                    break;
            }
            //switch (msgType) {
            //    case MessageType.DeviceConfiguration:
            //        Message<DeviceConfigurationNotificationMessageBody> deviceConfigurationMsg = CreateTypedMessageFromGenericMessage<DeviceConfigurationNotificationMessageBody>(msg);
            //        ProcessDeviceConfigurationNotificationMessage(deviceConfigurationMsg);
            //        break;
            //    case MessageType.DeviceSetStatus:
            //        Message<DeviceSetStatusNotificationMessageBody> deviceSetStatusNotificationMsg = CreateTypedMessageFromGenericMessage<DeviceSetStatusNotificationMessageBody>(msg);
            //        ProcessDeviceSetStatusNotificationMessage(deviceSetStatusNotificationMsg);
            //        break;
            //}
        }

        private void ProcessLocalClientStatusNotificationMessage(LocalClientStatusNotificationMessage msg) {
            _state.StatusNotificationMessage = msg;
            ProcessCurrentStatus();
        }

        private void ProcessLocalClientConfigurationNotificationMessage(LocalClientConfigurationNotificationMessage msg) {
            _state.ConfigurationNotificationMessage = msg;
        }

        private LocalClientPartialMessage DeserializeLocalClientPartialMessage(string jsonString) {
            var deserialized = JsonSerializer.Deserialize<LocalClientPartialMessage>(jsonString, _state.JsonSerializerOptions);
            return deserialized;
        }

        private LocalClientNotificationMessage<TBody> DeserializeLocalClientNotificationMessageBody<TBody>(string jsonString) {
            var deserialized = JsonSerializer.Deserialize<LocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
            var deserializedMsg = CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
            return deserializedMsg;
        }

        private void SafeChangeUI(Action action) {
            if (InvokeRequired) {
                Invoke(action);
            } else {
                action();
            }
        }

        //private LocalClientNotificationMessage<TBody> DeserializeLocalClientNotificationMessage<TBody>(string jsonString) {
        //    var deserialized = JsonSerializer.Deserialize<LocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
        //    var deserializedMsg = CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
        //    return deserializedMsg;
        //}

        private TMessage DeserializeLocalClientNotificationMessage<TMessage, TBody>(string jsonString) where TMessage : LocalClientNotificationMessage<TBody>, new() {
            var deserialized = JsonSerializer.Deserialize<LocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
            // TODO: Can we use reflection to infer the TBody ?
            var deserializedMsg = CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
            TMessage result = new TMessage();
            result.Header = deserializedMsg.Header;
            result.Body = deserializedMsg.Body;
            return result;
        }

        private TBody DeserializeBody<TBody>(string body) {
            var deserialized = JsonSerializer.Deserialize<TBody>(body, _state.JsonSerializerOptions);
            return deserialized;
        }

        private LocalClientNotificationMessage<TBody> CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(LocalClientNotificationMessage<object> msg) {
            string bodyString = msg.Body.ToString()!;
            TBody? body = DeserializeBody<TBody>(bodyString);
            LocalClientNotificationMessage<TBody> deserializedMsg = new LocalClientNotificationMessage<TBody>();
            deserializedMsg.Header = msg.Header;
            deserializedMsg.Body = body!;
            return deserializedMsg;
        }

        private void LocalServiceConnector_ValidatingRemoteCertificate(object? sender, ValidatingRemoteCertificateArgs e) {
        }

        private void LocalServiceConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        }

        private void LocalServiceConnector_ConnectError(object? sender, ConnectErrorEventArgs e) {
        }

        private async void LocalServiceConnector_Connected(object? sender, ConnectedEventArgs e) {
            //byte[] bytes = Encoding.UTF8.GetBytes("{\"header\":{\"type\":\"ping-request\"},\"body\":{}}");
            //ReadOnlyMemory<byte> ro = new ReadOnlyMemory<byte>(bytes);
            //await _state.LocalServiceConnector.SendData(ro);
        }

        private void ProcessCurrentStatus() {
            // TODO: Deal with desktop switching, displaying session information etc.
            if (_state.StatusNotificationMessage?.Body == null) {
                return;
            }
            bool started = _state.StatusNotificationMessage.Body.Started;
            bool shouldSwitchToSecuredDesktop = !started;
#if !DEBUG
            SwitchToDesktop(shouldSwitchToSecuredDesktop);
#endif
            SafeChangeUI(() => {
                var amounts = _state.StatusNotificationMessage.Body.Amounts;
                if (amounts.RemainingSeconds.HasValue && amounts.RemainingSeconds.Value > 0) {
                    //var ts = TimeSpan.FromSeconds(amounts.RemainingSeconds.Value);
                    //var hoursText = ts.Hours > 0 ? $"{ts.Hours}ч." : "";
                    //var minutesText = ts.Minutes > 0 ? $"{ts.Minutes}м." : "";
                    //var secondsText = ts.Seconds > 0 ? $"{ts.Seconds}с." : "";
                    //string[] parts = new string[] { hoursText, minutesText, secondsText };
                    //parts = parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    //string finalText = string.Join(" ", parts);
                    string finalText = SecondsToDurationText(amounts.RemainingSeconds.Value);
                    lblRemainingTimeValue.Text = finalText;
                    notifyIconMain.Text = "Ccs3 Client App. Оставащо време " + finalText;
                } else {
                    lblRemainingTimeValue.Text = "0";
                }
                if (amounts.StartedAt.HasValue) {
                    var startedAt = DateTimeOffset.FromUnixTimeMilliseconds(amounts.StartedAt.Value);
                    string startedAtText = startedAt.LocalDateTime.ToString();
                    lblStartedAtValue.Text = startedAtText;
                } else {
                    lblStartedAtValue.Text = "-";
                }
                if (amounts.TotalTime.HasValue) {
                    string finalText = SecondsToDurationText(amounts.TotalTime.Value);
                    lblTotalTimeValue.Text = finalText;
                } else {
                    lblTotalTimeValue.Text = "0";
                }
                if (amounts.TotalSum.HasValue) {
                    var mainValue = decimal.Truncate(amounts.TotalSum.Value);
                    var fractionalValue = Math.Floor((amounts.TotalSum.Value % 1m) * 100);
                    string mainText = $"{mainValue}";
                    string fractionalText = $"{fractionalValue}".PadLeft(2, '0');
                    string finalText = $"{mainText}.{fractionalText}";
                    lblTotalSumValue.Text = finalText;
                }
            });
        }

        private string SecondsToDurationText(long seconds) {
            var ts = TimeSpan.FromSeconds(seconds);
            var hoursText = ts.Hours > 0 ? $"{ts.Hours}ч." : "";
            var minutesText = ts.Minutes > 0 ? $"{ts.Minutes}м." : "";
            var secondsText = ts.Seconds > 0 ? $"{ts.Seconds}с." : "";
            string[] parts = new string[] { hoursText, minutesText, secondsText };
            parts = parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            string finalText = string.Join(" ", parts);
            return finalText;
        }

        private void SwitchToDesktop(bool secured) {
            if (secured) {
                _state.DesktopService.SwitchToRestrictedAccessDesktop();
                //_state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetRestrictedAccessDesktopPointer());
            } else {
                _state.DesktopService.SwitchToDefaultDesktop();
                //_state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetDefaultDesktopPointer());
            }

            //Task.Run(() => {
            //    _state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetRestrictedAccessDesktopPointer());
            //    Thread.Sleep(TimeSpan.FromSeconds(3));
            //    _state.DesktopService.SwitchToDefaultDesktop();
            //}
        }

        private void ProcessPingToServer() {
            if (_state.ConfigurationNotificationMessage == null) {
                return;
            }
            var now = GetNow();
            var diff = now - _state.LastServerPingSentAt;
            if (diff.TotalMilliseconds > _state.ConfigurationNotificationMessage.Body.PingInterval) {
                _state.LastServerPingSentAt = now;
                SendPingToServer();
            }
        }

        private async Task SendPingToServer() {
            byte[] bytes = Encoding.UTF8.GetBytes("{\"header\":{\"type\":\"local-client-ping-request\"},\"body\":{}}");
            ReadOnlyMemory<byte> ro = new ReadOnlyMemory<byte>(bytes);
            await _state.LocalServiceConnector.SendData(ro);
        }

        private DateTimeOffset GetNow() {
            return DateTimeOffset.Now;
        }

        private JsonSerializerOptions CreateJsonSerializerOptions() {
            JsonSerializerOptions options = new() {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.Strict,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            return options;
        }

        private class MainFormState {
            public RestrictedAccessDesktopService DesktopService { get; set; }
            public readonly string RestrictedAccessDesktopName = "RestrictedAccessDesktop";
            public Uri LocalServiceUri { get; set; }
            public WebSocketConnector LocalServiceConnector { get; set; }
            public CancellationToken CancellationToken { get; set; }
            public LocalClientConfigurationNotificationMessage? ConfigurationNotificationMessage { get; set; }
            public LocalClientStatusNotificationMessage? StatusNotificationMessage { get; set; }
            public JsonSerializerOptions JsonSerializerOptions { get; set; }
            public DateTimeOffset LastServerPingSentAt { get; set; }
        }

        private void btnTest_Click(object sender, EventArgs e) {
            Task.Run(() => {
                //lblTest.Text = "АСДФ";
                //btnTest.Text = "ЯВЕР";
                //cmbTest.Items.Add("ЗЦЖБ");
                _state.StatusNotificationMessage = new LocalClientStatusNotificationMessage {
                    Body = new LocalClientStatusNotificationMessageBody() {
                        Started = true,
                        Amounts = new Messages.Types.DeviceStatusAmounts {
                            RemainingSeconds = Random.Shared.Next(100, 50000),
                            TotalSum = (decimal)Math.Round(Random.Shared.NextDouble() * 156, 2),
                            TotalTime = Random.Shared.Next(100, 2000),
                            StartedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                        }
                    }
                };
                ProcessCurrentStatus();
            });
        }
    }
}
