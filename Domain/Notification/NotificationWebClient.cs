using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
    public class NotificationWebClient : INotificationClient
    {
        private readonly Uri _url;
        private readonly WebClient _client;

        public NotificationWebClient(Uri url)
        {
            _url = url;
            _client = new WebClient();
            _client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			_client.Headers.Add("Accept-Language", "en-US,en;q=0.8,sv-SE;q=0.7,sv;q=0.5");
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public string MakeRequest(string queryStringData)
        {
	        var result = _client.UploadValues(_url, "POST",
		        new NameValueCollection {{"data", queryStringData}});

	        return Encoding.UTF8.GetString(result);
        }
    }
}