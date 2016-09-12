﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return new AnalyticsTimeZone {TimeZoneId = 1};
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return new List<AnalyticsTimeZone>(); // TODO?
		}
	}
}