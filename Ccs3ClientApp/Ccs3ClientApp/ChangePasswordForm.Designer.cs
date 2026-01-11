namespace Ccs3ClientApp;

partial class ChangePasswordForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        lblCurrentPassword = new Label();
        lblNewPassword = new Label();
        lblConfirmNewPassword = new Label();
        txtCurrentPassword = new TextBox();
        txtNewPassword = new TextBox();
        txtConfirmNewPassword = new TextBox();
        btnChangePassword = new Button();
        lblMinPasswordLength = new Label();
        SuspendLayout();
        // 
        // lblCurrentPassword
        // 
        lblCurrentPassword.Location = new Point(12, 12);
        lblCurrentPassword.Name = "lblCurrentPassword";
        lblCurrentPassword.Size = new Size(228, 31);
        lblCurrentPassword.TabIndex = 0;
        lblCurrentPassword.Text = "Current password";
        // 
        // lblNewPassword
        // 
        lblNewPassword.Location = new Point(12, 53);
        lblNewPassword.Name = "lblNewPassword";
        lblNewPassword.Size = new Size(193, 32);
        lblNewPassword.TabIndex = 1;
        lblNewPassword.Text = "New password";
        // 
        // lblConfirmNewPassword
        // 
        lblConfirmNewPassword.Location = new Point(12, 94);
        lblConfirmNewPassword.Name = "lblConfirmNewPassword";
        lblConfirmNewPassword.Size = new Size(273, 32);
        lblConfirmNewPassword.TabIndex = 2;
        lblConfirmNewPassword.Text = "Confirm new password";
        // 
        // txtCurrentPassword
        // 
        txtCurrentPassword.Location = new Point(327, 9);
        txtCurrentPassword.Name = "txtCurrentPassword";
        txtCurrentPassword.Size = new Size(230, 35);
        txtCurrentPassword.TabIndex = 3;
        txtCurrentPassword.UseSystemPasswordChar = true;
        txtCurrentPassword.TextChanged += txtCurrentPassword_TextChanged;
        // 
        // txtNewPassword
        // 
        txtNewPassword.Location = new Point(327, 50);
        txtNewPassword.Name = "txtNewPassword";
        txtNewPassword.Size = new Size(230, 35);
        txtNewPassword.TabIndex = 4;
        txtNewPassword.UseSystemPasswordChar = true;
        txtNewPassword.TextChanged += txtNewPassword_TextChanged;
        // 
        // txtConfirmNewPassword
        // 
        txtConfirmNewPassword.Location = new Point(327, 91);
        txtConfirmNewPassword.Name = "txtConfirmNewPassword";
        txtConfirmNewPassword.Size = new Size(230, 35);
        txtConfirmNewPassword.TabIndex = 5;
        txtConfirmNewPassword.UseSystemPasswordChar = true;
        txtConfirmNewPassword.TextChanged += txtConfirmNewPassword_TextChanged;
        // 
        // btnChangePassword
        // 
        btnChangePassword.Enabled = false;
        btnChangePassword.Location = new Point(327, 164);
        btnChangePassword.Name = "btnChangePassword";
        btnChangePassword.Size = new Size(230, 38);
        btnChangePassword.TabIndex = 6;
        btnChangePassword.Text = "Change password";
        btnChangePassword.UseVisualStyleBackColor = true;
        btnChangePassword.Click += btnChangePassword_Click;
        // 
        // lblMinPasswordLength
        // 
        lblMinPasswordLength.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
        lblMinPasswordLength.Location = new Point(12, 134);
        lblMinPasswordLength.Name = "lblMinPasswordLength";
        lblMinPasswordLength.Size = new Size(545, 19);
        lblMinPasswordLength.TabIndex = 7;
        lblMinPasswordLength.Text = "The new password length must be at least 10 characters";
        lblMinPasswordLength.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ChangePasswordForm
        // 
        AutoScaleDimensions = new SizeF(14F, 27F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(578, 214);
        Controls.Add(lblMinPasswordLength);
        Controls.Add(btnChangePassword);
        Controls.Add(txtConfirmNewPassword);
        Controls.Add(txtNewPassword);
        Controls.Add(txtCurrentPassword);
        Controls.Add(lblConfirmNewPassword);
        Controls.Add(lblNewPassword);
        Controls.Add(lblCurrentPassword);
        Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
        Margin = new Padding(6, 5, 6, 5);
        Name = "ChangePasswordForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Change password";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblCurrentPassword;
    private Label lblNewPassword;
    private Label lblConfirmNewPassword;
    private TextBox txtCurrentPassword;
    private TextBox txtNewPassword;
    private TextBox txtConfirmNewPassword;
    private Button btnChangePassword;
    private Label lblMinPasswordLength;
}