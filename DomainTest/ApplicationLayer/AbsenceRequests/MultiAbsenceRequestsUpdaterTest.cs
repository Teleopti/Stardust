using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterTest
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