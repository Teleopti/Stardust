using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
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
	[TestFixture, RequestsTest, Toggle(Toggles.MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747)]
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
		public AbsenceRequestFormMapper AbsenceRequestFormToPersonRequest;
		public RequestsViewModelMapper RequestsViewModelMappingProfile;
		public FakeCommandDispatcher CommandDispatcher;
		
		private static readonly DateTime nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private DateOnly _today = new DateOnly(nowTime);
		private IWorkflowControlSet _workflowControlSet;
		private IAbsence _absence;
		private IPerson _person;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeTenants Tenants;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		private IBusinessUnit businessUnit;
		public FakePersonRepositoryLegacy PersonRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePersonWithPersonPeriod(_today.AddDays(-5)).WithId();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			_workflowControlSet = new WorkflowControlSet().WithId();
			_person.WorkflowControlSet = _workflowControlSet;

			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(_person);

			system.UseTestDouble(personRepository).For<IPersonRepository>();
			system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			system.UseTestDouble(new ThisIsNow(nowTime)).For<INow>();
			system.UseTestDouble<AbsenceRequestFormMapper>().For<AbsenceRequestFormMapper>();
			system.UseTestDouble<RequestsViewModelMapper>().For<RequestsViewModelMapper>();
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.UseTestDouble<FakeDisableDeletedFilter>().For<IDisableDeletedFilter>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();

			var tenants = new FakeTenants();
			var DefaultTenantName = "default";
			tenants.Has(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			system.UseTestDouble(tenants)
				.For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();

		}


        [Test]
		public void ShouldAddRequest()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: true);

			var personRequest = setupSimpleAbsenceRequest();

			personRequest.Should().Not.Be.Null();
			personRequest.IsDenied.Should().Be.False();
			personRequest.DenyReason.Should().Be.Empty();
		}

		[Test, Ignore("was already failing but we exposed it")]
		public void ShouldHandleRequestDirectlyWhenRequestShorterThan24HoursAndEndsWithin24HourWindow()
		{
			
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			
			CurrentBusinessUnit.FakeBusinessUnit(businessUnit);

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
			_absence = createAbsence();

			setWorkflowControlSet(usePersonAccountValidator: false);

			_workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new StaffingThresholdValidator();
			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(9)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

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
			personRequest.DenyReason.Should().Be(Resources.RequestDenyReasonNoWorkflow);
		}

		[Test]
		public void ShouldNotDenyWhenPersonHasZeroBalanceButWorkflowControlSetHasNoPersonAccountValidation()
		{
			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, CurrentScenario.Current(), _today.ToDateTimePeriod(UserTimeZone.TimeZone())));
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
			request.IsDenied.Should().Be(true);
			request.IsWaitlisted.Should().Be(isWaitlisted);
			request.DenyReason.Should().Be(isWaitlisted ? Resources.RequestWaitlistedReasonPersonAccount : Resources.RequestDenyReasonPersonAccount);
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
			request.IsWaitlisted.Should().Be(isWaitlisted);
			request.DenyReason.Should().Be(isWaitlisted ? Resources.RequestWaitlistedReasonPersonAccount : Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldUpdatePeriodForQueuedRequest()
		{
			_absence = createAbsence();
			setWorkflowControlSet();

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
		public void ShouldNotUpdateQueuedRequestPeriodIfItsSameAsRequest()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
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

		[Test, Ignore("Not valid")]
		public void ShouldThrowInvalidOperationIfQueuedRequestIsSent()
		{
			Assert.Throws<InvalidOperationException>(tryPersist);
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

			var personRequest1 = Persister.Persist(form);

			personRequest1.IsDenied.Should().Be(false);
			personRequest1.IsPending.Should().Be(true);
		}

		private void tryPersist()
		{
			_absence = createAbsence();
			setWorkflowControlSet();
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
