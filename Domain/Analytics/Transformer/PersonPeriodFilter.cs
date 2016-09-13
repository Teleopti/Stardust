using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodFilter : IPersonPeriodFilter
	{
		private readonly IAnalyticsDateRepository _analyticsDateRepository;

		public PersonPeriodFilter(IAnalyticsDateRepository analyticsDateRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
		{
			var minDate = _analyticsDateRepository.MinDate().DateDate;
			var maxDate = _analyticsDateRepository.MaxDate().DateDate;

			return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < AnalyticsDate.Eternity.DateDate &&
											  personPeriod.StartDate.Date <= maxDate &&
											  personPeriod.EndDate().Date >= minDate);
		}
	}
}
