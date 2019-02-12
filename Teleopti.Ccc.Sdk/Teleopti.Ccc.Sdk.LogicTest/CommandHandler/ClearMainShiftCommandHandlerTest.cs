using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
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
    [NoDefaultData]
	[DomainTest]
    public class ClearMainShiftCommandHandlerTest : IIsolateSystem, IExtendSystem
	{
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeScheduleTagRepository ScheduleTagRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public ClearMainShiftCommandHandler Target;

        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
        
	    [Test]
	    public void ClearMainShiftFromTheDictionarySuccessfully()
	    {
			var scenario = ScenarioRepository.Has("Default");
			
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period).WithId());
			
			Target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault() });

		    PersonAssignmentRepository.LoadAll().Single()
			    .MainActivities()
			    .Should()
			    .Be.Empty();
	    }

		[Test]
		public void ClearMainShiftShouldNotTouchOvertime()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, _period).WithId());

			Target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault() });

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
			personAssignment
				.MainActivities()
				.Should()
				.Be.Empty();
			personAssignment
				.OvertimeActivities()
				.Should().Not.Be.Empty();
		}
		[Test]
	    public void ClearMainShiftFromTheDictionaryForGivenScenarioSuccessfully()
		{
			ScenarioRepository.Has("Default");
			var newScenario = ScenarioFactory.CreateScenarioWithId("High", false);
			ScenarioRepository.Has(newScenario);

			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);
			
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, newScenario, _period).WithId());

			Target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault(), ScenarioId = newScenario.Id.GetValueOrDefault() });

			PersonAssignmentRepository.LoadAll().Single()
				.MainActivities()
				.Should()
				.Be.Empty();
		}

		[Test]
		public void ClearMainShiftWithScheduleTag()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			var scheduleTag = new ScheduleTag { Description = "Manual" }.WithId();
			ScheduleTagRepository.Add(scheduleTag);
			
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period));

			Target.Handle(new ClearMainShiftCommandDto { Date = _dateOnlydto, PersonId = person.Id.GetValueOrDefault(), ScheduleTagId = scheduleTag.Id.GetValueOrDefault() });
			
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), scenario)[person].ScheduledDay(new DateOnly(2012, 1, 1))
				.ScheduleTag()
				.Should()
				.Be.EqualTo(scheduleTag);
		}	
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ClearMainShiftCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleSaveHandler>().For<IScheduleSaveHandler>();
		}
	}
}

