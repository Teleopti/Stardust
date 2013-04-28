using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ExtractBudgetGroupPeriods : IExtractBudgetGroupPeriods
	{
		public IEnumerable<Tuple<DateOnlyPeriod, IBudgetGroup>> BudgetGroupsForPeriod(IPerson person, DateOnlyPeriod period)
		{
			return
				from p in person.PersonPeriods(period)
				select new Tuple<DateOnlyPeriod, IBudgetGroup>(period.Intersection(p.Period).Value, p.BudgetGroup);
					      
		}
	}
}