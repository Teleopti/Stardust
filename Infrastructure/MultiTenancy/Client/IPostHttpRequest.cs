namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IPostHttpRequest
	{
		T Send<T>(string url, string userAgent, string json);
	}
}