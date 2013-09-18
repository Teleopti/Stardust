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
            get { return webBrowser; }
        }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1 || keyData == (Keys.Shift | Keys.F1))
			{
				var local = keyData == (Keys.F1 & Keys.Shift);
                HelpHelper.GetHelp(WebBrowser.FindForm(), this, local);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData); 
        }
    }
}
