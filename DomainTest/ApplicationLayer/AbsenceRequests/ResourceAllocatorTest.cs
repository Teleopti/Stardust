using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class ResourceAllocatorTest : ISetup
	{
		public ResourceAllocator Target;
		public FakeConfigReader ConfigReader;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeLoggedOnUser LoggedOnUser;
		private ISkill _primarySkill15;
		private ISkill _secondarySkill;
		private ISkill _unsortedSkill15;
		private ISkill _unsortedSkill30;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}


		[Test]
		public void ShouldUpdateResourcesWhenApproveBySplitResourceDependentOnStaffing()
		{
			var request = createNewRequest();
			var startDateTime = new DateTime(2016, 3, 14, 13, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 14, 13, 15, 0, DateTimeKind.Utc);

			const double shrinkage = 0.2;

			const double forecastedPrimary = 2;
			const double staffingPrimary = 3;
			const double overstaffedPrimary1 = staffingPrimary - forecastedPrimary * shrinkage;

			const double forecastedSecondary = 2; //secondary skill, should not be used
			const double staffingSecondary = 5;

			const double forecastedUnsorted15 = 2;
			const double staffingUnsorted15 = 10;
			const double overstaffedUnsorted15 = staffingUnsorted15 - forecastedUnsorted15 * shrinkage;

			//const double forecastedUnsorted30 = 2;
			//const double staffingUnsorted30 = 10;
			//	const double overstaffedUnsorted30 = staffingUnsorted30 - forecastedUnsorted30 * shrinkage;


			const double totaloverstaffed = overstaffedPrimary1 + overstaffedUnsorted15;


			var resourceList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval()
				{
					SkillId = _primarySkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					Forecast = forecastedPrimary,
					ForecastWithShrinkage = forecastedPrimary*shrinkage,
					StaffingLevel = staffingPrimary
				},
				new SkillStaffingInterval()
				{
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					Forecast = forecastedUnsorted15,
					ForecastWithShrinkage = forecastedUnsorted15 * shrinkage,
					StaffingLevel = staffingUnsorted15
				},
				new SkillStaffingInterval()
				{
					SkillId = _secondarySkill.Id.GetValueOrDefault(),
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					Forecast = forecastedSecondary,
					ForecastWithShrinkage = forecastedSecondary * shrinkage,
					StaffingLevel = staffingSecondary
				}
			};

			ScheduleForecastSkillReadModelRepository.Persist(resourceList, DateTime.Now);

			var changes = Target.AllocateResource(request);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					SkillId = _primarySkill15.Id.GetValueOrDefault(),
					StaffingLevel = - overstaffedPrimary1/totaloverstaffed
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime,
					EndDateTime = endDateTime,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = - overstaffedUnsorted15/totaloverstaffed
				}
			};

			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}


		private IPersonRequest createNewRequest(bool useWorkflowControlSet = true)
		{

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 12, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 14, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var person = createAndSetupPerson(workflowControlSet, useWorkflowControlSet);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = createAbsenceRequest(person, absence, period);

			return request;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet, bool useWorkflowControlSet)
		{
			_primarySkill15 = SkillFactory.CreateSkillWithId("PrimarySkill1");
			_primarySkill15.SetCascadingIndex(1);
			_secondarySkill = SkillFactory.CreateSkillWithId("SecondarySkill");
			_secondarySkill.SetCascadingIndex(2);
			_unsortedSkill15 = SkillFactory.CreateSkillWithId("NotCascadingSkill15", 15);
			_unsortedSkill30 = SkillFactory.CreateSkillWithId("NotCascadingSkill30", 30);


			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { _primarySkill15, _unsortedSkill15, _secondarySkill});

			if (useWorkflowControlSet)
				person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = false};
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 01, 01), new DateOnly(2016, 12, 31));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}


		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new FakePersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			personRequest.SetCreated(new DateTime(2016, 3, 14, 0, 5, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}
	}
}