using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ccs3ClientApp;
public partial class ChangePasswordForm : Form {
    private ChangePasswordFormData formData = new();

    public ChangePasswordForm() {
        InitializeComponent();
    }

    public ChangePasswordFormData GetFormData() {
        return formData;
    }

    private void btnChangePassword_Click(object sender, EventArgs e) {
        formData.CurrentPassword = txtCurrentPassword.Text;
        formData.NewPassword = txtNewPassword.Text;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void txtCurrentPassword_TextChanged(object sender, EventArgs e) {
        RefreshFormValidation();
    }

    private void txtNewPassword_TextChanged(object sender, EventArgs e) {
        RefreshFormValidation();
    }

    private void txtConfirmNewPassword_TextChanged(object sender, EventArgs e) {
        RefreshFormValidation();
    }

    private void RefreshFormValidation() {
        bool canTryToChangePassword = true;
        string currentPassword = txtCurrentPassword.Text;
        if (string.IsNullOrWhiteSpace(currentPassword)) {
            txtCurrentPassword.BackColor = Color.Crimson;
            canTryToChangePassword = false;
        } else {
            txtCurrentPassword.BackColor = SystemColors.Window;
        }
        string newPassword = txtNewPassword.Text;
        string confirmNewPassword = txtConfirmNewPassword.Text;
        if (string.IsNullOrWhiteSpace(newPassword)
            || string.IsNullOrWhiteSpace(confirmNewPassword)
            || newPassword != confirmNewPassword) {
            canTryToChangePassword = false;
            txtNewPassword.BackColor = Color.Crimson;
            txtConfirmNewPassword.BackColor = Color.Crimson;
        } else {
            txtNewPassword.BackColor = SystemColors.Window;
            txtConfirmNewPassword.BackColor = SystemColors.Window;
        }
        btnChangePassword.Enabled = canTryToChangePassword;
    }
}

public class ChangePasswordFormData {
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
