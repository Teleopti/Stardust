using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class MenuControllerTest
	{
		private MenuController _target;

		[SetUp]
		public void Setup()
		{
			_target = new MenuController();
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		[Test]
		public void DefaultActionShouldRedirectToMyTimeArea()
		{
			var result = _target.Index() as RedirectToRouteResult;
			result.RouteValues["area"].Should().Be.EqualTo("MyTime");
		}
		
		[Test]
		public void MobileMenuShouldReturnDefaultView()
		{
			var result = _target.MobileMenu() as ViewResult;
			result.ViewName.Should().Be.EqualTo(string.Empty);
			
		}
	}
}