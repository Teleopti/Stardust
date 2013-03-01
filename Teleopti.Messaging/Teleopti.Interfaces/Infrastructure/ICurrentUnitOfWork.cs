namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// An interface for getting the current unit of work
	/// </summary>
	public interface ICurrentUnitOfWork
	{
		/// <summary>
		/// The current unit of work
		/// </summary>
		/// <returns></returns>
		IUnitOfWork Current();
	}
}