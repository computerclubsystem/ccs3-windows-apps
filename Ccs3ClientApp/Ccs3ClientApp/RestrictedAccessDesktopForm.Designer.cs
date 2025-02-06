namespace Ccs3ClientApp {
    partial class RestrictedAccessDesktopForm {
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
        private void InitializeComponent() {
            lblVersion = new Label();
            lblCustomerCardId = new Label();
            gbCustomerSignIn = new GroupBox();
            lblCustomerSignInErrorMessage = new Label();
            btnCustomerSignIn = new Button();
            txtCustomerCardPassword = new TextBox();
            lblCustomerCardPassword = new Label();
            txtCustomerCardID = new TextBox();
            chkToggleCustomerCardSignIn = new CheckBox();
            lblSecondsBeforeRestart = new Label();
            btnRestartNow = new Button();
            gbCustomerSignIn.SuspendLayout();
            SuspendLayout();
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblVersion.ForeColor = SystemColors.ButtonFace;
            lblVersion.Location = new Point(15, 11);
            lblVersion.Margin = new Padding(4, 0, 4, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(61, 18);
            lblVersion.TabIndex = 0;
            lblVersion.Text = "Version";
            // 
            // lblCustomerCardId
            // 
            lblCustomerCardId.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblCustomerCardId.ForeColor = SystemColors.ButtonFace;
            lblCustomerCardId.Location = new Point(8, 37);
            lblCustomerCardId.Margin = new Padding(4, 0, 4, 0);
            lblCustomerCardId.Name = "lblCustomerCardId";
            lblCustomerCardId.Size = new Size(146, 22);
            lblCustomerCardId.TabIndex = 1;
            lblCustomerCardId.Text = "Customer card ID";
            // 
            // gbCustomerSignIn
            // 
            gbCustomerSignIn.Controls.Add(lblCustomerSignInErrorMessage);
            gbCustomerSignIn.Controls.Add(btnCustomerSignIn);
            gbCustomerSignIn.Controls.Add(txtCustomerCardPassword);
            gbCustomerSignIn.Controls.Add(lblCustomerCardPassword);
            gbCustomerSignIn.Controls.Add(txtCustomerCardID);
            gbCustomerSignIn.Controls.Add(lblCustomerCardId);
            gbCustomerSignIn.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            gbCustomerSignIn.ForeColor = SystemColors.ButtonFace;
            gbCustomerSignIn.Location = new Point(15, 82);
            gbCustomerSignIn.Margin = new Padding(4);
            gbCustomerSignIn.Name = "gbCustomerSignIn";
            gbCustomerSignIn.Padding = new Padding(4);
            gbCustomerSignIn.Size = new Size(439, 174);
            gbCustomerSignIn.TabIndex = 2;
            gbCustomerSignIn.TabStop = false;
            gbCustomerSignIn.Text = "Sign in";
            gbCustomerSignIn.Visible = false;
            // 
            // lblCustomerSignInErrorMessage
            // 
            lblCustomerSignInErrorMessage.BackColor = Color.Crimson;
            lblCustomerSignInErrorMessage.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblCustomerSignInErrorMessage.ForeColor = Color.White;
            lblCustomerSignInErrorMessage.Location = new Point(162, 122);
            lblCustomerSignInErrorMessage.Margin = new Padding(4, 0, 4, 0);
            lblCustomerSignInErrorMessage.Name = "lblCustomerSignInErrorMessage";
            lblCustomerSignInErrorMessage.Size = new Size(271, 43);
            lblCustomerSignInErrorMessage.TabIndex = 6;
            lblCustomerSignInErrorMessage.TextAlign = ContentAlignment.MiddleLeft;
            lblCustomerSignInErrorMessage.Visible = false;
            // 
            // btnCustomerSignIn
            // 
            btnCustomerSignIn.BackColor = SystemColors.ButtonFace;
            btnCustomerSignIn.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnCustomerSignIn.ForeColor = SystemColors.ControlText;
            btnCustomerSignIn.Location = new Point(8, 122);
            btnCustomerSignIn.Margin = new Padding(4);
            btnCustomerSignIn.Name = "btnCustomerSignIn";
            btnCustomerSignIn.Size = new Size(146, 43);
            btnCustomerSignIn.TabIndex = 5;
            btnCustomerSignIn.Text = "Sign in";
            btnCustomerSignIn.UseVisualStyleBackColor = false;
            btnCustomerSignIn.Click += btnCustomerSignIn_Click;
            // 
            // txtCustomerCardPassword
            // 
            txtCustomerCardPassword.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            txtCustomerCardPassword.Location = new Point(162, 77);
            txtCustomerCardPassword.Margin = new Padding(4);
            txtCustomerCardPassword.Name = "txtCustomerCardPassword";
            txtCustomerCardPassword.Size = new Size(270, 26);
            txtCustomerCardPassword.TabIndex = 4;
            txtCustomerCardPassword.UseSystemPasswordChar = true;
            txtCustomerCardPassword.KeyUp += txtCustomerCardPassword_KeyUp;
            // 
            // lblCustomerCardPassword
            // 
            lblCustomerCardPassword.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblCustomerCardPassword.Location = new Point(8, 80);
            lblCustomerCardPassword.Margin = new Padding(4, 0, 4, 0);
            lblCustomerCardPassword.Name = "lblCustomerCardPassword";
            lblCustomerCardPassword.Size = new Size(146, 25);
            lblCustomerCardPassword.TabIndex = 3;
            lblCustomerCardPassword.Text = "Password";
            // 
            // txtCustomerCardID
            // 
            txtCustomerCardID.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            txtCustomerCardID.Location = new Point(162, 34);
            txtCustomerCardID.Margin = new Padding(4);
            txtCustomerCardID.Name = "txtCustomerCardID";
            txtCustomerCardID.Size = new Size(81, 26);
            txtCustomerCardID.TabIndex = 2;
            // 
            // chkToggleCustomerCardSignIn
            // 
            chkToggleCustomerCardSignIn.Appearance = Appearance.Button;
            chkToggleCustomerCardSignIn.Location = new Point(15, 45);
            chkToggleCustomerCardSignIn.Margin = new Padding(4);
            chkToggleCustomerCardSignIn.Name = "chkToggleCustomerCardSignIn";
            chkToggleCustomerCardSignIn.Size = new Size(175, 29);
            chkToggleCustomerCardSignIn.TabIndex = 3;
            chkToggleCustomerCardSignIn.Text = "Customer card sign in";
            chkToggleCustomerCardSignIn.UseVisualStyleBackColor = true;
            chkToggleCustomerCardSignIn.CheckedChanged += chkToggleCustomerCardSignIn_CheckedChanged;
            // 
            // lblSecondsBeforeRestart
            // 
            lblSecondsBeforeRestart.AutoSize = true;
            lblSecondsBeforeRestart.ForeColor = Color.Orange;
            lblSecondsBeforeRestart.Location = new Point(15, 260);
            lblSecondsBeforeRestart.Name = "lblSecondsBeforeRestart";
            lblSecondsBeforeRestart.Size = new Size(111, 18);
            lblSecondsBeforeRestart.TabIndex = 4;
            lblSecondsBeforeRestart.Text = "Restarting in ...";
            lblSecondsBeforeRestart.Visible = false;
            // 
            // btnRestartNow
            // 
            btnRestartNow.BackColor = Color.Orange;
            btnRestartNow.Location = new Point(15, 285);
            btnRestartNow.Name = "btnRestartNow";
            btnRestartNow.Size = new Size(179, 31);
            btnRestartNow.TabIndex = 5;
            btnRestartNow.Text = "Restart now";
            btnRestartNow.UseVisualStyleBackColor = false;
            btnRestartNow.Visible = false;
            btnRestartNow.Click += btnRestartNow_Click;
            // 
            // RestrictedAccessDesktopForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1029, 540);
            ControlBox = false;
            Controls.Add(btnRestartNow);
            Controls.Add(lblSecondsBeforeRestart);
            Controls.Add(chkToggleCustomerCardSignIn);
            Controls.Add(gbCustomerSignIn);
            Controls.Add(lblVersion);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RestrictedAccessDesktopForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Ccs3 Client App - idle info";
            WindowState = FormWindowState.Maximized;
            FormClosing += RestrictedAccessDesktopForm_FormClosing;
            Load += RestrictedAccessDesktopForm_Load;
            gbCustomerSignIn.ResumeLayout(false);
            gbCustomerSignIn.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblVersion;
        private Label lblCustomerCardId;
        private GroupBox gbCustomerSignIn;
        private TextBox txtCustomerCardPassword;
        private Label lblCustomerCardPassword;
        private TextBox txtCustomerCardID;
        private Button btnCustomerSignIn;
        private Label lblCustomerSignInErrorMessage;
        private CheckBox chkToggleCustomerCardSignIn;
        private Label lblSecondsBeforeRestart;
        private Button btnRestartNow;
    }
}