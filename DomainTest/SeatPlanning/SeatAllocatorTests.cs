using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	public class SeatAllocatorTests
	{
		[Test]
		public void ShouldAllocateTwoIntersectingBookingRequestsOverTwoLocations()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));
			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);

			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 12, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var seatBookingRequest2 = new SeatBookingRequest (agentShift2);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1, seatBookingRequest2);

			var allocatedSeats = new[] {agentShift1.Seat.Name, agentShift2.Seat.Name};
			Assert.That (allocatedSeats.Contains ("L1 Seat1") && allocatedSeats.Contains ("L2 Seat1"));
		}

		[Test]
		public void ShouldAllocateAgentGroupsTogether()
		{

			var startDateTime = new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc);

			var agentShift1 = new SeatBooking(PersonFactory.CreatePerson("Agent1"), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours (8));
			var agentShift2 = new SeatBooking(PersonFactory.CreatePerson("Agent2"), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours(8));
			var agentShift3 = new SeatBooking(PersonFactory.CreatePerson("Agent3"), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours(8));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);
			var seatBookingRequest2 = new SeatBookingRequest (agentShift2, agentShift3);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.Name = "Location1";
			location1.AddSeat ("L1 Seat1", 1);
			location1.AddSeat ("L1 Seat2", 2);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.Name = "Location2";
			location2.AddSeat ("L2 Seat1", 1);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1, seatBookingRequest2);

			Assert.AreEqual ("L2 Seat1", agentShift1.Seat.Name);
			Assert.AreEqual("L1 Seat1", agentShift2.Seat.Name);
			Assert.AreEqual("L1 Seat2", agentShift3.Seat.Name);
		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherForSecondLocation()
		{
			var startDateTime = new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc);

			var agentShift1 = new SeatBooking(new Person(), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours(8));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours(8));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(startDateTime), startDateTime, startDateTime.AddHours(8));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);
			var seatBookingRequest2 = new SeatBookingRequest (agentShift2, agentShift3);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);
			location2.AddSeat ("L2 Seat2", 2);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1, seatBookingRequest2);

			Assert.That (agentShift1.Seat.Name == "L1 Seat1");

		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherAcrossLocationsEvenIfFirstLocationHasMostSeats()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift3 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift4 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest (agentShift3, agentShift4);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);
			location1.AddSeat ("L1 Seat2", 2);
			location1.AddSeat ("L1 Seat3", 3);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);
			location2.AddSeat ("L2 Seat2", 2);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1, seatBookingRequest2);

			Assert.That (agentShift3.Seat.Name == "L2 Seat1");
			Assert.That (agentShift4.Seat.Name == "L2 Seat2");
		}

		[Test]
		public void ShouldAllocateOnePersonInEachSeatEvenWhenGrouped()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1);

			Assert.That (seatBookingRequest1.SeatBookings.All (s => s.Seat != null));
		}

		[Test]
		public void ShouldAllocatePeopleEvenWhenTheyCannotSeatInGroups()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift3 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2, agentShift3);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1);

			Assert.That (seatBookingRequest1.SeatBookings.SingleOrDefault (s => s.Seat == null) != null);
		}


		[Test]
		public void ShouldAllocatePeopleUsingPriorityEvenWhenTheyCannotSeatInGroups()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);

						
			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);
			location1.AddSeat("L1 Seat2", 2);
			location1.AddSeat("L1 Seat3", 3);
			location1.AddSeat("L1 Seat4", 4);
			location1.AddSeat("L1 Seat5", 5);

			var existingBooking = new SeatBooking(new Person(), new DateOnly(2014, 01, 01),
				new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));
			existingBooking.Book(location1.Seats[2]);


			new SeatAllocator(location1).AllocateSeats(seatBookingRequest1);

			Assert.That(seatBookingRequest1.SeatBookings.ElementAt(0).Seat.Name == location1.Seats[0].Name );
			Assert.That(seatBookingRequest1.SeatBookings.ElementAt(1).Seat.Name == location1.Seats[1].Name);
			Assert.That(seatBookingRequest1.SeatBookings.ElementAt(2).Seat.Name == location1.Seats[3].Name);
		}


		[Test, Ignore ("We're not handling this case yet. Don't know if it is relevant")]
		public void ShouldTryToAllocateSplittedGroupsTogether()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0),
				new DateTime (2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0),
				new DateTime (2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0),
				new DateTime (2014, 01, 01, 17, 0, 0));
			var agentShift4 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0),
				new DateTime (2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2, agentShift3, agentShift4);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);
			location1.AddSeat ("L1 Seat2", 2);
			location1.AddSeat ("L1 Seat3", 3);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);
			location2.AddSeat ("L2 Seat2", 2);

			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.SeatBookings.Select (s => s.Seat.Name);
			Assert.That (allocatedSeats.Contains ("L2 Seat1"));
			Assert.That (allocatedSeats.Contains ("L2 Seat2"));
		}

		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocation()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat1", 1);

			building.AddChild (room1);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.That (agentShift1.Seat.Name == "Room1 Seat1");
		}

		[Test]
		public void ShouldAllocateChildOfChildFromParentLocation()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};
			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			var room2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			var roomChild1 = new SeatMapLocation() {IncludeInSeatPlan = true};

			roomChild1.AddSeat ("Room1Child Seat1", 1);
			building.AddChild (room1);
			building.AddChild (room2);
			room1.AddChild (roomChild1);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.That (agentShift1.Seat.Name == "Room1Child Seat1");
		}

		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocationEvenWhenParentIsNotIncludedInSeatPlan()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);

			var building = new SeatMapLocation() {IncludeInSeatPlan = false};
			var room1 = new SeatMapLocation() {IncludeInSeatPlan = false};
			var room2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat1", 1);
			room2.AddSeat ("Room2 Seat1", 1);

			building.AddChild (room1);
			building.AddChild (room2);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.That (agentShift1.Seat.Name == "Room2 Seat1");
		}

		[Test]
		public void ShouldAllocateDirectlyOnParentLocation()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var seatBookingRequest1 = new SeatBookingRequest (agentShift1);
			var building = new SeatMapLocation() {IncludeInSeatPlan = true};
			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};

			building.AddChild (room1);
			building.AddSeat ("Building Seat1", 1);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.That (agentShift1.Seat.Name == "Building Seat1");
		}

		[Test]
		public void ShouldAllocateGroupToParentLocationWithEnoughSeats()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room 1 Seat 1", 1);

			building.AddChild (room1);
			building.AddSeat ("Building Seat1", 1);
			building.AddSeat ("Building Seat2", 2);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.SeatBookings.Select (s => s.Seat.Name);
			Assert.That (allocatedSeats.Contains ("Building Seat1"));
			Assert.That (allocatedSeats.Contains ("Building Seat2"));
		}

		[Test]
		public void ShouldAllocateGroupToParentAndChildLocationWithEnoughSeats()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift3 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift4 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift5 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest (agentShift3, agentShift4);
			var seatBookingRequest3 = new SeatBookingRequest (agentShift5);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat1", 1);
			room1.AddSeat ("Room1 Seat2", 2);

			building.AddChild (room1);
			building.AddSeat ("Building Seat1", 1);
			building.AddSeat ("Building Seat2", 2);
			building.AddSeat ("Building Seat3", 3);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1, seatBookingRequest2, seatBookingRequest3);

			var allocatedSeatsGroup1 = seatBookingRequest1.SeatBookings.Select (s => s.Seat.Name);

			var allocatedSeatsGroup2 = seatBookingRequest2.SeatBookings.Select (s => s.Seat.Name);

			Assert.That (allocatedSeatsGroup1.Contains ("Building Seat1"));
			Assert.That (allocatedSeatsGroup1.Contains ("Building Seat2"));

			Assert.That (allocatedSeatsGroup2.Contains ("Room1 Seat1"));
			Assert.That (allocatedSeatsGroup2.Contains ("Room1 Seat2"));

			Assert.That (agentShift5.Seat.Name == "Building Seat3");
		}

		[Test]
		public void ShouldAssignSplitGroupedAgentsToRoomAndToBuilding()
		{
			var agent1Shift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));
			var agent2Shift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agent1Shift1, agent2Shift1);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};
			building.AddSeat ("Building Seat 1", 1);

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room 1 Seat 1", 1);
			building.AddChild (room1);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.That (agent1Shift1.Seat.Name == "Room 1 Seat 1");
			Assert.That (agent2Shift1.Seat.Name == "Building Seat 1");
		}

		[Test]
		public void ShouldNotAssignAgentWhenPreviouslyBooked()
		{
			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room 1 Seat 1", 1);
			var existingBooking = new SeatBooking (new Person(), new DateOnly (2014, 01, 01),
				new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime (2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));
			existingBooking.Book (room1.Seats.Single());

			var agent2Shift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 12, 59, 59, DateTimeKind.Utc));
			var seatBookingRequest1 = new SeatBookingRequest (agent2Shift1);
			new SeatAllocator (room1).AllocateSeats (seatBookingRequest1);

			Assert.That (agent2Shift1.Seat == null);
			Assert.That (Equals (existingBooking.Seat, room1.Seats.Single()));
		}

		[Test]
		public void ShouldAllocateTeamGroupedBookingsOverMultiDaysWhileHonouringExistingBookings()
		{
			var agentShift1 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 00, 00, DateTimeKind.Utc));
			var agentShift2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 01, 8, 0, 0, DateTimeKind.Utc),
				new DateTime (2014, 01, 01, 17, 0, 0, DateTimeKind.Utc));
			var agentShift1_Day2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 02, 8, 0, 0, DateTimeKind.Utc), 
				new DateTime (2014, 01, 02, 17, 00, 00, DateTimeKind.Utc));
			var agentShift2_Day2 = new SeatBooking (new Person(), new DateOnly (2014, 01, 01), new DateTime (2014, 01, 02, 8, 0, 0, DateTimeKind.Utc), 
				new DateTime (2014, 01, 02, 17, 0, 0, DateTimeKind.Utc));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2, agentShift1_Day2, agentShift2_Day2);

			var location1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location1.AddSeat ("L1 Seat1", 1);

			var location2 = new SeatMapLocation() {IncludeInSeatPlan = true};
			location2.AddSeat ("L2 Seat1", 1);
			location2.AddSeat ("L2 Seat2", 2);

			var existingBooking = new SeatBooking (new Person(), new DateOnly (2014, 01, 01),
				new DateTime (2014, 01, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime (2014, 01, 02, 17, 00, 00, DateTimeKind.Utc));
			existingBooking.Book (location2.Seats.First());


			new SeatAllocator (location1, location2).AllocateSeats (seatBookingRequest1);

			Assert.That (agentShift1.Seat.Name == "L2 Seat1");
			Assert.That (agentShift2.Seat.Name == "L2 Seat2");
			Assert.That (agentShift1_Day2.Seat.Name == "L2 Seat2");
			Assert.That (agentShift2_Day2.Seat.Name == "L1 Seat1");
			Assert.That (existingBooking.Seat.Name == "L2 Seat1");

		}

		[Test]
		public void ShouldConsiderRolesWhenDecidingIfCanAllocateTeamToLocation()
		{

			var dayOfBooking = new DateOnly (2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dayOfBooking.Date, DateTimeKind.Utc);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole ("Outbound", "xxx");
			var inboundApplicationRole = ApplicationRoleFactory.CreateRole ("Inbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();
			var agent3 = PersonFactory.CreatePerson();
			var agent4 = PersonFactory.CreatePerson();

			new[] {agent1, agent2, agent3}.ForEach (
				(agent) => agent.PermissionInformation.AddApplicationRole (outboundApplicationRole));

			agent4.PermissionInformation.AddApplicationRole (inboundApplicationRole);

			var agentShift1 = new SeatBooking (agent1, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));
			var agentShift2 = new SeatBooking (agent2, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));
			var agentShift3 = new SeatBooking (agent3, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));
			var agentShift4 = new SeatBooking (agent4, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2, agentShift3);
			var seatBookingRequest2 = new SeatBookingRequest (agentShift4);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat0", 1);
			room1.AddSeat ("Room1 Seat1", 2);
			room1.AddSeat ("Room1 Seat2", 3);
			room1.AddSeat ("Room1 Seat3", 4);
			room1.AddSeat ("Room1 Seat4", 5);

			var room2 = new SeatMapLocation() {IncludeInSeatPlan = true};

			room2.AddSeat ("Room2 Seat1", 2);
			room2.AddSeat ("Room2 Seat2", 3);
			room2.AddSeat ("Room2 Seat3", 4);

			room1.Seats.ForEach ((seat) => seat.SetRoles (new[] {inboundApplicationRole}));
			room2.Seats.ForEach ((seat) => seat.SetRoles (new[] {outboundApplicationRole}));

			room2.AddSeat ("Room2 Seat0", 1); // make this seat have no role...

			building.AddChild (room1);
			building.AddChild (room2);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1, seatBookingRequest2);

			var allocatedSeatsGroup1 = seatBookingRequest1.SeatBookings.Select (s => s.Seat.Name);
			Assert.That (allocatedSeatsGroup1.Contains ("Room2 Seat1"));
			Assert.That (allocatedSeatsGroup1.Contains ("Room2 Seat2"));
			Assert.That (allocatedSeatsGroup1.Contains ("Room2 Seat3"));

			var allocatedSeatsGroup2 = seatBookingRequest2.SeatBookings.Select (s => s.Seat.Name);
			Assert.That (allocatedSeatsGroup2.Contains ("Room1 Seat0"));
			}
		
		[Test]
		public void ShouldConsiderOverlappingBookingsWhenDecidingHowManySeatsShouldBeAllocatedToTeam()
		{

			var dayOfBooking = new DateOnly (2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dayOfBooking.Date, DateTimeKind.Utc);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole("Outbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();
			var agent3 = PersonFactory.CreatePerson();

			// agent shift 1 and 2(or 3) can overlap
			var agentShift2 = new SeatBooking(agent2, dayOfBooking, bookingDateTime.AddHours(18), bookingDateTime.AddHours(23));
			var agentShift3 = new SeatBooking(agent3, dayOfBooking, bookingDateTime.AddHours(9),  bookingDateTime.AddHours(17));
			var agentShift1 = new SeatBooking(agent1, dayOfBooking, bookingDateTime.AddHours(9), bookingDateTime.AddHours(17));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);
			room1.AddSeat("Room1 Seat2", 2);
			room1.AddSeat("Room1 Seat3", 3);

			//Agent3 must sit in seat 3
			agent3.PermissionInformation.AddApplicationRole(outboundApplicationRole);
			room1.Seats[2].Roles.Add (outboundApplicationRole);
			
			building.AddChild(room1);
			
			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.AreEqual(room1.Seats[2].Name, agentShift3.Seat.Name);

			// agents 1 and 2 should sit here to be close to agent 3
			Assert.AreEqual(room1.Seats[1].Name, agentShift1.Seat.Name);
			Assert.AreEqual(room1.Seats[1].Name, agentShift2.Seat.Name);

			Assert.AreEqual(0, seatBookingRequest1.SeatBookings.Count (seatBooking => seatBooking.Seat == room1.Seats[0]));
		}

		[Test]
		public void ShouldConsiderRolesAndThenNumberOfSeatsWhenChoosingLocationForTeam()
		{

			var dayOfBooking = new DateOnly (2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dayOfBooking.Date, DateTimeKind.Utc);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole ("Outbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();

			new[] {agent1, agent2}.ForEach ((agent) => agent.PermissionInformation.AddApplicationRole (outboundApplicationRole));


			var agentShift1 = new SeatBooking (agent1, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));
			var agentShift2 = new SeatBooking (agent2, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);

			var building = new SeatMapLocation() {IncludeInSeatPlan = true};

			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat1", 1);
			room1.AddSeat ("Room1 Seat2", 2);
			room1.AddSeat ("Room1 Seat3", 3);

			var room2 = new SeatMapLocation() {IncludeInSeatPlan = true};

			room2.AddSeat ("Room2 Seat1", 1);
			room2.AddSeat ("Room2 Seat2", 2);

			room2.Seats.ForEach ((seat) => seat.SetRoles (new[] {outboundApplicationRole}));

			building.AddChild (room1);
			building.AddChild (room2);

			new SeatAllocator (building).AllocateSeats (seatBookingRequest1);

			Assert.AreEqual (room2.Seats[0].Name, seatBookingRequest1.SeatBookings.First().Seat.Name);
			Assert.AreEqual (room2.Seats[1].Name, seatBookingRequest1.SeatBookings.Last().Seat.Name);
		}

		[Test]
		public void ShouldConsiderRolesWhenChoosingLocationForTeam()
		{
			var dayOfBooking = new DateOnly (2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dayOfBooking.Date, DateTimeKind.Utc);
			var outboundApplicationRole = ApplicationRoleFactory.CreateRole ("Outbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();

			new[] {agent1, agent2}.ForEach ((agent) => agent.PermissionInformation.AddApplicationRole (outboundApplicationRole));


			var agentShift1 = new SeatBooking (agent1, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));
			var agentShift2 = new SeatBooking (agent2, dayOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17));

			var seatBookingRequest1 = new SeatBookingRequest (agentShift1, agentShift2);

			var rootLocation = new SeatMapLocation() {IncludeInSeatPlan = true};
			rootLocation.AddSeat ("Room2 Seat1", 1);
			rootLocation.AddSeat ("Room2 Seat2", 2);


			var room1 = new SeatMapLocation() {IncludeInSeatPlan = true};
			room1.AddSeat ("Room1 Seat1", 1);
			room1.AddSeat ("Room1 Seat2", 2);
			room1.AddSeat ("Room1 Seat3", 3);


			rootLocation.Seats.ForEach ((seat) => seat.SetRoles (new[] {outboundApplicationRole}));

			rootLocation.AddChild (room1);


			new SeatAllocator (rootLocation).AllocateSeats (seatBookingRequest1);

			Assert.AreEqual (rootLocation.Seats[0].Name, seatBookingRequest1.SeatBookings.First().Seat.Name);
			Assert.AreEqual (rootLocation.Seats[1].Name, seatBookingRequest1.SeatBookings.Last().Seat.Name);
		}



		[Test]
		public void ShouldConsiderFrequencyWhenChoosingLocationForTeam()
		{
			var dayOfBooking = new DateOnly (2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dayOfBooking.Date, DateTimeKind.Utc);

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();

			var agentShift1 = new SeatBooking(agent1, dayOfBooking, bookingDateTime.AddHours(8), bookingDateTime.AddHours(17));
			var agentShift2 = new SeatBooking(agent2, dayOfBooking, bookingDateTime.AddHours(8), bookingDateTime.AddHours(17));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var rootLocation = new SeatMapLocation() { IncludeInSeatPlan = true };
			rootLocation.AddSeat("Root Seat1", 1);
			rootLocation.AddSeat("Root Seat2", 2);


			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Child Seat1", 1);
			room1.AddSeat("Child Seat2", 2);
			
			rootLocation.AddChild(room1);

			var seatFrequencies = new Dictionary<Guid, List<ISeatOccupancyFrequency>>();
			seatFrequencies.Add(agent1.Id.GetValueOrDefault(), new List<ISeatOccupancyFrequency>()
			{
				new SeatOccupancyFrequency()
				{
					Seat = rootLocation.Seats[1], Frequency = 1

				}
			});

			new SeatAllocator(seatFrequencies, rootLocation).AllocateSeats(seatBookingRequest1);
			
			Assert.AreEqual(rootLocation.Seats[0].Name, seatBookingRequest1.SeatBookings.Last().Seat.Name);
			Assert.AreEqual(rootLocation.Seats[1].Name, seatBookingRequest1.SeatBookings.First().Seat.Name);
		}

		[Test]
		public void ShouldGroupTeamsAroundSeatBookingWithRoleCount()
		{
			var dateOfBooking = new DateOnly(2014, 01, 01);
			var bookingDateTime = DateTime.SpecifyKind(dateOfBooking.Date, DateTimeKind.Utc);
			
			
			var outboundApplicationRole = ApplicationRoleFactory.CreateRole("outbound", "xxx");
			var teamLeaderRole = ApplicationRoleFactory.CreateRole("teamLeader", "xxx");
			var team1 = TeamFactory.CreateSimpleTeam("team1");
			var team2 = TeamFactory.CreateSimpleTeam("team2");

			var agentsInTeam1 = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team1),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team1),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team1)
			};
			var seatBookingRequest1 = new SeatBookingRequest(
				new SeatBooking(agentsInTeam1[0], dateOfBooking, bookingDateTime.AddHours(8), bookingDateTime.AddHours(17)),
				new SeatBooking(agentsInTeam1[1], dateOfBooking, bookingDateTime.AddHours(8), bookingDateTime.AddHours(17)),
				new SeatBooking(agentsInTeam1[2], dateOfBooking, bookingDateTime.AddHours(8), bookingDateTime.AddHours(17))
			);
			agentsInTeam1[2].PermissionInformation.AddApplicationRole (outboundApplicationRole);

			

			var agentsInTeam2 = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team2),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team2),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam (dateOfBooking, team2)
			};
			var seatBookingRequest2 = new SeatBookingRequest (
				new SeatBooking (agentsInTeam2[0], dateOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17)),
				new SeatBooking (agentsInTeam2[1], dateOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17)),
				new SeatBooking (agentsInTeam2[2], dateOfBooking, bookingDateTime.AddHours (8), bookingDateTime.AddHours (17))
			);
			agentsInTeam2[2].PermissionInformation.AddApplicationRole(outboundApplicationRole);
			agentsInTeam2[1].PermissionInformation.AddApplicationRole(outboundApplicationRole);
			agentsInTeam2[1].PermissionInformation.AddApplicationRole(teamLeaderRole);


			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);
			location.AddSeat("Seat2", 3);
			location.AddSeat("Seat3", 5);
			location.AddSeat("Seat4", 6);
			location.AddSeat("Seat5", 7);
			location.AddSeat("Seat6", 8);
			location.AddSeat("Seat7", 9);
			location.AddSeat("Seat8", 10);
			location.AddSeat("Seat9", 11);

			location.Seats[3].SetRoles (new []{outboundApplicationRole});
			location.Seats[3].SetRoles(new[] { teamLeaderRole });
			location.Seats[4].SetRoles (new []{outboundApplicationRole});
			location.Seats[5].SetRoles (new []{outboundApplicationRole});
			

			new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			seatBookingRequest1.SeatBookings.ElementAt(0).Seat.Name.Should().Be (location.Seats[6].Name);
			seatBookingRequest1.SeatBookings.ElementAt(1).Seat.Name.Should().Be(location.Seats[7].Name);
			seatBookingRequest1.SeatBookings.ElementAt(2).Seat.Name.Should().Be(location.Seats[5].Name);

			seatBookingRequest2.SeatBookings.ElementAt(0).Seat.Name.Should().Be(location.Seats[2].Name);
			seatBookingRequest2.SeatBookings.ElementAt(1).Seat.Name.Should().Be(location.Seats[3].Name);
			seatBookingRequest2.SeatBookings.ElementAt (2).Seat.Name.Should().Be (location.Seats[4].Name);
		}

	
		#region Performance Benchmarks

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldHavePerformance()
		{
			var seatBookingRequests =
				new List<SeatBookingRequest>(
					Enumerable.Range(0, 10).Select(r => new SeatBookingRequest(Enumerable.Range(0, 8)
						.Select(s =>
							new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 01, 01, 17, 00, 00, DateTimeKind.Utc)))
						.ToArray())));

			var allocator = new SeatAllocator(Enumerable.Range(0, 2).Select(i =>
			{
				var location = new SeatMapLocation() { IncludeInSeatPlan = true };
				Enumerable.Range(0, 350).All(s =>
				{
					location.AddSeat("temp", s);
					return true;
				});
				return location;
			}).ToArray());

			var stopwatch = Stopwatch.StartNew();
			allocator.AllocateSeats(seatBookingRequests.ToArray());
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldHavePerformanceWithHierachyOfLocations()
		{
			var person = PersonFactory.CreatePerson();
			var seatBookingRequests =
				new List<SeatBookingRequest>(
					Enumerable.Range(0, 1000).Select(r => new SeatBookingRequest(Enumerable.Range(0, 30)
						.Select(s =>
							new SeatBooking(person, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)))
						.ToArray())));

			var allocator = new SeatAllocator(Enumerable.Range(0, 50).Select(i =>
			{
				var location = new SeatMapLocation() { IncludeInSeatPlan = true };
				Enumerable.Range(0, 10).All(s =>
				{
					Enumerable.Range(0, 20).All(r =>
					{
						var childLocation = new SeatMapLocation() { IncludeInSeatPlan = true };
						location.AddChild(childLocation);

						Enumerable.Range(0, 30).All(c =>
						{
							childLocation.AddSeat("temp", c);
							return true;
						});

						return true;
					});

					return true;
				});
				return location;
			}).ToArray());

			var stopwatch = Stopwatch.StartNew();
			allocator.AllocateSeats(seatBookingRequests.ToArray());
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);
		}


		#endregion

		#region Seat Booking Period Tests

		[Test]
		public void ShouldDetectPeriodsOverlap()
		{
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 01, 1, 15, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 7, 59, 59, DateTimeKind.Utc), new DateTime(2014, 01, 1, 8, 0, 0, DateTimeKind.Utc));

			Assert.True(booking1.Intersects(booking2));
		}


		[Test]
		public void ShouldDetectOvernightPeriodsOverlap()
		{
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 23, 0, 0, DateTimeKind.Utc), new DateTime(2014, 01, 2, 8, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 7, 59, 59, DateTimeKind.Utc), new DateTime(2014, 01, 2, 15, 0, 0, DateTimeKind.Utc));

			Assert.True(booking1.Intersects(booking2));
		}

		[Test]
		public void ShouldDetectPeriodsDoNotOverlap()
		{
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 01, 1, 15, 0, 0, DateTimeKind.Utc));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 7, 59, 58, DateTimeKind.Utc), new DateTime(2014, 01, 1, 7, 59, 59, DateTimeKind.Utc));

			Assert.True(!booking1.Intersects(booking2));
		}

		#endregion
	}
}


