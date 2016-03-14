using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

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
			ISite site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			createAndSaveReadModel(per1.Id.Value, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 10);
			createAndSaveReadModel(per2.Id.Value, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 7);
			createAndSaveReadModel(per3.Id.Value, Guid.NewGuid(), Guid.NewGuid(), new DateTime(2012, 8, 28), 8);

			var result = _target.ForPersons(new DateOnly(2012, 8, 28),
				new[] {per1.Id.Value, per2.Id.Value, per3.Id.Value}, new Paging() {Skip = 0, Take = 20});

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per3.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per1.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 7, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsAndDaySortedByShiftEndDescending()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			ISite site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("team");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			createAndSaveReadModel((Guid)per1.Id, (Guid)team.Id, (Guid)site.BusinessUnit.Id, new DateTime(2012, 8, 28), 10, 14);
			createAndSaveReadModel((Guid)per2.Id, (Guid)team.Id, (Guid)site.BusinessUnit.Id, new DateTime(2012, 8, 28), 7, 15);
			createAndSaveReadModel((Guid)per3.Id, (Guid)team.Id, (Guid)site.BusinessUnit.Id, new DateTime(2012, 8, 28), 8, 13);

			var result = _target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { per1.Id.Value, per2.Id.Value, per3.Id.Value }, new Paging(){Skip = 0, Take = 20},  timeSortOrder: "end desc");

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per1.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per3.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 7, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsWithEmptySchedule()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			ISite site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("team");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);


			createAndSaveReadModel((Guid) per2.Id, (Guid) team.Id, (Guid) site.BusinessUnit.Id, new DateTime(2012, 8, 28), 8);
			createAndSaveReadModel((Guid) per3.Id, (Guid) team.Id, (Guid) site.BusinessUnit.Id, new DateTime(2012, 8, 28), 10);

			var result = _target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { (Guid)per1.Id, (Guid)per2.Id, (Guid)per3.Id }, new Paging() { Skip = 0, Take = 20 });

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per1.Id));
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per3.Id));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 8, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsWithoutEmptySchedule()
		{
			_target = new PersonScheduleDayReadModelFinder(CurrentUnitOfWork.Make());
			ISite site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("team");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);


			createAndSaveReadModel((Guid)per2.Id, (Guid)team.Id, (Guid)site.BusinessUnit.Id, new DateTime(2012, 8, 28), 8);
			createAndSaveReadModel((Guid)per3.Id, (Guid)team.Id, (Guid)site.BusinessUnit.Id, new DateTime(2012, 8, 28), 10);


			var timeFilterInfo = new TimeFilterInfo() { IsDayOff = false, IsWorkingDay = true, IsEmptyDay = false };

			var result = _target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { (Guid)per1.Id, (Guid)per2.Id, (Guid)per3.Id }, new Paging() { Skip = 0, Take = 20 }, timeFilterInfo);

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.Count, Is.EqualTo(2));			
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

		private void createAndSaveReadModel(Guid personId, Guid teamId, Guid businessUnitId, DateTime date, int shiftStartHour, int? shiftEndHour = null)
		{
			var model = new PersonScheduleDayReadModel
			{
				Date = date,
				TeamId = teamId,
				BusinessUnitId = businessUnitId,
				PersonId = personId,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow
			};
			//Not empty schedule
			if (shiftStartHour >= 0)
			{
				model.Start = date.AddHours(shiftStartHour);
				model.End = date.AddHours( shiftEndHour.HasValue?shiftEndHour.Value: (shiftStartHour + 8)  );
			}
			else
			{
				// empty schedule, no start and end time.
			}
			var persister = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(),
																	MockRepository.GenerateMock<IMessageBrokerComposite>(),
																	MockRepository.GenerateMock<ICurrentDataSource>());

			persister.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, new[] { model }, false);
		}
		private IPersonContract createPersonContract(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}
	}
}

