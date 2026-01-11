using Ccs3ClientApp.Messages;
using Ccs3ClientApp.Messages.Declarations;
using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Ccs3ClientApp;

public partial class MainForm : Form {
    private MainFormState _state;
    private System.Windows.Forms.Timer _timer;
    private HotKeyManager _hotKeyManager = new HotKeyManager();

    public MainForm() {
        InitializeComponent();
        _state = new MainFormState();
        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = 1000;
        _timer.Tick += _timer_Tick;
        _timer.Start();
        var ci = CultureInfo.GetCultureInfo("bg-BG");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        _hotKeyManager.KeyPressed +=
            new EventHandler<KeyPressedEventArgs>(hotKeyManager_KeyPressed);
        _hotKeyManager.RegisterHotKey(
            HotKeyModifierKeys.Control | HotKeyModifierKeys.Shift,
            Keys.F12
        );
    }

    void hotKeyManager_KeyPressed(object? sender, KeyPressedEventArgs e) {
        // Process hotkeys only if not on restricted desktop
        if (_state.CurrentStatusNotificationMessage?.Body == null) {
            return;
        }
        bool started = _state.CurrentStatusNotificationMessage.Body.Started;
        if (!started) {
            // Not started - do not process hot keys
            return;
        }
        // TODO: Instead of using TopMost as indicator whether we should show or hide the window
        //       we should have a flag, which will be maintained on hot key and when the window
        //       is manually shown / closed by the user
        if (e.Key == Keys.F12 && e.Modifier == (HotKeyModifierKeys.Control | HotKeyModifierKeys.Shift)) {
            if (TopMost) {
                HideMainWindow();
            } else {
                ShowMainWindowOnTop();
            }
        }
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
        ProcessQrCodeExpiration();
    }

    private async void ProcessQrCodeExpiration() {
        if (_state.CurrentStatusNotificationMessage?.Body?.Started == true) {
            return;
        }
        if (_state.ConfigurationNotificationMessage?.Body?.FeatureFlags?.CodeSignIn != true) {
            return;
        }
        if (_state.CreateSignInCodeReplyMessage?.Body?.RemainingSeconds is null) {
            return;
        }
        var now = GetNow();
        var validTo = _state.CreateSignInCodeReplyMessageReceivedAt!.Value.AddSeconds(_state.CreateSignInCodeReplyMessage.Body.RemainingSeconds.Value);
        var diff = validTo - now;
        var remainingSeconds = (int)diff.TotalSeconds;
        if (remainingSeconds <= 0) {
            // Automatically refresh the code if shown to the user
            _state.CreateSignInCodeReplyMessage = null;
            _state.CreateSignInCodeReplyMessageReceivedAt = null;
            this.SetQrCodeVisibility(false);
            await SendLocalClientToDeviceCreateSignInCodeRequestMessage();
        } else {
            _state.RestrictedAccessDesktopForm.SetQrCodeRemainingSeconds(remainingSeconds);
            SetQrCodeVisibility(true);
        }
    }

    private void SetQrCodeVisibility(bool desiredVisibility) {
        bool qrCodeFeatureEnabled = _state.ConfigurationNotificationMessage?.Body?.FeatureFlags?.CodeSignIn == true;
        bool hasCurrentStatus = _state.CurrentStatusNotificationMessage is not null;
        bool started = _state.CurrentStatusNotificationMessage?.Body?.Started is true;
        bool qrCodeVisible = hasCurrentStatus && !started && qrCodeFeatureEnabled && desiredVisibility;
        _state.RestrictedAccessDesktopForm.SetQrCodeVisibility(qrCodeVisible);
    }

