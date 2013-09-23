using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectSdkScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenView _parent;

		public SelectSdkScreen(LogonScreenView parent)
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
