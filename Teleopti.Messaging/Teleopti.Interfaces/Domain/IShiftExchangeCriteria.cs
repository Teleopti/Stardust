namespace Teleopti.Interfaces.Domain
{
	public interface IShiftExchangeCriteria
	{
		DateTimePeriod? ShiftWithin { get; }
		IScheduleDayFilterCriteria Criteria { get; set; }
		DateOnly ValidTo { get; set; }
		bool IsValid(DateTimePeriod? targetShiftPeriod, bool targetDayOff = false);
	}
}