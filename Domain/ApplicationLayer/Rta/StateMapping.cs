using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateMapping
	{
		public Guid BusinessUnitId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string StateCode { get; set; }

		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public bool IsLogOutState { get; set; }
	}
}