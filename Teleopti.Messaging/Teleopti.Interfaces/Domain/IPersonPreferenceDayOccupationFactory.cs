namespace Teleopti.Interfaces.Domain
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
		PersonPreferenceDayOccupation GetPreferenceDayOccupation(DateOnly date);
	}
}
