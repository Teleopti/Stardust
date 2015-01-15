using System;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactSchedulePersonHandler : IAnalyticsFactSchedulePersonHandler
	{
		private readonly IAnalyticsScheduleRepository _repository;

		public AnalyticsFactSchedulePersonHandler(IAnalyticsScheduleRepository repository)
		{
			_repository = repository;
		}

		public IAnalyticsFactSchedulePerson Handle(Guid personPeriodCode)
		{
			var ret = new AnalyticsFactSchedulePerson();
			var person = _repository.PersonAndBusinessUnit(personPeriodCode);
			if (person != null)
			{
				ret.BusinessUnitId = person.BusinessUnitId;
				ret.PersonId = person.PersonId;
			}

			return ret;
		}
	}
}