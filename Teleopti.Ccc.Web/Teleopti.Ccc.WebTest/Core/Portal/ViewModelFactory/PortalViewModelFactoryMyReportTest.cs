using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryMyReportTest
	{

		private static NavigationItem relevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "MyReport" select i)
				.SingleOrDefault();
		}

		private IPersonNameProvider _personNameProvider;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person() { Name = new Name() });

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name)).Return("A B");
		}

		[Test]
		public void NavigationItems_WhenNoPermissionForMyReportWeb_ShouldNotContainLinkToReports()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.MyReportWeb))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<ILicenseActivatorProvider>(),
				MockRepository.GenerateMock<IPushMessageProvider>(), _loggedOnUser,
				MockRepository.GenerateMock<IReportsNavigationProvider>(), MockRepository.GenerateMock<IBadgeProvider>(),
				MockRepository.GenerateMock<IBadgeWithRankProvider>(),
				MockRepository.GenerateMock<IBadgeSettingProvider>(), MockRepository.GenerateMock<IToggleManager>(), _personNameProvider);

			var result = relevantTab(target.CreatePortalViewModel());

			result.Should().Be.Null();
		}
	}
}