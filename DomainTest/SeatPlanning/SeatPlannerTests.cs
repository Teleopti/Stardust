using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	[DomainTest]
	public class SeatPlannerTests
	{
		public FakeScenarioRepository FakeScenarioRepository;
		public FakeSeatBookingRepository SeatBookingRepository;
		public FakeSeatPlanRepository SeatPlanRepository;
		public FakeSeatMapRepository SeatMapRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonRepository PersonRepository;
		public FakeTeamRepository TeamRepository;
		public SeatPlanner Target;
		
		private IPerson createPersonWithPersonPeriodFromTeam(DateTime startDate, Team team)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			((Person)person).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			PersonRepository.Has(person);
			return person;
		}

		private void addAssignment(IPerson person, DateTime startDate, DateTime endDate)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				FakeScenarioRepository.LoadDefaultScenario(), new DateTimePeriod(startDate, endDate)).WithId();
			PersonAssignmentRepository.Add(assignment);
		}
		
		private static IList<Guid> getLocationGuids(IEnumerable<SeatMapLocation> locations)
		{
			return locations.Select(location => location.Id.Value).ToList();
		}

		private static IList<Guid> getTeamGuids(IEnumerable<Team> teams)
		{
			return teams.Select(team => team.Id.Value).ToList();
		}

		private void addSeatBooking(IPerson person, DateTime belongsToDate, DateTime startDateTime, DateTime endDateTime, ISeat seat)
		{
			var existingSeatBooking = new SeatBooking(person, new DateOnly(belongsToDate), startDateTime, endDateTime)
			{
				Seat = seat
			};

			existingSeatBooking.SetId(Guid.NewGuid());
			SeatBookingRepository.Add(existingSeatBooking);
		}

		private Team addTeam(String name)
		{
			var team = new Team().WithId();
			team.SetDescription(new Description(name));
			TeamRepository.Has(team);
			return team;
		}

		private SeatMapLocation addLocation(String name, IEnumerable<SeatMapLocation> childLocations, params Seat[] seats)
		{
			var seatMapLocation = new SeatMapLocation { Name = name }.WithId();
			seats?.ForEach(seat => seatMapLocation.AddSeat(seat.Name, seat.Priority));
			if (childLocations != null)
			{
				seatMapLocation.AddChildren(childLocations);
			}
			SeatMapRepository.Add(seatMapLocation);
			return seatMapLocation;
		}
		
		[Test]
		public void ShouldBookSeat()
		{
			FakeScenarioRepository.Has("Default");

			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam(startDate, team);

			addAssignment(person, startDate, endDate);

			var seatMapLocation = addLocation("Location", new SeatMapLocation[0], new Seat("Seat One", 1));
			
			Target.Plan(
				getLocationGuids(new[] { seatMapLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatBooking = SeatBookingRepository.Single() as SeatBooking;
			seatBooking.StartDateTime.Date.Should().Be(startDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(endDate.Date);
			seatBooking.Seat.Should().Be(seatMapLocation.Seats.Single());
		}
		
		[Test]
		public void ShouldBookSeatForMutipleDays()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 22, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team 1");
			var teams = new[] { team };
			
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);

			addAssignment(person, startDate, startDate.AddHours(8));
			addAssignment(person, startDate.AddDays(1), startDate.AddDays(1).AddHours(8));
			addAssignment(person, endDate, endDate.AddHours(8));
			
			var seatMapLocation = addLocation("Location", null, new Seat("Seat 1", 1));
			
			Target.Plan(
				getLocationGuids(new[] { seatMapLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			Assert.IsTrue(SeatBookingRepository.CountAllEntities() == 3);
		}

		[Test]
		public void ShouldSkipNonSelectedLocations()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			addAssignment(person, startDate, endDate);

			var childLocations = new[]
			{
				addLocation ("Location1",null, new Seat("Seat One", 1)),
				addLocation ("Location2",null, new Seat("Seat One", 1))
			};

			addLocation("Root Location", childLocations);
			
			Target.Plan(
				getLocationGuids(new[] { childLocations[1] }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatBooking = SeatBookingRepository.Single() as SeatBooking;
			seatBooking.Seat.Should().Be(childLocations[1].Seats.Single());
		}

		[Test]
		public void ShouldGroupTeamBookings()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var startDateOnly = new DateOnly(startDate);
			var teams = new[]
			{
				addTeam ("Team"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[0])
			};

			addAssignment(people[0], startDate, endDate);
			addAssignment(people[1], startDate, endDate);
			addAssignment(people[2], startDate, endDate);
			
			var childLocations = new[]
			{
				addLocation ("Location1",null, new Seat("Seat One", 1).WithId()),
				addLocation ("Location2",null, new Seat("Seat One", 1).WithId(), new Seat("Seat Two",2).WithId())
			};

			addLocation("Root Location", childLocations);
			
			Target.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
		}

		[Test]
		public void ShouldGroupManyTeamBookings()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var startDateOnly = new DateOnly(startDate);

			var teams = new[]
			{
				addTeam ("Team1"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, teams[0]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[0]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDate, teams[1])
			};
			
				addAssignment (people[0], startDate, endDate);
				addAssignment (people[1], startDate, endDate);
				addAssignment (people[2], startDate, endDate);
				addAssignment (people[3], startDate, endDate);
				addAssignment (people[4], startDate, endDate);
			addAssignment(people[5], startDate, endDate);
			
			var childLocations = new[]
			{
				addLocation ("Location1", null, new[]
				{
					new Seat ("Location 1, Seat 1", 1),
					new Seat ("Location 1, Seat 2", 2),
					new Seat ("Location 1, Seat 3", 3),
					new Seat ("Location 1, Seat 4", 4)
				}),
				addLocation ("Location2", null, new[]
				{
					new Seat ("Location 2, Seat 1", 1),
					new Seat ("Location 2, Seat 2", 2)
				})
			};

			addLocation("RootLocation", childLocations);
			
			Target.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);
		}

		[Test]
		public void ShouldGroupTeamBookingsAcrossMultipleDays()
		{
			FakeScenarioRepository.Has("Default");
			var startDateDay1 = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDateDay1 = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);

			var startDateDay2 = startDateDay1.AddDays(1);
			var endDateDay2 = startDateDay2.AddDays(1);

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDateDay1), new DateOnly(endDateDay2));

			var startDateOnlyDay1 = new DateOnly(startDateDay1);
			var startDateOnlyDay2 = new DateOnly(startDateDay2);
			var teams = new[]
			{
				addTeam ("Team"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[0]),
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[0]),
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[1]),
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[1])
			};
			
				addAssignment (people[0], startDateDay1, endDateDay1);
				addAssignment (people[1], startDateDay1, endDateDay1);
				addAssignment (people[2], startDateDay1, endDateDay1);
				addAssignment (people[3], startDateDay1, endDateDay1);
				addAssignment (people[4], startDateDay1, endDateDay1);
				addAssignment (people[5], startDateDay1, endDateDay1);
																	 
				addAssignment (people[0], startDateDay2, endDateDay2);
				addAssignment (people[1], startDateDay2, endDateDay2);
				addAssignment (people[2], startDateDay2, endDateDay2);
				addAssignment (people[3], startDateDay2, endDateDay2);
				addAssignment (people[4], startDateDay2, endDateDay2);
			addAssignment(people[5], startDateDay2, endDateDay2);
			
			var childLocations = new[]
			{
				addLocation ("Location1", null, new[]
				{
					new Seat ("Location 1, Seat 1", 1),
					new Seat ("Location 1, Seat 2", 2),
					new Seat ("Location 1, Seat 3", 3),
					new Seat ("Location 1, Seat 4", 4)
				}),
				addLocation ("Location2", null, new[]
				{
					new Seat ("Location 2, Seat 1", 1),
					new Seat ("Location 2, Seat 2", 2)
				})
			};

			addLocation("RootLocation", childLocations, null);
			
			Target.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);
			
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);

			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			SeatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);
		}

		[Test]
		public void ShouldHonourExistingBookingsForOtherAgents()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				// by putting people[1] in a different team, they will not be included in the seat 
				// planning process and should retain their existing booking.
				createPersonWithPersonPeriodFromTeam(endDate, addTeam("Team Other"))
			};

			addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should()
				.Be(seatMapLocation.Seats.First());

			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldHonourExistingBookingsOnChildLocation()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var people = new[]
			{
				// by putting people[0] in a different team, they will not be included in the seat 
				// planning process and should retain their existing booking (even though we havent created a schedule here).
					createPersonWithPersonPeriodFromTeam(startDate,  addTeam("Team Other")),
					createPersonWithPersonPeriodFromTeam(startDate, team)
			};

			var childLocation = addLocation("ChildLocation", null, new Seat("Seat 1", 1));
			addLocation("RootLocation", new[] { childLocation }, null);

			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);


			addSeatBooking(people[0], startDate, startDate, assignmentEndDateTime, childLocation.Seats[0]);

			addAssignment(people[1], startDate, assignmentEndDateTime);
			
			Target.Plan(
				getLocationGuids(new[] { childLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);
			
			SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0])
				.Seat.Should().Be(childLocation.Seats[0]);

			Assert.IsNull(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1]));
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgent()
		{
			FakeScenarioRepository.Has("Default");

			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2) 
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats.Last());

			addAssignment(person, date, date.AddHours(8));
			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);
			
			SeatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats.First());
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriod()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };

			var assignmentEndDateTime = new DateTime(date.Year, date.Month, date.Day, 9, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
		};

			addAssignment(people[0], assignmentEndDateTime.AddHours(-6), assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var bookingStartDate = new DateTime(date.Year, date.Month, date.Day - 1, 21, 00, 00, DateTimeKind.Utc);
			addSeatBooking(people[1], bookingStartDate, bookingStartDate, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}

		[Test]
		public void ShouldAddAfterOvernightBooking()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 1, 20, 9, 1, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));

			var team = addTeam("Team");
			var teams = new[] { team };

			var assignmentStartDateTime = new DateTime(2015, 1, 19, 21, 00, 00, DateTimeKind.Utc);
			var assignmentEndDateTime = new DateTime(2015, 1, 20, 9, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
		};

			addAssignment(people[0], date, date.AddHours(8));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], assignmentStartDateTime, assignmentEndDateTime, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);

			SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
		}

		[Test]
		public void BookingsOfAnEarlierTimeShouldGetPrecedence()
		{
			FakeScenarioRepository.Has("Default");
			var startDateTime = new DateTime(2015, 01, 21);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDateTime), new DateOnly(startDateTime));

			var teams = new[]
			{
				addTeam ("Team 1"),
				addTeam ("Team 2")
			};

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(new DateTime(2015, 01, 01), teams[0]),
				createPersonWithPersonPeriodFromTeam(new DateTime(2015, 01, 01), teams[1])
			};
			
				addAssignment (people[0],new DateTime(2015, 1, 21, 11, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 17, 0, 0, DateTimeKind.Utc));
				addAssignment (people[1],new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 12, 0, 0, DateTimeKind.Utc));
			
			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			Assert.That(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDateTime), people[0]) == null);
			Assert.That(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDateTime), people[1]) != null);
		}

		[Test]
		public void ShouldAllocateSeatsInOrderWhenPlanningOvernightEvenWhenBookingsExist()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var startDate = new DateOnly(date);
			var dateOnlyPeriod = new DateOnlyPeriod(startDate, new DateOnly(date.AddDays(1)));

			var team = addTeam("Team");
			var teams = new[] { team };

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
			};

			addAssignment(people[0], new DateTime(2015, 1, 20, 1, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 20, 9, 30, 0, DateTimeKind.Utc));
			addAssignment(people[1], new DateTime(2015, 1, 20, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 20, 18, 30, 0, DateTimeKind.Utc));
			addAssignment(people[2], new DateTime(2015, 1, 20, 19, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 21, 1, 30, 0, DateTimeKind.Utc));

			addAssignment(people[0], new DateTime(2015, 1, 21, 1, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 21, 9, 30, 0, DateTimeKind.Utc));
			
			var location = addLocation("Location", null, new Seat("Seat One", 1));
			var locations = new[] { location };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);
			
			SeatBookingRepository.LoadSeatBookingForPerson(startDate, people[0]).Seat.Should().Be(location.Seats[0]);

			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingsForDay(startDate).Count == 3);
			Assert.IsFalse(SeatBookingRepository.LoadSeatBookingsForDay(startDate.AddDays(1)).Any());

			// check overwrite gets same result
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.LoadSeatBookingForPerson(startDate, people[0]).Seat.Should().Be(location.Seats[0]);

			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingsForDay(startDate).Count == 3);
			Assert.IsTrue(!SeatBookingRepository.LoadSeatBookingsForDay(startDate.AddDays(1)).Any());
		}

		[Test]
		public void ShouldPersistSeatPlans()
		{
			FakeScenarioRepository.Has("Default");

			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam (startDate, team);
			addAssignment(person, startDate, startDate.AddHours(8));
			addAssignment(person, endDate, endDate.AddHours(8));

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = SeatPlanRepository.First();
			var seatPlanDayTwo = SeatPlanRepository.Last();
			seatPlanDayOne.Date.Date.Should().Be(startDate.Date);
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Date.Date.Should().Be(endDate.Date);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.Ok);
		}

		[Test]
		public void ShouldUpdateExistingSeatPlan()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			addAssignment(person, startDate, startDate.AddHours(8));

			SeatPlanRepository.Add(new SeatPlan
			{
				Date = new DateOnly(startDate.Date),
				Status = SeatPlanStatus.InError

			});

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);
			
			var seatPlan = SeatPlanRepository.Single();

			seatPlan.Date.Date.Should().Be(startDate.Date);
			seatPlan.Status.Should().Be(SeatPlanStatus.Ok);
		}

		[Test]
		public void ShouldPersistSeatPlanWithErrorStatus()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			addAssignment(person, startDate, endDate);

			var locations = new[] { addLocation("Location", null, null) };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = SeatPlanRepository.First();
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.InError);

		}

		[Test]
		public void ShouldPersistSeatPlanWithDifferentStatusInsidePlanningPeriod()
		{
			FakeScenarioRepository.Has("Default");
			var startDate1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate1 = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var startDate2 = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);
			var endDate2 = new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate1), new DateOnly(endDate2));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate1, team);
			var person2 = createPersonWithPersonPeriodFromTeam(startDate1, team);

			addAssignment(person, startDate1, endDate1);
			addAssignment(person2, startDate2, endDate2);
			addAssignment(person, startDate2, endDate2);

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = SeatPlanRepository.First();
			var seatPlanDayTwo = SeatPlanRepository.Last();

			SeatPlanRepository.Count().Should().Be(2);

			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.InError);
		}
		
		[Test]
		public void ShouldBookSeatBySeatAndPerson()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			addAssignment(person, startDate, endDate);

			var seatMapLocation = new SeatMapLocation().WithId();
			SeatMapRepository.Add(seatMapLocation);
			var seat = seatMapLocation.AddSeat("seat", 1);
			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seat.Id.Value }.ToList(),
				new[] { person.Id.Value }.ToList());

			var seatBooking = SeatBookingRepository.Single();
			seatBooking.StartDateTime.Date.Should().Be(startDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(endDate.Date);
			seatBooking.Seat.Should().Be(seat);
		}

		[Test]
		public void ShouldBookSeatBySeatsAndPeople()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 9, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(startDate, team)
			};
			
				addAssignment(people[0], startDate, endDate);
				addAssignment(people[1], startDate, endDate);
			addAssignment(people[2], startDate, endDate);
			
			var seatMapLocation = addLocation("location", null,
												new Seat("seat1", 1),
												new Seat("seat2", 2),
												new Seat("seat3", 3));


			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				seatMapLocation.Seats.Select(seat => seat.Id.Value).ToList(),
				people.Select(person => person.Id.Value).ToList());

			SeatBookingRepository.CountAllEntities().Should().Be(3);
			var seatBookings = SeatBookingRepository.LoadAll();

			foreach (var seatBooking in seatBookings)
			{
				seatBooking.StartDateTime.Date.Should().Be(startDate.Date);
				seatBooking.EndDateTime.Date.Should().Be(endDate.Date);
				seatBooking.Seat.Should().Not.Be(null);
			}
		}

		[Test]
		public void ShouldHonourExistingBookingsForOtherAgentsBySeatAndPerson()
		{
			FakeScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var assignmentEndDateTime = endDate.AddHours(13);

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(endDate, team)
			};

			addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[0].Id.Value }.ToList(),
				new[] { people[0].Id.Value }.ToList());
			
			SeatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should().Be(seatMapLocation.Seats.First());

			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgentBySeatAndPerson()
		{
			FakeScenarioRepository.Has("Default");

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var person = createPersonWithPersonPeriodFromTeam (date, team);
			addAssignment(person, date, date.AddHours(8));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2)
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats[1]);

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[1].Id.Value }.ToList(),
				new[] { person.Id.Value }.ToList());

			SeatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats[1]);
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriodIfGivingASeatAndPerson()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
			};

			addAssignment(people[0], date.AddHours(3), date.AddHours(10));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], date.AddDays(-1), date.AddDays(-1).AddHours(21), date.AddHours(8), seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[0].Id.Value }.ToList(),
				new[] { people[1].Id.Value }.ToList());
			
			SeatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}
		
		[Test]
		public void ShouldReturnSeatPlanningResultInformation()
		{
			FakeScenarioRepository.Has("Default");
			testSeatPlanningResultInformation( false);
		}

		[Test]
		public void ShouldReturnSeatPlanningResultInformationBySeatsAndPeople()
		{
			FakeScenarioRepository.Has("Default");

			testSeatPlanningResultInformation(true);
		}

		private void testSeatPlanningResultInformation(bool manuallyPlanSeatsAndPeople)
		{
			var startDate = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var team = addTeam("Team");
			var team2 = addTeam("Team2");
			var teams = new[] { team, team2 };

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(startDate, team2),
				createPersonWithPersonPeriodFromTeam(endDate, team)
			};

			addAssignment(people[0], startDate, startDate.AddHours(8));
			addAssignment(people[1], startDate, startDate.AddHours(8));
			addAssignment(people[2], endDate, endDate.AddHours(8));
			addAssignment(people[1], endDate.AddHours (9), endDate.AddHours(17));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat ("Seat One", 1)
			});

			var locations = new[] { seatMapLocation };
			
			ISeatPlanningResult result;
			
			if (manuallyPlanSeatsAndPeople)
			{
				result = Target.Plan(
									getLocationGuids(locations),
									null,
									dateOnlyPeriod,
									seatMapLocation.Seats.Select(seat => seat.Id.Value).ToList(),
									people.Select(person => person.Id.Value).ToList());
			}
			else
			{
				result = Target.Plan(
									getLocationGuids(locations),
									getTeamGuids(teams),
									dateOnlyPeriod,
									null,
									null);
			}


			Assert.AreEqual(4, result.NumberOfBookingRequests);
			Assert.AreEqual(3, result.RequestsGranted);
			Assert.AreEqual(1, result.RequestsDenied);
			Assert.AreEqual(2, result.NumberOfUnscheduledAgentDays);
		}

		[Test]
		public void ShouldPlanSeatsConsideringFrequency()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam (date, team);
			addAssignment(person, date, date.AddHours(8));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2)
			});

			var theDateBefore = date.AddDays (-1);
			addSeatBooking(person, theDateBefore, theDateBefore, theDateBefore.AddHours(10), seatMapLocation.Seats[1]);

			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod,
				null,
				null);

			SeatBookingRepository.CountAllEntities().Should().Be(2);
			var booking = SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats[1]);
		}
		
		[Test]
		public void ShouldNotAllocateASeatToAnAgentWithADayOff()
		{
			FakeScenarioRepository.Has("Default");
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1)
			});

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithDayOff(person,
				FakeScenarioRepository.LoadDefaultScenario(), dateOnlyPeriod.StartDate,
				new DayOffTemplate(new Description("for", "test"))));
			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.CountAllEntities().Should().Be(0);
			var booking = SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Should().Be.Null();
		}


		[Test]
		public void ShouldRemoveExistingBookingForAgentWhenTheirScheduleChangesToDayOff()
		{
			FakeScenarioRepository.Has("Default");

			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1),
				new Seat("Seat Two",2)
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats.Last());

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithDayOff(person,
				FakeScenarioRepository.LoadDefaultScenario(), dateOnlyPeriod.StartDate,
				new DayOffTemplate(new Description("for", "test"))));
			var locations = new[] { seatMapLocation };
			
			Target.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			SeatBookingRepository.CountAllEntities().Should().Be(0);
			var booking = SeatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Should().Be.Null();
		}
	}
}