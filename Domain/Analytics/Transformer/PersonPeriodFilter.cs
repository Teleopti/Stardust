using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodFilter
	{
		private static readonly DateTime eternity = new DateTime(2059, 12, 31);
		private readonly DateTime minDate;
		private readonly DateTime maxDate;

		public PersonPeriodFilter(DateTime minDate, DateTime maxDate)
		{
			this.minDate = minDate;
			this.maxDate = maxDate;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods)
		{
			return personPeriods.Where(personPeriod => personPeriod.StartDate.Date < eternity &&
											  personPeriod.StartDate.Date <= maxDate &&
											  personPeriod.EndDate().Date >= minDate);
		}
	}
}
