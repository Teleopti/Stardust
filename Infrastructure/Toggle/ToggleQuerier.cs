using System;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	//integration tests from Toggle.feature scenario
	public class ToggleQuerier : IToggleManager, IToggleFiller
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
			using (var client = new WebClient())
			{
				var jsonString = client.DownloadString(createUrl(toggle));
				return JsonConvert.DeserializeObject<ToggleEnabledResult>(jsonString).IsEnabled;
			}
		}

		private string createUrl(Toggles toggle)
		{
			var uriBuilder = new UriBuilder(_url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = toggle.ToString();
			query["datasource"] = _currentDataSource.CurrentName();
			uriBuilder.Query = query.ToString();
			return uriBuilder.ToString();
		}

		public void GetAllToggles()
		{
			
		}
	}
}