﻿namespace Ccs3ClientApp {
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
            SuspendLayout();
            // 
            // RestrictedAccessDesktopForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RestrictedAccessDesktopForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Ccs3 Client App - idle info";
            WindowState = FormWindowState.Maximized;
            FormClosing += RestrictedAccessDesktopForm_FormClosing;
            ResumeLayout(false);
        }

        #endregion
    }
}