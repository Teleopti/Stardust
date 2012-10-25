﻿using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public partial class WebBrowserControl : BaseUserControl
    {
        public WebBrowserControl()
        {
            InitializeComponent();
        }

        public WebBrowser WebBrowser
        {
            get { return this.webBrowser; }
        }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool local = keyData != (Keys.F1 | Keys.Shift);

            if (keyData == Keys.F1 || keyData == (Keys.Shift | Keys.F1))
            {
                HelpHelper.GetHelp(WebBrowser.FindForm(), this, local);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData); 
        }
    }
}
