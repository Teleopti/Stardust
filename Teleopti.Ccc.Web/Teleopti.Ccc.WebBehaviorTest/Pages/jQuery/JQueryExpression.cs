using System.Text;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

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
			return EvalIn(Browser.Current);
		}

		public string EvalIn(DomContainer domContainer)
		{
			_expression.Append(";");
			return domContainer.Eval(_expression.ToString());
		}

		public IJQueryExpression AddClass(string cssClass)
		{
			_expression.Append(string.Format(".addClass('{0}')", cssClass));
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

		public IJQueryExpression Widget(string widget, string method)
		{
			_expression.Append(string.Format(".{0}('{1}')", widget, method));
			return this;
		}

		public IJQueryExpression Is(string selector)
		{
			_expression.Append(string.Format(".is('{0}')", selector));
			return this;
		}
	}
}
