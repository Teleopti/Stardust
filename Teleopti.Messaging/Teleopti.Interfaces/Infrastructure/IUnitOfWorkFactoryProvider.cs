namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// Finds correct <see cref="IUnitOfWorkFactory"/>
	/// </summary>
	public interface IUnitOfWorkFactoryProvider
	{
		IUnitOfWorkFactory LoggedOnUnitOfWorkFactory();
	}
}