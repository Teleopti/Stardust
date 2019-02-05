using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaState : IBelongsToBusinessUnitId
    {
        string Name { get; set; }
        string StateCode { get; set; }
        IRtaStateGroup Parent { get; set;  }
	}
}