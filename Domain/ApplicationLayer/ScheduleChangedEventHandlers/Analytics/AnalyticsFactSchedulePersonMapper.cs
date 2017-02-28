using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactSchedulePersonMapper
	{
		IAnalyticsFactSchedulePerson Map(Guid personPeriodCode);
	}

	public class AnalyticsFactSchedulePersonMapper : IAnalyticsFactSchedulePersonMapper
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

		public AnalyticsFactSchedulePersonMapper(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		public IAnalyticsFactSchedulePerson Map(Guid personPeriodCode)
		{
			var ret = new AnalyticsFactSchedulePerson();
			var person = _analyticsPersonPeriodRepository.PersonAndBusinessUnit(personPeriodCode);
			if (person != null)
			{
				ret.BusinessUnitId = person.BusinessUnitId;
				ret.PersonId = person.PersonId;
			}

			return ret;
		}
	}
}