namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Change password result.
	/// </summary>
	public interface IChangePasswordResultInfo
	{
		/// <summary>
		/// true if change password successfully, false otherwise
		/// </summary>
		bool IsSuccessful { get; set; }
		/// <summary>
		/// true if authentication successfully using old password, false otherwise
		/// </summary>
		bool IsAuthenticationSuccessful { get; set; }
	}
}
