namespace Teleopti.Ccc.WebBehaviorTest.Pages.jQuery
{
	public static class JQuery
	{
		public static IJQueryExpression Select(string selector)
		{
			return new JQueryExpression().Select(selector);
		}

		public static IJQueryExpression SelectById(string Id)
		{
			return new JQueryExpression().SelectById(Id);
		}

		public static IJQueryExpression SelectByElementAttribute(string element, string attribute, string value)
		{
			return new JQueryExpression().SelectByElementAttribute(element, attribute, value);
		}

	}
}
