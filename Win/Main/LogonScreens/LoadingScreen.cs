using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoadingScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenManager _parent;

		public LoadingScreen(LogonScreenManager parent)
		{
			_parent = parent;
			InitializeComponent();
		}
	}
}
