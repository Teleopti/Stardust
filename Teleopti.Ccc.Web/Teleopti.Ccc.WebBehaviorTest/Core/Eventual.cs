﻿using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

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
	}



	public static class EventualTimeouts
	{
		public static TimeSpan Timeout { get; private set; }
		public static TimeSpan Poll { get; private set; }

		static EventualTimeouts() { Set(TimeSpan.FromSeconds(5)); }

		public static void Set(TimeSpan timeout)
		{
			Timeout = timeout;
			Poll = TimeSpan.FromMilliseconds(25);
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
										Func<Exception, T> failingValue = e =>
										                                  	{
										                                  		exception = e;
										                                  		return default(T);
										                                  	};

										Func<T> robustValue = () => Robustness.SafeIEOperation(value.Invoke, failingValue);

										Assert.That(() => robustValue.Invoke(), reusableConstraint,
										            string.IsNullOrEmpty(message) ? string.Empty : message);
										return true;
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