using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class LoadingScreen : UserControl, ILogonStep
	{
		private readonly LogonScreenView _parent;

		public LoadingScreen(LogonScreenView parent)
		{
			_parent = parent;
			InitializeComponent();
		}
	}
}
