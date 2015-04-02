using System.Collections.Generic;
using System.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	[ConfigurationCollection(typeof(Installation), AddItemName = "installation")]
	public class InstallationCollection : ConfigurationElementCollection, IEnumerable<Installation>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new Installation();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((Installation)element).Name;
		}

		public new IEnumerator<Installation> GetEnumerator()
		{
			for (var i = 0; i < Count; i++)
			{
				yield return (Installation) BaseGet(i);
			}
		}
	}
}