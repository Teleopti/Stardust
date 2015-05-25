using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	/// <summary>
	/// Attach application principal to context
	/// </summary>
	public interface IRequestContextInitializer
	{
		void SetupPrincipalAndCulture(bool forceUseGregorianCalendar);
	}
}