using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchDayOffRulesModel
	{
		IEnumerable<DayOffRulesModel> FetchAllWithoutAgentGroup();
		DayOffRulesModel Fetch(Guid id);
		IEnumerable<DayOffRulesModel> FetchAll(Guid agentGroupId);
	}
}