using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class IdentityProviderProviderTest
	{
		[Test]
		public void ShouldGetDefaultProvider()
		{
			var config = MockRepository.GenerateMock<IConfigurationWrapper>();
			config.Expect(x => x.AppSettings).Return(new Dictionary<string, string> {{"DefaultIdentityProvider", "Windows"}});
			var target = new IdentityProviderProvider(config);

			var defaultProvider = target.DefaultProvider();

			defaultProvider.Should().Be.EqualTo("urn:Windows");
		}
	}
}