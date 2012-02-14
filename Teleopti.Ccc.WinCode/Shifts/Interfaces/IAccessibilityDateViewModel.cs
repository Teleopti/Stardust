using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IAccessibilityDateViewModel : IBaseModel
    {
        DefaultAccessibility Accessibility { get; }

        DateTime Date { get; set; }

        string AccessibilityText { get; }
    }
}
