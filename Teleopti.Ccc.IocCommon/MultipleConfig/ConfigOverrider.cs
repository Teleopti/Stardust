using System.Xml;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class ConfigOverrider : IConfigOverrider
	{
		private readonly string _filePath;
		private XmlDocument _doc;

		public ConfigOverrider(string filePath)
		{
			_filePath = filePath;
		}
		
		public string AppSetting(string key)
		{
			makeSureConfigIsRead();
			var setting = _doc.DocumentElement
				.SelectSingleNode(string.Format("/appSettings/add[@key='{0}']", key));
			return setting==null ? null : setting.Attributes["value"].Value;
		}

		private void makeSureConfigIsRead()
		{
			if (_doc != null)
				return;

			_doc = new XmlDocument();
			_doc.Load(_filePath);
		}
	}
}