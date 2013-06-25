namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// Finds correct <see cref="IUnitOfWorkFactory"/>
	/// </summary>
	public interface ICurrentUnitOfWorkFactory
	{
		/// <summary>
		/// Returns <see cref="IUnitOfWorkFactory"/> for current logged on user
		/// </summary>
		/// <returns></returns>
		IUnitOfWorkFactory LoggedOnUnitOfWorkFactory();

		/// <summary>
		/// Returns <see cref="IUnitOfWorkFactory"/> for current logged on user
		/// </summary>
		/// <returns></returns>
		IUnitOfWorkFactory WithoutLoggedOnUnitOfWorkFactory();
	}
}