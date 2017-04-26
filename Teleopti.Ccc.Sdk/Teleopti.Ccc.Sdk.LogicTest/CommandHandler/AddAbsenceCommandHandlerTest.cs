using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
    public class AddAbsenceCommandHandlerTest
	{
		private static DateOnly _startDate = new DateOnly(2012, 1, 1);
        private readonly DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
        
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };
		
		[Test]
		public void AbsenceIsAddedSuccessfully()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate("Default",true);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absenceRepository = new FakeAbsenceRepository();
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var businessRulesForPersonalAccountUpdate =
				new BusinessRulesForPersonalAccountUpdate(new FakePersonAbsenceAccountRepository(),
					new SchedulingResultStateHolder());
			var scheduleTagRepository = new FakeScheduleTagRepository();
			var scheduleTagAssembler = new ScheduleTagAssembler(scheduleTagRepository);
			var scheduleSaveHandler =
				new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage),
					new FakePersonAbsenceAccountRepository(), new DoNothingScheduleDayChangeCallBack(), new EmptyScheduleDayDifferenceSaver()));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			absenceRepository.Add(absence);

			var scheduleTag = new ScheduleTag {Description = "test"}.WithId();
			scheduleTagRepository.Add(scheduleTag);
			
			var target = new AddAbsenceCommandHandler(dateTimePeriodAssembler, absenceRepository, scheduleStorage,
				personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
				businessRulesForPersonalAccountUpdate, scheduleTagAssembler, scheduleSaveHandler);
			
			var addAbsenceCommandDto = new AddAbsenceCommandDto
			{
				Period = _periodDto,
				AbsenceId = absence.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnlyDto {DateTime = _startDate.Date},
				ScheduleTagId = scheduleTag.Id
			};

			target.Handle(addAbsenceCommandDto);
			scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				_dateOnlyPeriod, scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.PersonAbsenceCollection()
				.Count.Should()
				.Be.EqualTo(1);
		}
		
		[Test]
		public void AbsenceIsAddedToAnotherScenarioSuccessfully()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate("Default", true).WithId();
			var otherScenario = ScenarioFactory.CreateScenarioAggregate("Test", false).WithId();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absenceRepository = new FakeAbsenceRepository();
			var scheduleStorage = new FakeScheduleStorage();
			var personRepository = new FakePersonRepositoryLegacy();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			scenarioRepository.Add(otherScenario);

			var businessRulesForPersonalAccountUpdate =
				new BusinessRulesForPersonalAccountUpdate(new FakePersonAbsenceAccountRepository(),
					new SchedulingResultStateHolder());
			var scheduleTagRepository = new FakeScheduleTagRepository();
			var scheduleTagAssembler = new ScheduleTagAssembler(scheduleTagRepository);
			var scheduleSaveHandler =
				new ScheduleSaveHandler(new SaveSchedulePartService(new FakeScheduleDifferenceSaver(scheduleStorage),
					new FakePersonAbsenceAccountRepository(), new DoNothingScheduleDayChangeCallBack(), new EmptyScheduleDayDifferenceSaver()));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			absenceRepository.Add(absence);

			var scheduleTag = new ScheduleTag { Description = "test" }.WithId();
			scheduleTagRepository.Add(scheduleTag);

			var target = new AddAbsenceCommandHandler(dateTimePeriodAssembler, absenceRepository, scheduleStorage,
				personRepository, scenarioRepository, new FakeCurrentUnitOfWorkFactory(),
				businessRulesForPersonalAccountUpdate, scheduleTagAssembler, scheduleSaveHandler);

			var addAbsenceCommandDto = new AddAbsenceCommandDto
			{
				Period = _periodDto,
				AbsenceId = absence.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnlyDto { DateTime = _startDate.Date },
				ScheduleTagId = scheduleTag.Id,
				ScenarioId = otherScenario.Id
			};

			target.Handle(addAbsenceCommandDto);
			scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				_dateOnlyPeriod, otherScenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.PersonAbsenceCollection()
				.Count.Should()
				.Be.EqualTo(1);
		}
    }
}
