using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IVisualizeViewModel
    {
        IWorkShiftRuleSet WorkShiftRuleSet { get; }

        ReadOnlyCollection<ReadOnlyCollection<VisualPayloadInfo>> PayloadInfo { get; }
    }
}
