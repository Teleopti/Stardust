using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAccount
{
	[TestFixture, DomainTest]
	public class PersonEmploymentChangedTest
	{
		public CalculatePersonAccount Target;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public ScheduleStorage ScheduleStorage;

		[Test]
		public void ShouldCalculatePersonAccount()
		{
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			DateOnly fromDate = new DateOnly(2018,03,01);
			var person = PersonFactory.CreatePersonWithGuid("me", "you");
			PersonRepository.Has(person);
			var absence = new Absence();
			absence.Tracker = Tracker.CreateTimeTracker();
			absence.InContractTime = true;
			AbsenceRepository.Has(absence);

			var absAcc = new PersonAbsenceAccount(person, AbsenceRepository.LoadAll().First());
			var acc = new AccountTime(fromDate);
			acc.Accrued = TimeSpan.FromHours(20);
			absAcc.Add(acc);
			PersonAbsenceAccountRepository.Add(absAcc);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 3, 1, 11, 2018, 3, 1, 20));
			PersonAssignmentRepository.Has(assignment);
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2018, 03, 01, 11, 2018, 03, 1, 20));
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			Target.Calculate(new PersonEmploymentChangedEvent {FromDate = fromDate, PersonId = person.Id.GetValueOrDefault()});
			//acc.CalculateUsed(ScheduleStorage,scenario);
			//acc.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(11));
		}
	}


}
