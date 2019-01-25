using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;

using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class WaitlistRequestHandlerPersonAccountDayTest : IIsolateSystem
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
		private WorkflowControlSet wfcs;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<SchedulerStateScheduleDayChangedCallback>().For<IScheduleDayChangeCallback>();
			isolate.UseTestDouble<ResourceCalculationWithCount>().For<IResourceCalculation>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
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
			absence = AbsenceFactory.CreateAbsenceWithTracker("Holiday", Tracker.CreateDayTracker());
			personAccounts = new PersonAbsenceAccount(waitListedAgent, absence);
			account = new AccountDay(new DateOnly(2016, 3, 1)) { Accrued = TimeSpan.FromDays(1) }.WithId();
			personAccounts.Add(account);
			PersonAbsenceAccountRepository.Add(personAccounts);

			wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			wfcs.AbsenceRequestExpiredThreshold = 15;

			waitListedAgent.WorkflowControlSet = wfcs;
			
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
			var skillCombinations = new HashSet<Guid> {skill.Id.GetValueOrDefault()};
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(hourPeriod, skillCombinations, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(1)), skillCombinations, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(2)), skillCombinations, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(3)), skillCombinations, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(4)), skillCombinations, 10),
				createSkillCombinationResource(hourPeriod.MovePeriod(TimeSpan.FromHours(5)), skillCombinations, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));
		}

		[Test]
		public void ShouldAllowMultipleAbsencesOnSameDayWhenUsingPersonAccountDayBug46284()
		{
			SetUp();
			var personAbsense = PersonAbsenceFactory.CreatePersonAbsence(waitListedAgent, ScenarioRepository.LoadAll().FirstOrDefault(),
				new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11), absence).WithId();
			PersonAbsenceRepository.Add(personAbsense);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest2.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldAllowMultipleRequestsOnSameDayOnSameAbsencePersonAccountDay()
		{
			SetUp();
			account.Accrued = TimeSpan.FromDays(1);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			waitListedRequest2.IsApproved.Should().Be.True();
		}
		
		[Test]
		public void ShouldApproveRequestSpanningMultipleDaysUsingPersonAccountDay()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 2, 14);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			account.Accrued = TimeSpan.FromDays(2);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldDenyRequestSpanningMultipleDaysUsingPersonAccountDay()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 2, 14);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			account.Accrued = TimeSpan.FromDays(1);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsDenied.Should().Be.True();
			waitListedRequest.IsWaitlisted.Should().Be.False();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromDays(1));
		}

		[Test]
		public void ShouldHandleRequestSpanningMultiplePersonAccountDay()
		{
			SetUp();
			PersonRequestRepository.RequestRepository.Clear();

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 2, 14);
			waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			var dayAccount2 = new AccountDay(new DateOnly(2016, 12, 2)).WithId();
			personAccounts.Add(dayAccount2);
			account.Accrued = TimeSpan.FromDays(1);
			dayAccount2.Accrued = TimeSpan.FromDays(1);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });
			waitListedRequest.IsApproved.Should().Be.True();
			account.Remaining.Should().Be.EqualTo(TimeSpan.FromDays(0));
			dayAccount2.Remaining.Should().Be.EqualTo(TimeSpan.FromDays(0));
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, HashSet<Guid> skillCombinations, double resource)
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