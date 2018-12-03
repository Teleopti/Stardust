using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IPersonRuleSetBagProvider
	{
		IRuleSetBag ForDate(IPerson person, DateOnly date);
		IDictionary<DateOnly, IRuleSetBag> ForPeriod(IPerson person, DateOnlyPeriod period);
	}
}