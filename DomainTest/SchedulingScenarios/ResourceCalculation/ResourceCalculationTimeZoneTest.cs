using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationTimeZoneTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;
		
		[Ignore("PBI75509 to be fixed")]
		[TestCase("Arabian Standard Time", 15)] //+04:00
		[TestCase("Iran Standard Time", 15)]    //+03:30
		[TestCase("Arabian Standard Time", 60)] //+04:00
		[TestCase("Iran Standard Time", 60)]    //+03:30
		public void ShouldHandleSkillInHalfHourTimeZoneWithDifferentResolutions(string timeZoneId, int defaultResolution)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(timeZone).DefaultResolution(defaultResolution).WithId().IsOpenBetween(8, 9);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(timeZone).WithPersonPeriod(skill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { assignment }, new[] { skillDay }, false, false));

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}
	}
}