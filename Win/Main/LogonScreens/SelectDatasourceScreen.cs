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

		private void buttonDataSourceListOK_Click(object sender, System.EventArgs e)
		{
			_parent.OkButtonClicked();
		}

		private void buttonDataSourcesListCancel_Click(object sender, System.EventArgs e)
		{
			_parent.CancelButtonClicked();
		}
	}
}
