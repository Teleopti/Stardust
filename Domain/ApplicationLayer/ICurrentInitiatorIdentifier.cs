using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICurrentInitiatorIdentifier
	{
		IInitiatorIdentifier Current();
	}
}