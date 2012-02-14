namespace Teleopti.Ccc.Win.Common
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
