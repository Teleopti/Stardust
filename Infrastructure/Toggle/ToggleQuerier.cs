using System;
using System.Collections.Generic;
using System.Web;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;

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
			return uriBuilder.ExecuteJsonRequest<ToggleEnabledResult>().IsEnabled;
		}

		public void RefetchToggles()
		{
			var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/AllToggles");
			_loadedToggles = uriBuilder.ExecuteJsonRequest<IDictionary<Toggles, bool>>();
		}
	}
}