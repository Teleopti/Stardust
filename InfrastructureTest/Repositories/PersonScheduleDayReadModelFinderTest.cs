using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PersonScheduleDayReadModelFinderTest : DatabaseTest
	{
		private PersonScheduleDayReadModelFinder _target;

		[Test]
		public void ShouldReturnReadModelsForPersonForDates()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
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
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 10);

				Assert.That(_target.ForPerson(new DateOnly(2012, 8, 28), personId), Is.Not.Null);
			}
		}
		
		[Test]
		public void ShouldReturnReadModelsForPersonsAndDaySortedByShiftStart()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			var personSortedFirst = Guid.NewGuid();
			var personSortedSecond = Guid.NewGuid();
			var personSortedThird = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personSortedThird, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 10);
				createAndSaveReadModel(personSortedFirst, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 7);
				createAndSaveReadModel(personSortedSecond, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 8);

				var result = _target.ForPersons(new DateOnly(2012, 8, 28),
				                                new[] {personSortedFirst, personSortedSecond, personSortedThird}, new Paging());

				var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
				Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(personSortedFirst));
				Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(personSortedSecond));
				Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(personSortedThird));
				Assert.That(scheduleReadModels.ElementAt(0).MinStart,Is.EqualTo(new DateTime(2012, 8, 28,7,0,0)));
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId, Guid.NewGuid(), new DateTime(2012, 8, 29), 10);

				var ret = _target.ForPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), personId);

				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPeople()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId1, teamId, businessUnitId, new DateTime(2012, 8, 29),10);
				createAndSaveReadModel(personId2, teamId, businessUnitId, new DateTime(2012, 8, 29),10);
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = _target.ForPeople(new DateTimePeriod(new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 29, 12, 0, 0, DateTimeKind.Utc)), new []{personId1, personId2});
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, new DateTime(2012, 8, 29));
				clearReadModel(personId2, businessUnitId, new DateTime(2012, 8, 29));
				uow.PersistAll();
			}
		}

		private void clearReadModel(Guid personId, Guid businessUnitId, DateTime date)
		{
			var persister = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(),
																	MockRepository.GenerateMock<IMessageBrokerComposite>(),
																	MockRepository.GenerateMock<ICurrentDataSource>());

			persister.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, null, false);
		}

		private void createAndSaveReadModel(Guid personId, Guid teamId, Guid businessUnitId, DateTime date, int shiftStartHour)
		{
			var model = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = teamId,
				BusinessUnitId = businessUnitId,
				PersonId = personId,
				Start = date.AddHours(shiftStartHour),
				End = date.AddHours(shiftStartHour + 8),
				Model = "{shift: blablabla}",
			};

			var persister = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(),
																	MockRepository.GenerateMock<IMessageBrokerComposite>(),
																	MockRepository.GenerateMock<ICurrentDataSource>());

			persister.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, new[] { model }, false);
		}
	}
}

