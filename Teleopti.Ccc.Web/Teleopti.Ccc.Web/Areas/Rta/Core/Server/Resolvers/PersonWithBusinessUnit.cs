using System;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers
{
	public struct PersonWithBusinessUnit
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}