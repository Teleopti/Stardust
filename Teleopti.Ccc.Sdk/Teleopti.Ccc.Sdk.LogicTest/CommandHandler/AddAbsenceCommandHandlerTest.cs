using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	[DomainTest]
    public class AddAbsenceCommandHandlerTest : IIsolateSystem, IExtendSystem
	{
		public FakeScenarioRepository ScenarioRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeScheduleTagRepository ScheduleTagRepository;
		public IScheduleStorage ScheduleStorage;
		public AddAbsenceCommandHandler Target;

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
			var scenario = ScenarioRepository.LoadDefaultScenario();
			
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			AbsenceRepository.Add(absence);

			var scheduleTag = new ScheduleTag {Description = "test"}.WithId();
			ScheduleTagRepository.Add(scheduleTag);
			
			var addAbsenceCommandDto = new AddAbsenceCommandDto
			{
				Period = _periodDto,
				AbsenceId = absence.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnlyDto {DateTime = _startDate.Date},
				ScheduleTagId = scheduleTag.Id
			};

			Target.Handle(addAbsenceCommandDto);
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				_dateOnlyPeriod, scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.PersonAbsenceCollection()
				.Length.Should()
				.Be.EqualTo(1);
		}
		
		[Test]
		public void AbsenceIsAddedToAnotherScenarioSuccessfully()
		{
			ScenarioRepository.Has("Default");
			var otherScenario = ScenarioFactory.CreateScenarioAggregate("Test", false).WithId();
			
			ScenarioRepository.Has(otherScenario);
			
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			AbsenceRepository.Add(absence);

			var scheduleTag = new ScheduleTag { Description = "test" }.WithId();
			ScheduleTagRepository.Add(scheduleTag);
			
			var addAbsenceCommandDto = new AddAbsenceCommandDto
			{
				Period = _periodDto,
				AbsenceId = absence.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnlyDto { DateTime = _startDate.Date },
				ScheduleTagId = scheduleTag.Id,
				ScenarioId = otherScenario.Id
			};

			Target.Handle(addAbsenceCommandDto);
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				_dateOnlyPeriod, otherScenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.PersonAbsenceCollection()
				.Length.Should()
				.Be.EqualTo(1);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AddAbsenceCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleSaveHandler>().For<IScheduleSaveHandler>();
		}
	}
}
