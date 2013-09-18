using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class DataMaker
	{
		public static ScenarioDataFactory Data()
		{
			if (ScenarioContext.Current.Value<ScenarioDataFactory>() == null)
				ScenarioContext.Current.Value(new ScenarioDataFactory());
			return ScenarioContext.Current.Value<ScenarioDataFactory>();
		}

		public static PersonDataFactory Me()
		{
			return Data().Me();
		}

		public static PersonDataFactory Person(string name)
		{
			return Data().Person(name);
		}

		public static bool PersonExists(string name)
		{
			return Data().HasPerson(name);
		}

		public static AnalyticsDataFactory Analytics()
		{
			return Data().Analytics();
		}

	}
}