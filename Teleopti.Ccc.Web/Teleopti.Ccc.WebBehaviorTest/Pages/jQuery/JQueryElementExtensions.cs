using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.jQuery
{
	public static class JQueryElementExtensions
	{
		public static IJQueryExpression JQuery(this Element element)
		{
			return jQuery.JQuery.SelectById(element.Id);
		}
	}
}