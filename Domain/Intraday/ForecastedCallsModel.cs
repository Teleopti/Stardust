using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{

	public class ForecastedCallsModel
	{
		public Dictionary<Guid, List<SkillIntervalStatistics>> CallsPerSkill { get; set; }
		public IList<SkillDayStatsRange> SkillDayStatsRange { get; set; }
	}
}