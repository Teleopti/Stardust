using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	class SeatMapRepositoryTest: RepositoryTest<ISeatMap>
	{
		protected override void ConcreteSetup()
		{
		}

		protected override ISeatMap CreateAggregateWithCorrectBusinessUnit()
		{
			const string locationName = "Test Location";
			var seatMap = new SeatMap();
			seatMap.CreateSeatMap("{DummyData}", locationName);
			return seatMap;
		}
		
		protected override void VerifyAggregateGraphProperties(ISeatMap loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.SeatMapJsonData);
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			const string locationName = "Test Location";

			var seatMap = new SeatMap();
			seatMap.CreateSeatMap("{DummyData}", locationName);
			PersistAndRemoveFromUnitOfWork(seatMap);
			
			var loaded = new SeatMapRepository(UnitOfWork).LoadAggregate(seatMap.Id.Value) as SeatMap;

			Assert.AreEqual(seatMap.Id, loaded.Id);
			Assert.AreEqual(locationName, loaded.Location.Name);
			Assert.AreEqual (seatMap.Location.Id, loaded.Location.Id);
			Assert.AreEqual(seatMap.SeatMapJsonData, loaded.SeatMapJsonData);
		}

		[Test]
		public void VerifyLoadRootSeatMap()
		{
			const string locationName = "Test Location";

			var seatMap = new SeatMap();
			seatMap.CreateSeatMap("{DummyData}", locationName);
			PersistAndRemoveFromUnitOfWork(seatMap);

			var loaded = new SeatMapRepository(UnitOfWork).LoadRootSeatMap() as SeatMap;

			Assert.AreEqual(seatMap.Id, loaded.Id);
			Assert.AreEqual(locationName, loaded.Location.Name);
			Assert.AreEqual(seatMap.Location.Id, loaded.Location.Id);
			Assert.AreEqual(seatMap.SeatMapJsonData, loaded.SeatMapJsonData);
		}

		protected override Repository<ISeatMap> TestRepository (IUnitOfWork unitOfWork)
		{
			return new SeatMapRepository(unitOfWork);
		}
	}
}


