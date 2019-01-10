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
		private readonly object locker = new object();

		public ToggleQuerier(string pathToWebAppOrFile)
		{
			_pathToWebAppOrFile = pathToWebAppOrFile;
		}

		public bool IsEnabled(Toggles toggle)
		{
			lock (locker)
			{
				if(_loadedToggles ==null)
					RefetchToggles();
			
				return _loadedToggles[toggle];				
			}
		}

		public void RefetchToggles()
		{
			lock (locker)
			{
				var uriBuilder = new UriBuilder(_pathToWebAppOrFile + "ToggleHandler/AllToggles");
				_loadedToggles = uriBuilder.ExecuteJsonRequest<IDictionary<Toggles, bool>>();				
			}
		}
	}
}