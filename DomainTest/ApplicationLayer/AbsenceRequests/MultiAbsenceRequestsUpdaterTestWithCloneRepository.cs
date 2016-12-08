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
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTestWithStaticDependenciesAvoidUse]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterTestWithCloneRepository : ISetup
	{
		public IMultiAbsenceRequestsUpdater Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonRequestRepositoryWithClone PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonRequestRepositoryWithClone>().For<IPersonRequestRepository>();
		}

		[Test]
		public void ShouldDenySecondRequestIfFirstWasApprovedAndCausedStaffingToBeOnTheEdge()
		{
			Now.Is(DateTime.UtcNow);
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
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = false }.WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = firstDay.ToDateOnlyPeriod().Inflate(1),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(DateTime.UtcNow), new DateOnly(DateTime.UtcNow.AddDays(1))),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");

			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
			agent.WorkflowControlSet = workflowControlSet;
			agent2.WorkflowControlSet = workflowControlSet;

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), category, scenario);
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent2, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), category, scenario);
			PersonAssignmentRepository.Has(assignment);
			PersonAssignmentRepository.Has(assignment2);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()))).WithId();
			var personRequest2 = new PersonRequest(agent2, new AbsenceRequest(absence, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()))).WithId();

			personRequest.Pending();
			personRequest2.Pending();
			PersonRequestRepository.Add(personRequest);
			PersonRequestRepository.Add(personRequest2);

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, agent);

			var reqs = new List<IPersonRequest>() { personRequest, personRequest2 };

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());
			
			reqs.First().IsApproved.Should().Be.True();
			reqs.Second().IsApproved.Should().Be.False();
		}
	}
}