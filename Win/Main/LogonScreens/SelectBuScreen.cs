using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectBuScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;
	    private readonly LogonModel _model;

        public SelectBuScreen(LogonView parent, LogonModel model)
		{
			_parent = parent;
            _model = model;
            InitializeComponent();
            labelChooseBu.Text = Resources.PleaseChooseABusinessUnit;
		}

        public void SetData(LogonModel model)
		{
			lbxSelectBu.DataSource = model.AvailableBus;
		}

	    public LogonModel GetData()
	    {
            _model.SelectedBu = (IBusinessUnit)lbxSelectBu.SelectedItem;
	        return _model;
	    }

	    private void selectSdkScreenLoad(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
