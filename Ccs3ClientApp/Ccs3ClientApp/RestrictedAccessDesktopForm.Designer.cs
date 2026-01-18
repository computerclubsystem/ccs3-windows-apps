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
            grpQrCodeSignIn = new GroupBox();
            lblQrCodeRemainingSecondsValue = new Label();
            lblQrCodeRemainingSeconds = new Label();
            picQrCode = new PictureBox();
            chkToggleCustomerCardSignIn = new CheckBox();
            lblSecondsBeforeRestart = new Label();
            btnRestartNow = new Button();
            lblConnectionStatus = new Label();
            grpSessionInfo = new GroupBox();
            lblTotalSumValue = new Label();
            lblTotalSum = new Label();
            lblTotalTimeValue = new Label();
            lblTotalTime = new Label();
            lblStartedAtValue = new Label();
            lblStartedAt = new Label();
            gbCustomerSignIn.SuspendLayout();
            grpQrCodeSignIn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picQrCode).BeginInit();
            grpSessionInfo.SuspendLayout();
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
            gbCustomerSignIn.Size = new Size(442, 175);
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
            // grpQrCodeSignIn
            // 
            grpQrCodeSignIn.BackColor = Color.Black;
            grpQrCodeSignIn.Controls.Add(lblQrCodeRemainingSecondsValue);
            grpQrCodeSignIn.Controls.Add(lblQrCodeRemainingSeconds);
            grpQrCodeSignIn.Controls.Add(picQrCode);
            grpQrCodeSignIn.ForeColor = SystemColors.ButtonFace;
            grpQrCodeSignIn.Location = new Point(464, 82);
            grpQrCodeSignIn.Name = "grpQrCodeSignIn";
            grpQrCodeSignIn.Size = new Size(379, 392);
            grpQrCodeSignIn.TabIndex = 8;
            grpQrCodeSignIn.TabStop = false;
            grpQrCodeSignIn.Text = "QR code sign in";
            grpQrCodeSignIn.Visible = false;
            // 
            // lblQrCodeRemainingSecondsValue
            // 
            lblQrCodeRemainingSecondsValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblQrCodeRemainingSecondsValue.Location = new Point(185, 16);
            lblQrCodeRemainingSecondsValue.Name = "lblQrCodeRemainingSecondsValue";
            lblQrCodeRemainingSecondsValue.Size = new Size(84, 30);
            lblQrCodeRemainingSecondsValue.TabIndex = 9;
            lblQrCodeRemainingSecondsValue.Text = "0";
            // 
            // lblQrCodeRemainingSeconds
            // 
            lblQrCodeRemainingSeconds.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblQrCodeRemainingSeconds.Location = new Point(16, 22);
            lblQrCodeRemainingSeconds.Name = "lblQrCodeRemainingSeconds";
            lblQrCodeRemainingSeconds.Size = new Size(161, 26);
            lblQrCodeRemainingSeconds.TabIndex = 8;
            lblQrCodeRemainingSeconds.Text = "Remaining seconds";
            // 
            // picQrCode
            // 
            picQrCode.Location = new Point(16, 51);
            picQrCode.Name = "picQrCode";
            picQrCode.Size = new Size(320, 320);
            picQrCode.TabIndex = 7;
            picQrCode.TabStop = false;
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
            lblSecondsBeforeRestart.Location = new Point(15, 486);
            lblSecondsBeforeRestart.Name = "lblSecondsBeforeRestart";
            lblSecondsBeforeRestart.Size = new Size(111, 18);
            lblSecondsBeforeRestart.TabIndex = 4;
            lblSecondsBeforeRestart.Text = "Restarting in ...";
            lblSecondsBeforeRestart.Visible = false;
            // 
            // btnRestartNow
            // 
            btnRestartNow.BackColor = Color.Orange;
            btnRestartNow.Location = new Point(278, 480);
            btnRestartNow.Name = "btnRestartNow";
            btnRestartNow.Size = new Size(179, 31);
            btnRestartNow.TabIndex = 5;
            btnRestartNow.Text = "Restart now";
            btnRestartNow.UseVisualStyleBackColor = false;
            btnRestartNow.Visible = false;
            btnRestartNow.Click += btnRestartNow_Click;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.BackColor = Color.Red;
            lblConnectionStatus.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblConnectionStatus.ForeColor = Color.Yellow;
            lblConnectionStatus.Location = new Point(262, 11);
            lblConnectionStatus.Margin = new Padding(4, 0, 4, 0);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(104, 18);
            lblConnectionStatus.TabIndex = 9;
            lblConnectionStatus.Text = "Disconnected";
            // 
            // grpSessionInfo
            // 
            grpSessionInfo.Controls.Add(lblTotalSumValue);
            grpSessionInfo.Controls.Add(lblTotalSum);
            grpSessionInfo.Controls.Add(lblTotalTimeValue);
            grpSessionInfo.Controls.Add(lblTotalTime);
            grpSessionInfo.Controls.Add(lblStartedAtValue);
            grpSessionInfo.Controls.Add(lblStartedAt);
            grpSessionInfo.ForeColor = SystemColors.ButtonFace;
            grpSessionInfo.Location = new Point(15, 290);
            grpSessionInfo.Name = "grpSessionInfo";
            grpSessionInfo.Size = new Size(442, 184);
            grpSessionInfo.TabIndex = 10;
            grpSessionInfo.TabStop = false;
            grpSessionInfo.Text = "Session info";
            grpSessionInfo.Visible = false;
            // 
            // lblTotalSumValue
            // 
            lblTotalSumValue.AutoSize = true;
            lblTotalSumValue.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblTotalSumValue.Location = new Point(205, 136);
            lblTotalSumValue.Margin = new Padding(6, 0, 6, 0);
            lblTotalSumValue.Name = "lblTotalSumValue";
            lblTotalSumValue.Size = new Size(48, 22);
            lblTotalSumValue.TabIndex = 8;
            lblTotalSumValue.Text = "0.00";
            // 
            // lblTotalSum
            // 
            lblTotalSum.AutoSize = true;
            lblTotalSum.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblTotalSum.Location = new Point(9, 136);
            lblTotalSum.Margin = new Padding(6, 0, 6, 0);
            lblTotalSum.Name = "lblTotalSum";
            lblTotalSum.Size = new Size(50, 22);
            lblTotalSum.TabIndex = 7;
            lblTotalSum.Text = "Total";
            // 
            // lblTotalTimeValue
            // 
            lblTotalTimeValue.AutoSize = true;
            lblTotalTimeValue.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblTotalTimeValue.Location = new Point(205, 83);
            lblTotalTimeValue.Margin = new Padding(6, 0, 6, 0);
            lblTotalTimeValue.Name = "lblTotalTimeValue";
            lblTotalTimeValue.Size = new Size(21, 22);
            lblTotalTimeValue.TabIndex = 6;
            lblTotalTimeValue.Text = "0";
            // 
            // lblTotalTime
            // 
            lblTotalTime.AutoSize = true;
            lblTotalTime.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblTotalTime.Location = new Point(9, 83);
            lblTotalTime.Margin = new Padding(6, 0, 6, 0);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Size = new Size(121, 22);
            lblTotalTime.TabIndex = 5;
            lblTotalTime.Text = "Elapsed time";
            // 
            // lblStartedAtValue
            // 
            lblStartedAtValue.AutoSize = true;
            lblStartedAtValue.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblStartedAtValue.Location = new Point(205, 34);
            lblStartedAtValue.Margin = new Padding(6, 0, 6, 0);
            lblStartedAtValue.Name = "lblStartedAtValue";
            lblStartedAtValue.Size = new Size(16, 22);
            lblStartedAtValue.TabIndex = 4;
            lblStartedAtValue.Text = "-";
            // 
            // lblStartedAt
            // 
            lblStartedAt.AutoSize = true;
            lblStartedAt.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblStartedAt.Location = new Point(9, 34);
            lblStartedAt.Margin = new Padding(6, 0, 6, 0);
            lblStartedAt.Name = "lblStartedAt";
            lblStartedAt.Size = new Size(91, 22);
            lblStartedAt.TabIndex = 3;
            lblStartedAt.Text = "Started at";
            // 
            // RestrictedAccessDesktopForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1029, 540);
            ControlBox = false;
            Controls.Add(grpSessionInfo);
            Controls.Add(lblConnectionStatus);
            Controls.Add(grpQrCodeSignIn);
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
            MouseClick += RestrictedAccessDesktopForm_MouseClick;
            gbCustomerSignIn.ResumeLayout(false);
            gbCustomerSignIn.PerformLayout();
            grpQrCodeSignIn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picQrCode).EndInit();
            grpSessionInfo.ResumeLayout(false);
            grpSessionInfo.PerformLayout();
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
        private PictureBox picQrCode;
        private GroupBox grpQrCodeSignIn;
        private Label lblQrCodeRemainingSecondsValue;
        private Label lblQrCodeRemainingSeconds;
        private Label lblConnectionStatus;
        private GroupBox grpSessionInfo;
        private Label lblStartedAt;
        private Label lblStartedAtValue;
        private Label lblTotalTime;
        private Label lblTotalTimeValue;
        private Label lblTotalSum;
        private Label lblTotalSumValue;
    }
}