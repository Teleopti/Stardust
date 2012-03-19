using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalViewModelFactoryTeamScheduleTest
	{

		private static SectionNavigationItem RelevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "TeamSchedule" select i)
					.SingleOrDefault();
		}

		private static T ToolBarItemOfType<T>(PortalViewModel result) where T : ToolBarItemBase
		{
			return (from i in RelevantTab(result).ToolBarItems where i is T select i)
				.Cast<T>()
				.SingleOrDefault();
		}

		[Test]
		public void ShouldNotHaveTeamScheduleNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.TeamSchedule))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = RelevantTab(target.CreatePortalViewModel());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldHaveDatePicker()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = ToolBarItemOfType<ToolBarDatePicker>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveTeamPicker()
		{
			var teams = new[] {new Team()};
			teams.ForEach(t => t.SetId(Guid.NewGuid()));
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<ICurrentPrincipalProvider>());

			var result = ToolBarItemOfType<ToolBarSelectBox>(target.CreatePortalViewModel());

			result.Type.Should().Be("TeamPicker");
			result.Options.Should().Be.Empty();
		}

	}
}