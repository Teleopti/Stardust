using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodFilter : IPersonPeriodFilter
	{
		private readonly DateTime minDate;
		private readonly DateTime maxDate;

		public PersonPeriodFilter(IAnalyticsDateRepository analyticsDateRepository)
		{
			minDate = analyticsDateRepository.MinDate().DateDate;
			maxDate = analyticsDateRepository.MaxDate().DateDate;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
		{
			return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < AnalyticsDate.Eternity.DateDate &&
											  personPeriod.StartDate.Date <= maxDate &&
											  personPeriod.EndDate().Date >= minDate);
		}
	}
}
