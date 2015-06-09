using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{
		private readonly IPersonPreferenceDayOccupationFactory _personPreferenceDayOccupationFactory;

		public PreferenceNightRestChecker(IPersonPreferenceDayOccupationFactory personPreferenceDayOccupationFactory)
		{
			_personPreferenceDayOccupationFactory = personPreferenceDayOccupationFactory;
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(DateOnly date)
		{
			throw new NotImplementedException();
		}
	}
}