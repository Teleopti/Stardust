using System;
using NUnit.Framework;

namespace Teleopti.Ccc.InfrastructureTest
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class RestoreDatabaseAfterTestAttribute : Attribute, ITestAction
	{
		public void BeforeTest(TestDetails testDetails)
		{
		}

		public void AfterTest(TestDetails testDetails)
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
		}

		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}
	}
}