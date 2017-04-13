using System.Windows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    /// <summary>
    /// SmartPart that loads a element
    /// </summary>
    public interface IExtendedSmartPartBase
    {
        void LoadExtender(UIElement sourceElement);
    }
}