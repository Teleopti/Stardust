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
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class CancelAbsenceRequestCommandHandlerEventTest :ISetup
	{
		public CancelAbsenceRequestCommandHandler Target;
		public ScheduleChangedEventDetector ScheduleChangedEventDetector;
		public FakePersonRequestRepository RequestRepository;
		public PersonRequestAuthorizationCheckerConfigurable PersonRequestAuthorizationChecker;
		public ApprovalServiceForTest ApprovalService;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public CancelAbsenceRequestCommandValidator CancelAbsenceRequestCommandValidator;
		public FakeCurrentScenario CurrentScenario;
		public WriteProtectedScheduleCommandValidator WriteProtectedScheduleCommandValidator;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeCommonAgentNameProvider CommonAgentNameProvider;
		public FakeUserCulture UserCulture;
		public FakeScheduleDifferenceSaver ScheduleDifferenceSaver;
		public FakeGlobalSettingDataRepository GlobalSetting;
		public INow Now;
	
		private IPerson person;
		private IAbsence absence;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ScheduleChangedEventDetector>().For<IHandleEvent<ScheduleChangedEvent>>();
			system.UseTestDouble<PersonAbsenceRemover>().For<IPersonAbsenceRemover>(); 
			system.UseTestDouble<SaveSchedulePartService>().For<ISaveSchedulePartService>();
			system.UseTestDouble<PersonAbsenceCreator>().For<IPersonAbsenceCreator>();		
			system.UseTestDouble<PersonRequestAuthorizationCheckerConfigurable>().For<IPersonRequestCheckAuthorization>();
			system.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<CancelAbsenceRequestCommandValidator>().For<ICancelAbsenceRequestCommandValidator>();
			system.UseTestDouble<WriteProtectedScheduleCommandValidator>().For<IWriteProtectedScheduleCommandValidator>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<FakeEventHandler>().For<FakeEventHandler>();
			var userCulture = new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture());
			system.UseTestDouble(userCulture).For<IUserCulture>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();			
			system.AddService<CancelAbsenceRequestCommandHandler>();			
		}
		
		private void commonSetup()
		{
			absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			person = PersonFactory.CreatePerson("Yngwie","Malmsteen").WithId();
			PersonRepository.Add(person);
			UserCulture.IsSwedish();
			ScheduleDifferenceSaver.SetScheduleStorage(ScheduleStorage);

			GlobalSetting.PersistSettingValue("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0)));
			GlobalSetting.PersistSettingValue("FullDayAbsenceRequestEndTime",new TimeSpanSetting(new TimeSpan(23,59,0)));
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
			Assert.AreEqual(1, ScheduleStorage.LoadAll().Count());
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
				CommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
				absenceRequest.Period.StartDateTimeLocal(LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", UserCulture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceHasBeenModified()
		{
			commonSetup();

			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016,03,01,08, 2016,03,01, 14);
			var absenceRequest = createApprovedAbsenceRequest(absence,dateTimePeriodOfAbsenceRequest,person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			createPersonAbsence(absence,new DateTimePeriod(2016,03,01, 09, 2016,03,01,13 ),person);

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(1,cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CouldNotCancelRequestNoAbsence,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
				absenceRequest.Period.StartDateTimeLocal(LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d",UserCulture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldBeAbleToCancelRequestWhenScheduleIsUnpublished()
		{
			commonSetup();
			
			var permissions = PrincipalAuthorization.Current() as FullPermission;
			permissions.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 01, 23, 59, 0,DateTimeKind.Utc));

			var absenceRequest = createApprovedAbsenceRequest(absence,dateTimePeriodOfAbsenceRequest,person);
			var personRequest = absenceRequest.Parent as PersonRequest;
			createShiftsForPeriod(new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 13));
			createPersonAbsence(absence,new DateTimePeriod(2016,03,01,08,2016,03,01,13),person);

			person.WorkflowControlSet = new WorkflowControlSet();
			person.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2016, 01, 01);
			person.WorkflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2016, 8, 1, 2016, 9, 1);
			person.WorkflowControlSet.PreferencePeriod = new DateOnlyPeriod(2016,9,1,2016,10,1);

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(0,cancelRequestCommand.ErrorMessages.Count);

			var schedules = ScheduleStorage.LoadAll();
			schedules.Single().Period.Should().Be.EqualTo(new DateTimePeriod(2016, 03, 01, 08, 2016, 03, 01, 13));
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
				CommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
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
			Assert.IsTrue(ScheduleStorage.LoadAll().IsEmpty());
			Assert.IsTrue(cancelRequestCommand.ErrorMessages.IsEmpty());
		}


		[Test]
		public void BasicCancelAbsenceRequestShouldFireRequestPersonAbsenceRemovedEvent()
		{
			commonSetup();
			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			basicCancelAbsenceRequest(cancelRequestCommand);
			var personAbsence = PersonAbsenceRepository.LoadAll().First();

			personAbsence.NotifyTransactionComplete(DomainUpdateType.Delete);
			personAbsence.NotifyDelete();
			var events = personAbsence.PopAllEvents().ToArray();

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

			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			cancelRequestCommand.PersonRequestId = personRequest.Id.GetValueOrDefault();
			Target.Handle(cancelRequestCommand);

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(3, accountDay2.Remaining.TotalDays);
		}

		private AccountDay createAccountDay(DateOnly startDate, TimeSpan balanceIn, TimeSpan accrued, TimeSpan balance)
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

		private AbsenceRequest createApprovedAbsenceRequest(IAbsence absence,DateTimePeriod dateTimePeriodOfAbsenceRequest,IPerson person)
		{
			var absenceRequest = new AbsenceRequest(absence,dateTimePeriodOfAbsenceRequest);
			createApprovedPersonRequest(person,absenceRequest);
			return absenceRequest;
		}

		private PersonRequest createApprovedPersonRequest(IPerson person,AbsenceRequest absenceRequest)
		{
			var personRequest = new PersonRequest(person,absenceRequest).WithId();
			RequestRepository.Add(personRequest);

			personRequest.Pending();
			personRequest.Approve(ApprovalService, PersonRequestAuthorizationChecker);
			return personRequest;
		}

		private void createPersonAbsence(IAbsence absence,DateTimePeriod dateTimePeriodOfAbsenceRequest,IPerson person)
		{
			var absenceLayer = new AbsenceLayer(absence,dateTimePeriodOfAbsenceRequest);
			var personAbsence = new PersonAbsence(person, CurrentScenario.Current(), absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
			ScheduleStorage.Add(personAbsence);
		}

		private void createShiftsForPeriod(DateTimePeriod period)
		{
			foreach(var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				// need to have a shift otherwise personAccount day off will not be affected
				ScheduleStorage.Add(createAssignment(person, day.StartDateTime,day.EndDateTime, CurrentScenario.Current()));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person,DateTime startDate,DateTime endDate, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, new DateTimePeriod(startDate,endDate));
		}

	}

	public class FakeEventHandler
	{
		public void Handle(RequestPersonAbsenceRemovedEvent requestPersonAbsenceEvent)
		{
			
		}
	}
}
