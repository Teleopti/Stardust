
using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsPersonPeriodRepository
	{
	    IList<IAnalyticsPersonPeriod> GetPersonPeriods(Guid personCode);
	}
}