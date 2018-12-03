using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class PersonPreferenceDayOccupation
	{
		public StartTimeLimitation StartTimeLimitation { get; set; }
		public EndTimeLimitation EndTimeLimitation { get; set; }
		public bool HasFullDayAbsence { get; set; }
		public bool HasDayOff { get; set; }
		public bool HasShift { get; set; }
		public bool HasPreference { get; set; }
	}

	public interface IPersonPreferenceDayOccupationFactory
	{
		PersonPreferenceDayOccupation GetPreferenceDayOccupation(IPerson person, DateOnly date);

		Dictionary<DateOnly, PersonPreferenceDayOccupation> GetPreferencePeriodOccupation(IPerson person,
			DateOnlyPeriod period);
	}
}