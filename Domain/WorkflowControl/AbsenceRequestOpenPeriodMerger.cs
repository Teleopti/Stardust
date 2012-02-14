using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class AbsenceRequestOpenPeriodMerger : IAbsenceRequestOpenPeriodMerger
    {
        public IAbsenceRequestOpenPeriod Merge(IEnumerable<IAbsenceRequestOpenPeriod> absenceRequestOpenPeriods)
        {
            IAbsenceRequestOpenPeriod mergedPeriod = new AbsenceRequestOpenDatePeriod();
            int processIndex = (absenceRequestOpenPeriods.IsEmpty()) ? 2 : 1; //Deny if no periods
            mergedPeriod.AbsenceRequestProcess = mergedPeriod.AbsenceRequestProcessList[processIndex];
            foreach (IAbsenceRequestOpenPeriod absenceRequestOpenPeriod in absenceRequestOpenPeriods)
            {
                if (typeof(PersonAccountBalanceValidator).IsInstanceOfType(absenceRequestOpenPeriod.PersonAccountValidator))
                {
                    mergedPeriod.PersonAccountValidator = absenceRequestOpenPeriod.PersonAccountValidator;
                }
                if (PrioritizedProccesses.IndexOf(absenceRequestOpenPeriod.AbsenceRequestProcess.GetType())<
                    PrioritizedProccesses.IndexOf(mergedPeriod.AbsenceRequestProcess.GetType()))
                {
                    mergedPeriod.AbsenceRequestProcess = absenceRequestOpenPeriod.AbsenceRequestProcess;
                }
                if (typeof(StaffingThresholdValidator).IsInstanceOfType(absenceRequestOpenPeriod.StaffingThresholdValidator)
                    ||typeof(BudgetGroupAllowanceValidator).IsInstanceOfType(absenceRequestOpenPeriod.StaffingThresholdValidator))
                {
                    mergedPeriod.StaffingThresholdValidator = absenceRequestOpenPeriod.StaffingThresholdValidator;
                }
            }
            return mergedPeriod;
        }

        private static IList<Type> PrioritizedProccesses
        {
            get
            {
                return new List<Type>
                           {typeof (DenyAbsenceRequest), typeof (PendingAbsenceRequest), typeof (GrantAbsenceRequest)};
            }
        }
    }
}