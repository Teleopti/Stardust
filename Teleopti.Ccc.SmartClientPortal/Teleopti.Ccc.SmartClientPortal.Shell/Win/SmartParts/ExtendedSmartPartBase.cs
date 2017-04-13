using System.Windows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    public partial class ExtendedSmartPartBase : SmartPartBase, IExtendedSmartPartBase
    {
      
        public ExtendedSmartPartBase()
        {
            InitializeComponent();
        }

        public void LoadExtender(UIElement sourceElement)
        {
            HostContainer.Child = sourceElement;
        }
    }
}
