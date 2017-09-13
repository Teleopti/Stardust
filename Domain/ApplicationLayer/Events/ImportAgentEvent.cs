using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ImportAgentEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
		public ImportAgentDefaults Defaults { get; set; }
	}
}