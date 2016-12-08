using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTestWithStaticDependenciesAvoidUse]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterTest : ISetup
	{
		public MultiAbsenceRequestsUpdater Target;
		public FakeCurrentScenario CurrentScenario;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeToggleManager ToggleManager;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeSkillDayRepository SkillDayRepository;
		public FullScheduling Scheduling;
		public IAuthorizationScope AuthorizationScope;
		public INow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MultiAbsenceRequestsUpdater>().For<IMultiAbsenceRequestsUpdater>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<CurrentAuthorization>().For<IAuthorizationScope>();
			system.UseTestDouble(new MutableNow(DateTime.UtcNow)).For<INow>();
		}

		[Test]
		public void ShouldDenyIfPeriodNotOpenForRequest()
		{
			ScenarioRepository.Add(CurrentScenario.Current());
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var wfcs = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator(), new AbsenceRequestNoneValidator());
			wfcs.RemoveOpenAbsenceRequestPeriod(wfcs.AbsenceRequestOpenPeriods.FirstOrDefault());

			var person = createAndSetupPerson(wfcs);

			var reqs = createNewRequest(absence, person);
			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());
			reqs.SingleOrDefault().DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonClosedPeriod);
		}

		[Test]
		public void ShouldDenyIfPersonIsAlreadyAbsent()
		{
			ScenarioRepository.Add(CurrentScenario.Current());
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var wfcs = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator(), new StaffingThresholdValidator());

			var person = createAndSetupPerson(wfcs);

			var reqs = createNewRequest(absence, person);

			createAbsence(absence, person);

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());
			reqs.SingleOrDefault().DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		//[Ignore("Can't run with Fake repos")]
		public void ShouldDenySecondRequestIfFirstWasApprovedAndCausedStaffingToBeOnTheEdge()
		{
			//AuthorizationScope.OnThisThreadUse(new FullPermission());
			//CurrentAuthorization.DefaultTo(new FullPermission());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = false};
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = firstDay.ToDateOnlyPeriod().Inflate(1);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow), new DateOnly(DateTime.UtcNow.AddDays(1))),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team {Site = new Site("site")};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");

			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team , schedulePeriod, skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
			agent.WorkflowControlSet = workflowControlSet;
			agent2.WorkflowControlSet = workflowControlSet;

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
																					2,
																					2)
			);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), category, CurrentScenario.Current());
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent2, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), category, CurrentScenario.Current());
			ScheduleStorage.Add(assignment);
			ScheduleStorage.Add(assignment2);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()))).WithId();
			var personRequest2 = new PersonRequest(agent2, new AbsenceRequest(absence, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()))).WithId();

			personRequest.Pending();
			personRequest2.Pending();
			PersonRequestRepository.Add(personRequest);
			PersonRequestRepository.Add(personRequest2);

			LoggedOnUser.SetFakeLoggedOnUser(agent);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, agent);

			var reqs = new List<IPersonRequest>() {personRequest, personRequest2};

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			
			reqs.First().IsApproved.Should().Be.True();
			reqs.Second().IsApproved.Should().Be.False();
		}


		[Test]
		public void ShouldDenyExpiredRequestWithWaitlistingEnabled()
		{
			ScenarioRepository.Add(CurrentScenario.Current());

			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var wfcs = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator(), new StaffingThresholdValidator());
			wfcs.AbsenceRequestExpiredThreshold = 15;
			wfcs.AbsenceRequestWaitlistEnabled = true;

			var person = createAndSetupPerson(wfcs);
			var reqs = createNewRequest(absence, person);

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			var req = reqs.SingleOrDefault();
			req.IsDenied.Should().Be.EqualTo(true);
			req.IsWaitlisted.Should().Be.EqualTo(false);
			req.DenyReason.Should().Be.EqualTo(string.Format(Resources.RequestDenyReasonRequestExpired, req.Request.Period.StartDateTime, 15));
		}

		
		private List<IPersonRequest> createNewRequest(IAbsence absence, IPerson person)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(CurrentScenario.Current(), person, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(8)));
			ScheduleStorage.Add(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddMinutes(10)))).WithId();
			
			personRequest.Pending();

			PersonRequestRepository.Add(personRequest);

			return new List<IPersonRequest> { personRequest };
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet wfcs)
		{
			var person = PersonFactory.CreatePersonWithId();
			person.WorkflowControlSet = wfcs; 
			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence, IAbsenceRequestValidator PersonAccountvalidator, IAbsenceRequestValidator validator)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = false };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-5), DateOnly.Today.AddDays(5));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = PersonAccountvalidator,
				StaffingThresholdValidator = validator,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private void createAbsence(IAbsence absence, IPerson person, TimeSpan offset = new TimeSpan())
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(CurrentScenario.Current(), person, new DateTimePeriod(DateTime.UtcNow.Date.Add(offset), DateTime.UtcNow.Date.Add(offset).AddHours(8)));
			ScheduleStorage.Add(assignment);

			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1)));
			var personAbsence = new PersonAbsence(person, CurrentScenario.Current(),
				absenceLayer);
			ScheduleStorage.Add(personAbsence);
		}

	}
}