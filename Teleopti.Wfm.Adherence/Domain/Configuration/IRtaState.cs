using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Configuration
{
    public interface IRtaState : IAggregateEntity, ICloneableEntity<IRtaState>
    {
		IBusinessUnit BusinessUnit { get; }
        string Name { get; set; }
        string StateCode { get; set; }
        IRtaStateGroup StateGroup { get; }
    }
}