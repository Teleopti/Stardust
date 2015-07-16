using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class IdentityProviderProviderTest
	{
		[Test]
		public void ShouldGetDefaultProvider()
		{
			var target =
				new IdentityProviderProvider(
					new FakeConfigReader(new Dictionary<string, string> {{"DefaultIdentityProvider", "Windows"}}));

			var defaultProvider = target.DefaultProvider();

			defaultProvider.Should().Be.EqualTo("urn:Windows");
		}
	}
}