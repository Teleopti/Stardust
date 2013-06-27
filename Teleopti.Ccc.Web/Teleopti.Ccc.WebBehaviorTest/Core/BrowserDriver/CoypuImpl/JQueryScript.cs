namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class JQueryScript
	{

		public static string JQuerySelector(string selector)
		{
			var jquery = string.Format("$('{0}')", selector);
			if (selector.Contains("'"))
				jquery = string.Format(@"$(""{0}"")", selector);
			return jquery;
		}

		public static string WhenFoundOrThrow(string selector, string script)
		{
			var jquery = JQuerySelector(selector);

			script = string.Format(script, "jq");

			var error = string.Format("throw \"Cannot find element with selector '{0}' using jquery \";", selector);

			return
				"var jq = " + jquery + ";" +
				"if (jq.length > 0) {" +
				script +
				"} else {" +
				error +
				"}"
				;
		}

		public static string WhenNotFoundOrThrow(string selector, string script)
		{
			var jquery = string.Format("$('{0}')", selector);
			if (selector.Contains("'"))
				jquery = string.Format(@"$(""{0}"")", selector);

			script = string.Format(script, "jq");

			var error = string.Format("throw \"Can find element with selector '{0}' using jquery \";", selector);

			return
				"var jq = " + jquery + ";" +
				"if (jq.length == 0) {" +
				script +
				"} else {" +
				error +
				"}"
				;
		}
	}
}