using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnSignInView()
		{
			var layoutBaseViewModelFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var target = new AuthenticationController(layoutBaseViewModelFactory, null, null, null);
			var layoutBaseViewModel = new LayoutBaseViewModel();

			layoutBaseViewModelFactory.Stub(x => x.CreateLayoutBaseViewModel()).Return(layoutBaseViewModel);

			var result = target.SignIn();
			result.ViewName.Should().Be.EqualTo(string.Empty);
			Assert.That(result.ViewBag.LayoutBase, Is.SameAs(layoutBaseViewModel));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void DefaultActionShouldRenderDefaultView()
		{
			var target = new AuthenticationController(null, null, null, null);
			var result = target.Index() as RedirectToRouteResult;
			result.RouteName.Should().Be.EqualTo(string.Empty);
		}
	}
}
