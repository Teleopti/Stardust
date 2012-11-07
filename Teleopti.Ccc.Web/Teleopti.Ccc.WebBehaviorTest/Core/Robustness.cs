using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using WatiN.Core.Exceptions;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Robustness
	{
		public static bool IESafeExists(this Element element)
		{
			return SafeIEOperation(() => element.Exists, e => false);
		}

		public static T SafeIEOperation<T>(Func<T> action, Func<Exception, T> failureCallback)
		{
			try
			{
				return action.Invoke();
			}
			// in some states our javascript "namespaces" havnt been initialized yet, so calling functions at those times will give JS exceptions
			// maybe we should look for the strings "TypeError" and "ReferenceError" here to be more specific.
			catch (JavaScriptException ex)
			{
				return failureCallback.Invoke(ex);
			}
			// sometimes IE api gives these errors when the page is in a state between pages or something, and elements like body is null
			// if so, lets just try again
			// maybe this behavior should be placed elsewhere and not only apply to asserts..
			catch (UnauthorizedAccessException ex)
			{
				return failureCallback.Invoke(ex);
			}
			catch (NullReferenceException ex)
			{
				return failureCallback.Invoke(ex);
			}
			catch (ArgumentNullException ex)
			{
				return failureCallback.Invoke(ex);
			}
			catch (COMException ex)
			{
				return failureCallback.Invoke(ex);
			}
		}

		public static void SafeIEOperation(Action action, Action<Exception> failureCallback)
		{
			try
			{
				action.Invoke();
			}
				// sometimes IE api gives these errors when the page is in a state between pages or something, and elements like body is null
				// if so, lets just try again
				// maybe this behavior should be placed elsewhere and not only apply to asserts..
			catch (UnauthorizedAccessException ex)
			{
				failureCallback.Invoke(ex);
			}
			catch (NullReferenceException ex)
			{
				failureCallback.Invoke(ex);
			}
			catch (ArgumentNullException ex)
			{
				failureCallback.Invoke(ex);
			}
			catch (COMException ex)
			{
				failureCallback.Invoke(ex);
			}
		}

		public static T RetryIEOperation<T>(Func<T> action)
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
			                        		value = SafeIEOperation(action, exceptionHandler);
			                        		return success;
			                        	};
			safeAction.WaitOrThrow(EventualTimeouts.Poll, EventualTimeouts.Timeout);
			return value;
		}


		public static void RetryIEOperation(Action action)
		{
			var success = false;
			Action<Exception> exceptionHandler = e =>
			                                     	{
			                                     		success = false;
			                                     	};
			Func<bool> safeAction = () =>
			                        	{
			                        		success = true;
			                        		SafeIEOperation(action, exceptionHandler);
			                        		return success;
			                        	};
			safeAction.WaitOrThrow(EventualTimeouts.Poll, EventualTimeouts.Timeout);
		}

	}
}