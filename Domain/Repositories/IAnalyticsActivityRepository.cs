﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsActivityRepository
	{
		IList<AnalyticsActivity> Activities();
		void AddActivity(AnalyticsActivity activity);
		void UpdateActivity(AnalyticsActivity activity);
	}
}