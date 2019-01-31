namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ICurrentUnitOfWork
	{
		bool HasCurrent();
		IUnitOfWork Current();
	}
}