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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	internal class SeatPlannerTests
	{
		private ICurrentScenario _currentScenario;
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatPlanRepository _seatPlanRepository;

		private void setup()
		{
			_currentScenario = new FakeCurrentScenario();
			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatPlanRepository = new FakeSeatPlanRepository();
		}

		#region helper methods


		private static IPerson createPersonWithPersonPeriodFromTeam(DateTime startDate, Team team)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			((Person)person).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			return person;
		}

		private IPersonAssignment addAssignment(IPerson person, DateTime startDate, DateTime endDate)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				_currentScenario.Current(), new DateTimePeriod(startDate, endDate));
		}

		private SeatPlanner setupSeatPlanner(ITeam[] teams, IEnumerable<IPerson> people, IEnumerable<ISeatMapLocation> seatMapLocations, params IPersonAssignment[] personAssignment)
		{
			var seatBookingRequestAssembler = new SeatBookingRequestAssembler(
				new FakeScheduleDataReadScheduleStorage(personAssignment), _seatBookingRepository, _currentScenario);

			var seatFrequencyCalculator = new SeatFrequencyCalculator (_seatBookingRepository);

			var seatPlanner = new SeatPlanner(new FakePersonRepositoryLegacy(people.ToArray()), seatBookingRequestAssembler,
				new SeatPlanPersister(_seatBookingRepository, _seatPlanRepository), new FakeTeamRepository(teams), new FakeSeatMapRepository(seatMapLocations.ToArray()), seatFrequencyCalculator);

			return seatPlanner;
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
			_seatBookingRepository.Add(existingSeatBooking);
		}

		private static Team addTeam(String name)
		{
			var team = new Team();
			team.SetDescription(new Description(name));
			team.SetId(Guid.NewGuid());
			return team;
		}

		private static SeatMapLocation addLocation(String name, IEnumerable<SeatMapLocation> childLocations, params Seat[] seats)
		{
			var seatMapLocation = new SeatMapLocation() { Name = name };
			seatMapLocation.SetId(Guid.NewGuid());
			if (seats != null)
			{
				seats.ForEach(seat => seatMapLocation.AddSeat(seat.Name, seat.Priority));
			}
			if (childLocations != null)
			{
				seatMapLocation.AddChildren(childLocations);
			}
			return seatMapLocation;
		}

		#endregion

		[Test]
		public void ShouldBookSeat()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam(startDate, team);

			var personAssignment = addAssignment(person, startDate, endDate);

			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));
			var seatPlanner = setupSeatPlanner(teams, new[] { person }, new[] { seatMapLocation }, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(new[] { seatMapLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.StartDateTime.Date.Should().Be(startDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(endDate.Date);
			seatBooking.Seat.Should().Be(seatMapLocation.Seats.Single());
		}

		

		[Test]
		public void ShouldBookSeatForMutipleDays()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 22, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team 1");
			var teams = new[] { team };
			
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);

			var personAssignments = new List<IPersonAssignment>()
			{
				addAssignment (person, startDate, startDate.AddHours (8)),
				addAssignment (person, startDate.AddDays (1), startDate.AddDays (1).AddHours (8)),
				addAssignment (person, endDate, endDate.AddHours (8))
			};

			var seatMapLocation = addLocation("Location", null, new Seat("Seat 1", 1));
			var seatPlanner = setupSeatPlanner(teams, new[] { person }, new[] { seatMapLocation }, personAssignments.ToArray());

			seatPlanner.Plan(
				getLocationGuids(new[] { seatMapLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			Assert.IsTrue(_seatBookingRepository.CountAllEntities() == 3);
		}

		[Test]
		public void ShouldSkipNonSelectedLocations()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var childLocations = new[]
			{
				addLocation ("Location1",null, new Seat("Seat One", 1)),
				addLocation ("Location2",null, new Seat("Seat One", 1))

			};

			var rootLocation = addLocation("Root Location", childLocations);
			var seatPlanner = setupSeatPlanner(teams, new[] { person }, new[] { rootLocation, childLocations[0], childLocations[1] }, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(new[] { childLocations[1] }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.Seat.Should().Be(childLocations[1].Seats.Single());
		}

		[Test]
		public void ShouldGroupTeamBookings()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
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

			var assignments = new[]
			{
				addAssignment (people[0], startDate, endDate),
				addAssignment (people[1], startDate, endDate),
				addAssignment (people[2], startDate, endDate)
			};

			var childLocations = new[]
			{
				addLocation ("Location1",null, new Seat("Seat One", 1)),
				addLocation ("Location2",null, new Seat("Seat One", 1), new Seat("Seat Two",2))
			};

			var rootLocation = addLocation("Root Location", childLocations);


			var seatPlanner = setupSeatPlanner(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] }, assignments);

			seatPlanner.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
		}

		[Test]
		public void ShouldGroupManyTeamBookings()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
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
				createPersonWithPersonPeriodFromTeam(startDate, teams[1]),
			};

			var personAssignments = new[]
			{
				addAssignment (people[0], startDate, endDate),
				addAssignment (people[1], startDate, endDate),
				addAssignment (people[2], startDate, endDate),
				addAssignment (people[3], startDate, endDate),
				addAssignment (people[4], startDate, endDate),
				addAssignment (people[5], startDate, endDate),
			};

			var childLocations = new[]
			{
				addLocation ("Location1", null, new[]
				{
					new Seat ("Location 1, Seat 1", 1),
					new Seat ("Location 1, Seat 2", 2),
					new Seat ("Location 1, Seat 3", 3),
					new Seat ("Location 1, Seat 4", 4),
				}),
				addLocation ("Location2", null, new[]
				{
					new Seat ("Location 2, Seat 1", 1),
					new Seat ("Location 2, Seat 2", 2),
				})
			};

			var rootLocation = addLocation("RootLocation", childLocations, null);


			var seatPlanner = setupSeatPlanner(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] },
				personAssignments);

			seatPlanner.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnly, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);

		}

		[Test]
		public void ShouldGroupTeamBookingsAcrossMultipleDays()
		{
			setup();
			var startDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

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
				createPersonWithPersonPeriodFromTeam(startDateDay1, teams[1]),
			};

			var assignments = new[]
			{
				addAssignment (people[0], startDateDay1, endDateDay1),
				addAssignment (people[1], startDateDay1, endDateDay1),
				addAssignment (people[2], startDateDay1, endDateDay1),
				addAssignment (people[3], startDateDay1, endDateDay1),
				addAssignment (people[4], startDateDay1, endDateDay1),
				addAssignment (people[5], startDateDay1, endDateDay1),
			
				addAssignment (people[0], startDateDay2, endDateDay2),
				addAssignment (people[1], startDateDay2, endDateDay2),
				addAssignment (people[2], startDateDay2, endDateDay2),
				addAssignment (people[3], startDateDay2, endDateDay2),
				addAssignment (people[4], startDateDay2, endDateDay2),
				addAssignment (people[5], startDateDay2, endDateDay2),
			};

			var childLocations = new[]
			{
				addLocation ("Location1", null, new[]
				{
					new Seat ("Location 1, Seat 1", 1),
					new Seat ("Location 1, Seat 2", 2),
					new Seat ("Location 1, Seat 3", 3),
					new Seat ("Location 1, Seat 4", 4),
				}),
				addLocation ("Location2", null, new[]
				{
					new Seat ("Location 2, Seat 1", 1),
					new Seat ("Location 2, Seat 2", 2),
				})
			};

			var rootLocation = addLocation("RootLocation", childLocations, null);

			var seatPlanner = setupSeatPlanner(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] }, assignments);

			seatPlanner.Plan(
				getLocationGuids(childLocations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[3]).Seat.Should().Be(childLocations[0].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[4]).Seat.Should().Be(childLocations[0].Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, people[5]).Seat.Should().Be(childLocations[0].Seats[3]);
		}

		[Test]
		public void ShouldHonourExistingBookingsForOtherAgents()
		{
			setup();
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
				createPersonWithPersonPeriodFromTeam(endDate, addTeam("Team Other")),
			};

			var personAssignment = addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should()
				.Be(seatMapLocation.Seats.First());

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldHonourExistingBookingsOnChildLocation()
		{
			setup();
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
					createPersonWithPersonPeriodFromTeam(startDate, team),
			};

			var childLocation = addLocation("ChildLocation", null, new Seat("Seat 1", 1));
			var rootLocation = addLocation("RootLocation", new[] { childLocation }, null);

			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);


			addSeatBooking(people[0], startDate, startDate, assignmentEndDateTime, childLocation.Seats[0]);

			var assignment = addAssignment(people[1], startDate, assignmentEndDateTime);

			var locations = new[] { rootLocation, childLocation };
			var seatPlanner = setupSeatPlanner(teams, people, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(new[] { childLocation }),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0])
				.Seat.Should().Be(childLocation.Seats[0]);

			Assert.IsNull(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1]));
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgent()
		{
			setup();
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2), 
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats.Last());

			var assignment = addAssignment(person, date, date.AddHours(8));
			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			_seatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats.First());
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriod()
		{
			setup();
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

			var assignment = addAssignment(people[0], assignmentEndDateTime.AddHours(-6), assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var bookingStartDate = new DateTime(date.Year, date.Month, date.Day - 1, 21, 00, 00, DateTimeKind.Utc);
			addSeatBooking(people[1], bookingStartDate, bookingStartDate, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}

		[Test]
		public void ShouldAddAfterOvernightBooking()
		{
			setup();
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

			var assignment = addAssignment(people[0], date, date.AddHours(8));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], assignmentStartDateTime, assignmentEndDateTime, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
		}

		[Test]
		public void BookingsOfAnEarlierTimeShouldGetPrecedence()
		{
			setup();
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
				createPersonWithPersonPeriodFromTeam(new DateTime(2015, 01, 01), teams[1]),
			};

			var assignments = new[]
			{
				addAssignment (people[0],new DateTime(2015, 1, 21, 11, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 17, 0, 0, DateTimeKind.Utc)),
				addAssignment (people[1],new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 12, 0, 0, DateTimeKind.Utc)),
			};

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignments);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			Assert.That(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDateTime), people[0]) == null);
			Assert.That(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDateTime), people[1]) != null);
		}

		[Test]
		public void ShouldAllocateSeatsInOrderWhenPlanningOvernightEvenWhenBookingsExist()
		{
			setup();
			var date = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date.AddDays(1)));

			var team = addTeam("Team");
			var teams = new[] { team };

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
			};

			var assignments = new[]
			{
				addAssignment (people[0], new DateTime(2015, 1, 20, 1, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 20, 9, 30, 0, DateTimeKind.Utc)),
				addAssignment (people[1], new DateTime(2015, 1, 20, 10, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 20, 18, 30, 0, DateTimeKind.Utc)),
				addAssignment (people[2], new DateTime(2015, 1, 20, 19, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 1, 30, 0, DateTimeKind.Utc)),

				addAssignment (people[0], new DateTime(2015, 1, 21, 1, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 9, 30, 0, DateTimeKind.Utc)),
			};

			var location = addLocation("Location", null, new Seat("Seat One", 1));
			var locations = new[] { location };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignments);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]).Seat.Should().Be(location.Seats[0]);

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date)).Count() == 3);
			Assert.IsFalse(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date).AddDays(1)).Any());

			// check overwrite gets same result
			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]).Seat.Should().Be(location.Seats[0]);

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date)).Count() == 3);
			Assert.IsTrue(!_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date).AddDays(1)).Any());

		}


		[Test]
		public void ShouldPersistSeatPlans()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };

			var person = createPersonWithPersonPeriodFromTeam (startDate, team);
			var personAssignment = addAssignment(person, startDate, startDate.AddHours(8));
			var personAssignment2 = addAssignment(person, endDate, endDate.AddHours(8));

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, personAssignment, personAssignment2);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = _seatPlanRepository.First();
			var seatPlanDayTwo = _seatPlanRepository.Last();
			seatPlanDayOne.Date.Date.Should().Be(startDate.Date);
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Date.Date.Should().Be(endDate.Date);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.Ok);
		}

		[Test]
		public void ShouldUpdateExistingSeatPlan()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			var personAssignment = addAssignment(person, startDate, startDate.AddHours(8));

			_seatPlanRepository.Add(new SeatPlan()
			{
				Date = new DateOnly(startDate.Date),
				Status = SeatPlanStatus.InError

			});

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);


			var seatPlan = _seatPlanRepository.Single();

			seatPlan.Date.Date.Should().Be(startDate.Date);
			seatPlan.Status.Should().Be(SeatPlanStatus.Ok);

		}

		[Test]
		public void ShouldPersistSeatPlanWithErrorStatus()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var locations = new[] { addLocation("Location", null, null) };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = _seatPlanRepository.First();
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.InError);

		}

		[Test]
		public void ShouldPersistSeatPlanWithDifferentStatusInsidePlanningPeriod()
		{
			setup();

			var startDate1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate1 = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var startDate2 = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);
			var endDate2 = new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate1), new DateOnly(endDate2));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate1, team);
			var person2 = createPersonWithPersonPeriodFromTeam(startDate1, team);

			var personAssignment = addAssignment(person, startDate1, endDate1);
			var personAssignmentPerson2 = addAssignment(person2, startDate2, endDate2);
			var personAssignment2 = addAssignment(person, startDate2, endDate2);

			var locations = new[] { addLocation("Location", null, new Seat("Seat One", 1)) };
			var seatPlanner = setupSeatPlanner(teams, new[] { person, person2 }, locations, personAssignment, personAssignmentPerson2, personAssignment2);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			var seatPlanDayOne = _seatPlanRepository.First();
			var seatPlanDayTwo = _seatPlanRepository.Last();

			_seatPlanRepository.Count().Should().Be(2);

			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.InError);
		}



		[Test]
		public void ShouldBookSeatBySeatAndPerson()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(startDate, team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetId(Guid.NewGuid());
			var seat = seatMapLocation.AddSeat("seat", 1);
			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seat.Id.Value }.ToList(),
				new[] { person.Id.Value }.ToList());

			var seatBooking = _seatBookingRepository.Single();
			seatBooking.StartDateTime.Date.Should().Be(startDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(endDate.Date);
			seatBooking.Seat.Should().Be(seat);
		}

		[Test]
		public void ShouldBookSeatBySeatsAndPeople()
		{
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(startDate, team)
			};
			var assignments = new[]
			{
				addAssignment(people[0], startDate, endDate),
				addAssignment(people[1], startDate, endDate),
				addAssignment(people[2], startDate, endDate)
			};

			var seatMapLocation = addLocation("location", null,
												new Seat("seat1", 1),
												new Seat("seat2", 2),
												new Seat("seat3", 3));


			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignments);

			seatPlanner.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				seatMapLocation.Seats.Select(seat => seat.Id.Value).ToList(),
				people.Select(person => person.Id.Value).ToList());

			_seatBookingRepository.CountAllEntities().Should().Be(3);
			var seatBookings = _seatBookingRepository.LoadAll();

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
			setup();
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var team = addTeam("Team");
			var teams = new[] { team };
			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(startDate, team),
				createPersonWithPersonPeriodFromTeam(endDate, team)
			};

			var personAssignment = addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, personAssignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[0].Id.Value }.ToList(),
				new[] { people[0].Id.Value }.ToList());


			_seatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should().Be(seatMapLocation.Seats.First());

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgentBySeatAndPerson()
		{
			setup();
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam (date, team);
			var assignment = addAssignment(person, date, date.AddHours(8));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2)
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats[1]);

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[1].Id.Value }.ToList(),
				new[] { person.Id.Value }.ToList());

			_seatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats[1]);
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriodIfGivingASeatAndPerson()
		{
			setup();
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var people = new[]
			{
				createPersonWithPersonPeriodFromTeam(date, team),
				createPersonWithPersonPeriodFromTeam(date, team)
			};

			var assignment = addAssignment(people[0], date.AddHours(3), date.AddHours(10));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], date.AddDays(-1), date.AddDays(-1).AddHours(21), date.AddHours(8), seatMapLocation.Seats[0]);

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				null,
				dateOnlyPeriod,
				new[] { seatMapLocation.Seats[0].Id.Value }.ToList(),
				new[] { people[1].Id.Value }.ToList());


			_seatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}


		[Test]
		public void ShouldReturnSeatPlanningResultInformation()
		{
			setup();
			testSeatPlanningResultInformation( false);
		}

		[Test]
		public void ShouldReturnSeatPlanningResultInformationBySeatsAndPeople()
		{
			setup();
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

			var assignmentPerson1 = addAssignment(people[0], startDate, startDate.AddHours(8));
			var assignmentPerson2 = addAssignment(people[1], startDate, startDate.AddHours(8));
			var assignmentPerson3 = addAssignment(people[2], endDate, endDate.AddHours(8));
			var assignment2Person2 = addAssignment(people[1], endDate.AddHours (9), endDate.AddHours(17));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat ("Seat One", 1)
			});

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, people, locations, assignmentPerson1, assignmentPerson2, assignmentPerson3, assignment2Person2);

			ISeatPlanningResult result;


			if (manuallyPlanSeatsAndPeople)
			{
				result = seatPlanner.Plan(
									getLocationGuids(locations),
									null,
									dateOnlyPeriod,
									seatMapLocation.Seats.Select(seat => seat.Id.Value).ToList(),
									people.Select(person => person.Id.Value).ToList());
			}
			else
			{
				result = seatPlanner.Plan(
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
			setup();
			var date = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam (date, team);
			var assignment = addAssignment(person, date, date.AddHours(8));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2)
			});

			var theDateBefore = date.AddDays (-1);
			addSeatBooking(person, theDateBefore, theDateBefore, theDateBefore.AddHours(10), seatMapLocation.Seats[1]);

			var locations = new[] { seatMapLocation };

			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod,
				null,
				null);

			_seatBookingRepository.CountAllEntities().Should().Be(2);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats[1]);
		}


		[Test]
		public void ShouldNotAllocateASeatToAnAgentWithADayOff()
		{
			setup();
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1)
			});

			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, _currentScenario.Current(), dateOnlyPeriod.StartDate, new DayOffTemplate(new Description("for", "test")));
			var locations = new[] { seatMapLocation };
			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.CountAllEntities().Should().Be(0);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Should().Be.Null();
		}


		[Test]
		public void ShouldRemoveExistingBookingForAgentWhenTheirScheduleChangesToDayOff()
		{
			setup();
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date));
			var team = addTeam("Team");
			var teams = new[] { team };
			var person = createPersonWithPersonPeriodFromTeam(date, team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1),
				new Seat("Seat Two",2),
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats.Last());

			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, _currentScenario.Current(), dateOnlyPeriod.StartDate, new DayOffTemplate(new Description("for", "test")));
			var locations = new[] { seatMapLocation };
			var seatPlanner = setupSeatPlanner(teams, new[] { person }, locations, assignment);

			seatPlanner.Plan(
				getLocationGuids(locations),
				getTeamGuids(teams),
				dateOnlyPeriod, null, null);

			_seatBookingRepository.CountAllEntities().Should().Be(0);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Should().Be.Null();
			
		}

	}
}