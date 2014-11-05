using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	//integration tests from Toggle.feature scenario and unit tests from ioc common
	public class ToggleQuerier : IToggleManager, IToggleFiller
	{
		private readonly string _pathToWebAppOrFile;
		private IDictionary<Toggles, bool> _loadedToggles;

		public ToggleQuerier(string pathToWebAppOrFile)
		{
			_pathToWebAppOrFile = pathToWebAppOrFile;
		}

		public bool IsEnabled(Toggles toggle)
		{
			if (_loadedToggles != null)
				return _loadedToggles[toggle];

			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/IsEnabled");
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = toggle.ToString();
			uriBuilder.Query = query.ToString();
			var url = uriBuilder.ToString();

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					var result = client.GetAsync(url);
					var jsonString = result.Result.Content.ReadAsStringAsync().Result;
					return JsonConvert.DeserializeObject<ToggleEnabledResult>(jsonString).IsEnabled;
				}
			}
		}

		public void FillAllToggles()
		{
			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/AllToggles");
			var url = uriBuilder.ToString();

			using (var client = new WebClient())
			{
				var jsonString = client.DownloadString(url);
				_loadedToggles = JsonConvert.DeserializeObject<IDictionary<Toggles, bool>>(jsonString);
			}
		}
	}
}