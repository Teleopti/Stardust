using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatAllocator
	{

		private readonly SeatMapLocation[] _seatMapLocations;

		public SeatAllocator(params SeatMapLocation[] seatMapLocations)
		{
			_seatMapLocations = seatMapLocations;
		}

		public void AllocateSeats(params SeatBookingRequest[] seatBookingRequests)
		{
			var sortedSeatBookingRequests = SeatAllocatorHelper.SortSeatBookingRequests(seatBookingRequests);

			bookSeatsByGroup(sortedSeatBookingRequests);
			bookUnallocatedShifts(sortedSeatBookingRequests);
		}

		private void bookSeatsByGroup(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests)
		{
			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				bookSeats(seatBookingRequest.SeatBookings, true);
			}
		}

		private void bookUnallocatedShifts(IEnumerable<SeatBookingRequest> sortedSeatBookingRequests)
		{
			var unallocatedShifts = sortedSeatBookingRequests.SelectMany(s => s.SeatBookings);
			if (unallocatedShifts.Any())
			{
				bookSeats(unallocatedShifts, false);
			}
		}

		private void bookSeats(IEnumerable<ISeatBooking> seatBookings, Boolean bookGroupedRequestsTogether)
		{
			var unallocatedBookings = seatBookings.Where(s => s.Seat == null).OrderBy(s => s.StartDateTime).ToArray();
			if (!unallocatedBookings.Any()) return;

			var allLocationsUnsorted = new List<SeatMapLocation>();

			foreach (var location in _seatMapLocations)
			{
				allLocationsUnsorted.AddRange(location.GetFullLocationHierachyAsList());
			}

			if (bookGroupedRequestsTogether)
			{
				var allLocationsSortedByBookingOrder = SeatAllocatorHelper.GetLocationsInOrderOfBookingOrder(allLocationsUnsorted, unallocatedBookings).ToList();

				foreach (var location in allLocationsSortedByBookingOrder.Where(location => location.CanAllocateShifts(unallocatedBookings)))
				{
					bookSeatsForLocation( unallocatedBookings, location);
					break;
				}
			}
			else
			{
				foreach (var booking in unallocatedBookings)
				{
					var allLocationsSortedByBookingOrder = SeatAllocatorHelper.GetLocationsInOrderOfBookingOrder(allLocationsUnsorted, booking).ToList();
					foreach (var location in allLocationsSortedByBookingOrder.Where(location => location.CanAllocateShifts(booking)))
					{
						bookSeatForLocation( location, booking);
						break;
					}

				}
			}
		}
		
		private void bookSeatsForLocation(IEnumerable<ISeatBooking> unallocatedShifts, ISeatMapLocation targetSeatMapLocation)
		{
			foreach (var shift in unallocatedShifts)
			{
				bookSeatForLocation(targetSeatMapLocation, shift);
			}
		}

		private void bookSeatForLocation (ISeatMapLocation targetSeatMapLocation, ISeatBooking shift)
		{
			var firstUnallocatedSeat = targetSeatMapLocation.GetNextUnallocatedSeat (shift);
			if (foundUnallocatedSet (firstUnallocatedSeat))
			{
				shift.Book (firstUnallocatedSeat);
			}
		}

		private bool foundUnallocatedSet(ISeat firstUnallocatedSeat)
		{
			return firstUnallocatedSeat != null;
		}
	}
}