namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{

    public partial class AbstractNavigator : BaseUserControl
    {
        protected AbstractNavigator()
        {
            InitializeComponent();
        }

		public virtual void RefreshNavigator() { }
    }
}
