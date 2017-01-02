﻿using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AbsenceWaitlisting
{
	[TestFixture]
	public class AbsenceRequestWaitlistProviderTest
	{
		private IPersonRepository _personRepository;
		private IPersonRequestRepository _personRequestRepository;
		private IAbsence _absence;
		private WorkflowControlSet _workflowControlSet;
		private AbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;

		[SetUp]
		public void SetUp()
		{
			_personRepository = new FakePersonRepository();
			_personRequestRepository = new FakePersonRequestRepository();
			_absence = AbsenceFactory.CreateAbsence("Holiday");
			_workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01),
				new DateTime(2059, 12, 31), _absence).WithId();
			_absenceRequestWaitlistProvider = new AbsenceRequestWaitlistProvider(_personRequestRepository);
		}

		[Test]
		public void ShouldReturnWaitlistByCreateTime()
		{
			var baseTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var baseCreatedOn = new DateTime(2016, 01, 01);

			var person1 = createAndSetupPerson(_workflowControlSet);
			var person2 = createAndSetupPerson(_workflowControlSet);
			var person3 = createAndSetupPerson(_workflowControlSet);

			var absenceRequest1 = createAutoDeniedAbsenceRequest(person1, _absence,
				new DateTimePeriod(baseTime.AddHours(15), baseTime.AddHours(19)));
			var absenceRequest2 = createNewAbsenceRequest(person2, _absence,
				new DateTimePeriod(baseTime.AddHours(08), baseTime.AddHours(16)));
			var absenceRequest3 = createAutoDeniedAbsenceRequest(person3, _absence,
				new DateTimePeriod(baseTime.AddHours(10), baseTime.AddHours(18)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest3.Parent, baseCreatedOn.AddHours(08));
			property.SetValue(absenceRequest1.Parent, baseCreatedOn.AddHours(10));
			property.SetValue(absenceRequest2.Parent, baseCreatedOn.AddHours(12));

			//absenceTwo intersects absenceOne and absenceThree
			var waitlist =
				_absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequest2.Period,
				_workflowControlSet).ToArray();

			Assert.AreEqual(3, waitlist.Length);

			Assert.IsTrue(waitlist[0].Request == absenceRequest3);
			Assert.IsTrue(waitlist[1].Request == absenceRequest1);
			Assert.IsTrue(waitlist[2].Request == absenceRequest2);
		}

		[Test]
		public void ShouldReturnWaitlistByPersonSeniorityThenCreateTime()
		{
			var baseTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var baseCreatedOn = new DateTime(2016, 01, 01);

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

			var absenceRequest1 = createAutoDeniedAbsenceRequest(person1, _absence,
				new DateTimePeriod(baseTime.AddHours(15), baseTime.AddHours(19)));
			var absenceRequest2 = createNewAbsenceRequest(person2, _absence,
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

			var waitlist =
				_absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequest2.Period,
				workflowControlSetProcessWaitlistBySeniority).ToArray();

			Assert.AreEqual(4, waitlist.Length);

			Assert.IsTrue(waitlist[0].Request == absenceRequest4);
			Assert.IsTrue(waitlist[1].Request == absenceRequest1);
			Assert.IsTrue(waitlist[2].Request == absenceRequest3);
			Assert.IsTrue(waitlist[3].Request == absenceRequest2);
		}

		[Test]
		public void ShouldOnlyReturnWaitlistedAbsenceRequestsThatOverlap()
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

			//absenceTwo intersects absenceThree only
			var waitlist = _absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequestTwo.Period, _workflowControlSet).ToArray();

			Assert.AreEqual(2, waitlist.Length);

			Assert.IsTrue(waitlist[0].Request == absenceRequestTwo);
			Assert.IsTrue(waitlist[1].Request == absenceRequestThree);
		}

		[Test]
		public void ShouldReturnCorrectPositionInWaitlist()
		{
			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 15, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 19, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));

			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 18, 00, 00, DateTimeKind.Utc)));

			var position = new AbsenceRequestWaitlistProvider(_personRequestRepository).GetPositionInWaitlist(absenceRequestTwo);

			Assert.AreEqual(2, position);
		}

		[Test]
		public void Bug37600_ShouldReturnCorrectPositionInWaitlist()
		{
			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 10, 00, 00, DateTimeKind.Utc)));

			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));

			var absenceRequestFour = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));

			var waitlistProvider = new AbsenceRequestWaitlistProvider(_personRequestRepository);

			Assert.AreEqual(1, waitlistProvider.GetPositionInWaitlist(absenceRequestOne));
			Assert.AreEqual(1, waitlistProvider.GetPositionInWaitlist(absenceRequestTwo));
			Assert.AreEqual(3, waitlistProvider.GetPositionInWaitlist(absenceRequestThree));
			Assert.AreEqual(3, waitlistProvider.GetPositionInWaitlist(absenceRequestFour));
		}

		[Test]
		public void Bug39588_ShouldHandleRequestWithPersonThatHasNoWorkflowControlSet()
		{
			var absenceRequestOne = createNewAbsenceRequest(createAndSetupPerson(null), _absence,
				new DateTimePeriod(new DateTime(2016, 3, 1, 15, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 19, 00, 00, DateTimeKind.Utc)));
			var absenceRequestTwo = createNewAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequestOne.Parent, new DateTime(2016, 01, 01, 10, 00, 00));
			property.SetValue(absenceRequestTwo.Parent, new DateTime(2016, 01, 01, 12, 00, 00));

			//absenceTwo intersects absenceOne
			var waitlist = _absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequestTwo.Period, _workflowControlSet).ToArray();

			Assert.AreEqual(1, waitlist.Length);
			Assert.IsTrue(waitlist[0].Request == absenceRequestTwo);
		}

		[Test]
		public void Bug39661_ShouldNotHandleRequestAfterPersonIsDeleted()
		{
			var absenceRequest1 = createNewAbsenceRequest(createAndSetupPerson(_workflowControlSet, true), _absence,
				new DateTimePeriod(new DateTime(2016, 7, 8, 15, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 7, 8, 19, 00, 00, DateTimeKind.Utc)));
			var absenceRequest2 = createNewAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence,
				new DateTimePeriod(new DateTime(2016, 7, 8, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 7, 8, 16, 00, 00, DateTimeKind.Utc)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest1.Parent, new DateTime(2016, 01, 01, 10, 00, 00));
			property.SetValue(absenceRequest2.Parent, new DateTime(2016, 01, 01, 12, 00, 00));

			//absenceTwo intersects absenceOne
			var waitlist =
				_absenceRequestWaitlistProvider.GetWaitlistedRequests(
					new DateTimePeriod(new DateTime(2016, 7, 7, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 7, 9, 16, 00, 00, DateTimeKind.Utc)), _workflowControlSet).ToArray();

			Assert.AreEqual(1, waitlist.Length);
			Assert.IsTrue(waitlist[0].Request == absenceRequest2);
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet,
			bool isPersonDeleted = false)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			person.WorkflowControlSet = workflowControlSet;

			if (isPersonDeleted) ((Person)person).SetDeleted();

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate,
			IAbsence absence, WaitlistProcessOrder processOrder = WaitlistProcessOrder.FirstComeFirstServed)
		{
			var workflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestWaitlistEnabled = true,
				AbsenceRequestWaitlistProcessOrder = processOrder
			};

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest()
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

		private IAbsenceRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod, bool isAutoDenied)
		{
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest);

			personRequest.SetId(Guid.NewGuid());

			if (isAutoDenied)
			{
				personRequest.Deny( "Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			}

			_personRequestRepository.Add(personRequest);

			return absenceRequest;
		}
	}
}