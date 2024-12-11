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
            _state.DesktopService = new RestrictedAccessDesktopService();
        }

        private class MainFormState {
            public RestrictedAccessDesktopService DesktopService { get; set; }
            public readonly string RestrictedAccessDesktopName = "RestrictedAccessDesktop";
        }
    }
}
