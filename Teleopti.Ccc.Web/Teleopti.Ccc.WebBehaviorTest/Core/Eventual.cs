using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class EventualTimeouts
	{
		public static TimeSpan Timeout { get; private set; }
		public static TimeSpan Poll { get; private set; }

		static EventualTimeouts() { Set(TimeSpan.FromSeconds(5)); }

		public static void Set(TimeSpan timeout)
		{
			Timeout = timeout;
			Poll = TimeSpan.FromMilliseconds(10);
			Settings.WaitForCompleteTimeOut = Convert.ToInt32(timeout.TotalSeconds);
			Settings.WaitUntilExistsTimeOut = Convert.ToInt32(timeout.TotalSeconds);
		}
	}

	public static class EventualAssert
	{
		public static void That<T>(Func<T> value, Constraint constraint)
		{
			That(value, constraint, null);
		}

		public static void That<T>(Func<T> value, Constraint constraint, string message)
		{
			ReusableConstraint reusableConstraint = constraint;
			Exception exception = null;
			Func<bool> longPollTimeSafeAssert = () =>
			                   	{
									try
									{
										if (string.IsNullOrEmpty(message))
											Assert.That(value.Invoke(), reusableConstraint);
										else
											Assert.That(value.Invoke(), reusableConstraint, message);
										return true;
									}
									catch (UnauthorizedAccessException ex)
									{
										// sometimes IE api gives these errors when the page is in a state between pages or something
										// if so, lets just try again
										// maybe this behavior should be placed elsewhere and not only apply to asserts..
										exception = ex;
										return false;
									}
									catch (AssertionException ex)
									{
										exception = ex;
										return false;
									}
			                   	};
			if (!longPollTimeSafeAssert.WaitUntil(EventualTimeouts.Poll, EventualTimeouts.Timeout))
				throw exception;
		}

		public static void WhenElementExists<TElement, TValue>(TElement element, Func<TElement, TValue> value, Constraint constraint) where  TElement : Element
		{
			element.WaitUntilExists();
			That(() => value(element), constraint);
		}

	}

	public static class EventualActions
	{
		private static int Timeout { get { return Convert.ToInt32(EventualTimeouts.Timeout.TotalSeconds); } }

		public static void EventualClick<T>(this T element) where T : Element
		{
			element.WaitUntilExists(Timeout);
			element.WaitUntilEnabled();
			element.Click();
		}

		public static T EventualGet<T>(this T element) where T : Element
		{
			element.WaitUntilExists(Timeout);
			return element;
		}

		//// Func here shouldnt really be required I think, cant figure out why it is sometimes...
		//public static IEnumerable<T> EventualGet<T>(this Func<IEnumerable<T>> elements) where T : Element
		//{
		//    Func<bool> hasItems = () => elements.Invoke().Any();
		//    hasItems.WaitUntil(EventualTimeouts.Poll, EventualTimeouts.Timeout);
		//    return elements.Invoke();
		//}

	}

	public static class EventualWaits
	{
		public static void WaitUntilAjaxContentComplete(this WatiN.Core.Browser browser)
		{
			var loading = browser.Div("loading");
			if (!loading.Exists) return;
			loading.WaitUntilHidden();
		}

		public static void WaitUntilHidden<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.DisplayHidden());
		}

		public static void WaitUntilDisplayed<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.DisplayVisible());
		}

		public static void WaitUntilEnabled<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.Enabled);
		}
	}
}