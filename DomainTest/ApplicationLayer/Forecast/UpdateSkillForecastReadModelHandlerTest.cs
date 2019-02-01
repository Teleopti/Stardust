using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class UpdateSkillForecastReadModelHandlerTest
	{
		public UpdateSkillForecastReadModelHandler Target;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public ISkillForecastReadModelRepository SkillForecastReadModelRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeTenants Tenants;
		public FakeSystemJobStartTimeRepository SystemJobStartTimeRepository;

		[Test]
		public void VerifyIfTheForecastChangedEventIsHandled()
		{

			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);
			
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			scenario.SetBusinessUnit(bu);
			scenario.DefaultScenario = true;
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7, 10, 200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Handle(new ForecastChangedEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				SkillDayIds = new []{skillDay.Id.GetValueOrDefault()}
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);

		}

		[Test]
		public void VerifyIfTheUpdateSkillForecastReadmodelEventIsHandled()
		{

			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);

			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			scenario.SetBusinessUnit(bu);
			scenario.DefaultScenario = true;
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7, 10, 200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Handle(new UpdateSkillForecastReadModelEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				StartDateTime = new DateTime(2019,02,17),
				EndDateTime = new DateTime(2019, 02, 17)
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);

		}


		[Test]
		public void ShouldFilterSkillDaysThatAreFarInFuture()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);

			var dtp = new DateOnlyPeriod(new DateOnly(2019, 1, 16), new DateOnly(2019, 7, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay1 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 6, 16), skillOpenHours, 10, 10, 200).WithId();
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 6, 17), skillOpenHours, 5, 10, 200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange(new[] { skillDay1, skillDay2 });
			ScenarioRepository.Add(scenario);

			Target.Handle(new ForecastChangedEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				SkillDayIds = new[] { skillDay1.Id.GetValueOrDefault(),skillDay2.Id.GetValueOrDefault() }
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(0);

		}

		[Test]
		public void ShouldFilterSkillDaysThatAreFarInPast()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);

			var dtp = new DateOnlyPeriod(new DateOnly(2019, 1, 16), new DateOnly(2019, 3, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay1 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 8), skillOpenHours, 10, 10, 200).WithId();
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 9), skillOpenHours, 5, 10, 200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange(new[] { skillDay1, skillDay2 });
			ScenarioRepository.Add(scenario);

			Target.Handle(new ForecastChangedEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				SkillDayIds = new[] { skillDay1.Id.GetValueOrDefault(), skillDay2.Id.GetValueOrDefault() }
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Agents == 5).Should().Be.EqualTo(4);

		}


		[Test]
		public void ShouldFilterSkillDaysThatAreInFuture()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);

			var dtp = new DateOnlyPeriod(new DateOnly(2019, 1, 16), new DateOnly(2019, 4, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			var skillDay1 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 4, 16), skillOpenHours, 10, 10, 200).WithId();
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 4, 17), skillOpenHours, 5, 10, 200).WithId();
			SkillRepository.Add(skill);
			SkillDayRepository.AddRange(new[] { skillDay1, skillDay2 });
			ScenarioRepository.Add(scenario);

			Target.Handle(new ForecastChangedEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				SkillDayIds = new[] { skillDay1.Id.GetValueOrDefault(), skillDay2.Id.GetValueOrDefault() }
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Agents == 10).Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldUpdateLastCalculatedTimeOnCalculation()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);

			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			scenario.SetBusinessUnit(bu);
			scenario.DefaultScenario = true;
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7, 10, 200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			var oldUpdatedTime = Now.UtcDateTime().AddDays(-9);
			SystemJobStartTimeRepository.EntryList.Add(new FakeStartTimeModel()
			{
				StartedAt = oldUpdatedTime,
				BusinessUnit = bu.Id.GetValueOrDefault(),
				JobName = JobNamesForJoStartTime.TriggerSkillForecastReadModel
			});

			Target.Handle(new UpdateSkillForecastReadModelEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				StartDateTime = new DateTime(2019, 02, 17),
				EndDateTime = new DateTime(2019, 02, 17)
			});

			SystemJobStartTimeRepository.GetLastCalculatedTime(bu.Id.GetValueOrDefault(), JobNamesForJoStartTime.TriggerSkillForecastReadModel)
				.Should().Be.GreaterThan(oldUpdatedTime);
		}



		protected ISkill createPhoneSkill(int intervalLength, string skillName, TimePeriod openHours)
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