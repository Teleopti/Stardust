using System;
using System.Threading;
using Manager.Integration.Test.LoadTests;

namespace Manager.Integration.LoadTests
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			//------------------------------------
			// Do NOT change order of execution.
			//------------------------------------

			var loadTests = new OneManagerAndOneNodeLoadTests();

			// Set up test.
			loadTests.TestFixtureSetUp();
			loadTests.SetUp();

			// Execute test.
			loadTests.YourTestGoesHere();

			// Tear down test.
			loadTests.TearDown();
			loadTests.TestFixtureTearDown();
		}
	}
}