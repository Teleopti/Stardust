using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ExtractBudgetGroupPeriods : IExtractBudgetGroupPeriods
	{
		public IEnumerable<Tuple<DateOnlyPeriod, IBudgetGroup>> BudgetGroupsForPeriod(IPerson person, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}
}