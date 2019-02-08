using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[Toggle(Toggles.WfmTeamSchedule_SuggestShiftCategory_152)]
	[NoDefaultData]
	public class AddActivityCommandHandlerWithBetterShiftCategorySelectionTest : IIsolateSystem
	{
		public AddActivityCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeShiftCategorySelectionRepository ShiftCategorySelectionRepository;
		public IPredictShiftCategory PredictShiftCategory;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AddActivityCommandHandler>().For<IHandleCommand<AddActivityCommand>>();
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
		}

		[Test]
		public void ShouldSetShiftCategoryForActivityAddedOnDayOff()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var alphabeticalFirstShiftCategory = ShiftCategoryFactory.CreateShiftCategory("AAA").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day").WithId();
			ShiftCategoryRepository.Add(alphabeticalFirstShiftCategory);
			ShiftCategoryRepository.Add(shiftCategory);

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			PersonAssignmentRepository.Add(dayOff);

			var trainedModel = PredictShiftCategory.Train(new[]
				{new ShiftCategoryExample {StartTime = 14.0, EndTime = 15.0, ShiftCategory = shiftCategory.Id.ToString()}});
			using (var ms = new MemoryStream())
			{
				trainedModel.Store(ms);
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				{
					ShiftCategorySelectionRepository.Add(new ShiftCategorySelection { Model = reader.ReadToEnd()});
				}
			}

			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldSetShiftCategoryForActivityAddedOnDayOffWithOffModel()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var alphabeticalFirstShiftCategory = ShiftCategoryFactory.CreateShiftCategory("AAA").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day").WithId();
			ShiftCategoryRepository.Add(alphabeticalFirstShiftCategory);
			ShiftCategoryRepository.Add(shiftCategory);

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			PersonAssignmentRepository.Add(dayOff);

			var trainedModel = PredictShiftCategory.Train(new[]
			{
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Monday, StartTime = 22, EndTime = 23, ShiftCategory = shiftCategory.Id.ToString()},
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Monday, StartTime = 21, EndTime = 23.5, ShiftCategory = shiftCategory.Id.ToString()},
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Monday, StartTime = 20, EndTime = 23.25, ShiftCategory = shiftCategory.Id.ToString()},
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Monday, StartTime = 22, EndTime = 23.5, ShiftCategory = shiftCategory.Id.ToString()},
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Tuesday, StartTime = 21, EndTime = 23.25, ShiftCategory = alphabeticalFirstShiftCategory.Id.ToString()},
				new ShiftCategoryExample{DayOfWeek = DayOfWeek.Monday, StartTime = 20, EndTime = 23, ShiftCategory = shiftCategory.Id.ToString()},
			});
			using (var ms = new MemoryStream())
			{
				trainedModel.Store(ms);
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				{
					ShiftCategorySelectionRepository.Add(new ShiftCategorySelection { Model = reader.ReadToEnd() });
				}
			}

			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(alphabeticalFirstShiftCategory);
		}

		[Test]
		public void ShouldSetShiftCategoryForActivityAddedOnDayOffWithEmptyModel()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var alphabeticalFirstShiftCategory = ShiftCategoryFactory.CreateShiftCategory("AAA").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day").WithId();
			ShiftCategoryRepository.Add(alphabeticalFirstShiftCategory);
			ShiftCategoryRepository.Add(shiftCategory);

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			PersonAssignmentRepository.Add(dayOff);

			ShiftCategorySelectionRepository.Add(new ShiftCategorySelection {Model = string.Empty});
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(alphabeticalFirstShiftCategory);
		}

		[Test]
		public void ShouldSetShiftCategoryForActivityAddedOnDayOffWithNoModel()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var alphabeticalFirstShiftCategory = ShiftCategoryFactory.CreateShiftCategory("AAA").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day").WithId();
			ShiftCategoryRepository.Add(alphabeticalFirstShiftCategory);
			ShiftCategoryRepository.Add(shiftCategory);

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			PersonAssignmentRepository.Add(dayOff);

			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(alphabeticalFirstShiftCategory);
		}
	}
}