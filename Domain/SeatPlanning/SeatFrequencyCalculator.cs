using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatFrequencyCalculator : ISeatFrequencyCalculator
	{
		private readonly ISeatBookingRepository _seatBookingRepository;

		public SeatFrequencyCalculator(ISeatBookingRepository seatBookingRepository)
		{
			_seatBookingRepository = seatBookingRepository;
		}

		public Dictionary<Guid, List<ISeatOccupancyFrequency>> GetSeatPopulationFrequency(DateOnlyPeriod period, List<IPerson> people)
		{
			const int sitInSameSeatNumberOfDaysForSeatFrequencyChecks = 7;
			var seatOccupancyFrequencies = new Dictionary<Guid, List<ISeatOccupancyFrequency>>();

			var endDateForSeatFrequencyChecks = period.StartDate.AddDays (-1);
			var startDateForFrequencyChecks = period.StartDate.AddDays (-sitInSameSeatNumberOfDaysForSeatFrequencyChecks);
			var periodForFrequencyChecks = new DateOnlyPeriod (startDateForFrequencyChecks, endDateForSeatFrequencyChecks);
			var allBookingsInPeriod = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod(periodForFrequencyChecks);

			foreach (var booking in allBookingsInPeriod.Where (booking => people.Contains(booking.Person) ))
			{
				List<ISeatOccupancyFrequency> frequenciesForSeats;
				if (!seatOccupancyFrequencies.TryGetValue(booking.Person.Id.GetValueOrDefault(), out frequenciesForSeats))
				{
					frequenciesForSeats = new List<ISeatOccupancyFrequency>();
					seatOccupancyFrequencies.Add(booking.Person.Id.GetValueOrDefault(), frequenciesForSeats);
				}

				addOrUpdateSeatFrequency(frequenciesForSeats, booking);
			}
			return seatOccupancyFrequencies;
		}

		private void addOrUpdateSeatFrequency(ICollection<ISeatOccupancyFrequency> frequenciesForSeats, ISeatBooking booking)
		{
			var frequencyForSeat = frequenciesForSeats.SingleOrDefault(frequency => frequency.Seat == booking.Seat);
			if (frequencyForSeat == null)
			{
				frequencyForSeat = new SeatOccupancyFrequency()
				{
					Seat = booking.Seat
				};
				frequenciesForSeats.Add(frequencyForSeat);
			}

			frequencyForSeat.Frequency++;
		}

	}
	
	public class SeatOccupancyFrequency : ISeatOccupancyFrequency
	{
		public ISeat Seat { get; set; }
		public int Frequency { get; set; }

	}
}