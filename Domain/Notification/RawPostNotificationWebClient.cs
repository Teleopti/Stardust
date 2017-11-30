using System;
using System.Net;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class RawPostNotificationWebClient : INotificationClient
	{
		public string MakeRequest(Uri url, string queryStringData)
		{
			var req = WebRequest.Create(url);
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			var bytes = Encoding.ASCII.GetBytes(queryStringData);
			req.ContentLength = bytes.Length;
			var os = req.GetRequestStream();
			os.Write(bytes, 0, bytes.Length);
			os.Close();
			var resp = req.GetResponse();
			var sr = new System.IO.StreamReader(resp.GetResponseStream());
			return sr.ReadToEnd().Trim();
		}
	}
}