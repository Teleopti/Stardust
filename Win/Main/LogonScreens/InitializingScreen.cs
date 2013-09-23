using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class InitializingScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenView _parent;

		public InitializingScreen(LogonScreenView parent)
		{
			_parent = parent;
			InitializeComponent();
		}
	}
}
