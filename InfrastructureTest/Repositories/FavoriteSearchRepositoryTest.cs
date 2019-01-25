using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class FavoriteSearchRepositoryTest : RepositoryTest<IFavoriteSearch>
	{
		private IPerson person;

		protected override void ConcreteSetup()
		{
			person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
		}

		protected override IFavoriteSearch CreateAggregateWithCorrectBusinessUnit()
		{
			var fav = new FavoriteSearch("testFav")
			{
				Creator = person,
				WfmArea = WfmArea.Teams,
				SearchTerm = "agent",
				TeamIds = "team1,team2",
				Status = FavoriteSearchStatus.Default
			};
			return fav;
		}

		protected override void VerifyAggregateGraphProperties(IFavoriteSearch loadedAggregateFromDatabase)
		{
			var origin = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(origin.Name, loadedAggregateFromDatabase.Name);
			Assert.AreEqual(origin.Creator, loadedAggregateFromDatabase.Creator);
			Assert.AreEqual(origin.WfmArea, loadedAggregateFromDatabase.WfmArea);
			Assert.AreEqual(origin.TeamIds, loadedAggregateFromDatabase.TeamIds);
			Assert.AreEqual(origin.SearchTerm, loadedAggregateFromDatabase.SearchTerm);
			Assert.AreEqual(origin.Status, loadedAggregateFromDatabase.Status);
		}

		protected override Repository<IFavoriteSearch> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new FavoriteSearchRepository(currentUnitOfWork);
		}
		
		[Test]
		public void CanFindFavoritesByPersonInArea()
		{
			var person = PersonFactory.CreatePerson("test");
			PersistAndRemoveFromUnitOfWork(person);
			var fav = new FavoriteSearch("myFav") {Creator = person, WfmArea = WfmArea.Teams};

			PersistAndRemoveFromUnitOfWork(fav);

			var allByPerson = new FavoriteSearchRepository(CurrUnitOfWork).FindAllForPerson(person, WfmArea.Teams);

			Assert.AreEqual(1, allByPerson.Count());
		}

		[Test]
		public void ShouldThrowExceptionWhenAddingFavoriteSearchWithSameNameForSamePersonInSameArea()
		{
			var name = "myfav";
			PersistAndRemoveFromUnitOfWork(person);
			var fav1 = new FavoriteSearch(name);
			var duplicate = new FavoriteSearch(name);
			fav1.Creator = person;
			fav1.WfmArea = WfmArea.Teams;
			duplicate.Creator = person;
			duplicate.WfmArea = WfmArea.Teams;
			PersistAndRemoveFromUnitOfWork(fav1);

			var target = new FavoriteSearchRepository(CurrUnitOfWork);
			Assert.Throws<Infrastructure.Foundation.ConstraintViolationException>(() =>
			{				
				target.Add(duplicate);
				UnitOfWork.PersistAll();				
			});
		}
	}
}
