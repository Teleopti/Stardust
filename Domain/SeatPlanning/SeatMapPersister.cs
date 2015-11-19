using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatMapPersister :ISeatMapPersister
	{
		private readonly IWriteSideRepository<ISeatMapLocation> _seatMapLocationRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatPlanRepository _seatPlanRepository;
		private readonly IApplicationRoleRepository _roleRepository;


		public SeatMapPersister(IWriteSideRepository<ISeatMapLocation> seatMapLocationRepository, IBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit, ISeatBookingRepository seatBookingRepository, ISeatPlanRepository seatPlanRepository, IApplicationRoleRepository roleRepository)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_seatBookingRepository = seatBookingRepository;
			_seatPlanRepository = seatPlanRepository;
			_roleRepository = roleRepository;
		}

		public void Save(ISaveSeatMapCommand command)
		{
			var seatMap = command.Id.HasValue
						? updateExistingSeatMap(command.Id.Value, command.SeatMapData)
						: createNewRootSeatMap(command.SeatMapData);

			deleteRemovedSeats(command.Seats, seatMap);
			updateChildSeatMaps(command.ChildLocations, seatMap);
			updateSeats(command.Seats, seatMap);
		}

		private SeatMapLocation updateExistingSeatMap(Guid seatMapId, string seatMapJsonData)
		{
			var seatMap = _seatMapLocationRepository.LoadAggregate(seatMapId) as SeatMapLocation;
			if (seatMap != null)
			{
				seatMap.SeatMapJsonData = seatMapJsonData;
			}
			return seatMap;
		}

		private SeatMapLocation createNewRootSeatMap(string seatMapJsonData)
		{
			var currentBusinessUnit = _businessUnitRepository.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault());
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation(seatMapJsonData, currentBusinessUnit.Name);
			_seatMapLocationRepository.Add(seatMapLocation);
			
			return seatMapLocation;
		}


		private void updateChildSeatMaps(LocationInfo[] childLocations, SeatMapLocation parentSeatMapLocation)
		{
			deleteRemovedChildSeatMaps(childLocations, parentSeatMapLocation);
			createChildSeatMaps(childLocations, parentSeatMapLocation);
		}

		private void deleteRemovedChildSeatMaps(LocationInfo[] childLocations, SeatMapLocation parentSeatMapLocation)
		{
			if (parentSeatMapLocation.ChildLocations == null) return;


			IEnumerable<SeatMapLocation> locationsToDelete;

			if (childLocations == null)
			{
				locationsToDelete = parentSeatMapLocation.ChildLocations;
			}
			else
			{
				locationsToDelete = from childLocation in parentSeatMapLocation.ChildLocations
									where childLocations.All(child => child.Id != childLocation.Id)
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

		private void createChildSeatMaps(LocationInfo[] childLocations, SeatMapLocation parentSeatMapLocation)
		{
			if (childLocations == null) return;

			foreach (var location in childLocations.Where(location => location.IsNew))
			{
				var seatMap = parentSeatMapLocation.CreateChildSeatMapLocation(location);
				_seatMapLocationRepository.Add(seatMap);
				parentSeatMapLocation.UpdateSeatMapTemporaryId(location.Id, seatMap.Id);
			}
		}

		private void updateSeats(SeatInfo[] seatsInfo, SeatMapLocation seatMapLocation)
		{
			if (seatsInfo == null) return;

			foreach (var seatInfo in seatsInfo)
			{
				if (seatInfo.IsNew)
				{
					createSeats(seatInfo, seatMapLocation);
				}
			}

			//todo: updateSeatProperties performance need to be improve.
			updateSeatProperties(seatsInfo, seatMapLocation);
		}

		private static void createSeats(SeatInfo seatInfo, SeatMapLocation seatMapLocation)
		{
			var seat = seatMapLocation.AddSeat(seatInfo.Name, seatInfo.Priority);
			seatMapLocation.UpdateSeatMapTemporaryId(seatInfo.Id, seat.Id);
			seatInfo.Id = seat.Id;
		}

		private void updateSeatProperties(SeatInfo[] seatsInfo, SeatMapLocation seatMapLocation)
		{
			var roles = _roleRepository.LoadAll();
			if (roles != null)
			{
				foreach (var seat in seatMapLocation.Seats)
				{
					var machedSeatInfo = seatsInfo.Single(seatInfo => seatInfo.Id == seat.Id);
					var foundRoles = roles.Where(role => machedSeatInfo.RoleIdList.Contains(role.Id.Value)).ToArray();
					seat.SetRoles(foundRoles.ToArray());
				}
			}
		}

		private void deleteRemovedSeats(SeatInfo[] seats, SeatMapLocation seatMapLocation)
		{
			IEnumerable<ISeat> seatsToDelete;

			if (seats == null)
			{
				seatsToDelete = seatMapLocation.Seats;
			}
			else
			{
				seatsToDelete =
					from seat in seatMapLocation.Seats
					where seats.All(currentSeat => currentSeat.Id != seat.Id)
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