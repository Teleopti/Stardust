using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;

		public SelectSdkScreen(ILogonView logonView, LogonModel model)
		{
		    _logonView = logonView;
		    _model = model;
            InitializeComponent();
			if(!DesignMode)
				runTimeDesign();
		}

		private void runTimeDesign()
		{
			comboBoxAdvSDKList.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
		}

        public void SetData()
		{
			comboBoxAdvSDKList.DataSource = _model.Sdks;
		}

		public void GetData()
        {
			_model.SelectedSdk = comboBoxAdvSDKList.SelectedItem.ToString();
        }

	    public void Release()
	    {
	       _model = null;
		   comboBoxAdvSDKList.DataSource = null;
	    }

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, true);
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_logonView.BtnBackClick(sender, e);
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnOkClick(sender, e);
		}
	}
}
