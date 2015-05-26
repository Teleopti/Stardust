using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Teleopti.Ccc.Domain.Common
{
	public static class ConfigurationManagerExtensions
	{
		public static IDictionary<string, string> ToDictionary(this NameValueCollection appSettings)
		{
			return appSettings.AllKeys.ToDictionary(key => key,
					key => ConfigurationManager.AppSettings[key]);
		}
	}
}