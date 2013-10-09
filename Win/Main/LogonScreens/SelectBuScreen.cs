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
		private LogonModel _model;

        public SelectBuScreen( LogonModel model)
		{
			_model = model;
            InitializeComponent();
            labelChooseBu.Text = Resources.PleaseChooseABusinessUnit;
		}

        public void SetData()
		{
			lbxSelectBu.DataSource = _model.AvailableBus;
		}

		public void GetData()
	    {
            _model.SelectedBu = (IBusinessUnit)lbxSelectBu.SelectedItem;
	    }

	    public void Release()
	    {
	        _model = null;
	        lbxSelectBu.DataSource = null;
	    }

	    private void selectSdkScreenLoad(object sender, EventArgs e)
        {
            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //BackColor = Color.FromArgb(175, Color.CornflowerBlue);
        }
	}
}
