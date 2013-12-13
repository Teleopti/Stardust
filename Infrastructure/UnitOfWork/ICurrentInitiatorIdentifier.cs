using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ICurrentInitiatorIdentifier
	{
		IInitiatorIdentifier Current();
	}
}