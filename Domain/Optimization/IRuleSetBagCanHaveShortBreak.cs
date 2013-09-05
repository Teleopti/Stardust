using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IRuleSetBagCanHaveShortBreak
    {
        bool CanHaveShortBreak(IRuleSetBag ruleSetBag);
    }
}