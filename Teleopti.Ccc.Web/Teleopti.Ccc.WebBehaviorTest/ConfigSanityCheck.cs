using System.Configuration;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[TestFixture]
	public class ConfigSanityCheck
	{
		[Test]
		public void ShouldLoadConfig()
		{
			Assert.That(ConfigurationManager.AppSettings["configSanityCheck"], 
				Is.EqualTo("Yes, configuration is loaded!"), 
				"No! Configuration is not loaded!!!");
		}
	}
}
