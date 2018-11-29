using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatMapCommandHandler : IHandleCommand<SaveSeatMapCommand>
	{
		private readonly IWriteSideRepository<ISeatMapLocation> _seatMapLocationRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatPlanRepository _seatPlanRepository;


		public AddSeatMapCommandHandler(IWriteSideRepository<ISeatMapLocation> seatMapLocationRepository, IBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit, ISeatBookingRepository seatBookingRepository, ISeatPlanRepository seatPlanRepository)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_seatBookingRepository = seatBookingRepository;
			_seatPlanRepository = seatPlanRepository;
		}

		public void Handle(SaveSeatMapCommand command)
		{

			var seatMap = command.Id.HasValue ? updateExistingSeatMap(command) : createNewRootSeatMap(command);
			deleteRemovedSeats(command, seatMap);
			updateChildSeatMaps(command, seatMap);
			updateSeats(command, seatMap);
		}

		private SeatMapLocation updateExistingSeatMap(SaveSeatMapCommand command)
		{
			var seatMap = _seatMapLocationRepository.LoadAggregate(command.Id.Value) as SeatMapLocation;
			if (seatMap != null)
			{
				seatMap.SeatMapJsonData = command.SeatMapData;
			}
			return seatMap;
		}

		private SeatMapLocation createNewRootSeatMap(SaveSeatMapCommand command)
		{
			var currentBusinessUnit = _businessUnitRepository.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault());
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation(command.SeatMapData, currentBusinessUnit.Name);
			_seatMapLocationRepository.Add(seatMapLocation);
			return seatMapLocation;
		}

		private void updateChildSeatMaps(SaveSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
		{
			deleteRemovedChildSeatMaps(command, parentSeatMapLocation);
			createChildSeatMaps(command, parentSeatMapLocation);
		}

		private void deleteRemovedChildSeatMaps(SaveSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
		{
			if (parentSeatMapLocation.ChildLocations == null) return;


			IEnumerable<SeatMapLocation> locationsToDelete;

			if (command.ChildLocations == null)
			{
				locationsToDelete = parentSeatMapLocation.ChildLocations;
			}
			else
			{
				locationsToDelete = from childLocation in parentSeatMapLocation.ChildLocations
									where command.ChildLocations.All(child => child.Id != childLocation.Id)
									select childLocation;
			}

			locationsToDelete.ToList().ForEach(deleteSeatMapAndLocation);
		}

		private void deleteSeatMapAndLocation(SeatMapLocation seatMapLocation)
		{
			deleteSeatsAndBookingsFromLocation(seatMapLocation, seatMapLocation.Seats);
			seatMapLocation.ParentLocation.ChildLocations.Remove(seatMapLocation);
			_seatMapLocationRepository.Remove(seatMapLocation);
		}

		private void createChildSeatMaps(SaveSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
		{
			if (command.ChildLocations == null) return;

			foreach (var location in command.ChildLocations.Where(location => location.IsNew))
			{
				var seatMap = parentSeatMapLocation.CreateChildSeatMapLocation(location);
				_seatMapLocationRepository.Add(seatMap);
				parentSeatMapLocation.UpdateSeatMapTemporaryId(location.Id, seatMap.Id);
			}
		}

		private void updateSeats(SaveSeatMapCommand command, SeatMapLocation seatMapLocation)
		{
			createSeats(command, seatMapLocation);
		}

		private static void createSeats(SaveSeatMapCommand command, SeatMapLocation seatMapLocation)
		{
			if (command.Seats == null) return;

			foreach (var seatMapInfo in command.Seats.Where(seat => seat.IsNew))
			{
				var seat = seatMapLocation.AddSeat(seatMapInfo.Name, seatMapInfo.Priority);
				seatMapLocation.UpdateSeatMapTemporaryId(seatMapInfo.Id, seat.Id);
			}
		}

		private void deleteRemovedSeats(SaveSeatMapCommand command, SeatMapLocation seatMapLocation)
		{
			IEnumerable<ISeat> seatsToDelete;

			if (command.Seats == null)
			{
				seatsToDelete = seatMapLocation.Seats;
			}
			else
			{
				seatsToDelete =
					from seat in seatMapLocation.Seats
					where command.Seats.All(currentSeat => currentSeat.Id != seat.Id)
					select seat;
			}

			var bookings = getBookingsForDeletedSeats(seatsToDelete);
			updateSeatPlans(bookings);
			deleteSeatsAndBookingsFromLocation(seatMapLocation, seatsToDelete);
		}

		private IEnumerable<IGrouping<DateOnly, ISeatBooking>> getBookingsForDeletedSeats(IEnumerable<ISeat> seatsToDelete)
		{
			return from seat in seatsToDelete
				   from booking in _seatBookingRepository.GetSeatBookingsForSeat(seat)
				   group booking by booking.BelongsToDate;
		}

		private void updateSeatPlans(IEnumerable<IGrouping<DateOnly, ISeatBooking>> seatBookingsByDate)
		{
			foreach (var deletedBookingsOnDate in seatBookingsByDate)
			{
				var date = deletedBookingsOnDate.Key;
				var bookings = from booking in _seatBookingRepository.LoadSeatBookingsForDay(date)
							   where !deletedBookingsOnDate.Contains(booking)
							   select booking;

				if (!bookings.Any())
				{
					_seatPlanRepository.RemoveSeatPlanForDate(date);
				}
			}
		}

		private void deleteSeatsAndBookingsFromLocation(ISeatMapLocation seatMapLocation, IEnumerable<ISeat> seatsToDelete)
		{
			_seatBookingRepository.RemoveSeatBookingsForSeats(seatsToDelete);
			seatsToDelete.ToList().ForEach(seat => seatMapLocation.Seats.Remove(seat));

		}

	}
}