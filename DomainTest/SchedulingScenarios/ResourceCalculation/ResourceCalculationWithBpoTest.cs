using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationWithBpoTest : ResourceCalculationScenario
	{
		public IResourceCalculation Target;
		public CascadingResourceCalculationContextFactory CascadingResourceCalculationContextFactory;

		[Test]
		public void ShouldConsiderBpoResourceWhenResourceCalculate()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, skillDay);
			var bpos = new[] {new BpoResource(5, new[] {skill}, skillDay.SkillStaffPeriodCollection.First().Period)};

			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill}, bpos, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(5); 
		}

		[Test]
		public void ShouldHandleMultiskilledBpos()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill1 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[]{skillDay1, skillDay2});
			var bpos = new[] {new BpoResource(1, new[] {skill1, skill2}, skillDay1.SkillStaffPeriodCollection.First().Period)};
			
			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill1, skill2}, bpos, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}


			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
		
		[Test]
		public void ShouldHandleMultiskilledWithDifferentActivitiesBpos()
		{
			var scenario = new Scenario();
			var activity1 = new Activity().WithId();
			var activity2 = new Activity().WithId();
			var date = DateOnly.Today;
			var skill1 = new Skill().For(activity1).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity2).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[]{skillDay1, skillDay2});
			var bpos = new[] {new BpoResource(1, new[] {skill1, skill2}, skillDay1.SkillStaffPeriodCollection.First().Period)};
			
			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill1, skill2}, bpos, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
	}
}