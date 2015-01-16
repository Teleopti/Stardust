namespace Teleopti.Interfaces.Domain
{
	public interface IScheduleDayFilterCriteria
	{
		DateTimePeriod? ShiftWithin { get; }
		ShiftExchangeLookingForDay DayType { get; }
	}
}