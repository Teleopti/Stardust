using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
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
            labelChooseDataSource.Text = Resources.PleaseChooseADatasource;
            tabPageWindowsDataSources.Text = Resources.WindowsLogon;
            tabPageApplicationDataSources.Text = Resources.ApplicationLogon;
		}

		public void SetData()
		{
            var logonableWindowsDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows).ToList();
            var availableApplicationDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application).ToList();

            listBoxApplicationDataSources.DataSource = availableApplicationDataSources;
            listBoxWindowsDataSources.DataSource = logonableWindowsDataSources;
			if (logonableWindowsDataSources.Count < 1)
			{
			    var idx = tabControlChooseDataSource.TabPages.IndexOf(tabPageWindowsDataSources);
                if(idx > -1)
                    tabControlChooseDataSource.TabPages.RemoveAt(idx);
				tabControlChooseDataSource.SelectedIndex = 0;
			}

            tabControlChooseDataSource.Visible = true;
		}

		public void GetData()
	    {
            _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxApplicationDataSources.SelectedItem;
            if (tabControlChooseDataSource.SelectedTab.Equals(tabPageWindowsDataSources))
                _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxWindowsDataSources.SelectedItem;
	    }

	    public void Release()
	    {
	        _model = null;
	        listBoxWindowsDataSources.DataSource = null;
	        listBoxApplicationDataSources.DataSource = null;
	    }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, listBoxWindowsDataSources.Focused || listBoxApplicationDataSources.Focused);
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
