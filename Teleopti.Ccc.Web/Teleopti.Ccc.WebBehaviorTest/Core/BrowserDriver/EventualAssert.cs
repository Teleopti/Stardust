using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class EventualAssert
	{
		public static void That<T>(Func<T> value, Constraint constraint, Func<string> message, IExceptionCatcher exceptionCatcher)
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

						Func<T> robustValue = () => exceptionCatcher.Action(value.Invoke, failingValue);

						Assert.That(() => robustValue.Invoke(), reusableConstraint);
						return true;
					}
					catch (AssertionException ex)
					{
						exception = ex;
						return false;
					}
				};

			if (longPollTimeSafeAssert.WaitUntil(Timeouts.Poll, Timeouts.Timeout)) return;

			if (message != null)
				Assert.Fail(message.Invoke());
			throw exception;
		}
	}
}