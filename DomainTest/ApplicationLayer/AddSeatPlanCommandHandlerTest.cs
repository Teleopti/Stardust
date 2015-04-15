using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
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

		[SetUp]
		public void SetUp()
		{
			_currentScenario = new FakeCurrentScenario();
			_seatBookingRepository = new FakeSeatBookingRepository();
		}

		[Test]
		public void ShouldBookSeat()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				_currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));

			var note = new PublicNote(person, new DateOnly(startDate), _currentScenario.Current(), "Original Note");
			var publicNoteRepository = new FakePublicNoteRepository(note);

			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person), _currentScenario, publicNoteRepository,
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);
			var updatedNote = publicNoteRepository.Single();
			updatedNote.GetScheduleNote(new NoFormatting()).Should().Contain(seatMapLocation.Name);

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

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDate, startDate.AddHours(8)));

			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDate.AddDays(1), startDate.AddDays(1).AddHours(8)));

			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(endDate, endDate.AddHours(8)));

			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var target = new AddSeatPlanCommandHandler(
				new FakeScheduleDataReadScheduleRepository(personAssignment, personAssignment2, personAssignment3),
				new FakeTeamRepository(team), new FakePersonRepository(person), _currentScenario, new FakePublicNoteRepository(),
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			Assert.IsTrue(_seatBookingRepository.CountAllEntities() == 3);
		}

		[Test]
		public void ShouldSkipNonSelectedLocations()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				_currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));

			var rootLocation = new SeatMapLocation() { Name = "RootLocation" };
			rootLocation.SetId(Guid.NewGuid());

			var seatMapLocation = new SeatMapLocation() { Name = "Location1" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var seatMapLocation2 = new SeatMapLocation() { Name = "Location2" };
			seatMapLocation2.SetId(Guid.NewGuid());
			seatMapLocation2.AddSeat("Seat One", 1);

			rootLocation.AddChildren(new[] { seatMapLocation, seatMapLocation2 });

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person), _currentScenario, new FakePublicNoteRepository(),
				new FakeSeatMapRepository(rootLocation, seatMapLocation, seatMapLocation2), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,
				Locations = new[] { seatMapLocation2.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.Seat.Should().Be(seatMapLocation2.Seats.Single());
		}

		[Test]
		public void ShouldGroupTeamBookings()
		{

			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var dateOnly = new DateOnly(startDate);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var team2 = new Team() { Description = new Description("Team2") };
			team2.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team2);
			var person3 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team2);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDate, endDate));
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person2,
				new DateTimePeriod(startDate, endDate));
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person3,
				new DateTimePeriod(startDate, endDate));

			var rootLocation = new SeatMapLocation() { Name = "RootLocation" };
			rootLocation.SetId(Guid.NewGuid());

			var seatMapLocation = new SeatMapLocation() { Name = "Location1" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);


			var seatMapLocation2 = new SeatMapLocation() { Name = "Location2" };
			seatMapLocation2.SetId(Guid.NewGuid());
			seatMapLocation2.AddSeat("Seat One", 1);
			seatMapLocation.AddSeat("Seat Two", 2);

			rootLocation.AddChildren(new[] { seatMapLocation, seatMapLocation2 });

			var personRepository = new FakePersonRepository(person, person2, person3);
			var scheduleRepository = new FakeScheduleDataReadScheduleRepository(personAssignment, personAssignment2,
				personAssignment3);
			var seatMapRepository = new FakeSeatMapRepository(rootLocation, seatMapLocation, seatMapLocation2);

			var target = new AddSeatPlanCommandHandler(scheduleRepository, new FakeTeamRepository(team, team2),
				personRepository,
				_currentScenario, new FakePublicNoteRepository(), seatMapRepository, _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,
				Locations = new[] { seatMapLocation.Id.Value, seatMapLocation2.Id.Value },
				Teams = new[] { team.Id.Value, team2.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, person).Seat.Should().Be(seatMapLocation2.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, person2).Seat.Should().Be(seatMapLocation.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(dateOnly, person3).Seat.Should().Be(seatMapLocation.Seats[1]);
		}


		[Test]
		public void ShouldGroupManyTeamBookings()
		{

			var startDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDateDay1 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var startDateOnlyDay1 = new DateOnly(startDateDay1);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var team2 = new Team() { Description = new Description("Team2") };
			team2.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team);
			var person3 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person4 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person5 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person6 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);

			var assignmentPerson1Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson2Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person2, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson3Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person3, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson4Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person4, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson5Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person5, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson6Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person6, new DateTimePeriod(startDateDay1, endDateDay1));

			var rootLocation = new SeatMapLocation() { Name = "RootLocation" };
			rootLocation.SetId(Guid.NewGuid());

			var seatMapLocation = new SeatMapLocation() { Name = "Location1" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Location 1, Seat 1", 1);
			seatMapLocation.AddSeat("Location 1, Seat 2", 2);
			seatMapLocation.AddSeat("Location 1, Seat 3", 3);
			seatMapLocation.AddSeat("Location 1, Seat 4", 4);

			var seatMapLocation2 = new SeatMapLocation() { Name = "Location2" };
			seatMapLocation2.SetId(Guid.NewGuid());
			seatMapLocation2.AddSeat("Location 2, Seat 1", 1);
			seatMapLocation2.AddSeat("Location 2, Seat 2", 2);

			rootLocation.AddChildren(new[] { seatMapLocation, seatMapLocation2 });

			var personRepository = new FakePersonRepository(person, person2, person3, person4, person5, person6);
			var scheduleRepository = new FakeScheduleDataReadScheduleRepository(
				assignmentPerson1Day1, assignmentPerson2Day1, assignmentPerson3Day1, assignmentPerson4Day1, assignmentPerson5Day1,
				assignmentPerson6Day1
				);
			var seatMapRepository = new FakeSeatMapRepository(rootLocation, seatMapLocation, seatMapLocation2);

			var target = new AddSeatPlanCommandHandler(scheduleRepository, new FakeTeamRepository(team, team2),
				personRepository,
				_currentScenario, new FakePublicNoteRepository(), seatMapRepository, _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDateDay1,
				EndDate = startDateDay1,
				Locations = new[] { seatMapLocation.Id.Value, seatMapLocation2.Id.Value },
				Teams = new[] { team.Id.Value, team2.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person)
				.Seat.Should()
				.Be(seatMapLocation2.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person2)
				.Seat.Should()
				.Be(seatMapLocation2.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person3)
				.Seat.Should()
				.Be(seatMapLocation.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person4)
				.Seat.Should()
				.Be(seatMapLocation.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person5)
				.Seat.Should()
				.Be(seatMapLocation.Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person6)
				.Seat.Should()
				.Be(seatMapLocation.Seats[3]);

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

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var team2 = new Team() { Description = new Description("Team2") };
			team2.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team);
			var person3 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person4 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person5 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);
			var person6 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDateDay1), team2);

			var assignmentPerson1Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson2Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person2, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson3Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person3, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson4Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person4, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson5Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person5, new DateTimePeriod(startDateDay1, endDateDay1));
			var assignmentPerson6Day1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person6, new DateTimePeriod(startDateDay1, endDateDay1));

			var assignmentPerson1Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person,
				new DateTimePeriod(startDateDay2, endDateDay2));
			var assignmentPerson2Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person2, new DateTimePeriod(startDateDay2, endDateDay2));
			var assignmentPerson3Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person3, new DateTimePeriod(startDateDay2, endDateDay2));
			var assignmentPerson4Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person4, new DateTimePeriod(startDateDay2, endDateDay2));
			var assignmentPerson5Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person5, new DateTimePeriod(startDateDay2, endDateDay2));
			var assignmentPerson6Day2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person6, new DateTimePeriod(startDateDay2, endDateDay2));

			var rootLocation = new SeatMapLocation() { Name = "RootLocation" };
			rootLocation.SetId(Guid.NewGuid());

			var seatMapLocation = new SeatMapLocation() { Name = "Location1" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Location 1, Seat 1", 1);
			seatMapLocation.AddSeat("Location 1, Seat 2", 2);
			seatMapLocation.AddSeat("Location 1, Seat 3", 3);
			seatMapLocation.AddSeat("Location 1, Seat 4", 4);

			var seatMapLocation2 = new SeatMapLocation() { Name = "Location2" };
			seatMapLocation2.SetId(Guid.NewGuid());
			seatMapLocation2.AddSeat("Location 2, Seat 1", 1);
			seatMapLocation2.AddSeat("Location 2, Seat 2", 2);

			rootLocation.AddChildren(new[] { seatMapLocation, seatMapLocation2 });

			var personRepository = new FakePersonRepository(person, person2, person3, person4, person5, person6);
			var scheduleRepository = new FakeScheduleDataReadScheduleRepository(
				assignmentPerson1Day1, assignmentPerson2Day1, assignmentPerson3Day1, assignmentPerson4Day1, assignmentPerson5Day1,
				assignmentPerson6Day1,
				assignmentPerson1Day2, assignmentPerson2Day2, assignmentPerson3Day2, assignmentPerson4Day2, assignmentPerson5Day2,
				assignmentPerson6Day2
				);
			var seatMapRepository = new FakeSeatMapRepository(rootLocation, seatMapLocation, seatMapLocation2);

			var target = new AddSeatPlanCommandHandler(scheduleRepository, new FakeTeamRepository(team, team2),
				personRepository,
				_currentScenario, new FakePublicNoteRepository(), seatMapRepository, _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDateDay1,
				EndDate = endDateDay2,
				Locations = new[] { seatMapLocation.Id.Value, seatMapLocation2.Id.Value },
				Teams = new[] { team.Id.Value, team2.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person).Seat.Should().Be(seatMapLocation2.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person2).Seat.Should().Be(seatMapLocation2.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person3).Seat.Should().Be(seatMapLocation.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person4).Seat.Should().Be(seatMapLocation.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person5).Seat.Should().Be(seatMapLocation.Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay1, person6).Seat.Should().Be(seatMapLocation.Seats[3]);

			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person).Seat.Should().Be(seatMapLocation2.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person2).Seat.Should().Be(seatMapLocation2.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person3).Seat.Should().Be(seatMapLocation.Seats[0]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person4).Seat.Should().Be(seatMapLocation.Seats[1]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person5).Seat.Should().Be(seatMapLocation.Seats[2]);
			_seatBookingRepository.LoadSeatBookingForPerson(startDateOnlyDay2, person6).Seat.Should().Be(seatMapLocation.Seats[3]);
		}


		[Test]
		public void ShouldHonourExistingBookingsForOtherAgents()
		{
			var startDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var assignmentEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 13, 00, 00, DateTimeKind.Utc);

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(startDate), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(endDate), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person, new DateTimePeriod(
					startDate,
					assignmentEndDateTime));

			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var existingSeatBooking = new SeatBooking(person2, startDate, assignmentEndDateTime)
			{
				Seat = seatMapLocation.Seats.Single()
			};
			existingSeatBooking.SetId(Guid.NewGuid());
			_seatBookingRepository.Add(existingSeatBooking);

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person, person2), _currentScenario,
				new FakePublicNoteRepository(),
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = startDate,
				EndDate = endDate,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), person2)
				.Seat.Should()
				.Be(seatMapLocation.Seats.First());
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(startDate), person) == null);

		}

		[Test]
		public void ShouldOverwriteExistingBookingForAgent()
		{
			var date = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);
			seatMapLocation.AddSeat("Seat Two", 2);

			var existingSeatBooking = new SeatBooking(person, date, date.AddHours(10))
			{
				Seat = seatMapLocation.Seats.Last()
			};
			existingSeatBooking.SetId(Guid.NewGuid());
			_seatBookingRepository.Add(existingSeatBooking);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person, new DateTimePeriod(
					date,
					date.AddHours(8)));

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person), _currentScenario, new FakePublicNoteRepository(),
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = date,
				EndDate = date,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

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

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var assignmentStartDateTime = new DateTime(date.Year, date.Month, date.Day - 1, 21, 00, 00, DateTimeKind.Utc);
			var assignmentEndDateTime = new DateTime(date.Year, date.Month, date.Day, 9, 00, 00, DateTimeKind.Utc);

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person, new DateTimePeriod(
					assignmentEndDateTime.AddHours(-6),
					assignmentEndDateTime));

			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var existingSeatBooking = new SeatBooking(person2, assignmentStartDateTime, assignmentEndDateTime)
			{
				Seat = seatMapLocation.Seats.Single()
			};
			existingSeatBooking.SetId(Guid.NewGuid());
			_seatBookingRepository.Add(existingSeatBooking);

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person, person2), _currentScenario,
				new FakePublicNoteRepository(),
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = date,
				EndDate = date,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), person2)
				.Seat.Should().Be(seatMapLocation.Seats[0]);
			Assert.IsTrue(_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person) == null);

		}
		
		[Test]
		public void ShouldAddAfterOvernightBooking()
		{
			var date = new DateTime(2015, 1, 20, 9, 1, 0, DateTimeKind.Utc);

			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var assignmentStartDateTime = new DateTime(2015, 1, 19, 21, 00, 00, DateTimeKind.Utc);
			var assignmentEndDateTime = new DateTime(2015, 1, 20,  9, 00, 00, DateTimeKind.Utc);

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(date), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(),
				person, new DateTimePeriod(
					date,
					date.AddHours (8)));

			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);

			var existingSeatBooking = new SeatBooking(person2, assignmentStartDateTime, assignmentEndDateTime)
			{
				Seat = seatMapLocation.Seats.Single()
			};
			existingSeatBooking.SetId(Guid.NewGuid());
			_seatBookingRepository.Add(existingSeatBooking);

			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment),
				new FakeTeamRepository(team), new FakePersonRepository(person, person2), _currentScenario,
				new FakePublicNoteRepository(),
				new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = date,
				EndDate = date,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date.AddDays(-1)), person2)
				.Seat.Should().Be(seatMapLocation.Seats[0]);

			_seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(date), person)
				.Seat.Should().Be(seatMapLocation.Seats[0]);
		}

	}
}