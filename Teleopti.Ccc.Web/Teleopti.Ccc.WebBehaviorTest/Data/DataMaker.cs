﻿using System;
using Autofac;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class DataMaker
	{
		public static ScenarioDataFactory Data() =>
			LocalSystem.DataMaker.Data();

		public static void EndSetupPhase() =>
			LocalSystem.DataMaker.EndSetupPhase();

		public static void AfterScenario() =>
			LocalSystem.DataMaker.AfterScenario();

		public static PersonDataFactory Me() =>
			LocalSystem.DataMaker.Data().Me();

		public static PersonDataFactory Person(string name) =>
			LocalSystem.DataMaker.Data().Person(name);

		public static bool PersonExists(string name) =>
			LocalSystem.DataMaker.Data().HasPerson(name);

		public static AnalyticsDataFactory Analytics() =>
			LocalSystem.DataMaker.Data().Analytics();

		public static void ApplyFromTable<T>(Table table) =>
			LocalSystem.DataMaker.ApplyFromTable<T>(table);

		public static void ApplyFromTable<T>(string name, Table table) =>
			LocalSystem.DataMaker.ApplyFromTable<T>(name, table);
	}

	public class DataMakerImpl
	{
		private readonly IComponentContext _container;

		public DataMakerImpl(IComponentContext container)
		{
			_container = container;
		}

		public ScenarioDataFactory Data()
		{
			if (ScenarioContext.Current.Value<ScenarioDataFactory>() == null)
				ScenarioContext.Current.Value(_container.Resolve<ScenarioDataFactory>());
			return ScenarioContext.Current.Value<ScenarioDataFactory>();
		}

		public void EndSetupPhase()
		{
			if (ScenarioContext.Current.Value<bool>("setupDone")) return;
			ScenarioContext.Current.Value("setupDone", true);
			Data().EndSetupPhase();
		}

		public void AfterScenario()
		{
			if (ScenarioContext.Current.Value<ScenarioDataFactory>() != null)
				Data().Dispose();
		}

		public void ApplyFromTable<T>(Table table)
		{
			var instance = table.CreateInstance<T>();
			if (instance is IDataSetup)
				Data().Apply(instance as IDataSetup);
			else if (instance is IUserSetup)
				Data().Me().Apply(instance as IUserSetup);
			else if (instance is IUserDataSetup)
				Data().Me().Apply(instance as IUserDataSetup);
			else
				throw new NotImplementedException("Can not apply setup of type " + typeof(T));
		}

		public void ApplyFromTable<T>(string name, Table table)
		{
			var instance = table.CreateInstance<T>();
			if (instance is IUserSetup)
				Data().Person(name).Apply(instance as IUserSetup);
			else if (instance is IUserDataSetup)
				Data().Person(name).Apply(instance as IUserDataSetup);
			else
				throw new NotImplementedException("Can not apply setup of type " + typeof(T) + " for " + name);
		}
	}
}