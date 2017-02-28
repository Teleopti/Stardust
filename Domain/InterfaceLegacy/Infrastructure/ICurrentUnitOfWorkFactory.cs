namespace Teleopti.Interfaces.Infrastructure
{
	public interface ICurrentUnitOfWorkFactory
	{
		IUnitOfWorkFactory Current();
	}
}