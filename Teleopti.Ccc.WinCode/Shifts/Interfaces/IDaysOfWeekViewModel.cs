using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IDaysOfWeekViewModel : IBaseModel
    {
        DefaultAccessibility Accessibility { get; }

        bool Sunday { get; set; }

        bool Monday { get; set; }

        bool Tuesday { get; set; }

        bool Wednesday { get; set; }

        bool Thursday { get; set; }

        bool Friday { get; set; }

        bool Saturday { get; set; }

        string AccessibilityText { get; }
    }
}
