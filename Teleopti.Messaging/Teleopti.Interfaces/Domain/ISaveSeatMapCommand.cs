using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISaveSeatMapCommand
	{
		Guid? Id { get; set; }
		string SeatMapData { get; set; }
		LocationInfo[] ChildLocations { get; set; }
		SeatInfo[] Seats { get; set; }
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
		public IList<Guid> RoleIdList { get; set; }
	}
}