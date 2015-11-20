using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchDayOffRulesModel
	{
		DayOffRulesModel FetchDefaultRules();
		IEnumerable<DayOffRulesModel> FetchAll();
	}
}