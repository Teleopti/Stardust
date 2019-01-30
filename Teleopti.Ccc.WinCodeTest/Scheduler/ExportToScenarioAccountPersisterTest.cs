using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ExportToScenarioAccountPersisterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var mock = new MockRepository();
			var personAccountPersister = mock.StrictMock<IPersonAccountPersister>();
			var uowFactory = mock.StrictMock<IUnitOfWorkFactory>();
			var uow = mock.DynamicMock<IUnitOfWork>();
			var person = new Person();
			var persons = new List<IPerson> {person};
			var dateOnly = new DateOnly(2016, 1, 1);
			var absence = new Absence();
			var allPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var involvedAbsences = new Dictionary<IPerson, HashSet<IAbsence>>();
			var personAccountCollection = new PersonAccountCollection(person);
			var accountDay = mock.StrictMock<IAccount>();
			var scenario = ScenarioFactory.CreateScenario("scenario", true, false);
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);

			var target = new ExportToScenarioAccountPersister(personAccountPersister);

			using (mock.Record())
			{
				Expect.Call(personAccountPersister.Persist(new List<IPersonAbsenceAccount>(), null)).IgnoreArguments().Return(true);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(accountDay.Period()).Return(new DateOnlyPeriod(dateOnly, dateOnly));
				Expect.Call(accountDay.Parent).Return(personAbsenceAccount);
				Expect.Call(() => accountDay.CalculateUsed(null, scenario)).IgnoreArguments();
				Expect.Call(() => accountDay.SetParent(personAbsenceAccount)).IgnoreArguments();
			}

			using (mock.Playback())
			{
				involvedAbsences.Add(person, new HashSet<IAbsence> { absence });
				personAccountCollection.Add(absence, accountDay);
				allPersonAccounts.Add(person, personAccountCollection);
				var result = target.Persist(scenario, uowFactory, persons, allPersonAccounts, involvedAbsences, new List<DateOnly> { dateOnly });
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotPersistWhenDayToExportIsNotTrackedAbsence()
		{
			var mock = new MockRepository();
			var personAccountPersister = mock.StrictMock<IPersonAccountPersister>();
			var uowFactory = mock.StrictMock<IUnitOfWorkFactory>();
			var uow = mock.DynamicMock<IUnitOfWork>();
			var person = new Person();
			var persons = new List<IPerson> { person };
			var dateOnly = new DateOnly(2016, 1, 1);
			var absence = new Absence();
			var otherAbsence = new Absence();
			var allPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var involvedAbsences = new Dictionary<IPerson, HashSet<IAbsence>>();
			var personAccountCollection = new PersonAccountCollection(person);
			var accountDay = mock.StrictMock<IAccount>();
			var scenario = ScenarioFactory.CreateScenario("scenario", true, false);
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);

			var target = new ExportToScenarioAccountPersister(personAccountPersister);

			using (mock.Record())
			{
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(accountDay.Period()).Return(new DateOnlyPeriod(dateOnly, dateOnly));
				Expect.Call(accountDay.Parent).Return(personAbsenceAccount);
				Expect.Call(() => accountDay.SetParent(personAbsenceAccount)).IgnoreArguments();
			}

			using (mock.Playback())
			{
				involvedAbsences.Add(person, new HashSet<IAbsence> { otherAbsence });
				personAccountCollection.Add(absence, accountDay);
				allPersonAccounts.Add(person, personAccountCollection);
				var result = target.Persist(scenario, uowFactory, persons, allPersonAccounts, involvedAbsences, new List<DateOnly> { dateOnly });
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldNotPersistOnNonDefaultScenarios()
		{
			var mock = new MockRepository();
			var personAccountPersister = mock.StrictMock<IPersonAccountPersister>();
			var scenario = ScenarioFactory.CreateScenario("scenario", false, false);
			var target = new ExportToScenarioAccountPersister(personAccountPersister);
			var result = target.Persist(scenario, null, null, null, null, null);
			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldNotPersistWhenNoPersonAccount()
		{
			var mock = new MockRepository();
			var personAccountPersister = mock.StrictMock<IPersonAccountPersister>();
			var uowFactory = mock.StrictMock<IUnitOfWorkFactory>();
			var person = new Person();
			var persons = new List<IPerson> { person };
			var allPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var scenario = ScenarioFactory.CreateScenario("scenario", true, false);
			
			var target = new ExportToScenarioAccountPersister(personAccountPersister);
			var result = target.Persist(scenario, uowFactory, persons, allPersonAccounts, null, null);
			Assert.IsFalse(result);
		}
	}
}
