using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface INavigationPresenter : IPresenterBase
    {
        IWorkShiftRuleSet CreateDefaultRuleSet();

        IRuleSetBag CreateDefaultRuleSetBag();

        void RemoveRuleSet(IWorkShiftRuleSet ruleSet, IRuleSetBag parentRuleSetBag);


        void RemoveRuleSetBag(IRuleSetBag ruleSetBag);


    }
}
