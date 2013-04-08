﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters 
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public abstract class ScheduleScreenPersisterIntegrationTest : DatabaseTestWithoutTransaction, IOwnMessageQueue, IReassociateData
	{
		private IClearReferredShiftTradeRequests _clearReferredShiftTradeRequests;
		private IMessageBrokerModule _messageBrokerModule;
		private IPersonAbsenceAccountValidator _personAbsenceAccountValidator;

		protected IScheduleDictionaryConflictCollector ScheduleDictionaryConflictCollector { get; set; }
		protected ScheduleRepository ScheduleRepository { get; set; }

		protected IScheduleDictionarySaver ScheduleDictionarySaver { get; set; }
		protected IPersonAbsenceAccount PersonAbsenceAccount { get; set; }
		protected IScenario Scenario { get; set; }
		protected IPerson Person { get; set; }
		protected IPersistableScheduleData ScheduleData { get; set; }
		protected IScheduleDictionary ScheduleDictionary { get; private set; }
		protected IAbsence Absence { get; private set; }
		protected DateOnly AccountDate { get; set; }
		protected IAccount Account { get; set; }
		protected DateTime ScheduleStartDate { get; set; }
		protected DateTime ScheduleEndDate { get; set; }
		protected ScheduleDateTimePeriod ScheduleDateTimePeriod { get; set; }
		protected DateTimePeriod DateTimePeriod { get; set; }
		protected DateOnly FirstDayDateOnly { get; set; }
		protected DateTimePeriod FirstDayDateTimePeriod { get; set; }
		protected ICollection<IPersonWriteProtectionInfo> PersonWriteProtectionInfoCollection { get; private set; }


		private ScheduleScreenRetryingPersister Target { get; set; }

		private void SetupDependencies()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(unitOfWork.DatabaseVersion(PersonAbsenceAccount), Is.EqualTo(1));
			}

			_clearReferredShiftTradeRequests = Mocks.DynamicMock<IClearReferredShiftTradeRequests>();
			_messageBrokerModule = Mocks.DynamicMock<IMessageBrokerModule>();
			_personAbsenceAccountValidator = Mocks.DynamicMock<IPersonAbsenceAccountValidator>();
			ScheduleRepository = new ScheduleRepository(UnitOfWorkFactory.Current);
			ScheduleDictionaryConflictCollector = Mocks.DynamicMock<IScheduleDictionaryConflictCollector>();
			ScheduleDictionarySaver = new ScheduleDictionarySaver();
			Mocks.ReplayAll();
		}


		protected void MakeTarget()
		{
			Target = new ScheduleScreenRetryingPersister(UnitOfWorkFactory.LoggedOnProvider(),
													   new WriteProtectionRepository(UnitOfWorkFactory.Current),
													   //ScheduleRepository,
													   new PersonRequestRepository(UnitOfWorkFactory.Current),
													   new PersonAbsenceAccountRepository(UnitOfWorkFactory.Current),
													   //ScheduleDictionarySaver,
													   new PersonRequestPersister(_clearReferredShiftTradeRequests),
													   new PersonAbsenceAccountConflictCollector(),
													   new PersonAbsenceAccountRefresher(new RepositoryFactory(), Scenario),
													   _personAbsenceAccountValidator,
													   ScheduleDictionaryConflictCollector,
													   //new ScheduleDictionaryModifiedCallback(),
													   _messageBrokerModule,
													   new ScheduleDictionaryBatchPersister(
														   UnitOfWorkFactory.LoggedOnProvider(),
														   ScheduleRepository,
														   ScheduleDictionarySaver,
														   new DifferenceEntityCollectionService<IPersistableScheduleData>(),
														   _messageBrokerModule, 
														   this, 
														   new ScheduleDictionaryModifiedCallback()), 
													   this);
		}


		[SetUp]
		public void ScheduleScreenPersisterIntegrationTestSetup()
		{
			SetupEntities();
			SetupDatabase();
			SetupScheduleDictionary();
			SetupDependencies();
		}

		private void SetupDatabase()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(unitOfWork).Add(Person);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new PersonAbsenceAccountRepository(unitOfWork).Add(PersonAbsenceAccount);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				new ScheduleRepository(unitOfWork).Add(ScheduleData);

				unitOfWork.PersistAll();
			}
		}

		private void SetupEntities() 
		{
			Person = PersonFactory.CreatePerson("person", "one");
			Absence = AbsenceFactory.CreateAbsence("absence");
			Absence.Tracker = Tracker.CreateDayTracker();
			PersonAbsenceAccount = new PersonAbsenceAccount(Person, Absence);
			PersonWriteProtectionInfoCollection = new List<IPersonWriteProtectionInfo>();
			AccountDate = DateOnly.Today;
			Account = new AccountDay(AccountDate);
			PersonAbsenceAccount.Add(Account);
			Scenario = new Scenario("scenario");
			ScheduleStartDate = DateTime.Now.AddMonths(-1).Date.ToUniversalTime();
			ScheduleEndDate = ScheduleStartDate.AddMonths(2).Date.ToUniversalTime();
			DateTimePeriod = new DateTimePeriod(ScheduleStartDate, ScheduleEndDate);
			ScheduleDateTimePeriod = new ScheduleDateTimePeriod(DateTimePeriod);
			FirstDayDateTimePeriod = new DateTimePeriod(ScheduleStartDate, ScheduleStartDate.AddDays(1));
			FirstDayDateOnly = new DateOnly(ScheduleStartDate);
			ScheduleData = MakeScheduleData();
		}

		protected abstract IPersistableScheduleData MakeScheduleData();

		private void SetupScheduleDictionary() 
		{
			var innerDictionary = new Dictionary<IPerson, IScheduleRange>();

			ScheduleDictionary = new ScheduleDictionaryForTest(Scenario, ScheduleDateTimePeriod, innerDictionary);

			var scheduleParameters = new ScheduleParameters(Scenario, Person, DateTimePeriod);
			var scheduleRange = new ScheduleRange(ScheduleDictionary, scheduleParameters);
			scheduleRange.Add(ScheduleData);

			innerDictionary[Person] = scheduleRange;

			//_scheduleDictionary.TakeSnapshot();
		}

		[TearDown]
		public void ScheduleScreenPersisterIntegrationTestTeardown()
		{
			// clean up database
			IPersonAbsenceAccount personAbsenceAccount;

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				personAbsenceAccount = new PersonAbsenceAccountRepository(unitOfWork).Get(PersonAbsenceAccount.Id.Value);
				personAbsenceAccount.AccountCollection().ForEach(personAbsenceAccount.Remove);
				unitOfWork.PersistAll();
			}

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork()) 
			{
				var personRepository = new PersonRepository(unitOfWork);
				var person = personRepository.Get(Person.Id.Value);
				var absenceRepository = new AbsenceRepository(unitOfWork);
				var absence = absenceRepository.Get(Absence.Id.Value);
				var scenarioRepository = new ScenarioRepository(unitOfWork);
				var scenario = scenarioRepository.Get(Scenario.Id.Value);
				var session = (ISession)typeof (NHibernateUnitOfWork).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unitOfWork);
				var scheduleData =
					session.CreateCriteria(typeof (IPersistableScheduleData)).Add(Restrictions.Eq("Id", ScheduleData.Id)).UniqueResult<IPersistableScheduleData>();

				var repository = new Repository(unitOfWork);
				repository.Remove(personAbsenceAccount);
				repository.Remove(person);
				repository.Remove(absence);
				repository.Remove(scenario);
				if (scheduleData != null)
					repository.Remove(scheduleData);
				unitOfWork.PersistAll();
			}

		}

		protected void DeleteScheduleDataAsAnotherUser()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new Repository(unitOfWork);
				//remove clone to simulate other user (and instance)
				repository.Remove((IPersistableScheduleData)ScheduleData.Clone());
				unitOfWork.PersistAll();
			}
		}

		protected void ModifyPersonAbsenceAccountAsAnotherUser()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(unitOfWork);
				var personAbsenceAccountToChange = personAbsenceAccountRepository.Get(PersonAbsenceAccount.Id.Value);

				ModifyPersonAbsenceAccount(personAbsenceAccountToChange);

				personAbsenceAccountRepository.Add(personAbsenceAccountToChange);

				unitOfWork.PersistAll();
			}
		}

		protected void ModifyPersonAbsenceAccount()
		{
			ModifyPersonAbsenceAccount(PersonAbsenceAccount);
		}

		private static void ModifyPersonAbsenceAccount(IPersonAbsenceAccount personAbsenceAccountToChange)
		{
			foreach (var account in personAbsenceAccountToChange.AccountCollection())
			{
				account.BalanceIn = account.BalanceIn.Add(TimeSpan.FromDays(1));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected IScheduleScreenPersisterResult TryPersistScheduleScreen()
		{
			// create and persist schedule with data using original entity objects which are now stale

			var personAbsenceAccounts = new List<IPersonAbsenceAccount>{PersonAbsenceAccount};
		    //var writeProtect = new PersonWriteProtectionInfo(Person);
            return Target.TryPersist(ScheduleDictionary, PersonWriteProtectionInfoCollection, new IPersonRequest[] { }, personAbsenceAccounts);
		}

		public void ReassociateDataWithAllPeople()
		{
			UnitOfWorkFactory.Current.CurrentUnitOfWork().Reassociate(Scenario);
			UnitOfWorkFactory.Current.CurrentUnitOfWork().Reassociate(TestDataToReassociate());
		}

		public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate)
		{
			return new[] { new IAggregateRoot[] { Scenario }, TestDataToReassociate() };
		}

		protected abstract IEnumerable<IAggregateRoot> TestDataToReassociate();

		public void NotifyMessageQueueSize()
		{
			throw new NotImplementedException();
		}

	}
}