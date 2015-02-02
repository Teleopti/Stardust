using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatAllocator
	{
		private readonly Location[] _locations;

		public SeatAllocator(params Location[] locations)
		{
			_locations = locations;
		}

		public void AllocateSeats(params SeatBookingRequest[] seatBookingRequests)
		{
			foreach (var location in _locations)
			{
				location.ClearBookingInformation();
			}

			var sortedSeatBookingRequests = seatBookingRequests.OrderByDescending(s => s.MemberCount);
			bookSeatsByGroup(sortedSeatBookingRequests);
			bookUnallocatedShifts(sortedSeatBookingRequests);
		}

		private void bookSeatsByGroup(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests)
		{
			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				bookSeats(seatBookingRequest.AgentShifts, true);
			}
		}

		private void bookUnallocatedShifts(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests)
		{
			var unallocatedShifts = sortedSeatBookingRequests.SelectMany(s => s.AgentShifts).Where(s => s.Seat == null);
			if (unallocatedShifts.Any())
			{
				bookSeats(unallocatedShifts, false);
			}
		}

		private void bookSeats(IEnumerable<AgentShift> shifts, Boolean bookGroupedRequestsTogether)
		{
			foreach (var location in _locations.OrderByDescending(l => l.SeatCount))
			{
				var unallocatedShifts = shifts.Where(s => s.Seat == null);
				if (!unallocatedShifts.Any()) return;

				var targetLocation = bookGroupedRequestsTogether
					? location.GetLocationToAllocateSeats(unallocatedShifts)
					: location;

				if (targetLocation != null)
				{
					bookSeatsForLocation(bookGroupedRequestsTogether, unallocatedShifts, targetLocation);
				}
			}
		}

		private void bookSeatsForLocation(bool bookGroupedRequestsTogether, IEnumerable<AgentShift> unallocatedShifts, Location targetLocation)
		{
			foreach (var shift in unallocatedShifts)
			{
				var firstUnallocatedSeat = targetLocation.GetNextUnallocatedSeat(shift.Period, bookGroupedRequestsTogether);
				if (foundUnallocatedSet(firstUnallocatedSeat))
				{
					shift.Book(firstUnallocatedSeat);
				}
			}
		}

		private bool foundUnallocatedSet(Seat firstUnallocatedSeat)
		{
			return firstUnallocatedSeat != null;
		}
	}
}