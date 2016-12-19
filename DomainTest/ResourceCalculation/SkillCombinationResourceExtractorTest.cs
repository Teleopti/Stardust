using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest, Toggle(Toggles.Wfm_Requests_ImproveStaffingForCascadingSkills_41969)]
	public class SkillCombinationResourceExtractorTest : ISetup
	{
		public UpdateStaffingLevelReadModel Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<IUpdateStaffingLevelReadModel>();
		}

		[Test]
		public void ShouldSpecifyRourceSkillCombinationOnInterval()
		{
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");
			
			var saleSkill =  SkillRepository.Has("sales", activity);
			var supportSkill= SkillRepository.Has("support", activity);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] {saleSkill, supportSkill}).WithId();

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity,person,period,ShiftCategoryFactory.CreateShiftCategory(),scenario));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			
			PersonRepository.Has(person);
			Target.Update(period);

			var persistedCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime,period.StartDateTime.AddMinutes(15)));
			persistedCombinations.Count().Should().Be.EqualTo(1);
			persistedCombinations.First().SkillCombination.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSpecifyResourceForMoreThanOneInterval()
		{
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var activityEmail = ActivityFactory.CreateActivity("Email");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity);
			var supportSkill = SkillRepository.Has("support", activityEmail);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, period, ShiftCategoryFactory.CreateShiftCategory(), scenario));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person2, period, ShiftCategoryFactory.CreateShiftCategory(), scenario));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person3, period, ShiftCategoryFactory.CreateShiftCategory(), scenario));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));

			PersonRepository.Has(person);
			PersonRepository.Has(person2);
			PersonRepository.Has(person3);
			Target.Update(period);

			var persistedCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinations.Count().Should().Be.EqualTo(1);
			persistedCombinations.First().SkillCombination.Count().Should().Be.EqualTo(2);
			persistedCombinations.First().Resource.Should().Be.EqualTo(3);
		}

		//[Test]
		//public void ShouldSpecifySkillCombinationOnIntervalWithShrinkage()
		//{
		//	var activity = ActivityFactory.CreateActivity("phone");
		//	activity.RequiresSkill = true;
		//	var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

		//	var scenario = ScenarioRepository.Has("default");

		//	var saleSkill = SkillRepository.Has("sales", activity);
		//	var supportSkill = SkillRepository.Has("support", activity);
		//	var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();

		//	PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, period, ShiftCategoryFactory.CreateShiftCategory(), scenario));


		//	var saleSkillDay = saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1);
		//	saleSkillDay.SkillDataPeriodCollection.ForEach(skillDataPeriod =>
		//	{
		//		skillDataPeriod.Shrinkage = new Percent(0.5);
		//	});
		//	SkillDayRepository.Has(saleSkillDay);
		//	SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

		//	PersonRepository.Has(person);
		//	Target.Update(period);

		//	var persistedCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
		//	persistedCombinations.Count().Should().Be.EqualTo(1);
		//	persistedCombinations.First().SkillCombination.Count().Should().Be.EqualTo(2);
		//	persistedCombinations.First().ResourceWithShrinkage.Should().Be.EqualTo(1);
		//}

	}
	
}