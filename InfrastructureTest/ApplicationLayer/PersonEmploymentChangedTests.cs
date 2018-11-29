using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	
	[InfrastructureTest]
	public class PersonEmploymentChangedTests 
	{
		public IBusinessUnitRepository BusinessUnitRepository;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public PersonEmploymentChangedEventHandler Target;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public WithUnitOfWork WithUnitOfWork;

		
		[Test]
		public void ShouldCalculateAndPersistPersonAccount()
		{
			var person = PersonFactory.CreatePerson("me", "you");
			DateOnly fromDate = new DateOnly(2018, 03, 01);
			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("1");
			WithUnitOfWork.Do(() =>
			{
				
				BusinessUnitRepository.Add(bu);

				var scenario = ScenarioFactory.CreateScenario("dummy", true, false);
				scenario.SetBusinessUnit(bu);
				ScenarioRepository.Add(scenario);
				
				PersonRepository.Add(person);

				var absence = AbsenceFactory.CreateAbsence("holiday");
				absence.Tracker = Tracker.CreateTimeTracker();
				absence.InContractTime = true;
				AbsenceRepository.Add(absence);

				var absAcc = new PersonAbsenceAccount(person, absence);
				var acc = new AccountTime(fromDate);
				acc.Accrued = TimeSpan.FromHours(20);
				absAcc.Add(acc);
				PersonAbsenceAccountRepository.Add(absAcc);

				var activity = new Activity("for test");
				activity.SetBusinessUnit(bu);
				ActivityRepository.Add(activity);

				var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("DAy");
				shiftCategory.SetBusinessUnit(bu);
				ShiftCategoryRepository.Add(shiftCategory);

				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2018, 3, 1, 11, 2018, 3, 1, 20), shiftCategory);
				PersonAssignmentRepository.Add(assignment);
				var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2018, 03, 01, 11, 2018, 03, 1, 20));
				IPersonAbsence personAbsence = new PersonAbsence(person, scenario, absenceLayer);
				((PersonAbsenceRepository)PersonAbsenceRepository).Add(personAbsence);
			});
			
			Target.Handle(new PersonEmploymentChangedEvent { FromDate = fromDate, PersonId = person.Id.GetValueOrDefault(),LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),LogOnDatasource = "TestData"});

			WithUnitOfWork.Do(() =>
			{
				var account = PersonAbsenceAccountRepository.Find(person).AllPersonAccounts().First();
				account.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromHours(9));
				account.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(11));
			});
		}
	}
}
