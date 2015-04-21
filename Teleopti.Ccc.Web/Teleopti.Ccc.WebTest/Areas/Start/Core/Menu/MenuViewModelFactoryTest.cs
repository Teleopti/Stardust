﻿using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.Menu
{
	[TestFixture]
	public class MenuViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateModelForUserWithAccessToAllDefinedAreas()
		{
			var target = new MenuViewModelFactory(new FakePermissionProvider());

			var result = target.CreateMenuViewModel();

			result.Count().Should().Be.EqualTo(DefinedApplicationAreas.ApplicationAreas.Count());
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToMyTime()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenuViewModel();

			result.Single().Area.Should().Be.EqualTo("MyTime");
			result.Single().Name.Should().Be.EqualTo(Resources.MyTime);
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToWfm()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenuViewModel();

			result.Single().Area.Should().Be.EqualTo("WFM");
			result.Single().Name.Should().Be.EqualTo("WFM");
		}

		[Test]
		public void ShouldCreateModelForUserWithAccessOnlyToAnywhere()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(true);
			var target = new MenuViewModelFactory(permissionProvider);

			var result = target.CreateMenuViewModel();

			result.Single().Area.Should().Be.EqualTo("Anywhere");
			result.Single().Name.Should().Be.EqualTo(getMenuText(DefinedRaptorApplicationFunctionPaths.Anywhere));
		}

		private string getMenuText(string applicationFunctionPath)
		{
			var factory = new DefinedRaptorApplicationFunctionFactory();
			var rawResourceKey = ApplicationFunction.FindByPath(factory.ApplicationFunctionList, applicationFunctionPath).FunctionDescription;

			return Resources.ResourceManager.GetString(rawResourceKey.Replace("xx", string.Empty));
		}
	}
}