using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoadingScreen : UserControl, ILogonStep
	{
		private readonly LogonView _parent;

		public LoadingScreen(LogonView parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		public void SetData(object data)
		{
			throw new System.NotImplementedException();
		}
	}
}
