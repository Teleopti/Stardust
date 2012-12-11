using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public interface IUpdateScheduleProjectionReadModel
	{
		void Execute(IScheduleRange scheduleRange, DateOnlyPeriod dateOnlyPeriod);
	}
}