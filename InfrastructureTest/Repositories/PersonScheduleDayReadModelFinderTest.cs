using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

using Teleopti.Messaging.Client.Composite;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonScheduleDayReadModelFinderTest
	{
		public IPersonScheduleDayReadModelFinder Target;
		public ISiteRepository Sites;
		public ITeamRepository Teams;
		public IPersonRepository Persons;
		public IContractRepository Contracts;
		public IContractScheduleRepository ContractSchedules;
		public IPartTimePercentageRepository PartTimePercentages;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldReturnReadModelsForPersonForDates()
		{
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(Target.ForPerson(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(5)), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldReturnReadModelForPersonDay()
		{
			var personId = Guid.NewGuid();
			createAndSaveReadModel(personId, Guid.NewGuid(), new DateTime(2012, 8, 28), 10);

			var result = WithUnitOfWork.Get(() => Target.ForPerson(new DateOnly(2012, 8, 28), personId));
			Assert.That(result, Is.Not.Null);
		}
		
		[Test]
		public void ShouldReturnReadModelsForPersonsAndDaySortedByShiftStart()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per1);
				Persons.Add(per2);
				Persons.Add(per3);
			});

			createAndSaveReadModel(per1.Id.Value, Guid.NewGuid(), new DateTime(2012, 8, 28), 10);
			createAndSaveReadModel(per2.Id.Value, Guid.NewGuid(), new DateTime(2012, 8, 28), 7);
			createAndSaveReadModel(per3.Id.Value, Guid.NewGuid(), new DateTime(2012, 8, 28), 8);

			var result = WithUnitOfWork.Get(() => Target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { per1.Id.Value, per2.Id.Value, per3.Id.Value }, new Paging() { Skip = 0, Take = 20 }));

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per3.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per1.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 7, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsAndDaySortedByShiftEndDescending()
		{
			ISite site = SiteFactory.CreateSimpleSite("site");
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("team"));

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
				Persons.Add(per1);
				Persons.Add(per2);
				Persons.Add(per3);
			});

			createAndSaveReadModel((Guid)per1.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 10, 14);
			createAndSaveReadModel((Guid)per2.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 7, 15);
			createAndSaveReadModel((Guid)per3.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 8, 13);

			var result = WithUnitOfWork.Get(() => Target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { per1.Id.Value, per2.Id.Value, per3.Id.Value }, new Paging() { Skip = 0, Take = 20 }, timeSortOrder: "end desc"));

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per1.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per3.Id.Value));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 7, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsWithEmptySchedule()
		{
			ISite site = SiteFactory.CreateSimpleSite("site");
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("team"));

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
				Persons.Add(per1);
				Persons.Add(per2);
				Persons.Add(per3);
			});


			createAndSaveReadModel((Guid) per2.Id, (Guid) site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 8);
			createAndSaveReadModel((Guid) per3.Id, (Guid) site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 10);

			var result = WithUnitOfWork.Get(() => Target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { (Guid)per1.Id, (Guid)per2.Id, (Guid)per3.Id }, new Paging() { Skip = 0, Take = 20 }));

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.ElementAt(2).PersonId, Is.EqualTo(per1.Id));
			Assert.That(scheduleReadModels.ElementAt(0).PersonId, Is.EqualTo(per2.Id));
			Assert.That(scheduleReadModels.ElementAt(1).PersonId, Is.EqualTo(per3.Id));
			Assert.That(scheduleReadModels.ElementAt(0).MinStart, Is.EqualTo(new DateTime(2012, 8, 28, 8, 0, 0)));
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsWithoutEmptySchedule()
		{
			ISite site = SiteFactory.CreateSimpleSite("site");
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("team"));

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));


			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
				Persons.Add(per1);
				Persons.Add(per2);
				Persons.Add(per3);
			});

			createAndSaveReadModel((Guid)per2.Id,  (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 8);
			createAndSaveReadModel((Guid)per3.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 10);


			var timeFilterInfo = new TimeFilterInfo() { IsDayOff = false, IsWorkingDay = true, IsEmptyDay = false };

			var result = WithUnitOfWork.Get(() => Target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { (Guid)per1.Id, (Guid)per2.Id, (Guid)per3.Id }, new Paging() { Skip = 0, Take = 20 }, timeFilterInfo));

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.Count, Is.EqualTo(2));			
		}

		[Test]
		public void ShouldReturnReadModelsForPersonsWithDayOff()
		{
			ISite site = SiteFactory.CreateSimpleSite("site");
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("team"));

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));


			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
				Persons.Add(per1);
				Persons.Add(per2);
				Persons.Add(per3);
			});

			createAndSaveReadModel((Guid)per2.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 8, isDayoff:true);
			createAndSaveReadModel((Guid)per3.Id, (Guid)site.GetOrFillWithBusinessUnit_DONTUSE().Id, new DateTime(2012, 8, 28), 10);

			var timeFilterInfo = new TimeFilterInfo { IsDayOff = true };

			var result = WithUnitOfWork.Get(() => Target.ForPersons(new DateOnly(2012, 8, 28),
				new[] { (Guid)per1.Id, (Guid)per2.Id, (Guid)per3.Id }, new Paging { Skip = 0, Take = 20 }, timeFilterInfo));

			var scheduleReadModels = result as IList<PersonScheduleDayReadModel> ?? result.ToList();
			Assert.That(scheduleReadModels.Count, Is.EqualTo(1));
			Assert.That(scheduleReadModels.ElementAt(0).IsDayOff, Is.EqualTo(true));
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			createAndSaveReadModel(personId, Guid.NewGuid(), new DateTime(2012, 8, 29), 10);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPerson(new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(5)), personId);

				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPeople()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			createAndSaveReadModel(personId1, businessUnitId, new DateTime(2012, 8, 29),10);
			createAndSaveReadModel(personId2, businessUnitId, new DateTime(2012, 8, 29),10);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 29, 12, 0, 0, DateTimeKind.Utc)), new []{personId1, personId2});
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, new DateTime(2012, 8, 29));
				clearReadModel(personId2, businessUnitId, new DateTime(2012, 8, 29));
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldLoadBothWorkingSchedules()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10);
			createAndSaveReadModel(personId2, businessUnitId, date, 10);
			var personInfo1 = new PersonInfoForShiftTradeFilter(){PersonId = personId1, IsDayOff = false};
			var personInfo2 = new PersonInfoForShiftTradeFilter(){PersonId = personId2, IsDayOff = false};
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date, date.AddHours(23)), new List<PersonInfoForShiftTradeFilter>{ personInfo1 , personInfo2});
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldLoadMyDayoffAndPersonToWorkingSchedule()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10, isDayoff: true);
			createAndSaveReadModel(personId2, businessUnitId, date, 10);
			var personInfo1 = new PersonInfoForShiftTradeFilter() { PersonId = personId1, IsDayOff = true };
			var personInfo2 = new PersonInfoForShiftTradeFilter() { PersonId = personId2, IsDayOff = false };
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date, date.AddHours(23)), new List<PersonInfoForShiftTradeFilter> { personInfo1, personInfo2 });
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldLoadMyWorkingScheduleAndPersonToDayoff()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10);
			createAndSaveReadModel(personId2, businessUnitId, date, 10, isDayoff: true);
			var personInfo1 = new PersonInfoForShiftTradeFilter() { PersonId = personId1, IsDayOff = false };
			var personInfo2 = new PersonInfoForShiftTradeFilter() { PersonId = personId2, IsDayOff = true };
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date, date.AddHours(23)), new List<PersonInfoForShiftTradeFilter> { personInfo1, personInfo2 });
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldNotLoadScheduleWhenNotFitFilterSet()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10, isDayoff:true);
			createAndSaveReadModel(personId2, businessUnitId, date, 10);
			var personInfo1 = new PersonInfoForShiftTradeFilter() { PersonId = personId1, IsDayOff = false };
			var personInfo2 = new PersonInfoForShiftTradeFilter() { PersonId = personId2, IsDayOff = false };
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date, date.AddHours(23)), new List<PersonInfoForShiftTradeFilter> { personInfo1, personInfo2 });
				Assert.That(ret.Count(), Is.EqualTo(0));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldNotLoadScheduleThatBeforePeriod()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10);
			createAndSaveReadModel(personId2, businessUnitId, date, 10);
			var personInfo1 = new PersonInfoForShiftTradeFilter() { PersonId = personId1, IsDayOff = false };
			var personInfo2 = new PersonInfoForShiftTradeFilter() { PersonId = personId2, IsDayOff = false };
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date.AddDays(1), date.AddDays(1).AddHours(23)), new List<PersonInfoForShiftTradeFilter> { personInfo1, personInfo2 });
				Assert.That(ret.Count(), Is.EqualTo(0));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		[Test]
		public void ShouldNotLoadScheduleThatAfterPeriod()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var date = new DateTime(2012, 8, 29, 0, 0, 0, DateTimeKind.Utc);
			createAndSaveReadModel(personId1, businessUnitId, date, 10);
			createAndSaveReadModel(personId2, businessUnitId, date, 10);
			var personInfo1 = new PersonInfoForShiftTradeFilter() { PersonId = personId1, IsDayOff = false };
			var personInfo2 = new PersonInfoForShiftTradeFilter() { PersonId = personId2, IsDayOff = false };
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = Target.ForPeople(new DateTimePeriod(date.AddDays(-1), date.AddDays(-1).AddHours(23)), new List<PersonInfoForShiftTradeFilter> { personInfo1, personInfo2 });
				Assert.That(ret.Count(), Is.EqualTo(0));
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				clearReadModel(personId1, businessUnitId, date);
				clearReadModel(personId2, businessUnitId, date);
				uow.PersistAll();
			}
		}

		private void clearReadModel(Guid personId, Guid businessUnitId, DateTime date)
		{
			var persister = new PersonScheduleDayReadModelPersister(CurrentUnitOfWork.Make(), new DoNotSend(), new FakeCurrentDatasource("asd"));

			persister.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, null, false);
		}

		private void createAndSaveReadModel(Guid personId, Guid businessUnitId, DateTime date, int shiftStartHour, int? shiftEndHour = null, bool isDayoff = false)
		{
			var model = new PersonScheduleDayReadModel
			{
				Date = date,			
				PersonId = personId,
				Model = "{shift: blablabla}",
				IsDayOff = isDayoff,
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

			WithUnitOfWork.Do(uow =>
			{

				var persister = new PersonScheduleDayReadModelPersister(uow, new DoNotSend(), new FakeCurrentDatasource("asd"));

				persister.UpdateReadModels(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)), personId, businessUnitId, new[] { model }, false);
			});
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
			WithUnitOfWork.Do(() =>
			{
				Contracts.Add(pContract.Contract);
				ContractSchedules.Add(pContract.ContractSchedule);
				PartTimePercentages.Add(pContract.PartTimePercentage);
			});

			return pContract;
		}
	}
}

