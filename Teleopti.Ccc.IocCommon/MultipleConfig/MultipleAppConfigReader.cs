using System.Xml;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class MultipleAppConfigReader : IAppConfigReader
	{
		private readonly IAppConfigReader _defaultAppConfigReader;
		private readonly string _fileWithOverridenAppSettings;
		private XmlDocument _doc;

		public MultipleAppConfigReader(IAppConfigReader defaultAppConfigReader, string fileWithOverridenAppSettings)
		{
			_defaultAppConfigReader = defaultAppConfigReader;
			_fileWithOverridenAppSettings = fileWithOverridenAppSettings;
		}

		public string AppConfig(string key)
		{
			var overrideSetting = overridenAppConfig(key);
			return overrideSetting ?? _defaultAppConfigReader.AppConfig(key);
		}

		private string overridenAppConfig(string key)
		{
			makeSureConfigIsRead();
			var setting = _doc.DocumentElement
				.SelectSingleNode(string.Format("/appSettings/add[@key='{0}']", key));
			return setting == null ? null : setting.Attributes["value"].Value;
		}

		private void makeSureConfigIsRead()
		{
			if (_doc != null)
				return;
			_doc = new XmlDocument();
			_doc.Load(_fileWithOverridenAppSettings);
		}
	}
}