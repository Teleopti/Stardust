using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class UpdateStaffingReadModelForecastTest : ISetup
	{
		public UpdateStaffingLevelReadModel Target;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
		}

		[Test]
		public void ShouldPerformResourceCalculation()
		{
			Now.Is("2016-12-19 00:00");
			var scenario = ScenarioRepository.Has("scenario");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var person = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));

			Target.Update(period);

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), period.StartDateTime, period.EndDateTime).ToList();
			staffing.Count.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldCalculateStaffing()
		{
			Now.Is("2016-12-19 00:00");
			var scenario = ScenarioRepository.Has("scenario");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var person = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));

			Target.Update(period);

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), period.StartDateTime, period.EndDateTime).ToList();
			staffing.First().StaffingLevel.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCalculateStaffingWithShrinkage()
		{
			Now.Is("2016-12-19 00:00");
			var scenario = ScenarioRepository.Has("scenario");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();

			var person = PersonRepository.Has(skill, skill2);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));
			var person2 = PersonRepository.Has(skill, skill2);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));
			var person3 = PersonRepository.Has(skill, skill2);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person3, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday);
			SkillDayRepository.Has(skill2.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));
			
			Target.Update(period);

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), period.StartDateTime, period.EndDateTime).ToList();
			staffing.First().StaffingLevelWithShrinkage.Should().Be.EqualTo(2);
			var staffing2 = ScheduleForecastSkillReadModelRepository.GetBySkill(skill2.Id.GetValueOrDefault(), period.StartDateTime, period.EndDateTime).ToList();
			staffing2.First().StaffingLevelWithShrinkage.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldCalculateForecast()
		{
			var dateTime = new DateTime(2016, 10, 03, 11, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioRepository.Has("scenario");


			AbsenceFactory.CreateAbsence("Holiday");
			ShiftCategoryRepository.Add(ShiftCategoryFactory.CreateShiftCategory("Perfect"));

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			PersonRepository.Has(skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(dateTime), 1.75));

			Target.Update(new DateTimePeriod(dateTime, dateTime.AddDays(1)));

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), dateTime, dateTime.AddMinutes(15)).ToList();
			staffing.Count.Should().Be.EqualTo(1);
			staffing.First().Forecast.Should().Be.EqualTo(1.75);
			staffing.First().FStaff.Should().Be.EqualTo(1.75);
		}

		[Test]
		public void ShouldCalculateForecastWithShrinkage()
		{
			var dateTime = new DateTime(2016, 10, 03, 11, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioRepository.Has("scenario");


			AbsenceFactory.CreateAbsence("Holiday");
			ShiftCategoryRepository.Add(ShiftCategoryFactory.CreateShiftCategory("Perfect"));

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			PersonRepository.Has(skill);

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(dateTime), 7);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday);

			Target.Update(new DateTimePeriod(dateTime, dateTime.AddDays(1)));

			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), dateTime, dateTime.AddMinutes(15)).ToList();
			staffing.Count.Should().Be.EqualTo(1);
			staffing.First().ForecastWithShrinkage.Should().Be.EqualTo(14);
		}

		[Test]
		public void ShouldCallPurge()
		{
			Now.Is("2016-12-19 00:00");
			var scenario = ScenarioRepository.Has("scenario");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 2);

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var person = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));
			
			Target.Update(period);
			ScheduleForecastSkillReadModelRepository.PurgeWasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldPurgeChangesOlderThanTimeWhenDataWasLoaded()
		{
			Now.Is("2016-12-19 00:00");
			var scenario = ScenarioRepository.Has("scenario");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 2);

			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var person = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));

			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange { StartDateTime = period.StartDateTime, EndDateTime = period.StartDateTime });

			Target.Update(period);
			var staffing = ScheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), period.StartDateTime, period.EndDateTime).ToList();
			staffing.First().StaffingLevel.Should().Be.EqualTo(1);
		}

	}
}