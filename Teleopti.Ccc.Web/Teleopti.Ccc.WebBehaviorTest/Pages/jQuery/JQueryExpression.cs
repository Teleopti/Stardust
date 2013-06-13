using System.Text;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.jQuery
{
	public class JQueryExpression : IJQueryExpression
	{
		private readonly StringBuilder _expression = new StringBuilder();

		private string MakeSelector(string selector)
		{
			if (selector.Contains("'"))
			{
				return string.Format(@"$(""{0}"")", selector);
			}
			return string.Format("$('{0}')", selector);
		}

		public IJQueryExpression Select(string selector)
		{
			_expression.Append(MakeSelector(selector));
			return this;
		}

		public IJQueryExpression SelectById(string id)
		{
			_expression.Append(MakeSelector(string.Format("#{0}", id)));
			return this;
		}

		public IJQueryExpression SelectByElementAttribute(string element, string attribute, string value)
		{
			_expression.Append(MakeSelector(string.Format("{0}[{1}='{2}']", element, attribute, value)));
			return this;
		}

		public string Eval()
		{
			_expression.Append(";");
			_expression.Insert(0, "return ");
			return Core.Browser.Interactions.Javascript(_expression.ToString()) as string;
		}

		public IJQueryExpression AddClass(string cssClass)
		{
			_expression.Append(string.Format(".addClass('{0}')", cssClass));
			return this;
		}

		public IJQueryExpression Change()
		{
			_expression.Append(".change()");
			return this;
		}

		public IJQueryExpression Trigger(string eventName)
		{
			_expression.Append(string.Format(".trigger('{0}')", eventName));
			return this;
		}

		public IJQueryExpression Trigger(string eventName, string arguments)
		{
			_expression.Append(string.Format(".trigger('{0}', {1})", eventName, arguments));
			return this;
		}

		public IJQueryExpression WidgetCall(string widget, string method)
		{
			_expression.Append(string.Format(".{0}('{1}')", widget, method));
			return this;
		}

		public IJQueryExpression WidgetOptions(string widget, dynamic options)
		{
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(options);
			_expression.Append(string.Format(".{0}({1})", widget, json));
			return this;
		}

		public IJQueryExpression Is(string selector)
		{
			_expression.Append(string.Format(".is('{0}')", selector));
			return this;
		}
	}
}
