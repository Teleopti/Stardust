using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTestWithStaticDependenciesAvoidUse]
	public class NonoverwritableLayerMovabilityCheckerTest : ISetup
	{
		public FakePersonRepository PersonRepository;
		public FakeCurrentScenario_DoNotUse CurrentScenario;
		public FakeScheduleStorage_DoNotUse ScheduleStorage;
		public NonoverwritableLayerMovabilityChecker Target;

		[Test]
		public void ShouldDetectThereIsNonoverwritableLayerInGivenPeriod()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(x => x.WithId());

			ScheduleStorage.Add(pa);

			var scheduleDay = getScheduleDay(new DateOnly(2013, 11, 14), person);

			var result = Target.HasNonoverwritableLayer(scheduleDay, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 14), shortBreakActivity);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldDetectThereIsNoNonoverwritableLayerInGivenPeriod()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(x => x.WithId());

			ScheduleStorage.Add(pa);

			var scheduleDay = getScheduleDay(new DateOnly(2013, 11, 14), person);

			var result = Target.HasNonoverwritableLayer(scheduleDay, new DateTimePeriod(2013, 11, 14, 15, 2013, 11, 14, 16), shortBreakActivity);

			result.Should().Be.False();
		}

		[Test]
		public void ShiftWithConflictingProjectedActivityFromMultipleNonoverwritableShiftLayersShouldNotBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 14));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13), person, date);

			result.Should().Be.False();
		}

		[Test]
		public void ShiftWithConflictingProjectedActivityFromSingleVisualNonoverwritableLayerShouldBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13), person, date);

			result.Should().Be.True();
		}

		[Test]
		public void ShiftWithConflictingProjectedActivityFromMultipleNonoverwritableVisualLayersShouldNotBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 14, 2013, 11, 14, 15));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 16), person, date);

			result.Should().Be.False();
		}

		[Test]
		public void ShiftWithConflictingProjectedActivityFromMultipleNonoverwritableVisualLayersButSingleShiftLayerShouldNotBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 15));
			pa.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 14));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 16), person, date);

			result.Should().Be.False();
		}

		[Test]
		public void ShiftWithSingleConflictProjectedActivityButWithIlegitimateScheduleInTheNewLayerPeriodShouldNotBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			pa.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 15));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 16), person, date);

			result.Should().Be.False();
		}

		[Test]
		public void ShiftWithNoConflictProjectedActivityButWithIlegitimateScheduleOutSideOfTheNewLayerPeriodShouldNotBeFixable()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13));
			pa.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));

			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var schedules = getScheduleDictionary(date, person);

			var result = Target.IsFixableByMovingNonoverwritableLayer(schedules,
				new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 16), person, date);

			result.Should().Be.False();
		}



		private IScheduleDictionary getScheduleDictionary(DateOnly date, IPerson person)
		{
			var period = new DateOnlyPeriod(date, date).Inflate(1);
			var schedules = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				CurrentScenario.Current());
			return schedules;
		}

		private IScheduleDay getScheduleDay(DateOnly date, IPerson person)
		{
			var schedules = getScheduleDictionary(date, person);
			return schedules[person].ScheduledDay(date);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeCurrentScenario_DoNotUse>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage_DoNotUse>().For<IScheduleStorage>();
			system.UseTestDouble<NonoverwritableLayerMovabilityChecker>().For<INonoverwritableLayerMovabilityChecker>();
		}
	}
}
