using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public interface IRtaMap : IAggregateRoot_Events
	{
		Guid? Activity { get; }
		IRtaStateGroup StateGroup { get; }
		IRtaRule RtaRule { get; set; }
		IRtaMap EntityClone();
	}
}