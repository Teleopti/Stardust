using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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

		[Test]
		public void VerifyLoadCorrectSeatObject()
		{
			var role = new ApplicationRole {Name = "RoleForSeat"};
			PersistAndRemoveFromUnitOfWork(role);

			const string locationName = "Test Location";
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", locationName);
			var seat = seatMapLocation.AddSeat("Test Seat", 0);
			seat.AddRoles(role);
			PersistAndRemoveFromUnitOfWork(seatMapLocation);


			var loaded = new SeatMapLocationRepository(UnitOfWork).LoadAggregate(seatMapLocation.Id.Value) as SeatMapLocation;

			Assert.AreEqual(seatMapLocation.Id, loaded.Id);
			Assert.AreEqual(seatMapLocation.SeatCount, 1);
			Assert.AreEqual(seat.Name, loaded.Seats.First().Name);
			Assert.AreEqual(seat.Priority, loaded.Seats.First().Priority);
			Assert.AreEqual(seat.Roles.Count, loaded.Seats.First().Roles.Count);
			Assert.AreEqual(seat.Roles.First().Name, loaded.Seats.First().Roles.First().Name);
			Assert.AreEqual(seat.Roles.First().Id, loaded.Seats.First().Roles.First().Id);
		}

		[Test]
		public void VerifyCreatingCorrectNumberOfRecordsInApplicationRolesForSeatTable()
		{
			var role1 = new ApplicationRole { Name = "RoleForSeat1" };
			var role2 = new ApplicationRole { Name = "RoleForSeat2" };
			PersistAndRemoveFromUnitOfWork(new[] { role1, role2 });

			const string locationName = "Test Location";
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", locationName);
			var seat1 = seatMapLocation.AddSeat("Test Seat1", 0);
			seat1.AddRoles(role1, role2);
			var seat2 = seatMapLocation.AddSeat("Test Seat2", 1);
			seat2.AddRoles(role1);
			seatMapLocation.AddSeat("Test Seat3", 2);

			PersistAndRemoveFromUnitOfWork(seatMapLocation);

			var applicationRolesForSeatRecords = Session.CreateSQLQuery("Select * From ApplicationRolesForSeat").List();
			Assert.AreEqual(3, applicationRolesForSeatRecords.Count);
			Assert.AreEqual(seat1.Id, ((dynamic)applicationRolesForSeatRecords[0])[0]);
			Assert.AreEqual(seat1.Id, ((dynamic)applicationRolesForSeatRecords[1])[0]);
			Assert.AreEqual(seat2.Id, ((dynamic)applicationRolesForSeatRecords[2])[0]);
			Assert.AreEqual(role1.Id, ((dynamic)applicationRolesForSeatRecords[0])[1]);
			Assert.AreEqual(role2.Id, ((dynamic)applicationRolesForSeatRecords[1])[1]);
			Assert.AreEqual(role1.Id, ((dynamic)applicationRolesForSeatRecords[2])[1]);
		}

		[Test]
		public void VerifyDeleteSeatRemoveRelatedRoles()
		{
			var role1 = new ApplicationRole {Name = "RoleForSeat1"};
			var role2 = new ApplicationRole {Name = "RoleForSeat2"};
			PersistAndRemoveFromUnitOfWork(new[] {role1, role2});

			const string locationName = "Test Location";
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", locationName);
			var seat1 = seatMapLocation.AddSeat("Test Seat1", 0);
			var seat2 = seatMapLocation.AddSeat("Test Seat2", 1);
			seat1.AddRoles(role1, role2);
			seat2.AddRoles(role1);
			PersistAndRemoveFromUnitOfWork(seatMapLocation);

			seatMapLocation.Seats.Remove(seat1);
			PersistAndRemoveFromUnitOfWork(seatMapLocation);

			var applicationRolesForSeatRecords = Session.CreateSQLQuery("Select * From ApplicationRolesForSeat").List();
			Assert.AreEqual(1, applicationRolesForSeatRecords.Count);
			Assert.AreEqual(seat2.Id, ((dynamic)applicationRolesForSeatRecords[0])[0]);
			Assert.AreEqual(role1.Id, ((dynamic)applicationRolesForSeatRecords[0])[1]);
		}

		protected override Repository<ISeatMapLocation> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SeatMapLocationRepository(currentUnitOfWork);
		}
	}
}


