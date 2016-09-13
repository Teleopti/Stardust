using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class QueuedAbsenceRequestRepositoryTest : RepositoryTest<IQueuedAbsenceRequest>
	{
		private IPerson _person;
		private IAbsence _absence;
		private IScenario _defaultScenario;
		private IPersonRequest _personRequest;
		private IAbsenceRequest _absenceRequest;
		protected override void ConcreteSetup()
		{
			_defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
			_person = PersonFactory.CreatePerson("sdfoj");
			_absence = AbsenceFactory.CreateAbsence("Sick leave");

			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
			_personRequest = new PersonRequest(_person);
			_absenceRequest = new AbsenceRequest(_absence ?? _absence, period);

			_personRequest.Request = _absenceRequest;
			_personRequest.Pending();
			
			PersistAndRemoveFromUnitOfWork(_defaultScenario);
			PersistAndRemoveFromUnitOfWork(_person);
			PersistAndRemoveFromUnitOfWork(_absence);
			PersistAndRemoveFromUnitOfWork(_personRequest);
		}

		protected override IQueuedAbsenceRequest CreateAggregateWithCorrectBusinessUnit()
		{
			return createAbsenceRequestAndBusinessUnit();
		}

		private IQueuedAbsenceRequest createAbsenceRequestAndBusinessUnit(IAbsence absence = null)
		{
			return new QueuedAbsenceRequest
			{
				PersonRequest = _personRequest.Id.Value,
				StartDateTime = _absenceRequest.Period.StartDateTime,
				EndDateTime = _absenceRequest.Period.EndDateTime,
				Created = _personRequest.CreatedOn.Value,
				//Sent = DateTime.Now
			};
		}

		
		protected override void VerifyAggregateGraphProperties(IQueuedAbsenceRequest loadedAggregateFromDatabase)
		{
			var org = CreateAggregateWithCorrectBusinessUnit();
			org.PersonRequest.Should().Be.EqualTo(loadedAggregateFromDatabase.PersonRequest);
			org.StartDateTime.Should().Be.EqualTo(loadedAggregateFromDatabase.StartDateTime);
		}

		protected override Repository<IQueuedAbsenceRequest> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new QueuedAbsenceRequestRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldFindAllQueuedWithSomePartInsidePeriod()
		{
			var sent = DateTime.Now.Truncate(TimeSpan.FromSeconds(1));
			createQueduedRequest(new DateTime(2008, 7, 10, 8, 0, 0), new DateTime(2008, 7, 10, 9, 0, 0), sent);
			System.Threading.Thread.Sleep(1000);
			createQueduedRequest(new DateTime(2008, 7, 10, 10, 0, 0), new DateTime(2008, 7, 14, 9, 0, 0), sent);
			createQueduedRequest(new DateTime(2008, 7, 10, 18, 0, 0), new DateTime(2008, 7, 10, 20, 0, 0), sent);
			createQueduedRequest(new DateTime(2008, 7, 11, 8, 0, 0), new DateTime(2008, 7, 11, 9, 0, 0), sent);

			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 9, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 10, 15, 0, 0, DateTimeKind.Utc));
			_personRequest = new PersonRequest(_person);

			var queued = new QueuedAbsenceRequestRepository(CurrUnitOfWork).Find(period);
			queued.Count.Should().Be.EqualTo(2);
			queued[0].StartDateTime.Should().Be.EqualTo(new DateTime(2008, 7, 10, 8, 0, 0));
			queued[1].StartDateTime.Should().Be.EqualTo(new DateTime(2008, 7, 10, 10, 0, 0));
			queued.First().Sent.GetValueOrDefault().Should().Be.EqualTo(sent);
		}

		[Test]
		public void ShouldRemoveQueuedAbsenceRequests()
		{
			var sent = DateTime.UtcNow;

			createQueduedRequest(new DateTime(2008, 7, 10, 10, 0, 0), new DateTime(2008, 7, 14, 9, 0, 0), sent);
			createQueduedRequest(new DateTime(2008, 7, 10, 18, 0, 0), new DateTime(2008, 7, 10, 20, 0, 0), sent);
			var notDeletedId =  createQueduedRequest(new DateTime(2008, 7, 11, 8, 0, 0), new DateTime(2008, 7, 11, 9, 0, 0), null);

			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			target.Remove(sent);
			var result = target.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			result.First().PersonRequest.Should().Be.EqualTo(notDeletedId);
		}

		private Guid createQueduedRequest(DateTime start, DateTime end, DateTime? sentDateTime)
		{
			var period = new DateTimePeriod(
				new DateTime(start.Ticks, DateTimeKind.Utc),
				new DateTime(end.Ticks, DateTimeKind.Utc));
			var personRequest = new PersonRequest(_person);
			var absenceRequest = new AbsenceRequest(_absence ?? _absence, period);
			personRequest.Request = absenceRequest;
			personRequest.Pending();

			PersistAndRemoveFromUnitOfWork(personRequest);

			var queued = new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.Value,
				StartDateTime = absenceRequest.Period.StartDateTime,
				EndDateTime = absenceRequest.Period.EndDateTime,
				Created = personRequest.CreatedOn.Value,
				Sent = sentDateTime
			};
			PersistAndRemoveFromUnitOfWork(queued);
			return queued.PersonRequest;
		}

		[Test]
		public void ShouldUpdateSent()
		{
			createQueduedRequest(new DateTime(2008, 7, 10, 10, 0, 0), new DateTime(2008, 7, 14, 9, 0, 0), DateTime.UtcNow.AddMinutes(-100));
			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			target.CheckAndUpdateSent(90);
			var requests = target.LoadAll();
			requests.Count.Should().Be.EqualTo(1);
			requests.FirstOrDefault().Sent.Should().Be.EqualTo(null);
		}
	}
}
