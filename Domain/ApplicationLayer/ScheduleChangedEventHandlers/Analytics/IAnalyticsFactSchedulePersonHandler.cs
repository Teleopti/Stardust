using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactSchedulePersonHandler
	{
		IAnalyticsFactSchedulePerson Handle(Guid personPeriodCode);
	}
}