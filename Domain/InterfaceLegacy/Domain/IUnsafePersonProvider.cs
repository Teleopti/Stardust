namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Current ("unsafe") person
	/// </summary>
	public interface IUnsafePersonProvider
	{
		/// <summary>
		/// Returns current user
		/// </summary>
		/// <returns></returns>
		IPerson CurrentUser();
	}
}