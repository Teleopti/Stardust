using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	public class CommonSeatAllocatorTests
	{
		public static void ShouldAllocateAnAgentToASeat(bool useSeatLevelAllocator)
		{
			var agentShift = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest = new SeatBookingRequest(agentShift);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest);
			}
			
			Assert.That(agentShift.Seat.Name == "Seat1");
		}

		public static void ShouldAllocateTwoAgentsToOneSeatEach(bool useSeatLevelAllocator)
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);
			location.AddSeat("Seat2", 2);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}


			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };
			Assert.That(allocatedSeats.Contains("Seat1") && allocatedSeats.Contains("Seat2"));
		}

		public static void ShouldAllocateAccordingToPriority(bool useSeatLevelAllocator)
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 2);
			location.AddSeat("Seat2", 1);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift1.Seat.Name == "Seat2");
		}

		public static void ShouldAllocateTwoAgentsSequentiallyToOneSeat(bool useSeatLevelAllocator)
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 11, 59, 59));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}

			var allocatedSeats = new[] { agentShift1.Seat.Name, agentShift2.Seat.Name };

			Assert.That(allocatedSeats[0].Equals("Seat1") && allocatedSeats[1].Equals("Seat1"));
		}

		public static void ShouldAllocateSeatsByEarliestFirst(bool useSeatLevelAllocator)
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 1, 0, 0), new DateTime(2014, 01, 01, 9, 30, 00));
			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 10, 0, 0), new DateTime(2014, 01, 01, 18, 30, 0));
			var agentShift3 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 19, 0, 0), new DateTime(2014, 01, 02, 1, 30, 0));
			var agentShift4 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 02, 1, 0, 0), new DateTime(2014, 01, 02, 9, 30, 0));

			var seatBookingRequest1 = new SeatBookingRequest(agentShift1, agentShift2, agentShift3);
			var seatBookingRequest2 = new SeatBookingRequest(agentShift4);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest2, seatBookingRequest1);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest2, seatBookingRequest1);
			}
			
			Assert.That(seatBookingRequest1.SeatBookings.Count(booking => booking.Seat != null) == 3);
			Assert.That(seatBookingRequest2.SeatBookings.Count(booking => booking.Seat == null) == 1);
		}

		public static void ShouldNotAllocateTwoAgentsSequentiallyToOneSeat(bool useSeatLevelAllocator)
		{
			var agentShift1 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 12, 59, 59));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 12, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}

			Assert.That(agentShift2.Seat == null);
		}

		public static void ShouldAllocateAccordingToStartTime(bool useSeatLevelAllocator)
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var agentShift2 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(7), bookingDate.Date.AddHours(16));
			var seatBookingRequest = new SeatBookingRequest(agentShift1, agentShift2);
			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			var seats = new List<Seat>()
			{
				new Seat("Seat1", 1)
				
			};
			location.AddSeats (seats);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest);
			}
			Assert.That(agentShift2.Seat.Name == "Seat1");

		}

		public static void ShouldAllocateToGroupFirst(bool useSeatLevelAllocator)
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var agentShift3 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2, agentShift3);
			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			var seats = new List<Seat>()
			{
				new Seat("Seat1", 1),
				new Seat("Seat2", 2)
			};
			location.AddSeats(seats);
			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest2, seatBookingRequest1);
			}
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift3.Seat.Name == "Seat2");
			Assert.That(agentShift1.Seat == null);

		}

		public static void ShouldNotAllocateAnAgentToAnAlreadyBookedSeat(bool useSeatLevelAllocator)
		{
			var bookingDate = new DateOnly(2014, 01, 01);
			var booking = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(17));
			var seatBookingRequest = new SeatBookingRequest(booking);
			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			var seats = new List<Seat>()
			{
				new Seat ("Seat1", 1)
			};
			seats[0].AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date, bookingDate.Date.AddHours(9)));
			location.AddSeats (seats);

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest);
			}
			Assert.That(booking.Seat == null);

		}

		public static void ShouldAllocateToAvailableSeat(bool useSeatLevelAllocator)
		{
			var bookingDate = DateOnly.Today;

			var agentShift1 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(8), bookingDate.Date.AddHours(12));
			var seatBookingRequest1 = new SeatBookingRequest(agentShift1);

			var agentShift2 = new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(13), bookingDate.Date.AddHours(17));
			var seatBookingRequest2 = new SeatBookingRequest(agentShift2);
			var location = new SeatMapLocation() { IncludeInSeatPlan = true };

			var seats = new List<Seat>()
			{
				new Seat ("Seat1", 1),
				new Seat ("Seat2", 2)
			};

			location.AddSeats (seats);

			seats[0].AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date, bookingDate.Date.AddHours(10)));
			seats[1].AddSeatBooking(new SeatBooking(new Person(), bookingDate, bookingDate.Date.AddHours(15), bookingDate.Date.AddHours(20)));
			
			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest1, seatBookingRequest2);
			}
			Assert.That(agentShift2.Seat.Name == "Seat1");
			Assert.That(agentShift1.Seat.Name == "Seat2");
		}



		public static void ShouldNotAllocateAnAgentToASeatWhereRolesDoNotMatch(bool useSeatLevelAllocator)
		{
			var agentShift = new SeatBooking(new Person(), new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var seatBookingRequest = new SeatBookingRequest(agentShift);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1);

			location.Seats[0].SetRoles(new List<IApplicationRole>()
			{
				new ApplicationRole()
				{
					Name="ARole"
				}
			});

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest);
			}

			Assert.IsNull(agentShift.Seat);
		}

		public static void ShouldAllocateAnAgentToASeatWhereRolesMatch(bool useSeatLevelAllocator)
		{
			var outboundRole = new ApplicationRole()
			{
				Name = "OutboundRole"
			};
			var inboundRole = new ApplicationRole()
			{
				Name = "InboundRole"
			};

			var agentWithOutboundRole = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			agentWithOutboundRole.PermissionInformation.AddApplicationRole(outboundRole);

			var agentWithInboundRole = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			agentWithInboundRole.PermissionInformation.AddApplicationRole(inboundRole);

			var outboundAgentShift = new SeatBooking(agentWithOutboundRole, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var outboundAgentSeatBookingRequest = new SeatBookingRequest(outboundAgentShift);

			var inboundAgentShift = new SeatBooking(agentWithInboundRole, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var inboundAgentSeatBookingRequest = new SeatBookingRequest(inboundAgentShift);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1).SetRoles(new List<IApplicationRole>());
			location.AddSeat("Seat2", 2).SetRoles(new List<IApplicationRole>() { inboundRole });
			location.AddSeat("Seat3", 3).SetRoles(new List<IApplicationRole>() { outboundRole });

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(outboundAgentSeatBookingRequest, inboundAgentSeatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(outboundAgentSeatBookingRequest, inboundAgentSeatBookingRequest);
			}

			Assert.AreEqual(location.Seats[1], inboundAgentShift.Seat);
			Assert.AreEqual(location.Seats[2], outboundAgentShift.Seat);

		}

		public static void ShouldAllocateAnAgentToASeatWhereAllRolesMatch(bool useSeatLevelAllocator)
		{
			var outboundRole = new ApplicationRole()
			{
				Name = "OutboundRole"
			};
			var teamLeader = new ApplicationRole()
			{
				Name = "TeamLeader"
			};

			var outboundTeamLeader = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			outboundTeamLeader.PermissionInformation.AddApplicationRole(outboundRole);
			outboundTeamLeader.PermissionInformation.AddApplicationRole(teamLeader);

			var outBoundAgent1 = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			outBoundAgent1.PermissionInformation.AddApplicationRole(outboundRole);

			var outBoundAgent2 = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			outBoundAgent2.PermissionInformation.AddApplicationRole(outboundRole);

			var teamLeaderShift = new SeatBooking(outboundTeamLeader, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var outboundAgent1Shift = new SeatBooking(outBoundAgent1, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));
			var outboundAgent2Shift = new SeatBooking(outBoundAgent2, new DateOnly(2014, 01, 01), new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 17, 0, 0));

			var seatBookingRequest = new SeatBookingRequest(teamLeaderShift, outboundAgent1Shift, outboundAgent2Shift);

			var location = new SeatMapLocation() { IncludeInSeatPlan = true };
			location.AddSeat("Seat1", 1).SetRoles(new List<IApplicationRole>() { outboundRole });
			location.AddSeat("Seat2", 2).SetRoles(new List<IApplicationRole>() { outboundRole, teamLeader });
			location.AddSeat("Seat3", 3).SetRoles(new List<IApplicationRole>() { outboundRole });

			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator(location.Seats).AllocateSeats(seatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(seatBookingRequest);
			}

			Assert.AreEqual(location.Seats[1], teamLeaderShift.Seat);
		}

		
		public static void ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority(bool useSeatLevelAllocator)
		{
			var outboundRole = new ApplicationRole()
			{
				Name = "OutboundRole"
			};

			var agentWithOutboundRole = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			agentWithOutboundRole.PermissionInformation.AddApplicationRole (outboundRole);

			var outboundAgentShift = new SeatBooking (agentWithOutboundRole, new DateOnly (2014, 01, 01),
				new DateTime (2014, 01, 01, 8, 0, 0), new DateTime (2014, 01, 01, 17, 0, 0));
			var outboundAgentSeatBookingRequest = new SeatBookingRequest (outboundAgentShift);


			var location = new SeatMapLocation() {IncludeInSeatPlan = true};
			location.AddSeat ("Seat1", 1).SetRoles (new List<IApplicationRole>());
			location.AddSeat ("Seat2", 3).SetRoles (new List<IApplicationRole>() {outboundRole});
			location.AddSeat ("Seat3", 2).SetRoles (new List<IApplicationRole>() {outboundRole});
			location.AddSeat ("Seat4", 4).SetRoles (new List<IApplicationRole>() {outboundRole});


			if (useSeatLevelAllocator)
			{
				new SeatLevelAllocator (location.Seats).AllocateSeats (outboundAgentSeatBookingRequest);
			}
			else
			{
				new SeatAllocator(location).AllocateSeats(outboundAgentSeatBookingRequest);
			}
			
			Assert.AreEqual (location.Seats[2], outboundAgentShift.Seat);
		}
	}
}