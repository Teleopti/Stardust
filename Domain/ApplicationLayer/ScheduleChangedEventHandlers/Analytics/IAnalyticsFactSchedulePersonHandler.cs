using System;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactSchedulePersonHandler
	{
		IAnalyticsFactSchedulePerson Handle(Guid personPeriodCode);
	}
}