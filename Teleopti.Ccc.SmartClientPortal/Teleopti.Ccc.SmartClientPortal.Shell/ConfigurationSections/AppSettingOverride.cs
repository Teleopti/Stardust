using System.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	public class AppSettingOverride : ConfigurationElement
	{
		[ConfigurationProperty("key", IsKey = true, IsRequired = true)]
		public string Key => (string) this["key"];

		[ConfigurationProperty("value", IsRequired = true)]
		public string Value => (string)this["value"];
	}
}