    private void CreateRestrictedDesktop() {
#if DEBUG
        DebugCreateRestrictedDesktop();
        return;
#endif
        //_state.DesktopService.CloseDesktop(_state.DesktopService.GetDefaultDesktopPointer());
        IntPtr restrictedAccessDesktopPointer = _state.DesktopService.CreateRestrictedAccessDesktop(_state.RestrictedAccessDesktopName);
        if (restrictedAccessDesktopPointer != IntPtr.Zero) {
            // TODO: Use cancellation token
            Task.Factory.StartNew(() => {
                _state.DesktopService.SetCurrentThreadDesktop(restrictedAccessDesktopPointer);
                _state.RestrictedAccessDesktopForm = new();
                _state.RestrictedAccessDesktopForm.CustomerSignIn += RadForm_CustomerSignIn;
                _state.RestrictedAccessDesktopForm.RestartNow += RadForm_RestartNow;
                SetQrCodeVisibility(false);
                Application.Run(_state.RestrictedAccessDesktopForm);
            });
            Debug.WriteLine(string.Format("Restricted desktop has been created: {0}", _state.DesktopService.GetRestrictedAccessDesktopPointer()));
        } else {
            var lastError = _state.DesktopService.GetLastError();
            Debug.WriteLine(string.Format("Cannot create restricted access desktop. Error {0}", lastError));
        }
    }

    private async void RadForm_RestartNow(object? sender, EventArgs e) {
        var reqMsg = LocalClientToDeviceRestartNowRequestMessageHelper.CreateMessage();
        await SendMessage(reqMsg);
    }

    private void DebugCreateRestrictedDesktop() {
        Task.Factory.StartNew(() => {
            _state.RestrictedAccessDesktopForm = new();
            _state.RestrictedAccessDesktopForm.WindowState = FormWindowState.Normal;
            _state.RestrictedAccessDesktopForm.FormBorderStyle = FormBorderStyle.Sizable;
            _state.RestrictedAccessDesktopForm.MinimizeBox = true;
            _state.RestrictedAccessDesktopForm.MaximizeBox = true;
            _state.RestrictedAccessDesktopForm.ControlBox = true;
            _state.RestrictedAccessDesktopForm.ShowInTaskbar = true;
            _state.RestrictedAccessDesktopForm.CustomerSignIn += RadForm_CustomerSignIn;
            _state.RestrictedAccessDesktopForm.RestartNow += RadForm_RestartNow;
            SetQrCodeVisibility(false);
            Application.Run(_state.RestrictedAccessDesktopForm);
        });

    }

    private async void RadForm_CustomerSignIn(object? sender, CustomerSignInEventArgs e) {
        await SendLocalClientToDeviceStartOnPrepaidTariffRequestMessage(e.CustomerCardID, e.PasswordHash);
    }

    private async Task SendLocalClientToDeviceStartOnPrepaidTariffRequestMessage(int tariffId, string passwordHash) {
        var reqMsg = LocalClientToDeviceStartOnPrepaidTariffRequestMessageHelper.CreateMessage();
        reqMsg.Body.TariffId = tariffId;
        reqMsg.Body.PasswordHash = passwordHash;
        await SendMessage(reqMsg);
    }

    private async Task SendLocalClientToDeviceEndDeviceSessionByCustomerRequestMessage() {
        var reqMsg = LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageHelper.CreateMessage();
        await SendMessage(reqMsg);
    }

    //private void testButtonSwitchToRestrictedAccessDesktopFor3Seconds_Click(object sender, EventArgs e) {
    //    _state.DesktopService.SwitchToRestrictedAccessDesktop();
    //    Task.Run(() => {
    //        //_state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetRestrictedAccessDesktopPointer());
    //        Thread.Sleep(TimeSpan.FromSeconds(3));
    //        _state.DesktopService.SwitchToDefaultDesktop();
    //    });
    //}

    private void MainForm_Load(object sender, EventArgs e) {
        notifyIconMain.Icon = this.Icon;
        notifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
        notifyIconMain.Visible = true;
        notifyIconMain.Text = "Ccs3 Client App";
        notifyIconMain.Click += NotifyIconMain_Click;
        notifyIconMain.ShowBalloonTip(3000, "Ccs3 Client App", "CTRL+SHIFT+F12 - От тук може да видите информация за текущата сесия", ToolTipIcon.Info);
        notifyIconMain.BalloonTipClicked += NotifyIconMain_BalloonTipClicked;
        lblRemainingTimeValue.Text = "";
        Text = "Ccs3 Client App " + typeof(MainForm).Assembly.GetName().Version.ToString();
        Initialize();
    }

