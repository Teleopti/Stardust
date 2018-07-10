using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsDateRepository
	{
		IAnalyticsDate MaxDate();
		IAnalyticsDate MinDate();
		IAnalyticsDate Date(DateTime dateDate);
		IList<IAnalyticsDate> GetRange(DateTime fromDate, DateTime toDate);
		IList<IAnalyticsDate> GetAllPartial();
	}
}