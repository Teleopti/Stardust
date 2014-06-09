using System;
using System.IO;
using System.Net;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
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
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Stream MakeRequest(string queryStringData)
        {
            return _client.OpenRead(_url + queryStringData);
        }
    }
}