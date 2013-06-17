using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuIE
{
	public class CoypuExceptionCatcher : IExceptionCatcher
	{
		public T Action<T>(Func<T> action, Func<Exception, T> failureCallback)
		{
			return action.Invoke();
		}

		public void Action(Action action, Action<Exception> failureCallback)
		{
			action.Invoke();
		}
	}
}