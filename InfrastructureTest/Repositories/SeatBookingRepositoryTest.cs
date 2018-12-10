using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Messaging.Client.Composite;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	class SeatBookingRepositoryTest : RepositoryTest<ISeatBooking>
	{
		private readonly DateOnly startDate = new DateOnly(2015, 10, 1);
		
		private ISeat seat1;
		private ISeat seat2;
		private IPerson person1;
		private IPerson person2;

		protected override void ConcreteSetup()
		{
			var seatMapLocation1 = new SeatMapLocation();
			seatMapLocation1.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation1.AddSeat("Test Seat", 0);
			
			var seatMapLocation2 = new SeatMapLocation();
			seatMapLocation2.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation2.AddSeat("Test Seat", 0);

			PersistAndRemoveFromUnitOfWork(seatMapLocation1);
			PersistAndRemoveFromUnitOfWork(seatMapLocation2);
			seat1 = seatMapLocation1.Seats.First();
			seat2 = seatMapLocation2.Seats.First();
			
			var contract1 = new Contract("contract1");
			PersistAndRemoveFromUnitOfWork(contract1);

			var team1 = TeamFactory.CreateTeam("Team 1","Site 1");
			var team2 = new Team {Site = team1.Site}.WithDescription(new Description("Team 2"));
			PersistAndRemoveFromUnitOfWork(team1.Site);
			PersistAndRemoveFromUnitOfWork(team1);
			PersistAndRemoveFromUnitOfWork(team2);
			
			var pContract = PersonContractFactory.CreatePersonContract(contract1);
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			
			person1 = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			person1.AddPersonPeriod(new PersonPeriod(startDate, pContract, team1));

			person2 = PersonFactory.CreatePerson("Yngwie2", "Malmsteen2");
			person2.AddPersonPeriod(new PersonPeriod(startDate, pContract, team2));

			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
		}

		protected override ISeatBooking CreateAggregateWithCorrectBusinessUnit()
		{
			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));
			booking.Book(seat1);

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
			var start = new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			
			var booking = new SeatBooking(person1, new DateOnly(2015, 10, 1), start, end);

			booking.Book(seat1);
			PersistAndRemoveFromUnitOfWork(booking);

			var loaded = new SeatBookingRepository(CurrUnitOfWork).Get(booking.Id.GetValueOrDefault());

			Assert.AreEqual(booking.Id, loaded.Id);
			Assert.AreEqual(booking.Seat, seat1);
			Assert.AreEqual(booking.StartDateTime, start);
			Assert.AreEqual(booking.EndDateTime, end);
		}

		[Test]
		public void VerifyLoadBookingsForPeriod()
		{
			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 12, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			booking.Book(seat1);
			booking2.Book(seat1);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsForDateOnlyPeriod(new DateOnlyPeriod(2015, 10, 1, 2015, 10, 1));

			Assert.AreEqual(seatBookings.Single(), booking2);
		}

		[Test]
		public void VerifyLoadBookingsForDay()
		{
			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 12, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			booking.Book(seat1);
			booking2.Book(seat1);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			var seatBookings = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsForDay(new DateOnly(2015, 10, 1));

			Assert.AreEqual(seatBookings.Count, 2);
		}

		[Test]
		public void ShouldLoadBookingsThatIntersectWithDateTimePeriod()
		{
			var targetDateTime = new DateTime(2015, 10, 2, 0, 0, 0, DateTimeKind.Utc);
			
			var bookingOnCurrentDay = new SeatBooking(person1,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 12, 0, 0, DateTimeKind.Utc));
			var bookingOnPreviousDay = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 7, 0, 0, DateTimeKind.Utc));
			var bookingOnNextDay = new SeatBooking(person1,
				new DateOnly(2015, 10, 3),
				new DateTime(2015, 10, 2, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 3, 7, 0, 0, DateTimeKind.Utc));

			bookingOnCurrentDay.Book(seat1);
			bookingOnPreviousDay.Book(seat1);
			bookingOnNextDay.Book(seat1);

			PersistAndRemoveFromUnitOfWork(bookingOnCurrentDay);
			PersistAndRemoveFromUnitOfWork(bookingOnPreviousDay);
			PersistAndRemoveFromUnitOfWork(bookingOnNextDay);

			var dateTimePeriod = new DateTimePeriod(targetDateTime, targetDateTime.AddDays(1).AddSeconds(-1));
			var seatBookings = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriod, seat1.Parent.Id.Value);

			Assert.AreEqual(seatBookings.Count, 3);
		}
		
		[Test]
		public void ShouldLoadBookingsIntersectingWithDateTimePeriodForLocation()
		{
			var targetDateTime = new DateTime(2015, 10, 2, 0, 0, 0, DateTimeKind.Utc);
			
			var bookingOnCurrentDay = new SeatBooking(person1,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 12, 0, 0, DateTimeKind.Utc));
			var bookingOnPreviousDay = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 7, 0, 0, DateTimeKind.Utc));


			bookingOnCurrentDay.Book(seat2);
			bookingOnPreviousDay.Book(seat1);

			PersistAndRemoveFromUnitOfWork(bookingOnCurrentDay);
			PersistAndRemoveFromUnitOfWork(bookingOnPreviousDay);

			var dateTimePeriod = new DateTimePeriod(targetDateTime, targetDateTime.AddDays(1).AddSeconds(-1));
			var seatBookings = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriod, seat1.Parent.Id.Value);

			Assert.AreEqual(seatBookings.Count, 1);
		}

		[Test]
		public void ShouldFilterSeatBookingReportByPeriod()
		{
			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 12, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 17, 0, 0, DateTimeKind.Utc));

			booking.Book(seat1);
			booking2.Book(seat1);
			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);

			var criteria = new SeatBookingReportCriteria
			{
				Period = new DateOnlyPeriod(2015, 10, 1,2015, 10, 2)
			};

			var viewModel = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(2, viewModel.SeatBookings.Count());
			Assert.IsTrue(viewModel.SeatBookings.First().PersonId == person1.Id);
			Assert.IsTrue(viewModel.SeatBookings.First().SeatId == seat1.Id);

		}

		[Test]
		public void ShouldFilterSeatBookingReportByLocation()
		{
			var seatMapLocation = new SeatMapLocation();
			var seatMapLocation2 = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation2.SetLocation("{DummyData}", "TestLocation2");

			var seatLocation1 = seatMapLocation.AddSeat("Test Seat", 0);
			var seatLocation2 = seatMapLocation2.AddSeat("Test Seat", 0);

			PersistAndRemoveFromUnitOfWork(seatMapLocation);
			PersistAndRemoveFromUnitOfWork(seatMapLocation2);

			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 12, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 17, 0, 0, DateTimeKind.Utc));

			booking.Book(seatLocation1);
			booking2.Book(seatLocation2);

			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);
			
			var criteria = new SeatBookingReportCriteria
			{
				Locations = new List<SeatMapLocation> { seatMapLocation2 },
				Period = new DateOnlyPeriod(2015, 10, 1,2015, 10, 2)
			};

			var viewModel = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(1, viewModel.SeatBookings.Count());
			Assert.AreEqual(viewModel.SeatBookings.First().PersonId, person2.Id);
			Assert.AreEqual(viewModel.SeatBookings.First().SeatId, seatLocation2.Id);
			Assert.AreEqual(viewModel.RecordCount, 1);
		}

		[Test]
		public void ShouldFilterSeatBookingReportByTeam()
		{
			var rep = new SeatMapLocationRepository(CurrUnitOfWork);

			var seatMapLocation = new SeatMapLocation();

			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			var seat = seatMapLocation.AddSeat("Test Seat", 0);

			rep.Add(seatMapLocation);

			var booking = new SeatBooking(person1,
				new DateOnly(2015, 10, 1),
				new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 1, 12, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(person2,
				new DateOnly(2015, 10, 2),
				new DateTime(2015, 10, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 2, 17, 0, 0, DateTimeKind.Utc));

			booking.Book(seat);
			booking2.Book(seat);

			PersistAndRemoveFromUnitOfWork(booking);
			PersistAndRemoveFromUnitOfWork(booking2);

			updatePersonScheduleDayFromBooking(booking);
			updatePersonScheduleDayFromBooking(booking2);
			
			var criteria = new SeatBookingReportCriteria
			{
				Teams = new List<Team> { (Team)person2.MyTeam(new DateOnly(2015, 10, 2)) },
				Period = new DateOnlyPeriod(new DateOnly(2015, 10, 1), new DateOnly(2015, 10, 2))
			};

			var viewModel = new SeatBookingRepository(CurrUnitOfWork).LoadSeatBookingsReport(criteria);

			Assert.AreEqual(1, viewModel.SeatBookings.Count());
			Assert.AreEqual(viewModel.SeatBookings.First().PersonId, person2.Id);
		}

		[Test]
		public void LoadSeatBookingReportForPeriodShouldPageCorrectly()
		{
			var dateOnly = startDate;
			
			Enumerable.Range(0, 20).ForEach(count =>
			{

				var startDateTime = DateTime.SpecifyKind(dateOnly.Date, DateTimeKind.Utc);
				
				var morningBooking = new SeatBooking(person1,
					dateOnly,
					startDateTime.AddHours(8),
					startDateTime.AddHours(12));

				var afternoonBooking = new SeatBooking(person2,
					dateOnly,
					startDateTime.AddHours(13),
					startDateTime.AddHours(17));

				morningBooking.Book(seat1);
				afternoonBooking.Book(seat1);
				PersistAndRemoveFromUnitOfWork(morningBooking);
				PersistAndRemoveFromUnitOfWork(afternoonBooking);

				updatePersonScheduleDayFromBooking(morningBooking);
				updatePersonScheduleDayFromBooking(afternoonBooking);

				dateOnly = dateOnly.AddDays(1);
			});

			var repo = new SeatBookingRepository(CurrUnitOfWork);

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
		public void LoadSeatBookingReportShowOnlyUnseatedCriteriaFalseShouldLoadAll()
		{
			var viewModel = doShowOnlyUnseatedCriteriaTest(false);
			Assert.AreEqual(2, viewModel.SeatBookings.Count());
		}


		[Test]
		public void LoadSeatBookingReportShowOnlyUnseatedCriteriaFalseShouldShowOnlyAgentsWithNoSeat()
		{
			var viewModel = doShowOnlyUnseatedCriteriaTest(true);

			Assert.AreEqual(1, viewModel.SeatBookings.Count());
			var seatBooking = viewModel.SeatBookings.FirstOrDefault();
			if (seatBooking != null)
			{
				Assert.AreEqual (seatBooking.PersonId, person2.Id);
				Assert.IsTrue(seatBooking.SeatId == Guid.Empty);
			}
		}

		private ISeatBookingReportModel doShowOnlyUnseatedCriteriaTest(bool showOnlyUnseated)
		{
			var dateOnly = startDate;
			var startDateTime = DateTime.SpecifyKind(dateOnly.Date, DateTimeKind.Utc);
			
			var morningBooking = new SeatBooking (person1,
				dateOnly,
				startDateTime.AddHours (8),
				startDateTime.AddHours (12));

			morningBooking.Book (seat1);
			PersistAndRemoveFromUnitOfWork (morningBooking);
			updatePersonScheduleDayFromBooking (morningBooking);

			updatePersonScheduleDay (dateOnly.Date.AddHours (13), dateOnly.Date.AddHours (17), person2, morningBooking.BusinessUnit);

			var repo = new SeatBookingRepository (CurrUnitOfWork);

			var criteria = new SeatBookingReportCriteria()
			{
				Period = new DateOnlyPeriod (new DateOnly (2015, 10, 1), new DateOnly (2015, 10, 19)),
				ShowOnlyUnseated = showOnlyUnseated
			};

			var viewModel = repo.LoadSeatBookingsReport (criteria);
			return viewModel;
		}

		[Test]
		public void ShouldLoadSeatBookingsForSeatIntersectingDayInOrder()
		{
			var startDateTime = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
			
			var morningBooking = new SeatBooking(person1, startDate,
					startDateTime.AddHours(8),
					startDateTime.AddHours(12));

			var afternoonBooking = new SeatBooking(person2, startDate,
				startDateTime.AddHours(13),
				startDateTime.AddHours(17));

			morningBooking.Book(seat1);
			afternoonBooking.Book(seat1);

			PersistAndRemoveFromUnitOfWork(afternoonBooking);
			PersistAndRemoveFromUnitOfWork(morningBooking);

			var repo = new SeatBookingRepository(CurrUnitOfWork);

			var targetDateTime = new DateTime(2015, 10, 1, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(targetDateTime, targetDateTime.AddDays(1).AddSeconds(-1));

			var seatBookings = repo.LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriod, new[] { seat1.Id.GetValueOrDefault() });
			seatBookings.Count.Should().Be(2);
			seatBookings.First().StartDateTime.Hour.Should().Be(8);
			seatBookings.Second().StartDateTime.Hour.Should().Be(13);
		}
		
		[Test]
		public void ShouldLoadSeatBookingsForMultipleSeatsIntersectingDayInOrder()
		{
			var rep = new SeatMapLocationRepository(CurrUnitOfWork);
			var seatMapLocation = new SeatMapLocation();

			seatMapLocation.SetLocation("{DummyData}", "TestLocation");

			var testSeat1 = seatMapLocation.AddSeat("Test Seat", 0);
			var testSeat2 = seatMapLocation.AddSeat("Test Seat 2", 0);
			var testSeat3 = seatMapLocation.AddSeat("Test Seat 3", 0);
			
			var startDateTime = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);

			rep.Add(seatMapLocation);

			var morningBooking = new SeatBooking(person1, startDate,
					startDateTime.AddHours(8),
					startDateTime.AddHours(12));

			var afternoonBooking = new SeatBooking(person1, startDate,
				startDateTime.AddHours(13),
				startDateTime.AddHours(17));

			var eveningBooking = new SeatBooking(person1, startDate,
				startDateTime.AddHours(18),
				startDateTime.AddHours(23));

			morningBooking.Book(testSeat1);
			afternoonBooking.Book(testSeat2);
			eveningBooking.Book(testSeat3);

			PersistAndRemoveFromUnitOfWork(morningBooking);
			PersistAndRemoveFromUnitOfWork(afternoonBooking);
			PersistAndRemoveFromUnitOfWork(eveningBooking);

			var repo = new SeatBookingRepository(CurrUnitOfWork);
			
			var targetDateTime = new DateTime(2015, 10, 1, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(targetDateTime, targetDateTime.AddDays(1).AddSeconds(-1));

			var seatBookings = repo.LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriod, new[]{
				testSeat1.Id.GetValueOrDefault(), 
				testSeat2.Id.GetValueOrDefault()}
			);

			seatBookings.Count.Should().Be(2);
			seatBookings[0].Seat.Id.Should().Equals(testSeat1.Id);
			seatBookings[1].Seat.Id.Should().Equals(testSeat2.Id);
		}
		
		private static void updatePersonScheduleDayFromBooking(SeatBooking booking)
		{
			updatePersonScheduleDay (booking.StartDateTime, booking.EndDateTime, booking.Person,booking.BusinessUnit);
		}


		private static void updatePersonScheduleDay(DateTime startDateTime, DateTime endDateTime, IPerson person, IAggregateRoot businessUnit)
		{
			var uow = CurrentUnitOfWork.Make();
			var target = new PersonScheduleDayReadModelPersister(uow, new DoNotSend(), 
				new FakeCurrentDatasource("dummy"));

			var model = new PersonScheduleDayReadModel
			{
				Date = startDateTime,			
				PersonId = person.Id.GetValueOrDefault(),
				IsDayOff = false,
				Start = startDateTime,
				End = endDateTime,
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow
			};

			target.UpdateReadModels(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)), model.PersonId,
				businessUnit.Id.GetValueOrDefault(), new[] { model }, false);
		}


		protected override Repository<ISeatBooking> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SeatBookingRepository(currentUnitOfWork);
		}
	}
}