using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
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
		private FakeCurrentScenario _currentScenario;
		private FakeWriteSideRepository<ISeatBooking> _seatBookingRepository;

		[SetUp]
		public void SetUp()
		{
			_currentScenario = new FakeCurrentScenario();
			_seatBookingRepository = new FakeWriteSideRepository<ISeatBooking>();
		}

		[Test]
		public void ShouldBookSeat()
		{
			var dateTimePeriod = new DateTimePeriod(new DateTime(2015, 1, 20, 9, 0, 0).ToUniversalTime(), new DateTime(2015, 1, 20, 17, 0, 0).ToUniversalTime());
			var team = new Team() { Description = new Description("Team") };
			team.SetId(Guid.NewGuid());

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(dateTimePeriod.StartDateTime), team);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_currentScenario.Current(), person, dateTimePeriod);

			var note = new PublicNote(person, new DateOnly(dateTimePeriod.StartDateTime), _currentScenario.Current(), "Original Note");
			var publicNoteRepository = new FakePublicNoteRepository(note);
			
			var seatMapLocation = new SeatMapLocation() { Name = "Location" };
			seatMapLocation.SetId(Guid.NewGuid());
			seatMapLocation.AddSeat("Seat One", 1);
			
			var target = new AddSeatPlanCommandHandler(new FakeScheduleDataReadScheduleRepository(personAssignment), new FakeTeamRepository(team), new FakePersonRepository(person), _currentScenario, publicNoteRepository, new FakeSeatMapRepository(seatMapLocation), _seatBookingRepository);

			var command = new AddSeatPlanCommand()
			{
				StartDate = dateTimePeriod.StartDateTime,
				EndDate = dateTimePeriod.EndDateTime,
				Locations = new[] { seatMapLocation.Id.Value },
				Teams = new[] { team.Id.Value },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);
			var updatedNote = publicNoteRepository.Get(Guid.Empty);
			updatedNote.GetScheduleNote(new NoFormatting()).Should().Contain(seatMapLocation.Name);

			var seatBooking = _seatBookingRepository.Single() as SeatBooking;
			seatBooking.StartDateTime.Date.Should().Be(command.StartDate.Date);
			seatBooking.EndDateTime.Date.Should().Be(command.EndDate.Date);
			seatBooking.Seat.Should().Be(seatMapLocation.Seats.Single());
		}
		
	}
}