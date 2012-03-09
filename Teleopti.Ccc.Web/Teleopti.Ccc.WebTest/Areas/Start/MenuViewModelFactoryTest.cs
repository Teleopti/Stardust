namespace Teleopti.Ccc.WebTest.Areas.Start
{
	using System.Linq;

	using NUnit.Framework;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Security.AuthorizationData;
	using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

	[TestFixture]
	public class MenuViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateModelForUserWithAccessToAllDefinedAreas()
		{
			
			var target = new MenuViewModelFactory(new FakePermissionProvider(new []{ DefinedRaptorApplicationFunctionPaths.Anywhere, DefinedRaptorApplicationFunctionPaths.MyTimeWeb  }));

			var result = target.CreateMenyViewModel();

			result.Count().Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToMyTime()
		{
			var target = new MenuViewModelFactory(new FakePermissionProvider(new[] { DefinedRaptorApplicationFunctionPaths.MyTimeWeb }));

			var result = target.CreateMenyViewModel();

			result.Count().Should().Be.EqualTo(1);
			result.First().Area.Should().Be.EqualTo("MyTime");
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToMobileReports()
		{
			var target = new MenuViewModelFactory(new FakePermissionProvider(new[] { DefinedRaptorApplicationFunctionPaths.Anywhere }));

			var result = target.CreateMenyViewModel();

			result.Count().Should().Be.EqualTo(1);
			result.First().Area.Should().Be.EqualTo("MobileReports");
		}

	}
}