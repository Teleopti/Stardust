using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectDatasourceScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;
		private readonly bool _showDataSourceSelection;
		private List<IDataSourceContainer> _logonableWindowsDataSources;
		private List<IDataSourceContainer> _availableApplicationDataSources;

		public SelectDatasourceScreen(ILogonView logonView, LogonModel model, bool showDataSourceSelection)
		{
			_logonView = logonView;
			_model = model;
			_showDataSourceSelection = showDataSourceSelection;
			InitializeComponent();
			if (!DesignMode)
				runTimeDesign();
		}

		private void runTimeDesign()
		{
			comboBoxAdvDataSource.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
		}
		public void SetData()
		{
			_logonableWindowsDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows).ToList();
			_availableApplicationDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application).ToList();
			comboBoxAdvDataSource.DisplayMember = "DataSourceName";
			comboBoxAdvDataSource.DataSource = _logonableWindowsDataSources;

			if (_logonableWindowsDataSources.Count < 1 && _showDataSourceSelection)
			{
				radioButtonAdvApplication.Visible = false;
				radioButtonAdvWindows.Visible = false;
				comboBoxAdvDataSource.DataSource = _availableApplicationDataSources;
			}

			setCorrectList();
			comboBoxAdvDataSource.Select();
			
			comboBoxAdvDataSource.Visible = _showDataSourceSelection;
			//labelChooseDataSource.Visible = _showDataSourceSelection;
		}

		private void setCorrectList()
		{
			radioButtonAdvWindows.Checked = _model.AuthenticationType == AuthenticationTypeOption.Windows;
			radioButtonAdvApplication.Checked = _model.AuthenticationType == AuthenticationTypeOption.Application;
			comboBoxAdvDataSource.DataSource = radioButtonAdvWindows.Checked ? _logonableWindowsDataSources : _availableApplicationDataSources;
		}

		public void GetData()
		{
			_model.SelectedDataSourceContainer = (IDataSourceContainer)comboBoxAdvDataSource.SelectedItem;
		}

		public void Release()
		{
			_model = null;
			comboBoxAdvDataSource.DataSource = null;
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
			comboBoxAdvDataSource.DataSource = null;
			comboBoxAdvDataSource.DisplayMember = "DataSourceName";
			if (radioButtonAdvWindows.Checked)
			{
				_model.AuthenticationType = AuthenticationTypeOption.Windows;
				comboBoxAdvDataSource.DataSource = _logonableWindowsDataSources;
			}
			else
			{
				_model.AuthenticationType = AuthenticationTypeOption.Application;
				comboBoxAdvDataSource.DataSource = _availableApplicationDataSources;
			}

			comboBoxAdvDataSource.Select();
		}

	}
}
