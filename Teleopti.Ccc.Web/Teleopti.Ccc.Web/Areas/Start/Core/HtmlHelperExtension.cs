using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.Start.Core
{
	public static class HtmlHelperExtension
	{
		public static LayoutBaseHtmlHelper LayoutBase(this HtmlHelper htmlHelper)
		{
			return new LayoutBaseHtmlHelper(htmlHelper);
		}
	}
}