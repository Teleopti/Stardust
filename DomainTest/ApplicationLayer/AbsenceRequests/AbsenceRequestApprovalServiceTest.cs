using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[NoDefaultData]
	public class AbsenceRequestApprovalServiceTest : IIsolateSystem
	{
		private IScheduleDictionary _scheduleDictionary;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		private IRequestApprovalService _requestApprovalService;

		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public FullPermission Permission;
		public ICurrentScenario Scenario;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario { DefaultScenario = true,Restricted = true}.WithId()))
				.For<IScenarioRepository>();
			isolate.UseTestDouble(new MutableNow(DateTime.Now)).For<INow>();
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenAbsenceIsApproved()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			setScheduleDictionary(absenceDateTimePeriod, person);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			createAccount(person, absence, accountDay1, accountDay2);

			setAbsenceApprovalService(person);
			var responses = _requestApprovalService.Approve(personRequest.Request);

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

			setScheduleDictionary(absenceDateTimePeriod, person);

			var holidayAccountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var holidayAccountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			var lieuAccountDay = createAccountDay(new DateOnly(2016, 08, 18));
			createAccount(person, holidayAbsence, holidayAccountDay1, holidayAccountDay2);
			createAccount(person, lieuAbsence, lieuAccountDay);

			setAbsenceApprovalService(person);
			var responses = _requestApprovalService.Approve(personRequest.Request);

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

			setScheduleDictionary(new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 23, 23), person);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			createAccount(person, absence, accountDay1, accountDay2);

			setAbsenceApprovalService(person);
			var absence1Responses = _requestApprovalService.Approve(personRequest1.Request);
			var absence2Responses = _requestApprovalService.Approve(personRequest2.Request);

			Assert.AreEqual(0, absence1Responses.Count());
			Assert.AreEqual(0, absence2Responses.Count());
			Assert.AreEqual(24, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(22, accountDay2.Remaining.TotalDays);
		}

		[Test]
		public void ShouldNotApproveWhenTeamLeaderHasNoPermissionToEditRestrictedScenarios()
		{
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);

			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			setScheduleDictionary(absenceDateTimePeriod, person);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18));
			createAccount(person, absence, accountDay1, accountDay2);

			setAbsenceApprovalService(person);
			var responses = _requestApprovalService.Approve(personRequest.Request);

			Assert.AreEqual(1, responses.Count());
			Assert.AreEqual(Resources.CanNotApproveOrDenyRequestDueToNoPermissionToModifyRestrictedScenarios, responses.ElementAt(0).Message);
		}

		[Test]
		public void ShouldApproveWhenTheScenarioIsRestrictedAndAutoGrantIsTrue()
		{
			Now.Is(new DateTime(2016, 08, 16, 0, 0, 0, DateTimeKind.Utc));
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);

			var absence = new Absence();
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), false);
			var absenceRollingPeriod = new AbsenceRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1,
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 08, 16), new DateOnly(2016, 12, 31))
			};

			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRollingPeriod);

			var person = PersonFactory.CreatePersonWithId();
			person.WorkflowControlSet = workflowControlSet;

			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			setScheduleDictionary(absenceDateTimePeriod, person);

			var accountDay = createAccountDay(new DateOnly(2015, 12, 1));
			createAccount(person, absence, accountDay);

			setAbsenceApprovalService(person);
			var responses = _requestApprovalService.Approve(personRequest.Request);

			Assert.AreEqual(0, responses.Count());
		}

		private static PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var personRequestFactory = new PersonRequestFactory() { Person = person };
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(absence, dateTimePeriod);
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId(Guid.NewGuid());
			return personRequest;
		}

		private void setAbsenceApprovalService(IPerson person)
		{
			_requestApprovalService =
				RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(_scheduleDictionary, Scenario.Current(), person);
		}

		private void createAccount(IPerson person, IAbsence absence, params IAccount[] accountDays)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			foreach (var accountDay in accountDays)
			{
				personAbsenceAccount.Add(accountDay);
			}
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
		}

		private static AccountDay createAccountDay(DateOnly startDate, TimeSpan? balance = null)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance.GetValueOrDefault(TimeSpan.Zero)
			};
		}

		private void setScheduleDictionary(DateTimePeriod dateTimePeriod, params IPerson[] persons)
		{
			var scheduleDatas = new List<IScheduleData>();
			foreach (var person in persons)
			{
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriods = dateTimePeriod.ToDateOnlyPeriod(timeZone).DayCollection();
				foreach (var dateOnlyPeriod in dateOnlyPeriods)
				{
					scheduleDatas.Add(addAssignment(person, dateOnlyPeriod.ToDateTimePeriod(new TimePeriod(0, 0, 23, 0), timeZone)));
				}
			}
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(Scenario.Current(), dateTimePeriod, Permission, data: scheduleDatas.ToArray());
		}

		private IPersonAssignment addAssignment(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), dateTimePeriod);
			return assignment;
		}
	}
}