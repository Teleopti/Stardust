namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleDayAvailableForDayOffSpecification
	{
		bool IsSatisfiedBy(IScheduleDay part);
	}
}
