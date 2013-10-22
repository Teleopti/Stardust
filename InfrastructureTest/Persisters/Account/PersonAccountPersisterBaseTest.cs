using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	[TestFixture]
	public abstract class PersonAccountPersisterBaseTest : DatabaseTestWithoutTransaction
	{
		private IPersonAbsenceAccount personAbsenceAccount;
		private IPersonAccountPersister target;

		[Test]
		public void DoTheTest()
		{
			var theirAccounts = fetchPersonAccount();
			var myAccounts = fetchPersonAccount();

			if(GivenOtherHasChanged(theirAccounts.Single()))
			{
				target.Persist(theirAccounts);
				theirAccounts.Should().Be.Empty();				
			}

			var myAccount = myAccounts.Single();
			WhenImChanging(myAccount);
			target.Persist(myAccounts);
			myAccounts.Should().Be.Empty();

			Then(myAccount);
			Then(fetchPersonAccount().Single());
		}

		private IList<IPersonAbsenceAccount> fetchPersonAccount()
		{
			using(var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonAbsenceAccountRepository(uow);
				var accounts = rep.Find(personAbsenceAccount.Person).ToList();
				//needed because deep inside account stuff references are DI:ed instead of services
				foreach (var absenceAccount in accounts)
				{
					LazyLoadingManager.Initialize(absenceAccount.Absence);
					LazyLoadingManager.Initialize(absenceAccount.Person.PersonSchedulePeriodCollection);
				}
				return accounts;
			}
		}

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			setupEntities();
			makeTarget();
		}

		private void makeTarget()
		{
			var currUnitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var uowFactory = UnitOfWorkFactory.Current;
			var rep = new PersonAbsenceAccountRepository(currUnitOfWork);
			target = new PersonAccountPersister(uowFactory, 
																	rep, 
																	MockRepository.GenerateMock<IMessageBrokerIdentifier>(),
																	new PersonAccountConflictCollector(uowFactory),
																	new PersonAccountConflictResolver(
																		uowFactory,
																		new TraceableRefreshService(new DefaultScenarioFromRepository(new ScenarioRepository(currUnitOfWork)), 
																		new ScheduleRepository(currUnitOfWork)), 
																		new PersonAbsenceAccountRepository(currUnitOfWork)));
		}

		protected abstract bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount);
		protected abstract void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount);
		protected abstract void Then(IPersonAbsenceAccount myPersonAbsenceAccount);

		private void setupEntities()
		{
			var absenceWithTracker = new Absence { Tracker = Tracker.CreateDayTracker(), Description = new Description("persist", "test")};
			var person = PersonFactory.CreatePerson("persist", "test");
			personAbsenceAccount = new PersonAbsenceAccount(person, absenceWithTracker);
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2000, 1, 1)));
			var defaultScenario = new Scenario("persist test") {DefaultScenario = true};
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new Repository(unitOfWork);
				rep.Add(person);
				rep.Add(absenceWithTracker);
				rep.Add(personAbsenceAccount);
				rep.Add(defaultScenario);
				unitOfWork.PersistAll();
			}
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PersonAbsenceAccountRepository(unitOfWork);
				var account = repository.Get(personAbsenceAccount.Id.Value);
				repository.Remove(account.Absence);
				repository.Remove(account.Person);
				repository.Remove(account);
				repository.Remove(account);
				var scenarioRep = new ScenarioRepository(unitOfWork);
				foreach (var scenario in scenarioRep.LoadAll())
				{
					repository.Remove(scenario);
				}
				unitOfWork.PersistAll();
			}
		}
	}
}