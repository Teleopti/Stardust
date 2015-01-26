using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;
using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class MenuControllerTest
	{
		[Test]
		public void ShouldReturnAvailableApplications()
		{
			var menuViewModelFactory = MockRepository.GenerateMock<IMenuViewModelFactory>();
			using (var target = new MenuController(menuViewModelFactory))
			{
				var applicationViewModel = new ApplicationViewModel();
				menuViewModelFactory.Stub(x => x.CreateMenuViewModel()).Return(new[] { applicationViewModel });

				var result = target.Applications();

				var resultApplications = result.Data as IEnumerable<ApplicationViewModel>;
				resultApplications.Single().Should().Be(applicationViewModel);
			}
		}
		
	}
}