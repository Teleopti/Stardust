using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;

		public SelectSdkScreen(LogonView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		private void buttonDataSourceListOK_Click(object sender, System.EventArgs e)
		{
			_parent.OkButtonClicked(lbxSelectSDK.SelectedItem);
		}

		private void buttonDataSourcesListCancel_Click(object sender, System.EventArgs e)
		{
			_parent.CancelButtonClicked();
		}

		public void SetData(object data)
		{
			lbxSelectSDK.DataSource = data;
		}
	}
}
