using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class PostHttpRequestFake : IPostHttpRequest
	{
		private object _value;
		public string CalledUrl { get; private set; }

		public T Send<T>(string url, string json, string userAgent = null)
		{
			CalledUrl = url;
			if (_value != null)
				return (T)_value;

			return default(T);
		}

		public void SetReturnValue<T>(T value)
		{
			_value = value;
		}
	}
}