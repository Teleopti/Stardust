using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.jQuery
{
	public interface IJQueryExpression
	{
		string Eval();
		IJQueryExpression AddClass(string cssClass);
		IJQueryExpression Trigger(string eventName);
		IJQueryExpression Trigger(string eventName, string arguments);
		IJQueryExpression WidgetCall(string widget, string method);
		IJQueryExpression WidgetOptions(string widget, dynamic options);
		IJQueryExpression Is(string selector);
		IJQueryExpression Change();
	}
}
