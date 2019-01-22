using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.AppGuide;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture, MyTimeWebTest]
	public class AppGuideControllerTest
	{
		public AppGuideController Target;
		public CurrentTenantFake CurrentTenant;
		
		[Test]
		public void ShouldCreateViewModel()
		{
			Target.WFMApp().Model.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnUrlForConfiguration()
		{
			CurrentTenant.Current().SetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl, "test");

			var result = Target.WFMApp();

			(result.Model as WFMAppGuideViewModel).UrlForMyTimeWeb.Should().Be
				.EqualTo(CurrentTenant.Current()
				.GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl));
		}

	}
}
