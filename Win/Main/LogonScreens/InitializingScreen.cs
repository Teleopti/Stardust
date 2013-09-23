using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class InitializingScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenManager _parent;

		public InitializingScreen(LogonScreenManager parent)
		{
			_parent = parent;
			InitializeComponent();
		}
	}
}
