using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class TerminatePersonHandlerTest
	{
		public MutableNow Now;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public TerminatePersonHandler Target;

		[Test]
		public void ShouldRemovePersonAssignmentAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));

			ScenarioRepository.Add(scenario);
			PersonRepository.Add(person);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 4, 8, 2018, 1, 4, 17)).WithId());
			
			Target.Handle(new PersonTerminalDateChangedEvent()
			{
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 1, 3)
			});

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2018, 1, 3, 2018, 1, 5), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17));
		}

		[Test]
		public void ShouldNotRemoveScheduleWhenClearingLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));

			ScenarioRepository.Add(scenario);
			PersonRepository.Add(person);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 1, 8, 2018, 1, 1, 17)).WithId());
			
			Target.Handle(new PersonTerminalDateChangedEvent()
			{
				PersonId = person.Id.Value
			});
			
			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2018, 1, 1, 2018, 1, 2), scenario)
				.Count
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveScheduleAfterLeavingDateInAllScenarios()
		{
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var scenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, defaultScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, defaultScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());

			PersonRepository.Add(person);
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(scenario);
			
			Target.Handle(new PersonTerminalDateChangedEvent()
			{
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});
			
			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28), defaultScenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));
		}


		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));
		}

		[Test]
		public void ShouldNotRemoveAbsenceSpanningOverLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be.EqualTo(new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17));
		}

		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDateInAllScenarios()
		{
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var otherScenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, defaultScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, defaultScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());

			PersonRepository.Add(person);
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(otherScenario);

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), defaultScenario)
				.Count
				.Should().Be.EqualTo(1);
			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), otherScenario)
				.Count
				.Should().Be.EqualTo(1);
		}
	}
}