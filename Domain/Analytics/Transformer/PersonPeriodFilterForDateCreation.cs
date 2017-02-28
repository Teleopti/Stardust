using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodFilterForDateCreation : IPersonPeriodFilter
	{
		private readonly IAnalyticsDateRepository _analyticsDateRepository;

		public PersonPeriodFilterForDateCreation(IAnalyticsDateRepository analyticsDateRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
		{
			var minDate = _analyticsDateRepository.MinDate().DateDate;

			return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < AnalyticsDate.Eternity.DateDate &&
											  personPeriod.EndDate().Date >= minDate);
		}
	}
}