using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationWithExternalStaffTest : ResourceCalculationScenario
	{
		public IResourceCalculation Target;
		public CascadingResourceCalculationContextFactory CascadingResourceCalculationContextFactory;

		[TestCase(true, 5, ExpectedResult = 5)]
		[TestCase(false, 5, ExpectedResult = 0)]
		public double ShouldConsiderExternalStaffWhenResourceCalculate(bool defaultScenario, double externalResources)
		{
			var scenario = new Scenario {DefaultScenario = defaultScenario};
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, skillDay);
			var externalStaff = new[] {new ExternalStaff(externalResources, new[] {skill}, skillDay.SkillStaffPeriodCollection.First().Period)};

			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill}, externalStaff, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			return skillDay.SkillStaffPeriodCollection.First().CalculatedResource;
		}

		[Test]
		public void ShouldHandleExternalStaffCorrectlyWhenSkillWithOtherIntervalLengthExists()
		{
			var scenario = new Scenario { DefaultScenario = true };
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skillWithExternalStaff = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(60).IsOpenBetween(8, 9).WithId();
			var otherSkill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(15).IsOpenBetween(8, 9).WithId();
			var skillDayWithExternalStaff = skillWithExternalStaff.CreateSkillDayWithDemand(scenario, date, 10);
			var otherSkillDay = otherSkill.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[] { skillDayWithExternalStaff, otherSkillDay });
			var bpos = new[] { new ExternalStaff(1, new[] { skillWithExternalStaff }, skillDayWithExternalStaff.SkillStaffPeriodCollection.First().Period) };

			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[] { skillWithExternalStaff, otherSkill }, bpos, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			var firstExternalStaffPeriod = skillDayWithExternalStaff.SkillStaffPeriodCollection.First();
			firstExternalStaffPeriod.Period.ElapsedTime()
				.Should().Be.EqualTo(TimeSpan.FromHours(1));
			firstExternalStaffPeriod.CalculatedResource
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotExternalStaffOnIntervalAfterCurrent()
		{
			var scenario = new Scenario { DefaultScenario = true };
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, skillDay);
			var externalStaff = new[] { new ExternalStaff(10, new[] { skill }, skillDay.SkillStaffPeriodCollection.First().Period) };

			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[] { skill }, externalStaff, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			skillDay.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleMultiskilledExternalStaff()
		{
			var scenario = new Scenario {DefaultScenario = true};
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill1 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[]{skillDay1, skillDay2});
			var externalStaff = new[] {new ExternalStaff(1, new[] {skill1, skill2}, skillDay1.SkillStaffPeriodCollection.First().Period)};
			
			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill1, skill2}, externalStaff, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}


			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
		
		[Test]
		public void ShouldHandleMultiskilledWithDifferentActivitiesExternalStaff()
		{
			var scenario = new Scenario {DefaultScenario = true};
			var activity1 = new Activity().WithId();
			var activity2 = new Activity().WithId();
			var date = DateOnly.Today;
			var skill1 = new Skill().For(activity1).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity2).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[]{skillDay1, skillDay2});
			var externalStaff = new[] {new ExternalStaff(1, new[] {skill1, skill2}, skillDay1.SkillStaffPeriodCollection.First().Period)};
			
			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill1, skill2}, externalStaff, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
		
		[Test]
		public void ShouldShovelToSecondarySkill()
		{
			var scenario = new Scenario {DefaultScenario = true};
			var activity = new Activity().WithId();
			var date = DateOnly.Today;
			var skill1 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).CascadingIndex(1).WithId();
			var skill2 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).CascadingIndex(2).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, date, 5);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, date, 5);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, date, new[]{skillDay1, skillDay2});
			var externalStaff = new[] {new ExternalStaff(11, new[] {skill1, skill2}, skillDay1.SkillStaffPeriodCollection.First().Period)};
			
			using (CascadingResourceCalculationContextFactory.Create(
				ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod()), new[]{skill1, skill2}, externalStaff, false, date.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(date, resCalcData);
			}

			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(6); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(5); 
		}
	}
}