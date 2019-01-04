using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.AbsenceHandler
{
	[TestFixture,TeamScheduleTest]
	public class AbsencePersisterNoMockTest : IIsolateSystem
	{
		public AbsencePersister Target;
		public FakePersonAssignmentRepository PersonAssignmentRepo;
		public FakePersonRepository PersonRepo;
		public FakeScenarioRepository ScenarioRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeSaveSchedulePartService ScheduleSaver;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSaveSchedulePartService>().For<ISaveSchedulePartService>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IProxyForId<IAbsence>>();
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<AbsencePersister>().For<IAbsencePersister>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
		}

		[Test]
		public void ShouldAbsenceBeAddedThroughCurrentDayScheduleWhenCurrentDayHasOvernightShiftAndTheIntradayAbsIsAddedToTheOvernightPart()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenario("default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId());
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();
			var shiftPeriod = new DateTimePeriod(2019, 1, 3, 20, 2019, 1, 4, 5);

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var overNightShift =
				PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, shiftPeriod);
			foreach (var shiftLayer in overNightShift.ShiftLayers)
			{
				shiftLayer.WithId();
			}
			PersonAssignmentRepo.Has(overNightShift);

			Target.PersistIntradayAbsence(new AddIntradayAbsenceCommand
			{
				AbsenceId = abs.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 4, 4, 0, 0),
				EndTime = new DateTime(2019, 1, 4, 5, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo {TrackId = Guid.NewGuid()}
			});

			ScheduleSaver.SavedScheduleDays().Single().DateOnlyAsPeriod.DateOnly.Should().Be.EqualTo(new DateOnly(2019, 1, 3));
		}

		[Test]
		public void ShouldAbsenceBeAddedThroughPeriodStartDayScheduleWhenPeriodHasNoIntersectionWithPreviousDayShift()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenario("default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId());
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			Target.PersistIntradayAbsence(new AddIntradayAbsenceCommand
			{
				AbsenceId = abs.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 4, 4, 0, 0),
				EndTime = new DateTime(2019, 1, 4, 5, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			});

			ScheduleSaver.SavedScheduleDays().Single().DateOnlyAsPeriod.DateOnly.Should().Be.EqualTo(new DateOnly(2019, 1, 4));
		}

	}

	public class FakeSaveSchedulePartService : ISaveSchedulePartService
	{
		private readonly IList<IScheduleDay> _saveScheduleDays = new List<IScheduleDay>();

		public IList<IScheduleDay> SavedScheduleDays()
		{
			return _saveScheduleDays;
		}
		public IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{

			_saveScheduleDays.Add(scheduleDay);
			return null;
		}

		public IList<string> Save(IEnumerable<IScheduleDay> scheduleDays, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			throw new NotImplementedException();
		}
	}
}