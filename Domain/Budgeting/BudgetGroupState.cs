using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	// cut/paste from schedulingresultstateholder
	public class BudgetGroupState
	{
		private readonly ConcurrentDictionary<IBudgetDay, double> addedAbsenceMinutesDictionary = new ConcurrentDictionary<IBudgetDay, double>();
		public double AddedAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			double value;
			if (!addedAbsenceMinutesDictionary.TryGetValue(budgetDay, out value))
				return 0;
			return value;
		}

		public void AddAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay, double minutes)
		{
			addedAbsenceMinutesDictionary.AddOrUpdate(budgetDay, minutes,(b, m) => m+minutes);
		}

		public void SubtractAbsenceMinutesDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay, double minutes)
		{
			addedAbsenceMinutesDictionary.AddOrUpdate(budgetDay, 0, (b, m) => Math.Max(0, m - minutes));
		}

		private readonly ConcurrentDictionary<IBudgetDay, int> addedAbsenceHeadCountDictionary = new ConcurrentDictionary<IBudgetDay, int>();
		public int AddedAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			int value;
			if (!addedAbsenceHeadCountDictionary.TryGetValue(budgetDay, out value))
				return 0;
			return value;
		}

		public void AddAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			addedAbsenceHeadCountDictionary.AddOrUpdate(budgetDay,1, (b, m) => m + 1);
		}

		public void SubtractAbsenceHeadCountDuringCurrentRequestHandlingCycle(IBudgetDay budgetDay)
		{
			addedAbsenceHeadCountDictionary.AddOrUpdate(budgetDay, 0, (b, m) => Math.Max(0, m - 1));
		}
	}
}