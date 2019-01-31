using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	//integration tests from Toggle.feature scenario and unit tests from ioc common
	public class ToggleQuerier : IToggleManager, IToggleFiller
	{
		private readonly string _pathToWebAppOrFile;
		private IGetHttpRequest _getHttpRequest = new GetHttpRequest();
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
				_loadedToggles = _getHttpRequest.Get<IDictionary<Toggles, bool>>(uriBuilder.ToString(), new NameValueCollection());
			}
		}
	}
}