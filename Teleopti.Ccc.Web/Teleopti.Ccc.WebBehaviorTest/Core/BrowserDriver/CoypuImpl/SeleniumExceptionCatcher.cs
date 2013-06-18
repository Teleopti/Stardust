using System;
using OpenQA.Selenium;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class SeleniumExceptionCatcher : IExceptionCatcher
	{
		public T Action<T>(Func<T> action, Func<Exception, T> failureCallback)
		{
			try
			{
				return action.Invoke();
			}
			// occurs on js "throw", and that is used with interactions implemented using jquery
			catch (WebDriverException ex)
			{
				return failureCallback.Invoke(ex);
			}
		}

		public void Action(Action action, Action<Exception> failureCallback)
		{
			try
			{
				action.Invoke();
			}
			// occurs on js "throw", and that is used with interactions implemented using jquery
			catch (WebDriverException ex)
			{
				failureCallback.Invoke(ex);
			}
		}
	}
}