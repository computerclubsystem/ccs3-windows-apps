namespace Ccs3ClientApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            notifyIconMain = new NotifyIcon(components);
            lblRemainingTime = new Label();
            lblRemainingTimeValue = new Label();
            lblStartedAt = new Label();
            lblStartedAtValue = new Label();
            lblTotalTime = new Label();
            lblTotalTimeValue = new Label();
            lblTotalSum = new Label();
            lblTotalSumValue = new Label();
            btnEndSession = new Button();
            gbCustomerCardGroup = new GroupBox();
            btnChangePassword = new Button();
            gbCustomerCardGroup.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIconMain
            // 
            notifyIconMain.Text = "Ccs3 Client App";
            notifyIconMain.Visible = true;
            // 
            // lblRemainingTime
            // 
            lblRemainingTime.AutoSize = true;
            lblRemainingTime.Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblRemainingTime.Location = new Point(24, 16);
            lblRemainingTime.Margin = new Padding(6, 0, 6, 0);
            lblRemainingTime.Name = "lblRemainingTime";
            lblRemainingTime.Size = new Size(179, 27);
            lblRemainingTime.TabIndex = 0;
            lblRemainingTime.Text = "Remaining time";
            // 
            // lblRemainingTimeValue
            // 
            lblRemainingTimeValue.AutoSize = true;
            lblRemainingTimeValue.Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblRemainingTimeValue.Location = new Point(270, 16);
            lblRemainingTimeValue.Margin = new Padding(6, 0, 6, 0);
            lblRemainingTimeValue.Name = "lblRemainingTimeValue";
            lblRemainingTimeValue.Size = new Size(25, 27);
            lblRemainingTimeValue.TabIndex = 1;
            lblRemainingTimeValue.Text = "0";
            // 
            // lblStartedAt
            // 
            lblStartedAt.AutoSize = true;
            lblStartedAt.Font = new Font("Arial", 18F);
            lblStartedAt.Location = new Point(24, 63);
            lblStartedAt.Margin = new Padding(6, 0, 6, 0);
            lblStartedAt.Name = "lblStartedAt";
            lblStartedAt.Size = new Size(117, 27);
            lblStartedAt.TabIndex = 2;
            lblStartedAt.Text = "Started at";
            // 
            // lblStartedAtValue
            // 
            lblStartedAtValue.AutoSize = true;
            lblStartedAtValue.Font = new Font("Arial", 18F);
            lblStartedAtValue.Location = new Point(270, 63);
            lblStartedAtValue.Margin = new Padding(6, 0, 6, 0);
            lblStartedAtValue.Name = "lblStartedAtValue";
            lblStartedAtValue.Size = new Size(20, 27);
            lblStartedAtValue.TabIndex = 3;
            lblStartedAtValue.Text = "-";
            // 
            // lblTotalTime
            // 
            lblTotalTime.AutoSize = true;
            lblTotalTime.Font = new Font("Arial", 18F);
            lblTotalTime.Location = new Point(24, 110);
            lblTotalTime.Margin = new Padding(6, 0, 6, 0);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Size = new Size(152, 27);
            lblTotalTime.TabIndex = 4;
            lblTotalTime.Text = "Elapsed time";
            // 
            // lblTotalTimeValue
            // 
            lblTotalTimeValue.AutoSize = true;
            lblTotalTimeValue.Font = new Font("Arial", 18F);
            lblTotalTimeValue.Location = new Point(270, 110);
            lblTotalTimeValue.Margin = new Padding(6, 0, 6, 0);
            lblTotalTimeValue.Name = "lblTotalTimeValue";
            lblTotalTimeValue.Size = new Size(25, 27);
            lblTotalTimeValue.TabIndex = 5;
            lblTotalTimeValue.Text = "0";
            // 
            // lblTotalSum
            // 
            lblTotalSum.AutoSize = true;
            lblTotalSum.Font = new Font("Arial", 18F);
            lblTotalSum.Location = new Point(24, 158);
            lblTotalSum.Margin = new Padding(6, 0, 6, 0);
            lblTotalSum.Name = "lblTotalSum";
            lblTotalSum.Size = new Size(62, 27);
            lblTotalSum.TabIndex = 6;
            lblTotalSum.Text = "Total";
            // 
            // lblTotalSumValue
            // 
            lblTotalSumValue.AutoSize = true;
            lblTotalSumValue.Font = new Font("Arial", 18F);
            lblTotalSumValue.Location = new Point(270, 158);
            lblTotalSumValue.Margin = new Padding(6, 0, 6, 0);
            lblTotalSumValue.Name = "lblTotalSumValue";
            lblTotalSumValue.Size = new Size(58, 27);
            lblTotalSumValue.TabIndex = 7;
            lblTotalSumValue.Text = "0.00";
            // 
            // btnEndSession
            // 
            btnEndSession.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEndSession.Location = new Point(6, 34);
            btnEndSession.Name = "btnEndSession";
            btnEndSession.Size = new Size(175, 48);
            btnEndSession.TabIndex = 8;
            btnEndSession.Text = "End session";
            btnEndSession.UseVisualStyleBackColor = true;
            btnEndSession.Click += btnEndSession_Click;
            // 
            // gbCustomerCardGroup
            // 
            gbCustomerCardGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            gbCustomerCardGroup.Controls.Add(btnChangePassword);
            gbCustomerCardGroup.Controls.Add(btnEndSession);
            gbCustomerCardGroup.Location = new Point(12, 231);
            gbCustomerCardGroup.Name = "gbCustomerCardGroup";
            gbCustomerCardGroup.Size = new Size(440, 89);
            gbCustomerCardGroup.TabIndex = 9;
            gbCustomerCardGroup.TabStop = false;
            gbCustomerCardGroup.Text = "Customer card";
            gbCustomerCardGroup.Visible = false;
            // 
            // btnChangePassword
            // 
            btnChangePassword.Location = new Point(187, 34);
            btnChangePassword.Name = "btnChangePassword";
            btnChangePassword.Size = new Size(247, 48);
            btnChangePassword.TabIndex = 9;
            btnChangePassword.Text = "Change password";
            btnChangePassword.UseVisualStyleBackColor = true;
            btnChangePassword.Click += btnChangePassword_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(14F, 27F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(621, 332);
            Controls.Add(gbCustomerCardGroup);
            Controls.Add(lblTotalSumValue);
            Controls.Add(lblTotalSum);
            Controls.Add(lblTotalTimeValue);
            Controls.Add(lblTotalTime);
            Controls.Add(lblStartedAtValue);
            Controls.Add(lblStartedAt);
            Controls.Add(lblRemainingTimeValue);
            Controls.Add(lblRemainingTime);
            Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Margin = new Padding(6, 5, 6, 5);
            Name = "MainForm";
            Text = "Ccs3 Client App";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Click += MainForm_Click;
            gbCustomerCardGroup.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon notifyIconMain;
        private Label lblRemainingTime;
        private Label lblRemainingTimeValue;
        private Label lblStartedAt;
        private Label lblStartedAtValue;
        private Label lblTotalTime;
        private Label lblTotalTimeValue;
        private Label lblTotalSum;
        private Label lblTotalSumValue;
        private Button btnEndSession;
        private GroupBox gbCustomerCardGroup;
        private Button btnChangePassword;
    }
}
