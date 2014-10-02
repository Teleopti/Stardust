using System;

namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public struct PersonWithBusinessUnit
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}