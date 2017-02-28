namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ICurrentUnitOfWorkFactory
	{
		IUnitOfWorkFactory Current();
	}
}