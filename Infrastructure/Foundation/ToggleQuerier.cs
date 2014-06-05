using System;
using System.IO;
using System.Net;
using System.Web;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	//integration tests from Toggle.feature scenario
	public class ToggleQuerier : IToggleManager
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly string _url;

		public ToggleQuerier(ICurrentDataSource currentDataSource, string url)
		{
			_currentDataSource = currentDataSource;
			_url = url;
		}

		public bool IsEnabled(Toggles toggle)
		{
			var request = createWebRequest(toggle);

			using (var response = request.GetResponse())
			{
				using (var stream = new StreamReader(response.GetResponseStream()))
				{
					return Convert.ToBoolean(stream.ReadToEnd());
				}
			}
		}

		private WebRequest createWebRequest(Toggles toggle)
		{
			var uriBuilder = new UriBuilder(_url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = toggle.ToString();
			query["datasource"] = _currentDataSource.CurrentName();
			uriBuilder.Query = query.ToString();
			var request = WebRequest.Create(uriBuilder.ToString());
			return request;
		}
	}
}