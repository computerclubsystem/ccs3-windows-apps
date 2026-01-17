using Ccs3ClientApp.Messages;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ccs3ClientApp {
    public partial class RestrictedAccessDesktopForm : Form {
        public event EventHandler<CustomerSignInEventArgs> CustomerSignIn;
        public event EventHandler<EventArgs> RestartNow;

        private bool _canClose = false;
        private bool _qrCodeSignInEnabled = false;
        private int _qrCodeRemainingSeconds = 0;

        public RestrictedAccessDesktopForm() {
            InitializeComponent();
        }

        public void SetCanClose(bool canClose) {
            _canClose = canClose;
        }

        public void SetQrCodeVisibility(bool visible) {
            SafeChangeUI(() => {
                _qrCodeSignInEnabled = visible;
                grpQrCodeSignIn.Visible = gbCustomerSignIn.Visible && _qrCodeSignInEnabled && _qrCodeRemainingSeconds > 0;
            });
        }

        public void SetQrCodeUrl(string? url) {
            if (url is null) {
                return;
            }
            SafeChangeUI(() => {
                SetQrCodeUrlValue(url);
            });
        }

        public void SetQrCodeRemainingSeconds(int remainingSeconds) {
            SafeChangeUI(() => {
                _qrCodeRemainingSeconds = remainingSeconds;
                lblQrCodeRemainingSecondsValue.Text = remainingSeconds.ToString();
                // TODO: This must be revisited
                grpQrCodeSignIn.Visible = gbCustomerSignIn.Visible && _qrCodeSignInEnabled && _qrCodeRemainingSeconds > 0;
            });
        }

        private void SetQrCodeUrlValue(string qrCodeUrl) {
            var payloadGen = new PayloadGenerator.Url(qrCodeUrl);
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payloadGen))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData)) {
                byte[] qrCodeImage = qrCode.GetGraphic(5);
                Bitmap bmp;
                using (var ms = new MemoryStream(qrCodeImage)) {
                    bmp = new Bitmap(ms);
                    picQrCode.Image = bmp;
                }
            }
        }

        public void SetSessionInfo(SessionTextInfo? sessionTextInfo) {
            SafeChangeUI(() => {
                if (sessionTextInfo is null) {
                    grpSessionInfo.Visible = false;
                } else {
                    lblStartedAtValue.Text = sessionTextInfo.StartedAt;
                    lblTotalTimeValue.Text = sessionTextInfo.ElapsedTime;
                    lblTotalSumValue.Text = sessionTextInfo.TotalSum;
                    grpSessionInfo.Visible = true;
                }
            });
        }

        public void SetSecondsBeforeRestart(int seconds) {
            SafeChangeUI(() => {
                if (seconds <= 0) {
                    lblSecondsBeforeRestart.Visible = false;
                    btnRestartNow.Visible = false;
                    return;
                }
                lblSecondsBeforeRestart.Visible = true;
                lblSecondsBeforeRestart.Text = "Restaring in " + seconds.ToString() + " seconds";
                btnRestartNow.Visible = true;
            });
        }

        public void ChangeStartedState(bool isStarted) {
            SafeChangeUI(() => {
                lblCustomerSignInErrorMessage.Visible = false;
                txtCustomerCardID.Text = "";
                txtCustomerCardPassword.Text = "";
            });
        }

        public void SetConnectionStatus(bool connected) {
            SafeChangeUI(() => {
                lblConnectionStatus.Visible = !connected;
            });
        }

        public void SetCustomerSignInResult(DeviceToLocalClientStartOnPrepaidTariffReplyMessage message) {
            SafeChangeUI(() => SetSignInResult(message));
        }

        private void SetSignInResult(DeviceToLocalClientStartOnPrepaidTariffReplyMessage msg) {
            bool success = false;
            if (msg.Header.Failure.HasValue && msg.Header.Failure.Value == true) {
                success = false;
            } else {
                success = msg.Body.Success ?? false;
            }
            var body = msg.Body;
            lblCustomerSignInErrorMessage.Visible = false;
            bool passwordDoesNotMatch = body.PasswordDoesNotMatch ?? false;
            bool notAllowed = body.NotAllowed ?? false;
            bool alreadyInUse = body.AlreadyInUse ?? false;
            bool notAvailableForThisDeviceGroup = body.NotAvailableForThisDeviceGroup ?? false;
            bool noRemainingTime = body.NoRemainingTime ?? false;
            if (passwordDoesNotMatch) {
                lblCustomerSignInErrorMessage.Text = "Password does not match";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (notAllowed) {
                lblCustomerSignInErrorMessage.Text = "Not allowed";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (alreadyInUse) {
                lblCustomerSignInErrorMessage.Text = "The card is already in use on another computer";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (notAvailableForThisDeviceGroup) {
                lblCustomerSignInErrorMessage.Text = "The card is not available for this device group";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (noRemainingTime) {
                lblCustomerSignInErrorMessage.Text = "The card has no remaining time";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (!success) {
                lblCustomerSignInErrorMessage.Text = "Generic error";
                lblCustomerSignInErrorMessage.Visible = true;
            }
        }

        private void RestrictedAccessDesktopForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (!_canClose) {
                e.Cancel = true;
            }
        }

        private void RestrictedAccessDesktopForm_Load(object sender, EventArgs e) {
            lblVersion.Text = "Ccs3 Client App " + typeof(MainForm).Assembly.GetName().Version.ToString();
        }

        private void btnCustomerSignIn_Click(object sender, EventArgs e) {
            OnSignIn();
        }

        private void OnSignIn() {
            lblCustomerSignInErrorMessage.Visible = false;
            bool customerCardParsed = int.TryParse(txtCustomerCardID.Text, out var cardId);
            if (!customerCardParsed) {
                return;
            }
            string password = txtCustomerCardPassword.Text;
            if (string.IsNullOrWhiteSpace(password)) {
                return;
            }
            txtCustomerCardPassword.Text = string.Empty;
            CustomerSignIn?.Invoke(this, new CustomerSignInEventArgs { CustomerCardID = cardId, PasswordHash = Utils.GetSha512(password) });
        }

        private void txtCustomerCardPassword_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                OnSignIn();
            }
        }

        private void chkToggleCustomerCardSignIn_CheckedChanged(object sender, EventArgs e) {
            ChangeSignInControlsVisibility(chkToggleCustomerCardSignIn.Checked);
        }

        private void ChangeSignInControlsVisibility(bool visible) {
            gbCustomerSignIn.Visible = visible;
            grpQrCodeSignIn.Visible = gbCustomerSignIn.Visible && _qrCodeSignInEnabled && _qrCodeRemainingSeconds > 0;
        }

        private void SafeChangeUI(Action action) {
            if (InvokeRequired) {
                Invoke(action);
            } else {
                action();
            }
        }

        private void btnRestartNow_Click(object sender, EventArgs e) {
            RestartNow?.Invoke(this, EventArgs.Empty);
        }

        private void picQrCode_Click(object sender, EventArgs e) {

        }

        private void RestrictedAccessDesktopForm_MouseClick(object sender, MouseEventArgs e) {
#if DEBUG
            //grpQrCodeSignIn.Visible = true;
            //SetQrCodeUrlValue("https://192.168.1.3:65503/?sign-in-code=12345678-1234-123456-12345678-1234&identifierType=customer-card");
#endif
        }
    }

    public class CustomerSignInEventArgs : EventArgs {
        public int CustomerCardID { get; set; }
        public string PasswordHash { get; set; }
    }

    public class SessionTextInfo {
        public string StartedAt { get; set; }
        public string ElapsedTime { get; set; }
        public string TotalSum { get; set; }
    }
}
