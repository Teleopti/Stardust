namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public interface ISchedulePlanningPeriodCommandHandler
	{
		object Execute(SchedulePlanningPeriodCommand command);
	}
}