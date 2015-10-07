using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class SkillControllerTest
	{
		[Test]
		public void ShouldGetActivies()
		{
			var activityProvider = MockRepository.GenerateMock<IActivityProvider>();
			var activityViewModels = new ActivityViewModel[] {};
			activityProvider.Stub(x => x.GetAll()).Return(activityViewModels);
			var target = new SkillController(activityProvider);
			var result  = target.Activities();

			result.Should().Be.SameInstanceAs(activityViewModels);
		}
	}
}