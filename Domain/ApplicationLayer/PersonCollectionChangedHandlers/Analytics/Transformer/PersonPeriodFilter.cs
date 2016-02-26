using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer
{
	public class PersonPeriodFilter
	{
		private static DateTime Eternity = new DateTime(2059, 12, 31);

		public static IEnumerable<PersonPeriod> GetFiltered(IEnumerable<PersonPeriod> list)
		{
			foreach (var personPeriod in list)
				// Om startdatum är eternity eller större än max datum i analytics ska de inte tas med.
				if (personPeriod.StartDate.Date < Eternity)
				{
					yield return personPeriod;
				}
		}
	}
}
