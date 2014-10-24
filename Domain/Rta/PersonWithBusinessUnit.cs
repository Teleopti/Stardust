using System;

namespace Teleopti.Ccc.Domain.Rta
{
	public struct PersonWithBusinessUnit
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}