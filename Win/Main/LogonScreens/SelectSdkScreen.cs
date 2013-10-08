using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private LogonModel _model;

	    public SelectSdkScreen(LogonModel model)
		{
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

	    public void Release()
	    {
	       _model = null;
	        lbxSelectSDK.DataSource = null;
	    }

	    private void selectSdkScreenLoad(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
