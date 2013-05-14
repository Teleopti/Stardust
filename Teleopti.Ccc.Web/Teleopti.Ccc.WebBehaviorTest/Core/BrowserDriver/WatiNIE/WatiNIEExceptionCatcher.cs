using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using WatiN.Core.Exceptions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNIEExceptionCatcher : IExceptionCatcher
	{
		public T Action<T>(Func<T> action, Func<Exception, T> failureCallback)
		{
			try
			{
				return action.Invoke();
			}
			// in some states even Sizzle cant be called. if so, Find.BySelector may fail with this exception
			catch (RunScriptException ex)
			{
				return failureCallback.Invoke(ex);
			}
			// in some states our javascript "namespaces" havnt been initialized yet, so calling functions at those times will give JS exceptions
			// maybe we should look for the strings "TypeError" and "ReferenceError" here to be more specific.
			catch (JavaScriptException ex)
			{
				return failureCallback.Invoke(ex);
			}
			// sometimes IE api gives these errors when the page is in a state between pages or something, and elements like body is null
			catch (UnauthorizedAccessException ex)
			{
				return failureCallback.Invoke(ex);
			}
			catch (SecurityException ex)
			{
				return failureCallback.Invoke(ex);
			}
			catch (TargetInvocationException ex)
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

		public void Action(Action action, Action<Exception> failureCallback)
		{
			try
			{
				action.Invoke();
			}
			// in some states even Sizzle cant be called. if so, Find.BySelector may fail with this exception
			catch (RunScriptException ex)
			{
				failureCallback.Invoke(ex);
			}
			// in some states our javascript "namespaces" havnt been initialized yet, so calling functions at those times will give JS exceptions
			// maybe we should look for the strings "TypeError" and "ReferenceError" here to be more specific.
			catch (JavaScriptException ex)
			{
				failureCallback.Invoke(ex);
			}
			// sometimes IE api gives these errors when the page is in a state between pages or something, and elements like body is null
			catch (UnauthorizedAccessException ex)
			{
				failureCallback.Invoke(ex);
			}
			catch (SecurityException ex)
			{
				failureCallback.Invoke(ex);
			}
			catch (TargetInvocationException ex)
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
	}
}