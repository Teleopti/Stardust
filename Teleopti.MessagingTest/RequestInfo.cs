using System.Net.Http;

namespace Teleopti.MessagingTest
{
	public class RequestInfo
	{
		public HttpClient Client;
		public string Uri;
		public HttpContent HttpContent;
	}
}