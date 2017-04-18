using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface INavigationPresenter : IPresenterBase
    {
        IWorkShiftRuleSet CreateDefaultRuleSet();

        IRuleSetBag CreateDefaultRuleSetBag();

        void RemoveRuleSet(IWorkShiftRuleSet ruleSet, IRuleSetBag parentRuleSetBag);


        void RemoveRuleSetBag(IRuleSetBag ruleSetBag);


    }
}
