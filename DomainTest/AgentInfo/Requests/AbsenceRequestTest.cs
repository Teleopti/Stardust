using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[DomainTest]
	public class AbsenceRequestTest : IIsolateSystem
	{
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IPersonRequestRepository PersonRequestRepository;
		public ICurrentScenario CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonRepository PersonRepository;
		public IScheduleStorage ScheduleStorage;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		private DateTimePeriod _period;
		private Absence _absence;

		private AbsenceRequest _target;

		public void Isolate(IIsolate isolate)
		{
			var person = new Person().WithId();
			isolate.UseTestDouble(new FakeLoggedOnUser(person)).For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("default") {DefaultScenario = true}))
				.For<IScenarioRepository>();
			isolate.UseTestDouble<businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify>()
				.For<IBusinessRulesForPersonalAccountUpdate>();
		}

		[SetUp]
		public void Setup()
		{
			_period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));

			_absence = new Absence {Description = new Description("Holiday", "861")}.WithId();
			_absence.Tracker = Tracker.CreateTimeTracker();
			_absence.InContractTime = true;
			_absence.InWorkTime = true;
			_absence.InPaidTime = true;

			_target = new AbsenceRequest(_absence, _period);
		}

		[Test]
		public void VerifyHasEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(_period, _target.Period);
			Assert.AreEqual(_absence, _target.Absence);

			_target.RequestTypeDescription = "Absence";
			Assert.AreEqual("Absence", _target.RequestTypeDescription);

			Assert.AreEqual(_absence.Description, _target.RequestPayloadDescription);
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "personRequest")]
		[Test]
		public void VerifyDenySetsTextForNotificationIfMultipleDaysAbsence()
		{
			new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			_target.Deny(null);
			Assert.IsNotEmpty(_target.TextForNotification);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "personRequest")]
		[Test]
		public void VerifyDenySetsTextForNotificationIfOneDayAbsence()
		{
			var period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc));
			var absence = new Absence {Description = new Description("Holiday", "861")};

			var target = new AbsenceRequest(absence, period);

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), target);
			target.Deny(null);
			Assert.IsNotEmpty(target.TextForNotification);
		}

		[Test]
		public void VerifyApproveAbsenceCallWorks()
		{
			var service = createAbsenceRequestApproveService();
			IPersonRequestCheckAuthorization authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			personRequest.Pending();

			var brokenRules = personRequest.Approve(service, authorization);
			Assert.AreEqual(0, brokenRules.Count);

			Assert.IsNotEmpty(_target.TextForNotification);
		}

		[Test]
		public void VerifyApproveOneDayAbsenceCallWorks()
		{
			var absence = new Absence {Description = new Description("Holiday", "861")};

			var requestApprovalService = createAbsenceRequestApproveService();

			var authorization = new PersonRequestAuthorizationCheckerForTest();
			var period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc));
			var target = new AbsenceRequest(absence, period);
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), target);

			personRequest.Pending();

			var brokenRules = personRequest.Approve(requestApprovalService, authorization);
			Assert.AreEqual(0, brokenRules.Count);
			Assert.That(target.TextForNotification, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void VerifyApproveAbsenceCallDoesNotSendNotificationWhenFailing()
		{
			setCurrentPrincipal();
			setPermissions();

			var person = LoggedOnUser.CurrentUser();
			var personAbsenceAccount = new PersonAbsenceAccount(person, _absence).WithId();
			var balance = TimeSpan.Zero;
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2008, 7, 15)) {LatestCalculatedBalance = balance}.WithId());
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2008, 7, 16)) {LatestCalculatedBalance = balance}.WithId());
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2008, 7, 17)) {LatestCalculatedBalance = balance}.WithId());
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2008, 7, 18)) {LatestCalculatedBalance = balance}.WithId());
			personAbsenceAccount.Add(new AccountDay(new DateOnly(2008, 7, 19)) {LatestCalculatedBalance = balance}.WithId());
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);

			var requestApprovalService = createAbsenceRequestApproveService();
			var authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(person, _target);
			personRequest.Pending();

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = _absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				BetweenDays = new MinMax<int>(0, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2008, 7, 15, 2008, 7, 19)
			});
			person.WorkflowControlSet = workflowControlSet;

			var brokenRules = personRequest.Approve(requestApprovalService, authorization);
			Assert.AreEqual(1, brokenRules.Count);
			Assert.IsTrue(string.IsNullOrEmpty(_target.TextForNotification));
		}

		[Test]
		public void VerifyClone()
		{
			var entityClone = (IAbsenceRequest) _target.EntityClone();

			Assert.AreEqual(_target.Person, entityClone.Person);
			Assert.AreEqual(_target.Id, entityClone.Id);
			Assert.AreEqual(_target.Absence, entityClone.Absence);
			Assert.AreEqual(_target.Period, entityClone.Period);
			Assert.AreEqual(_target.RequestTypeDescription, entityClone.RequestTypeDescription);

			var clone = (IAbsenceRequest) _target.Clone();
			Assert.AreNotEqual(_target, clone);
		}

		[Test]
		public void VerifyPersonReturnsNullWithoutParent()
		{
			var obj = new AbsenceRequest(new Absence {Description = new Description("The other Absence")},
				new DateTimePeriod(new DateTime(2009, 12, 10, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2009, 12, 10, 23, 59, 0, DateTimeKind.Utc)));
			Assert.IsNull(obj.Person);
		}

		[Test]
		public void VerifyCanGetDetails()
		{
			var person = LoggedOnUser.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var absenceRequest = new PersonRequest(person,
				new AbsenceRequest(new Absence {Description = new Description("The Absence")}, _period)).Request;
			var text = absenceRequest.GetDetails(new CultureInfo("en-US"));

			Assert.AreEqual("The Absence, 2:00 AM - 2:00 AM", text);
			text = absenceRequest.GetDetails(new CultureInfo("ko-KR"));
			Assert.AreEqual("The Absence, 오전 2:00 - 오전 2:00", text);
			text = absenceRequest.GetDetails(new CultureInfo("zh-TW"));
			Assert.AreEqual("The Absence, 上午 02:00 - 上午 02:00", text);

			var obj2 =
				new PersonRequest(person,
					new AbsenceRequest(new Absence {Description = new Description("The other Absence")},
						new DateTimePeriod(new DateTime(2009, 12, 10, 0, 0, 0, DateTimeKind.Utc),
							new DateTime(2009, 12, 10, 23, 59, 0, DateTimeKind.Utc)))).Request;
			var otertext = obj2.GetDetails(new CultureInfo("es-ES"));
			Assert.AreEqual("The other Absence", otertext);
		}

		[Test]
		public void VerifyTheOnlyOneThatShouldGetNotifiedIsThePersonInvolvedInTheRequest()
		{
			Assert.IsTrue(_target.ReceiversForNotification.Contains(_target.Person));
			Assert.AreEqual(1, _target.ReceiversForNotification.Count);
		}

		[Test]
		public void CanNotUpdateAbsenceTypeOfAnApprovedRequest()
		{
			var service = createAbsenceRequestApproveService();
			var authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			personRequest.Pending();

			personRequest.Approve(service, authorization);
			personRequest.IsApproved.Should().Be(true);
			personRequest.Persisted();

			var anotherAbsence = new Absence().WithId();

			Assert.Throws<InvalidOperationException>(() =>
			{
				(personRequest.Request as AbsenceRequest)?.SetAbsence(anotherAbsence);
			});
		}

		[Test]
		public void CanNotUpdateAbsenceTypeOfAnDeniedRequest()
		{
			var authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			personRequest.Pending();

			personRequest.Deny(string.Empty, authorization);
			personRequest.IsDenied.Should().Be(true);
			personRequest.Persisted();

			var anotherAbsence = new Absence().WithId();

			Assert.Throws<InvalidOperationException>(() =>
			{
				(personRequest.Request as AbsenceRequest)?.SetAbsence(anotherAbsence);
			});
		}

		[Test]
		public void CanNotUpdateAbsencePeriodOfAnApprovedRequest()
		{
			var service = createAbsenceRequestApproveService();
			var authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			personRequest.Pending();

			personRequest.Approve(service, authorization);
			personRequest.IsApproved.Should().Be(true);
			personRequest.Persisted();

			Assert.Throws<InvalidOperationException>(() =>
			{
				(personRequest.Request as AbsenceRequest)?.SetPeriod(new DateTimePeriod(2018,1,2,2018,2,2));
			});
		}

		[Test]
		public void CanNotUpdateAbsencePeriodOfAnDeniedRequest()
		{
			var authorization = new PersonRequestAuthorizationCheckerForTest();

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), _target);
			personRequest.Pending();

			personRequest.Deny(string.Empty, authorization);
			personRequest.IsDenied.Should().Be(true);
			personRequest.Persisted();

			Assert.Throws<InvalidOperationException>(() =>
			{
				(personRequest.Request as AbsenceRequest)?.SetPeriod(new DateTimePeriod(2018, 1, 2, 2018, 2, 2));
			});
		}

		[Test]
		public void WhenAbsenceRequestIsAcceptedAbsenceShouldBeCorrect()
		{
			var startDateTime = new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2008, 7, 16, 23, 59, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDateTime, endDateTime);

			var person = LoggedOnUser.CurrentUser();
			PersonRepository.Add(person);

			var personRequest = createAbsenceRequest(person, _absence, period);
			personRequest.Pending();

			var scheduleDictionary = initScheduleDictionary();
			var absenceRequestApproveService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, CurrentScenario.Current(),
				LoggedOnUser.CurrentUser());
			personRequest.Approve(absenceRequestApproveService, new PersonRequestAuthorizationCheckerForTest());

			var scheduleDay = scheduleDictionary[person].ScheduledDay(new DateOnly(startDateTime));
			var personAbsences = scheduleDay.PersonAbsenceCollection(true);
			var personAbsence = personAbsences[0];

			personAbsence.Layer.Period.Should().Be.EqualTo(period);
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod)).WithId();
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private IRequestApprovalService createAbsenceRequestApproveService()
		{
			var scheduleDictionary = initScheduleDictionary();

			return RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(scheduleDictionary, CurrentScenario.Current(),
				LoggedOnUser.CurrentUser());
		}

		private IScheduleDictionary initScheduleDictionary()
		{
			var dateTimePeriod = new DateTimePeriod(2008, 7, 16, 2008, 7, 18);
			var person = LoggedOnUser.CurrentUser();

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(),
				new Activity("test")
				, dateTimePeriod, new ShiftCategory("category")));

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false)
				, new DateOnlyPeriod(2008, 7, 15, 2008, 7, 19), CurrentScenario.Current());
			((IReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
			return scheduleDictionary;
		}

		private static void setPermissions()
		{
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			var claims = new List<Claim>
			{
				new Claim(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
						, "/", DefinedRaptorApplicationFunctionPaths.ViewSchedules)
					, new AuthorizeEveryone(), Rights.PossessProperty),
				new Claim(
					string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
						"/AvailableData"), new AuthorizeEveryone(), Rights.PossessProperty)
			};
			principal.AddClaimSet(new DefaultClaimSet(ClaimSet.System, claims));
		}

		private static void setCurrentPrincipal()
		{
			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(
				TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var principal = new TeleoptiPrincipalForLegacyFactory().MakePrincipal(new PersonAndBusinessUnit(loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest), dataSource, null);
			Thread.CurrentPrincipal = principal;
		}

		private class
			businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify : BusinessRulesForPersonalAccountUpdate,
				IBusinessRulesForPersonalAccountUpdate
		{
			private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
			private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

			public businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify(
				IPersonAbsenceAccountRepository personAbsenceAccountRepository,
				ISchedulingResultStateHolder schedulingResultStateHolder) : base(personAbsenceAccountRepository,
				schedulingResultStateHolder)
			{
				_personAbsenceAccountRepository = personAbsenceAccountRepository;
				_schedulingResultStateHolder = schedulingResultStateHolder;
			}

			public new INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange)
			{
				var personAccounts = _personAbsenceAccountRepository.FindByUsers(new Collection<IPerson> {scheduleRange.Person});
				var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder, personAccounts);
				((IValidateScheduleRange) scheduleRange).ValidateBusinessRules(rules);
				return rules;
			}
		}
	}
}