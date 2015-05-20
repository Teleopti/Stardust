namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class PostHttpRequestFake : IPostHttpRequest
	{
		private object _value;
		public string CalledUrl { get; private set; }
		public string SentJson { get; private set; }
		public TenantCredentials SendTenantCredentials { get; private set; }

		public T Send<T>(string url, string json, string userAgent = null)
		{
			SentJson = json;
			CalledUrl = url;
			if (_value != null)
				return (T)_value;

			return default(T);
		}

		public T SendSecured<T>(string url, string json, TenantCredentials tenantCredentials)
		{
			SendTenantCredentials = tenantCredentials;
			return Send<T>(url, json);
		}

		public void SetReturnValue<T>(T value)
		{
			_value = value;
		}
	}
}