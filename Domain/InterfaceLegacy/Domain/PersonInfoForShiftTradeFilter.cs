using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class PersonInfoForShiftTradeFilter
	{
		public Guid PersonId { get; set; }
		public bool IsDayOff { get; set; }
	}
}
