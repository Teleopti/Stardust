using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
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

		public static void ApplyFromTable<T>(Table table)
		{
			var instance = table.CreateInstance<T>();
			if (instance is IDataSetup)
				Data().Apply(instance as IDataSetup);
			else if (instance is IUserSetup)
				Me().Apply(instance as IUserSetup);
			else if (instance is IUserDataSetup)
				Me().Apply(instance as IUserDataSetup);
			else
				throw new NotImplementedException("Can not apply setup of type " + typeof (T));
		}

		public static void ApplyFromTable<T>(string name, Table table)
		{
			var instance = table.CreateInstance<T>();
			if (instance is IUserSetup)
				Person(name).Apply(instance as IUserSetup);
			else if (instance is IUserDataSetup)
				Person(name).Apply(instance as IUserDataSetup);
			else
				throw new NotImplementedException("Can not apply setup of type " + typeof (T) + " for " + name);
		}

	}
}