﻿using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.WebTest.Areas.Start
{
	[TestFixture]
	public class MenuViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateModelForUserWithAccessToAllDefinedAreas()
		{
			var target = new MenuViewModelFactory(new FakePermissionProvider());

			var result = target.CreateMenyViewModel();

			result.Count().Should().Be.EqualTo(DefinedApplicationAreas.ApplicationAreas.Count());
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToMyTime()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenyViewModel();

			result.Single().Area.Should().Be.EqualTo("MyTime");
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToMobileReports()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenyViewModel();

			result.Single().Area.Should().Be.EqualTo("MobileReports");
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToTeam()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AdminWeb)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenyViewModel();

			result.Single().Area.Should().Be.EqualTo("Team");
		}

	}
}