using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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

			using (var controller = new AsmController(asmModelFactory, layoutFactory,null))
			{
				var model = controller.Today(asmZero);
				((AsmViewModel)model.Data).Layers.First().Payload.Should().Be.EqualTo(payload);
			}
		}

		[Test]
		public void ShouldRetriveAlertTimeSetting()
		{
			var globalSettingDataRepo = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			var alertTime = new AsmAlertTime { SecondsBeforeChange = 60};
			globalSettingDataRepo.Expect(fac => fac.FindValueByKey("AsmAlertTime", new AsmAlertTime())).IgnoreArguments().Return(alertTime);
			using (var controller = new AsmController(null, null, globalSettingDataRepo))
			{
				var setting = controller.AlertTimeSetting();
				((AsmAlertTime)setting.Data).SecondsBeforeChange.Should().Be.EqualTo(60);
			}
		}

		[Test]
		public void ShouldSetPortalViewBag()
		{
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var viewBag = new LayoutBaseViewModel{CultureSpecific = new CultureSpecificViewModel()};
			layoutFactory.Expect(fac => fac.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger)).Return(viewBag);

			using (var controller = new AsmController(asmModelFactory, layoutFactory,null))
			{
				((object)controller.Index().ViewBag.LayoutBase)
					.Should().Be.SameInstanceAs(viewBag);
			}
		}

		[Test]
		public void ShouldAlwaysShowLeftToRight()
		{
			var asmModelFactory = MockRepository.GenerateMock<IAsmViewModelFactory>();
			var layoutFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var viewBag = new LayoutBaseViewModel { CultureSpecific = new CultureSpecificViewModel{Rtl = true} };
			layoutFactory.Expect(fac => fac.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger)).Return(viewBag);

			using (var controller = new AsmController(asmModelFactory, layoutFactory, null))
			{
				((object)controller.Index().ViewBag.LayoutBase.CultureSpecific.Rtl)
					.Should().Be.EqualTo(false);
			}
		}
	}
}
