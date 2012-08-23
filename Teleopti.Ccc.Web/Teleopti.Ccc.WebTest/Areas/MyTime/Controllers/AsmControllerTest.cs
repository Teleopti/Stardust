using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class AsmControllerTest
	{
		[Test]
		public void ShouldRetriveModelWithPhoneActivity()
		{
			const string payload = "phone";
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var asmViewModel = new AsmViewModel();
			asmViewModel.Layers.Add(new AsmLayer {Payload = payload});
			asmModelFactory.Expect(fac => fac.CreateViewModel()).Return(asmViewModel);

			using (var controller = new AsmController(asmModelFactory, layoutFactory))
			{
				var model = controller.Index().Model as AsmViewModel;
				model.Layers.First().Payload.Should().Be.EqualTo(payload);
			}
		}

		[Test]
		public void ShouldSetPortalViewBag()
		{
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var viewBag = new LayoutBaseViewModel();
			layoutFactory.Expect(fac => fac.CreateLayoutBaseViewModel()).Return(viewBag);

			using (var controller = new AsmController(asmModelFactory, layoutFactory))
			{
				((object)controller.Index().ViewBag.LayoutBase)
					.Should().Be.SameInstanceAs(viewBag);
			}
		}
	}
}
