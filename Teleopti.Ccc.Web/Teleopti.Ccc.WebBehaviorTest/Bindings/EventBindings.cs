using System;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class EventBindings
	{
		[BeforeScenario]
		public static void BeforeScenario() => SetupFixtureForAssembly.TestRun.BeforeTest(new ScenarioTest());

		[AfterStep]
		public static void AfterStep() => SetupFixtureForAssembly.TestRun.AfterStep();

		[AfterScenario]
		public static void AfterScenario() => SetupFixtureForAssembly.TestRun.AfterTest();
	}
}