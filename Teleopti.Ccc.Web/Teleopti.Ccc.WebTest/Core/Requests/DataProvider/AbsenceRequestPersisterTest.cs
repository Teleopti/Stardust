using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, RequestsTest,Toggle(Toggles.MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747), Toggle(Toggles.Wfm_Requests_Check_Expired_Requests_40274)]
	public class AbsenceRequestPersisterTest : ISetup
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IUserTimeZone UserTimeZone;
		public IAbsenceRepository AbsenceRepository;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestPersister Persister;
		public AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest AbsenceRequestFormToPersonRequest;
		public RequestsViewModelMappingProfile RequestsViewModelMappingProfile;
		public FakeCommandDispatcher CommandDispatcher;
		public FakeToggleManager ToggleManager;

		private static readonly DateTime nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private DateOnly _today = new DateOnly(nowTime);
		private IWorkflowControlSet _workflowControlSet;
		private IAbsence _absence;
		private IPerson _person;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			_workflowControlSet = new WorkflowControlSet().WithId();
			_person.WorkflowControlSet = _workflowControlSet;

			var personRepository = new FakePersonRepository();
			personRepository.Add(_person);

			system.UseTestDouble(personRepository).For<IPersonRepository>();
			system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			system.UseTestDouble(new FakeQueuedAbsenceRequestRepository()).For<IQueuedAbsenceRequestRepository>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			system.UseTestDouble(new ThisIsNow(nowTime)).For<INow>();
			system.UseTestDouble<AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest>().For<AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest> ();
			system.UseTestDouble<RequestsViewModelMappingProfile>().For<RequestsViewModelMappingProfile> ();
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit> ();

		}

		[Test]
		public void ShouldAddRequest()
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator:true);

			var personRequest = setupSimpleAbsenceRequest();

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleRequestDirectlyWhenRequestShorterThan24HoursAndEndsWithin24HourWindow()
		{
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("a"));
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);
			
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: false);

			_workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(7)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			Persister.Persist(form);

			QueuedAbsenceRequestRepository.LoadAll().Should().Be.Empty();
			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDenyWhenPersonHasNoWorkflowControlSet()
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();
			_person.WorkflowControlSet = null;

			var personRequest = setupSimpleAbsenceRequest();
			
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(Resources.RequestDenyReasonNoWorkflow);
		}
		
		[Test]
		public void ShouldNotDenyWhenPersonHasZeroBalanceButWorkflowControlSetHasNoPersonAccountValidation()
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: false);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldDenyExpiredRequest([Values]bool autoGrant)
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(15, autoGrant);

			var personRequest = setupSimpleAbsenceRequest();

			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(string.Format(Resources.RequestDenyReasonRequestExpired, personRequest.Request.Period.StartDateTime, 15));
		}

		[Test]
		public void ShouldDenyWhenAutoDenyIsOn()
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(autoDeny: true);

			var personRequest = setupSimpleAbsenceRequest();
			
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be("RequestDenyReasonAutodeny");
		}

		[Test]
		public void ShouldDenyWhenAlreadyAbsent([Values]bool autoGrant)
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(autoGrant: autoGrant);

			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var form = createAbsenceRequestForm(dateTimePeriodForm);

			ScheduleStorage.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, CurrentScenario.Current()
				, _today.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
					, UserTimeZone.TimeZone()), _absence).WithId());

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountDaysAreExceeded([Values]bool autoGrant)
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = autoGrant;
			
			setWorkflowControlSet(usePersonAccountValidator:true, autoGrant:autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(1)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.IsWaitlisted.Should().Be(isWaitlisted);
			request.DenyReason.Should().Be(isWaitlisted ? Resources.RequestWaitlistedReasonPersonAccount : Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountTimeIsExceeded([Values]bool autoGrant)
		{
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = autoGrant;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountTime1 = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromMinutes(60)
			};
			var accountTime2 = new AccountTime(_today.AddDays(1))
			{
				Accrued = TimeSpan.FromMinutes(20)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime1, accountTime2);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.IsWaitlisted.Should().Be(isWaitlisted);
			request.DenyReason.Should().Be(isWaitlisted ? Resources.RequestWaitlistedReasonPersonAccount : Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldUpdatePeriodForQueuedRequest()
		{
			ToggleManager.Enable(Toggles.Wfm_Requests_ApprovingModifyRequests_41930);
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);
			_absence = createAbsence();
			var newPersonRequest = setupSimpleAbsenceRequest();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest()
			{
				PersonRequest = newPersonRequest.Id.GetValueOrDefault(),
				StartDateTime = newPersonRequest.Request.Period.StartDateTime,
				EndDateTime = newPersonRequest.Request.Period.EndDateTime
			});

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();
			Persister.Persist(form);

			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(23)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.AddDays(1).Add(TimeSpan.FromMinutes(21)));
		}

		[Test]
		public void ShouldNotUpdatePeriodForQueuedRequestIfToggleIsDisabled()
		{
			ToggleManager.Disable(Toggles.Wfm_Requests_ApprovingModifyRequests_41930);
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);
			_absence = createAbsence();
			var newPersonRequest = setupSimpleAbsenceRequest();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest()
			{
				PersonRequest = newPersonRequest.Id.GetValueOrDefault(),
				StartDateTime = newPersonRequest.Request.Period.StartDateTime,
				EndDateTime = newPersonRequest.Request.Period.EndDateTime
			});

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();
			Persister.Persist(form);

			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(8)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(17)));
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfItsSameAsRequest()
		{
			ToggleManager.Enable(Toggles.Wfm_Requests_ApprovingModifyRequests_41930);
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);
			_absence = createAbsence();
			var newPersonRequest = setupSimpleAbsenceRequest();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest()
			{
				PersonRequest = newPersonRequest.Id.GetValueOrDefault(),
				StartDateTime = newPersonRequest.Request.Period.StartDateTime,
				EndDateTime = newPersonRequest.Request.Period.EndDateTime
			});

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();
			Persister.Persist(form);

			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(8)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(17)));
			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldThrowInvalidOperationIfQueuedRequestIsSent()
		{
			Assert.Throws<InvalidOperationException>(tryPersist);
		}

		private void tryPersist()
		{
			ToggleManager.Enable(Toggles.Wfm_Requests_ApprovingModifyRequests_41930);
			Mapper.Configuration.AddProfile(new AbsenceRequestFormMappingProfile(() => AbsenceRequestFormToPersonRequest));
			Mapper.Configuration.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
			Mapper.Configuration.AddProfile(RequestsViewModelMappingProfile);
			_absence = createAbsence();
			var newPersonRequest = setupSimpleAbsenceRequest();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest()
			{
				PersonRequest = newPersonRequest.Id.GetValueOrDefault(),
				StartDateTime = newPersonRequest.Request.Period.StartDateTime,
				EndDateTime = newPersonRequest.Request.Period.EndDateTime,
				Sent = DateTime.Now
			});

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();
			Persister.Persist(form);
		}

		private void setWorkflowControlSet(int? absenceRequestExpiredThreshold = null, bool autoGrant = false
			, bool usePersonAccountValidator = false, bool autoDeny = false, bool absenceRequestWaitlistEnabled = false)
		{
			_workflowControlSet.AbsenceRequestWaitlistEnabled = absenceRequestWaitlistEnabled;
			_workflowControlSet.AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold;
			var absenceRequestProcess = autoGrant
				? (IProcessAbsenceRequest) new GrantAbsenceRequest()
				: new PendingAbsenceRequest();
			if (autoDeny)
				absenceRequestProcess = new DenyAbsenceRequest();
			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = _absence,
				AbsenceRequestProcess = absenceRequestProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, DateOnly.Today.AddDays(30)),
				Period = new DateOnlyPeriod(_today, DateOnly.Today.AddDays(30)),
				PersonAccountValidator = usePersonAccountValidator? (IAbsenceRequestValidator)new PersonAccountBalanceValidator() : new AbsenceRequestNoneValidator()
			});
		}
		
		private IPersonRequest setupSimpleAbsenceRequest()
		{
			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(1)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Persister.Persist(form);
			return PersonRequestRepository.Get(Guid.Parse(personRequest.Id));
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, params IAccount[] accounts)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);

			foreach (var account in accounts)
			{
				if (account is AccountDay)
					personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();

				if (account is AccountTime)
					personAbsenceAccount.Absence.Tracker = Tracker.CreateTimeTracker();

				personAbsenceAccount.Add(account);
			}
			
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
		}
		
		private AbsenceRequestForm createAbsenceRequestForm(DateTimePeriodForm period)
		{
			var form = new AbsenceRequestForm
			{
				AbsenceId = _absence.Id.Value,
				Subject = "test",
				Period = period
			};
			return form;
		}

		private IAbsence createAbsence()
		{
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			absence.Description = new Description("hliday");
			absence.SetBusinessUnit(BusinessUnitFactory.CreateWithId("temp"));
			AbsenceRepository.Add(absence);
			return absence;
		}
	}
}
