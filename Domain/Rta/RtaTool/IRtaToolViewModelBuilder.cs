using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.RtaTool
{
	public class RtaToolAgentStateFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; }
		public IEnumerable<Guid> TeamIds { get; set; }
	}
}