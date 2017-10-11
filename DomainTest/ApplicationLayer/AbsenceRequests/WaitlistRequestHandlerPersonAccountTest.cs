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
	public class WaitlistRequestHandlerPersonAccountTest : ISetup
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
			var absence = AbsenceFactory.CreateAbsenceWithTracker("Holiday", Tracker.CreateTimeTracker());

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var waitListedAgent = PersonRepository.Has(skill);
			var personAccounts = new PersonAbsenceAccount(waitListedAgent, absence);
			account = new AccountTime(new DateOnly(2016, 3, 1)) { Accrued = TimeSpan.FromHours(2) };
			personAccounts.Add(account);
			PersonAbsenceAccountRepository.Add(personAccounts);

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(), //personvalidator??
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			waitListedAgent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 11);
			var period2 = new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 1, 14);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));

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
			account.Accrued = TimeSpan.FromHours(2);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsDenied.Should().Be.True();
			waitListedRequest2.IsDenied.Should().Be.True();
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