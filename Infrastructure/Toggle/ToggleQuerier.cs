using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

			var jsonString = downloadString(url);
			return JsonConvert.DeserializeObject<ToggleEnabledResult>(jsonString).IsEnabled;
		}

		public void FillAllToggles()
		{
			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/AllToggles");
			var url = uriBuilder.ToString();

			var jsonString = downloadString(url);
			_loadedToggles = JsonConvert.DeserializeObject<IDictionary<Toggles, bool>>(jsonString);
		}

		private static string downloadString(string url)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.AllowAutoRedirect = false;
			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					return reader.ReadToEnd();
				}
			}
		}

	}
}