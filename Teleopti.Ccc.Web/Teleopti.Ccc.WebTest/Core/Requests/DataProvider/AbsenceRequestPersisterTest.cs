using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class AbsenceRequestPersisterTest : IIsolateSystem
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IUserTimeZone UserTimeZone;
		public IAbsenceRepository AbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestPersister Target;
		public FakeCommandDispatcher CommandDispatcher;
		public MutableNow Now;
		public FakeLoggedOnUser LoggedOnUser;

		private static readonly DateTime nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private DateOnly _today = new DateOnly(nowTime);
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			//isolate.UseTestDouble<AbsenceRequestModelMapper>().For<AbsenceRequestModelMapper>();
			//isolate.UseTestDouble<RequestsViewModelMapper>().For<RequestsViewModelMapper>();
		}

		[Test]
		public void ShouldAddRequest()
		{
			var scenario = ScenarioRepository.Has("Default");
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			setupPersonSkills(person);
			var personRequest = setupSimpleAbsenceRequest(person,absence);

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleRequestDirectlyWhenRequestShorterThan24HoursAndEndsWithin24HourWindow()
		{
			var scenario = ScenarioRepository.Has("Default");
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet,absence,_today);

			workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(9)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills(person);

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			QueuedAbsenceRequestRepository.LoadAll().Should().Be.Empty();
			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDenyWhenPersonHasNoWorkflowControlSet()
		{
			var scenario = ScenarioRepository.Has("Default");
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();
			person.WorkflowControlSet = null;

			var personRequest = setupSimpleAbsenceRequest(person, absence);

			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(nameof(Resources.RequestDenyReasonNoWorkflow));
		}

		[Test]
		public void ShouldNotDenyWhenPersonHasZeroBalanceButWorkflowControlSetHasNoPersonAccountValidation()
		{
			var scenario = ScenarioRepository.Has("Default");
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldDenyExpiredRequest([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, 15, autoGrant);

			var personRequest = setupSimpleAbsenceRequest(person, absence);

			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(string.Format(Resources.RequestDenyReasonRequestExpired, personRequest.Request.Period.StartDateTime, 15));
		}

		[Test]
		public void ShouldDenyWhenAutoDenyIsOn()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, autoDeny: true);

			var personRequest = setupSimpleAbsenceRequest(person, absence);

			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be("RequestDenyReasonAutodeny");
		}

		[Test]
		public void ShouldDenyWhenAlreadyAbsent([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, autoGrant: autoGrant);

			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var form = createAbsenceRequestForm(absence, dateTimePeriodForm);

			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario
				, _today.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
					, UserTimeZone.TimeZone()), absence).WithId());

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyWhenAlreadyAbsentAndAbsenceStartsTheDayBefore([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var alreadyAbsentPeriod = _today.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
												  , UserTimeZone.TimeZone()).ChangeStartTime(TimeSpan.FromDays(-1));

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, alreadyAbsentPeriod));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, autoGrant: autoGrant);

			var form = createAbsenceRequestForm(absence, dateTimePeriodForm);

			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario
				, alreadyAbsentPeriod, absence).WithId());

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldNotDenyWhenAbsentTheDayBeforeAndShiftSpansOverMidnight([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			var requestDate = _today.AddDays(2);
			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = requestDate,
				EndDate = requestDate,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var shiftPeriod = requestDate.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
												  , UserTimeZone.TimeZone()).ChangeStartTime(TimeSpan.FromDays(-1));

			var alreadyAbsentPeriod = requestDate.AddDays(-1).ToDateTimePeriod(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
												  , UserTimeZone.TimeZone());

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, shiftPeriod));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, autoGrant: autoGrant);

			var form = createAbsenceRequestForm(absence, dateTimePeriodForm);

			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario
				, alreadyAbsentPeriod, absence).WithId());
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be("");
		}

		[Test]
		public void ShouldDenyWhenUpdateAbsenceOutOfDate()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, _today),
				Period = new DateOnlyPeriod(_today, _today),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
			});
			setupPersonSkills(person);

			var newPersonRequest = setupSimpleAbsenceRequest(person, absence);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.IsDenied.Should().Be(true);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountDaysAreExceeded([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var isWaitlisted = autoGrant;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(1)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be.True();
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldNotCountDaysDisabledInContractScheduleAndUnscheduled()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var saturday = _today.AddDays(4);
			setupPersonSkillAlwaysOpen(person, saturday);
			var absence = createAbsence();

			var isWaitlisted = true;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = saturday,
				EndDate = saturday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(new TimeSpan(18, 0, 0))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.DenyReason.Should().Be.Empty();
			request.IsDenied.Should().Be.False();
			request.IsWaitlisted.Should().Be.False();
			request.IsApproved.Should().Be.False();
			request.IsPending.Should().Be.True();

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo<ApproveRequestCommand>();
		}

		[Test]
		public void ShouldCountDaysDisabledInContractScheduleButScheduled()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			setupPersonSkills(person);
			var saturday = _today.AddDays(4);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, saturday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var isWaitlisted = false;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = saturday,
				EndDate = saturday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(new TimeSpan(8, 0, 0))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.DenyReason.Should().Not.Be.Empty();
			request.IsDenied.Should().Be.True();
			request.IsWaitlisted.Should().Be.False();
			request.IsApproved.Should().Be.False();
			request.IsPending.Should().Be.False();
		}

		[Test]
		public void ShouldNotCountAbsenceTimeOnContractDaysOff()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			setupPersonSkillAlwaysOpen(person, _today);
			var thursday = _today.AddDays(2); // 20oct 2016
			var friday = _today.AddDays(3);
			var sunday = _today.AddDays(5);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, thursday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var isWaitlisted = false;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(22)
			};
			createPersonAbsenceAccount(person, absence, accountDay);
			var period = new DateTimePeriodForm
			{
				StartDate = friday,
				EndDate = sunday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			};
			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Subject = "test",
				Period = period
			};
			
			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.DenyReason.Should().Be.Empty();
			request.IsDenied.Should().Be.False();
			request.IsWaitlisted.Should().Be.False();
			request.IsApproved.Should().Be.False();
			request.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldNotCountAbsenceTimeOnContractDaysOffWithDayOff()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			setupPersonSkillAlwaysOpen(person, _today);
			var thursday = _today.AddDays(2); // 20oct 2016
			var friday = _today.AddDays(3);
			var saturday = _today.AddDays(4);
			var sunday = _today.AddDays(5);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, thursday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, saturday, new DayOffTemplate(new Description("test"))));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, sunday, new DayOffTemplate(new Description("test"))));
			var isWaitlisted = false;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(22)
			};
			createPersonAbsenceAccount(person, absence, accountDay);
			var period = new DateTimePeriodForm
			{
				StartDate = friday,
				EndDate = sunday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			};
			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Subject = "test",
				Period = period
			};
			
			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.DenyReason.Should().Be.Empty();
			request.IsDenied.Should().Be.False();
			request.IsWaitlisted.Should().Be.False();
			request.IsApproved.Should().Be.False();
			request.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenPersonAccountTimeIsExceeded([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var isWaitlisted = autoGrant;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountTime1 = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromMinutes(60)
			};
			var accountTime2 = new AccountTime(_today.AddDays(1))
			{
				Accrued = TimeSpan.FromMinutes(20)
			};
			createPersonAbsenceAccount(person, absence, accountTime1, accountTime2);

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountIsMissing([Values]bool autoGrant)
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var isWaitlisted = autoGrant;
			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);
			absence.Tracker = Tracker.CreateDayTracker();

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be.True();
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfItsSameAsRequest()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);
			var newPersonRequest = setupSimpleAbsenceRequest(person, absence);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = newPersonRequest.Id.GetValueOrDefault(),
				StartDateTime = newPersonRequest.Request.Period.StartDateTime,
				EndDateTime = newPersonRequest.Request.Period.EndDateTime
			});

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			form.EntityId = newPersonRequest.Id.GetValueOrDefault();
			setupPersonSkills(person);

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(8)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(17)));
			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfNoStaffingValidatorIsUsed()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);
			workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new AbsenceRequestNoneValidator();

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills(person);

			var result = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			form.EntityId = result.Id;
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfIntradayRequestAndStaffingThresholdValidatorEnabled()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);
			workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills(person);

			var result = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			form.EntityId = result.Id;
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test, Ignore("we need to handle no toggle")]
		public void ShouldUpdateQueuedRequestForNonIntradayRequest()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);
			workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(2),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills(person);

			var result = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			form.EntityId = result.Id;
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.True();
			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.AddDays(2).Add(TimeSpan.FromHours(1)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.AddDays(2).Add(TimeSpan.FromHours(21)));
		}

		[Test]
		public void ShouldUpdateQueuedRequestForBudgetGroupValidator()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);
			workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new BudgetGroupAllowanceValidator();
			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills(person);

			var result = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			form.EntityId = result.Id;
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.True();
			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(1)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(21)));
		}

		[Test]
		public void ShouldApproveFullDayRequestWhenPreviousDayIsAbsentWithCrossDayShift()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			var startDateTime = new DateTime(2017, 3, 21, 0, 0, 0, DateTimeKind.Utc);

			var absence = createAbsence();

			// create the first day night shift
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, new DateTimePeriod(startDateTime.AddHours(23), startDateTime.AddDays(1).AddHours(7))));

			// create cross day night shift
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, new DateTimePeriod(startDateTime.AddDays(1).AddHours(23), startDateTime.AddDays(2).AddHours(7))));

			setWorkflowControlSet(workflowControlSet, absence, _today, autoGrant: true);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = new DateOnly(startDateTime.AddDays(1)),
				EndDate = new DateOnly(startDateTime.AddDays(1)),
				StartTime = new TimeOfDay(TimeSpan.Zero),
				EndTime = new TimeOfDay(TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1)))
			});

			// create cross day absence
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario
				, new DateTimePeriod(startDateTime.AddHours(23), startDateTime.AddDays(1).AddHours(7)), absence).WithId());

			setupPersonSkills(person);

			var personRequest1 = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			personRequest1.IsDenied.Should().Be(false);
			personRequest1.IsPending.Should().Be(true);
		}

		[Test]
		public void ShouldApproveRequestWhenChangingToAutoGrant()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence1 = createAbsence();
			var absence2 = createAbsence("absence2");

			var absenceRequestAutoGrantOnProcess = (IProcessAbsenceRequest)new GrantAbsenceRequest();
			var absenceRequestAutoGrantOffProcess = new PendingAbsenceRequest();

			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence1,
				AbsenceRequestProcess = absenceRequestAutoGrantOnProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, _today.AddDays(10)),
				Period = new DateOnlyPeriod(_today, _today.AddDays(10)),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});

			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence2,
				AbsenceRequestProcess = absenceRequestAutoGrantOffProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-14), _today.AddDays(30)),
				Period = new DateOnlyPeriod(_today.AddDays(-14), _today.AddDays(30)),
				PersonAccountValidator = new PersonAccountBalanceValidator()
			});

			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today.AddDays(15),
				EndDate = _today.AddDays(15),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};
			workflowControlSet.AbsenceRequestOpenPeriods[1].StaffingThresholdValidator = new StaffingThresholdValidator();

			var form = createAbsenceRequestForm(dateTimePeriodForm, absence2.Id.Value);
			form.EntityId = null;
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsPending.Should().Be(true);

			dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			form = createAbsenceRequestForm(dateTimePeriodForm, absence1.Id.Value);
			form.EntityId = personRequest.Id;
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDenyOffHourRequest()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);

			setupPersonSkills(person);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonNoPersonSkillOpen);
			request.IsWaitlisted.Should().Be.False();
		}

		[Test]
		public void ShouldApproveWithOvernightSkillOpenHour()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenNextDayIsDayoffAndPersonalAccountIsEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(person
				, scenario, _today.AddDays(1), new DayOffTemplate(new Description("test"))));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenPersonalAccountIsEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenTodayIsEmpty()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenTodayIsEmptyInStockholmTimeZone()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(1).ToDateTimePeriod(timeZone)));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(2).ToDateTimePeriod(timeZone)));

			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(480),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(20)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			});
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.IsDenied.Should().Be(false);
		}

		[Test]
		public void ShouldDenyOvernightRequestWhenFirstDayRequestTimeEqualsRemainingTimeAndPersonalAccountIsNotEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyOvernightRequestWhenFirstDayRequestTimeLessThanRemainingTimeAndPersonalAccountIsNotEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(15))
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldApproveOvernightRequestWhenNextDayIsScheduledAndPersonalAccountIsEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.AddDays(1).ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromHours(20)),
					UserTimeZone.TimeZone())));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldDenyFullDayRequestWhenTodayHasOvernightScheduleAndPersonalAccountIsNotEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario,
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(2))),
					UserTimeZone.TimeZone())));

			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(9)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(24))
			});
			form.FullDay = true;

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyFullDayRequestWhenTodayHasNoScheduleAndPersonalAccountIsNotEnough()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(7)
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(person, skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(24))
			});
			form.FullDay = true;

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestWhenSkillOpenHourIsClosedOnPreviousDay()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Wednesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod}
			});
			var date = new DateOnly(2016, 10, 18);
			person.AddSkill(skill, date);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
			request.DenyReason.Should(Resources.RequestDenyReasonNoPersonSkillOpen);
		}

		[Test]
		public void ShouldApproveRequestWhenOvernightSkillOpenHourIsOpenOnToday()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			ScenarioRepository.Has("Default");
			var absence = createAbsence();
			setWorkflowControlSet(workflowControlSet, absence, _today);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Tuesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod}
			});
			var date = new DateOnly(2016, 10, 18);
			person.AddSkill(skill, date);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldNotDenyRequestWhenPeriodIsOutsideSkillOpenHoursAndOnSkillessActivity()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true);

			setupPersonSkills(person);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(6)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(7))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldDenyWhenRemainingHourIsNotEnoughOnEmptyDay()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			var absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(60),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenRemainingHourIsNotEnoughForMultipleEmptyDays()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(2).ToDateTimePeriod(UserTimeZone.TimeZone())));

			var absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(480),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldApproveWhenRemainingHourIsEnoughOnEmptyDay()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			var absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(60),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(person, absence, accountTime);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromMinutes(480)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(510))
			});
			setupPersonSkills(person);

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be(string.Empty);
		}

		[Test]
		public void ShouldDenyOnTechnicalIssuesWhenNoSkillCombinations()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			setWorkflowControlSet(workflowControlSet, absence, _today);

			workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(9)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			setupPersonSkills(person);

			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
			((DenyRequestCommand)CommandDispatcher.LatestCommand).DenyReason.Should().Be
				.EqualTo(Resources.DenyReasonNoSkillCombinationsFound);
		}

		[Test]
		public void ShouldUsePersonTimezoneWhenCheckingRequest47148()
		{
			Now.Is(new DateTime(2017, 12, 09, 1, 0, 0, DateTimeKind.Utc));
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			var today = new DateOnly(2017, 12, 08);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var absenceRequestProcess = new GrantAbsenceRequest();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = absenceRequestProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 12, 1, 2017, 12, 31),
				BetweenDays = new MinMax<int>(0, 15),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = today,
				EndDate = today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(16)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			setupPersonSkills(person);
			Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyWhenPersonAccountIsOutsidePeriod()
		{
			Now.Is(nowTime);
			var person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var workflowControlSet = new WorkflowControlSet().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Has(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			var absence = createAbsence();

			var isWaitlisted = false;

			setWorkflowControlSet(workflowControlSet, absence, _today, usePersonAccountValidator: true, autoGrant: false, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today.AddDays(10))
			{
				Accrued = TimeSpan.FromDays(100)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			var request = PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be.True();

			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		private static void setWorkflowControlSet(IWorkflowControlSet workflowControlSet, IAbsence absence, DateOnly today, int? absenceRequestExpiredThreshold = null, bool autoGrant = false
			, bool usePersonAccountValidator = false, bool autoDeny = false, bool absenceRequestWaitlistEnabled = false)
		{
			workflowControlSet.AbsenceRequestWaitlistEnabled = absenceRequestWaitlistEnabled;
			workflowControlSet.AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold;
			var absenceRequestProcess = autoGrant
				? (IProcessAbsenceRequest)new GrantAbsenceRequest()
				: new PendingAbsenceRequest();
			if (autoDeny)
				absenceRequestProcess = new DenyAbsenceRequest();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				AbsenceRequestProcess = absenceRequestProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(today, DateOnly.Today.AddDays(30)),
				Period = new DateOnlyPeriod(today, DateOnly.Today.AddDays(30)),
				PersonAccountValidator = usePersonAccountValidator ? (IAbsenceRequestValidator)new PersonAccountBalanceValidator() : new AbsenceRequestNoneValidator()
			});
		}

		private IPersonRequest setupSimpleAbsenceRequest(IPerson person, IAbsence absence)
		{
			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(1)
			};
			createPersonAbsenceAccount(person, absence, accountDay);

			var form = createAbsenceRequestForm(absence, new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = Target.Persist(form.ToModel(UserTimeZone, LoggedOnUser));
			return PersonRequestRepository.Get(personRequest.Id.GetValueOrDefault());
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

		private AbsenceRequestForm createAbsenceRequestForm(IAbsence absence, DateTimePeriodForm period)
		{
			var form = new AbsenceRequestForm
			{
				AbsenceId = absence.Id.Value,
				Subject = "test",
				Period = period
			};
			return form;
		}

		private AbsenceRequestForm createAbsenceRequestForm(DateTimePeriodForm period, Guid absenceId)
		{
			var form = new AbsenceRequestForm
			{
				AbsenceId = absenceId,
				Subject = "test",
				Period = period
			};
			form.EntityId = absenceId;
			return form;
		}

		private IAbsence createAbsence(string name = "holiday")
		{
			var absence = AbsenceFactory.CreateAbsence(name).WithId();
			absence.Description = new Description("desc" + name);
			AbsenceRepository.Add(absence);
			return absence;
		}

		private void setupPersonSkills(IPerson person)
		{
			var skill = SkillFactory.CreateSkill("Phone");
			setupPersonSkills(person, skill);
		}

		private void setupPersonSkills(IPerson person, ISkill skill, TimePeriod? skillOpenPeriod = null)
		{
			var timePeriods = skillOpenPeriod.HasValue ? new[] { skillOpenPeriod.Value } : Enumerable.Repeat(new TimePeriod(8, 18), 5).ToArray();
			WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, timePeriods);
			var date = new DateOnly(2016, 10, 18);
			person.AddSkill(skill, date);
		}

		private void setupPersonSkillAlwaysOpen(IPerson person, DateOnly personPeriodDate)
		{
			var skill = SkillFactory.CreateSkill("Phone");
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			person.AddSkill(skill, personPeriodDate);
		}
	}
}
