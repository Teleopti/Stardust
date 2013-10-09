using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
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
	        labelChooseSDK.Text = Resources.PleaseChooseSDK;
		}

        public void SetData()
		{
			lbxSelectSDK.DataSource = _model.Sdks;
		}

		public void GetData()
        {
            _model.SelectedSdk = lbxSelectSDK.SelectedItem.ToString();
        }

	    public void Release()
	    {
	       _model = null;
	        lbxSelectSDK.DataSource = null;
	    }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, lbxSelectSDK.Focused);
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
