using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface IWebClient : IDisposable
	{
		WebHeaderCollection Headers { get; set; }
		byte[] UploadValues(Uri address, string method, NameValueCollection data);
	}

	public interface IWebClientFactory
	{
		IWebClient Create();
	}

	public class SystemWebClient : WebClient, IWebClient
	{
	}

	public class SystemWebClientFactory : IWebClientFactory
	{
		public IWebClient Create()
		{
			return new SystemWebClient();
		}
	}

	public class ClickatellNotificationWebClient : INotificationClient
    {
	    private readonly IWebClientFactory _webClientFactory;

	    public ClickatellNotificationWebClient():this(new SystemWebClientFactory())
	    {
		}

	    public ClickatellNotificationWebClient(IWebClientFactory webClientFactory)
	    {
		    _webClientFactory = webClientFactory;
	    }

        public string MakeRequest(INotificationConfigReader config, string queryStringData)
        {
	        using (var client = _webClientFactory.Create())
	        {
				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				client.Headers.Add("Accept-Language", "en-US,en;q=0.8,sv-SE;q=0.7,sv;q=0.5");

				var result = client.UploadValues(config.Url, "POST",
					new NameValueCollection { { "data", queryStringData } });

				return Encoding.UTF8.GetString(result);
			}
        }
    }
}