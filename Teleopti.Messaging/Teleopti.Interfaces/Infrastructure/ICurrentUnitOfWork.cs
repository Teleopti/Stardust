namespace Teleopti.Interfaces.Infrastructure
{
	public interface ICurrentUnitOfWork
	{
		bool HasCurrent();
		IUnitOfWork Current();
	}
}