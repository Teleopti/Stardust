using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodFilter : IPersonPeriodFilter
	{
		private static readonly DateTime eternity = new DateTime(2059, 12, 31);
		private readonly DateTime minDate;
		private readonly DateTime maxDate;

		public PersonPeriodFilter(IAnalyticsDateRepository analyticsDateRepository)
		{
			minDate = analyticsDateRepository.MinDate().DateDate;
			maxDate = analyticsDateRepository.MaxDate().DateDate;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
		{
			return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < eternity &&
											  personPeriod.StartDate.Date <= maxDate &&
											  personPeriod.EndDate().Date >= minDate);
		}
	}

	//public class PersonPeriodWithoutMaxDate : IPersonPeriodFilter
	//{
	//	private static readonly DateTime eternity = new DateTime(2059, 12, 31);
	//	private readonly DateTime minDate;

	//	public PersonPeriodWithoutMaxDate(IAnalyticsDateRepository analyticsDateRepository)
	//	{
	//		minDate = analyticsDateRepository.MinDate().DateDate;
	//	}

	//	public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
	//	{
	//		return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < eternity &&
	//										  personPeriod.EndDate().Date >= minDate);
	//	}
	//}

	public interface IPersonPeriodFilter
	{
		IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods);
	}
}
