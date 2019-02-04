using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationContextPeriodTest : ResourceCalculationScenario
	{
		public CascadingResourceCalculationContextFactory ResourceCalculationContextFactory;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test, Combinatorial]
		public void ShouldBeEnoughToPassInActualPeriod(
			[Values("Mountain Standard Time", "UTC", "Singapore Standard Time")] string myTimeZone,
			[Values("Mountain Standard Time", "UTC", "Singapore Standard Time")] string agentTimeZone,
			[Values(-1, 0, 1)] int shiftDayFromToday,
			[Values(0, 23)] int shiftStart
			) 
		{
			var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(myTimeZone);
			TimeZoneGuard.Set(userTimeZone);
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(agentTimeZone)).IsOpen();
			var today = DateOnly.Today;
			var agent = new Person().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(agentTimeZone)).WithPersonPeriod(skill);
			var assignment = new PersonAssignment(agent, scenario, today.AddDays(shiftDayFromToday)).WithLayer(activity, new TimePeriod(shiftStart, shiftStart + 8));
			var period = new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1));
			var skillDay = skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromMinutes(30));
			var resourceCalculationData = ResourceCalculationDataCreator.WithData(scenario, period , new[] { assignment }, skillDay, false, false);

			var expected = assignment.Period.Intersect(today.ToDateTimePeriod(userTimeZone));
			using (ResourceCalculationContextFactory.Create(resourceCalculationData.Schedules, resourceCalculationData.Skills, Enumerable.Empty<ExternalStaff>(), false, today.ToDateOnlyPeriod()))
			{
				var partOfAssignmentInContext = ResourceCalculationContext.Fetch().AffectedResources(activity, today.ToDateTimePeriod(userTimeZone)).Any();
				partOfAssignmentInContext.Should().Be.EqualTo(expected);
			}
		}
	}
}