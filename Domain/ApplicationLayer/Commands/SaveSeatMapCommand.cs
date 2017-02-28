using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class SaveSeatMapCommand : ISaveSeatMapCommand
	{
		public Guid? Id { get; set; }
		public string SeatMapData { get; set; }
		public LocationInfo[] ChildLocations { get; set; }
		public SeatInfo[] Seats { get; set; }
		public string LocationPrefix { get; set; }
		public string LocationSuffix { get; set; }
	}
}