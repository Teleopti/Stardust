using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IAccessibilityDateViewModel : IBaseModel
    {
        DefaultAccessibility Accessibility { get; }

        DateTime Date { get; set; }

        string AccessibilityText { get; }
    }
}
