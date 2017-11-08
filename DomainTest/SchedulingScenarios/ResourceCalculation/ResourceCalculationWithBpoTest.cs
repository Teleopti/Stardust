﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationWithBpoTest
	{
		public IResourceCalculation Target;

		[Test]
		public void ShouldConsiderBpoResourceWhenResourceCalculate()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var bpo = new BpoResource(5, new[]{skillDay.Skill}, skillDay.SkillStaffPeriodCollection.First().Period);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, skillDay, bpo);
			
			Target.ResourceCalculate(dateOnly, resCalcData);

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(5); 
		}

		[Test]
		public void ShouldHandleMultiskilledBpos()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var dateOnly = DateOnly.Today;
			var skill1 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var bpo = new BpoResource(1, new[] {skillDay1.Skill, skillDay2.Skill},
				skillDay1.SkillStaffPeriodCollection.First().Period);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[]{skillDay1, skillDay2}, bpo);
			
			Target.ResourceCalculate(dateOnly, resCalcData);

			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
		
		[Test]
		[Ignore("46265 - to be fixed")]
		public void ShouldHandleMultiskilledWithDifferentActivitiesBpos()
		{
			var scenario = new Scenario();
			var activity1 = new Activity().WithId();
			var activity2 = new Activity().WithId();
			var dateOnly = DateOnly.Today;
			var skill1 = new Skill().For(activity1).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skill2 = new Skill().For(activity2).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var bpo = new BpoResource(1, new[] {skillDay1.Skill, skillDay2.Skill},
				skillDay1.SkillStaffPeriodCollection.First().Period);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[]{skillDay1, skillDay2}, bpo);
			
			Target.ResourceCalculate(dateOnly, resCalcData);

			skillDay1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
			skillDay2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(0.5); 
		}
	}
}