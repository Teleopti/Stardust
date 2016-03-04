using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AbsenceWaitlisting
{
	[TestFixture]
	class AbsenceRequestWaitlistProviderTest
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
			_workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), _absence);
			_absenceRequestWaitlistProvider = new AbsenceRequestWaitlistProvider (_personRequestRepository);

		}

		[Test]
		public void CanReturnWaitlistInCorrectOrder()
		{
			var absenceRequestOne = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 19, 00, 00, DateTimeKind.Utc)));
			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));
			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 18, 00, 00, DateTimeKind.Utc)));

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequestThree.Parent, new DateTime(2016, 01, 01, 08, 00, 00));
			property.SetValue(absenceRequestOne.Parent, new DateTime(2016, 01, 01, 10, 00, 00));
			property.SetValue(absenceRequestTwo.Parent, new DateTime(2016, 01, 01, 12, 00, 00));

			//absenceTwo intersects absenceOne and absenceThree
			var waitlist = _absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequestTwo.Period, _workflowControlSet).ToArray();

			Assert.IsTrue(waitlist[0].Request == absenceRequestThree);
			Assert.IsTrue(waitlist[1].Request == absenceRequestOne);
			Assert.IsTrue(waitlist[2].Request == absenceRequestTwo);
		}


		[Test]
		public void ShouldOnlyReturnWaitlistedAbsenceRequestsThatOverlap()
		{
			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 11, 00, 00, DateTimeKind.Utc)));
			
			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));
			var absenceRequestThree = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 14, 00, 00, DateTimeKind.Utc)));

			//absenceTwo intersects absenceThree only
			var waitlist = _absenceRequestWaitlistProvider.GetWaitlistedRequests(absenceRequestTwo.Period, _workflowControlSet).ToArray();

			Assert.AreEqual (2, waitlist.Count());

			Assert.IsTrue(waitlist[0].Request == absenceRequestTwo);
			Assert.IsTrue(waitlist[1].Request == absenceRequestThree);
		}


		[Test]
		public void ShouldReturnCorrectPositionInWaitlist()
		{
			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 19, 00, 00, DateTimeKind.Utc)));
			var absenceRequestTwo = createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 16, 00, 00, DateTimeKind.Utc)));
			createAutoDeniedAbsenceRequest(createAndSetupPerson(_workflowControlSet), _absence, new DateTimePeriod(new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 18, 00, 00, DateTimeKind.Utc)));
			
			var position = new AbsenceRequestWaitlistProvider(_personRequestRepository).GetPositionInWaitlist(absenceRequestTwo);
			
			Assert.AreEqual (2, position);
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = true };

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
			var absenceRequest = new AbsenceRequest (absence, requestDateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest );

			personRequest.SetId(Guid.NewGuid());

			personRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			_personRequestRepository.Add(personRequest);

			return absenceRequest;
		}

		

	}
}

