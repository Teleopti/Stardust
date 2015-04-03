using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class SaveSeatMapCommand : ITrackableCommand
	{
		public Guid? Id { get; set; }
		public string SeatMapData { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public LocationInfo[] ChildLocations { get; set; }
		public SeatInfo[] Seats { get; set; }
	}

	public class LocationInfo
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public Boolean IsNew { get; set; }
		
	}

	public class SeatInfo
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public Boolean IsNew { get; set; }
		public Int32 Priority { get; set; }
	}

	
}