using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IDataHelper
    {
        void Save(IWorkShiftRuleSet ruleSet);

        void PersistAll();

        void Delete(IWorkShiftRuleSet ruleSet);

        void Delete(IRuleSetBag bag);

        IList<IWorkShiftRuleSet> FindRuleSets(IUnitOfWork uow);

        IList<IRuleSetBag> FindRuleSetBags(IUnitOfWork uow);

        TypedBindingCollection<IActivity> FindAllActivities(IUnitOfWork uow);

        TypedBindingCollection<IShiftCategory> FindAllCategories(IUnitOfWork uow);

        ReadOnlyCollection<string> FindAllOperatorLimits();

        ReadOnlyCollection<string> FindAllAccessibilities();

        IWorkShiftRuleSet CreateDefaultRuleSet(IActivity defaultActivity,
                                               IShiftCategory category,
                                               TimePeriod defaultStartPeriod,
                                               TimeSpan defaultStartPeriodSegment,
                                               TimePeriod defaultEndPeriod,
                                               TimeSpan defaultEndPeriodSegment);

        IRuleSetBag CreateDefaultRuleSetBag();

        ActivityTimeLimiter CreateDefaultActivityTimeLimiter(IWorkShiftRuleSet ruleSet, TimeSpan timeLimit);

        int DefaultSegment();

        bool HasUnsavedData();
    }
}
