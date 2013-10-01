using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoginScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;

		public LoginScreen(LogonView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			// TODO
			_parent.OkButtonClicked(new object());
		}

		private void buttonLogOnCancel_Click(object sender, System.EventArgs e)
		{
			_parent.CancelButtonClicked();
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_parent.BackButtonClicked();
		}

		public void SetData(object data)
		{
			throw new System.NotImplementedException();
		}
	}
}
