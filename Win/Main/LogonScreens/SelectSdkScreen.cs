using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;
	    private readonly LogonModel _model;

	    public SelectSdkScreen(LogonView parent, LogonModel model)
		{
			_parent = parent;
            _model = model;
            InitializeComponent();
		}

        public void SetData(LogonModel model)
		{
			lbxSelectSDK.DataSource = model.Sdks;
		}

        public LogonModel GetData()
        {
            _model.SelectedSdk = lbxSelectSDK.SelectedItem.ToString();
            return _model;
        }

        private void selectSdkScreenLoad(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
