using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
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
			var result = Target.WFMApp();

			(result.Model as WFMAppGuideViewModel).UrlForMyTimeWeb.Should().Be
				.EqualTo(CurrentTenant.Current()
				.GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl.ToString()));
		}

	}
}
