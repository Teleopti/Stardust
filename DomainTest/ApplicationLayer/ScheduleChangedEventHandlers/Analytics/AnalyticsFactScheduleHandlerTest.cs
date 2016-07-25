using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactScheduleHandlerTest
	{
		IAnalyticsFactScheduleHandler _target;
		private IIntervalLengthFetcher _intervalLengthFetcher;
		private IAnalyticsFactScheduleDateHandler _dateHandler;
		private IAnalyticsFactScheduleTimeHandler _timeHandler;
		private IScheduleStorage _scheduleStorage;
		private IScenarioRepository _scenarioRepository;
		private IPersonRepository _personRepository;
		private readonly Guid absenceId = Guid.NewGuid();

		[SetUp]
		public void Setup()
		{
			_intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_timeHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleTimeHandler>();
			_scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_target = new AnalyticsFactScheduleHandler(_intervalLengthFetcher, _dateHandler, _timeHandler, _scheduleStorage, _scenarioRepository,_personRepository);
		}

		[Test]
		public void ShouldReturnEmptyListIfNoShift()
		{
			var result = _target.AgentDaySchedule(new ProjectionChangedEventScheduleDay(), null, DateTime.Now, 1, 1, Guid.NewGuid(), Guid.NewGuid());
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetIntervalLength()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = new DateTime(),
					EndDateTime = new DateTime()
				}
			};
			_target.AgentDaySchedule(scheduleDay, null, DateTime.Now, 1, 1, Guid.NewGuid(), Guid.NewGuid());

			_intervalLengthFetcher.AssertWasCalled(x => x.IntervalLength);
		}

		[Test]
		public void ShouldReturnCorrectSchemaWithBrokenInterval() // PBI 37562
		{
			var shiftStart = new DateTime(2015, 1, 1, 8, 10, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);

			_dateHandler.Stub(x => x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
				Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(new AnalyticsFactScheduleDate());

			var repoMock = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			var analyticsActivityMock = MockRepository.GenerateMock<IAnalyticsActivityRepository>();
			var overtimeRepository = MockRepository.GenerateMock<IAnalyticsOvertimeRepository>();
			_timeHandler = new AnalyticsFactScheduleTimeHandler(repoMock, new FakeAnalyticsAbsenceRepository(), overtimeRepository, analyticsActivityMock);
			overtimeRepository.Stub(x => x.Overtimes()).Return(new List<AnalyticsOvertime>());

			repoMock.Stub(x => x.ShiftLengths()).Return(new List<IAnalyticsShiftLength>());
			analyticsActivityMock.Stub(x => x.Activities()).Return(new List<AnalyticsActivity>());

			_target = new AnalyticsFactScheduleHandler(_intervalLengthFetcher, _dateHandler, _timeHandler, _scheduleStorage, _scenarioRepository, _personRepository);

			var result = _target.AgentDaySchedule(scheduleDay, null, DateTime.Now, 1, 1, Guid.NewGuid(), Guid.NewGuid());

			result.First().TimePart.ScheduledMinutes.Should().Be.EqualTo(5);
			result.Last().TimePart.ScheduledMinutes.Should().Be.EqualTo(10);
			result.Sum(a => a.TimePart.ScheduledMinutes).Should().Be.EqualTo(60);
			result.Count.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldReturnNullWhenDateCouldNotBeHandled()
		{
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(null);

			var result = _target.AgentDaySchedule(scheduleDay, null, DateTime.Now, 1, 1, Guid.NewGuid(), Guid.NewGuid());
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGatherPartsForFactScheduleRow()
		{
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			var datePart = new AnalyticsFactScheduleDate();
			var timePart = new AnalyticsFactScheduleTime();
			var personPart = new AnalyticsFactSchedulePerson();

			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);

			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(datePart);

			_timeHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything,
						Arg<int>.Is.Anything)).Return(timePart);

			var result = _target.AgentDaySchedule(scheduleDay, personPart, DateTime.Now, 1, 1, Guid.NewGuid(), Guid.NewGuid());

			result.Count.Should().Be.EqualTo(4);
			result[0].DatePart.Should().Be.SameInstanceAs(datePart);
			result[0].TimePart.Should().Be.SameInstanceAs(timePart);
			result[0].PersonPart.Should().Be.SameInstanceAs(personPart);
		}

		[Test]
		public void ShouldGetTimeCorrectly()
		{
			var scenarioCode = Guid.NewGuid();
			var personCode = Guid.NewGuid();
			var person = PersonFactory.CreatePerson("test1");
			person.SetId(personCode);
			_personRepository.Stub(x => x.Get(personCode)).Return(person);
			var scenario = ScenarioFactory.CreateScenario("scenario1", true, true);
			_scenarioRepository.Stub(x => x.Get(scenarioCode)).Return(scenario);
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var day = MockRepository.GenerateMock<IScheduleDay>();
			var projectionService = MockRepository.GenerateMock<IProjectionService>();
			var visualLayerCollection = MockRepository.GenerateMock<IVisualLayerCollection>();
			visualLayerCollection.Stub(x => x.ContractTime(new DateTimePeriod(shiftStart, shiftStart.AddMinutes(15))))
				.Return(TimeSpan.FromMinutes(6));
			visualLayerCollection.Stub(x => x.WorkTime(new DateTimePeriod(shiftStart, shiftStart.AddMinutes(15))))
				.Return(TimeSpan.FromMinutes(7));
			visualLayerCollection.Stub(x => x.Overtime(new DateTimePeriod(shiftStart, shiftStart.AddMinutes(15))))
				.Return(TimeSpan.FromMinutes(8));
			visualLayerCollection.Stub(x => x.PaidTime(new DateTimePeriod(shiftStart, shiftStart.AddMinutes(15))))
				.Return(TimeSpan.FromMinutes(9));

			projectionService.Stub(x => x.CreateProjection()).Return(visualLayerCollection);
			day.Stub(x => x.ProjectionService()).Return(projectionService);
			scheduleDictionary.Stub(x => x.SchedulesForDay(new DateOnly(scheduleDay.Date)))
				.Return(new [] { day });
			_scheduleStorage.Stub(
				x =>
					x.FindSchedulesForPersonOnlyInGivenPeriod(Arg<IPerson>.Matches(y => y.Id == person.Id),
						Arg<IScheduleDictionaryLoadOptions>.Matches(y => y.LoadRestrictions == false && y.LoadNotes == false),
						Arg<DateOnlyPeriod>.Matches(
							y => y.StartDate == new DateOnly(scheduleDay.Date) && y.EndDate == new DateOnly(scheduleDay.Date)),
						Arg<IScenario>.Matches(y => y.Id == scenario.Id)))
				.Return(scheduleDictionary);

			var datePart = new AnalyticsFactScheduleDate();
			var personPart = new AnalyticsFactSchedulePerson();

			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);

			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(datePart);

			var repoMock = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			var analyticsActivityMock = MockRepository.GenerateMock<IAnalyticsActivityRepository>();
			var overtimeRepository = MockRepository.GenerateMock<IAnalyticsOvertimeRepository>();
			var fakeAnalyticsAbsenceRepository = new FakeAnalyticsAbsenceRepository();
			fakeAnalyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absenceId
			});
			_timeHandler = new AnalyticsFactScheduleTimeHandler(repoMock, fakeAnalyticsAbsenceRepository, overtimeRepository, analyticsActivityMock);
			overtimeRepository.Stub(x => x.Overtimes()).Return(new List<AnalyticsOvertime>());

			repoMock.Stub(x => x.ShiftLengths()).Return(new List<IAnalyticsShiftLength>());
			analyticsActivityMock.Stub(x => x.Activities()).Return(new List<AnalyticsActivity>());

			_target = new AnalyticsFactScheduleHandler(_intervalLengthFetcher, _dateHandler, _timeHandler, _scheduleStorage, _scenarioRepository, _personRepository);

			var result = _target.AgentDaySchedule(scheduleDay, personPart, DateTime.Now, 1, 1, scenarioCode, personCode);

			result.Count.Should().Be.EqualTo(4);
			result[0].DatePart.Should().Be.SameInstanceAs(datePart);
			result[0].PersonPart.Should().Be.SameInstanceAs(personPart);
			result[0].TimePart.ContractTimeMinutes.Should().Be.EqualTo(6);
			result[0].TimePart.WorkTimeMinutes.Should().Be.EqualTo(7);
			result[0].TimePart.OverTimeMinutes.Should().Be.EqualTo(8);
			result[0].TimePart.PaidTimeMinutes.Should().Be.EqualTo(9);
		}


		private IEnumerable<ProjectionChangedEventLayer> createLayers(DateTime startOfShift, IEnumerable<int> lengthCollection)
		{
			int accStart = 0;
			var layerList = new List<ProjectionChangedEventLayer>();
			

			foreach (int length in lengthCollection)
			{
				layerList.Add(
					new ProjectionChangedEventLayer
					{
						StartDateTime = startOfShift.AddMinutes(accStart),
						EndDateTime = startOfShift.AddMinutes(accStart).AddMinutes(length),
						ContractTime = new TimeSpan(0, 0, length),
						WorkTime = new TimeSpan(0, 0, length),
						DisplayColor = Color.Brown.ToArgb(),
						IsAbsence = true,
						IsAbsenceConfidential = true,
						Name = "Jonas",
						ShortName = "JN",
						PayloadId = absenceId
					}
					);
				accStart += length;
			}
			return layerList;
		}
	}
}
