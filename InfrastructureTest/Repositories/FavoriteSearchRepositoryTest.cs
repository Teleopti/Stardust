using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Exceptions;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class FavoriteSearchRepositoryTest : RepositoryTest<IFavoriteSearch>
	{
		protected override IFavoriteSearch CreateAggregateWithCorrectBusinessUnit()
		{
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav = new FavoriteSearch("testFav");
			fav.Creator = person;

			return fav;
		}

		protected override void VerifyAggregateGraphProperties(IFavoriteSearch loadedAggregateFromDatabase)
		{
			var origin = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(origin.Name, loadedAggregateFromDatabase.Name);
		}

		protected override Repository<IFavoriteSearch> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new FavoriteSearchRepository(currentUnitOfWork);
		}

		[Test]
		public void VerifyCanPersistProperties()
		{
			var person = PersonFactory.CreatePerson("test");
			var fav = new FavoriteSearch("myFav")
			{
				SearchTerm = "agent",
				TeamIds = "team1,team2",
				Status = FavoriteSearchStatus.Default,
				Creator = person
			};
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(fav);

			var loadedAll = new FavoriteSearchRepository(UnitOfWork).LoadAll();

			Assert.AreEqual(1, loadedAll.Count);
			Assert.AreEqual("myFav", loadedAll.First().Name);
			Assert.AreEqual("agent", loadedAll.First().SearchTerm);
			Assert.AreEqual(FavoriteSearchStatus.Default, loadedAll.First().Status);
			Assert.AreEqual("team1,team2", loadedAll.First().TeamIds);
		}

		[Test]
		public void CanFindFavoritesByPerson()
		{
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav = new FavoriteSearch("myFav");
			fav.Creator = person;
			PersistAndRemoveFromUnitOfWork(fav);

			var allByPerson = new FavoriteSearchRepository(UnitOfWork).FindAllForPerson(person.Id.Value);

			Assert.AreEqual(1, allByPerson.Count());
		}

		[Test]
		public void CanFindFavoritesByPersonAndName()
		{
			var name1 = "myfav";
			var name2 = "search1";
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav1 = new FavoriteSearch(name1);
			var fav2 = new FavoriteSearch(name2);
			fav1.Creator = person;
			fav2.Creator = person;
			PersistAndRemoveFromUnitOfWork(fav1);
			PersistAndRemoveFromUnitOfWork(fav2);

			var results = new FavoriteSearchRepository(UnitOfWork).FindByPersonAndName(person.Id.Value, name1);

			Assert.AreEqual(1, results.Count());
		}

		[Test]
		public void ShouldThrowExceptionWhenAddingFavoriteSearchWithSameNameForSamePerson()
		{
			var name = "myfav";
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav1 = new FavoriteSearch(name);
			var duplicate = new FavoriteSearch(name);
			fav1.Creator = person;
			duplicate.Creator = person;
			PersistAndRemoveFromUnitOfWork(fav1);

			var target = new FavoriteSearchRepository(UnitOfWork);
			Assert.Throws<Infrastructure.Foundation.ConstraintViolationException>(() =>
			{				
				target.Add(duplicate);
				UnitOfWork.PersistAll();				
			});
		}
	}
}
