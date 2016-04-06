using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICurrentInitiatorIdentifier
	{
		IInitiatorIdentifier Current();
	}
}