using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoginScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenView _parent;

		public LoginScreen(LogonScreenView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			_parent.OkButtonClicked();
		}

		private void buttonLogOnCancel_Click(object sender, System.EventArgs e)
		{
			_parent.CancelButtonClicked();
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_parent.BackButtonClicked();
		}
	}
}
