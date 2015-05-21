namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface ITenantAuthentication
	{
		bool HasAccess();
	}
}