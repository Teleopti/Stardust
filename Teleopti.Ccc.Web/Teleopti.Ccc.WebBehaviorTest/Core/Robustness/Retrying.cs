using System;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
	public static class Retrying
	{
		public static string Javascript(string javaScriptCode)
		{
			return Action(() => Browser.Current.Eval(javaScriptCode));
		}

		public static T Action<T>(Func<T> action)
		{
			var value = default(T);
			var success = false;
			Func<Exception, T> exceptionHandler = e =>
			                                      	{
			                                      		success = false;
			                                      		return default(T);
			                                      	};
			Func<bool> safeAction = () =>
			                        	{
			                        		success = true;
			                        		value = ExceptionHandling.Action(action, exceptionHandler);
			                        		return success;
			                        	};
			safeAction.WaitOrThrow(Timeouts.Poll, Timeouts.Timeout);
			return value;
		}

		public static void Action(Action action)
		{
			var success = false;
			Action<Exception> exceptionHandler = e =>
			                                     	{
			                                     		success = false;
			                                     	};
			Func<bool> safeAction = () =>
			                        	{
			                        		success = true;
			                        		ExceptionHandling.Action(action, exceptionHandler);
			                        		return success;
			                        	};
			safeAction.WaitOrThrow(Timeouts.Poll, Timeouts.Timeout);
		}
	}
}