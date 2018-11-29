namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShiftExchangeCriteria
	{
		bool IsValid(DateTimePeriod? targetShiftPeriod, bool targetDayOff = false);
		DateOnly ValidTo { get; set; }
		ShiftExchangeLookingForDay DayType { get; set; }		
	}
}
