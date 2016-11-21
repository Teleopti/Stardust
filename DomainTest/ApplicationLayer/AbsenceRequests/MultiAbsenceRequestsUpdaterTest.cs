using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
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
	[DomainTest]
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
		public INow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MultiAbsenceRequestsUpdater>().For<IMultiAbsenceRequestsUpdater>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
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



		[Test, Ignore("Nightmare to test due to cast between FakeScheduleRange and ScheduleRange")]
		public void ShouldDenyIfNotEnoughPersonalAccount()
		{
			ScenarioRepository.Add(CurrentScenario.Current());
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.Tracker = Tracker.CreateDayTracker();

			var wfcs = createWorkFlowControlSet(absence, new PersonAccountBalanceValidator(), new StaffingThresholdValidator());

			var person = createAndSetupPerson(wfcs);

			var personAccount = new PersonAbsenceAccount(person, absence);

			personAccount.Add(new AccountDay(new DateOnly(DateTime.Today))
			{
				BalanceIn = TimeSpan.Zero,
				BalanceOut = TimeSpan.Zero,
				Accrued = TimeSpan.Zero,
				Extra = TimeSpan.Zero
			});

			PersonAbsenceAccountRepository.Add(personAccount);

			var reqs = createNewRequest(absence, person);

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());
			reqs.SingleOrDefault().DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonPersonAccount);
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

		[Test, Ignore("Nightmare to test due to cast between FakeScheduleRange and ScheduleRange")]
		public void ShouldDenyIfInvalidSchedule()
		{
			
		}

		[Test, Ignore("Nightmare to test due to cast between FakeScheduleRange and ScheduleRange")]
		public void ShouldDenyIfUnderstaffed()
		{

		}

		[Test, Ignore("Nightmare to test due to cast between FakeScheduleRange and ScheduleRange")]
		public void ShouldApproveIfAllIsOk()
		{
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