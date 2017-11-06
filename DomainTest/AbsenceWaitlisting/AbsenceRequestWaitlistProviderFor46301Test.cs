﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.AbsenceWaitlisting
{
	[DomainTest]
	[TestFixture]
	[Toggle(Domain.FeatureFlags.Toggles.MyTimeWeb_WaitListPositionEnhancement_46301)]
	public class AbsenceRequestWaitlistProviderFor46301Test : ISetup
	{
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRequestWaitlistProvider AbsenceRequestWaitlistProvider;
		public MutableNow Now;
		private IAbsence _absence;
		private WorkflowControlSet _workflowControlSet;
		private DateTime baseTime;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonRepositoryLegacy2>().For<IPersonRepository>();

			_absence = AbsenceFactory.CreateAbsence("Holiday");

			_workflowControlSet = createWorkFlowControlSet(new DateTime(2014, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			baseTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
		}

		[Test]
		public void ShouldReturnWaitlistPositionByCreateTime()
		{
			Now.Is(baseTime);

			var baseCreatedOn = new DateTime(2016, 01, 01);

			var person1 = createAndSetupPerson(_workflowControlSet);
			var person2 = createAndSetupPerson(_workflowControlSet);
			var person3 = createAndSetupPerson(_workflowControlSet);

			var absenceRequest1 = createAutoDeniedAbsenceRequest(person1, _absence,
				new DateTimePeriod(baseTime.AddHours(15), baseTime.AddHours(19)));
			var absenceRequest2 = createAutoDeniedAbsenceRequest(person2, _absence,
				new DateTimePeriod(baseTime.AddHours(08), baseTime.AddHours(16)));
			var absenceRequest3 = createAutoDeniedAbsenceRequest(person3, _absence,
				new DateTimePeriod(baseTime.AddHours(10), baseTime.AddHours(18)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest3.Parent, baseCreatedOn.AddHours(08));
			property.SetValue(absenceRequest1.Parent, baseCreatedOn.AddHours(10));
			property.SetValue(absenceRequest2.Parent, baseCreatedOn.AddHours(12));
			
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest3).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest1).Should().Be(2);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest2).Should().Be(3);
		}

		[Test]
		public void ShouldReturnWaitlistPositionByPersonSeniorityThenCreateTime()
		{
			var baseCreatedOn = new DateTime(2016, 01, 01);
			Now.Is(baseTime);

			var workflowControlSetProcessWaitlistBySeniority = createWorkFlowControlSet(
				new DateTime(2016, 01, 01), new DateTime(2059, 12, 31), _absence,
				WaitlistProcessOrder.BySeniority).WithId();
			var personContract = PersonContractFactory.CreatePersonContract();
			var team = TeamFactory.CreateTeamWithId("Beijing");

			var person1 = createAndSetupPerson(workflowControlSetProcessWaitlistBySeniority);
			var person2 = createAndSetupPerson(workflowControlSetProcessWaitlistBySeniority);
			var person3 = createAndSetupPerson(workflowControlSetProcessWaitlistBySeniority);
			var person4 = createAndSetupPerson(workflowControlSetProcessWaitlistBySeniority);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 03, 01), personContract, team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 05, 01), personContract, team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 05, 01), personContract, team));
			person4.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 01, 01), personContract, team));

			var absenceRequest1 = createPendingAbsenceRequest(person1, _absence,
				new DateTimePeriod(baseTime.AddHours(15), baseTime.AddHours(19)));
			var absenceRequest2 = createAutoDeniedAbsenceRequest(person2, _absence,
				new DateTimePeriod(baseTime.AddHours(08), baseTime.AddHours(16)));
			var absenceRequest3 = createAutoDeniedAbsenceRequest(person3, _absence,
				new DateTimePeriod(baseTime.AddHours(10), baseTime.AddHours(18)));
			var absenceRequest4 = createAutoDeniedAbsenceRequest(person4, _absence,
				new DateTimePeriod(baseTime.AddHours(10), baseTime.AddHours(18)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest1.Parent, baseCreatedOn.AddHours(10));
			property.SetValue(absenceRequest2.Parent, baseCreatedOn.AddHours(12));
			property.SetValue(absenceRequest3.Parent, baseCreatedOn.AddHours(08));
			property.SetValue(absenceRequest4.Parent, baseCreatedOn.AddHours(11));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest4).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest3).Should().Be(3);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest2).Should().Be(4);

		}

		[Test]
		public void ShouldOnlyReturnWaitlistPositionWhereAbsenceRequestsThatOverlap()
		{
			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 3, 1, 13, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));
			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 14, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(3);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionRegardlessWorkflowControlSet()
		{
			Now.Is(baseTime);

			var workflowControlSet1 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var workflowControlSet2 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 20, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(2);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(3);
		}

		[Test]
		public void ShouldHandleRequestWithPersonThatHasNoWorkflowControlSet()
		{
			Now.Is(baseTime);

			var workflowControlSet1 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var workflowControlSet2 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(null), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 20, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(0);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(2);
		}

		[Test]
		public void ShouldGetWaitlistPostionExcludingDeletedPerson()
		{
			var absenceRequest1 = createPendingAbsenceRequest(createAndSetupPerson(_workflowControlSet, true), _absence,
				new DateTimePeriod(new DateTime(2016, 7, 8, 15, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 7, 8, 19, 00, 00, DateTimeKind.Utc)));
			var absenceRequest2 = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 7, 8, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 7, 8, 16, 00, 00, DateTimeKind.Utc)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest1.Parent, new DateTime(2016, 01, 01, 10, 00, 00));
			property.SetValue(absenceRequest2.Parent, new DateTime(2016, 01, 01, 12, 00, 00));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequest2).Should().Be(1);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionBasedOnWaitlistedAndAutoGrantPendingStatus()
		{
			Now.Is(baseTime);

			var workflowControlSet1 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var workflowControlSet2 = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			createNewAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			createNewAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var pendingAbsenceRequest = createNewAbsenceRequest(createAndSetupPerson(workflowControlSet1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));
			((PersonRequest)pendingAbsenceRequest.Parent).ForcePending();

			var waitlistedAbsenceRequest = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 20, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(waitlistedAbsenceRequest).Should().Be(2);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionBasedOnBudgetGroups()
		{
			Now.Is(baseTime);

			var budgetGroup1 = createBudgetGroup("group1");
			var budgetGroup2 = createBudgetGroup("group2");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var personPeriod1 = createPersonPeriod(new DateOnly(baseTime), budgetGroup1);
			var personPeriod2 = createPersonPeriod(new DateOnly(baseTime), budgetGroup2);

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 20, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(2);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionRegardlessBudgetGroupWhenAgentHasNoBudgetGroup()
		{
			Now.Is(baseTime);

			var budgetGroup = createBudgetGroup("group1");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var personPeriod1 = createPersonPeriod(new DateOnly(baseTime), budgetGroup);
			var personPeriod2 = createPersonPeriod(new DateOnly(baseTime), null);

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 20, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(2);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(1);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionWhenNoOverlapPeriods()
		{
			Now.Is(baseTime);

			var budgetGroup = createBudgetGroup("group1");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var personPeriod1 = createPersonPeriod(new DateOnly(baseTime), budgetGroup);

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 12, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(1);
		}

		[Test]
		public void ShouldCalculateWaitListedPositionWithDifferentPeriodsAndBudgetGroups()
		{
			Now.Is(baseTime);

			var budgetGroup1 = createBudgetGroup("group1");
			var budgetGroup2 = createBudgetGroup("group2");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();

			var personPeriod1 = createPersonPeriod(new DateOnly(baseTime), budgetGroup1);
			var personPeriod2 = createPersonPeriod(new DateOnly(baseTime), budgetGroup2);
			var personPeriod3 = createPersonPeriod(new DateOnly(baseTime), null);

			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod1), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 12, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod2), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 12, 00, 00, DateTimeKind.Utc)));

			var absenceRequestFour = createAutoDeniedAbsenceRequest(createAndSetupPerson(workflowControlSet, personPeriod: personPeriod3), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 12, 00, 00, DateTimeKind.Utc)));

			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestOne).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestTwo).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestThree).Should().Be(1);
			AbsenceRequestWaitlistProvider.GetPositionInWaitlist(absenceRequestFour).Should().Be(1);
		}

		private static IBudgetGroup createBudgetGroup(string name)
		{
			var budgetGroup = new BudgetGroup { Name = name };
			budgetGroup.SetId(Guid.NewGuid());
			return budgetGroup;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet,
			bool isPersonDeleted = false, IPersonPeriod personPeriod = null)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			person.WorkflowControlSet = workflowControlSet;

			if (isPersonDeleted) ((Person)person).SetDeleted();

			if (personPeriod != null)
				person.AddPersonPeriod(personPeriod);

			return person;
		}

		private IPersonPeriod createPersonPeriod(DateOnly startDate, IBudgetGroup budgetGroup)
		{
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate);
			personPeriod.BudgetGroup = budgetGroup;
			return personPeriod;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate,
			IAbsence absence, WaitlistProcessOrder processOrder = WaitlistProcessOrder.FirstComeFirstServed, bool isAutoGrant = true)
		{
			var workflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestWaitlistEnabled = true,
				AbsenceRequestWaitlistProcessOrder = processOrder
			};

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod,
				AbsenceRequestProcess = isAutoGrant ? new GrantAbsenceRequest() : (ProcessAbsenceRequest)new PendingAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private IAbsenceRequest createAutoDeniedAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, true);
		}

		private IAbsenceRequest createNewAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, false);
		}
		private IAbsenceRequest createPendingAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var request = createNewAbsenceRequest(person, absence, requestDateTimePeriod);
			((IPersonRequest) request.Parent).ForcePending();
			return request;
		}

		private IAbsenceRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod, bool isAutoDenied)
		{
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest);

			personRequest.SetId(Guid.NewGuid());

			if (isAutoDenied)
			{
				personRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			}

			PersonRequestRepository.Add(personRequest);

			return absenceRequest;
		}
	}
}