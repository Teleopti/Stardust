using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PersonLeavingScheduleTest
	{
		public MutableNow Now;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldRemoveScheduleAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default",true,false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, new DateTimePeriod(2001,1,1,8,2001,1,1,17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, new DateTimePeriod(2001,2,1,8,2001,2,1,17)).WithId());
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001,1,31), new PersonAccountUpdaterDummy(), new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Count().Should()
				.Be.EqualTo(1);
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
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(), new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Count().Should()
				.Be.EqualTo(1);
		}
	}

	[TestFixture]
	[DomainTest]
	public class PersonLeavingAbsenceTest
	{
		public MutableNow Now;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person,scenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(), new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDateInAllScenarios()
		{
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var otherScenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(otherScenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(), new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), otherScenario)
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotRemoveAbsenceSpanningOverLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest); 
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17)).WithId());
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(), new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single()
				.Period.Should()
				.Be.EqualTo(new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17));
		}
	}

	[TestFixture]
	[DomainTest]
	public class PersonLeavingPeriodsTest
	{
		public MutableNow Now;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldRemovePersonPeriodStartingAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001,2,2)));
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(),
				new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));
			person.ActivatePerson(new PersonAccountUpdaterDummy());

			person.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldRemoveSchedulePeriodStartingAfterLeavingDate()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2001, 2, 2)));
			ScenarioRepository.Add(scenario);

			person.TerminatePerson(new DateOnly(2001, 1, 31), new PersonAccountUpdaterDummy(),
				new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));
			person.ActivatePerson(new PersonAccountUpdaterDummy());

			person.PersonSchedulePeriodCollection.Count.Should().Be.EqualTo(1);
		}
	}
}