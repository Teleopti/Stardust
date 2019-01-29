using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class EventualAssert
	{
		public static void That<T>(Func<T> value, Constraint constraint, Func<string> message, IExceptionCatcher exceptionCatcher, TimeSpan poll, TimeSpan timeout)
		{
			ReusableConstraint reusableConstraint = constraint;
			Exception exception = null;
			Func<bool> longPollTimeSafeAssert = () =>
			{
				Func<Exception, T> failingValue = e =>
				{
					exception = e;
					return default(T);
				};

				Func<T> robustValue = () => exceptionCatcher.Action(value.Invoke, failingValue);

				var result = robustValue.Invoke();
				var constraintResult = reusableConstraint.Resolve().ApplyTo(result);
				return constraintResult.IsSuccess;
			};

			if (longPollTimeSafeAssert.WaitUntil(poll, timeout)) return;

			if (message != null)
				Assert.Fail(message.Invoke());
			throw exception;
		}
	}
}