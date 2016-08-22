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

			var assignment1 = addAssignment(person, new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 17, 23));
			var assignment2 = addAssignment(person, new DateTimePeriod(2016, 08, 18, 00, 2016, 08, 18, 23));
			var assignment3 = addAssignment(person, new DateTimePeriod(2016, 08, 19, 00, 2016, 08, 19, 23));
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleData(person, _scenario, absenceDateTimePeriod,
				assignment1, assignment2, assignment3);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			var account = createAccount(person, absence, accountDay1, accountDay2);

			_schedulingResultStateHolder.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
			{
				{person, new PersonAccountCollection(person) {account}}
			};
			_schedulingResultStateHolder.Schedules = _scheduleDictionary;
			_newBusinessRules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder);

			setRequestApprovalService();
			var reponses = _requestApprovalService.ApproveAbsence(absence, absenceDateTimePeriod, person, personRequest);

			Assert.AreEqual(0, reponses.Count());
			Assert.AreEqual(24, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(23, accountDay2.Remaining.TotalDays);
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
				_newBusinessRules, _scheduleDayChangeCallback, _globalSettingsDataRepository, _personAbsenceAccountRepository);
		}
	}
}
