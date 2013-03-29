using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public interface IUpdateScheduleProjectionReadModel
	{
		void Execute(IScheduleRange scheduleRange, DateOnlyPeriod dateOnlyPeriod);
	}
}