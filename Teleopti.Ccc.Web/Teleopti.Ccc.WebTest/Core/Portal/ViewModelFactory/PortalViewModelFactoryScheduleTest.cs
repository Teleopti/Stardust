using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryScheduleTest
	{

		private static SectionNavigationItem RelevantTab(PortalViewModel result)
		{
			return (from i in result.NavigationItems where i.Controller == "Schedule" select i)
					.SingleOrDefault();
		}

		private static T ToolBarItemOfType<T>(PortalViewModel result) where T : ToolBarItemBase
		{
			return (from i in RelevantTab(result).ToolBarItems where i is T select i)
				.Cast<T>()
				.SingleOrDefault();
		}

		
		[Test]
		public void ShouldHaveDatePicker()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = ToolBarItemOfType<ToolBarDatePicker>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveTodayButton()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = ToolBarItemOfType<ToolBarButtonItem>(target.CreatePortalViewModel());

			result.Should().Not.Be.Null();
		}

	}
}