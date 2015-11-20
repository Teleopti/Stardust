using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class ConfigControllerTest
	{
		[Test]
		public void ShouldGetSharedSettings()
		{
			var expected = new SharedSettings();
			var factory = MockRepository.GenerateMock<ISharedSettingsFactory>();
			factory.Expect(x => x.Create()).Return(expected);
			var target = new ConfigController(factory);
			var result = target.SharedSettings().Result<SharedSettings>();

			result.Should().Be.SameInstanceAs(expected);
		}
	}
}