using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	internal class AddSeatPlanCommandHandlerTest
	{
		private ICurrentScenario _currentScenario;
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatPlanRepository _seatPlanRepository;
		[SetUp]
		public void SetUp()
		{
			_currentScenario = new FakeCurrentScenario();
			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatPlanRepository = new FakeSeatPlanRepository();
		}

		#region helper methods

		private IPersonAssignment addAssignment(IPerson person, DateTime startDate, DateTime endDate)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				_currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
		}

		private AddSeatPlanCommandHandler setupHandler(ITeam[] teams, IEnumerable<IPerson> people, IEnumerable<ISeatMapLocation> seatMapLocations, params IPersonAssignment[] personAssignment)
		{
			var seatBookingRequestAssembler = new SeatBookingRequestAssembler(
				new FakeScheduleDataReadScheduleRepository(personAssignment), _seatBookingRepository, _currentScenario);

			var seatPlanner = new SeatPlanner(new FakePersonRepository(people.ToArray()), seatBookingRequestAssembler,
				new SeatPlanPersister(_seatBookingRepository, _seatPlanRepository));

			return new AddSeatPlanCommandHandler(new FakeTeamRepository(teams), new FakeSeatMapRepository(seatMapLocations.ToArray()), seatPlanner);
		}

		private static AddSeatPlanCommand addSeatPlanCommand(DateTime startDate, DateTime endDate, IEnumerable<Guid> locations, IEnumerable<Guid> seatIds, IEnumerable<Guid> personIds)
		{
			var command = createSeatPlanCommand(startDate, endDate);
			command.SeatIds = seatIds.ToList();
			command.PersonIds = personIds.ToList();
			command.Locations = locations.ToList();
			return command;
		}

		private static AddSeatPlanCommand addSeatPlanCommand(DateTime startDate, DateTime endDate, IEnumerable<SeatMapLocation> locations, IEnumerable<Team> teams)
		{
			if (locations == null) throw new ArgumentNullException("locations");
			var command = createSeatPlanCommand(startDate, endDate);
			command.Locations = locations.Select(location => location.Id.Value).ToList();
			command.Teams = teams.Select(team => team.Id.Value).ToList();
			return command;
		}

		private static AddSeatPlanCommand createSeatPlanCommand(DateTime startDate, DateTime endDate)
		{
			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,

				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			return command;
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
			var team = new Team() { Description = new Description(name) };
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
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignment);

			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.StartDateTime.Date.Should().Be(command.StartDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(command.EndDate.Date);
			seatBooking.Seat.Should().Be(seatMapLocation.Seats.Single());
		}

		[Test]
		public void ShouldBookSeatForMutipleDays()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 22, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team 1");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);

			var personAssignments = new List<IPersonAssignment>()
			{
				addAssignment (person, startDate, startDate.AddHours (8)),
				addAssignment (person, startDate.AddDays (1), startDate.AddDays (1).AddHours (8)),
				addAssignment (person, endDate, endDate.AddHours (8))
			};

			var seatMapLocation = addLocation("Location", null, new Seat("Seat 1", 1));
			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignments.ToArray());

			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			Assert.IsTrue(_seatBookingRepository.CountAllEntities() == 3);
		}

		[Test]
		public void ShouldSkipNonSelectedLocations()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var childLocations = new[]
			{
				addLocation ("Location1",null, new Seat("Seat One", 1)),
				addLocation ("Location2",null, new Seat("Seat One", 1))

			};

			var rootLocation = addLocation("Root Location", childLocations);
			var target = setupHandler(new[] { team }, new[] { person }, new[] { rootLocation, childLocations[0], childLocations[1] }, personAssignment);

			var command = addSeatPlanCommand(startDate, endDate, new[] { childLocations[1] }, new[] { team });

			target.Handle(command);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.Seat.Should().Be(childLocations[1].Seats.Single());
		}

		[Test]
		public void ShouldGroupTeamBookings()
		{

			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var dateOnly = new DateOnly(startDate);
			var teams = new[]
			{
				addTeam ("Team"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[0])
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

			var target = setupHandler(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] }, assignments);


			var command = addSeatPlanCommand(startDate, endDate, childLocations, teams);

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, people[0]).Seat.Should().Be(childLocations[1].Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, people[1]).Seat.Should().Be(childLocations[1].Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, people[2]).Seat.Should().Be(childLocations[0].Seats[0]);
		}

		[Test]
		public void ShouldGroupManyTeamBookings()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var startDateOnly = new DateOnly(startDate);

			var teams = new[]
			{
				addTeam ("Team1"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[0]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[0]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), teams[1]),
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

			var target = setupHandler(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] }, personAssignments);
			var command = addSeatPlanCommand(startDate, startDate, childLocations, teams);


			target.Handle(command);

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
			var startDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var startDateDay2 = startDateDay1.AddDays(1);
			var endDateDay2 = startDateDay2.AddDays(1);

			var startDateOnlyDay1 = new DateOnly(startDateDay1);
			var startDateOnlyDay2 = new DateOnly(startDateDay2);
			var teams = new[]
			{
				addTeam ("Team"),
				addTeam ("Team2")
			};

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[0]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[0]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[1]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDateOnlyDay1, teams[1])
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

			var target = setupHandler(teams, people, new[] { rootLocation, childLocations[0], childLocations[1] }, assignments);
			var command = addSeatPlanCommand(startDateDay1, endDateDay2, childLocations, teams);

			target.Handle(command);

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
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(endDate), team)
			};

			var personAssignment = addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, personAssignment);
			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should()
				.Be(seatMapLocation.Seats.First());
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldHonourExistingBookingsOnChildLocation()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team)
			};

			var childLocation = addLocation("ChildLocation", null, new Seat("Seat 1", 1));
			var rootLocation = addLocation("RootLocation", new[] { childLocation }, null);

			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);


			addSeatBooking(people[0], startDate, startDate, assignmentEndDateTime, childLocation.Seats[0]);

			var assignment = addAssignment(people[1], startDate, assignmentEndDateTime);
			var target = setupHandler(new[] { team }, people, new[] { rootLocation, childLocation }, assignment);

			var command = addSeatPlanCommand(startDate, endDate, new[] { childLocation }, new[] { team });

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0])
				.Seat.Should().Be(childLocation.Seats[0]);

			Assert.IsNull(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[1]));
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgent()
		{
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2), 
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats.Last());

			var assignment = addAssignment(person, date, date.AddHours(8));
			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, assignment);

			var command = addSeatPlanCommand(date, date, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			_seatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats.First());
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriod()
		{
			var date = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");

			var assignmentEndDateTime = new DateTime(date.Year, date.Month, date.Day, 9, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team)
			};

			var assignment = addAssignment(people[0], assignmentEndDateTime.AddHours(-6), assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var bookingStartDate = new DateTime(date.Year, date.Month, date.Day - 1, 21, 00, 00, DateTimeKind.Utc);
			addSeatBooking(people[1], bookingStartDate, bookingStartDate, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, assignment);
			var command = addSeatPlanCommand(date, date, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}

		[Test]
		public void ShouldAddAfterOvernightBooking()
		{
			var date = new DateTime(2015, 1, 20, 9, 1, 0, DateTimeKind.Utc);
			var team = addTeam("Team");

			var assignmentStartDateTime = new DateTime(2015, 1, 19, 21, 00, 00, DateTimeKind.Utc);
			var assignmentEndDateTime = new DateTime(2015, 1, 20, 9, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team)
			};

			var assignment = addAssignment(people[0], date, date.AddHours(8));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], assignmentStartDateTime, assignmentEndDateTime, assignmentEndDateTime, seatMapLocation.Seats[0]);

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, assignment);
			var command = addSeatPlanCommand(date, date, new[] { seatMapLocation }, new[] { team });

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
		}

		[Test]
		public void BookingsOfAnEarlierTimeShouldGetPrecedence()
		{
			var teams = new[]
			{
				addTeam ("Team 1"),
				addTeam ("Team 2")
			};

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2015, 01, 01), teams[0]),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2015, 01, 01), teams[1]),
			};

			var assignments = new[]
			{
				addAssignment (people[0],new DateTime(2015, 1, 21, 11, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 17, 0, 0, DateTimeKind.Utc)),
				addAssignment (people[1],new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 12, 0, 0, DateTimeKind.Utc)),
			};

			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));
			var target = setupHandler(teams, people, new[] { seatMapLocation }, assignments);
			var command = addSeatPlanCommand(new DateTime(2015, 01, 21), new DateTime(2015, 01, 21), new[] { seatMapLocation }, teams);

			target.Handle(command);

			Assert.That(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(2015, 1, 21), people[0]) == null);
			Assert.That(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(2015, 1, 21), people[1]) != null);
		}

		[Test]
		public void ShouldAllocateSeatsInOrderWhenPlanningOvernightEvenWhenBookingsExist()
		{
			var date = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team)
			};

			var assignments = new[]
			{
				addAssignment (people[0], new DateTime(2015, 1, 20, 1, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 20, 9, 30, 0, DateTimeKind.Utc)),
				addAssignment (people[1], new DateTime(2015, 1, 20, 10, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 20, 18, 30, 0, DateTimeKind.Utc)),
				addAssignment (people[2], new DateTime(2015, 1, 20, 19, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 1, 30, 0, DateTimeKind.Utc)),

				addAssignment (people[0], new DateTime(2015, 1, 21, 1, 0, 0, DateTimeKind.Utc),new DateTime(2015, 1, 21, 9, 30, 0, DateTimeKind.Utc)),
			};

			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, assignments);
			var command = addSeatPlanCommand(date, date.AddDays(1), new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]).Seat.Should().Be(seatMapLocation.Seats[0]);

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date)).Count() == 3);
			Assert.IsFalse(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date).AddDays(1)).Any());

			// check overwrite gets same result
			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]).Seat.Should().Be(seatMapLocation.Seats[0]);

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date)).Count() == 3);
			Assert.IsTrue(!_seatBookingRepository.LoadSeatBookingsForDay(new DateOnly(date).AddDays(1)).Any());

		}


		[Test]
		public void ShouldPersistSeatPlans()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, startDate.AddHours(8));
			var personAssignment2 = addAssignment(person, endDate, endDate.AddHours(8));

			var note = new PublicNote(person, new DateOnly(startDate), _currentScenario.Current(), "Original Note");
			var publicNoteRepository = new FakePublicNoteRepository(note);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignment, personAssignment2);

			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			var seatPlanDayOne = _seatPlanRepository.First();
			var seatPlanDayTwo = _seatPlanRepository.Last();
			seatPlanDayOne.Date.Date.Should().Be(command.StartDate.Date);
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Date.Date.Should().Be(command.EndDate.Date);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.Ok);

		}

		[Test]
		public void ShouldUpdateExistingSeatPlan()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, startDate.AddHours(8));

			var note = new PublicNote(person, new DateOnly(startDate), _currentScenario.Current(), "Original Note");
			var publicNoteRepository = new FakePublicNoteRepository(note);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			_seatPlanRepository.Add(new SeatPlan()
			{
				Date = new DateOnly(startDate.Date),
				Status = SeatPlanStatus.InError

			});

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignment);

			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			var seatPlan = _seatPlanRepository.Single();

			seatPlan.Date.Date.Should().Be(command.StartDate.Date);
			seatPlan.Status.Should().Be(SeatPlanStatus.Ok);

		}

		[Test]
		public void ShouldPersistSeatPlanWithErrorStatus()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var publicNoteRepository = new FakePublicNoteRepository();
			var seatMapLocation = addLocation("Location", null, null);

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignment);

			var command = addSeatPlanCommand(startDate, endDate, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			var seatPlanDayOne = _seatPlanRepository.First();
			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.InError);

		}

		[Test]
		public void ShouldPersistSeatPlanWithDifferentStatusInsidePlanningPeriod()
		{
			var startDate1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate1 = new DateTime(2015, 1, 20, 8, 0, 0, DateTimeKind.Utc);
			var startDate2 = new DateTime(2015, 1, 21, 0, 0, 0, DateTimeKind.Utc);
			var endDate2 = new DateTime(2015, 1, 21, 8, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate1), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate1), team);

			var personAssignment = addAssignment(person, startDate1, endDate1);
			var personAssignmentPerson2 = addAssignment(person2, startDate2, endDate2);
			var personAssignment2 = addAssignment(person, startDate2, endDate2);


			var publicNoteRepository = new FakePublicNoteRepository();
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			var target = setupHandler(new[] { team }, new[] { person, person2 }, new[] { seatMapLocation }, new[] { personAssignment, personAssignmentPerson2, personAssignment2 });

			var command = addSeatPlanCommand(startDate1, endDate2, new[] { seatMapLocation }, new[] { team });
			target.Handle(command);

			var seatPlanDayOne = _seatPlanRepository.First();
			var seatPlanDayTwo = _seatPlanRepository.Last();

			_seatPlanRepository.Count().Should().Be(2);

			seatPlanDayOne.Status.Should().Be(SeatPlanStatus.Ok);
			seatPlanDayTwo.Status.Should().Be(SeatPlanStatus.InError);
		}



		[Test]
		public void ShouldBookSeatBySeatAndPerson()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = addAssignment(person, startDate, endDate);

			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetId(Guid.NewGuid());
			var seat = seatMapLocation.AddSeat("seat", 1);

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, personAssignment);

			var command = addSeatPlanCommand(startDate, endDate,
												new[] { seatMapLocation.Id.Value },
												new[] { seat.Id.Value },
												new[] { person.Id.Value });

			target.Handle(command);

			var seatBooking = _seatBookingRepository.Single();
			seatBooking.StartDateTime.Date.Should().Be(command.StartDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(command.EndDate.Date);
			seatBooking.Seat.Should().Be(seat);
		}

		[Test]
		public void ShouldBookSeatBySeatsAndPeople()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team)
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

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, assignments);

			var command = addSeatPlanCommand(startDate,
											endDate,
											new[] { seatMapLocation.Id.Value },
											seatMapLocation.Seats.Select(seat => seat.Id.Value),
											people.Select(person => person.Id.Value));


			target.Handle(command);

			_seatBookingRepository.CountAllEntities().Should().Be(3);
			var seatBookings = _seatBookingRepository.LoadAll();

			foreach (var seatBooking in seatBookings)
			{
				seatBooking.StartDateTime.Date.Should().Be(command.StartDate.Date);
				seatBooking.EndDateTime.Date.Should().Be(command.EndDate.Date);
				seatBooking.Seat.Should().Not.Be(null);
			}

		}

		[Test]
		public void ShouldHonourExistingBookingsForOtherAgentsBySeatAndPerson()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = addTeam("Team");
			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);

			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(endDate), team)
			};

			var personAssignment = addAssignment(people[0], startDate, assignmentEndDateTime);
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], startDate, startDate, assignmentEndDateTime, seatMapLocation.Seats.Single());

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, personAssignment);
			var command = addSeatPlanCommand(startDate, endDate,
												new[] { seatMapLocation.Id.Value },
												new[] { seatMapLocation.Seats[0].Id.Value },
												new[] { people[0].Id.Value });

			target.Handle(command);


			_seatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(startDate), people[1])
				.Seat.Should().Be(seatMapLocation.Seats.First());

			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), people[0]) == null);
		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgentBySeatAndPerson()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var assignment = addAssignment(person, date, date.AddHours(8));

			var seatMapLocation = addLocation("Location", null, new[]
			{
				new Seat("Seat One",1), 
				new Seat("Seat Two",2)
			});

			addSeatBooking(person, date, date, date.AddHours(10), seatMapLocation.Seats[1]);

			var target = setupHandler(new[] { team }, new[] { person }, new[] { seatMapLocation }, assignment);

			var command = addSeatPlanCommand(date, date,
											new[] { seatMapLocation.Id.Value },
											new[] { seatMapLocation.Seats[1].Id.Value },
											new[] { person.Id.Value });

			target.Handle(command);

			_seatBookingRepository.CountAllEntities().Should().Be(1);
			var booking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person);
			booking.Seat.Should().Be(seatMapLocation.Seats[1]);
			booking.EndDateTime.Should().Be(date.AddHours(8));
		}

		[Test]
		public void ShouldHonourExistingOvernightBookingOutsideOfCommandPeriodIfGivingASeatAndPerson()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var team = addTeam("Team");
			var people = new[]
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team),
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team)
			};

			var assignment = addAssignment(people[0], date.AddHours(3), date.AddHours(10));
			var seatMapLocation = addLocation("Location", null, new Seat("Seat One", 1));

			addSeatBooking(people[1], date.AddDays(-1), date.AddDays(-1).AddHours(21), date.AddHours(8), seatMapLocation.Seats[0]);

			var target = setupHandler(new[] { team }, people, new[] { seatMapLocation }, assignment);
			var command = addSeatPlanCommand(date,
											date,
											new[] { seatMapLocation.Id.Value },
											new[] { seatMapLocation.Seats[0].Id.Value },
											new[] { people[1].Id.Value });
			target.Handle(command);

			_seatBookingRepository
				.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), people[1])
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), people[0]) == null);
		}


	}
}