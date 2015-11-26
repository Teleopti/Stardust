using System;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public interface IExceptionCatcher
	{
		T Action<T>(Func<T> action, Func<Exception, T> failureCallback);
		void Action(Action action, Action<Exception> failureCallback);
	}
}