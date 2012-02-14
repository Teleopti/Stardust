namespace Teleopti.Ccc.Web.Core.RequestContext
{
	/// <summary>
	/// Stores and grabs data that needs to be kept during a session
	/// </summary>
	public interface ISessionSpecificDataProvider
	{
		void Store(SessionSpecificData data);

		/// <summary>
		/// Gets the session data
		/// </summary>
		/// <returns>Returns null if no user data is not present</returns>
		SessionSpecificData Grab();
	}
}
