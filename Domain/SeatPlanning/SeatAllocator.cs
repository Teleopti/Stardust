﻿using System;
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
			var sortedSeatBookingRequests = seatBookingRequests.OrderByDescending(s => s.MemberCount);
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
			var unallocatedShifts = sortedSeatBookingRequests.SelectMany(s => s.SeatBookings).Where(s => s.Seat == null);
			if (unallocatedShifts.Any())
			{
				bookSeats(unallocatedShifts, false);
			}
		}

		private void bookSeats(IEnumerable<ISeatBooking> seatBookings, Boolean bookGroupedRequestsTogether)
		{
			foreach (var location in _seatMapLocations.OrderByDescending(l => l.SeatCount))
			{
				var unallocatedBookings = seatBookings.Where(s => s.Seat == null);
				if (!unallocatedBookings.Any()) return;

				var targetLocation = bookGroupedRequestsTogether
					? location.GetLocationToAllocateSeats(unallocatedBookings)
					: location;

				if (targetLocation != null)
				{
					bookSeatsForLocation(bookGroupedRequestsTogether, unallocatedBookings, targetLocation);
				}
			}
		}

		private void bookSeatsForLocation(bool bookGroupedRequestsTogether, IEnumerable<ISeatBooking> unallocatedShifts, SeatMapLocation targetSeatMapLocation)
		{
			foreach (var shift in unallocatedShifts)
			{
				var firstUnallocatedSeat = targetSeatMapLocation.GetNextUnallocatedSeat(shift, bookGroupedRequestsTogether);
				if (foundUnallocatedSet(firstUnallocatedSeat))
				{
					shift.Book(firstUnallocatedSeat);
				}
			}
		}

		private bool foundUnallocatedSet(ISeat firstUnallocatedSeat)
		{
			return firstUnallocatedSeat != null;
		}
	}
}