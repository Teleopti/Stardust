using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICurrentInitiatorIdentifier
	{
		IInitiatorIdentifier Current();
	}
}