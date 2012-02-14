using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.jQuery
{
	public interface IJQueryExpression
	{
		string Eval();
		string EvalIn(DomContainer domContainer);
		IJQueryExpression AddClass(string cssClass);
		IJQueryExpression Trigger(string eventName);
		IJQueryExpression Trigger(string eventName, string arguments);
		IJQueryExpression Widget(string widget, string method);
		IJQueryExpression Is(string selector);
	}
}
