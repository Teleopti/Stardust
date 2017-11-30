using System;
using System.IO;
using System.Net;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface IWebRequestFactory
	{
		WebRequest Create(Uri url);
	}

	public class WebRequestFactory : IWebRequestFactory
	{
		public WebRequest Create(Uri url)
		{
			return WebRequest.Create(url);
		}
	}

	public class RawPostNotificationWebClient : INotificationClient
	{
		private readonly IWebRequestFactory _webRequestFactory;

		public RawPostNotificationWebClient():this(new WebRequestFactory())
		{
		}

		public RawPostNotificationWebClient(IWebRequestFactory webRequestFactory)
		{
			_webRequestFactory = webRequestFactory;
		}
		public string MakeRequest(Uri url, string queryStringData)
		{
			var req = _webRequestFactory.Create(url);
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			var bytes = Encoding.ASCII.GetBytes(queryStringData);
			req.ContentLength = bytes.Length;
			var os = req.GetRequestStream();
			os.Write(bytes, 0, bytes.Length);
			os.Close();
			var resp = req.GetResponse();
			var sr = new StreamReader(resp.GetResponseStream());
			return sr.ReadToEnd().Trim();
		}
	}
}