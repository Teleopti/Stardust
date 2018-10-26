﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
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
	class FakeDisableDeletedFilter : IDisableDeletedFilter
	{
		public IDisposable Disable()
		{
			return null;
		}
	}

	[TestFixture]
	[DomainTest]
	[WebTest]
	[DefaultData]
	public class AbsenceRequestPersisterTest : IIsolateSystem
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IUserTimeZone UserTimeZone;
		public IAbsenceRepository AbsenceRepository;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestPersister Persister;
		public FakeCommandDispatcher CommandDispatcher;

		private static readonly DateTime nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private DateOnly _today = new DateOnly(nowTime);
		private IWorkflowControlSet _workflowControlSet;
		private IAbsence _absence;
		private IPerson _person;
		private ThisIsNow _now;

		public void Isolate(IIsolate isolate)
		{
			_person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			_person.PersonPeriodCollection[0].PersonContract.ContractSchedule = new ContractScheduleWorkingMondayToFriday();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			_workflowControlSet = new WorkflowControlSet().WithId();
			_person.WorkflowControlSet = _workflowControlSet;

			var personRepository = new FakePersonRepositoryLegacy { _person };
			_now = new ThisIsNow(nowTime);

			isolate.UseTestDouble(personRepository).For<IPersonRepository>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			isolate.UseTestDouble(_now).For<INow>();
			isolate.UseTestDouble<AbsenceRequestFormMapper>().For<AbsenceRequestFormMapper>();
			isolate.UseTestDouble<RequestsViewModelMapper>().For<RequestsViewModelMapper>();
			isolate.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			isolate.UseTestDouble<FakeDisableDeletedFilter>().For<IDisableDeletedFilter>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			var tenants = new FakeTenants();
			var DefaultTenantName = "default";
			tenants.Has(DefaultTenantName, "a key");
			isolate.UseTestDouble(tenants)
				.For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();

		}

		[Test]
		public void ShouldAddRequest()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: true);

			setupPersonSkills();
			var personRequest = setupSimpleAbsenceRequest();

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleRequestDirectlyWhenRequestShorterThan24HoursAndEndsWithin24HourWindow()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet();

			_workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(9)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills();

			Persister.Persist(form);

			QueuedAbsenceRequestRepository.LoadAll().Should().Be.Empty();
			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDenyWhenPersonHasNoWorkflowControlSet()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();
			_person.WorkflowControlSet = null;

			var personRequest = setupSimpleAbsenceRequest();

			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be(nameof(Resources.RequestDenyReasonNoWorkflow));
		}

		[Test]
		public void ShouldNotDenyWhenPersonHasZeroBalanceButWorkflowControlSetHasNoPersonAccountValidation()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet();

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
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldDenyExpiredRequest([Values]bool autoGrant)
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
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
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
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
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
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
		public void ShouldDenyWhenAlreadyAbsentAndAbsenceStartsTheDayBefore([Values]bool autoGrant)
		{
			var dateTimePeriodForm = new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var alreadyAbsentPeriod = _today.ToDateTimePeriod(new TimePeriod(dateTimePeriodForm.StartTime.Time, dateTimePeriodForm.EndTime.Time)
												  , UserTimeZone.TimeZone()).ChangeStartTime(TimeSpan.FromDays(-1));

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), alreadyAbsentPeriod));
			_absence = createAbsence();

			setWorkflowControlSet(autoGrant: autoGrant);

			var form = createAbsenceRequestForm(dateTimePeriodForm);

			ScheduleStorage.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, CurrentScenario.Current()
				, alreadyAbsentPeriod, _absence).WithId());

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldNotDenyWhenAbsentTheDayBeforeAndShiftSpansOverMidnight([Values]bool autoGrant)
		{
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

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), shiftPeriod));
			_absence = createAbsence();

			setWorkflowControlSet(autoGrant: autoGrant);

			var form = createAbsenceRequestForm(dateTimePeriodForm);

			ScheduleStorage.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, CurrentScenario.Current()
				, alreadyAbsentPeriod, _absence).WithId());
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be("");
		}

		[Test]
		public void ShouldDenyWhenUpdateAbsenceOutOfDate()
		{
			_absence = createAbsence();
			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = _absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, _today),
				Period = new DateOnlyPeriod(_today, _today),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
			});
			setupPersonSkills();

			var newPersonRequest = setupSimpleAbsenceRequest();

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});
			form.EntityId = newPersonRequest.Id.GetValueOrDefault();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.IsDenied.Should().Be(true);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountDaysAreExceeded([Values]bool autoGrant)
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = autoGrant;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);

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
			request.IsDenied.Should().Be.True();
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldNotCountDaysDisabledInContractScheduleAndUnscheduled()
		{
			var saturday = _today.AddDays(4);
			setupPersonSkillAlwaysOpen(saturday);
			_absence = createAbsence();

			var isWaitlisted = true;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = saturday,
				EndDate = saturday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(new TimeSpan(18, 0, 0))
			});

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

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
			setupPersonSkills();
			var saturday = _today.AddDays(4);
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), saturday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = false;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today)
			{
				Accrued = TimeSpan.FromDays(0)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = saturday,
				EndDate = saturday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(new TimeSpan(8, 0, 0))
			});

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

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
			setupPersonSkillAlwaysOpen(_today);
			var thursday = _today.AddDays(2); // 20oct 2016
			var friday = _today.AddDays(3);
			var saturday = _today.AddDays(4);
			var sunday = _today.AddDays(5);
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), thursday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var isWaitlisted = false;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(22)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);
			var period = new DateTimePeriodForm
			{
				StartDate = friday,
				EndDate = sunday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			};
			var form = new AbsenceRequestForm
			{
				AbsenceId = _absence.Id.Value,
				Subject = "test",
				Period = period
			};



			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

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
			setupPersonSkillAlwaysOpen(_today);
			var thursday = _today.AddDays(2); // 20oct 2016
			var friday = _today.AddDays(3);
			var saturday = _today.AddDays(4);
			var sunday = _today.AddDays(5);
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), thursday.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, CurrentScenario.Current(), saturday, new DayOffTemplate(new Description("test"))));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, CurrentScenario.Current(), sunday, new DayOffTemplate(new Description("test"))));
			var isWaitlisted = false;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(22)
			};
			createPersonAbsenceAccount(_person, _absence, accountDay);
			var period = new DateTimePeriodForm
			{
				StartDate = friday,
				EndDate = sunday,
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			};
			var form = new AbsenceRequestForm
			{
				AbsenceId = _absence.Id.Value,
				Subject = "test",
				Period = period
			};



			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

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
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
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

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

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
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountIsMissing([Values]bool autoGrant)
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = autoGrant;
			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: autoGrant, absenceRequestWaitlistEnabled: isWaitlisted);
			_absence.Tracker = Tracker.CreateDayTracker();

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
			request.IsDenied.Should().Be.True();
			request.IsWaitlisted.Should().Be.False();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfItsSameAsRequest()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
			var newPersonRequest = setupSimpleAbsenceRequest();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
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
			setupPersonSkills();

			Persister.Persist(form);

			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(8)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(17)));
			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfNoStaffingValidatorIsUsed()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
			_workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new AbsenceRequestNoneValidator();

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills();

			var result = Persister.Persist(form);

			form.EntityId = Guid.Parse(result.Id);
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Persister.Persist(form);

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateQueuedRequestPeriodIfIntradayRequestAndStaffingThresholdValidatorEnabled()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
			_workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills();

			var result = Persister.Persist(form);

			form.EntityId = Guid.Parse(result.Id);
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Persister.Persist(form);

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.False();
		}

		[Test, Ignore("we need to handle no toggle")]
		public void ShouldUpdateQueuedRequestForNonIntradayRequest()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
			_workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(2),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills();

			var result = Persister.Persist(form);

			form.EntityId = Guid.Parse(result.Id);
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Persister.Persist(form);

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.True();
			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.AddDays(2).Add(TimeSpan.FromHours(1)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.AddDays(2).Add(TimeSpan.FromHours(21)));
		}

		[Test]
		public void ShouldUpdateQueuedRequestForBudgetGroupValidator()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
			_workflowControlSet.AbsenceRequestOpenPeriods.ElementAt(0).StaffingThresholdValidator = new BudgetGroupAllowanceValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(21))
			});

			setupPersonSkills();

			var result = Persister.Persist(form);

			form.EntityId = Guid.Parse(result.Id);
			form.Period.StartTime = new TimeOfDay(TimeSpan.FromHours(1));
			Persister.Persist(form);

			QueuedAbsenceRequestRepository.UpdateRequestPeriodWasCalled.Should().Be.True();
			var queuedRequest = QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(1)));
			queuedRequest.EndDateTime.Should().Be.EqualTo(_today.Date.Add(TimeSpan.FromHours(21)));
		}

		[Test]
		public void ShouldApproveFullDayRequestWhenPreviousDayIsAbsentWithCrossDayShift()
		{
			var startDateTime = new DateTime(2017, 3, 21, 0, 0, 0, DateTimeKind.Utc);

			_absence = createAbsence();

			// create the first day night shift
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), new DateTimePeriod(startDateTime.AddHours(23), startDateTime.AddDays(1).AddHours(7))));

			// create cross day night shift
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), new DateTimePeriod(startDateTime.AddDays(1).AddHours(23), startDateTime.AddDays(2).AddHours(7))));

			setWorkflowControlSet(autoGrant: true);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = new DateOnly(startDateTime.AddDays(1)),
				EndDate = new DateOnly(startDateTime.AddDays(1)),
				StartTime = new TimeOfDay(TimeSpan.Zero),
				EndTime = new TimeOfDay(TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1)))
			});

			// create cross day absence
			ScheduleStorage.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, CurrentScenario.Current()
				, new DateTimePeriod(startDateTime.AddHours(23), startDateTime.AddDays(1).AddHours(7)), _absence).WithId());

			setupPersonSkills();

			var personRequest1 = Persister.Persist(form);

			personRequest1.IsDenied.Should().Be(false);
			personRequest1.IsPending.Should().Be(true);
		}

		[Test]
		public void ShouldApproveRequestWhenChangingToAutoGrant()
		{
			var absence1 = createAbsence();
			var absence2 = createAbsence("absence2");

			var absenceRequestAutoGrantOnProcess = (IProcessAbsenceRequest)new GrantAbsenceRequest();
			var absenceRequestAutoGrantOffProcess = new PendingAbsenceRequest();

			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence1,
				AbsenceRequestProcess = absenceRequestAutoGrantOnProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, _today.AddDays(10)),
				Period = new DateOnlyPeriod(_today, _today.AddDays(10)),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});

			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
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
			_workflowControlSet.AbsenceRequestOpenPeriods[1].StaffingThresholdValidator = new StaffingThresholdValidator();

			var form = createAbsenceRequestForm(dateTimePeriodForm, absence2.Id.Value);
			var formId = form.EntityId;
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

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
			form.EntityId = formId;
			Persister.Persist(form);

			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDenyOffHourRequest()
		{
			_absence = createAbsence();
			setWorkflowControlSet();

			setupPersonSkills();

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(21))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
			request.DenyReason.Should().Be(Resources.RequestDenyReasonNoPersonSkillOpen);
			request.IsWaitlisted.Should().Be.False();
		}

		[Test]
		public void ShouldApproveWithOvernightSkillOpenHour()
		{
			_absence = createAbsence();
			setWorkflowControlSet();

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenNextDayIsDayoffAndPersonalAccountIsEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person
				, CurrentScenario.Current(), _today.AddDays(1), new DayOffTemplate(new Description("test"))));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenPersonalAccountIsEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenTodayIsEmpty()
		{
			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldApproveOvernightAbsenceRequestWhenTodayIsEmptyInStockholmTimeZone()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(1).ToDateTimePeriod(timeZone)));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(2).ToDateTimePeriod(timeZone)));

			_person.PermissionInformation.SetDefaultTimeZone(timeZone);
			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			_absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(480),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(20)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(4))
			});
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.IsDenied.Should().Be(false);
		}

		[Test]
		public void ShouldDenyOvernightRequestWhenFirstDayRequestTimeEqualsRemainingTimeAndPersonalAccountIsNotEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(22)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyOvernightRequestWhenFirstDayRequestTimeLessThanRemainingTimeAndPersonalAccountIsNotEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(15))
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldApproveOvernightRequestWhenNextDayIsScheduledAndPersonalAccountIsEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1))),
					UserTimeZone.TimeZone())));

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.AddDays(1).ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromHours(20)),
					UserTimeZone.TimeZone())));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(2)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(23)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldDenyFullDayRequestWhenTodayHasOvernightScheduleAndPersonalAccountIsNotEnough()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(),
				_today.ToDateTimePeriod(
					new TimePeriod(TimeSpan.FromHours(16), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(2))),
					UserTimeZone.TimeZone())));

			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(9)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(24))
			});
			form.FullDay = true;

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyFullDayRequestWhenTodayHasNoScheduleAndPersonalAccountIsNotEnough()
		{
			_absence = createAbsence();
			_absence.Tracker = Tracker.CreateTimeTracker();

			var accountTime = new AccountTime(_today)
			{
				Accrued = TimeSpan.FromHours(7)
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			setWorkflowControlSet(usePersonAccountValidator: true);

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			setupPersonSkills(skill, skillOpenPeriod);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(24))
			});
			form.FullDay = true;

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestWhenSkillOpenHourIsClosedOnPreviousDay()
		{
			_absence = createAbsence();
			setWorkflowControlSet();

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Wednesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod}
			});
			var date = new DateOnly(2016, 10, 18);
			_person.AddSkill(skill, date);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.True();
			request.DenyReason.Should(Resources.RequestDenyReasonNoPersonSkillOpen);
		}

		[Test]
		public void ShouldApproveRequestWhenOvernightSkillOpenHourIsOpenOnToday()
		{
			_absence = createAbsence();
			setWorkflowControlSet();

			var skillOpenPeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(25));
			var skill = SkillFactory.CreateSkill("Phone");
			skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Tuesday, skillOpenPeriod},
				{DayOfWeek.Thursday, skillOpenPeriod}
			});
			var date = new DateOnly(2016, 10, 18);
			_person.AddSkill(skill, date);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(0)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(1))
			});

			Persister.Persist(form);

			var request = PersonRequestRepository.LoadAll().FirstOrDefault();
			request.IsDenied.Should().Be.False();
		}

		[Test]
		public void ShouldNotDenyRequestWhenPeriodIsOutsideSkillOpenHoursAndOnSkillessActivity()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: true);

			setupPersonSkills();

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(6)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(7))
			});

			var personRequest = Persister.Persist(form);

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test]
		public void ShouldDenyWhenRemainingHourIsNotEnoughOnEmptyDay()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			_absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(60),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenRemainingHourIsNotEnoughForMultipleEmptyDays()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(2).ToDateTimePeriod(UserTimeZone.TimeZone())));

			_absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(480),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(2),
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(true);
			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldApproveWhenRemainingHourIsEnoughOnEmptyDay()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateEmptyAssignment(_person, CurrentScenario.Current(),
				_today.AddDays(1).ToDateTimePeriod(UserTimeZone.TimeZone())));

			_absence = createAbsence("Time Off In Lieu");
			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: true);

			var accountTime = new AccountTime(_today.AddDays(-1))
			{
				BalanceIn = TimeSpan.FromMinutes(0),
				Accrued = TimeSpan.FromMinutes(60),
				Extra = TimeSpan.FromMinutes(0),
				LatestCalculatedBalance = TimeSpan.Zero
			};
			createPersonAbsenceAccount(_person, _absence, accountTime);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today.AddDays(1),
				EndDate = _today.AddDays(1),
				StartTime = new TimeOfDay(TimeSpan.FromMinutes(480)),
				EndTime = new TimeOfDay(TimeSpan.FromMinutes(510))
			});
			setupPersonSkills();

			var personRequest = Persister.Persist(form);
			var request = PersonRequestRepository.Get(Guid.Parse(personRequest.Id));

			request.Should().Not.Be(null);
			request.IsDenied.Should().Be(false);
			request.DenyReason.Should().Be(string.Empty);
		}

		[Test]
		public void ShouldDenyOnTechnicalIssuesWhenNoSkillCombinations()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet();

			_workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(9)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			setupPersonSkills();

			Persister.Persist(form);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
			((DenyRequestCommand)CommandDispatcher.LatestCommand).DenyReason.Should().Be
				.EqualTo(Resources.DenyReasonNoSkillCombinationsFound);
		}

		[Test]
		public void ShouldUsePersonTimezoneWhenCheckingRequest47148()
		{
			_now = new ThisIsNow(new DateTime(2017, 12, 09, 1, 0, 0, DateTimeKind.Utc));
			ServiceLocatorForEntity.Now = _now;
			var today = new DateOnly(2017, 12, 08);
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var absenceRequestProcess = new GrantAbsenceRequest();
			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod()
			{
				Absence = _absence,
				AbsenceRequestProcess = absenceRequestProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 12, 1, 2017, 12, 31),
				BetweenDays = new MinMax<int>(0, 15),
				PersonAccountValidator = new AbsenceRequestNoneValidator()
			});

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = today,
				EndDate = today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(16)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			setupPersonSkills();
			Persister.Persist(form);
			CommandDispatcher.LatestCommand.Should().Not.Be.Null();
			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyWhenPersonAccountIsOutsidePeriod()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			var isWaitlisted = false;

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: false, absenceRequestWaitlistEnabled: isWaitlisted);

			var accountDay = new AccountDay(_today.AddDays(10))
			{
				Accrued = TimeSpan.FromDays(100)
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
			request.IsDenied.Should().Be.True();

			request.DenyReason.Should().Be(Resources.RequestDenyReasonPersonAccount);
		}

		private void setWorkflowControlSet(int? absenceRequestExpiredThreshold = null, bool autoGrant = false
			, bool usePersonAccountValidator = false, bool autoDeny = false, bool absenceRequestWaitlistEnabled = false)
		{
			_workflowControlSet.AbsenceRequestWaitlistEnabled = absenceRequestWaitlistEnabled;
			_workflowControlSet.AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold;
			var absenceRequestProcess = autoGrant
				? (IProcessAbsenceRequest)new GrantAbsenceRequest()
				: new PendingAbsenceRequest();
			if (autoDeny)
				absenceRequestProcess = new DenyAbsenceRequest();
			_workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = _absence,
				AbsenceRequestProcess = absenceRequestProcess,
				OpenForRequestsPeriod = new DateOnlyPeriod(_today, DateOnly.Today.AddDays(30)),
				Period = new DateOnlyPeriod(_today, DateOnly.Today.AddDays(30)),
				PersonAccountValidator = usePersonAccountValidator ? (IAbsenceRequestValidator)new PersonAccountBalanceValidator() : new AbsenceRequestNoneValidator()
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
			absence.SetBusinessUnit(BusinessUnitFactory.CreateWithId("temp" + name));
			AbsenceRepository.Add(absence);
			return absence;
		}

		private void setupPersonSkills()
		{
			var skill = SkillFactory.CreateSkill("Phone");
			setupPersonSkills(skill);
		}

		private void setupPersonSkills(ISkill skill, TimePeriod? skillOpenPeriod = null)
		{
			var timePeriods = skillOpenPeriod.HasValue ? new[] { skillOpenPeriod.Value } : Enumerable.Repeat(new TimePeriod(8, 18), 5).ToArray();
			WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, timePeriods);
			var date = new DateOnly(2016, 10, 18);
			_person.AddSkill(skill, date);
		}

		private void setupPersonSkillAlwaysOpen(DateOnly personPeriodDate)
		{
			var skill = SkillFactory.CreateSkill("Phone");
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			_person.AddSkill(skill, personPeriodDate);
		}
	}
}
