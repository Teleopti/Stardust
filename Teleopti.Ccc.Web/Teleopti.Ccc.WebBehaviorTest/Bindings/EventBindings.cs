using System;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class BeforeTestRunException : Exception
	{
		public BeforeTestRunException(Exception innerException)
			: base("Exception occurred in BeforeTestRun", innerException)
		{
		}
	}

	[Binding]
	public class EventBindings
	{
		private static Exception _testRunSetupException;
		private static TestRun _testRun;

		// THIS METHOD WILL BE RETRIED WHEN FAILED!
		// So we catch all exceptions...
		// .. so that it wont be retried
		// .. and rethrow in BeforeScenario so all tests will fail with the same exception
		// .. and dont run AfterScenario at all
		// .. and let TearDown clean up
		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			try
			{
				_testRun = new TestRun();
				_testRun.Setup();
			}
			catch (Exception e)
			{
				_testRunSetupException = e;
			}
		}
		
		[BeforeScenario]
		public static void BeforeScenario()
		{
			if (_testRunSetupException != null)
				throw new BeforeTestRunException(_testRunSetupException);

			_testRun.BeforeScenario();
		}


		[AfterScenario]
		public static void AfterScenario()
		{
			if (_testRunSetupException != null)
				return;

			_testRun.AfterScenario();
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			_testRun.TearDown();
		}

	}
}
