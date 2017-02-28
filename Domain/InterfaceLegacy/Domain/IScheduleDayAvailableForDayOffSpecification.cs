namespace Teleopti.Interfaces.Domain
{
	public interface IScheduleDayAvailableForDayOffSpecification
	{
		bool IsSatisfiedBy(IScheduleDay part);
	}
}
