using System.Windows;

namespace Teleopti.Common.UI.SmartPartControls.SmartParts
{
    /// <summary>
    /// SmartPart that loads a element
    /// </summary>
    public interface IExtendedSmartPartBase
    {
        void LoadExtender(UIElement sourceElement);
    }
}