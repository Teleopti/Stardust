using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	public class SeatAllocatorTests
	{
		[Test]
		public void ShouldAllocateAnAgentToASeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeat(false);
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeatEach()
		{
			CommonSeatAllocatorTests.ShouldAllocateTwoAgentsToOneSeatEach(false);
		}

		[Test]
		public void ShouldAllocateAccordingToPriority()
		{
			CommonSeatAllocatorTests.ShouldAllocateAccordingToPriority(false);
		}

		[Test]
		public void ShouldAllocateTwoAgentsSequentiallyToOneSeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateTwoAgentsSequentiallyToOneSeat(false);
		}

		[Test]
		public void ShouldAllocateSeatsByEarliestFirst()
		{
			CommonSeatAllocatorTests.ShouldAllocateSeatsByEarliestFirst(false);
		}

		[Test]
		public void ShouldNotAllocateTwoAgentsSequentiallyToOneSeat()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateTwoAgentsSequentiallyToOneSeat(false);
		}

		[Test]
		public void ShouldAllocateAccordingToStartTime()
		{
			CommonSeatAllocatorTests.ShouldAllocateAccordingToStartTime(false);
		}


		[Test]
		public void ShouldAllocateToGroupFirst()
		{
			CommonSeatAllocatorTests.ShouldAllocateToGroupFirst(false);

		}

		[Test]
		public void ShouldNotAllocateAnAgentToAnAlreadyBookedSeat()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateAnAgentToAnAlreadyBookedSeat(false);

		}

		[Test]
		public void ShouldAllocateToAvailableSeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateToAvailableSeat(false);

		}

		[Test]
		public void ShouldAllocateTwoIntersectingBookingRequestsOverTwoLocations()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };
			Assert.That(allocatedSeats.Contains("L1 Seat1") && allocatedSeats.Contains("L2 Seat1"));
		}

		[Test]
		public void ShouldAllocateAgentGroupsTogether()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);
			location1.AddSeat("L1 Seat2", 2);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift1.Seat.Name == "L2 Seat1");

		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherForSecondLocation()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);
			location2.AddSeat("L2 Seat2", 2);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift1.Seat.Name == "L1 Seat1");

		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherAcrossLocationsEvenIfFirstLocationHasMostSeats()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift4 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift3, agentShift4);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);
			location1.AddSeat("L1 Seat2", 2);
			location1.AddSeat("L1 Seat3", 3);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);
			location2.AddSeat("L2 Seat2", 2);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift3.Seat.Name == "L2 Seat1");
			Assert.That(agentShift4.Seat.Name == "L2 Seat2");
		}

		[Test]
		public void ShouldAllocateOnePersonInEachSeatEvenWhenGrouped()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1);

			Assert.That(seatBookingRequest1.SeatBookings.All(s => s.Seat != null));
		}

		[Test]
		public void ShouldAllocatePeopleEvenWhenTheyCannotSeatInGroups()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1);

			Assert.That(seatBookingRequest1.SeatBookings.SingleOrDefault(s => s.Seat == null) != null);
		}

		[Test, Ignore("We're not handling this case yet. Don't know if it is relevant")]
		public void ShouldTryToAllocateSplittedGroupsTogether()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift4 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3, agentShift4);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);
			location1.AddSeat("L1 Seat2", 2);
			location1.AddSeat("L1 Seat2", 3);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);
			location2.AddSeat("L2 Seat2", 2);

			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.SeatBookings.Select(s => s.Seat.Name);
			Assert.That(allocatedSeats.Contains("L2 Seat1"));
			Assert.That(allocatedSeats.Contains("L2 Seat2"));
		}

		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocation()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);

			building.AddChild(room1);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room1 Seat1");
		}

		[Test]
		public void ShouldAllocateChildOfChildFromParentLocation()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };
			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			var room2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			var roomChild1 = new SeatMapLocation() { IncludeInSeatPlan = true };

			roomChild1.AddSeat("Room1Child Seat1", 1);
			building.AddChild(room1);
			building.AddChild(room2);
			room1.AddChild(roomChild1);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room1Child Seat1");
		}

		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocationEvenWhenParentIsNotIncludedInSeatPlan()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new SeatMapLocation() { IncludeInSeatPlan = false };
			var room1 = new SeatMapLocation() { IncludeInSeatPlan = false };
			var room2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);
			room2.AddSeat("Room2 Seat1", 1);

			building.AddChild(room1);
			building.AddChild(room2);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room2 Seat1");
		}

		[Test]
		public void ShouldAllocateDirectlyOnParentLocation()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);
			var building = new SeatMapLocation() { IncludeInSeatPlan = true };
			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };

			building.AddChild(room1);
			building.AddSeat("Building Seat1", 1);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Building Seat1");
		}

		[Test]
		public void ShouldAllocateGroupToParentLocationWithEnoughSeats()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room 1 Seat 1", 1);

			building.AddChild(room1);
			building.AddSeat("Building Seat1", 1);
			building.AddSeat("Building Seat2", 2);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.SeatBookings.Select(s => s.Seat.Name);
			Assert.That(allocatedSeats.Contains("Building Seat1"));
			Assert.That(allocatedSeats.Contains("Building Seat2"));
		}

		[Test]
		public void ShouldAllocateGroupToParentAndChildLocationWithEnoughSeats()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift4 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift5 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift3, agentShift4);
			var seatBookingRequest3 = new SeatBookingRequest(agentShift5);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);
			room1.AddSeat("Room1 Seat2", 2);
			
			building.AddChild(room1);
			building.AddSeat("Building Seat1", 1);
			building.AddSeat("Building Seat2", 2);
			building.AddSeat("Building Seat3", 3);
			
			new SeatAllocator(building).AllocateSeats(seatBookingRequest1, seatBookingRequest2, seatBookingRequest3);

			var allocatedSeatsGroup1 = seatBookingRequest1.SeatBookings.Select(s => s.Seat.Name);

			Assert.That(allocatedSeatsGroup1.Contains("Building Seat1"));
			Assert.That(allocatedSeatsGroup1.Contains("Building Seat2"));

			var allocatedSeatsGroup2 = seatBookingRequest2.SeatBookings.Select(s => s.Seat.Name);
			Assert.That(allocatedSeatsGroup2.Contains("Room1 Seat1"));
			Assert.That(allocatedSeatsGroup2.Contains("Room1 Seat2"));

			Assert.That(agentShift5.Seat.Name == "Building Seat3");
		}

		[Test]
		public void ShouldAssignSplitGroupedAgentsToRoomAndToBuilding()
		{
			var agent1Shift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			var agent2Shift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));

			var seatBookingRequest1 = new SeatBookingRequest(agent1Shift1, agent2Shift1);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };
			building.AddSeat("Building Seat 1", 1);

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room 1 Seat 1", 1);
			building.AddChild(room1);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.That(agent1Shift1.Seat.Name == "Room 1 Seat 1");
			Assert.That(agent2Shift1.Seat.Name == "Building Seat 1");
		}

		[Test]
		public void ShouldNotAssignAgentWhenPreviouslyBooked()
		{
			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room 1 Seat 1", 1);
			var existingBooking = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			existingBooking.Book(room1.Seats.Single());

			var agent2Shift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			var seatBookingRequest1 = new SeatBookingRequest(agent2Shift1);
			new SeatAllocator(room1).AllocateSeats(seatBookingRequest1);

			Assert.That(agent2Shift1.Seat == null);
			Assert.That(Equals(existingBooking.Seat, room1.Seats.Single()));
		}

		[Test]
		public void ShouldAllocateTeamGroupedBookingsOverMultiDaysWhileHonouringExistingBookings()
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1_Day2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 8, 0, 0), new DateTime(2014, 01, 02, 17, 00, 00));
			var agentShift2_Day2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 8, 0, 0), new DateTime(2014, 01, 02, 17, 0, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift1_Day2, agentShift2_Day2);

			var location1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location1.AddSeat("L1 Seat1", 1);

			var location2 = new SeatMapLocation() { IncludeInSeatPlan = true };
			location2.AddSeat("L2 Seat1", 1);
			location2.AddSeat("L2 Seat2", 2);

			var existingBooking = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 8, 0, 0), new DateTime(2014, 01, 02, 17, 00, 00));
			existingBooking.Book(location2.Seats.First());


			new SeatAllocator(location1, location2).AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "L2 Seat1");
			Assert.That(agentShift2.Seat.Name == "L2 Seat2");
			Assert.That(agentShift1_Day2.Seat.Name == "L2 Seat2");
			Assert.That(agentShift2_Day2.Seat.Name == "L1 Seat1");
			Assert.That(existingBooking.Seat.Name == "L2 Seat1");

		}

		[Test]
		public void ShouldConsiderRolesWhenDecidingIfCanAllocateTeamToLocation()
		{

			var dateOnly = new DateOnly(2014, 01, 01);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole("Outbound", "xxx");
			var inboundApplicationRole = ApplicationRoleFactory.CreateRole("Inbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();
			var agent3 = PersonFactory.CreatePerson();
			var agent4 = PersonFactory.CreatePerson();

			new[] { agent1, agent2, agent3 }.ForEach((agent) => agent.PermissionInformation.AddApplicationRole(outboundApplicationRole));

			agent4.PermissionInformation.AddApplicationRole(inboundApplicationRole);

			var agentShift1 = new SeatBooking(agent1, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));
			var agentShift2 = new SeatBooking(agent2, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));
			var agentShift3 = new SeatBooking(agent3, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));
			var agentShift4 = new SeatBooking(agent4, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift4);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat0", 1);
			room1.AddSeat("Room1 Seat1", 2);
			room1.AddSeat("Room1 Seat2", 3);
			room1.AddSeat("Room1 Seat3", 4);
			room1.AddSeat("Room1 Seat4", 5);

			var room2 = new SeatMapLocation() { IncludeInSeatPlan = true };

			room2.AddSeat("Room2 Seat1", 1);
			room2.AddSeat("Room2 Seat2", 2);
			room2.AddSeat("Room2 Seat3", 3);

			room1.Seats.ForEach((seat) => seat.SetRoles(new[] { inboundApplicationRole }));
			room2.Seats.ForEach((seat) => seat.SetRoles(new[] { outboundApplicationRole }));

			room2.AddSeat("Room2 Seat0", 1); // make this seat have no role...

			building.AddChild(room1);
			building.AddChild(room2);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			var allocatedSeatsGroup1 = seatBookingRequest1.SeatBookings.Select(s => s.Seat.Name);
			Assert.That(allocatedSeatsGroup1.Contains("Room2 Seat1"));
			Assert.That(allocatedSeatsGroup1.Contains("Room2 Seat2"));
			Assert.That(allocatedSeatsGroup1.Contains("Room2 Seat3"));

			var allocatedSeatsGroup2 = seatBookingRequest2.SeatBookings.Select(s => s.Seat.Name);
			Assert.That(allocatedSeatsGroup2.Contains("Room1 Seat0"));

		}


		[Test]
		public void ShouldConsiderRolesAndThenNumberOfSeatsWhenChoosingLocationForTeam()
		{

			var dateOnly = new DateOnly(2014, 01, 01);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole("Outbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();

			new[] { agent1, agent2 }.ForEach((agent) => agent.PermissionInformation.AddApplicationRole(outboundApplicationRole));


			var agentShift1 = new SeatBooking(agent1, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));
			var agentShift2 = new SeatBooking(agent2, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var building = new SeatMapLocation() { IncludeInSeatPlan = true };

			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);
			room1.AddSeat("Room1 Seat2", 2);
			room1.AddSeat("Room1 Seat3", 3);

			var room2 = new SeatMapLocation() { IncludeInSeatPlan = true };

			room2.AddSeat("Room2 Seat1", 1);
			room2.AddSeat("Room2 Seat2", 2);

			room2.Seats.ForEach((seat) => seat.SetRoles(new[] { outboundApplicationRole }));

			building.AddChild(room1);
			building.AddChild(room2);

			new SeatAllocator(building).AllocateSeats(seatBookingRequest1);

			Assert.AreEqual(room2.Seats[0].Name, seatBookingRequest1.SeatBookings.First().Seat.Name);
			Assert.AreEqual(room2.Seats[1].Name, seatBookingRequest1.SeatBookings.Last().Seat.Name);
		}

		[Test]
		public void ShouldConsiderRolesAndThenNumberOfSeatsWhenChoosingLocationForTeamAndAllocateToParent()
		{

			var dateOnly = new DateOnly(2014, 01, 01);

			var outboundApplicationRole = ApplicationRoleFactory.CreateRole("Outbound", "xxx");

			var agent1 = PersonFactory.CreatePerson();
			var agent2 = PersonFactory.CreatePerson();

			new[] { agent1, agent2 }.ForEach((agent) => agent.PermissionInformation.AddApplicationRole(outboundApplicationRole));


			var agentShift1 = new SeatBooking(agent1, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));
			var agentShift2 = new SeatBooking(agent2, dateOnly, dateOnly.Date.AddHours(8), dateOnly.Date.AddHours(17));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var rootLocation = new SeatMapLocation() { IncludeInSeatPlan = true };
			rootLocation.AddSeat("Room2 Seat1", 1);
			rootLocation.AddSeat("Room2 Seat2", 2);


			var room1 = new SeatMapLocation() { IncludeInSeatPlan = true };
			room1.AddSeat("Room1 Seat1", 1);
			room1.AddSeat("Room1 Seat2", 2);
			room1.AddSeat("Room1 Seat3", 3);

			
			rootLocation.Seats.ForEach((seat) => seat.SetRoles(new[] { outboundApplicationRole }));

			rootLocation.AddChild(room1);
			

			new SeatAllocator(rootLocation).AllocateSeats(seatBookingRequest1);

			Assert.AreEqual(rootLocation.Seats[0].Name, seatBookingRequest1.SeatBookings.First().Seat.Name);
			Assert.AreEqual(rootLocation.Seats[1].Name, seatBookingRequest1.SeatBookings.Last().Seat.Name);
		}

		[Test]
		public void ShouldNotAllocateAnAgentToASeatWhereRolesDoNotMatch()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateAnAgentToASeatWhereRolesDoNotMatch(false);
		}

		[Test]
		public void ShouldAllocateAnAgentToASeatWhereRolesMatch()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereRolesMatch(false);

		}

		[Test]
		public void ShouldAllocateAnAgentToASeatWhereAllRolesMatch()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereAllRolesMatch(false);
		}

		[Test]
		public void ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority(false);
		}


		#region Performance Benchmarks

		[Test, Ignore]
		public void ShouldHavePerformance()
		{

			var seatBookingRequests =
				new List<SeatBookingRequest>(
					Enumerable.Range(0, 1000).Select(r => new SeatBookingRequest(Enumerable.Range(0, 30)
						.Select(s =>
							new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)))
						.ToArray())));

			var allocator = new SeatAllocator(Enumerable.Range(0, 50).Select(i =>
			{
				var location = new SeatMapLocation() { IncludeInSeatPlan = true };
				Enumerable.Range(0, 600).All(s =>
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

		[Test, Ignore]
		public void ShouldHavePerformanceWithHierachyOfLocations()
		{

			var seatBookingRequests =
				new List<SeatBookingRequest>(
					Enumerable.Range(0, 1000).Select(r => new SeatBookingRequest(Enumerable.Range(0, 30)
						.Select(s =>
							new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)))
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
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 1, 15, 0, 0));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 7, 59, 59), new DateTime(2014, 01, 1, 8, 0, 0));

			Assert.True(booking1.Intersects(booking2));
		}


		[Test]
		public void ShouldDetectOvernightPeriodsOverlap()
		{
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 23, 0, 0), new DateTime(2014, 01, 2, 8, 0, 0));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 7, 59, 59), new DateTime(2014, 01, 2, 15, 0, 0));

			Assert.True(booking1.Intersects(booking2));
		}

		[Test]
		public void ShouldDetectPeriodsDoNotOverlap()
		{
			var booking1 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 1, 15, 0, 0));
			var booking2 = new SeatBooking(null, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 7, 59, 58), new DateTime(2014, 01, 1, 7, 59, 59));

			Assert.True(!booking1.Intersects(booking2));
		}

		#endregion
	}
}


