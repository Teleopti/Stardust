using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	public class RequestApprovalServiceSchedulerNoMockTest
	{
		private IRequestApprovalService _requestApprovalService;
		private IScheduleDictionary _scheduleDictionary;
		private IScenario _scenario;
		private ISwapAndModifyService _swapAndModifyService;
		private INewBusinessRuleCollection _newBusinessRules;
		private IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private IGlobalSettingDataRepository _globalSettingsDataRepository;
		private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private FakeSchedulingResultStateHolder _schedulingResultStateHolder;

		[SetUp]
		public void Setup()
		{
			_scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			_scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			_swapAndModifyService = new SwapAndModifyService(new SwapService(), _scheduleDayChangeCallback);

			_schedulingResultStateHolder = new FakeSchedulingResultStateHolder();

			_globalSettingsDataRepository = new FakeGlobalSettingDataRepository();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenAbsenceIsApproved()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			setScheduleDictionary(person, absenceDateTimePeriod);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			var account = createAccount(person, absence, accountDay1, accountDay2);

			setBusinessRules(person, account);

			setRequestApprovalService();
			var responses = _requestApprovalService.ApproveAbsence(absence, absenceDateTimePeriod, person, personRequest);

			Assert.AreEqual(0, responses.Count());
			Assert.AreEqual(24, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(23, accountDay2.Remaining.TotalDays);
		}

		[Test]
		public void ShouldOnlyUpdateSpecificAbsencePersonalAccounts()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var person = PersonFactory.CreatePersonWithId();
			var holidayAbsence = AbsenceFactory.CreateAbsence("holiday");
			var lieuAbsence = AbsenceFactory.CreateAbsence("lieu");
			var personRequest = createAbsenceRequest(person, holidayAbsence, absenceDateTimePeriod);

			setScheduleDictionary(person, absenceDateTimePeriod);

			var holidayAccountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var holidayAccountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			var lieuAccountDay = createAccountDay(new DateOnly(2016, 08, 18));
			var holidayAccount = createAccount(person, holidayAbsence, holidayAccountDay1, holidayAccountDay2);
			createAccount(person, lieuAbsence, lieuAccountDay);

			setBusinessRules(person, holidayAccount);

			setRequestApprovalService();
			var responses = _requestApprovalService.ApproveAbsence(holidayAbsence, absenceDateTimePeriod, person, personRequest);

			Assert.AreEqual(0, responses.Count());
			Assert.AreEqual(24, holidayAccountDay1.Remaining.TotalDays);
			Assert.AreEqual(23, holidayAccountDay2.Remaining.TotalDays);
			Assert.AreEqual(25, lieuAccountDay.Remaining.TotalDays);
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenMultipleAbsencesAreApproved()
		{
			
			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			var absence1DateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var personRequest1 = createAbsenceRequest(person, absence, absence1DateTimePeriod);

			var absence2DateTimePeriod = new DateTimePeriod(2016, 08, 23, 00, 2016, 08, 23, 23);
			var personRequest2 = createAbsenceRequest(person, absence, absence2DateTimePeriod);

			setScheduleDictionary(person, new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 23, 23));

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			var account = createAccount(person, absence, accountDay1, accountDay2);

			setBusinessRules(person, account);

			setRequestApprovalService();
			var absence1Responses =_requestApprovalService.ApproveAbsence(absence, absence1DateTimePeriod, person, personRequest1);
			var absence2Responses = _requestApprovalService.ApproveAbsence(absence, absence2DateTimePeriod, person, personRequest2);

			Assert.AreEqual(0, absence1Responses.Count());
			Assert.AreEqual(0, absence2Responses.Count());
			Assert.AreEqual(24, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(22, accountDay2.Remaining.TotalDays);
		}


		private void setScheduleDictionary(IPerson person, DateTimePeriod absenceDateTimePeriod)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var scheduleDatas = new List<IScheduleData>();
			var dateOnlyPeriods = absenceDateTimePeriod.ToDateOnlyPeriod(timeZone).DayCollection();
			foreach (var dateOnlyPeriod in dateOnlyPeriods)
			{
				scheduleDatas.Add(addAssignment(person, dateOnlyPeriod.ToDateTimePeriod(new TimePeriod(0, 0, 23, 0), timeZone)));
			}
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleData(person, _scenario, absenceDateTimePeriod,
				scheduleDatas.ToArray());
		}

		private void setBusinessRules(IPerson person, PersonAbsenceAccount account)
		{
			_schedulingResultStateHolder.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
			{
				{person, new PersonAccountCollection(person) {account}}
			};
			_schedulingResultStateHolder.Schedules = _scheduleDictionary;
			_newBusinessRules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder);
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var personRequestFactory = new PersonRequestFactory() { Person = person };
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(absence, dateTimePeriod);
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId(Guid.NewGuid());
			return personRequest;
		}

		private PersonAbsenceAccount createAccount(IPerson person, IAbsence absence, params IAccount[] accountDays)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			foreach (var accountDay in accountDays)
			{
				personAbsenceAccount.Add(accountDay);
			}
			_personAbsenceAccountRepository.Add(personAbsenceAccount);
			return personAbsenceAccount;
		}

		private AccountDay createAccountDay(DateOnly startDate, TimeSpan? balance = null)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance.GetValueOrDefault(TimeSpan.Zero)
			};
		}

		private IPersonAssignment addAssignment(IPerson person, DateTimePeriod absenceDateTimePeriod)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, person,
				absenceDateTimePeriod);
			return assignment;
		}

		private void setRequestApprovalService()
		{
			_requestApprovalService = new RequestApprovalServiceScheduler(_scheduleDictionary, _scenario, _swapAndModifyService,
				_newBusinessRules, _scheduleDayChangeCallback, _globalSettingsDataRepository, new CheckingPersonalAccountDaysProvider(_personAbsenceAccountRepository));
		}
	}
}
