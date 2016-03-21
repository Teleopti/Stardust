using System;
using System.Threading;
using Manager.Integration.Test.LoadTests;

namespace Manager.Integration.LoadTests
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var loadTests = new OneManagerAndOneNodeLoadTests();

			loadTests.TestFixtureSetUp();
			loadTests.SetUp();

			loadTests.YourTestGoesHere();

			Thread.Sleep(TimeSpan.FromMinutes(10));

			loadTests.TearDown();
			loadTests.TestFixtureTearDown();
		}
	}
}