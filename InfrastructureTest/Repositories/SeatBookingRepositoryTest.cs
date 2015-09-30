using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Rhino.Mocks;
using SharpTestsEx;

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
			var startDate = new DateOnly(2015, 10, 1);
			var person = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 17, 0, 0));
			booking.Book(seat);

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
			var startDate = new DateOnly(2015, 10, 1);
			var person = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var start = new DateTime(2015, 10, 1, 8, 0, 0);
			var end = new DateTime(2015, 10, 1, 17, 0, 0);
			var booking = new SeatBooking(person, new DateOnly(2015, 10, 1), start, end);

			booking.Book(seat);
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
			var startDate = new DateOnly(2015, 10, 1);
			var person = createPerson(startDate);
			var person2 = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0),
				new DateTime(2015, 10, 2, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 13, 0, 0),
				new DateTime(2015, 10, 1, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsForDateOnlyPeriod(new DateOnlyPeriod(2015, 10, 1, 2015, 10, 1));

			Assert.AreEqual(seatBookings.Single(), booking2);

		}

		[Test]
		public void VerifyLoadBookingsForDay()
		{
			var startDate = new DateOnly(2015, 10, 1);
			var person = createPerson(startDate);
			var person2 = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 13, 0, 0),
				new DateTime(2015, 10, 1, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsForDay(new DateOnly(2015, 10, 1));

			Assert.AreEqual(seatBookings.Count, 2);

		}

		[Test]
		public void ShouldLoadBookingsThatIntersectWithDay()
		{
			var utcStartDateTime = new DateTime (2015, 10, 2, 0, 0, 0, DateTimeKind.Utc);
			var startDate = new DateOnly(2015,10,1);
			var person = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var bookingOnCurrentDay = new SeatBooking(person,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0),
				new DateTime(2015, 10, 2, 12, 0, 0));
			var bookingOnPreviousDay = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 23, 0, 0),
				new DateTime(2015, 10, 2, 7, 0, 0));
			var bookingOnNextDay = new SeatBooking(person,
				new DateOnly(2015, 10, 3),
				new DateTime(2015, 10, 2, 23, 0, 0),
				new DateTime(2015, 10, 3, 7, 0, 0));

			bookingOnCurrentDay.Book(seat);
			bookingOnPreviousDay.Book(seat);
			bookingOnNextDay.Book(seat);

			PersistAndRemoveFromUnitOfWork(bookingOnCurrentDay);
			PersistAndRemoveFromUnitOfWork(bookingOnPreviousDay);
			PersistAndRemoveFromUnitOfWork(bookingOnNextDay);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsIntersectingDay(new DateOnly(utcStartDateTime), seat.Parent.Id.Value);

			Assert.AreEqual(seatBookings.Count, 3);
		}


		[Test]
		public void ShouldLoadBookingsIntersectingWithDayForLocation()
		{
			var utcStartDateTime = new DateTime(2015, 10, 2, 0, 0, 0, DateTimeKind.Utc);
			var startDate = new DateOnly(2015, 10, 1);
			var person = createPerson(startDate);
			var seat = createSeatMapLocationAndSeatInDb();
			var seat2 = createSeatMapLocationAndSeatInDb();

			var bookingOnCurrentDay = new SeatBooking(person,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0),
				new DateTime(2015, 10, 2, 12, 0, 0));
			var bookingOnPreviousDay = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 23, 0, 0),
				new DateTime(2015, 10, 2, 7, 0, 0));


			bookingOnCurrentDay.Book(seat2);
			bookingOnPreviousDay.Book(seat);

			PersistAndRemoveFromUnitOfWork(bookingOnCurrentDay);
			PersistAndRemoveFromUnitOfWork(bookingOnPreviousDay);

			var seatBookings = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsIntersectingDay(new DateOnly(utcStartDateTime), seat.Parent.Id.Value);

			Assert.AreEqual(seatBookings.Count, 1);
		}

		[Test]
		public void ShouldFilterSeatBookingReportByPeriod()
		{

			var startDate = new DateOnly(2015, 10, 1);

			var team = createTeam ("team");

			var person = createPerson(startDate, team);
			var person2 = createPerson(startDate, team);
			var seat = createSeatMapLocationAndSeatInDb();
			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0),
				new DateTime(2015, 10, 2, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);
			
			var criteria = new SeatBookingReportCriteria()
			{
				Period = new DateOnlyPeriod(new DateOnly(2015, 10, 1), new DateOnly(2015, 10, 2))
			};

			var viewModel = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(2, viewModel.SeatBookings.Count());
			Assert.IsTrue(viewModel.SeatBookings.First().PersonId== person.Id);
			Assert.IsTrue(viewModel.SeatBookings.First().SeatId == seat.Id);

		}

		[Test]
		public void ShouldFilterSeatBookingReportByLocation()
		{
			var startDate = new DateOnly(2015, 10, 1);

			var person = createPerson(startDate);
			var person2 = createPerson(startDate);
			
			var seatMapLocation = new SeatMapLocation();
			var seatMapLocation2 = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation2.SetLocation("{DummyData}", "TestLocation2");

			var seatLocation1 = seatMapLocation.AddSeat("Test Seat", 0);
			var seatLocation2 = seatMapLocation2.AddSeat("Test Seat", 0);

			PersistAndRemoveFromUnitOfWork(seatMapLocation);
			PersistAndRemoveFromUnitOfWork(seatMapLocation2);

			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0),
				new DateTime(2015, 10, 2, 17, 0, 0));

			booking.Book(seatLocation1);
			booking2.Book(seatLocation2);
			
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);


			var criteria = new SeatBookingReportCriteria()
			{
				Locations = new List<SeatMapLocation>() { seatMapLocation2 },
				Period = new DateOnlyPeriod(new DateOnly(2015, 10, 1), new DateOnly(2015, 10, 2))
			};

			var viewModel = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(1, viewModel.SeatBookings.Count());
			Assert.AreEqual(viewModel.SeatBookings.First().PersonId, person2.Id);
			Assert.AreEqual(viewModel.SeatBookings.First().SeatId, seatLocation2.Id);
			Assert.AreEqual (viewModel.RecordCount, 1);

		}
		
		[Test]
		public void ShouldFilterSeatBookingReportByTeam()
		{
			var startDate = new DateOnly(2015, 10, 1);

			var person = createPerson(startDate);
			var person2 = createPerson(startDate);
			var rep = new SeatMapLocationRepository(UnitOfWork);

			var seatMapLocation = new SeatMapLocation();

			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			var seat = seatMapLocation.AddSeat("Test Seat", 0);

			rep.Add(seatMapLocation);

			var booking = new SeatBooking(person,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0),
				new DateTime(2015, 10, 1, 12, 0, 0));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0),
				new DateTime(2015, 10, 2, 17, 0, 0));

			booking.Book(seat);
			booking2.Book(seat);

			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);


			var criteria = new SeatBookingReportCriteria()
			{
				Teams = new List<Team>() { (Team)person2.MyTeam(new DateOnly(2015, 10, 2)) },
				Period = new DateOnlyPeriod(new DateOnly(2015, 10, 1), new DateOnly(2015, 10, 2))
			};

			var viewModel = new SeatBookingRepository(UnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(1, viewModel.SeatBookings.Count());
			Assert.AreEqual(viewModel.SeatBookings.First().PersonId, person2.Id);
		}

		[Test]
		public void LoadSeatBookingReportForPeriodShouldPageCorrectly()
		{
			var dateOnly = new DateOnly(2015, 10, 1);
			var person = createPerson(dateOnly);
			var person2 = createPerson(dateOnly);
			var seat = createSeatMapLocationAndSeatInDb();

			Enumerable.Range(0, 20).ForEach(count =>
			{
				var morningBooking = new SeatBooking(person,
					dateOnly,
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 8, 0, 0),
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 12, 0, 0));

				var afternoonBooking = new SeatBooking(person2,
					dateOnly,
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 13, 0, 0),
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 17, 0, 0));

				morningBooking.Book(seat);
				afternoonBooking.Book(seat);
				PersistAndRemoveFromUnitOfWork(morningBooking);
				PersistAndRemoveFromUnitOfWork(afternoonBooking);

				updatePersonScheduleDayFromBooking(morningBooking);
				updatePersonScheduleDayFromBooking(afternoonBooking);
				
				dateOnly = dateOnly.AddDays(1);
			});

			var repo = new SeatBookingRepository(UnitOfWork);

			var criteria = new SeatBookingReportCriteria()
			{
				Period = new DateOnlyPeriod(new DateOnly(2015, 10, 1), new DateOnly(2015, 10, 19))
			};

			var viewModel = repo.LoadSeatBookingsReport(criteria, new Paging() { Skip = 4, Take = 4 });

			Assert.AreEqual(4, viewModel.SeatBookings.Count());
			Assert.AreEqual(38, viewModel.RecordCount); //38 as criteria stops on the 19th October
			Assert.IsTrue(viewModel.SeatBookings.First().BelongsToDate == new DateOnly(2015, 10, 3));

		}

		[Test]
		public void ShouldLoadSeatBookingsForSeatIntersectingDayInOrder()
		{
			var dateOnly = new DateOnly(2015, 10, 1);
			var person = createPerson(dateOnly);
			var person2 = createPerson(dateOnly);
			var seat = createSeatMapLocationAndSeatInDb();
			

			var morningBooking = new SeatBooking(person, dateOnly,
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 8, 0, 0),
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 12, 0, 0));

			var afternoonBooking = new SeatBooking(person2, dateOnly,
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 13, 0, 0),
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 17, 0, 0));

			morningBooking.Book(seat);
			afternoonBooking.Book(seat);

			PersistAndRemoveFromUnitOfWork(afternoonBooking);
			PersistAndRemoveFromUnitOfWork(morningBooking);
			//updatePersonScheduleDayFromBooking(afternoonBooking);
			//updatePersonScheduleDayFromBooking(morningBooking);


			var repo = new SeatBookingRepository(UnitOfWork);

			var seatBookings = repo.LoadSeatBookingsForSeatIntersectingDay(dateOnly, seat.Id.GetValueOrDefault());
			seatBookings.Count().Should().Be(2);
			seatBookings.First().StartDateTime.Hour.Should().Be(8);
			seatBookings.Second().StartDateTime.Hour.Should().Be(13);
		}


		[Test]
		public void ShouldLoadSeatBookingsForMultipleSeatsIntersectingDayInOrder()
		{
			var dateOnly = new DateOnly(2015, 10, 1);
			var person = createPerson(dateOnly);
			
			var rep = new SeatMapLocationRepository(UnitOfWork);
			var seatMapLocation = new SeatMapLocation();

			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			rep.Add(seatMapLocation);

			var seat1 = seatMapLocation.AddSeat("Test Seat", 0);
			var seat2 = seatMapLocation.AddSeat("Test Seat 2", 0);
			var seat3 = seatMapLocation.AddSeat("Test Seat 3", 0);

			var morningBooking = new SeatBooking(person, dateOnly,
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 8, 0, 0),
					new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 12, 0, 0));

			var afternoonBooking = new SeatBooking(person, dateOnly,
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 13, 0, 0),
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 17, 0, 0));

			var eveningBooking = new SeatBooking(person, dateOnly,
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 18, 0, 0),
				new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 23, 0, 0));
			
			morningBooking.Book (seat1);
			afternoonBooking.Book (seat2);
			eveningBooking.Book (seat3);

			PersistAndRemoveFromUnitOfWork(morningBooking);
			PersistAndRemoveFromUnitOfWork(afternoonBooking);
			PersistAndRemoveFromUnitOfWork(eveningBooking);
			
			var repo = new SeatBookingRepository(UnitOfWork);

			var seatBookings = repo.LoadSeatBookingsForSeatsIntersectingDay(dateOnly, new []{
				seat1.Id.GetValueOrDefault(), 
				seat2.Id.GetValueOrDefault()} 
			);
			
			seatBookings.Count().Should().Be(2);
			seatBookings[0].Seat.Id.Should().Equals (seat1.Id);
			seatBookings[1].Seat.Id.Should().Equals(seat2.Id);
		}

		private IPerson createPerson(DateOnly startDate, Team team = null)
		{
			team = team ?? createTeam("Team");

			var contract1 = new Contract("contract1");
			var contract2 = new Contract("contract2");
			PersistAndRemoveFromUnitOfWork(contract1);
			PersistAndRemoveFromUnitOfWork(contract2);

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			person.AddPersonPeriod(new PersonPeriod(startDate, createPersonContract(contract1), team));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			PersistAndRemoveFromUnitOfWork(person);

			return person;
		}

		private Team createTeam (String name)
		{
			var site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(site);


			var team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description (name);
			PersistAndRemoveFromUnitOfWork (team);
			return team;
		}


		private IPersonContract createPersonContract(IContract contract, IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract(contract);
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


		private ISeat createSeatMapLocationAndSeatInDb()
		{
			var rep = new SeatMapLocationRepository(UnitOfWork);
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation.AddSeat("Test Seat", 0);
			rep.Add(seatMapLocation);
			return seatMapLocation.Seats.First();
		}

		private static void updatePersonScheduleDayFromBooking(SeatBooking booking)
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(uow, MockRepository.GenerateMock<IMessageBrokerComposite>(),
				MockRepository.GenerateMock<ICurrentDataSource>());

			var model = new PersonScheduleDayReadModel
			{
				Date = booking.StartDateTime,
				TeamId = booking.Person.MyTeam(booking.BelongsToDate).Id.GetValueOrDefault(),
				PersonId = booking.Person.Id.GetValueOrDefault(),
				BusinessUnitId = booking.BusinessUnit.Id.GetValueOrDefault(),
				IsDayOff = false,
				Start = booking.StartDateTime,
				End = booking.EndDateTime,
				Model = "{shift: blablabla}",
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId,
				model.BusinessUnitId, new[] { model }, false);
		}

		protected override Repository<ISeatBooking> TestRepository(IUnitOfWork unitOfWork)
		{
			return new SeatBookingRepository(unitOfWork);
		}
	}
}