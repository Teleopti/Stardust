using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability
{
	[TestFixture]
	public class StudentAvilabilityHtmlExtensionTest
	{
		private HtmlHelper<StudentAvailabilityViewModel> _htmlHelper;

		[SetUp]
		public void Setup()
		{
			_htmlHelper = CreateHtmlHelper(new ViewDataDictionary());
		}

		[Test]
		public void ShouldGenerateHtmlForAvailableShiftDayInfo()
		{
			var availableViewModel = new AvailableDayViewModel
			                         	{
			                         		Date = new DateOnly(),
			                         		EndTimeSpan = "18:00 - 21:00",
			                         		StartTimeSpan = "09:00 - 12:00",
			                         		WorkTimeSpan = "08:00 - 09:00",
			                         		AvailableTimeSpan = "07:00 - 21:00"
			                         	};

			var result = _htmlHelper.DayContent(availableViewModel);

			result.ToString().Should().Not.Be.Empty();

			result.ToHtmlString().Should().StartWith("<div class=\"day-content");
			result.ToHtmlString().Should().Contain("<span class=\"fullwidth displayblock clearrightfloat\">07:00 - 21:00</span>");
		}

		[Test]
		public void ShouldGenerateHtmlForScheduledDayInfo()
		{
			var scheduledDayInfoViewModel = new ScheduledDayViewModel
			                                	{
			                                		Date = new DateOnly(),
			                                		Summary = "8:00",
			                                		TimeSpan = "08:00 - 16:00",
			                                		Title = "Day",
			                                		StyleClassName = "p_777777"
			                                	};

			var result = _htmlHelper.DayContent(scheduledDayInfoViewModel);

			result.ToString().Should().Not.Be.Empty();
			// 
			result.ToHtmlString().Should().StartWith("<div class=\"day-content");
			result.ToHtmlString().Should().Contain("<span class=\"fullwidth displayblock\">Day</span>");
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		public static HtmlHelper<StudentAvailabilityViewModel> CreateHtmlHelper(ViewDataDictionary viewData)
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

			//mockViewContext.Expect(x => x.ClientValidationEnabled).Return(true);
			var mockViewDataContainer = MockRepository.GenerateMock<IViewDataContainer>();

			mockViewDataContainer.Stub(v => v.ViewData).Return(viewData);

			var htmlHelper = new HtmlHelper<StudentAvailabilityViewModel>(mockViewContext, mockViewDataContainer);


			return htmlHelper;
		}
	}
}