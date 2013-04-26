using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Service for getting affected budgetgroups from different personperiods within a period
	/// </summary>
	public interface IExtractBudgetGroupPeriods
	{
		/// <summary>
		/// Gets all affected periods within the supplied period and the corresponding budgetgroups
		/// </summary>
		IEnumerable<Tuple<DateOnlyPeriod, IBudgetGroup>> BudgetGroupsForPeriod(IPerson person, DateOnlyPeriod period);
	}
}