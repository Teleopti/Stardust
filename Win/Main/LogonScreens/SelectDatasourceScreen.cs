using System.Collections.Generic;
using System.Drawing;
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
		private LogonModel _model;

	    public SelectDatasourceScreen(LogonModel model)
		{
			_model = model;
            InitializeComponent();
            labelChooseDataSource.Text = Resources.PleaseChooseADatasource;
            tabPageWindowsDataSources.Text = Resources.WindowsLogon;
            tabPageApplicationDataSources.Text = Resources.ApplicationLogon;
		}

		public void SetData(LogonModel model)
		{
            var logonableWindowsDataSources = model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows).ToList();
            var availableApplicationDataSources = model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application).ToList();

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

	    public LogonModel GetData()
	    {
            _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxApplicationDataSources.SelectedItem;
            if (tabControlChooseDataSource.SelectedTab.Equals(tabPageWindowsDataSources))
                _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxWindowsDataSources.SelectedItem;

            return _model;
	    }

	    public void Release()
	    {
	        _model = null;
	        listBoxWindowsDataSources.DataSource = null;
	        listBoxApplicationDataSources.DataSource = null;
	    }

	    private void SelectDatasourceScreen_Load(object sender, System.EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
            
        }

        
	}
}
