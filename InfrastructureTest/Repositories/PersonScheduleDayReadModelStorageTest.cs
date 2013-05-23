﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PersonScheduleDayReadModelStorageTest : DatabaseTest
	{
		private PersonScheduleDayReadModelStorage _target;
 
		[Test]
		public void ShouldReturnReadModelsForPersonForDates()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), null);	
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.ForPerson(dateOnly, dateOnly.AddDays(5), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldReturnReadModelForPersonDay()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), null);
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, Guid.NewGuid(), new DateTime(2012, 8, 28));

				Assert.That(_target.ForPerson(new DateOnly(2012, 8, 28), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), MockRepository.GenerateMock<ICurrentDataSource>());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));

				var ret = _target.ForPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), personId);

				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), null);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.IsInitialized(), Is.False);

				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));

				Assert.That(_target.IsInitialized(), Is.True);

				_target.ClearPeriodForPerson(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2)), personId);

				Assert.That(_target.IsInitialized(), Is.False);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForTeam()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), MockRepository.GenerateMock<ICurrentDataSource>());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = _target.ForTeam(new DateTimePeriod(new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 29, 12, 0, 0, DateTimeKind.Utc)), teamId);
				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}

		private void createAndSaveReadModel(Guid personId, Guid teamId, DateTime date)
			{
				var model = new PersonScheduleDayReadModel
				            	{
				            		Date = date,
				            		TeamId = teamId,
				            		PersonId = personId,
				            		ShiftStart = date.AddHours(10),
									ShiftEnd = date.AddHours(18),
				            		Shift = "",
				            	};

				_target.SaveReadModel(model);
			}

		[Test]
		public void ShouldNotCrashIfShiftIsBiggerThanFourThousand()
		{
			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), MockRepository.GenerateMock<IMessageBroker>(), MockRepository.GenerateMock<ICurrentDataSource>());
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
				ShiftStart = new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc),
				ShiftEnd = new DateTime(2012, 8, 29, 18, 0, 0, DateTimeKind.Utc),
				Shift = shift,
			};
			_target.SaveReadModel(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSendToMessageBrokerOnCommit()
		{
			var messageBroker = MockRepository.GenerateMock<IMessageBroker>();
			var currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			currentDataSource.Stub(x => x.CurrentName()).Return("datasource");

			_target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), messageBroker, currentDataSource);

			var model = new PersonScheduleDayReadModel
				{
					Date = new DateTime(2013, 4, 3),
					TeamId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					ShiftStart = new DateTime(2013, 4, 3, 10, 0, 0, DateTimeKind.Utc),
					ShiftEnd = new DateTime(2013, 4, 3, 18, 0, 0, DateTimeKind.Utc),
					Shift = "",
				};

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target.SaveReadModel(model);

				messageBroker.AssertWasNotCalled(x => x.SendEventMessage("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
				
				uow.PersistAll();
			}

			messageBroker.AssertWasCalled(x => x.SendEventMessage("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldNotSendToMessageBrokerOnCommitWhenNoficationDisabled()
        {
            var messageBroker = MockRepository.GenerateMock<IMessageBroker>();
            var currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
            currentDataSource.Stub(x => x.CurrentName()).Return("datasource");

            _target = new PersonScheduleDayReadModelStorage(CurrentUnitOfWork.Make(), messageBroker, currentDataSource);

            var model = new PersonScheduleDayReadModel
            {
                Date = new DateTime(2013, 4, 3),
                TeamId = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                BusinessUnitId = Guid.NewGuid(),
                ShiftStart = new DateTime(2013, 4, 3, 10, 0, 0, DateTimeKind.Utc),
                ShiftEnd = new DateTime(2013, 4, 3, 18, 0, 0, DateTimeKind.Utc),
                Shift = "",
            };

            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _target.SaveReadModel(model, notifyBroker: false);

                messageBroker.AssertWasNotCalled(x => x.SendEventMessage("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));

                uow.PersistAll();
            }

            messageBroker.AssertWasNotCalled(x => x.SendEventMessage("datasource", model.BusinessUnitId, model.BelongsToDate, model.BelongsToDate, Guid.Empty, model.PersonId, typeof(Person), Guid.Empty, typeof(IPersonScheduleDayReadModel), DomainUpdateType.NotApplicable, null));
        }
	}

}

