using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectDatasourceScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;

		public SelectDatasourceScreen(LogonView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		private void buttonDataSourceListOK_Click_1(object sender, System.EventArgs e)
		{
			_parent.OkButtonClicked(new object());
		}

		private void buttonDataSourcesListCancel_Click_1(object sender, System.EventArgs e)
		{
			_parent.CancelButtonClicked();
		}

		public void SetData(object data)
		{
			listBoxApplicationDataSources.DataSource = data;
			listBoxWindowsDataSources.DataSource = data;
		}
	}
}
