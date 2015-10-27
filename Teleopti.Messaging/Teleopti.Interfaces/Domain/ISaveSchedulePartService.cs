namespace Teleopti.Interfaces.Domain
{
	public interface ISaveSchedulePartService
	{
		void Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);
	}
}