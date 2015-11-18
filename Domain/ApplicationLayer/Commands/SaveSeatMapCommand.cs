using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class SaveSeatMapCommand : ISaveSeatMapCommand
	{
		public Guid? Id { get; set; }
		public string SeatMapData { get; set; }
		public LocationInfo[] ChildLocations { get; set; }
		public SeatInfo[] Seats { get; set; }
	}
}