﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	[DomainTest]
	public class SkillForecastIntervalCalculatorTest : IIsolateSystem
	{
		public SkillForecastIntervalCalculator Target;
		public FakeSkillRepository SkillRepository;
		public MutableNow Now;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public ISkillForecastReadModelRepository SkillForecastReadModelRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		

		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<SkillForecastIntervalCalculator>().For<SkillForecastIntervalCalculator>();
		}

		[Test]
		public void ShouldSaveTheIntervalsForSkillDay()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 11);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x",true);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7,10,200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			var skillList = new List<ISkill>(){skill};

			Target.Calculate(skillList, dtp);

			var skillStaffIntervals= SkillForecastReadModelRepository.LoadSkillForecast(new[] {skill.Id.GetValueOrDefault()}, new DateTimePeriod(dtp.StartDate.Utc(),dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(12);
			skillStaffIntervals.Count(x => x.Agents == 15.7).Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.AverageHandleTime == 210).Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Calls == 200).Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldSave15MinIntervalsForDifferentSkill()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var phoneSkillOpenHours = new TimePeriod(8, 9);
			var emailSkillOpenHours = new TimePeriod(7, 10);
			var phoneSkill = createSkill(15, "phone", phoneSkillOpenHours);
			var emailSkill = createSkill(60, "email", emailSkillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var phoneSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(phoneSkill, scenario, new DateTime(2019, 2, 17), phoneSkillOpenHours, 15.7, 10, 200);
			var emailSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(emailSkill, scenario, new DateTime(2019, 2, 17), emailSkillOpenHours, 5, 5, 30);
			SkillRepository.Add(phoneSkill);
			SkillDayRepository.AddRange(new [] {emailSkillDay,phoneSkillDay});
			ScenarioRepository.Add(scenario);

			var skillList = new List<ISkill>() { phoneSkill,emailSkill };

			Target.Calculate(skillList, dtp);

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(
				new[] {phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault()}, new DateTimePeriod(dtp.StartDate.Date.Utc(),dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(16);
		}

		[Test]
		public void ShouldSaveTheIntervaWitinProvidedPeriod()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay15 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 15), skillOpenHours, 11, 5, 100);
			var skillDay17 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), skillOpenHours, 5.7, 8, 200);
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange( new[]{ skillDay15,skillDay17});
			ScenarioRepository.Add(scenario);

			var skillList = new List<ISkill>() { skill };

			Target.Calculate(skillList, dtp);

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() },new DateTimePeriod(dtp.StartDate.Date.Utc(),dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			//skillStaffIntervals.Count(x => x.Agents == 15.7).Should().Be.EqualTo(4);
			//skillStaffIntervals.Count(x => x.AverageHandleTime == 210).Should().Be.EqualTo(4);
			//skillStaffIntervals.Count(x => x.Calls == 200).Should().Be.EqualTo(4);
		}

		protected ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}

	}
}