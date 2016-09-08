using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsDateRepository
	{
		IAnalyticsDate MaxDate();
		IAnalyticsDate MinDate();
		IAnalyticsDate Date(DateTime dateDate);
	}
}