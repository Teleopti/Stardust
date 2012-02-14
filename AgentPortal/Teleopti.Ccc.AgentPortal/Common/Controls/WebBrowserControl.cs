using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
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
    }
}
