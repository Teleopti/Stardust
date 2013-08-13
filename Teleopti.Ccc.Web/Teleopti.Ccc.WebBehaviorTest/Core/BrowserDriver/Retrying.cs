using System;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class Retrying
	{
		public static T Action<T>(Func<T> action, IExceptionCatcher exceptionCatcher)
		{
			var value = default(T);
			Func<bool> retryingAction = () =>
				{
					var success = true;
					value = exceptionCatcher.Action(
						action,
						e =>
							{
								success = false;
								return default(T);
							}
						);
					return success;
				};
			retryingAction.WaitOrThrow(Timeouts.Poll, Timeouts.Timeout);
			return value;
		}

		public static void Action(Action action, IExceptionCatcher exceptionCatcher)
		{
			Func<bool> retryingAction = () =>
				{
					var success = true;
					exceptionCatcher.Action(
						action,
						e =>
							{
								success = false;
							}
						);
					return success;
				};
			retryingAction.WaitOrThrow(Timeouts.Poll, Timeouts.Timeout);
		}
	}
}