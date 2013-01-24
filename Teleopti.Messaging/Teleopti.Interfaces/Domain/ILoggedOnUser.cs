namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Who's logged on?
	/// </summary>
	public interface ILoggedOnUser
	{
		/// <summary>
		/// Returns current logged on <see cref="IPerson"/>
		/// </summary>
		IPerson CurrentUser();
	}
}