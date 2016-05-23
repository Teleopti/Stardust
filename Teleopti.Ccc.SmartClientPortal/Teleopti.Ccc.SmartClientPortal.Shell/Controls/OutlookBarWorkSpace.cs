using System;
using System.Diagnostics;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	public partial class OutlookBarWorkSpace : UserControl
	{
		private Control _navigatorControl;
	    private string _previewUrl;


        public OutlookBarWorkSpace()
		{
            InitializeComponent();
            linkLabel1.Text = Resources.PreviewTheNewIntradayTool;
        }

		public Control NavigatorControl
		{
			get { return _navigatorControl; }
		}

		public void SetHeader(ModulePanelItem selectedModulePanelItem)
		{
			selectedModulePanel1.Set(selectedModulePanelItem);
		}

        public void SetNavigatorControl(UserControl navigatorControl, string previewText, Uri previewUrl)
        {
            panel1.Controls.Clear();

            linkLabel1.Text = previewText;
            this.tableLayoutPanel1.SuspendLayout();
            if (previewUrl == null)
            {
                _previewUrl = null;
                tableLayoutPanel1.RowStyles[1].Height = 0F;
            }
            else
            {
                _previewUrl = previewUrl.ToString();
                tableLayoutPanel1.RowStyles[1].Height = 45F;
            }
            this.tableLayoutPanel1.ResumeLayout();

            
			panel1.Controls.Add(navigatorControl);
			_navigatorControl = navigatorControl;
			navigatorControl.Dock = DockStyle.Fill;
			navigatorControl.Focus();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_previewUrl);
        }
	}
}
