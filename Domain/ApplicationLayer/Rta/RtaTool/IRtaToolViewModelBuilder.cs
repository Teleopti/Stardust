using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool
{
	public interface IRtaToolViewModelBuilder
	{
		IEnumerable<RtaToolViewModel> Build();
		IEnumerable<RtaToolViewModel> Build(RtaToolAgentStateFilter rtaToolAgentStateFilter);
	}
	public class RtaToolAgentStateFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; }
		public IEnumerable<Guid> TeamIds { get; set; }
	}
}