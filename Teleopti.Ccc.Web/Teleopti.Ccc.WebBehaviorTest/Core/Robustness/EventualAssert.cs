using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions.WatiNIE;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
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

			                                    			Func<T> robustValue = () => ExceptionHandling.Action(value.Invoke, failingValue);

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
			if (!longPollTimeSafeAssert.WaitUntil(Timeouts.Poll, Timeouts.Timeout))
				throw exception;
		}

	}
}