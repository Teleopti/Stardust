using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectBuScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;

		public SelectBuScreen(ILogonView logonView, LogonModel model)
		{
	        _logonView = logonView;
	        _model = model;
            InitializeComponent();
            labelChooseBu.Text = Resources.PleaseChooseABusinessUnit;
			buttonLogOnCancel.Text = Resources.Cancel;
			buttonLogOnOK.Text = Resources.Ok;
			btnBack.Text = Resources.Back;
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

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, lbxSelectBu.Focused);
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

		private void buttonLogOnCancel_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnCancelClick(sender, e);
		}
	}
}
