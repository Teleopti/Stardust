using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
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
            if(!DesignMode)
				runTimeDesign();
		}

		private void runTimeDesign()
		{
			comboBoxAdvBUList.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
		}
		
        public void SetData()
        {
	        comboBoxAdvBUList.DisplayMember = "Name";
			comboBoxAdvBUList.DataSource = _model.AvailableBus;
		}

		public void GetData()
	    {
			_model.SelectedBu = (IBusinessUnit)comboBoxAdvBUList.SelectedItem;
	    }

	    public void Release()
	    {
	        _model = null;
			comboBoxAdvBUList.DataSource = null;
	    }

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, comboBoxAdvBUList.Focused);
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
