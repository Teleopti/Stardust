using System;
using System.IO;
using System.Net;
using System.Web;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	//integration tests from Toggle.feature scenario
	public class ToggleQuerier : IToggleManager
	{
		private readonly string _url;

		public ToggleQuerier(string url)
		{
			_url = url;
		}

		public bool IsEnabled(Toggles toggle)
		{
			var uriBuilder = new UriBuilder(_url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = toggle.ToString();
			uriBuilder.Query = query.ToString();
			var request = WebRequest.Create(uriBuilder.ToString());
			
			using (var response = request.GetResponse())
			{
				using (var stream = new StreamReader(response.GetResponseStream()))
				{
					return Convert.ToBoolean(stream.ReadToEnd());
				}
			}
		}
	}
}