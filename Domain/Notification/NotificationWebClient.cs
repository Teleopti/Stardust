using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
    public class NotificationWebClient : INotificationClient
    {
        public string MakeRequest(Uri url, string queryStringData)
        {
	        using (var client = new WebClient())
	        {
				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				client.Headers.Add("Accept-Language", "en-US,en;q=0.8,sv-SE;q=0.7,sv;q=0.5");

				var result = client.UploadValues(url, "POST",
					new NameValueCollection { { "data", queryStringData } });

				return Encoding.UTF8.GetString(result);
			}
        }
    }
}