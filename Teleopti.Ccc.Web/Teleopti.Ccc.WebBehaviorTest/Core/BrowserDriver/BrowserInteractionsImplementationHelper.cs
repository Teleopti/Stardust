using System;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public class BrowserInteractionsImplementationHelper
	{
		private readonly IBrowserInteractions _interactions;
		private readonly IExceptionCatcher _exceptionCatcher;

		public BrowserInteractionsImplementationHelper(IBrowserInteractions interactions, IExceptionCatcher exceptionCatcher)
		{
			_interactions = interactions;
			_exceptionCatcher = exceptionCatcher;
		}

		private string buildCompleteErrorMessage(string message)
		{
			var builder = new StringBuilder();
			builder.Append(message);
			builder.Append(" ");
			_interactions.DumpInfo(s => builder.Append(s));
			return builder.ToString();
		}

		public void TryOperation(Action operation, string message)
		{
			try
			{
				operation.Invoke();
			}
			catch (Exception ex)
			{
				throw new BrowserInteractionException(buildCompleteErrorMessage(message), ex);
			}
		}

		public void RetryingOperation(Action operation, string message)
		{
			try
			{
				Retrying.Action(operation, _exceptionCatcher);
			}
			catch (Exception ex)
			{
				throw new BrowserInteractionException(buildCompleteErrorMessage(message), ex);
			}
		}

		public T RetryingOperation<T>(Func<T> operation, string message)
		{
			try
			{
				return Retrying.Action(operation, _exceptionCatcher);
			}
			catch (Exception ex)
			{
				throw new BrowserInteractionException(buildCompleteErrorMessage(message), ex);
			}
		}

		public void Assert<T>(T value, Constraint constraint, string message)
		{
			try
			{
				NUnit.Framework.Assert.That(value, constraint);
			}
			catch (AssertionException)
			{
				NUnit.Framework.Assert.Fail(buildCompleteErrorMessage(message));
			}
		}

		public void EventualAssert<T>(Func<T> value, Constraint constraint, Func<string> message)
		{
			BrowserDriver.EventualAssert.That(value, constraint, () => buildCompleteErrorMessage(message()), _exceptionCatcher);
		}

		public void EventualAssert<T>(Func<T> value, Constraint constraint, string message)
		{
			BrowserDriver.EventualAssert.That(value, constraint, () => buildCompleteErrorMessage(message), _exceptionCatcher);
		}





		private string getUnsafeString(Func<string> operation)
		{
			try
			{
				return operation();
			}
			catch (Exception)
			{
				return "Failed";
			}
		}

		public void DumpInfo(Action<string> writer, Func<string> url, Func<string> html)
		{
			writer(" Time: ");
			writer(DateTime.Now.ToString());
			writer(" Url: ");
			writer(getUnsafeString(url));
			writer(" Html: ");
			writer(getUnsafeString(html));
		}

		public void DumpUrl(Action<string> writer, Func<string> url)
		{
			writer(getUnsafeString(url));
		}




		public string JavascriptMessage(string javascript)
		{
			return "Failed to execute javascript " + javascript;
		}

		public string GoToWaitForCompletedMessage(string uri)
		{
			return "Failed to goto url " + uri + " and wait for completion";
		}

		public string GoToWaitForUrlAssertMessage(string uri, string assertUrlContains)
		{
			return "Failed to goto url " + uri + " without waiting";
		}

		public string ClickMessage(string selector)
		{
			return "Failed to click " + selector;
		}

		public string ClickContainingMessage(string selector, string text)
		{
			return "Failed to click " + selector + " containing " + text;
		}

		public string AssertUrlContainsMessage(string url)
		{
			return "Failed to assert that current url contains " + url;
		}

		public string AssertUrlNotContainsMessage(string urlContains, string urlNotContains)
		{
			return "Failed to assert that current url did not contain " + urlNotContains;
		}

		public string AssertJavascriptResultContainsMessage(string javascript, string text, string lastActual)
		{
			return "Failed to assert that javascript " + javascript + " returned a value containing " + text + ". Last value returned was " + lastActual;
		}

		public string AssertExistsMessage(string selector)
		{
			return "Could not find element matching selector " + selector;
		}

		public string AssertNotExistsMessage(string existsSelector, string notExistsSelector)
		{
			return "Found element matching selector " + notExistsSelector + " although I shouldnt";
		}

		public string AssertAnyContainsMessage(string selector, string text)
		{
			return "Could not find element matching selector " + selector;
		}

		public string AssertFirstContainsMessage(string selector, string text)
		{
			return "Failed to assert that " + selector + " contained text " + text;
		}

		public string AssertFirstNotContainsMessage(string selector, string text)
		{
			return "Failed to assert that " + selector + " did not contain text " + text;
		}
	}
}