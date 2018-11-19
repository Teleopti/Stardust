using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class FakeNextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		public PlanningPeriod Current(PlanningGroup planningGroup)
		{
			return new PlanningPeriod(new DateOnlyPeriod(2015,06,10,2015,07,10),SchedulePeriodType.Day, 31, planningGroup).WithId();
		}
	}
}