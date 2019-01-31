using System.Collections.Specialized;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class GetHttpRequestFake : IGetHttpRequest
	{
		private object _value;
		public string CalledUrl { get; private set; }
		public NameValueCollection Sent { get; private set; }
		public TenantCredentials SendTenantCredentials { get; private set; }

		public T Get<T>(string url, NameValueCollection arguments)
		{
			Sent = arguments;
			CalledUrl = url;
			if (_value != null)
				return (T)_value;

			return default(T);
		}

		public T GetSecured<T>(string url, NameValueCollection arguments, TenantCredentials tenantCredentials)
		{
			SendTenantCredentials = tenantCredentials;
			return Get<T>(url, arguments);
		}

		public void SetReturnValue<T>(T value)
		{
			_value = value;
		}
	}
}