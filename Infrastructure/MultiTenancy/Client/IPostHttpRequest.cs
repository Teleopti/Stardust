namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IPostHttpRequest
	{
		T Send<T>(string url, string json, string userAgent = null);
	}
}