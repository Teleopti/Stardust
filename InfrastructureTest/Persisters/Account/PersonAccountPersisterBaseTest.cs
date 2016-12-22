﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	[TestFixture]
	[DatabaseTest]
	public abstract class PersonAccountPersisterBaseTest
	{
		private IPersonAbsenceAccount personAbsenceAccount;
		protected IPersonAccountPersister Target { get; private set; }

		protected IList<IPersonAbsenceAccount> FetchPersonAccount()
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

		[SetUp]
		public void Setup()
		{
			setupEntities();
			makeTarget();
		}

		private void makeTarget()
		{
			var uowFactory = CurrentUnitOfWorkFactory.Make();
			var currUnitOfWork = new CurrentUnitOfWork(uowFactory);
			var rep = new PersonAbsenceAccountRepository(currUnitOfWork);
			var repositoryFactory = new RepositoryFactory();
			Target = new PersonAccountPersister(
				uowFactory,
				rep,
				new FakeInitiatorIdentifier(), 
				new PersonAccountConflictCollector(new DatabaseVersion(currUnitOfWork)),
				new PersonAccountConflictResolver(
					uowFactory,
					new TraceableRefreshService(
						new DefaultScenarioFromRepository(new ScenarioRepository(currUnitOfWork)),
						new ScheduleStorage(
							currUnitOfWork, 
							repositoryFactory, 
							new PersistableScheduleDataPermissionChecker(),
							new FalseToggleManager(), 
							new ScheduleStorageRepositoryWrapper(repositoryFactory, currUnitOfWork))),
					new PersonAbsenceAccountRepository(currUnitOfWork)));
		}

		private void setupEntities()
		{
			var absenceWithTracker = new Absence { Tracker = Tracker.CreateDayTracker(), Description = new Description("persist", "test")};
			var person = PersonFactory.CreatePerson("persist", "test");
			personAbsenceAccount = new PersonAbsenceAccount(person, absenceWithTracker);
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2000, 1, 1)));
			var defaultScenario = new Scenario("persist test") {DefaultScenario = true};
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = unitOfWork.FetchSession();
				session.Save(person);
				session.Save(absenceWithTracker);
				session.Save(personAbsenceAccount);
				session.Save(defaultScenario);
				unitOfWork.PersistAll();
			}
		}

	}
}