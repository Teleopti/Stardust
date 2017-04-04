using System.Windows;

namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// SmartPart that loads a element
    /// </summary>
    public interface IExtendedSmartPartBase
    {
        void LoadExtender(UIElement sourceElement);
    }
}