﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
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
		public IMultiAbsenceRequestsUpdater Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldDenyIfPeriodNotOpenForRequest()
		{
			Now.Is(DateTime.UtcNow);
			ScenarioRepository.Has("scnearioName");
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
			Now.Is(DateTime.UtcNow);
			ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var wfcs = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator(), new StaffingThresholdValidator());

			var person = createAndSetupPerson(wfcs);

			var reqs = createNewRequest(absence, person);

			createAbsence(absence, person);

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());
			reqs.SingleOrDefault().DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyExpiredRequestWithWaitlistingEnabled()
		{
			Now.Is(DateTime.UtcNow);
			ScenarioRepository.Has("scnearioName");
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

		[Test]
		public void ShouldOnlyApprove50RequestsSoNotUnderstaffed()
		{
			Now.Is(DateTime.UtcNow);
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

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
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 150));

			var reqs = new List<IPersonRequest>();
			for (int i = 0; i < 200; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), category, scenario);
				PersonAssignmentRepository.Has(assignment);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, firstDay.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(50); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(150);

		}


		private List<IPersonRequest> createNewRequest(IAbsence absence, IPerson person)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(ScenarioRepository.LoadDefaultScenario(), person, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(8)));
			PersonAssignmentRepository.Has(assignment);

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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(ScenarioRepository.LoadDefaultScenario(), person, new DateTimePeriod(DateTime.UtcNow.Date.Add(offset), DateTime.UtcNow.Date.Add(offset).AddHours(8)));
			PersonAssignmentRepository.Has(assignment);

			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1)));
			var personAbsence = new PersonAbsence(person, ScenarioRepository.LoadDefaultScenario(),
				absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);
		}


	}
}