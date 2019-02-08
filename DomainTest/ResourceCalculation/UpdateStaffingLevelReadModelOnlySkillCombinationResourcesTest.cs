using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	[NoDefaultData]
	public class UpdateStaffingLevelReadModelOnlySkillCombinationResourcesTest
	{
		public IUpdateStaffingLevelReadModel Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public MutableNow Now;

		[Test]
		public void ShouldSpecifyResourceSkillCombinationOnInterval()
		{
			Now.Is("2016-12-19 00:00");
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity);
			var supportSkill = SkillRepository.Has("support", activity);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));

			PersonRepository.Has(person);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinationResources.Count().Should().Be.EqualTo(1);
			persistedCombinationResources.First().SkillCombination.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSpecifyResourceSkillCombinationOnWithCascadingSkills()
		{
			Now.Is("2016-12-19 00:00");
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity);
			saleSkill.SetCascadingIndex(1);
			var supportSkill = SkillRepository.Has("support", activity);
			supportSkill.SetCascadingIndex(2);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));

			PersonRepository.Has(person);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinationResources.Count().Should().Be.EqualTo(1);
			persistedCombinationResources.First().SkillCombination.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSpecifyResourceSkillCombinationOnWithCascadingSkillsWhenSecondaryIsOpenOutsidePrimary()
		{
			Now.Is("2016-12-19 00:00");
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity);
			saleSkill.SetCascadingIndex(1);
			var supportSkill = SkillRepository.Has("support", activity);
			supportSkill.SetCascadingIndex(2);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 20), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));

			PersonRepository.Has(person);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinationResources.Count().Should().Be.EqualTo(1);
			persistedCombinationResources.First().SkillCombination.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSpecifyResourceForMoreThanOneInterval()
		{
			Now.Is("2016-12-19 00:00");
			var activity = ActivityFactory.CreateActivity("phone");
			activity.RequiresSkill = true;
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity).DefaultResolution(15);
			var supportSkill = SkillRepository.Has("support", activity);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			var person3 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, supportSkill }).WithId();
			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);
			person2.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);
			person3.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person3, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory()));

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));
			SkillDayRepository.Has(supportSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 0));

			PersonRepository.Has(person);
			PersonRepository.Has(person2);
			PersonRepository.Has(person3);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
			persistedCombinationResources.Count().Should().Be.EqualTo(4);
			persistedCombinationResources.First().SkillCombination.Count().Should().Be.EqualTo(2);
			persistedCombinationResources.First().Resource.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldIgnoreToPersistResourceIfSkillCombinationIsAnEmptyKey()
		{
			Now.Is("2016-12-19 00:00");
			var activity = ActivityFactory.CreateActivity("phone");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);

			var scenario = ScenarioRepository.Has("default");

			var saleSkill = SkillRepository.Has("sales", activity);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new ISkill[] { }).WithId();
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill }).WithId();

			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);
			person2.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			var ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass);
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			PersonRepository.Has(person);
			PersonRepository.Has(person2);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinationResources.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddSkillsMatchingScheduledActivityOnly()
		{
			Now.Is("2016-12-19 00:00");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);
			var scenario = ScenarioRepository.Has("default");

			var activity = ActivityFactory.CreateActivity("phone");
			var activity2 = ActivityFactory.CreateActivity("email");

			var saleSkill = SkillRepository.Has("sales", activity);
			var buySkill = SkillRepository.Has("buy", activity2);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill, buySkill }).WithId();

			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass);

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));
			SkillDayRepository.Has(buySkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			PersonRepository.Has(person);
			Target.Update(period);

			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)));
			persistedCombinationResources.Single().SkillCombination.Count().Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldIgnoreResourcesThatDoesNotHaveAnActivityForItsSkill()
		{
			Now.Is("2016-12-19 00:00");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);
			var scenario = ScenarioRepository.Has("default");

			var activity = ActivityFactory.CreateActivity("phone");
			var activity2 = ActivityFactory.CreateActivity("email");

			var saleSkill = SkillRepository.Has("sales", activity);
			var buySkill = SkillRepository.Has("buy", activity2);
			saleSkill.DefaultResolution = buySkill.DefaultResolution = 60;

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { saleSkill }).WithId();
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { buySkill }).WithId();

			person.PermissionInformation.SetDefaultTimeZone(saleSkill.TimeZone);
			person2.PermissionInformation.SetDefaultTimeZone(buySkill.TimeZone);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass);
			var ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(saleSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));
			SkillDayRepository.Has(buySkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			PersonRepository.Has(person);
			PersonRepository.Has(person2);
			Target.Update(period);
			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			persistedCombinationResources.Count().Should().Be.EqualTo(1);
			persistedCombinationResources.First().Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDistributeResourceEvenlyToCompleteInterval()
		{
			Now.Is("2016-12-19 00:00");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);
			var scenario = ScenarioRepository.Has("default");

			var activity = ActivityFactory.CreateActivity("phone");
			var activity2 = ActivityFactory.CreateActivity("email");

			var phoneSkill = SkillRepository.Has("sales", activity);
			var emailSkill = SkillRepository.Has("buy", activity2);
			phoneSkill.DefaultResolution = 15;
			emailSkill.DefaultResolution = 60;

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { phoneSkill }).WithId();
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { emailSkill }).WithId();

			person.PermissionInformation.SetDefaultTimeZone(phoneSkill.TimeZone);
			person2.PermissionInformation.SetDefaultTimeZone(emailSkill.TimeZone);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass);
			var ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity2, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));
			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			PersonRepository.Has(person);
			PersonRepository.Has(person2);
			Target.Update(period);
			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			persistedCombinationResources.Count().Should().Be.EqualTo(5);
			persistedCombinationResources
				.First(x => x.SkillCombination.NonSequenceEquals(new[] { emailSkill.Id.GetValueOrDefault() }))
				.Resource.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldSpecifyResourceSkillCombinationForMultisiteSkill()
		{
			Now.Is("2016-12-19 00:00");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);
			var scenario = ScenarioRepository.Has("default");

			var activity = ActivityFactory.CreateActivity("phone");

			var multiSkill = SkillRepository.HasMultisiteSkill("MultiSkillen", activity);
			multiSkill.DefaultResolution = 15;
			var childSkill = multiSkill.ChildSkills[0];
			childSkill.DefaultResolution = 15;

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 12, 19), new[] { childSkill }).WithId();

			person.PermissionInformation.SetDefaultTimeZone(childSkill.TimeZone);
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period, ShiftCategoryFactory.CreateShiftCategory());
			PersonAssignmentRepository.Has(ass);

			SkillDayRepository.Has(multiSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			PersonRepository.Has(person);
			Target.Update(period);
			var persistedCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			persistedCombinationResources.Count().Should().Be.EqualTo(4);
		}
		
		[Test]
		public void ShouldSkipBusinessUnitIfDefaultScenarioIsMissing()
		{
			Now.Is("2016-12-19 00:00");
			var period = new DateTimePeriod(2016, 12, 19, 0, 2016, 12, 19, 1);
			
			Assert.DoesNotThrow(() => Target.Update(period));
		}
	}
}
