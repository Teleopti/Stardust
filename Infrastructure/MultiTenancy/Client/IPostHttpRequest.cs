using System.Collections.Specialized;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IPostHttpRequest
	{
		T Send<T>(string url, string json, string userAgent = null);
		T SendSecured<T>(string url, string json, TenantCredentials tenantCredentials);
	}

	public interface IGetHttpRequest
	{
		T Get<T>(string url, NameValueCollection arguments);
		T GetSecured<T>(string url, NameValueCollection arguments, TenantCredentials tenantCredentials);
	}
}