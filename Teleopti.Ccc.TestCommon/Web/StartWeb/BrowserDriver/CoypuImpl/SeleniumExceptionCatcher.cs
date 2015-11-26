using System;
using OpenQA.Selenium;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver.CoypuImpl
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
			// occurs on js script error, when jquery $ is not initialized for example
			catch (InvalidOperationException ex)
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
			// occurs on js script error, when jquery $ is not initialized for example
			catch (InvalidOperationException ex)
			{
				failureCallback.Invoke(ex);
			}
		}
	}
}