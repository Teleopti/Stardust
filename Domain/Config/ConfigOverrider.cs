﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Config
{
	public class ConfigOverrider : IConfigReader
	{
		private readonly IConfigReader _defaultConfigReader;
		private readonly IDictionary<string, string> _overridenSettings;

		public ConfigOverrider(IConfigReader defaultConfigReader, IDictionary<string, string> overridenSettings)
		{
			_defaultConfigReader = defaultConfigReader;
			_overridenSettings = overridenSettings;
		}

		public string AppConfig(string name)
		{
			string retValue;
			if (!_overridenSettings.TryGetValue(name, out retValue))
			{
				retValue = _defaultConfigReader.AppConfig(name);
			}
			return retValue;
		}

		public string ConnectionString(string name)
		{
			return _defaultConfigReader.ConnectionString(name);
		}
	}
}