using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class InitializingScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;

		public InitializingScreen(LogonView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		public void SetData(object data)
		{
		}
	}
}
