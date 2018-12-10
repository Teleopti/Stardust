using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
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
    public class CancelAbsenceCommandHandlerTest : IIsolateSystem, IExtendSystem
    {
        public IScheduleStorage ScheduleStorage;
        public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
        public CancelAbsenceCommandHandler Target;
		
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto
        {
            UtcStartTime = _startDate,
            UtcEndTime = _startDate.AddDays(1)
        };
		
	    [Test]
	    public void AbsenceCancelSuccessfully()
	    {
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);
			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			AbsenceRepository.Has(absence);
			var scenario = ScenarioRepository.Has("Default");

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period).WithId());
		    PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period)).WithId());

		    Target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = person.Id.GetValueOrDefault() });

		    ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
			    new ScheduleDictionaryLoadOptions(false, false), _period,
			    scenario)[person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Length.Should().Be.EqualTo(0);
	    }

	    [Test]
	    public void AbsenceCancelSuccessfullyForGivenType()
	    {
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);
			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			var absence2 = AbsenceFactory.CreateAbsence("Holiday").WithId();
			AbsenceRepository.Has(absence);
			AbsenceRepository.Has(absence2);
			var scenario = ScenarioRepository.Has("Default");
			
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period)).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period)).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence2, _period)).WithId());
			
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				scenario)[person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Length.Should().Be.EqualTo(3);
		    Target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = person.Id.GetValueOrDefault(),AbsenceId = absence.Id});

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				scenario)[person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Length.Should().Be.EqualTo(1);
	    }

	    [Test]
        public void AbsenceCancelSuccessfullyForGivenScenario()
        {
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);
			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			AbsenceRepository.Has(absence);

			var scenario = ScenarioRepository.Has("Default");
			var otherScenario = ScenarioFactory.CreateScenarioWithId("other",false);
			ScenarioRepository.Add(otherScenario);
			
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period)).WithId());

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, otherScenario, _period).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, otherScenario, new AbsenceLayer(absence, _period)).WithId());
			
			Target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = person.Id.GetValueOrDefault(),ScenarioId = otherScenario.Id.GetValueOrDefault()});

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), _period,
				otherScenario)[person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Length.Should().Be.EqualTo(0);
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), _period,
					scenario)[person].ScheduledDay(new DateOnly(_startDate)).PersonAbsenceCollection().Length.Should().Be.EqualTo(1);
		}

		[Test]
		public void AbsenceCancelSuccessfullyForTwoOverlappingAbsences()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var absence = AbsenceFactory.CreateAbsence("Sick").WithId();
			AbsenceRepository.Has(absence);

			var scenario = ScenarioRepository.Has("Default");

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period.ChangeEndTime(TimeSpan.FromDays(2)))).WithId());
			PersonAbsenceRepository.Add(new PersonAbsence(person, scenario, new AbsenceLayer(absence, _period.ChangeEndTime(TimeSpan.FromDays(4)))).WithId());
			_periodDto.UtcEndTime = _period.EndDateTime.Add(TimeSpan.FromDays(2));
			Target.Handle(new CancelAbsenceCommandDto { Period = _periodDto, PersonId = person.Id.GetValueOrDefault() });

			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), _period.ChangeEndTime(TimeSpan.FromDays(3)),
					scenario)[person].ScheduledDay(new DateOnly(_startDate).AddDays(3)).PersonAbsenceCollection(true).Length.Should().Be.EqualTo(1);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<CancelAbsenceCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleSaveHandler>().For<IScheduleSaveHandler>();
		}
	}
}
