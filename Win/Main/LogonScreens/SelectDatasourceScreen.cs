using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectDatasourceScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;
	    private readonly LogonModel _model;

	    public SelectDatasourceScreen(LogonView parent, LogonModel model)
		{
			_parent = parent;
            _model = model;
            InitializeComponent();
		}

		public void SetData(LogonModel model)
		{
            var logonableWindowsDataSources = model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows).ToList();
            var availableApplicationDataSources = model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application).ToList();

            listBoxApplicationDataSources.DataSource = availableApplicationDataSources;
            listBoxWindowsDataSources.DataSource = logonableWindowsDataSources;
			if (logonableWindowsDataSources.Count < 1)
			{
				tabControlChooseDataSource.TabPages.RemoveAt(0);
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

        private void SelectDatasourceScreen_Load(object sender, System.EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(175, Color.CornflowerBlue);
            
        }

        
	}
}
