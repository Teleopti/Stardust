using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest
{
	public class TestHtmlHelperBuilder
	{
		public HtmlHelper CreateHtmlHelper()
		{
			return CreateHtmlHelper(new ViewDataDictionary());
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		public HtmlHelper CreateHtmlHelper(ViewDataDictionary viewData)
		{
			var controllerContext = MockRepository.GenerateMock<ControllerContext>(
				MockRepository.GenerateMock<HttpContextBase>(),
				new RouteData(),
				MockRepository.GenerateMock<ControllerBase>());
			controllerContext.Stub(x => x.Controller).Return(null);

			var viewContext = MockRepository.GenerateMock<ViewContext>(
				controllerContext,
				MockRepository.GenerateMock<IView>(),
				viewData,
				new TempDataDictionary(), new StringWriter());

			var mockViewDataContainer = MockRepository.GenerateMock<IViewDataContainer>();

			mockViewDataContainer.Stub(v => v.ViewData).Return(viewData);

			return new HtmlHelper(viewContext, mockViewDataContainer);
		}

	}
}