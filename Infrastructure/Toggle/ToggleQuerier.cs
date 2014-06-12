using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	//integration tests from Toggle.feature scenario and unit tests from ioc common
	public class ToggleQuerier : IToggleManager, IToggleFiller
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly string _pathToWebAppOrFile;
		private IDictionary<Toggles, bool> _loadedToggles;

		public ToggleQuerier(ICurrentDataSource currentDataSource, string pathToWebAppOrFile)
		{
			_currentDataSource = currentDataSource;
			_pathToWebAppOrFile = pathToWebAppOrFile;
		}

		public bool IsEnabled(Toggles toggle)
		{
			if (_loadedToggles != null)
				return _loadedToggles[toggle];

			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/IsEnabled");
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["toggle"] = toggle.ToString();
			query["datasource"] = _currentDataSource.CurrentName();
			uriBuilder.Query = query.ToString();
			var url = uriBuilder.ToString();

			using (var client = new WebClient())
			{
				var jsonString = client.DownloadString(url);
				return JsonConvert.DeserializeObject<ToggleEnabledResult>(jsonString).IsEnabled;
			}
		}

		public void FillAllToggles()
		{
			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/AllToggles");
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["datasource"] = _currentDataSource.CurrentName();
			uriBuilder.Query = query.ToString();
			var url = uriBuilder.ToString();

			using (var client = new WebClient())
			{
				var jsonString = client.DownloadString(url);
				_loadedToggles = JsonConvert.DeserializeObject<IDictionary<Toggles, bool>>(jsonString);
			}
		}
	}
}