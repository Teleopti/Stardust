using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, RequestsTest]
	public class AbsenceRequestPersisterNoMocksTest : ISetup
	{
		public IPersonRequestRepository PersonRequestRepository;
		public ILoggedOnUser LoggedOnUser;
		public IUserTimeZone UserTimeZone;
		public IAbsenceRepository AbsenceRepository;
		public IEventPublisher EventPublisher;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public ICurrentDataSource CurrentDataSource;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario CurrentScenario;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;

		private static readonly DateTime _nowTime = new DateTime(2016, 10, 18, 8, 0, 0, DateTimeKind.Utc);
		private INow _now = new ThisIsNow(_nowTime);
		private DateOnly _today = new DateOnly(_nowTime);
		private AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest _absenceRequestFormToPersonRequest;
		private RequestsViewModelMappingProfile _requestsViewModelMappingProfile;
		private IWorkflowControlSet _workflowControlSet;
		private IAbsence _absence;
		private IPerson _person;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeAbsenceRepository()).For<IAbsenceRepository>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			system.UseTestDouble(new FakeCurrentDatasource("test")).For<ICurrentDataSource>();
		}

		[Test]
		public void ShouldAddRequest()
		{
			setUp();

			setWorkflowControlSet(usePersonAccountValidator:true);

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

			var personRequest = persist(form);
			assertPersonRequest(personRequest, false, string.Empty);
		}

		[Test]
		public void ShouldDenyExpiredRequest([Values]bool autoGrant)
		{
			setUp();

			setWorkflowControlSet(15, autoGrant);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = _today,
				EndDate = _today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = persist(form);
			assertPersonRequest(personRequest, true
				, string.Format(Resources.RequestDenyReasonRequestExpired, personRequest.Request.Period.StartDateTime, 15));
		}

		[Test]
		public void ShouldDenyWhenAlreadyAbsent([Values]bool autoGrant)
		{
			setUp();

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

			var personRequest = persist(form);
			assertPersonRequest(personRequest, true, Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyWhenAutoDenyIsOn()
		{
			setUp();

			setWorkflowControlSet(autoDeny: true);

			var form = createAbsenceRequestForm(new DateTimePeriodForm
			{
				StartDate = DateOnly.Today,
				EndDate = DateOnly.Today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(17))
			});

			var personRequest = persist(form);
			assertPersonRequest(personRequest, true, "RequestDenyReasonAutodeny");
		}

		[Test]
		public void ShouldDenyWhenPersonAccountDaysAreExceeded([Values]bool autoGrant)
		{
			setUp();

			setWorkflowControlSet(usePersonAccountValidator:true, autoGrant:autoGrant);

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

			var personRequest = persist(form);
			var isWaitlisted = autoGrant;
			assertPersonRequest(personRequest, true, Resources.RequestDenyReasonPersonAccount, isWaitlisted);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountTimeIsExceeded([Values]bool autoGrant)
		{
			setUp();

			setWorkflowControlSet(usePersonAccountValidator: true, autoGrant: autoGrant);

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

			var personRequest = persist(form);
			var isWaitlisted = autoGrant;
			assertPersonRequest(personRequest, true, Resources.RequestDenyReasonPersonAccount, isWaitlisted);
		}

		private void setUp()
		{
			_absenceRequestFormToPersonRequest =
				new AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest(() => Mapper.Engine, () => LoggedOnUser, () => AbsenceRepository, () => UserTimeZone);

			_requestsViewModelMappingProfile = new RequestsViewModelMappingProfile(UserTimeZone
				, null, LoggedOnUser, null, null,
				null);

			Mapper.Reset();
			Mapper.Initialize(c =>
			{
				c.AddProfile(new AbsenceRequestFormMappingProfile(() => _absenceRequestFormToPersonRequest));
				c.AddProfile(new DateTimePeriodFormMappingProfile(() => UserTimeZone));
				c.AddProfile(_requestsViewModelMappingProfile);
			});

			((FakeScheduleDataReadScheduleStorage)ScheduleStorage).Clear();

			_person = LoggedOnUser.CurrentUser();

			ScheduleStorage.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(
				CurrentScenario.Current(), _person
				, _today.ToDateTimePeriod(UserTimeZone.TimeZone())));

			((FakeCurrentBusinessUnit)CurrentBusinessUnit).FakeBusinessUnit(new BusinessUnit("test"));
			_workflowControlSet = new WorkflowControlSet().WithId();
			_absence = createAbsence();
		}

		private void setWorkflowControlSet(int? absenceRequestExpiredThreshold = null, bool autoGrant = false
			, bool usePersonAccountValidator = false, bool autoDeny = false)
		{
			_workflowControlSet.AbsenceRequestWaitlistEnabled = true;
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
			
			_person.WorkflowControlSet = _workflowControlSet;
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

		private IPersonRequest persist(AbsenceRequestForm form)
		{
			var absenceRequestPersonAccountValidator = new AbsenceRequestPersonAccountValidator(
				new PersonAbsenceAccountProvider(PersonAbsenceAccountRepository));
			var absenceRequestSynchronousValidator =
				new AbsenceRequestSynchronousValidator(new ExpiredRequestValidator(new FakeGlobalSettingDataRepository(), _now),
					new AlreadyAbsentValidator(), ScheduleStorage, CurrentScenario, new AbsenceRequestWorkflowControlSetValidator(), absenceRequestPersonAccountValidator);
			var target = new AbsenceRequestPersister(PersonRequestRepository, Mapper.Engine, EventPublisher
				, CurrentBusinessUnit, CurrentDataSource, _now, new ThisUnitOfWork(new FakeUnitOfWork())
				, absenceRequestSynchronousValidator, new PersonRequestAuthorizationCheckerForTest());
			var requestViewModel = target.Persist(form);
			return PersonRequestRepository.Get(new Guid(requestViewModel.Id));
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
			AbsenceRepository.Add(absence);
			return absence;
		}

		private void assertPersonRequest(IPersonRequest personRequest, bool isDenied, string denyReason = null, bool isWaitlisted = false)
		{
			personRequest.Should().Not.Be(null);
			personRequest.IsDenied.Should().Be(isDenied);
			personRequest.IsWaitlisted.Should().Be(isWaitlisted);
			personRequest.DenyReason.Should().Be(denyReason);
			publishedEventsCountShouldBe(isDenied ? 0 : 1);
		}

		private void publishedEventsCountShouldBe(int count)
		{
			((FakeEventPublisher)EventPublisher).PublishedEvents.Count().Should().Be(count);
		}
	}
}
