using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	class SeatMapLocationRepositoryTest: RepositoryTest<ISeatMapLocation>
	{
		protected override void ConcreteSetup()
		{
		}

		protected override ISeatMapLocation CreateAggregateWithCorrectBusinessUnit()
		{
			var location = new SeatMapLocation();
			location.SetLocation("{DummyData}", "Test Location");
			return location;
		}
		
		protected override void VerifyAggregateGraphProperties(ISeatMapLocation loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.SeatMapJsonData);
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			const string locationName = "Test Location";
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", locationName);
			seatMapLocation.AddSeat ("Test Seat", 0);

			PersistAndRemoveFromUnitOfWork(seatMapLocation);
			
			var loaded = new SeatMapLocationRepository(UnitOfWork).LoadAggregate(seatMapLocation.Id.Value) as SeatMapLocation;

			Assert.AreEqual(seatMapLocation.Id, loaded.Id);
			Assert.AreEqual(locationName, loaded.Name);
			Assert.AreEqual (seatMapLocation.Id, loaded.Id);
			Assert.AreEqual (seatMapLocation.SeatCount, 1);
			Assert.AreEqual(seatMapLocation.SeatMapJsonData, loaded.SeatMapJsonData);
		}

		[Test]
		public void VerifyLoadRootSeatMap()
		{
			const string locationName = "Test Location";

			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", locationName);
			PersistAndRemoveFromUnitOfWork(seatMapLocation);

			var loaded = new SeatMapLocationRepository(UnitOfWork).LoadRootSeatMap() as SeatMapLocation;

			Assert.AreEqual(seatMapLocation.Id, loaded.Id);
			Assert.AreEqual(locationName, loaded.Name);
			Assert.AreEqual(seatMapLocation.Id, loaded.Id);
			Assert.AreEqual(seatMapLocation.SeatMapJsonData, loaded.SeatMapJsonData);
		}

		protected override Repository<ISeatMapLocation> TestRepository (ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SeatMapLocationRepository(currentUnitOfWork);
		}
	}
}


