using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PersonScheduleDayReadModelPersisterTest : DatabaseTest
	{

		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), null);
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(target.IsInitialized(), Is.False);

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
				};

				target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, new[] { model }, false);

				Assert.That(target.IsInitialized(), Is.True);

				target.UpdateReadModels(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2)), personId, businessUnitId, null, false);

				Assert.That(target.IsInitialized(), Is.False);
			}
		}

		[Test]
		public void ShouldNotCrashIfShiftIsBiggerThanFourThousandAsCompressed()
		{
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), MockRepository.GenerateMock<ICurrentDataSource>());
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
				};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), personId, model.BusinessUnitId, new[] { model }, false);
		}

		[Test]
		public void ShouldSendToMessageBrokerOnCommit()
		{
			var messageBroker = MockRepository.GenerateMock<IMessageBroker>();
			var currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			currentDataSource.Stub(x => x.CurrentName()).Return("datasource");

			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), messageBroker, currentDataSource);

			var model = new PersonScheduleDayReadModel
				{
					Date = new DateTime(2013, 4, 3),
					TeamId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					Start = new DateTime(2013, 4, 3, 10, 0, 0, DateTimeKind.Utc),
					End = new DateTime(2013, 4, 3, 18, 0, 0, DateTimeKind.Utc),
					Model = "",
				};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId, model.BusinessUnitId, new[] { model }, false);

				messageBroker.AssertWasNotCalled(x => x.Send("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));

				uow.PersistAll();
			}

			messageBroker.AssertWasCalled(x => x.Send("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
		}

		[Test]
		public void ShouldNotSendToMessageBrokerOnCommitWhenNoficationDisabled()
		{
			var messageBroker = MockRepository.GenerateMock<IMessageBroker>();
			var currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			currentDataSource.Stub(x => x.CurrentName()).Return("datasource");

			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), messageBroker, currentDataSource);

			var model = new PersonScheduleDayReadModel
				{
					Date = new DateTime(2013, 4, 3),
					TeamId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					Start = new DateTime(2013, 4, 3, 10, 0, 0, DateTimeKind.Utc),
					End = new DateTime(2013, 4, 3, 18, 0, 0, DateTimeKind.Utc),
					Model = "",
				};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId, model.BusinessUnitId, new[] { model }, true);

				messageBroker.AssertWasNotCalled(x => x.Send("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));

				uow.PersistAll();
			}

			messageBroker.AssertWasNotCalled(x => x.Send("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
		}



		[Test]
		public void ShouldPersistIsDayOff()
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), MockRepository.GenerateMock<ICurrentDataSource>());
			
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
				};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId, model.BusinessUnitId, new[] { model }, false);

			new PersonScheduleDayReadModelFinder(uow)
				.ForPerson(new DateOnly(model.Date), model.PersonId)
				.IsDayOff.Should()
				.Be.True();
		}

	}
}