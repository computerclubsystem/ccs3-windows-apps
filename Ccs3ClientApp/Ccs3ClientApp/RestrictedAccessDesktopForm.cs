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

        private bool _canClose = false;

        public RestrictedAccessDesktopForm() {
            InitializeComponent();
        }

        public void SetCanClose(bool canClose) {
            _canClose = canClose;
        }

        public void SetCustomerSignInResult(bool passwordDoesNotMatch, bool notAllowed, bool alreadyInUse) {
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
            CustomerSignIn?.Invoke(this, new CustomerSignInEventArgs { CustomerCardID = cardId, PasswordHash = GetSha512(password) });
        }

        private string GetSha512(string value) {
            using var sha512 = SHA512.Create();
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes = sha512.ComputeHash(valueBytes);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            return hashString;
        }
    }

    public class CustomerSignInEventArgs : EventArgs {
        public int CustomerCardID { get; set; }
        public string PasswordHash { get; set; }
    }
}
