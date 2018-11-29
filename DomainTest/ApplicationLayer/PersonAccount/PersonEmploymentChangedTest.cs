using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


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
			acc.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(9));
			acc.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(11));
		}

		[Test]
		public void ShouldCalculatePersonAccountWithAbsenceOnEmptyDay()
		{
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			DateOnly fromDate = new DateOnly(2018, 03, 01);
			var person = PersonFactory.CreatePersonWithPersonPeriod(fromDate).WithId();
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
			
			var fullDayAbsence = new AbsenceLayer(absence,
				new DateTimePeriod(new DateTime(2018, 03, 01, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 03, 1, 23, 59, 0, DateTimeKind.Utc)));
			var personAbsence = new PersonAbsence(person, scenario, fullDayAbsence);
			PersonAbsenceRepository.Has(personAbsence);

			Target.Calculate(new PersonEmploymentChangedEvent { FromDate = fromDate, PersonId = person.Id.GetValueOrDefault() });
			acc.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(8));
			acc.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(12));
		}

		[Test]
		public void ShouldCalculatePersonAccountWithPartTimePercentage50()
		{
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			DateOnly fromDate = new DateOnly(2018, 03, 01);
			var person = PersonFactory.CreatePersonWithPersonPeriod(fromDate).WithId();
			PersonRepository.Has(person);
			var absence = new Absence();
			absence.Tracker = Tracker.CreateTimeTracker();
			absence.InContractTime = true;
			AbsenceRepository.Has(absence);

			var absAcc = new PersonAbsenceAccount(person, AbsenceRepository.LoadAll().First());
			var acc = new AccountTime(fromDate);
			acc.Accrued = TimeSpan.FromHours(20);
			acc.LatestCalculatedBalance = TimeSpan.FromHours(8);
			absAcc.Add(acc);
			PersonAbsenceAccountRepository.Add(absAcc);

			var fullDayAbsence = new AbsenceLayer(absence,
				new DateTimePeriod(new DateTime(2018, 03, 01, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 03, 1, 23, 59, 0, DateTimeKind.Utc)));
			var personAbsence = new PersonAbsence(person, scenario, fullDayAbsence);
			PersonAbsenceRepository.Has(personAbsence);

			person.PersonPeriodCollection.First().PersonContract.PartTimePercentage.Percentage = new Percent(0.5);

			Target.Calculate(new PersonEmploymentChangedEvent { FromDate = fromDate, PersonId = person.Id.GetValueOrDefault() });
			acc.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(4));
			acc.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(16));
		}

		[Test]
		public void ShouldCalculatePersonAccountWithTwoPersonAccountperiod()
		{
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			DateOnly fromDate = new DateOnly(2018, 03, 01);
			var previousFromDate = new DateOnly(2018, 01, 01);
			var person = PersonFactory.CreatePersonWithPersonPeriod(previousFromDate).WithId();
			PersonRepository.Has(person);
			var absence = new Absence();
			absence.Tracker = Tracker.CreateTimeTracker();
			absence.InContractTime = true;
			AbsenceRepository.Has(absence);

			var personAcc = new PersonAbsenceAccount(person, AbsenceRepository.LoadAll().First());
			var acc1 = new AccountTime(previousFromDate);
			acc1.Accrued = TimeSpan.FromHours(20);
			
			var acc2 = new AccountTime(fromDate);
			acc2.Accrued = TimeSpan.FromHours(30);

			personAcc.Add(acc1);
			personAcc.Add(acc2);

			PersonAbsenceAccountRepository.Add(personAcc);

			var previousAbsence = new AbsenceLayer(absence,
				new DateTimePeriod(new DateTime(2018, 01, 01, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 01, 1, 23, 59, 0, DateTimeKind.Utc)));
			var previousPersonAbsence = new PersonAbsence(person, scenario, previousAbsence);
			PersonAbsenceRepository.Has(previousPersonAbsence);

			var fullDayAbsence = new AbsenceLayer(absence,
				new DateTimePeriod(new DateTime(2018, 03, 01, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 03, 1, 23, 59, 0, DateTimeKind.Utc)));
			var nextPersonAbsence = new PersonAbsence(person, scenario, fullDayAbsence);
			PersonAbsenceRepository.Has(nextPersonAbsence);

			person.PersonPeriodCollection.First().PersonContract.PartTimePercentage.Percentage = new Percent(0.5);

			Target.Calculate(new PersonEmploymentChangedEvent { FromDate = previousFromDate, PersonId = person.Id.GetValueOrDefault() });

			acc1.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(4));
			acc1.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(16));

			acc2.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(4));
			acc2.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(26));
		}
		
	}

}