    private void NotifyIconMain_BalloonTipClicked(object? sender, EventArgs e) {
        ShowAndActivateMainWindow();
    }

    private void NotifyIconMain_Click(object? sender, EventArgs e) {
        ShowAndActivateMainWindow();
    }

    private void ShowMainWindow() {
        Show();
        if (this.WindowState == FormWindowState.Minimized) {
            this.WindowState = FormWindowState.Normal;
        }
    }

    private void ShowMainWindowOnTop() {
        ShowMainWindow();
        Refresh();
        this.TopMost = true;
    }

    private void HideMainWindow() {
        Hide();
        this.TopMost = false;
    }

    private void ShowAndActivateMainWindow() {
        ShowMainWindow();
        Activate();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
        if (e.CloseReason == CloseReason.UserClosing) {
            // Mark it as not topmost so hotkey will show it when pressed
            TopMost = false;
            // Just hide the window - the user should be able to open it again from the systray icon
            Hide();
            e.Cancel = true;
            return;
        }

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
            _state.DeviceConnector = new WebSocketConnector();
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
            _state.DeviceConnector.Initialize(connectorCfg);
            _state.DeviceConnector.Connected += DeviceConnector_Connected;
            _state.DeviceConnector.ConnectError += DeviceConnector_ConnectError;
            _state.DeviceConnector.Disconnected += DeviceConnector_Disconnected;
            _state.DeviceConnector.ValidatingRemoteCertificate += DeviceConnector_ValidatingRemoteCertificate;
            _state.DeviceConnector.DataReceived += DeviceConnector_DataReceived;
            _state.DeviceConnector.ReceiveError += DeviceConnector_ReceiveError;
            _state.DeviceConnector.SendDataError += DeviceConnector_SendDataError;
            _state.DeviceConnector.Start();
        } else {
            // TODO: Cannot parse local service Uri string
        }
        CreateRestrictedDesktop();
    }

    private void DeviceConnector_SendDataError(object? sender, SendDataErrorEventArgs e) {
    }

    private void DeviceConnector_ReceiveError(object? sender, ReceiveErrorEventArgs e) {
        var deviceConnector = (WebSocketConnector)sender;
        if (deviceConnector.GetWebSocketState() != System.Net.WebSockets.WebSocketState.Open) {
            SetConnectionStatusUI(false);
        }
    }

    private void DeviceConnector_DataReceived(object? sender, DataReceivedEventArgs e) {
        if (e.Data.Length == 0) {
            return;
        }
        try {
            string stringData = Encoding.UTF8.GetString(e.Data.ToArray());
            try {
                ProcessDeviceConnectorReceivedMessage(stringData);
            } catch (Exception ex) {

            }
        } catch (Exception ex) {

        }
    }

    private void ProcessDeviceConnectorReceivedMessage(string stringData) {
        PartialMessage partialMsg = DeserializePartialMessage(stringData);
        if (partialMsg?.Header?.Type == null) {
            // TODO: Can't process the message
            return;
        }
        string msgType = partialMsg.Header.Type;
        switch (msgType) {
            case DeviceToLocalClientReplyMessageType.CreateSignInCode: {
                    var msg = DeserializeDeviceToLocalClientReplyMessage<DeviceToLocalClientCreateSignInCodeReplyMessage, DeviceToLocalClientCreateSignInCodeReplyMessageBody>(stringData);
                    ProcessDeviceToLocalClientCreateSignInCodeReplyMessage(msg);
                    break;
                }
            case DeviceToLocalClientNotificationMessageType.SessionWillEndSoon: {
                    var msg = DeserializeDeviceToLocalClientNotificationMessage<DeviceToLocalClientSessionWillEndSoonNotificationMessage, DeviceToLocalClientSessionWillEndSoonNotificationMessageBody>(stringData);
                    ProcessDeviceToLocalClientSessionWillEndSoonNotificationMessage(msg);
                    break;
                }
            case DeviceToLocalClientNotificationMessageType.SecondsBeforeRestart: {
                    var msg = DeserializeDeviceToLocalClientNotificationMessage<DeviceToLocalClientSecondsBeforeRestartNotificationMessage, DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody>(stringData);
                    ProcessDeviceToLocalClientSecondsBeforeRestartNotificationMessage(msg);
                    break;
                }
            case DeviceToLocalClientReplyMessageType.ChangePrepaidTariffPasswordByCustomer: {
                    var msg = DeserializeDeviceToLocalClientReplyMessage<DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage, DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageBody>(stringData);
                    ProcessDeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage(msg);
                    break;
                }
            case DeviceToLocalClientReplyMessageType.StartOnPrepaidTariff: {
                    var msg = DeserializeDeviceToLocalClientReplyMessage<DeviceToLocalClientStartOnPrepaidTariffReplyMessage, DeviceToLocalClientStartOnPrepaidTariffReplyMessageBody>(stringData);
                    ProcessDeviceToLocalClientStartOnPrepaidTariffReplyMessage(msg);
                    break;
                }
            case DeviceToLocalClientNotificationMessageType.CurrentStatus: {
                    var msg = DeserializeDeviceToLocalClientNotificationMessage<DeviceToLocalClientCurrentStatusNotificationMessage, DeviceToLocalClientCurrentStatusNotificationMessageBody>(stringData);
                    ProcessDeviceToLocalClientCurrentStatusNotificationMessage(msg);
                    break;
                }
            case DeviceToLocalClientNotificationMessageType.Configuration: {
                    var msg = DeserializeDeviceToLocalClientNotificationMessage<DeviceToLocalClientConfigurationNotificationMessage, DeviceToLocalClientConfigurationNotificationMessageBody>(stringData);
                    ProcessDeviceToLocalClientConfigurationNotificationMessage(msg);
                    break;
                }
            case DeviceToLocalClientNotificationMessageType.ConnectionStatus: {
                    var msg = DeserializeDeviceToLocalClientNotificationMessage<DeviceToLocalClientConnectionStatusNotificationMessage, DeviceToLocalClientConnectionStatusNotificationMessageBody>(stringData);
                    ProcessDeviceToLocalClientConnectionStatusNotificationMessage(msg);
                    break;
                }
        }
    }

    private void ProcessDeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage(DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage msg) {
        if (msg.Header.Failure.HasValue && msg.Header.Failure.Value == true) {
            MessageBox.Show("Failed to change the password. Check if current password is correct", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        } else {
            MessageBox.Show("Password has been changed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ProcessDeviceToLocalClientCreateSignInCodeReplyMessage(DeviceToLocalClientCreateSignInCodeReplyMessage msg) {
        _state.CreateSignInCodeReplyMessage = msg;
        _state.CreateSignInCodeReplyMessageReceivedAt = GetNow();
        if (msg.Header.Failure == true) {
            SetQrCodeVisibility(false);
            return;
        }
        if (msg.Body.Url is not null) {
            SetQrCodeVisibility(true);
            _state.RestrictedAccessDesktopForm.SetQrCodeUrl(msg.Body.Url);
        } else {
            SetQrCodeVisibility(false);
        }
    }

    private void ProcessDeviceToLocalClientStartOnPrepaidTariffReplyMessage(DeviceToLocalClientStartOnPrepaidTariffReplyMessage msg) {
        _state.RestrictedAccessDesktopForm.SetCustomerSignInResult(msg);
    }

    private TMessage DeserializeDeviceToLocalClientReplyMessage<TMessage, TBody>(string jsonString) where TMessage : DeviceToLocalClientReplyMessage<TBody>, new() {
        var deserialized = JsonSerializer.Deserialize<DeviceToLocalClientReplyMessage<object>>(jsonString, _state.JsonSerializerOptions);
        var deserializedMsg = CreateTypedDeviceToLocalClientReplyMessageFromGenericMessage<TBody>(deserialized);
        TMessage result = new TMessage();
        result.Header = deserializedMsg.Header;
        result.Body = deserializedMsg.Body;
        return result;
    }

    private TMessage DeserializeDeviceToLocalClientNotificationMessage<TMessage, TBody>(string jsonString) where TMessage : DeviceToLocalClientNotificationMessage<TBody>, new() {
        var deserialized = JsonSerializer.Deserialize<DeviceToLocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
        var deserializedMsg = CreateTypedDeviceToLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
        TMessage result = new TMessage();
        result.Header = deserializedMsg.Header;
        result.Body = deserializedMsg.Body;
        return result;
    }

    private DeviceToLocalClientNotificationMessage<TBody> CreateTypedDeviceToLocalClientNotificationMessageFromGenericMessage<TBody>(DeviceToLocalClientNotificationMessage<object> msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        DeviceToLocalClientNotificationMessage<TBody> deserializedMsg = new DeviceToLocalClientNotificationMessage<TBody>();
        deserializedMsg.Header = new DeviceToLocalClientNotificationMessageHeader {
            Type = msg.Header.Type,
        };
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private DeviceToLocalClientReplyMessage<TBody> CreateTypedDeviceToLocalClientReplyMessageFromGenericMessage<TBody>(DeviceToLocalClientReplyMessage<object> msg) {
        string bodyString = msg.Body.ToString()!;
        TBody? body = DeserializeBody<TBody>(bodyString);
        DeviceToLocalClientReplyMessage<TBody> deserializedMsg = new DeviceToLocalClientReplyMessage<TBody>();
        deserializedMsg.Header = new DeviceToLocalClientReplyMessageHeader();
        TransferReplyHeader(msg.Header, deserializedMsg.Header);
        deserializedMsg.Body = body!;
        return deserializedMsg;
    }

    private void TransferReplyHeader(ReplyMessageHeader source, ReplyMessageHeader destination) {
        destination.CorrelationId = source.CorrelationId;
        destination.MessageErrors = source.MessageErrors;
        destination.Failure = source.Failure;
        destination.Type = source.Type;
    }

    private void ProcessDeviceToLocalClientSessionWillEndSoonNotificationMessage(DeviceToLocalClientSessionWillEndSoonNotificationMessage msg) {
        SafeChangeUI(() => {
            string tooltipTitle = "Your session will end in " + SecondsToDurationText(msg.Body.RemainingSeconds);
            // The timeout is ignored - the baloon timeouts are controlled by accessibility settings of the OS
            int timeout = 10000;
            // If string.Empty is provided for tooltip message, it is not shown, space is enough to show tooltip with only title
            notifyIconMain.ShowBalloonTip(timeout, tooltipTitle, " ", ToolTipIcon.Warning);
            if (!string.IsNullOrWhiteSpace(msg.Body.NotificationSoundFile)) {
                try {
                    using var player = new SoundPlayer(msg.Body.NotificationSoundFile);
                    player.Play();
                } catch { }
            }
        });
    }

    private void ProcessDeviceToLocalClientSecondsBeforeRestartNotificationMessage(DeviceToLocalClientSecondsBeforeRestartNotificationMessage msg) {
        _state.RestrictedAccessDesktopForm.SetSecondsBeforeRestart(msg.Body.Seconds);
    }

    private void ProcessDeviceToLocalClientCurrentStatusNotificationMessage(DeviceToLocalClientCurrentStatusNotificationMessage msg) {
        _state.CurrentStatusNotificationMessage = msg;
        ProcessCurrentStatus();
    }

    private async void ProcessDeviceToLocalClientConnectionStatusNotificationMessage(DeviceToLocalClientConnectionStatusNotificationMessage msg) {
        _state.ConnectionStatusNotificationMessage = msg;
        SetConnectionStatusUI(msg.Body.Connected);
    }

    private void SetConnectionStatusUI(bool connected) {
        SafeChangeUI(() => {
            lblConnectionStatus.Visible = !connected;
            _state.RestrictedAccessDesktopForm.SetConnectionStatus(connected);
        });
    }

    private async void ProcessDeviceToLocalClientConfigurationNotificationMessage(DeviceToLocalClientConfigurationNotificationMessage msg) {
        _state.ConfigurationNotificationMessage = msg;
        if (_state.ConfigurationNotificationMessage.Body.FeatureFlags?.CodeSignIn == true) {
            // Request new QR code
            await SendLocalClientToDeviceCreateSignInCodeRequestMessage();
        } else {
            SetQrCodeVisibility(false);
        }
    }

    private async Task SendLocalClientToDeviceCreateSignInCodeRequestMessage() {
        if (_state.CurrentStatusNotificationMessage?.Body?.Started is true
            || _state.ConfigurationNotificationMessage?.Body?.FeatureFlags?.CodeSignIn is not true
            ) {
            return;
        }
        var createSignInCodeReqMsg = LocalClientToDeviceCreateSignInCodeRequestMessageHelper.CreateMessage();
        await SendMessage(createSignInCodeReqMsg);
    }

    //private LocalClientPartialMessage DeserializeLocalClientPartialMessage(string jsonString) {
    //    var deserialized = JsonSerializer.Deserialize<LocalClientPartialMessage>(jsonString, _state.JsonSerializerOptions);
    //    return deserialized;
    //}

    private PartialMessage DeserializePartialMessage(string jsonString) {
        var deserialized = JsonSerializer.Deserialize<PartialMessage>(jsonString, _state.JsonSerializerOptions);
        return deserialized;
    }

    private DeviceToLocalClientNotificationMessage<TBody> DeserializeDeviceToLocalClientNotificationMessageBody<TBody>(string jsonString) {
        var deserialized = JsonSerializer.Deserialize<DeviceToLocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
        var deserializedMsg = CreateTypedDeviceToLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
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

    //private TMessage DeserializeLocalClientNotificationMessage<TMessage, TBody>(string jsonString) where TMessage : LocalClientNotificationMessage<TBody>, new() {
    //    var deserialized = JsonSerializer.Deserialize<LocalClientNotificationMessage<object>>(jsonString, _state.JsonSerializerOptions);
    //    // TODO: Can we use reflection to infer the TBody ?
    //    var deserializedMsg = CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(deserialized);
    //    TMessage result = new TMessage();
    //    result.Header = deserializedMsg.Header;
    //    result.Body = deserializedMsg.Body;
    //    return result;
    //}

    private TBody DeserializeBody<TBody>(string body) {
        var deserialized = JsonSerializer.Deserialize<TBody>(body, _state.JsonSerializerOptions);
        return deserialized;
    }

    //private LocalClientNotificationMessage<TBody> CreateTypedLocalClientNotificationMessageFromGenericMessage<TBody>(LocalClientNotificationMessage<object> msg) {
    //    string bodyString = msg.Body.ToString()!;
    //    TBody? body = DeserializeBody<TBody>(bodyString);
    //    LocalClientNotificationMessage<TBody> deserializedMsg = new LocalClientNotificationMessage<TBody>();
    //    deserializedMsg.Header = msg.Header;
    //    deserializedMsg.Body = body!;
    //    return deserializedMsg;
    //}

    private ReadOnlyMemory<byte> SerializeMessage(object message) {
        var serializedText = JsonSerializer.Serialize(message, _state.JsonSerializerOptions);
        byte[] serializedByteArray = Encoding.UTF8.GetBytes(serializedText);
        ReadOnlyMemory<byte> ro = new ReadOnlyMemory<byte>(serializedByteArray);
        return serializedByteArray;
    }

    private void DeviceConnector_ValidatingRemoteCertificate(object? sender, ValidatingRemoteCertificateArgs e) {
    }

    private void DeviceConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        SetConnectionStatusUI(false);
    }

    private void DeviceConnector_ConnectError(object? sender, ConnectErrorEventArgs e) {
    }

    private async void DeviceConnector_Connected(object? sender, ConnectedEventArgs e) {
        //byte[] bytes = Encoding.UTF8.GetBytes("{\"header\":{\"type\":\"ping-request\"},\"body\":{}}");
        //ReadOnlyMemory<byte> ro = new ReadOnlyMemory<byte>(bytes);
        //await _state.LocalServiceConnector.SendData(ro);
    }

    private void HandleStartedToStoppedTransition() {
        _state.StartedToStoppedTransitionDate = DateTimeOffset.Now;
        SetQrCodeVisibility(false);
        SendLocalClientToDeviceCreateSignInCodeRequestMessage();
        _state.RestrictedAccessDesktopForm.ChangeStartedState(false);
    }

    private void HandleStoppedToStartedTransition() {
        _state.StartedToStoppedTransitionDate = null;
        SetQrCodeVisibility(false);
        _state.RestrictedAccessDesktopForm.ChangeStartedState(true);
    }

    private void ProcessCurrentStatus() {
        // Deal with desktop switching, displaying session information etc.
        if (_state.CurrentStatusNotificationMessage?.Body == null) {
            return;
        }
        bool started = _state.CurrentStatusNotificationMessage.Body.Started;
        if (started) {
            _state.RestrictedAccessDesktopForm.SetSecondsBeforeRestart(0);
            _state.CreateSignInCodeReplyMessageReceivedAt = null;
            _state.CreateSignInCodeReplyMessage = null;
            _state.RestrictedAccessDesktopForm.SetQrCodeRemainingSeconds(0);
            SetQrCodeVisibility(false);
        }
        bool shouldSwitchToSecuredDesktop = !started;
        if (_state.StartedState == true && started == false) {
            // Was started but now it is stopped
            HandleStartedToStoppedTransition();
        } else if (_state.StartedState == false && started == true) {
            // Was stopped but now it is started
            HandleStoppedToStartedTransition();
        }
        _state.StartedState = started;

#if !DEBUG
        SwitchToDesktop(shouldSwitchToSecuredDesktop);
#endif
        SafeChangeUI(() => {
            bool canBeStoppedByCustomer = _state.CurrentStatusNotificationMessage.Body.CanBeStoppedByCustomer.HasValue ? _state.CurrentStatusNotificationMessage.Body.CanBeStoppedByCustomer.Value : false;
            if (started && canBeStoppedByCustomer) {
                gbCustomerCardGroup.Visible = true;
            } else {
                gbCustomerCardGroup.Visible = false;
            }

            if (canBeStoppedByCustomer && _state.CurrentStatusNotificationMessage.Body.TariffId.HasValue) {

            }
            var amounts = _state.CurrentStatusNotificationMessage.Body.Amounts;
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
                notifyIconMain.Text = "Ccs3 Client App. Remaining time " + finalText;
            } else {
                lblRemainingTimeValue.Text = "0";
            }
            var continuationTariffInfo = _state.CurrentStatusNotificationMessage.Body.ContinuationTariffShortInfo;
            if (continuationTariffInfo != null) {
                string tariffDurationText = string.Empty;
                if (continuationTariffInfo.Duration.HasValue) {
                    tariffDurationText = SecondsToDurationText(continuationTariffInfo.Duration.Value * 60);
                }
                string finalText = continuationTariffInfo.Name + (!string.IsNullOrWhiteSpace(tariffDurationText) ? $" ( {tariffDurationText} )" : string.Empty);
                lblContinuationTariffValue.Text = finalText;
                lblContinuationTariff.Visible = true;
                lblContinuationTariffValue.Visible = true;
            } else {
                lblContinuationTariff.Visible = false;
                lblContinuationTariffValue.Visible = false;
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
                //var mainValue = decimal.Truncate(amounts.TotalSum.Value);
                //var fractionalValue = Math.Floor((amounts.TotalSum.Value % 1m) * 100);
                //string mainText = $"{mainValue}";
                //string fractionalText = $"{fractionalValue}".PadLeft(2, '0');
                string finalText = GetAmountString(amounts.TotalSum.Value);
                if (_state.ConfigurationNotificationMessage?.Body?.FeatureFlags is not null) {
                    bool hasSecondPrice = _state.ConfigurationNotificationMessage.Body.FeatureFlags.SecondPrice;
                    if (hasSecondPrice && amounts.TotalSumSecondPrice is not null) {
                        string secondPriceString = GetAmountString(amounts.TotalSumSecondPrice.Value);
                        finalText += $" / {secondPriceString} {_state.ConfigurationNotificationMessage.Body.SecondPriceCurrency}";
                    }
                }
                lblTotalSumValue.Text = finalText;
            }
        });
    }

    private string GetAmountString(decimal amount) {
        var mainValue = decimal.Truncate(amount);
        var fractionalValue = Math.Floor((amount % 1m) * 100);
        string mainText = $"{mainValue}";
        string fractionalText = $"{fractionalValue}".PadLeft(2, '0');
        string finalText = $"{mainText}.{fractionalText}";
        return finalText;
    }

    private string SecondsToDurationText(long seconds) {
        var ts = TimeSpan.FromSeconds(seconds);
        var hours = ts.Days * 24 + ts.Hours;
        var hoursText = hours > 0 ? $"{hours}h." : "";
        var minutesText = ts.Minutes > 0 ? $"{ts.Minutes}m." : "";
        var secondsText = ts.Seconds > 0 ? $"{ts.Seconds}s." : "";
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
        var reqMsg = LocalClientToDevicePingNotificationMessageHelper.CreateMessage();
        await SendMessage(reqMsg);
    }

    private async Task SendMessage(object message) {
        var serialized = SerializeMessage(message);
        await _state.DeviceConnector.SendData(serialized);
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

    private async void button1_Click(object sender, EventArgs e) {
        //var reqMsg = LocalClientToDeviceStartOnPrepaidTariffRequestMessageHelper.CreateMessage();
        //reqMsg.Body.TariffId = 140;
        //reqMsg.Body.PasswordHash = GetSha512("123");
        //await SendMessage(reqMsg);
    }

    private void btnEndSession_Click(object sender, EventArgs e) {
        DialogResult dlgResult = MessageBox.Show("Do you want to end the session?", "End session", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
        if (dlgResult != DialogResult.Yes) {
            return;
        }
        SendLocalClientToDeviceEndDeviceSessionByCustomerRequestMessage();
    }

    private async void btnChangePassword_Click(object sender, EventArgs e) {
        ChangePasswordForm form = new();
        DialogResult dlgResult = form.ShowDialog();
        if (dlgResult != DialogResult.OK) {
            return;
        }
        ChangePasswordFormData formData = form.GetFormData();
        var msg = LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageHelper.CreateMessage();
        msg.Body.CurrentPasswordHash = Utils.GetSha512(formData.CurrentPassword);
        msg.Body.NewPasswordHash = Utils.GetSha512(formData.NewPassword);
        await SendMessage(msg);
    }

    private class MainFormState {
        public RestrictedAccessDesktopService DesktopService { get; set; }
        public readonly string RestrictedAccessDesktopName = "RestrictedAccessDesktop";
        public Uri LocalServiceUri { get; set; }
        public WebSocketConnector DeviceConnector { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public DeviceToLocalClientConfigurationNotificationMessage? ConfigurationNotificationMessage { get; set; }
        public DeviceToLocalClientConnectionStatusNotificationMessage? ConnectionStatusNotificationMessage { get; set; }
        public DeviceToLocalClientCurrentStatusNotificationMessage? CurrentStatusNotificationMessage { get; set; }
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
        public DateTimeOffset LastServerPingSentAt { get; set; }
        public RestrictedAccessDesktopForm RestrictedAccessDesktopForm { get; set; }
        public DeviceToLocalClientCreateSignInCodeReplyMessage? CreateSignInCodeReplyMessage { get; set; }
        public DateTimeOffset? CreateSignInCodeReplyMessageReceivedAt { get; set; }
        public DateTimeOffset? StartedToStoppedTransitionDate { get; set; }
        public bool? StartedState { get; set; }
    }
}
