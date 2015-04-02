using System.Collections.Generic;
using System.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	[ConfigurationCollection(typeof(AppSettingOverride), AddItemName = "appSetting")]
	public class OverrideCollection : ConfigurationElementCollection, IEnumerable<AppSettingOverride>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new AppSettingOverride();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((AppSettingOverride)element).Key;
		}

		public new IEnumerator<AppSettingOverride> GetEnumerator()
		{
			for (var i = 0; i < Count; i++)
			{
				yield return (AppSettingOverride)BaseGet(i);
			}
		}
	}
}