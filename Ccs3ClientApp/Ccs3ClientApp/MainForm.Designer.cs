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
            lblRemainingTime.Location = new Point(12, 9);
            lblRemainingTime.Name = "lblRemainingTime";
            lblRemainingTime.Size = new Size(203, 27);
            lblRemainingTime.TabIndex = 0;
            lblRemainingTime.Text = "Оставащо време";
            // 
            // lblRemainingTimeValue
            // 
            lblRemainingTimeValue.AutoSize = true;
            lblRemainingTimeValue.Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblRemainingTimeValue.Location = new Point(265, 9);
            lblRemainingTimeValue.Name = "lblRemainingTimeValue";
            lblRemainingTimeValue.Size = new Size(25, 27);
            lblRemainingTimeValue.TabIndex = 1;
            lblRemainingTimeValue.Text = "0";
            // 
            // lblStartedAt
            // 
            lblStartedAt.AutoSize = true;
            lblStartedAt.Font = new Font("Arial", 18F);
            lblStartedAt.Location = new Point(12, 63);
            lblStartedAt.Name = "lblStartedAt";
            lblStartedAt.Size = new Size(163, 27);
            lblStartedAt.TabIndex = 2;
            lblStartedAt.Text = "Стартиран на";
            // 
            // lblStartedAtValue
            // 
            lblStartedAtValue.AutoSize = true;
            lblStartedAtValue.Font = new Font("Arial", 18F);
            lblStartedAtValue.Location = new Point(268, 63);
            lblStartedAtValue.Name = "lblStartedAtValue";
            lblStartedAtValue.Size = new Size(20, 27);
            lblStartedAtValue.TabIndex = 3;
            lblStartedAtValue.Text = "-";
            // 
            // lblTotalTime
            // 
            lblTotalTime.AutoSize = true;
            lblTotalTime.Font = new Font("Arial", 18F);
            lblTotalTime.Location = new Point(12, 124);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Size = new Size(219, 27);
            lblTotalTime.TabIndex = 4;
            lblTotalTime.Text = "Използвано време";
            // 
            // lblTotalTimeValue
            // 
            lblTotalTimeValue.AutoSize = true;
            lblTotalTimeValue.Font = new Font("Arial", 18F);
            lblTotalTimeValue.Location = new Point(268, 124);
            lblTotalTimeValue.Name = "lblTotalTimeValue";
            lblTotalTimeValue.Size = new Size(25, 27);
            lblTotalTimeValue.TabIndex = 5;
            lblTotalTimeValue.Text = "0";
            // 
            // lblTotalSum
            // 
            lblTotalSum.AutoSize = true;
            lblTotalSum.Font = new Font("Arial", 18F);
            lblTotalSum.Location = new Point(12, 190);
            lblTotalSum.Name = "lblTotalSum";
            lblTotalSum.Size = new Size(71, 27);
            lblTotalSum.TabIndex = 6;
            lblTotalSum.Text = "Сума";
            // 
            // lblTotalSumValue
            // 
            lblTotalSumValue.AutoSize = true;
            lblTotalSumValue.Font = new Font("Arial", 18F);
            lblTotalSumValue.Location = new Point(268, 190);
            lblTotalSumValue.Name = "lblTotalSumValue";
            lblTotalSumValue.Size = new Size(58, 27);
            lblTotalSumValue.TabIndex = 7;
            lblTotalSumValue.Text = "0.00";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(638, 259);
            Controls.Add(lblTotalSumValue);
            Controls.Add(lblTotalSum);
            Controls.Add(lblTotalTimeValue);
            Controls.Add(lblTotalTime);
            Controls.Add(lblStartedAtValue);
            Controls.Add(lblStartedAt);
            Controls.Add(lblRemainingTimeValue);
            Controls.Add(lblRemainingTime);
            Name = "MainForm";
            Text = "Ccs3 Client App";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
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
    }
}
