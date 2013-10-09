using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
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

	    private void selectSdkScreenLoad(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
