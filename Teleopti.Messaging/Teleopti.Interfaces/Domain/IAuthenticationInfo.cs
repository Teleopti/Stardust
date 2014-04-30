namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Token based authentication
	/// </summary>
	public interface IAuthenticationInfo : IAggregateEntity
	{
		/// <summary>
		/// Gets or sets the identity.
		/// </summary>
		/// <value>The identity.</value>
		string Identity { get; set; }
	}
}