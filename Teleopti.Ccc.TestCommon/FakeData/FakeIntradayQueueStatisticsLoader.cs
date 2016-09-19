using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private IList<SkillWorkload> _latestStatisticsTimeAndWorkload = new List<SkillWorkload>();

		public IList<SkillWorkload> LoadActualWorkloadInSeconds(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return _latestStatisticsTimeAndWorkload;
		}

		public void Has(IList<SkillWorkload> latestStatisticsTimeAndWorkload)
		{
			_latestStatisticsTimeAndWorkload = latestStatisticsTimeAndWorkload;
		}
	}
}