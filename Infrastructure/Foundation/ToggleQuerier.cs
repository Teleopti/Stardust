using System;
using System.IO;
using System.Net;
using System.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	//integration tests from Toggle.feature scenario
	public class ToggleQuerier 
	{
		private readonly string _url;

		public ToggleQuerier(string url)
		{
			_url = url;
		}

		public bool IsEnabled(string flag)
		{
			var uriBuilder = new UriBuilder(_url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = flag;
			uriBuilder.Query = query.ToString();
			var request = WebRequest.Create(uriBuilder.ToString());
			using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				return Convert.ToBoolean(reader.ReadToEnd());
			}
		}
	}
}