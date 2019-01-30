using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
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
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 11);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x",true);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7,10,200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> {skillDay.Id.GetValueOrDefault() });

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
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var phoneSkillOpenHours = new TimePeriod(8, 9);
			var emailSkillOpenHours = new TimePeriod(7, 10);
			var phoneSkill = createSkill(15, "phone", phoneSkillOpenHours);
			var emailSkill = createSkill(60, "email", emailSkillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var phoneSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(phoneSkill, scenario, new DateTime(2019, 2, 17), phoneSkillOpenHours, 15.7, 10, 200).WithId();
			var emailSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(emailSkill, scenario, new DateTime(2019, 2, 17), emailSkillOpenHours, 5, 5, 30).WithId();
			SkillRepository.Add(phoneSkill);
			SkillDayRepository.AddRange(new [] {emailSkillDay,phoneSkillDay});
			ScenarioRepository.Add(scenario);

		Target.Calculate(new List<Guid> { phoneSkillDay.Id.GetValueOrDefault(), emailSkillDay.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(
				new[] {phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault()}, new DateTimePeriod(dtp.StartDate.Date.Utc(),dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(16);
		}
		

		[Test, Ignore("Tested in another way. tested in filter tests that are below")]
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

			//Target.Calculate(new List<ISkillDay> { skillDay15});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() },new DateTimePeriod(dtp.StartDate.Date.Utc(),dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
		}


		[Test]
		public void ShouldCalculateDemandWithShrinkage()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 11);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), skillOpenHours, 10, 10, 200).WithId();
			skillDay.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> { skillDay.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(12);
			skillStaffIntervals.Count(x => x.AgentsWithShrinkage == 12.5).Should().Be.EqualTo(12);
			skillStaffIntervals.Count(x => x.Agents == 10).Should().Be.EqualTo(12);
		}

		[Test]
		public void ShouldCalculateDemandWithDifferentShrinkage()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), skillOpenHours, 10, 10, 200).WithId();
			skillDay.SkillDataPeriodCollection[32].Shrinkage = new Percent(0.2);
			skillDay.SkillDataPeriodCollection[33].Shrinkage = new Percent(0.2);
			skillDay.SkillDataPeriodCollection[35].Shrinkage = new Percent(0.5);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> { skillDay.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			skillStaffIntervals[0].Agents.Should().Be(10);
			skillStaffIntervals[0].AgentsWithShrinkage.Should().Be(12.5);

			skillStaffIntervals[1].Agents.Should().Be(10);
			skillStaffIntervals[1].AgentsWithShrinkage.Should().Be(12.5);

			skillStaffIntervals[2].Agents.Should().Be(10);
			skillStaffIntervals[2].AgentsWithShrinkage.Should().Be(10);

			skillStaffIntervals[3].Agents.Should().Be(10);
			skillStaffIntervals[3].AgentsWithShrinkage.Should().Be(20);
		}

		[Test]
		public void ShouldhaveIsBackofficeFlagSetOnEmailSkill()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var phoneSkillOpenHours = new TimePeriod(8, 9);
			var emailSkillOpenHours = new TimePeriod(7, 8);
			var phoneSkill = createSkill(15, "phone", phoneSkillOpenHours);
			var emailSkill = createSkillEmail(60, "email", emailSkillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var phoneSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(phoneSkill, scenario, new DateTime(2019, 2, 17), phoneSkillOpenHours, 15.7, 10, 200).WithId();
			var emailSkillDay = SkillSetupHelper.CreateSkillDayWithDemand(emailSkill, scenario, new DateTime(2019, 2, 17), emailSkillOpenHours, 5, 5, 30).WithId();
			SkillRepository.Add(phoneSkill);
			SkillDayRepository.AddRange(new[] { emailSkillDay, phoneSkillDay });
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> { phoneSkillDay.Id.GetValueOrDefault(), emailSkillDay.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(
				new[] { phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Date.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count(x => x.SkillId == phoneSkill.Id.GetValueOrDefault() && x.IsBackOffice == false)
				.Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.SkillId == emailSkill.Id.GetValueOrDefault() && x.IsBackOffice)
				.Should().Be.EqualTo(4);

		}

		[Test]
		public void ShouldFilterSkillDaysThatAreFarInPast()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 1, 16), new DateOnly(2019, 3, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay1 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 8), skillOpenHours, 10, 10, 200).WithId();
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 9), skillOpenHours, 5, 10, 200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange(new []{skillDay1, skillDay2 });
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> { skillDay1.Id.GetValueOrDefault(), skillDay2.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Agents == 5).Should().Be.EqualTo(4);

		}

		[Test, Ignore("WIP")]
		public void ShouldFilterSkillDaysThatAreFarInFuture()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 1, 16), new DateOnly(2019, 3, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay1 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 4, 16), skillOpenHours, 10, 10, 200).WithId();
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 4, 17), skillOpenHours, 5, 10, 200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange(new[] { skillDay1, skillDay2 });
			ScenarioRepository.Add(scenario);

			Target.Calculate(new List<Guid> { skillDay1.Id.GetValueOrDefault(), skillDay2.Id.GetValueOrDefault() });

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Agents == 10).Should().Be.EqualTo(4);

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

		protected ISkill createSkillEmail(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email))
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