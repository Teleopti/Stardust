using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer
{
	public class PersonPeriodFilter
	{
		private static DateTime Eternity = new DateTime(2059, 12, 31);
		private DateTime minDate;
		private DateTime maxDate;

		public PersonPeriodFilter(DateTime minDate, DateTime maxDate)
		{
			this.minDate = minDate;
			this.maxDate = maxDate;
		}

		public IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> list)
		{
			foreach (var personPeriod in list)
			{
				// If start date is eternity or later then max date in analytics it should filtered out
				if (personPeriod.StartDate.Date < Eternity &&
					personPeriod.StartDate.Date <= maxDate)
				{
					yield return personPeriod;
				}
			}
		}
	}
}
