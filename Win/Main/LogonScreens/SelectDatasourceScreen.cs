using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectDatasourceScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;
		
		public SelectDatasourceScreen(ILogonView logonView, LogonModel model)
		{
			_logonView = logonView;
			_model = model;
			InitializeComponent();
		}

		
		public void SetData()
		{
			radioButtonAdvWindows.Checked = _model.AuthenticationType == AuthenticationTypeOption.Windows;
			radioButtonAdvApplication.Checked = _model.AuthenticationType == AuthenticationTypeOption.Application;
			
			radioButtonAdvWindows.Select();
		}
		
		public void GetData()
		{
		}

		public void Release()
		{
			_model = null;
		}

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ActiveControl == null)
			{
				_logonView.HandleKeyPress(msg, keyData, true);
				return base.ProcessCmdKey(ref msg, keyData);
			}

			var controlType = ActiveControl.GetType();
			if (controlType == typeof(ComboBoxAdv) || controlType == typeof(RadioButtonAdv))
			{
				_logonView.HandleKeyPress(msg, keyData, true);
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnOkClick(sender, e);
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_logonView.BtnBackClick(sender, e);
		}

		private void radioButtonAdvWindows_CheckChanged(object sender, System.EventArgs e)
		{
			if (radioButtonAdvWindows.Checked)
			{
				_model.AuthenticationType = AuthenticationTypeOption.Windows;
			}
			else
			{
				_model.AuthenticationType = AuthenticationTypeOption.Application;
			}

		}

	}
}
