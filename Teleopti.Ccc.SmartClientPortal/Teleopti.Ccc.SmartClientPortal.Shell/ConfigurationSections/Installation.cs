using System.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections
{
	public class Installation : ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
		}

		[ConfigurationProperty("overrides")]
		public OverrideCollection Overrides
		{
			get { return (OverrideCollection)this["overrides"]; }
		}
	}
}