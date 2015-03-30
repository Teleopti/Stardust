using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	class SeatBookingRepositoryTest : RepositoryTest<ISeatBooking>
	{
		protected override void ConcreteSetup()
		{
		}

		protected override ISeatBooking CreateAggregateWithCorrectBusinessUnit()
		{
			var person = createPersonInDb();
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking (person, 
				new DateTime (2015, 10, 1, 8, 0, 0),
				new DateTime (2015, 10, 1, 17, 0, 0));
			booking.Book (seat);
			
			return booking;
		}

		protected override void VerifyAggregateGraphProperties(ISeatBooking loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.Id);
			Assert.IsNotNull(loadedAggregateFromDatabase.StartDateTime);
			Assert.IsNotNull(loadedAggregateFromDatabase.EndDateTime);
			Assert.IsNotNull(loadedAggregateFromDatabase.Seat);
			Assert.IsNotNull(loadedAggregateFromDatabase.Person);
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			var person = createPersonInDb();
			var seat = createSeatMapLocationAndSeatInDb();
			var start = new DateTime (2015, 10, 1, 8, 0, 0);
			var end = new DateTime (2015, 10, 1, 17, 0, 0);
			var booking = new SeatBooking (person, start, end);

			booking.Book (seat);
			PersistAndRemoveFromUnitOfWork(booking);

			var loaded = new SeatBookingRepository(UnitOfWork).LoadAggregate(booking.Id.Value) as SeatBooking;

			Assert.AreEqual(booking.Id, loaded.Id);
			Assert.AreEqual(booking.Seat, seat);
			Assert.AreEqual(booking.StartDateTime, start);
			Assert.AreEqual(booking.EndDateTime, end);
		}

		[Test]
		public void VerifyLoadBookingsForPeriod()
		{
			var person = createPersonInDb();
			var person2 = createPersonInDb();
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateTime(2015, 10, 2, 8, 0, 0),
				new DateTime(2015, 10, 2, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateTime(2015, 10, 1, 13, 0, 0),
				new DateTime(2015, 10, 1, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsForDateOnlyPeriod(new DateOnlyPeriod(2015, 10, 1, 2015, 10, 1));

			Assert.AreEqual(seatBookings.Single(),booking2 );
			
		}

		[Test]
		public void VerifyLoadBookingsForDay()
		{
			var person = createPersonInDb();
			var person2 = createPersonInDb();
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateTime(2015, 10, 1, 13, 0, 0),
				new DateTime(2015, 10, 1, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsForDay(new DateOnly(2015, 10, 1));

			Assert.AreEqual(seatBookings.Count, 2);

		}
			
		private Person createPersonInDb()
		{
			var rep = new Repository(UnitOfWork);
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			rep.Add(person);
			return person;
		}

		private ISeat createSeatMapLocationAndSeatInDb()
		{
			var rep = new Repository(UnitOfWork);
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation.AddSeat("Test Seat", 0);
			rep.Add(seatMapLocation);
			return seatMapLocation.Seats.First();
		}

		protected override Repository<ISeatBooking> TestRepository(IUnitOfWork unitOfWork)
		{
			return new SeatBookingRepository(unitOfWork);
		}
	}
}