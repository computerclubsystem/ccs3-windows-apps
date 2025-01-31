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
            gbCustomerSignIn.SuspendLayout();
            SuspendLayout();
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblVersion.ForeColor = SystemColors.ButtonFace;
            lblVersion.Location = new Point(12, 9);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(61, 18);
            lblVersion.TabIndex = 0;
            lblVersion.Text = "Version";
            // 
            // lblCustomerCardId
            // 
            lblCustomerCardId.ForeColor = SystemColors.ButtonFace;
            lblCustomerCardId.Location = new Point(6, 31);
            lblCustomerCardId.Name = "lblCustomerCardId";
            lblCustomerCardId.Size = new Size(100, 18);
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
            gbCustomerSignIn.ForeColor = SystemColors.ButtonFace;
            gbCustomerSignIn.Location = new Point(12, 68);
            gbCustomerSignIn.Name = "gbCustomerSignIn";
            gbCustomerSignIn.Size = new Size(309, 156);
            gbCustomerSignIn.TabIndex = 2;
            gbCustomerSignIn.TabStop = false;
            gbCustomerSignIn.Text = "Customer card";
            // 
            // lblCustomerSignInErrorMessage
            // 
            lblCustomerSignInErrorMessage.BackColor = Color.LightCoral;
            lblCustomerSignInErrorMessage.ForeColor = SystemColors.ControlText;
            lblCustomerSignInErrorMessage.Location = new Point(6, 108);
            lblCustomerSignInErrorMessage.Name = "lblCustomerSignInErrorMessage";
            lblCustomerSignInErrorMessage.Size = new Size(188, 36);
            lblCustomerSignInErrorMessage.TabIndex = 6;
            lblCustomerSignInErrorMessage.Visible = false;
            // 
            // btnCustomerSignIn
            // 
            btnCustomerSignIn.BackColor = SystemColors.ButtonFace;
            btnCustomerSignIn.ForeColor = SystemColors.ControlText;
            btnCustomerSignIn.Location = new Point(200, 108);
            btnCustomerSignIn.Name = "btnCustomerSignIn";
            btnCustomerSignIn.Size = new Size(103, 36);
            btnCustomerSignIn.TabIndex = 5;
            btnCustomerSignIn.Text = "Sign in";
            btnCustomerSignIn.UseVisualStyleBackColor = false;
            btnCustomerSignIn.Click += btnCustomerSignIn_Click;
            // 
            // txtCustomerCardPassword
            // 
            txtCustomerCardPassword.Location = new Point(112, 64);
            txtCustomerCardPassword.Name = "txtCustomerCardPassword";
            txtCustomerCardPassword.Size = new Size(191, 23);
            txtCustomerCardPassword.TabIndex = 4;
            txtCustomerCardPassword.UseSystemPasswordChar = true;
            // 
            // lblCustomerCardPassword
            // 
            lblCustomerCardPassword.Location = new Point(6, 67);
            lblCustomerCardPassword.Name = "lblCustomerCardPassword";
            lblCustomerCardPassword.Size = new Size(100, 21);
            lblCustomerCardPassword.TabIndex = 3;
            lblCustomerCardPassword.Text = "Password";
            // 
            // txtCustomerCardID
            // 
            txtCustomerCardID.Location = new Point(112, 26);
            txtCustomerCardID.Name = "txtCustomerCardID";
            txtCustomerCardID.Size = new Size(64, 23);
            txtCustomerCardID.TabIndex = 2;
            // 
            // RestrictedAccessDesktopForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Controls.Add(gbCustomerSignIn);
            Controls.Add(lblVersion);
            FormBorderStyle = FormBorderStyle.None;
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
    }
}