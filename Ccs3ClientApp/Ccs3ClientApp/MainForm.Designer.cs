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
            testButtonCreateRestrictedAccessDesktop = new Button();
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds = new Button();
            SuspendLayout();
            // 
            // testButtonCreateRestrictedAccessDesktop
            // 
            testButtonCreateRestrictedAccessDesktop.Location = new Point(46, 38);
            testButtonCreateRestrictedAccessDesktop.Name = "testButtonCreateRestrictedAccessDesktop";
            testButtonCreateRestrictedAccessDesktop.Size = new Size(126, 64);
            testButtonCreateRestrictedAccessDesktop.TabIndex = 0;
            testButtonCreateRestrictedAccessDesktop.Text = "Create restricted access desktop";
            testButtonCreateRestrictedAccessDesktop.UseVisualStyleBackColor = true;
            testButtonCreateRestrictedAccessDesktop.Click += testButtonCreateRestrictedAccessDesktop_Click;
            // 
            // testButtonSwitchToRestrictedAccessDesktopFor3Seconds
            // 
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.Location = new Point(241, 38);
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.Name = "testButtonSwitchToRestrictedAccessDesktopFor3Seconds";
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.Size = new Size(166, 64);
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.TabIndex = 1;
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.Text = "Switch to restricted desktop for 3 seconds";
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.UseVisualStyleBackColor = true;
            testButtonSwitchToRestrictedAccessDesktopFor3Seconds.Click += testButtonSwitchToRestrictedAccessDesktopFor3Seconds_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(testButtonSwitchToRestrictedAccessDesktopFor3Seconds);
            Controls.Add(testButtonCreateRestrictedAccessDesktop);
            Name = "MainForm";
            Text = "Form1";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button testButtonCreateRestrictedAccessDesktop;
        private Button testButtonSwitchToRestrictedAccessDesktopFor3Seconds;
    }
}
