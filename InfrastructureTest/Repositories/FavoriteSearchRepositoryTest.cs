using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

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
			fav.WfmArea = WfmArea.Teams;

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
				Creator = person,
				WfmArea = WfmArea.Teams
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
		public void CanFindFavoritesByPersonInArea()
		{
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav = new FavoriteSearch("myFav");
			fav.Creator = person;
			fav.WfmArea = WfmArea.Teams;

			PersistAndRemoveFromUnitOfWork(fav);

			var allByPerson = new FavoriteSearchRepository(UnitOfWork).FindAllForPerson(person.Id.Value, WfmArea.Teams);

			Assert.AreEqual(1, allByPerson.Count());
		}

		[Test]
		public void CanFindFavoritesByPersonAndNameInGivenArea()
		{
			var name1 = "myfav";
			var name2 = "search1";
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav1 = new FavoriteSearch(name1);
			var fav2 = new FavoriteSearch(name2);
			fav1.Creator = person;
			fav1.WfmArea = WfmArea.Teams;
			fav2.Creator = person;
			fav2.WfmArea = WfmArea.Requests;
			PersistAndRemoveFromUnitOfWork(fav1);
			PersistAndRemoveFromUnitOfWork(fav2);

			var results = new FavoriteSearchRepository(UnitOfWork).FindByPersonAndName(person.Id.Value, name1, WfmArea.Teams);

			Assert.AreEqual(1, results.Count());
		}

		[Test]
		public void ShouldThrowExceptionWhenAddingFavoriteSearchWithSameNameForSamePersonInSameArea()
		{
			var name = "myfav";
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav1 = new FavoriteSearch(name);
			var duplicate = new FavoriteSearch(name);
			fav1.Creator = person;
			fav1.WfmArea = WfmArea.Teams;
			duplicate.Creator = person;
			duplicate.WfmArea = WfmArea.Teams;
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
