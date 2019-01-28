using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class CancelAbsenceRequestCommandHandlerEventTest : IIsolateSystem, IExtendSystem
	{
		public CancelAbsenceRequestCommandHandler Target;
		public ScheduleChangedEventDetector ScheduleChangedEventDetector;
		public FakePersonRequestRepository RequestRepository;
		public PersonRequestAuthorizationCheckerConfigurable PersonRequestAuthorizationChecker;
		public ApprovalServiceForTest ApprovalService;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeCommonAgentNameProvider CommonAgentNameProvider;
		public FakeUserCulture UserCulture;
		public FakeGlobalSettingDataRepository GlobalSetting;
		public MutableNow Now;
		public FullPermission FullPermission;
		public IFakeStorage Storage;

		private IPerson person;
		private IAbsence absence;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<CancelAbsenceRequestCommandHandler>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleChangedEventDetector>().For<IHandleEvent<ScheduleChangedEvent>>();
			isolate.UseTestDouble<PersonRequestAuthorizationCheckerConfigurable>().For<IPersonRequestCheckAuthorization>();
			isolate.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
			isolate.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			isolate.UseTestDouble<CancelAbsenceRequestCommandValidator>().For<ICancelAbsenceRequestCommandValidator>();
			isolate.UseTestDouble<WriteProtectedScheduleCommandValidator>().For<IWriteProtectedScheduleCommandValidator>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			isolate.UseTestDouble<ScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			var userCulture = new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture());
			isolate.UseTestDouble(userCulture).For<IUserCulture>();
		}

		private void commonSetup()
		{
			ScenarioRepository.Has("Default");
			absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			person = PersonFactory.CreatePerson("Yngwie", "Malmsteen").WithId();
			PersonRepository.Add(person);
			UserCulture.IsSwedish();
			
			GlobalSetting.PersistSettingValue("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0)));
			GlobalSetting.PersistSettingValue("FullDayAbsenceRequestEndTime", new TimeSpanSetting(new TimeSpan(23, 59, 0)));
		}

		[Test]
		public void TargetShouldBeDefined()
		{
			commonSetup();
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotCancelRequestWhenNoPermission()
		{
			commonSetup();
			var cancelRequestCommand = new CancelAbsenceRequestCommand();

			PersonRequestAuthorizationChecker.RevokeCancelRequestPermission();


			var personRequest = basicCancelAbsenceRequest(cancelRequestCommand);

			Assert.AreEqual(false, personRequest.IsCancelled);
			Assert.AreEqual(1, PersonAbsenceRepository.LoadAll().Count());
			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceCannotBeFound()
		{
			commonSetup();
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 01);

			var absenceRequest = createApprovedAbsenceRequest(absence, dateTimePeriodOfAbsenceRequest, person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CouldNotCancelRequestNoAbsence,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				absenceRequest.Period.StartDateTimeLocal(LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", UserCulture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceHasBeenModified()
		{
			commonSetup();

			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 14);
			var absenceRequest = createApprovedAbsenceRequest(absence, dateTimePeriodOfAbsenceRequest, person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			createPersonAbsence(absence, new DateTimePeriod(2016, 03, 01, 09, 2016, 03, 01, 13), person);

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CouldNotCancelRequestNoAbsence,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				absenceRequest.Period.StartDateTimeLocal(LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", UserCulture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldBeAbleToCancelRequestWhenScheduleIsUnpublished()
		{
			commonSetup();

			var permissions = PrincipalAuthorization.Current_DONTUSE() as FullPermission;
			permissions.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 01, 23, 59, 0, DateTimeKind.Utc));

			var absenceRequest = createApprovedAbsenceRequest(absence, dateTimePeriodOfAbsenceRequest, person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			createShiftsForPeriod(new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 13));
			createPersonAbsence(absence, new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 13), person);

			person.WorkflowControlSet = new WorkflowControlSet
			{
				SchedulePublishedToDate = new DateTime(2016, 01, 01),
				PreferenceInputPeriod = new DateOnlyPeriod(2016, 8, 1, 2016, 9, 1),
				PreferencePeriod = new DateOnlyPeriod(2016, 9, 1, 2016, 10, 1)
			};

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(0, cancelRequestCommand.ErrorMessages.Count);

			var result = Storage.Commit();
			var personAbsence = result.Where(r => r.Status == DomainUpdateType.Delete).Select(r => r.Root).OfType<IPersonAbsence>().Single();
			personAbsence.Period.Should().Be.EqualTo(new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 13));
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceRequestHasNotBeenAccepted()
		{
			commonSetup();
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 01);

			var absenceRequest = new AbsenceRequest(absence, dateTimePeriodOfAbsenceRequest);
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			RequestRepository.Add(personRequest);
			personRequest.Pending();

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CanOnlyCancelApprovedAbsenceRequest,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				absenceRequest.Period.StartDateTimeLocal(LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", UserCulture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		[Ignore("PBI #41943 No longer support unmatched periods when cancelling request")]
		public void CancellingARequestWithMultipleAbsencesShouldUpdatePersonAccount()
		{
			commonSetup();

			var accountDay = createAccountDay(new DateOnly(2016, 03, 1), TimeSpan.FromDays(5), TimeSpan.FromDays(25),
				TimeSpan.FromDays(8)); // have used 8 days

			createPersonAbsenceAccount(person, absence, accountDay);

			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			cancelAbsenceRequestWithMultipleAbsences(cancelRequestCommand, true);

			Assert.AreEqual(30, accountDay.Remaining.TotalDays);
		}

		[Test]
		[Ignore("PBI #41943 No longer support unmatched periods when cancelling request")]
		public void ShouldCancelAcceptedRequestAndDeleteMultipleRelatedAbsences()
		{
			commonSetup();
			var accountDay = createAccountDay(new DateOnly(2016, 03, 1), TimeSpan.FromDays(5), TimeSpan.FromDays(25),
				TimeSpan.FromDays(0));
			createPersonAbsenceAccount(person, absence, accountDay);

			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			var personRequest = cancelAbsenceRequestWithMultipleAbsences(cancelRequestCommand);

			Assert.IsTrue(personRequest.IsCancelled);
			Assert.IsTrue(PersonAbsenceRepository.LoadAll().IsEmpty());
			Assert.IsTrue(cancelRequestCommand.ErrorMessages.IsEmpty());
		}

		[Test]
		public void BasicCancelAbsenceRequestShouldFireRequestPersonAbsenceRemovedEvent()
		{
			commonSetup();
			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			basicCancelAbsenceRequest(cancelRequestCommand);
			var result = Storage.Commit();
			var personAbsence = result.Where(r => r.Status == DomainUpdateType.Delete).Select(r => r.Root).OfType<IPersonAbsence>().Single();

			personAbsence.NotifyTransactionComplete(DomainUpdateType.Delete);
			personAbsence.NotifyDelete();
			var events = personAbsence.PopAllEvents(null).ToArray();

			events.Length.Should().Be.EqualTo(2);
			events[0].Should().Be.OfType<PersonAbsenceRemovedEvent>();
			events[1].Should().Be.OfType<RequestPersonAbsenceRemovedEvent>();
		}

		[Test]
		public void ShouldUpdateMessageWhenThereIsAReplyMessage()
		{
			commonSetup();
			var cancelRequestCommand = new CancelAbsenceRequestCommand { ReplyMessage = "test" };
			var messagePropertyChanged = false;
			PropertyChangedEventHandler propertyChanged = (sender, e) =>
			{
				if (e.PropertyName.Equals("Message", StringComparison.OrdinalIgnoreCase))
				{
					messagePropertyChanged = true;
				}
			};
			var personRequest = basicCancelAbsenceRequest(cancelRequestCommand, propertyChanged);
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains("test"));
			Assert.IsTrue(messagePropertyChanged);
			Assert.IsTrue(cancelRequestCommand.IsReplySuccess);
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenRequestIsCancelled()
		{
			commonSetup();

			var absenceDateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var absenceRequest = createApprovedAbsenceRequest(absence, absenceDateTimePeriod, person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			createShiftsForPeriod(absenceDateTimePeriod);
			createPersonAbsence(absence, absenceDateTimePeriod, person);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(5), TimeSpan.FromDays(1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18), TimeSpan.FromDays(0), TimeSpan.FromDays(3), TimeSpan.FromDays(2));
			createPersonAbsenceAccount(person, absence, accountDay1, accountDay2);

			var cancelRequestCommand = new CancelAbsenceRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};
			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(3, accountDay2.Remaining.TotalDays);
		}

		[Test]
		public void ShouldRecoverPersonalAccountWhenAgentCancellingAnAbsenceRequest()
		{
			Now.Is(new DateTime(2018, 04, 14, 08, 00, 00, DateTimeKind.Utc));
			commonSetup();
			var timeInLieuabsence = AbsenceFactory.CreateAbsenceWithTracker("Time in lieu", Tracker.CreateTimeTracker());
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), false);
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = timeInLieuabsence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				BetweenDays = new MinMax<int>(0, 30)
			});
			person.WorkflowControlSet = workflowControlSet;

			var overnightShiftPeriod = new DateTimePeriod(2018, 04, 15, 17, 2018, 04, 16, 6);
			createShiftsForPeriod(overnightShiftPeriod);

			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, ScenarioRepository.LoadDefaultScenario(),
				new DateOnly(2018, 04, 16), new DayOffTemplate(new Description("dayoff template")));
			PersonAssignmentRepository.Add(dayOff);

			var absenceDateTimePeriod = new DateTimePeriod(2018, 04, 16, 05, 2018, 04, 16, 06);
			var absenceRequest = createApprovedAbsenceRequest(timeInLieuabsence, absenceDateTimePeriod, person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			createPersonAbsence(timeInLieuabsence, absenceDateTimePeriod, person);

			var accountDay =
				createAccountDay(new DateOnly(2017, 7, 1), TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.FromHours(1));
			createPersonAbsenceAccount(person, timeInLieuabsence, accountDay);

			var cancelRequestCommand = new CancelAbsenceRequestCommand {PersonRequestId = personRequest.Id.GetValueOrDefault()};
			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(1, accountDay.Remaining.TotalHours);
		}

		[Test]
		public void ShouldGetErrorMessageWhenCancellingTextRequest()
		{
			commonSetup();

			var period = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 19, 23);
			var textRequest = new TextRequest(period);
			var personRequest = new PersonRequest(person, textRequest).WithId();
			RequestRepository.Add(personRequest);

			var cancelRequestCommand = new CancelAbsenceRequestCommand { PersonRequestId = personRequest.Id.GetValueOrDefault() };

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(null, cancelRequestCommand.AffectedRequestId);
			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(Resources.OnlyAbsenceRequestCanBeCancelled, cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldRecoverPersonalAccountWhenAgentCancellingAnAbsenceRequestOutOfSchedulePublishedDate()
		{
			commonSetup();

			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new PendingAbsenceRequest(),
				false);
			workflowControlSet.SchedulePublishedToDate = new DateTime(2017, 10, 15);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(new DateOnly(2016, 1, 1), new DateOnly(2017, 10, 15));
			workflowControlSet.PreferencePeriod = workflowControlSet.PreferenceInputPeriod;
			person.WorkflowControlSet = workflowControlSet;

			FullPermission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var absenceDateTimePeriod = new DateTimePeriod(2018, 04, 15, 00, 2018, 04, 19, 23);
			var absenceRequest = createApprovedAbsenceRequest(absence, absenceDateTimePeriod, person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			createShiftsForPeriod(absenceDateTimePeriod);
			createPersonAbsence(absence, absenceDateTimePeriod, person);

			var accountDay1 = createAccountDay(new DateOnly(2017, 7, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(5),
				TimeSpan.FromDays(5));
			createPersonAbsenceAccount(person, absence, accountDay1);

			var cancelRequestCommand = new CancelAbsenceRequestCommand { PersonRequestId = personRequest.Id.GetValueOrDefault() };
			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
		}

		private static AccountDay createAccountDay(DateOnly startDate, TimeSpan balanceIn, TimeSpan accrued, TimeSpan balance)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = balanceIn,
				Accrued = accrued,
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance
			};
		}

		private PersonRequest cancelAbsenceRequestWithMultipleAbsences(CancelAbsenceRequestCommand cancelRequestCommand, bool checkPersonAccounts = false)
		{
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 14);
			var absenceRequest = createApprovedAbsenceRequest(absence, dateTimePeriodOfAbsenceRequest, person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			if (checkPersonAccounts)
			{
				createShiftsForPeriod(dateTimePeriodOfAbsenceRequest);
			}

			createPersonAbsence(absence, new DateTimePeriod(2016, 03, 05, 2016, 03, 07), person);
			createPersonAbsence(absence, new DateTimePeriod(2016, 03, 09, 2016, 03, 13), person);
			createPersonAbsence(absence, new DateTimePeriod(2016, 03, 01, 2016, 03, 03), person);

			cancelRequestCommand.PersonRequestId = personRequest.Id.GetValueOrDefault();

			Target.Handle(cancelRequestCommand);

			return personRequest;
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, params IAccount[] accountDays)
		{
			PersonAbsenceAccountRepository.Add(PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absence,
				accountDays));
		}

		private PersonRequest basicCancelAbsenceRequest(CancelAbsenceRequestCommand cancelRequestCommand, PropertyChangedEventHandler propertyChanged = null)
		{
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 03);

			var absenceRequest = createApprovedAbsenceRequest(absence, dateTimePeriodOfAbsenceRequest, person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			if (propertyChanged != null)
				personRequest.PropertyChanged += propertyChanged;

			createPersonAbsence(absence, dateTimePeriodOfAbsenceRequest, person);
			cancelRequestCommand.PersonRequestId = personRequest.Id.GetValueOrDefault();

			Target.Handle(cancelRequestCommand);
			return personRequest;
		}

		private AbsenceRequest createApprovedAbsenceRequest(IAbsence absence, DateTimePeriod dateTimePeriodOfAbsenceRequest, IPerson person)
		{
			var absenceRequest = new AbsenceRequest(absence, dateTimePeriodOfAbsenceRequest);
			createApprovedPersonRequest(person, absenceRequest);
			return absenceRequest;
		}

		private void createApprovedPersonRequest(IPerson person, AbsenceRequest absenceRequest)
		{
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			RequestRepository.Add(personRequest);

			personRequest.Pending();
			personRequest.Approve(ApprovalService, PersonRequestAuthorizationChecker);
		}

		private void createPersonAbsence(IAbsence absence, DateTimePeriod dateTimePeriodOfAbsenceRequest, IPerson person)
		{
			var absenceLayer = new AbsenceLayer(absence, dateTimePeriodOfAbsenceRequest);
			var personAbsence = new PersonAbsence(person, ScenarioRepository.LoadDefaultScenario(), absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
		}

		private void createShiftsForPeriod(DateTimePeriod period)
		{
			var defaultScenario = ScenarioRepository.LoadDefaultScenario();
			foreach (var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				// need to have a shift otherwise personAccount day off will not be affected
				PersonAssignmentRepository.Add(createAssignment(person, day.StartDateTime, day.EndDateTime, defaultScenario));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, new DateTimePeriod(startDate, endDate)).WithId();
		}
	}
}
