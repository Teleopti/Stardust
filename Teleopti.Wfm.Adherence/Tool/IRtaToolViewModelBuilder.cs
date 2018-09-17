using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Tool
{
	public class RtaToolAgentStateFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; }
		public IEnumerable<Guid> TeamIds { get; set; }
	}
}