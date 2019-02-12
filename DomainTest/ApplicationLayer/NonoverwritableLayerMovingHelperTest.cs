using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[NoDefaultData]
	public class NonOverwritableLayerMovingHelperTest : IIsolateSystem
	{
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository CurrentScenario;
		public IScheduleStorage ScheduleStorage;
		public NonoverwritableLayerMovingHelper Target;

		[Test]
		public void MoveShiftLayerToLocationThatWouldCauseTheTargetToBeOverlappedByOtherLayerShouldNotBeValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");
			var emailActivity = ActivityFactory.CreateActivity("Email");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			emailActivity.AllowOverwrite = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);

			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));
			pa.AddActivity(emailActivity,new DateTimePeriod(2013,11,14,14,2013,11,14,15));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.IsDestinationValidForMovedShiftLayer(scheduleDay,layerToMove,TimeSpan.FromHours(2));
			result.Should().Be.False();
		}


		[Test]
		public void MoveShiftLayerToLocationCausingNewOverlappedNonoverwritableLayerShouldNotBeValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			shortBreakActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);

			pa.AddActivity(shortBreakActivity,new DateTimePeriod(2013,11,14,14,2013,11,14,15));
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.IsDestinationValidForMovedShiftLayer(scheduleDay,layerToMove,TimeSpan.FromHours(2));
			result.Should().Be.False();
		}

		[Test]
		public void MoveShiftLayerToLocationWithoutCausingNewOverlappedNonoverwritableLayerShouldBeValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			shortBreakActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);

			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));
			pa.AddActivity(shortBreakActivity,new DateTimePeriod(2013,11,14,16,2013,11,14,17));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.IsDestinationValidForMovedShiftLayer(scheduleDay,layerToMove,TimeSpan.FromHours(2));
			result.Should().Be.True();
		}

		[Test]
		public void MoveShiftLayerToLocationLaterTheMainShiftShouldNotBeValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16), shiftCategory);

			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.IsDestinationValidForMovedShiftLayer(scheduleDay,layerToMove,TimeSpan.FromHours(4));
			result.Should().Be.False();
		}

		[Test]
		public void MoveShiftLayerToLocationEarlierTheMainShiftShouldNotBeValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16), shiftCategory);

			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.IsDestinationValidForMovedShiftLayer(scheduleDay,layerToMove,TimeSpan.FromHours(-4));
			result.Should().Be.False();
		}

		[Test]		
		public void ShouldGetCorrectDistanceForMovingToBeforeIfTheDistanceIsShorterAndTheLocationIsValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16), shiftCategory);

			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013,11,14);
			var scheduleDay = getScheduleDay(date,person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.GetMovingDistance(scheduleDay, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 15),
				layerToMove.Id.Value);

			result.Should().Be.EqualTo(TimeSpan.FromHours(-3));
		}
		
		[Test]
		public void ShouldMoveToAfterIfTheDistanceIsShorterAndTheLocationIsValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var scheduleDay = getScheduleDay(date, person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.GetMovingDistance(scheduleDay, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 15),
				layerToMove.Id.Value);

			result.Should().Be.EqualTo(TimeSpan.FromHours(3));
		}
		
		[Test]
		public void ShouldMoveToBeforeIfMoveToAfterIsNotValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var scheduleDay = getScheduleDay(date, person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.GetMovingDistance(scheduleDay, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 15),
				layerToMove.Id.Value);

			result.Should().Be.EqualTo(TimeSpan.FromHours(-4));
		}

		[Test]
		public void ShouldMoveToAfterIfMoveToBeforeIsNotValid()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 9, 2013, 11, 14, 18), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var scheduleDay = getScheduleDay(date, person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.GetMovingDistance(scheduleDay, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 15),
				layerToMove.Id.Value);

			result.Should().Be.EqualTo(TimeSpan.FromHours(4));
		}

		[Test]
		public void ShouldNotMoveLayerIfTheLayerIsAlreadyOutsideOfGivenPeriod()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 14));

			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var date = new DateOnly(2013, 11, 14);
			var scheduleDay = getScheduleDay(date, person);

			var layerToMove = pa.ShiftLayers.First(l => l.Payload == lunchActivity);

			var result = Target.GetMovingDistance(scheduleDay, new DateTimePeriod(2013, 11, 11, 10, 2013, 11, 14, 13),
				layerToMove.Id.Value);

			result.Should().Be.EqualTo(TimeSpan.Zero);
		}

		private IScheduleDictionary getScheduleDictionary(DateOnly date,IPerson person)
		{
			var period = new DateOnlyPeriod(date,date).Inflate(1);
			var schedules = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false,false),
				period,
				CurrentScenario.LoadDefaultScenario());
			return schedules;
		}

		private IScheduleDay getScheduleDay(DateOnly date,IPerson person)
		{
			var schedules = getScheduleDictionary(date,person);
			return schedules[person].ScheduledDay(date);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<NonoverwritableLayerMovingHelper>().For<INonoverwritableLayerMovingHelper>();
			isolate.UseTestDouble<NonoverwritableLayerChecker>().For<INonoverwritableLayerChecker>();
		}
	}
}
