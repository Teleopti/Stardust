using System;
using System.Linq;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Tool
{
	public class SavePmConfigurationCommand : ISupportCommand
	{
		private readonly DBHelper _dbHelper;

		public SavePmConfigurationCommand(string connectionString)
		{
			_dbHelper = new DBHelper(connectionString);
		}

		public void Execute(ModeFile modeFile)
		{
			var pmConfiguration = readPmConfigurationFromSettingsTxt();
			_dbHelper.SavePmConfiguration(pmConfiguration);
		}

		private PmConfiguration readPmConfigurationFromSettingsTxt()
		{
			var settings = new SettingsFileManager().ReadFile();
			var result = new PmConfiguration();
			var searchReplaces = settings.ForDisplay();
			foreach (var setting in searchReplaces)
			{
				if (setting.SearchFor == $"$({nameof(PmConfiguration.PM_INSTALL)})")
				{
					result.PM_INSTALL = bool.Parse(setting.ReplaceWith);
				}
				if (setting.SearchFor == $"$({nameof(PmConfiguration.AS_SERVER_NAME)})")
				{
					result.AS_SERVER_NAME = setting.ReplaceWith;
				}
				if (setting.SearchFor == $"$({nameof(PmConfiguration.AS_DATABASE)})")
				{
					result.AS_DATABASE = setting.ReplaceWith;
				}
				if (setting.SearchFor == $"$({nameof(PmConfiguration.PM_SERVICE)})")
				{
					result.PM_SERVICE = setting.ReplaceWith;
				}
				if (setting.SearchFor == $"$({nameof(PmConfiguration.ETLPM_BINDING_NAME)})")
				{
					result.ETLPM_BINDING_NAME = setting.ReplaceWith;
				}
			}
			return result;
		}
	}

	public class PmConfiguration
	{
		public bool PM_INSTALL { get; set; }
		public string AS_SERVER_NAME { get; set; }
		public string AS_DATABASE { get; set; }
		public string PM_SERVICE { get; set; }
		public string ETLPM_BINDING_NAME { get; set; }
	}

	public class ConfigServerCommand : ISupportCommand
	{
		private readonly DBHelper _dbHelper;

		public ConfigServerCommand(string connectionString)
		{
			_dbHelper = new DBHelper(connectionString);
		}

		public void Execute(ModeFile modeFile)
		{
			var configurations = _dbHelper.GetServerConfigurations();
			string val;
			if (configurations.TryGetValue(ServerConfiguration.FrameAncestors, out val))
			{
				new FrameAncestorsUpdator().Update(val);
			}
			if (configurations.TryGetValue(ServerConfiguration.InstrumentationKey, out val) && !string.IsNullOrEmpty(val))
			{
				new ApplicationInsightsInstrumentationKeyUpdator().Update(val);
			}
		}
	}

	public class ServerConfiguration
	{
		public const string FrameAncestors = "FrameAncestors";
		public const string InstrumentationKey = "InstrumentationKey";
	}
}