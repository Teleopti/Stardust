using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters 
{
	public abstract class ScheduleScreenPersisterIntegrationTest : DatabaseTestWithoutTransaction, IOwnMessageQueue, IReassociateData
	{
		private IClearReferredShiftTradeRequests _clearReferredShiftTradeRequests;
		private IMessageBrokerIdentifier _messageBrokerIdentifier;
	    private ICurrentScenario _currentScenario;

	    protected IScheduleDictionaryConflictCollector ScheduleDictionaryConflictCollector { get; set; }
		protected ScheduleRepository ScheduleRepository { get; set; }
		protected PersonAssignmentRepository PersonAssignmentRepository { get; set; }

		protected IScheduleDictionarySaver ScheduleDictionarySaver { get; set; }
		protected IPersonAbsenceAccount PersonAbsenceAccount { get; set; }
		protected IScenario Scenario { get; set; }
		protected IPerson Person { get; set; }
		protected IPersistableScheduleData ScheduleData { get; set; }
		protected IScheduleDictionary ScheduleDictionary { get; private set; }
		protected IAbsence Absence { get; private set; }
		protected IActivity Activity { get; private set; }
		protected IShiftCategory ShiftCategory { get; private set; }
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



		[SetUp]
		public void ScheduleScreenPersisterIntegrationTestSetup()
		{
			SetupEntities();
			SetupDatabase();
			SetupScheduleDictionary();
			SetupDependencies();
		}

		private void SetupEntities() 
		{
			Person = PersonFactory.CreatePerson("person", "one");
			Absence = AbsenceFactory.CreateAbsence("absence");
			Absence.Tracker = Tracker.CreateDayTracker();
			PersonAbsenceAccount = new PersonAbsenceAccount(Person, Absence);
			Activity = new Activity("activity");
			ShiftCategory = new ShiftCategory("shift category");
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
			ScheduleData = SetupScheduleData();
		}

		protected abstract IPersistableScheduleData SetupScheduleData();

		private void SetupDatabase()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(unitOfWork).Add(Person);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new PersonAbsenceAccountRepository(unitOfWork).Add(PersonAbsenceAccount);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				if (ScheduleData != null)
					new ScheduleRepository(unitOfWork).Add(ScheduleData);

				unitOfWork.PersistAll();
			}
		}

		private void SetupScheduleDictionary() 
		{
			var innerDictionary = new Dictionary<IPerson, IScheduleRange>();

			ScheduleDictionary = new ScheduleDictionaryForTest(Scenario, ScheduleDateTimePeriod, innerDictionary);

			var scheduleParameters = new ScheduleParameters(Scenario, Person, DateTimePeriod);
			var scheduleRange = new ScheduleRange(ScheduleDictionary, scheduleParameters);
			if (ScheduleData != null)
				scheduleRange.Add(ScheduleData);

			innerDictionary[Person] = scheduleRange;
		}

		private void SetupDependencies()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(unitOfWork.DatabaseVersion(PersonAbsenceAccount), Is.EqualTo(1));
			}

			_clearReferredShiftTradeRequests = Mocks.DynamicMock<IClearReferredShiftTradeRequests>();
			_messageBrokerIdentifier = Mocks.DynamicMock<IMessageBrokerIdentifier>();
			ScheduleRepository = new ScheduleRepository(UnitOfWorkFactory.Current);
			PersonAssignmentRepository = new PersonAssignmentRepository(UnitOfWorkFactory.Current);
			ScheduleDictionaryConflictCollector = Mocks.DynamicMock<IScheduleDictionaryConflictCollector>();
			ScheduleDictionarySaver = new ScheduleDictionarySaver();
		    _currentScenario = Mocks.DynamicMock<ICurrentScenario>();
		    _currentScenario.Stub(x => x.Current()).Return(Scenario);
			Mocks.ReplayAll();
		}







		protected void MakeTarget()
		{
			Target = new ScheduleScreenRetryingPersister(UnitOfWorkFactory.CurrentUnitOfWorkFactory(),
													   new WriteProtectionRepository(UnitOfWorkFactory.Current),
													   new PersonRequestRepository(UnitOfWorkFactory.Current),
													   new PersonAbsenceAccountRepository(UnitOfWorkFactory.Current),
													   new PersonRequestPersister(_clearReferredShiftTradeRequests),
													   new PersonAbsenceAccountConflictCollector(),
													   new TraceableRefreshService(_currentScenario,new ScheduleRepository(UnitOfWorkFactory.Current)), 
													   ScheduleDictionaryConflictCollector,
													   _messageBrokerIdentifier,
													   new ScheduleDictionaryBatchPersister(
														   UnitOfWorkFactory.CurrentUnitOfWorkFactory(),
														   ScheduleRepository,
														   ScheduleDictionarySaver,
														   new DifferenceEntityCollectionService<IPersistableScheduleData>(),
														   _messageBrokerIdentifier,
														   this,
														   new ScheduleDictionaryModifiedCallback()),
													   this);
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

		protected void DeleteCurrentPersonAbsenceAccountAsAnotherUser()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(unitOfWork);
				var personAbsenceAccount = personAbsenceAccountRepository.Get(PersonAbsenceAccount.Id.Value);
				deleteLastAccount(personAbsenceAccount);
				personAbsenceAccountRepository.Add(personAbsenceAccount);
				unitOfWork.PersistAll();
			}
		}

		protected void ModifyPersonAbsenceAccountAsAnotherUser()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(unitOfWork);
				var personAbsenceAccount = personAbsenceAccountRepository.Get(PersonAbsenceAccount.Id.Value);
				modifyPersonAbsenceAccount(personAbsenceAccount);
				personAbsenceAccountRepository.Add(personAbsenceAccount);

				unitOfWork.PersistAll();
			}
		}

		protected IPersonAssignment AddPersonAssignmentAsAnotherUser(DateOnly date)
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personAssignmentRepository = new PersonAssignmentRepository(unitOfWork);
				var personAssignment = new PersonAssignment(Person, Scenario, date);
				personAssignment.AddMainLayer(Activity, FirstDayDateTimePeriod);
				personAssignmentRepository.Add(personAssignment);

				unitOfWork.PersistAll();

				return personAssignment;
			}
		}

		protected void AddPersonAssignmentInMemory(DateOnly date)
		{
			if (ScheduleData != null)
				throw new Exception("You'v already created a schedule data, and I can only handle 1, unless you modify me");
			var personAssignment = new PersonAssignment(Person, Scenario, date);
			personAssignment.AddMainLayer(Activity, new DateTimePeriod(date, date.AddDays(1)));

			
			var scheduleDay = ScheduleDictionary[Person].ScheduledDay(date);
			scheduleDay.Add(personAssignment);
			ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		protected void ModifyPersonAbsenceAccountInMemory()
		{
			modifyPersonAbsenceAccount(PersonAbsenceAccount);
		}

		private static void modifyPersonAbsenceAccount(IPersonAbsenceAccount personAbsenceAccountToChange)
		{
			foreach (var account in personAbsenceAccountToChange.AccountCollection())
			{
				account.BalanceIn = account.BalanceIn.Add(TimeSpan.FromDays(1));
			}
		}

		private static void deleteLastAccount(IPersonAbsenceAccount personAbsenceAccountToChange)
		{
			var absences = personAbsenceAccountToChange.AccountCollection().ToList();
			var toRemove = absences[absences.Count - 1];
			personAbsenceAccountToChange.Remove(toRemove);
		}

		protected IScheduleScreenPersisterResult TryPersistScheduleScreen()
		{
			// create and persist schedule with data using original entity objects which are now stale

			var personAbsenceAccounts = new List<IPersonAbsenceAccount> { PersonAbsenceAccount };
			//var writeProtect = new PersonWriteProtectionInfo(Person);
			return Target.TryPersist(ScheduleDictionary, PersonWriteProtectionInfoCollection, new IPersonRequest[] { }, personAbsenceAccounts);
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
				var activityRepository = new ActivityRepository(unitOfWork);
				var activity = activityRepository.Get(Activity.Id.Value);
				var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
				var shiftCategory = shiftCategoryRepository.Get(ShiftCategory.Id.Value);
				var scenarioRepository = new ScenarioRepository(unitOfWork);
				var scenario = scenarioRepository.Get(Scenario.Id.Value);
				var session = unitOfWork.FetchSession();
				IPersistableScheduleData scheduleData = null;
				if (ScheduleData != null)
					scheduleData = session.CreateCriteria(typeof (IPersistableScheduleData)).Add(Restrictions.Eq("Id", ScheduleData.Id)).UniqueResult<IPersistableScheduleData>();

				var repository = new Repository(unitOfWork);
				repository.Remove(personAbsenceAccount);
				repository.Remove(person);
				repository.Remove(absence);
				repository.Remove(activity);
				repository.Remove(shiftCategory);
				repository.Remove(scenario);
				if (scheduleData != null)
					repository.Remove(scheduleData);
				unitOfWork.PersistAll();
			}

		}





		public void ReassociateDataWithAllPeople()
		{
			UnitOfWorkFactory.Current.CurrentUnitOfWork().Reassociate(Scenario);
			UnitOfWorkFactory.Current.CurrentUnitOfWork().Reassociate(TestDataToReassociate());
		}

		public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate)
		{
			return new[] { new IAggregateRoot[] { Scenario, Activity, ShiftCategory }, TestDataToReassociate() };
		}

		protected abstract IEnumerable<IAggregateRoot> TestDataToReassociate();

		public void NotifyMessageQueueSizeChange()
		{
			throw new NotImplementedException();
		}

	}
}