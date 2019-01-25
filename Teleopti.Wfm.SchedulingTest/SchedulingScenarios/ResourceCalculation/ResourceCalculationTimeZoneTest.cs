using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationTimeZoneTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[TestCase("Arabian Standard Time", 15)]			//+04:00
		[TestCase("Iran Standard Time", 15)]			//+03:30
		[TestCase("Newfoundland Standard Time", 15)]	//-03:30
		[TestCase("Nepal Standard Time", 15)]			//+05:45
		[TestCase("Arabian Standard Time", 60)]			//+04:00
		[TestCase("Iran Standard Time", 60)]			//+03:30
		[TestCase("Newfoundland Standard Time", 60)]	//-03:30
		[TestCase("Nepal Standard Time", 60)]			//+05:45
		public void ShouldHandleSkillInHalfHourTimeZoneWithDifferentResolutions(string timeZoneId, int defaultResolution)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(timeZone).DefaultResolution(defaultResolution).WithId()
				.IsOpenBetween(8, 9);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(timeZone).WithPersonPeriod(skill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly,
				ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] {assignment}, new[] {skillDay}, false,
					false));

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[TestCase("Arabian Standard Time", 60)]         //+04:00
		[TestCase("Iran Standard Time", 60)]            //+03:30
		[TestCase("Newfoundland Standard Time", 60)]    //-03:30
		[TestCase("Nepal Standard Time", 60)]           //+05:45
		public void ShouldPlaceFullResourceOnInterval(string timeZoneId, int defaultResolution)
		{	
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(timeZone).DefaultResolution(defaultResolution).WithId().IsOpenBetween(8, 10);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(timeZone).WithPersonPeriod(skill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
		
			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { assignment }, new[] { skillDay}, false, false));

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(1);
		}

		[TestCase("Arabian Standard Time", 60)]         //+04:00
		[TestCase("Iran Standard Time", 60)]            //+03:30
		[TestCase("Newfoundland Standard Time", 60)]    //-03:30
		[TestCase("Nepal Standard Time", 60)]           //+05:45
		public void ShouldSplitResourceOnIntervals(string timeZoneId, int defaultResolution)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(timeZone).DefaultResolution(defaultResolution).WithId().IsOpenBetween(8, 10);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(timeZone).WithPersonPeriod(skill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 30, 9, 30));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { assignment }, new[] { skillDay }, false, false));

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(0.5);
			skillDay.SkillStaffPeriodCollection.Second().CalculatedResource.Should().Be.EqualTo(0.5);
		}
	}
}