namespace Teleopti.Interfaces.Domain
{
	public interface IHasContractDayOffDefinition
	{
		bool IsDayOff(IScheduleDay scheduleDay);
	}
}
