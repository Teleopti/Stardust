using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleRangePersister : IScheduleRangePersister
	{
		public SchedulePersistResult Persist(IScheduleRange scheduleRange)
		{
			return new SchedulePersistResult();
		}
	}
}