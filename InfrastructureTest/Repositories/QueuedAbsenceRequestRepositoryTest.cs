using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
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

		private IQueuedAbsenceRequest createAbsenceRequestAndBusinessUnit()
		{
			return new QueuedAbsenceRequest
			{
				PersonRequest = _personRequest.Id.GetValueOrDefault(),
				StartDateTime = _absenceRequest.Period.StartDateTime,
				EndDateTime = _absenceRequest.Period.EndDateTime,
				Created = _personRequest.CreatedOn.GetValueOrDefault()

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
			result.Count().Should().Be.EqualTo(1);
			result.First().PersonRequest.Should().Be.EqualTo(notDeletedId);
		}

		[Test]
		public void ShouldFindQueuedAbsenceRequestsByPersonRequestIds()
		{
			var sent = DateTime.UtcNow;

			var personRequest1 = createQueduedRequest(new DateTime(2008, 7, 10, 10, 0, 0), new DateTime(2008, 7, 14, 9, 0, 0), sent);
			
			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			var queuedAbsenceRequests = target.FindByPersonRequestIds(new[] {personRequest1});

			queuedAbsenceRequests.Count.Should().Be(1);
			queuedAbsenceRequests.First().PersonRequest.Should().Be(personRequest1);
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
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				StartDateTime = absenceRequest.Period.StartDateTime,
				EndDateTime = absenceRequest.Period.EndDateTime,
				Created = personRequest.CreatedOn.GetValueOrDefault(),
				Sent = sentDateTime
			};
			PersistAndRemoveFromUnitOfWork(queued);
			return queued.PersonRequest;
		}

		[Test]
		public void ShouldSetSentForBulkRequests()
		{
			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			var guidList  = new List<Guid>();
			for (var i = 0; i < 3000; i++)
			{
				guidList.Add(Guid.NewGuid());
			}
			target.Send(guidList, DateTime.UtcNow);
			var requests = target.LoadAll();

			requests.ForEach(request =>
			{
				request.Sent.Should().Not.Be.EqualTo(null);
			});
		}

		[Test]
		public void ShouldUpdatePeriodIfNotSent()
		{
			var startDateTime = new DateTime(2008, 7, 10, 10, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2008, 7, 14, 9, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDateTime,endDateTime);
			var personRequest = new PersonRequest(_person);
			var absenceRequest = new AbsenceRequest(_absence ?? _absence, period);
			personRequest.Request = absenceRequest;
			personRequest.Pending();

			PersistAndRemoveFromUnitOfWork(personRequest);

			var queued = new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				StartDateTime = absenceRequest.Period.StartDateTime,
				EndDateTime = absenceRequest.Period.EndDateTime,
				Created = personRequest.CreatedOn.GetValueOrDefault()
			};
			PersistAndRemoveFromUnitOfWork(queued);

			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			var updatedRows = target.UpdateRequestPeriod(queued.PersonRequest, new DateTimePeriod(startDateTime.AddHours(1),endDateTime.AddHours(1)));
			updatedRows.Should().Be.EqualTo(1);
			var queuedRequest = target.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(startDateTime.AddHours(1));
			queuedRequest.EndDateTime.Should().Be.EqualTo(endDateTime.AddHours(1));
		}

		[Test]
		public void ShouldNotUpdatePeriodIfSent()
		{
			var startDateTime = new DateTime(2008, 7, 10, 10, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2008, 7, 14, 9, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDateTime, endDateTime);
			var personRequest = new PersonRequest(_person);
			var absenceRequest = new AbsenceRequest(_absence ?? _absence, period);
			personRequest.Request = absenceRequest;
			personRequest.Pending();

			PersistAndRemoveFromUnitOfWork(personRequest);

			var queued = new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				StartDateTime = absenceRequest.Period.StartDateTime,
				EndDateTime = absenceRequest.Period.EndDateTime,
				Created = personRequest.CreatedOn.GetValueOrDefault(),
				Sent = DateTime.Now
			};
			PersistAndRemoveFromUnitOfWork(queued);

			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			var updatedRows =  target.UpdateRequestPeriod(queued.PersonRequest, new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)));
			updatedRows.Should().Be.EqualTo(0);
			var queuedRequest = target.LoadAll().FirstOrDefault();
			queuedRequest.StartDateTime.Should().Be.EqualTo(startDateTime);
			queuedRequest.EndDateTime.Should().Be.EqualTo(endDateTime);
		}

		[Test]
		public void ShouldResetSentColumn()
		{
			var startDateTime = new DateTime(2008, 7, 10, 10, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2008, 7, 14, 9, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDateTime, endDateTime);
			var personRequest = new PersonRequest(_person);
			var absenceRequest = new AbsenceRequest(_absence ?? _absence, period);
			personRequest.Request = absenceRequest;
			personRequest.Pending();

			var sent = DateTime.Now;

			PersistAndRemoveFromUnitOfWork(personRequest);

			var queued = new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				StartDateTime = absenceRequest.Period.StartDateTime,
				EndDateTime = absenceRequest.Period.EndDateTime,
				Created = personRequest.CreatedOn.GetValueOrDefault(),
				Sent = sent
			};
			PersistAndRemoveFromUnitOfWork(queued);

			var target = new QueuedAbsenceRequestRepository(CurrUnitOfWork);
			target.ResetSent(sent);
			
			var queuedRequest = target.LoadAll().FirstOrDefault();
			queuedRequest.Sent.HasValue.Should().Be.False();
		}
	}
}
