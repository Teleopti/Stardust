using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
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
			var asmZero = new DateTime(2010, 1, 2);
			const string payload = "phone";
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var asmViewModel = new AsmViewModel {Layers = new List<AsmLayer> {new AsmLayer {Payload = payload}}};
			asmModelFactory.Expect(fac => fac.CreateViewModel(asmZero)).Return(asmViewModel);

			using (var controller = new AsmController(asmModelFactory, layoutFactory))
			{
				var model = controller.Today(asmZero);
				((AsmViewModel)model.Data).Layers.First().Payload.Should().Be.EqualTo(payload);
			}
		}

		[Test]
		public void ShouldSetPortalViewBag()
		{
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var viewBag = new LayoutBaseViewModel();
			layoutFactory.Expect(fac => fac.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger)).Return(viewBag);

			using (var controller = new AsmController(asmModelFactory, layoutFactory))
			{
				((object)controller.Index().ViewBag.LayoutBase)
					.Should().Be.SameInstanceAs(viewBag);
			}
		}
	}
}
