using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class PortalControllerTest
	{
		private PortalController target;
		private MockRepository mocks;
		private IPortalViewModelFactory viewModelFactory;
		private ILayoutBaseViewModelFactory layoutBaseViewModelFactory;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			viewModelFactory = mocks.DynamicMock<IPortalViewModelFactory>();
	    	layoutBaseViewModelFactory = mocks.DynamicMock<ILayoutBaseViewModelFactory>();
			target = new PortalController(viewModelFactory, layoutBaseViewModelFactory);
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}


		[Test]
		public void ShouldCreatePortalModelAndView()
		{
			using (mocks.Record())
			{
				Expect.Call(viewModelFactory.CreatePortalViewModel())
					.Return(new PortalViewModel());
				Expect.Call(layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentPortal))
					.Return(new LayoutBaseViewModel());
			}
			using (mocks.Playback())
			{
				var result = target.Index() as ViewResult;
				var model = result.Model as PortalViewModel;

				result.ViewName.Should().Be.Empty(); // default view
				model.Should().Not.Be.Null();
				LayoutBaseViewModel layoutBase = result.ViewBag.LayoutBase;
				layoutBase.Should().Not.Be.Null();
			}
		}

	}
}