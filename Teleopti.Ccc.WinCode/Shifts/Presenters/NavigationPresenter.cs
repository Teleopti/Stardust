﻿using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Presenters
{
    public class NavigationPresenter : BasePresenter, INavigationPresenter
    {
        public NavigationPresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
            : base(explorer,dataHelper)
        {}

        public IWorkShiftRuleSet CreateDefaultRuleSet()
        {
            var model = Explorer.Model;
            return DataWorkHelper.CreateDefaultRuleSet(model.ActivityCollection[0],
                                                       model.CategoryCollection[0],
                                                       model.DefaultStartPeriod,
                                                       model.StartPeriodSegment,
                                                       model.DefaultEndPeriod,
                                                       model.EndPeriodSegment);
        }

        public IRuleSetBag CreateDefaultRuleSetBag()
        {
            return DataWorkHelper.CreateDefaultRuleSetBag();
        }

        public void RemoveRuleSet(IWorkShiftRuleSet ruleSet, IRuleSetBag parentRuleSetBag)
        {
            if (parentRuleSetBag!=null)
            {
                parentRuleSetBag.RemoveRuleSet(ruleSet);
                return;
            }

            Explorer.Model.RemoveRuleSet(ruleSet);
            DataWorkHelper.Delete(ruleSet);
        }

        public void RemoveRuleSetBag(IRuleSetBag ruleSetBag, IWorkShiftRuleSet parentWorkShiftRuleSet)
        {
            if (parentWorkShiftRuleSet!=null)
            {
                parentWorkShiftRuleSet.RemoveRuleSetBag(ruleSetBag);
                return;
            }
            for (int index = ruleSetBag.RuleSetCollection.Count - 1; index >= 0; index--)
            {
                IWorkShiftRuleSet current = ruleSetBag.RuleSetCollection[index];
                current.RemoveRuleSetBag(ruleSetBag);
            }
            Explorer.Model.RemoveBag(ruleSetBag);
            if (ruleSetBag.Id != null)
                DataWorkHelper.Delete(ruleSetBag);
        }
    }
}
