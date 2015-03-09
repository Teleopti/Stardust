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
		private readonly IWriteSideRepository<ISeatMap> _seatMapRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public AddSeatMapCommandHandler(IWriteSideRepository<ISeatMap>seatMapRepository, IBusinessUnitRepository businessUnitRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_seatMapRepository = seatMapRepository;
			_businessUnitRepository = businessUnitRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void Handle(AddSeatMapCommand command)
		{
			var seatMap = command.Id.HasValue ? updateExistingSeatMap(command) : createNewRootSeatMap(command);
			updateChildSeatMaps(command, seatMap);
			updateSeats (command, seatMap);
		}

		private SeatMap updateExistingSeatMap (AddSeatMapCommand command)
		{
			var seatMap = _seatMapRepository.LoadAggregate (command.Id.Value) as SeatMap;
			if (seatMap != null)
			{
				seatMap.SeatMapJsonData = command.SeatMapData;
			}
			return seatMap;
		}

		private SeatMap createNewRootSeatMap (AddSeatMapCommand command)
		{
			var currentBusinessUnit = _businessUnitRepository.Get (_currentBusinessUnit.Current().Id.GetValueOrDefault());
			var seatMap = new SeatMap();
			seatMap.CreateSeatMap (command.SeatMapData, currentBusinessUnit.Name);
			_seatMapRepository.Add (seatMap);
			return seatMap;
		}

		private void updateChildSeatMaps (AddSeatMapCommand command, SeatMap parentSeatMap)
		{
			deleteRemovedChildSeatMaps(command, parentSeatMap);
			createChildSeatMaps(command, parentSeatMap);
		}

		private void deleteRemovedChildSeatMaps(AddSeatMapCommand command, SeatMap parentSeatMap)
		{
			var locationsToDelete =
				from childLocation in parentSeatMap.Location.ChildLocations
				where command.ChildLocations.All(child => child.Id != childLocation.Id)
				select childLocation;
			
			locationsToDelete.ToList().ForEach(deleteSeatMapAndLocation);
		}

		private void deleteSeatMapAndLocation(Location location)
		{
			location.ParentLocation.ChildLocations.Remove (location);
			_seatMapRepository.Remove((SeatMap)location.Parent);
		}
		
		private void createChildSeatMaps (AddSeatMapCommand command, SeatMap parentSeatMap)
		{
			if (command.ChildLocations == null) return;

			foreach (var location in command.ChildLocations.Where (location => location.IsNew))
			{
				var seatMap = parentSeatMap.CreateChildSeatMap (location);
				_seatMapRepository.Add (seatMap);
				parentSeatMap.UpdateSeatMapTemporaryId(location.Id, seatMap.Location.Id);
				parentSeatMap.UpdateSeatMapTemporaryId(location.SeatMapId, seatMap.Id);
			}
		}

		private void updateSeats(AddSeatMapCommand command, SeatMap seatMap)
		{
			deleteRemovedSeats(command, seatMap);
			createSeats(command, seatMap);
		}

		private void deleteRemovedSeats(AddSeatMapCommand command, SeatMap seatMap)
		{

			IEnumerable<Seat> seatsToDelete;

			if (command.Seats == null)
			{
				seatsToDelete = seatMap.Location.Seats;
			}
			else
			{
				seatsToDelete =
					from seat in seatMap.Location.Seats
					where command.Seats.All(currentSeat => currentSeat.Id != seat.Id)
					select seat;
			}

			seatsToDelete.ToList().ForEach(seat => seatMap.Location.Seats.Remove(seat));	
			
		}

		private static void createSeats(AddSeatMapCommand command, SeatMap seatMap)
		{
			if (command.Seats == null) return;

			foreach (var seatMapInfo in command.Seats.Where(seat => seat.IsNew))
			{
				var seat = seatMap.Location.AddSeat(seatMapInfo.Name, seatMapInfo.Priority);
				seatMap.UpdateSeatMapTemporaryId(seatMapInfo.Id, seat.Id);
			}
		}
	}
}