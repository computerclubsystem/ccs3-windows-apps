namespace Ccs3ClientApp {
    public partial class MainForm : Form {
        private MainFormState _state;

        public MainForm() {
            InitializeComponent();
            _state = new MainFormState();
        }

        private void testButtonCreateRestrictedAccessDesktop_Click(object sender, EventArgs e) {
            //if (_state.DesktopService.HasDesktop(_state.RestrictedAccessDesktopName)) {
            //    MessageBox.Show(string.Format("Restricted desktop already exists: {0}", _state.RestrictedAccessDesktopPointer));
            //    return;
            //} else {
            //}
            _state.DesktopService.CloseDesktop(_state.DesktopService.GetDefaultDesktopPointer());
            IntPtr restrictedAccessDesktopPointer = _state.DesktopService.CreateRestrictedAccessDesktop(_state.RestrictedAccessDesktopName);
            if (restrictedAccessDesktopPointer != IntPtr.Zero) {
                // TODO: Use cancellation token
                Task.Factory.StartNew(() => {
                    _state.DesktopService.SetCurrentThreadDesktop(restrictedAccessDesktopPointer);
                    RestrictedAccessDesktopForm radForm = new();
                    Application.Run(radForm);
                });
                MessageBox.Show(string.Format("Restricted desktop has been created: {0}", _state.DesktopService.GetRestrictedAccessDesktopPointer()));
            } else {
                var lastError = _state.DesktopService.GetLastError();
                MessageBox.Show(string.Format("Cannot create restricted access desktop. Error {0}", lastError));
            }
        }

        private void testButtonSwitchToRestrictedAccessDesktopFor3Seconds_Click(object sender, EventArgs e) {
            _state.DesktopService.SwitchToRestrictedAccessDesktop();
            Task.Run(() => {
                _state.DesktopService.SetCurrentThreadDesktop(_state.DesktopService.GetRestrictedAccessDesktopPointer());
                Thread.Sleep(TimeSpan.FromSeconds(3));
                _state.DesktopService.SwitchToDefaultDesktop();
            });
        }

        private void MainForm_Load(object sender, EventArgs e) {
            Initialize();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            IntPtr radPointer = _state.DesktopService.OpenDesktop(_state.RestrictedAccessDesktopName);
            if (radPointer != IntPtr.Zero) {
                _state.DesktopService.CloseDesktopPointer(radPointer);
            }
        }

        private void Initialize() {
            _state = new MainFormState();
            _state.CancellationToken = new CancellationToken();
            _state.DesktopService = new RestrictedAccessDesktopService();
            string? localServiceUriString = Environment.GetEnvironmentVariable("CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL") ?? "https://localhost:30000";
            localServiceUriString = "wss://127.0.0.1:30000";
            bool localServiceUriParsed = Uri.TryCreate(localServiceUriString, UriKind.Absolute, out Uri? localServiceUri);
            if (localServiceUri is not null) {
                if (localServiceUri.Scheme != Uri.UriSchemeWss) {
                    // The scheme is not wss - use wss
                    UriBuilder ub = new(localServiceUri);
                    ub.Scheme = Uri.UriSchemeWss;
                    localServiceUri = ub.Uri;
                }
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
        }

        private void LocalServiceConnector_SendDataError(object? sender, SendDataErrorEventArgs e) {
        }

        private void LocalServiceConnector_ReceiveError(object? sender, ReceiveErrorEventArgs e) {
        }

        private void LocalServiceConnector_DataReceived(object? sender, DataReceivedEventArgs e) {
        }

        private void LocalServiceConnector_ValidatingRemoteCertificate(object? sender, ValidatingRemoteCertificateArgs e) {
        }

        private void LocalServiceConnector_Disconnected(object? sender, DisconnectedEventArgs e) {
        }

        private void LocalServiceConnector_ConnectError(object? sender, ConnectErrorEventArgs e) {
        }

        private void LocalServiceConnector_Connected(object? sender, ConnectedEventArgs e) {
            
        }

        private class MainFormState {
            public RestrictedAccessDesktopService DesktopService { get; set; }
            public readonly string RestrictedAccessDesktopName = "RestrictedAccessDesktop";
            public Uri LocalServiceUri { get; set; }
            public WebSocketConnector LocalServiceConnector { get; set; }
            public CancellationToken CancellationToken { get; set; }
        }
    }
}
