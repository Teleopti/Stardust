using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class DataMaker
	{
		public static DataFactoryCoordinator Data()
		{
			if (ScenarioContext.Current.Value<DataFactoryCoordinator>() == null)
				ScenarioContext.Current.Value(new DataFactoryCoordinator());
			return ScenarioContext.Current.Value<DataFactoryCoordinator>();
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
	}
}