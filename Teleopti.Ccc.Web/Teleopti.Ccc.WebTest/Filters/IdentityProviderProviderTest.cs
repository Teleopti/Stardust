using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class IdentityProviderProviderTest
	{
		[Test]
		public void ShouldGetDefaultProvider()
		{
			var config = new ConfigurationManagerWrapper();
			config.AppSettings["DefaultIdentityProvider"] = "urn:Windows";
			var target = new IdentityProviderProvider(config);

			var defaultProvider = target.DefaultProvider();

			defaultProvider.Should().Be.EqualTo("urn:Windows");
		}
	}
}