using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PersonScheduleDayReadModelPersisterTest : DatabaseTest
	{
		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			clearReadModel();
			var target = new PersonScheduleDayReadModelPersister(CurrUnitOfWork,
				MockRepository.GenerateMock<IMessageBrokerComposite>(), null);
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			
			var date = new DateTime(2012, 8, 29);
			var model = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = teamId,
				BusinessUnitId = businessUnitId,
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow
			};

			Assert.That(target.IsInitialized(), Is.False);

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId,
				new[] {model}, false);

			Assert.That(target.IsInitialized(), Is.True);
		}

		[Test]
		public void ShouldNotCrashIfShiftIsBiggerThanFourThousandAsCompressed()
		{
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBrokerComposite>(), MockRepository.GenerateMock<ICurrentDataSource>());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			const string shift = @"{\'FirstName\':\'????????? ?????\',\'LastName\':\'7004202\',\'EmploymentNumber\':\'\',\'Id\':\'4b9853d6-4073-48d4-a9b0-9e3f0101ff55\',\'Date\':\'2012-01-12T00:00:00\',\'WorkTimeMinutes\':664,\'ContractTimeMinutes\':664,\'Projection\':[{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T09:45:00Z\',\'End\':\'2012-01-12T10:52:00Z\',\'Minutes\':67,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T10:52:00Z\',\'End\':\'2012-01-12T10:55:00Z\',\'Minutes\':3,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T10:55:00Z\',\'End\':\'2012-01-12T11:00:00Z\',\'Minutes\':5,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T11:00:00Z\',\'End\':\'2012-01-12T11:15:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T11:15:00Z\',\'End\':\'2012-01-12T11:34:00Z\',\'Minutes\':19,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T11:34:
00Z\',\'End\':\'2012-01-12T11:43:00Z\',\'Minutes\':9,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T11:43:00Z\',\'End\':\'2012-01-12T12:15:00Z\',\'Minutes\':32,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T12:15:00Z\',\'End\':\'2012-01-12T12:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T12:30:00Z\',\'End\':\'2012-01-12T13:45:00Z\',\'Minutes\':75,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T13:45:00Z\',\'End\':\'2012-01-12T14:00:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T14:00:00Z\',\'End\':\'2012-01-12T15:01:00Z\',\'Minutes\':61,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T15:01:00Z\',\'End\':\'2012-01-12T15:05:00Z\',\'Minutes\':4,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T15:05:00Z\',\'En
d\':\'2012-01-12T15:14:00Z\',\'Minutes\':9,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T15:14:00Z\',\'End\':\'2012-01-12T15:19:00Z\',\'Minutes\':5,\'Title\':\'????????? ??????\'},{\'Color\':\'#FFFF00\',\'Start\':\'2012-01-12T15:19:00Z\',\'End\':\'2012-01-12T16:15:00Z\',\'Minutes\':56,\'Title\':\'???? / Lunch\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T16:15:00Z\',\'End\':\'2012-01-12T16:41:00Z\',\'Minutes\':26,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T16:41:00Z\',\'End\':\'2012-01-12T16:45:00Z\',\'Minutes\':4,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T16:45:00Z\',\'End\':\'2012-01-12T17:15:00Z\',\'Minutes\':30,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T17:15:00Z\',\'End\':\'2012-01-12T17:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T17:30:00Z\',\'End\':\'2012-01-12T17:45:00Z\',\'Minutes\':15
,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T17:45:00Z\',\'End\':\'2012-01-12T17:52:00Z\',\'Minutes\':7,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T17:52:00Z\',\'End\':\'2012-01-12T18:45:00Z\',\'Minutes\':53,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T18:45:00Z\',\'End\':\'2012-01-12T19:00:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T19:00:00Z\',\'End\':\'2012-01-12T20:15:00Z\',\'Minutes\':75,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T20:15:00Z\',\'End\':\'2012-01-12T20:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T20:30:00Z\',\'End\':\'2012-01-12T21:16:00Z\',\'Minutes\':46,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T21:16:00Z\',\'End\':\'2012-01-12T21:20:00Z\',\'Minutes\':4,\'Title\':\'????????? ??????
??? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T21:20:00Z\',\'End\':\'2012-01-12T21:35:00Z\',\'Minutes\':15,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T21:35:00Z\',\'End\':\'2012-01-12T21:42:00Z\',\'Minutes\':7,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T21:42:00Z\',\'End\':\'2012-01-12T21:45:00Z\',\'Minutes\':3,\'Title\':\'??????? / ????? ???????\'}]}";

			var model = new PersonScheduleDayReadModel
				{
					Date = new DateTime(2012, 8, 29),
					TeamId = teamId,
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					Start = new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc),
					End = new DateTime(2012, 8, 29, 18, 0, 0, DateTimeKind.Utc),
					Model = shift,
					ScheduleLoadTimestamp = DateTime.UtcNow
				};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), personId, model.BusinessUnitId, new[] { model }, false);
		}

		[Test]
		public void ShouldPersistIsDayOff()
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBrokerComposite>(), MockRepository.GenerateMock<ICurrentDataSource>());
			
			var model = new PersonScheduleDayReadModel
				{
					Date = new DateTime(2012, 8, 29),
					TeamId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					IsDayOff = true,
					Start = new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc),
					End = new DateTime(2012, 8, 29, 18, 0, 0, DateTimeKind.Utc),
					Model = "{shift: blablabla}",
					ScheduleLoadTimestamp = DateTime.UtcNow
				};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId, model.BusinessUnitId, new[] { model }, false);

			new PersonScheduleDayReadModelFinder(uow)
				.ForPerson(new DateOnly(model.Date), model.PersonId)
				.IsDayOff.Should()
				.Be.True();
		}

		[Test]
		public void ShouldPersistNewReadModel()
		{
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBrokerComposite>(), null);						
			var date = new DateTime(2012, 8, 29);
			var readModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				Start = date.AddHours(10),
				End = date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), readModel.PersonId, readModel.BusinessUnitId, new[] { readModel }, false);

			new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make())
				.ForPerson(new DateOnly(readModel.Date), readModel.PersonId)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistNewerReadModel()
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(uow, MockRepository.GenerateMock<IMessageBrokerComposite>(), null);						
			var date = new DateTime(2012, 8, 29);
			DateTime oldTimestamp = DateTime.UtcNow;
			var personId = Guid.NewGuid();

			var oldReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp
			};

			var newerReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = true,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp.AddHours(1)
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), oldReadModel.PersonId, oldReadModel.BusinessUnitId, new[] { oldReadModel }, false);
			new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make())
				.ForPerson(new DateOnly(oldReadModel.Date), oldReadModel.PersonId)
				.IsDayOff.Should()
				.Be.False();
			
			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), newerReadModel.PersonId, newerReadModel.BusinessUnitId, new[] { newerReadModel }, false);
			new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make())
				.ForPerson(new DateOnly(newerReadModel.Date), newerReadModel.PersonId)
				.IsDayOff.Should()
				.Be.True();
		}

		[Test]
		public void ShouldNotPersistOlderReadModel()
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(uow, MockRepository.GenerateMock<IMessageBrokerComposite>(), null);
			var date = new DateTime(2012, 8, 29);
			DateTime oldTimestamp = DateTime.UtcNow;
			var personId = Guid.NewGuid();

			var oldTimestampReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = false,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp
			};

			var newTimestampReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = true,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp.AddHours(1)
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), newTimestampReadModel.PersonId, newTimestampReadModel.BusinessUnitId, new[] { newTimestampReadModel }, false);

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), oldTimestampReadModel.PersonId, oldTimestampReadModel.BusinessUnitId, new[] { oldTimestampReadModel }, false);
			new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make())
				.ForPerson(new DateOnly(oldTimestampReadModel.Date), oldTimestampReadModel.PersonId)
				.IsDayOff.Should()
				.Be.True();
		}


		[Test]
		public void ShouldNotSendToMessageBrokerOnCommitWhenThereIsNoReadModelUpdate()
		{
			var uow = CurrUnitOfWork;

			var messageBroker = new FakeMessageBrokerComposite();
			messageBroker.ResetSendInvokedCount();
			messageBroker.Enable();

			var target = new PersonScheduleDayReadModelPersister(uow, messageBroker, new FakeCurrentDatasource("test"));
			var date = new DateTime(2012, 8, 29);
			DateTime oldTimestamp = DateTime.UtcNow;
			var personId = Guid.NewGuid();

			var oldTimestampReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = false,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp
			};

			var newTimestampReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = true,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp.AddHours(1)
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), newTimestampReadModel.PersonId, newTimestampReadModel.BusinessUnitId, new[] { newTimestampReadModel }, false);
			uow.Current().PersistAll();

			messageBroker.GetSendInvokedCount().Should().Be.EqualTo(1);
			
			messageBroker.ResetSendInvokedCount();

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), oldTimestampReadModel.PersonId, oldTimestampReadModel.BusinessUnitId, new[] { oldTimestampReadModel }, false);
			uow.Current().PersistAll();
			messageBroker.GetSendInvokedCount().Should().Be.EqualTo(0);
			CleanUpAfterTest();
		}

		[Test]
		public void ShouldSendToMessageBrokerOnCommitWhenThereIsReadModelUpdate()
		{
			var uow = CurrUnitOfWork;

			var messageBroker = new FakeMessageBrokerComposite();
			messageBroker.ResetSendInvokedCount();
			messageBroker.Enable();

			var target = new PersonScheduleDayReadModelPersister(uow, messageBroker, new FakeCurrentDatasource("test"));
			var date = new DateTime(2012, 8, 29);
			DateTime oldTimestamp = DateTime.UtcNow;
			var personId = Guid.NewGuid();

			var oldReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp
			};

			var newerReadModel = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				Start = date.AddHours(10),
				End = date.AddHours(18),
				IsDayOff = true,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = oldTimestamp.AddHours(1)
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), oldReadModel.PersonId, oldReadModel.BusinessUnitId, new[] { oldReadModel }, false);
			uow.Current().PersistAll();

			messageBroker.GetSendInvokedCount().Should().Be.EqualTo(1);

			messageBroker.ResetSendInvokedCount();

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), newerReadModel.PersonId, newerReadModel.BusinessUnitId, new[] { newerReadModel }, false);
			uow.Current().PersistAll();
			messageBroker.GetSendInvokedCount().Should().Be.EqualTo(1);
			CleanUpAfterTest();
		}

		private void clearReadModel()
		{
			Session.CreateSQLQuery("TRUNCATE TABLE ReadModel.PersonScheduleDay")
				.ExecuteUpdate();
		}
	}

	class FakeMessageBrokerComposite : IMessageBrokerComposite
	{
		private int _sendInvokedCount;
		private bool _disabled;

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void ResetSendInvokedCount()
		{
			_sendInvokedCount = 0;
		}

		public void Disable()
		{
			_disabled = true;
		}

		public void Enable()
		{
			_disabled = false;
		}

		public int GetSendInvokedCount()
		{
			return _sendInvokedCount;			
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
			DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
			if (!_disabled) _sendInvokedCount++;
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			if (!_disabled) _sendInvokedCount++;
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			if (!_disabled) _sendInvokedCount++;
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			throw new NotImplementedException();
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			throw new NotImplementedException();
		}

		public void Send(Message message)
		{
			throw new NotImplementedException();
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			throw new NotImplementedException();
		}

		public bool IsAlive { get; private set; }
		public bool IsPollingAlive { get; private set; }
		public string ServerUrl { get; set; }
		public void StartBrokerService(bool useLongPolling = false)
		{
			throw new NotImplementedException();
		}
	}
}