using NUnit.Framework;

namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	public class SeatLevelAllocatorTests
	{
		[Test]
		public void ShouldAllocateAnAgentToASeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority(true);
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeatEach()
		{

			CommonSeatAllocatorTests.ShouldAllocateTwoAgentsToOneSeatEach(true);
		}

		[Test]
		public void ShouldAllocateTwoAgentsToOneSeat()
		{

			CommonSeatAllocatorTests.ShouldAllocateTwoAgentsSequentiallyToOneSeat(true);
		}

		[Test]
		public void ShouldAllocateAccordingToPriority()
		{
			CommonSeatAllocatorTests.ShouldAllocateAccordingToPriority(true);
		}


		[Test, Ignore]
		public void ShouldAllocateSeatsByEarliestFirst()
		{
			CommonSeatAllocatorTests.ShouldAllocateSeatsByEarliestFirst(true);
		}

		[Test, Ignore]
		public void ShouldNotAllocateTwoAgentsSequentiallyToOneSeat()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateTwoAgentsSequentiallyToOneSeat(true);
		}

		[Test]
		public void ShouldAllocateAccordingToStartTime()
		{
			CommonSeatAllocatorTests.ShouldAllocateAccordingToStartTime (true);
		}


		[Test, Ignore]
		public void ShouldAllocateToGroupFirst()
		{
			CommonSeatAllocatorTests.ShouldAllocateToGroupFirst(true);

		}

		[Test]
		public void ShouldNotAllocateAnAgentToAnAlreadyBookedSeat()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateAnAgentToAnAlreadyBookedSeat(true);

		}

		[Test]
		public void ShouldAllocateToAvailableSeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateToAvailableSeat(true);

		}

		[Test]
		public void ShouldNotAllocateAnAgentToASeatWhereRolesDoNotMatch()
		{
			CommonSeatAllocatorTests.ShouldNotAllocateAnAgentToASeatWhereRolesDoNotMatch(true);
		}

		[Test]
		public void ShouldAllocateAnAgentToASeatWhereRolesMatch()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereRolesMatch (true);

		}

		[Test]
		public void ShouldAllocateAnAgentToASeatWhereAllRolesMatch()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereAllRolesMatch (true);
		}


		[Test]
		public void ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority()
		{
			CommonSeatAllocatorTests.ShouldAllocateAnAgentToASeatWhereRolesMatchBySeatPriority (true);
		}

		[Test]
		public void ShouldAllocateAgentToMostPreviouslyOccupiedSeatWhenRolesAreSame()
		{
			CommonSeatAllocatorTests.ShouldAllocateAgentToMostPreviouslyOccupiedSeatWhenRolesAreSame(true);
		}

		[Test]
		public void ShouldGroupAgentsAroundSeatBookingWithHighestRoleMatchCount()
		{
			CommonSeatAllocatorTests.ShouldGroupAgentsAroundSeatBookingWithHighestRoleCount(true);
		}

		[Test]
		public void ShouldAllocateAgentToPreviouslyOccupiedSeat()
		{
			CommonSeatAllocatorTests.ShouldAllocateAgentToMostPreviouslyOccupiedSeat(true);
		}

	}

}