using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.WebTest.Core.Preference
{
	[TestFixture]
	public class PreferenceHtmlExtensionTest
	{

		[Test]
		public void ShouldCreateHtmlForPreferenceDayInfo()
		{
			var helper = CreateHtmlHelper(new ViewDataDictionary());
			var preferenceDayModel = new PreferenceDayViewModel
			                         	{
			                         		Preference = "PM"
			                         	};
			
			var result = helper.DayContent(preferenceDayModel);

			result.ToString().Should().Be("<div class=\"day-content pdt10\"><span class=\"fullwidth displayblock\"></span><span class=\"preference fullwidth displayblock clearrightfloat\">PM</span></div>");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static HtmlHelper<PreferenceViewModel> CreateHtmlHelper(ViewDataDictionary viewData)
		{
			var cc = MockRepository.GenerateMock<ControllerContext>(
				MockRepository.GenerateMock<HttpContextBase>(),
				new RouteData(),
				MockRepository.GenerateMock<ControllerBase>());
			cc.Stub(x => x.Controller).Return(null);
			var mockViewContext = MockRepository.GenerateMock<ViewContext>(
				cc,
				MockRepository.GenerateMock<IView>(),
				viewData,
				new TempDataDictionary(), new StringWriter());

			var mockViewDataContainer = MockRepository.GenerateMock<IViewDataContainer>();

			mockViewDataContainer.Stub(v => v.ViewData).Return(viewData);

			var htmlHelper = new HtmlHelper<PreferenceViewModel>(mockViewContext, mockViewDataContainer);

			return htmlHelper;
		}
	}
}
