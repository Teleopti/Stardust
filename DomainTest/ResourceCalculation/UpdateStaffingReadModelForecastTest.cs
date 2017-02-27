using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
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

	}
}