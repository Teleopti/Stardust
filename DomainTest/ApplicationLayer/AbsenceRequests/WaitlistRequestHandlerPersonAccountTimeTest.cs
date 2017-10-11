using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.DomainTest.Scheduling.Tracking;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[Toggle(Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767)]
	[Toggle(Toggles.MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747)]
	[Toggle(Toggles.Wfm_Requests_HandleFourteenDaysFast_43390)]
	public class WaitlistRequestHandlerPersonAccountTimeTest : ISetup
	{
		public WaitlistRequestHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeTenants Tenants;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public ResourceCalculationWithCount ResourceCalculation;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		private IBusinessUnit businessUnit;

		private IPersonRequest waitListedRequest;
		private IPersonRequest waitListedRequest2;
		private IAccount account;
		private IPerson waitListedAgent;
		private IAbsence absence;
		private PersonAbsenceAccount personAccounts;
		private WorkflowControlSet _wfcs;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<SchedulerStateScheduleDayChangedCallback>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<ResourceCalculationWithCount>().For<IResourceCalculation>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
		}

		public void SetUp()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);

			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));
			
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

			waitListedAgent = PersonRepository.Has(skill);
			absence = AbsenceFactory.CreateAbsenceWithTracker("Holiday", Tracker.CreateTimeTracker());
			personAccounts = new PersonAbsenceAccount(waitListedAgent, absence);
			account = new AccountTime(new DateOnly(2016, 3, 1)) { Accrued = TimeSpan.FromHours(2) }.WithId();
			personAccounts.Add(account);
			PersonAbsenceAccountRepository.Add(personAccounts);

			_wfcs = new WorkflowControlSet().WithId();
			_wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			_wfcs.AbsenceRequestWaitlistEnabled = true;
			_wfcs.AbsenceRequestExpiredThreshold = 15;

			waitListedAgent.WorkflowControlSet = _wfcs;
			
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 11);
			var period2 = new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 1, 14);
			var assignmentPeriod = new DateTimePeriod(period.StartDateTime, period2.EndDateTime);

			PersonAssignmentRepository.Has(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, assignmentPeriod, new ShiftCategory("category")));

			PersonAssignmentRepository.Has(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, assignmentPeriod.MovePeriod(TimeSpan.FromDays(1)), new ShiftCategory("category")));

			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			waitListedRequest2 = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period2)).WithId();
			waitListedRequest2.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest2);

			var hourPeriod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(hourPeriod, new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(1)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(2)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(3)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(4)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(5)), new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));
		}

		[Test]
		public void ShouldDenyAbsenceRequestWhenPersonAccountIsInsufficient()
		{
			SetUp();
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldApproveAbsenceRequestWhenPersonAccountIsSufficient()
		{
			SetUp();
			account.BalanceIn = TimeSpan.FromHours(3);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldOnlyApproveRequestsUntilPersonAccountIsExceeded()
		{
			SetUp();
			account.Accrued = TimeSpan.FromHours(4);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			waitListedRequest2.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDeny2RequestsWhenPersonAccountIsInsufficient()
		{
			SetUp();
			account.Accrued = TimeSpan.FromHours(2);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsDenied.Should().Be.True();
			waitListedRequest2.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldOnlyCalculateTimeWithinShift()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();
			Now.Is(new DateTime(2016, 12, 1, 4, 00, 00, DateTimeKind.Utc));

			var period = new DateTimePeriod(2016, 12, 1, 5, 2016, 12, 1, 9);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			account.Accrued = TimeSpan.FromHours(2);
			
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldOnlyCalculateTimeWithinShiftSpanning2Days()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();
			Now.Is(new DateTime(2016, 12, 1, 4, 00, 00, DateTimeKind.Utc));

			var period = new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 2, 12);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			account.Accrued = TimeSpan.FromHours(10);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(4));
		}

		[Test]
		public void ShouldHandleRequestSpanningMultipleAccounts()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();
			Now.Is(new DateTime(2016, 12, 1, 4, 00, 00, DateTimeKind.Utc));

			var account2 = new AccountTime(new DateOnly(2016, 12, 2)) { Accrued = TimeSpan.FromHours(10) }.WithId();
			personAccounts.Add(account2);

			var period = new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 2, 12);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);
			account.Accrued = TimeSpan.FromHours(10);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(8));
			account2.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(6));
		}

		[Test]
		public void ShouldHandleWhenOneAccountIsInsufficent()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();
			Now.Is(new DateTime(2016, 12, 1, 4, 00, 00, DateTimeKind.Utc));

			var account2 = new AccountTime(new DateOnly(2016, 12, 2)) { Accrued = TimeSpan.FromHours(3) }.WithId();
			personAccounts.Add(account2);

			var period = new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 2, 12);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);
			account.Accrued = TimeSpan.FromHours(10);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsDenied.Should().Be.True();
			waitListedRequest.IsWaitlisted.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(10));
			account2.Remaining.Should().Be.EqualTo(TimeSpan.FromHours(3));
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, Guid[] skillCombinations, double resource)
		{
			return new SkillCombinationResource
			{
				StartDateTime = period1.StartDateTime,
				EndDateTime = period1.EndDateTime,
				Resource = resource,
				SkillCombination = skillCombinations
			};
		}
	}


}