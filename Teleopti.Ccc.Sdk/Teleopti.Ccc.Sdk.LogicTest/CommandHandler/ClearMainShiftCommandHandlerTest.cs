using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class ClearMainShiftCommandHandlerTest
    {
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        
	    [Test]
	    public void ClearMainShiftFromTheDictionarySuccessfully()
	    {
			var scenario = ScenarioFactory.CreateScenarioAggregate("Default", true).WithId();
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var accountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleTagRepository = new FakeScheduleTagRepository();
			
			var person = PersonFactory.CreatePerson("test").WithId();
			personRepository.Add(person);

			scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period));

		    var target = new ClearMainShiftCommandHandler(new ScheduleTagAssembler(scheduleTagRepository), scheduleStorage,
			    personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
			    new BusinessRulesForPersonalAccountUpdate(accountRepository, new FakeSchedulingResultStateHolder()),
			    new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage, new EmptyScheduleDayDifferenceSaver()),
				    accountRepository, new DoNothingScheduleDayChangeCallBack())));
			target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault() });

		    scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false,false),
			    new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
			    .PersonAssignment()
			    .MainActivities()
			    .Should()
			    .Be.Empty();
	    }

		[Test]
		public void ClearMainShiftShouldNotTouchOvertime()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate("Default", true).WithId();
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var accountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleTagRepository = new FakeScheduleTagRepository();

			var person = PersonFactory.CreatePerson("test").WithId();
			personRepository.Add(person);

			scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, _period));

			var target = new ClearMainShiftCommandHandler(new ScheduleTagAssembler(scheduleTagRepository), scheduleStorage,
				personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
				new BusinessRulesForPersonalAccountUpdate(accountRepository, new FakeSchedulingResultStateHolder()),
				new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage, new EmptyScheduleDayDifferenceSaver()),
					accountRepository, new DoNothingScheduleDayChangeCallBack())));
			target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault() });

			var scheduledDay = scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1));

			scheduledDay
				.PersonAssignment()
				.MainActivities()
				.Should()
				.Be.Empty();
			scheduledDay
				.PersonAssignment()
				.OvertimeActivities()
				.Should().Not.Be.Empty();
		}
		[Test]
	    public void ClearMainShiftFromTheDictionaryForGivenScenarioSuccessfully()
	    {
			var newScenario = ScenarioFactory.CreateScenarioWithId("High", false);
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(newScenario);
			var accountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleTagRepository = new FakeScheduleTagRepository();

			var person = PersonFactory.CreatePerson("test").WithId();
			personRepository.Add(person);

			scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, newScenario, _period));

			var target = new ClearMainShiftCommandHandler(new ScheduleTagAssembler(scheduleTagRepository), scheduleStorage,
				personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
				new BusinessRulesForPersonalAccountUpdate(accountRepository, new FakeSchedulingResultStateHolder()),
				new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage, new EmptyScheduleDayDifferenceSaver()),
					accountRepository, new DoNothingScheduleDayChangeCallBack())));
			target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault(), ScenarioId = newScenario.Id.GetValueOrDefault() });
			
			scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), newScenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.PersonAssignment()
				.MainActivities()
				.Should()
				.Be.Empty();
		}

		[Test]
		public void ClearMainShiftWithScheduleTag()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate("Default", true).WithId();
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var accountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleTagRepository = new FakeScheduleTagRepository();

			var scheduleTag = new ScheduleTag { Description = "Manual" }.WithId();
			scheduleTagRepository.Add(scheduleTag);

			var person = PersonFactory.CreatePerson("test").WithId();
			personRepository.Add(person);

			scheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period));

			var target = new ClearMainShiftCommandHandler(new ScheduleTagAssembler(scheduleTagRepository), scheduleStorage,
				personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
				new BusinessRulesForPersonalAccountUpdate(accountRepository, new FakeSchedulingResultStateHolder()),
				new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage, new EmptyScheduleDayDifferenceSaver()),
					accountRepository, new DoNothingScheduleDayChangeCallBack())));
			target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault(), ScheduleTagId = scheduleTag.Id.GetValueOrDefault() });
			
			scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.ScheduleTag()
				.Should()
				.Be.EqualTo(scheduleTag);
		}
    }
}
