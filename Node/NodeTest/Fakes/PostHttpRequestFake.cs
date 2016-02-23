using Stardust.Node.Interfaces;

namespace NodeTest.Fakes
{
	public class PostHttpRequestFake : IPostHttpRequest
	{
		private object _value;
		public string CalledUrl { get; private set; }
		public string SentJson { get; private set; }

		public T Send<T>(string url,
		                 string json,
		                 string userAgent = null)
		{
			SentJson = json;
			CalledUrl = url;
			if (_value != null)
			{
				return (T) _value;
			}

			return default(T);
		}

		public void SetReturnValue<T>(T value)
		{
			_value = value;
		}
	}
}