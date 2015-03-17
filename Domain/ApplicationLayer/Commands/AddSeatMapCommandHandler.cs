using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatMapCommandHandler : IHandleCommand<AddSeatMapCommand>
	{
		private readonly IWriteSideRepository<ISeatMapLocation> _seatMapLocationRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public AddSeatMapCommandHandler(IWriteSideRepository<ISeatMapLocation>seatMapLocationRepository, IBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void Handle(AddSeatMapCommand command)
		{
			var seatMap = command.Id.HasValue ? updateExistingSeatMap(command) : createNewRootSeatMap(command);
			updateChildSeatMaps(command, seatMap);
			updateSeats (command, seatMap);
		}

		private SeatMapLocation updateExistingSeatMap (AddSeatMapCommand command)
		{
			var seatMap = _seatMapLocationRepository.LoadAggregate(command.Id.Value) as SeatMapLocation;
			if (seatMap != null)
			{
				seatMap.SeatMapJsonData = command.SeatMapData;
			}
			return seatMap;
		}

		private SeatMapLocation createNewRootSeatMap(AddSeatMapCommand command)
		{
			var currentBusinessUnit = _businessUnitRepository.Get (_currentBusinessUnit.Current().Id.GetValueOrDefault());
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation (command.SeatMapData, currentBusinessUnit.Name);
			_seatMapLocationRepository.Add (seatMapLocation);
			return seatMapLocation;
		}

		private void updateChildSeatMaps (AddSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
		{
			deleteRemovedChildSeatMaps(command, parentSeatMapLocation);
			createChildSeatMaps(command, parentSeatMapLocation);
		}

		private void deleteRemovedChildSeatMaps(AddSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
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
				where command.ChildLocations.All (child => child.Id != childLocation.Id)
				select childLocation;

			}

			locationsToDelete.ToList().ForEach (deleteSeatMapAndLocation);
		}

		private void deleteSeatMapAndLocation(SeatMapLocation seatMapLocation)
		{
			seatMapLocation.ParentLocation.ChildLocations.Remove (seatMapLocation);
			_seatMapLocationRepository.Remove(seatMapLocation);
		}
		
		private void createChildSeatMaps (AddSeatMapCommand command, SeatMapLocation parentSeatMapLocation)
		{
			if (command.ChildLocations == null) return;

			foreach (var location in command.ChildLocations.Where (location => location.IsNew))
			{
				var seatMap = parentSeatMapLocation.CreateChildSeatMapLocation (location);
				_seatMapLocationRepository.Add (seatMap);
				parentSeatMapLocation.UpdateSeatMapTemporaryId(location.Id, seatMap.Id);
			}
		}

		private void updateSeats(AddSeatMapCommand command, SeatMapLocation seatMapLocation)
		{
			deleteRemovedSeats(command, seatMapLocation);
			createSeats(command, seatMapLocation);
		}

		private void deleteRemovedSeats(AddSeatMapCommand command, SeatMapLocation seatMapLocation)
		{

			IEnumerable<Seat> seatsToDelete;

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

			seatsToDelete.ToList().ForEach(seat => seatMapLocation.Seats.Remove(seat));	
			
		}

		private static void createSeats(AddSeatMapCommand command, SeatMapLocation seatMapLocation)
		{
			if (command.Seats == null) return;

			foreach (var seatMapInfo in command.Seats.Where(seat => seat.IsNew))
			{
				var seat = seatMapLocation.AddSeat(seatMapInfo.Name, seatMapInfo.Priority);
				seatMapLocation.UpdateSeatMapTemporaryId(seatMapInfo.Id, seat.Id);
			}
		}
	}
}