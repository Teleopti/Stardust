namespace Teleopti.Ccc.Web.Core.RequestContext
{
	/// <summary>
	/// Attach application principal to context
	/// </summary>
	public interface IRequestContextInitializer
	{
		void SetupPrincipalAndCulture();
	}
}