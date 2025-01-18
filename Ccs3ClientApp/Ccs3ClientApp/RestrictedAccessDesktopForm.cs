using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ccs3ClientApp {
    public partial class RestrictedAccessDesktopForm : Form {
        private bool _canClose = false;

        public RestrictedAccessDesktopForm() {
            InitializeComponent();
        }

        public void SetCanClose(bool canClose) {
            _canClose = canClose;
        }

        private void RestrictedAccessDesktopForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (!_canClose) {
                e.Cancel = true;
            }
        }
    }
}
