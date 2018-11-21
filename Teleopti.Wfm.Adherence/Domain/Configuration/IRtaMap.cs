using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Configuration
{
	public interface IRtaMap : IAggregateRootWithEvents, ICloneableEntity<IRtaMap>
	{
		IActivity Activity { get; }
		IRtaStateGroup StateGroup { get; }
		IRtaRule RtaRule { get; set; }
	}
}