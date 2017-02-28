using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IWorkShiftRuleSetCanHaveShortBreak
    {
        bool CanHaveShortBreak(IWorkShiftRuleSet workShiftRuleSet);
    }
}