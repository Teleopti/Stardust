using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class DaysOffSchedulingDesktopTest : SchedulingScenario
	{
		[Test]
		public void DaysOffFromContractScheduleShouldBePossibleToPlaceOnClosedDays()
		{

		}

		public DaysOffSchedulingDesktopTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerXXL76496, bool resourcePlannerHalfHourSkillTimeZon75509, bool resourcePlannerReducingSkillsDifferentOpeningHours76176) : base(seperateWebRequest, resourcePlannerXXL76496, resourcePlannerHalfHourSkillTimeZon75509, resourcePlannerReducingSkillsDifferentOpeningHours76176)
		{
		}
	}
}