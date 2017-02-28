using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IIsDayScheduled
	{
		bool Check(IScheduleDay scheduleDay);
	}
}