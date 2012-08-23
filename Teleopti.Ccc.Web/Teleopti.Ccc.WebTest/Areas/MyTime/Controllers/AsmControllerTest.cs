using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;

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
			var asmViewModel = new AsmViewModel();
			asmViewModel.Layers.Add(new AsmLayer {Payload = payload});
			asmModelFactory.Expect(fac => fac.CreateViewModel()).Return(asmViewModel);

			using (var controller = new AsmController(asmModelFactory))
			{
				var model = controller.Index().Model as AsmViewModel;
				model.Layers.First().Payload.Should().Be.EqualTo(payload);
			}
		}
	}
}
