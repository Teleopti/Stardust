using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public interface IRtaMap : IAggregateRootWithEvents
	{
		Guid? Activity { get; }
		IRtaStateGroup StateGroup { get; }
		IRtaRule RtaRule { get; set; }
		IRtaMap EntityClone();
	}
}