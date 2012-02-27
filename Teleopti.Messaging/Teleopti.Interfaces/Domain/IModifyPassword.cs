namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// To get rid of the dependencies on person
	/// </summary>
	public interface IModifyPassword
	{
		/// <summary>
		/// Changes a persons password
		/// </summary>
		/// <param name="person">The person</param>
		/// <param name="oldPassword">Old password</param>
		/// <param name="newPassword">New password</param>
		/// <returns><code>true</code> if changed</returns>
		bool Change(IPerson person, string oldPassword, string newPassword);
	}
}