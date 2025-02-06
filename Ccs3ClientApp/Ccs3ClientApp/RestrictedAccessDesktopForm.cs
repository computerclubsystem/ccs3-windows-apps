using Ccs3ClientApp.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ccs3ClientApp {
    public partial class RestrictedAccessDesktopForm : Form {
        public event EventHandler<CustomerSignInEventArgs> CustomerSignIn;
        public event EventHandler<EventArgs> RestartNow;

        private bool _canClose = false;

        public RestrictedAccessDesktopForm() {
            InitializeComponent();
        }

        public void SetCanClose(bool canClose) {
            _canClose = canClose;
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

        public void SetCustomerSignInResult(bool passwordDoesNotMatch, bool notAllowed, bool alreadyInUse, bool success) {
            SafeChangeUI(() => SetSignInResult(passwordDoesNotMatch, notAllowed, alreadyInUse, success));
        }

        private void SetSignInResult(bool passwordDoesNotMatch, bool notAllowed, bool alreadyInUse, bool success) {
            lblCustomerSignInErrorMessage.Visible = false;
            if (passwordDoesNotMatch) {
                lblCustomerSignInErrorMessage.Text = "Password does not match";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (notAllowed) {
                lblCustomerSignInErrorMessage.Text = "Not allowed";
                lblCustomerSignInErrorMessage.Visible = true;
            } else if (alreadyInUse) {
                lblCustomerSignInErrorMessage.Text = "The card is already in use on another computer";
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
            gbCustomerSignIn.Visible = chkToggleCustomerCardSignIn.Checked;
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
    }

    public class CustomerSignInEventArgs : EventArgs {
        public int CustomerCardID { get; set; }
        public string PasswordHash { get; set; }
    }
}
