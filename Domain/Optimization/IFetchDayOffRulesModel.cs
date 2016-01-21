using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchDayOffRulesModel
	{
		IEnumerable<DayOffRulesModel> FetchAll();
		DayOffRulesModel Fetch(Guid id);
	}
}