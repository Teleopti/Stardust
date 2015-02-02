using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;

namespace SeatPlanner
{
	public class SeatPlanAllocationTests
	{

		[Test]
		public void ShouldAllocateAnAgentToASeat()
		{
			var agentShift = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest = new SeatBookingRequest(agentShift);

			var location = new Location(){IncludeInSeatPlan = true};
			location.AddSeat(Guid.NewGuid(), "Seat1");

			var seatAllocator = new SeatAllocator(location);
			seatAllocator.AllocateSeats(seatBookingRequest);

			Assert.That(agentShift.Seat.Name == "Seat1");
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeatEach()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new Location() { IncludeInSeatPlan = true };
			location.AddSeat(Guid.NewGuid(), "Seat1");
			location.AddSeat(Guid.NewGuid(), "Seat2");

			var seatAllocator = new SeatAllocator(location);
			seatAllocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };
			Assert.That(allocatedSeats.Contains("Seat1") && allocatedSeats.Contains("Seat2"));
		}

		[Test]
		public void ShouldAllocateTwoAgentsSequentiallyToOneSeat()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 11, 59, 59)), null, null);
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new Location() { IncludeInSeatPlan = true };
			location.AddSeat(Guid.NewGuid(), "Seat1");

			var seatAllocator = new SeatAllocator(location);
			seatAllocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };

			Assert.That(allocatedSeats[0].Equals("Seat1") && allocatedSeats[1].Equals("Seat1"));
		}

		[Test]
		public void ShouldNotAllocateTwoAgentsSequentiallyToOneSeat()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59)), null, null);
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new Location() { IncludeInSeatPlan = true };
			location.AddSeat(Guid.NewGuid(), "Seat1");

			var seatAllocator = new SeatAllocator(location);
			seatAllocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift2.Seat == null);
		}

		[Test]
		public void ShouldAllocateTwoIntersectingBookingRequestsOverTwoLocations()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59)), null, null);
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location1= new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(), "L1 Seat1");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(), "L2 Seat1");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };
			Assert.That(allocatedSeats.Contains("L1 Seat1") && allocatedSeats.Contains("L2 Seat1"));
		}


		[Test]
		public void ShouldAllocateAgentGroupsTogether()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)), null, null);
			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var agentShift3 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(), "L1 Seat1");
			location1.AddSeat(Guid.NewGuid(), "L1 Seat2");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(), "L2 Seat1");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift1.Seat.Name == "L2 Seat1");

		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherForSecondLocation()
		{
			var agentShift1 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)), null, null);
			var agentShift2 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);
			var agentShift3 = new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0)), null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(), "L1 Seat1");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(), "L2 Seat1");
			location2.AddSeat(Guid.NewGuid(), "L2 Seat2");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift1.Seat.Name == "L1 Seat1");

		}

		[Test]
		public void ShouldAllocateAgentGroupsTogetherAcrossLocationsEvenIfFirstLocationHasMostSeats()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);
			var agentShift2 = new AgentShift(bookingPeriod, null, null);
			var agentShift3 = new AgentShift(bookingPeriod, null, null);
			var agentShift4 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift3, agentShift4);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(), "L1 Seat1");
			location1.AddSeat(Guid.NewGuid(), "L1 Seat2");
			location1.AddSeat(Guid.NewGuid(), "L1 Seat3");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(), "L2 Seat1");
			location2.AddSeat(Guid.NewGuid(), "L2 Seat2");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2);

			Assert.That(agentShift3.Seat.Name == "L2 Seat1");
			Assert.That(agentShift4.Seat.Name == "L2 Seat2");
		}

		[Test]
		public void ShouldAllocateOnePersonInEachSeatEvenWhenGrouped()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);
			var agentShift2 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(),"L1 Seat1");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(),"L2 Seat1");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(seatBookingRequest1.AgentShifts.All(s => s.Seat != null));
		}

		[Test]
		public void ShouldAllocatePeopleEvenWhenTheyCannotSeatInGroups()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null,null);
			var agentShift2 = new AgentShift(bookingPeriod, null,null);
			var agentShift3 = new AgentShift(bookingPeriod, null,null);


			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(),"L1 Seat1");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(),"L2 Seat1");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(seatBookingRequest1.AgentShifts.SingleOrDefault(s => s.Seat == null) != null);
		}

		[Test, Ignore("We're not handling this case yet. Don't know if it is relevant")]
		public void ShouldTryToAllocateSplittedGroupsTogether()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);
			var agentShift2 = new AgentShift(bookingPeriod, null, null);
			var agentShift3 = new AgentShift(bookingPeriod, null, null);
			var agentShift4 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3, agentShift4);

			var location1 = new Location() { IncludeInSeatPlan = true };
			location1.AddSeat(Guid.NewGuid(),"L1 Seat1");
			location1.AddSeat(Guid.NewGuid(),"L1 Seat2");
			location1.AddSeat(Guid.NewGuid(),"L1 Seat3");

			var location2 = new Location() { IncludeInSeatPlan = true };
			location2.AddSeat(Guid.NewGuid(),"L2 Seat1");
			location2.AddSeat(Guid.NewGuid(),"L2 Seat2");

			var allocator = new SeatAllocator(location1, location2);
			allocator.AllocateSeats(seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.AgentShifts.Select(s => s.Seat.Name);
			Assert.That(allocatedSeats.Contains("L2 Seat1"));
			Assert.That(allocatedSeats.Contains("L2 Seat2"));
		}


		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocation()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new Location() { IncludeInSeatPlan = true };

			var room1 = new Location() { IncludeInSeatPlan = true };
			room1.AddSeat(Guid.NewGuid(),"Room1 Seat1");

			building.AddChild(room1);

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room1 Seat1");
		}

		[Test]
		public void ShouldAllocateChildOfChildFromParentLocation()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new Location() { IncludeInSeatPlan = true };
			var room1 = new Location() { IncludeInSeatPlan = true };
			var room2 = new Location() {IncludeInSeatPlan = true};
			var roomChild1 = new Location() { IncludeInSeatPlan = true };
		
			roomChild1.AddSeat(Guid.NewGuid(), "Room1Child Seat1");
			building.AddChild(room1);
			building.AddChild(room2);
			room1.AddChild (roomChild1);

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room1Child Seat1");
		}

		[Test]
		public void ShouldAllocateChildLocationSeatFromParentLocationEvenWhenParentIsNotIncludedInSeatPlan()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new Location() { IncludeInSeatPlan = false };
			var room1 = new Location() { IncludeInSeatPlan = false };
			var room2 = new Location() { IncludeInSeatPlan = true };
			room1.AddSeat(Guid.NewGuid(), "Room1 Seat1");
			room2.AddSeat(Guid.NewGuid(), "Room2 Seat1");

			building.AddChild(room1);
			building.AddChild(room2);

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Room2 Seat1");
		}


		[Test]
		public void ShouldAllocateDirectlyOnParentLocation()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var building = new Location() { IncludeInSeatPlan = true };

			var room1 = new Location() { IncludeInSeatPlan = true };

			building.AddChild(room1);
			building.AddSeat(Guid.NewGuid(),"Building Seat1");

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(agentShift1.Seat.Name == "Building Seat1");
		}


		[Test]
		public void ShouldAllocateGroupToParentLocationWithEnoughSeats()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);
			var agentShift2 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);

			var building = new Location() { IncludeInSeatPlan = true };

			var room1 = new Location() { IncludeInSeatPlan = true };
			room1.AddSeat(Guid.NewGuid(),"Room 1 Seat 1");

			building.AddChild(room1);
			building.AddSeat(Guid.NewGuid(),"Building Seat1");
			building.AddSeat(Guid.NewGuid(),"Building Seat2");

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			var allocatedSeats = seatBookingRequest1.AgentShifts.Select(s => s.Seat.Name);
			Assert.That(allocatedSeats.Contains("Building Seat1"));
			Assert.That(allocatedSeats.Contains("Building Seat2"));
		}

		[Test]
		public void ShouldAllocateGroupToParentAndChildLocationWithEnoughSeats()
		{
			var bookingPeriod = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var agentShift1 = new AgentShift(bookingPeriod, null, null);
			var agentShift2 = new AgentShift(bookingPeriod, null, null);
			var agentShift3 = new AgentShift(bookingPeriod, null, null);
			var agentShift4 = new AgentShift(bookingPeriod, null, null);
			var agentShift5 = new AgentShift(bookingPeriod, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift3, agentShift4);
			var seatBookingRequest3 = new SeatBookingRequest(agentShift5);

			var building = new Location() { IncludeInSeatPlan = true };

			var room1 = new Location() { IncludeInSeatPlan = true };
			room1.AddSeat(Guid.NewGuid(),"Room1 Seat1");
			room1.AddSeat(Guid.NewGuid(),"Room1 Seat2");

			building.AddChild(room1);
			building.AddSeat(Guid.NewGuid(),"Building Seat1");
			building.AddSeat(Guid.NewGuid(),"Building Seat2");
			building.AddSeat(Guid.NewGuid(),"Building Seat3");

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1, seatBookingRequest2, seatBookingRequest3);

			var allocatedSeatsGroup1 = seatBookingRequest1.AgentShifts.Select(s => s.Seat.Name);
			Assert.That(allocatedSeatsGroup1.Contains("Room1 Seat1"));
			Assert.That(allocatedSeatsGroup1.Contains("Room1 Seat2"));

			var allocatedSeatsGroup2 = seatBookingRequest2.AgentShifts.Select(s => s.Seat.Name);
			Assert.That(allocatedSeatsGroup2.Contains("Building Seat1"));
			Assert.That(allocatedSeatsGroup2.Contains("Building Seat2"));

			Assert.That(agentShift5.Seat.Name == "Building Seat3");
		}


		[Test]
		public void ShouldAssignSplitGroupedAgentsToRoomAndToBuilding()
		{
			var bookingPeriod1 = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			var agent1Shift1 = new AgentShift(bookingPeriod1, null, null);
			var agent2Shift1 = new AgentShift(bookingPeriod1, null, null);

			var seatBookingRequest1 = new SeatBookingRequest(agent1Shift1, agent2Shift1);

			var building = new Location() { IncludeInSeatPlan = true };
			building.AddSeat(Guid.NewGuid(),"Building Seat 1");

			var room1 = new Location() { IncludeInSeatPlan = true };
			room1.AddSeat(Guid.NewGuid(),"Room 1 Seat 1");
			building.AddChild(room1);

			var allocator = new SeatAllocator(building);
			allocator.AllocateSeats(seatBookingRequest1);

			Assert.That(agent1Shift1.Seat.Name == "Room 1 Seat 1");
			Assert.That(agent2Shift1.Seat.Name == "Building Seat 1");

		}

		#region Performance Benchmarks

		[Test, Ignore]
		public void ShouldHavePerformance()
		{

			var seatBookingRequests =
				new List<SeatBookingRequest>(
					Enumerable.Range(0, 1000).Select(r => new SeatBookingRequest(Enumerable.Range(0, 30)
						.Select(s =>
							new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)), null, null))
						.ToArray())));

			var allocator = new SeatAllocator(Enumerable.Range(0, 50).Select(i =>
			{
				var location = new Location() { IncludeInSeatPlan = true };
				Enumerable.Range(0, 600).All(s =>
				{
					location.AddSeat(Guid.NewGuid(),"temp");
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
							new AgentShift(new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 00, 00)), null, null))
						.ToArray())));

			var allocator = new SeatAllocator(Enumerable.Range(0, 50).Select(i =>
			{
				var location = new Location() { IncludeInSeatPlan = true };
				Enumerable.Range(0, 10).All(s =>
				{
					Enumerable.Range(0, 20).All(r =>
					{
						var childLocation = new Location() { IncludeInSeatPlan = true };
						location.AddChild(childLocation);

						Enumerable.Range(0, 30).All(c =>
						{
							childLocation.AddSeat(Guid.NewGuid(),"temp");
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

		#region Booking Period Tests

		[Test]
		public void ShouldDetectPeriodsOverlap()
		{

			var period1 = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 1, 15, 0, 0));
			var period2 = new BookingPeriod(new DateTime(2014, 01, 01, 7, 59, 59), new DateTime(2014, 01, 1, 8, 0, 0));

			Assert.True(period1.Intersects(period2));

		}

		[Test]
		public void ShouldDetectPeriodsDoNotOverlap()
		{

			var period1 = new BookingPeriod(new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 1, 15, 0, 0));
			var period2 = new BookingPeriod(new DateTime(2014, 01, 01, 7, 59, 58), new DateTime(2014, 01, 1, 7, 59, 59));

			Assert.True(!period1.Intersects(period2));

		}

		#endregion
	}
}


